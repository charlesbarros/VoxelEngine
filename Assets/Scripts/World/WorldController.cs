/// <summary>
/// This class is used to forward some cfg to other classes and create the world.
/// There is many things that I would did different now, but as a prototype I think that it's ok.
/// </summary>

using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour 
{
	public ChunkObject _chunkPrefab;
	public Transform _worldTransform;
	public CharacterSkills _character;
	public CharacterSkills _characterMobile;
	public GUIText _console;
	
	// Debug 
	// Note: To see the world being created, enable the following flags:
	// "showHeightMapGizmos", "showChunksBoundingBox", "asyncWorldCreation" and hit play on Unity Editor
	public bool _showHeightMapGizmos = false;
	public bool _showLightFloodGizmos = false;
	public bool _showChunksBoundingBox = false;
	public bool _asyncWorldCreation = false;
	
	// Light Cfg
	public bool _lightAttenuation = true;
	public bool _smoothLight = true;

	private World _world;
	private SimplexNoise3D _simplexNoise3D;
	
	IEnumerator Start () 
	{	
		_world = new World();
		
		ChunkRenderer.SmoothLight = _smoothLight;
		World.LightAttenuation = _lightAttenuation;
		ChunkRenderer.WorldHeight = new float[_world.WorldVisibleSizeX, _world.WorldVisibleSizeZ];
		
		_simplexNoise3D = new SimplexNoise3D(WorldConfig.WorldSeed);
		
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_characterMobile.Init(_world);
		}
		else
		{
			_character.Init(_world);
		}
		
		// The following code avoid the player fall before the world creation finish
		// Note: Yeah, I know... I'm a lazy bastard. 
		// Since this project is not focused on the character control I don't mind to do some ugly stuff like this
		StartCoroutine( "FreezePlayer" );
			
		yield return StartCoroutine( CreateWorld() );
		
		StopCoroutine( "FreezePlayer" );
		
	}
	
	private IEnumerator FreezePlayer()
	{
		Vector3 pos = Vector3.zero;
		
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			pos = _characterMobile.transform.position;
			
			while (true)
			{
				_characterMobile.transform.position = pos;
				yield return null;
			}
		}
		else
		{
			pos = _character.transform.position;
			
			while (true)
			{
				_character.transform.position = pos;
				yield return null;
			}
		}	
		
	}
	
	// This method first create all world data using the Simplex3D Noise, and then start to build the chunks meshes.
	// Note: if you wanna to create a infinite world on the fly, you will need to do booth steps together. Something like:
	// 		 1. create the data for one chunk
	// 		 2. build its mesh
	// 		 3. iterate until all visible chunks are created
	IEnumerator CreateWorld()
	{
		int total = 0;
		int count = 0;

		#region CreatingWorldData
		_console.material.color = Color.black;
		_console.text = "Creating World Data: 0%";

		total = _world.WorldVisibleSizeX * _world.WorldVisibleSizeY * _world.WorldVisibleSizeZ;
		count = 0;
		
		float heightMult = 50.0f;
		float heightOffSet = 10.0f;
		
		// The following value is used to create some dirt blocks on the floor to add more randomness to the world. 
		// The random seed is based on the first character of the worldSeed used in the simplex 3D.
		UnityEngine.Random.seed = System.Convert.ToInt32(WorldConfig.WorldSeed[0]);
		int dirtRandomHeight = UnityEngine.Random.Range(5, 7);
		
		for (int x = 0; x < _world.WorldVisibleSizeX; x++)
		{
			for (int z = 0; z < _world.WorldVisibleSizeZ; z++)
			{
				// I'm using the default values of the simplexNoise3D. 
				// For better results you will have to tune some of those parameters:
				// octaves, multiplier, amplitude, lacunarity, persistence
				// There is a good article about it at: 
				// http://www.gamedev.net/blog/33/entry-2227887-more-on-minecraft-type-world-gen/
				int height =  Mathf.RoundToInt(_simplexNoise3D.CoherentNoise( x, 0, z, 1, 50 ) * heightMult + heightOffSet);
				height = Mathf.Min(_world.WorldVisibleSizeY-1, height);
				ChunkRenderer.WorldHeight[x,z] = height;

				for (int y = 0; y < _world.WorldVisibleSizeY; y++)
				{
					CreateCollum(x,y,z, height, dirtRandomHeight);				
					count++;
				}
			}
			LogProgress("Creating World Data: ", count, total);
			if (_asyncWorldCreation) yield return null;
		}
		#endregion
		
		#region CreatingWorldMesh
		_console.text = "Creating World Mesh: 0%";

		total = _world.VisibleX * _world.VisibleY * _world.VisibleZ;
		count = 0;

		for (int x = 0; x <  _world.VisibleX; x++)
		{
			for (int y = 0; y <  _world.VisibleY; y++)
			{
				for (int z = 0; z <  _world.VisibleZ; z++)
				{
					CreateWorldMesh(x,y,z);
					
					count++;
					LogProgress("Creating World Mesh: ", count, total);
					if (_asyncWorldCreation) yield return null;
				}
			}
		}
		_console.text = string.Empty;
		#endregion
	}
	
	// Define the block type basedDefine the block type based on its height
	// Note: there is many other ways to do that. This is the simplest way, but the result is very primitive...int y, int z, int height, int dirtRandomHeight)
	void CreateCollum(int x, int y, int z, int height, int dirtRandomHeight)
	{
		// Base
		if (y <= 0)
		{
			_world[x,y,z] = new Block(BlockType.Lava, x,y,z);
		}
		// Cave floor
		else if (y == 1)
		{
			_world[x,y,z] = new Block(BlockType.Stone, x,y,z);
		}
		// Highest block of the column
		else if (y == height)
		{
			_world[x,y,z] = new Block(BlockType.Height, x,y,z);
		}
		// Highest solid block of the column
		else if (y == (height-1) )
		{
			if (y < dirtRandomHeight )
				_world[x,y,z] = new Block(BlockType.Dirt, x,y,z);
			else
				_world[x,y,z] = new Block(BlockType.Grass, x,y,z);
		}
		// Dirt range
		else if (y <= (height-2) && y > (height-4) )
		{
			_world[x,y,z] = new Block(BlockType.Dirt, x,y,z);
		}
		// Caves
		else if (y <= (height-4))
		{
			float density = _simplexNoise3D.GetDensity( new Vector3(x, y, z) );
		
			if (density > 0)
				_world[x,y,z] = new Block(BlockType.Stone, x,y,z);
			else
				_world[x,y,z] = new Block(BlockType.Air, x,y,z);
		}
		// Everything else
		else
		{
			_world[x,y,z] = new Block(BlockType.Air, x,y,z);
			_world[x,y,z].Light = Block.MaxLight;
		}		
	}
	
	void CreateWorldMesh(int x, int y, int z)
	{
		ChunkObject chunkObject = Instantiate(_chunkPrefab) as ChunkObject;
		chunkObject.name = "Chunk["+x+","+y+","+z+"]";
		chunkObject.transform.parent = _worldTransform;
		chunkObject.transform.position = new Vector3(x*Chunk.SizeX, y*Chunk.SizeY, z*Chunk.SizeZ);
		_world.CreateChunkMesh(x, y, z, chunkObject );		
	}
	
	void LogProgress(string msg, int current, int total)
	{
		string progress = string.Format("{0:0.00}", ((float)current/total)*100);
		_console.text = msg+progress+"%";
	}

	void OnDrawGizmos() 
	{
		if (Application.isPlaying == true)
		{
			// HeightMap
			if (_showHeightMapGizmos && ChunkRenderer.WorldHeight != null)
			{
				for (int x = 0; x < ChunkRenderer.WorldHeight.GetLength(0); x++)
				{
					for(int z = 0; z < ChunkRenderer.WorldHeight.GetLength(1); z++)
					{
						Vector3 cubePos= new Vector3(x, ChunkRenderer.WorldHeight[x,z], z);
						cubePos +=  Vector3.one/2;
						Gizmos.color = Color.gray;
						Gizmos.DrawWireCube( cubePos, Vector3.one );
					}
				}
			}
			
			// LightFlood
			if (_showLightFloodGizmos && ChunkRenderer.LightningBlocks != null)
			{
				foreach(Vector3 light in ChunkRenderer.LightningBlocks)
				{
					Vector3 cubePos= new Vector3(light.x, light.y, light.z);
					cubePos +=  Vector3.one/2;
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireCube( cubePos, Vector3.one );
				}
			}	
			
			// Chunks
			if (_showChunksBoundingBox && _world.Chunks != null)
			{
				foreach (Chunk chunk in _world.Chunks)
				{
					if (chunk.ChunkObject != null)
					{
						Vector3 cubePos= chunk.ChunkObject.transform.position;
						Vector3 size = new Vector3(Chunk.SizeX, Chunk.SizeY, Chunk.SizeZ);
						cubePos +=  size/2;
						Gizmos.color = Color.black;
						Gizmos.DrawWireCube(cubePos , size );
					}
				}
			}
		}
	}
}
