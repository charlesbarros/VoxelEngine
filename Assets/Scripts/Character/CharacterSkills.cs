using UnityEngine;
using System.Collections;

// This class handle the create/block skills of our character.
public class CharacterSkills : MonoBehaviour 
{
	public ParticleSystem _grassDestroy;
	public ParticleSystem _dirtyDestroy;
	public ParticleSystem _stoneDestroy;
	public GameObject _pickAxe;
	public Button _buttonCreateBlock;
	public Button _buttonDestroy;
	public Button _buttonBlockGrass;
	public Button _buttonBlockDirt;
	public Button _buttonBlockStone;
	public CharacterSoundController _soundController;
	
	private World _world;
	private Block _selectedBlock;
	private Vector3 _selectedBlockPosition;
	private Vector3 _colisionPoint;
	private BlockType _selectedBlockType;
	
	public void Init(World world)
	{
		_world = world;
		_selectedBlock = new Block(BlockType.Unknown);
		
		if (_buttonCreateBlock != null && _buttonDestroy != null)
		{
			_buttonCreateBlock.EventPressed += CreateBlock;
			_buttonDestroy.EventPressed     += DestroyBlock;
		}
		
		_buttonBlockGrass.EventPressed += SelectGrassBlock;
		_buttonBlockDirt.EventPressed  += SelectDirtBlock;
		_buttonBlockStone.EventPressed += SelectStoneBlock;
		
		_buttonBlockDirt.ForcePress();
		_selectedBlockType = BlockType.Dirt;
	}
	
	public void SetSelectedBlock(Block block) 
	{
		_selectedBlock = block;
	}

	public Block GetSelectedBlock() 
	{
		return _selectedBlock;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Screen.showCursor) return;
		
		// The anim duration is used like a cooldown to avoid the player create/destroy blocks to fast
		if( Input.GetMouseButtonDown(0) && IsPlaying() == false) 
		{
			DestroyBlock();
		}
		
		if( Input.GetMouseButtonDown(1) && IsPlaying() == false) 
		{
			CreateBlock();
		}
		
	}
	
	bool IsPlaying()
	{
		return (_pickAxe.animation["Take 001"].normalizedTime <= 0.85f && _pickAxe.animation["Take 001"].normalizedTime > 0.0f);	
	}
	
	void SelectGrassBlock()
	{
		_selectedBlockType = BlockType.Grass;
		_buttonBlockGrass.ForcePress();
		_buttonBlockDirt.ForceRelease();
		_buttonBlockStone.ForceRelease();
	}

	void SelectDirtBlock()
	{
		_selectedBlockType = BlockType.Dirt;
		_buttonBlockDirt.ForcePress();
		_buttonBlockGrass.ForceRelease();
		_buttonBlockStone.ForceRelease();
	}
	
	void SelectStoneBlock()
	{
		_selectedBlockType = BlockType.Stone;
		_buttonBlockStone.ForcePress();
		_buttonBlockGrass.ForceRelease(); 
		_buttonBlockDirt.ForceRelease(); 		
	}
	
	void CreateBlock()
	{
		_pickAxe.animation.Stop();
		_pickAxe.animation.Play();
		
		Block selectedBlock = GetBlock(true);

		if ( selectedBlock.IsSolid() == false && selectedBlock.Type != BlockType.Unknown)
		{
			_soundController.PlayCreateBlock();
			
			_selectedBlock = selectedBlock;
			_selectedBlock.Create(_selectedBlockType);
	
			StartCoroutine( _world.RefreshChunkMesh( new Vector3i(_selectedBlockPosition), false  ) );
		}		
	}
	
	void DestroyBlock()
	{
		_pickAxe.animation.Stop();
		_pickAxe.animation.Play();
		
		Block selectedBlock = GetBlock();
		
		if ( selectedBlock.IsSolid() && selectedBlock.Type != BlockType.Lava)
		{
			_selectedBlock = selectedBlock;
			
			_soundController.PlayDestroyBlock();
			
			#region DestroyFx
			ParticleSystem destroyParticle = null;
			
			switch (_selectedBlock.Type)
			{
				case BlockType.Grass:
					destroyParticle = _grassDestroy;
					break;
				
				case BlockType.Dirt:
					destroyParticle = _dirtyDestroy;
					break;
				
				case BlockType.Stone:
					destroyParticle = _stoneDestroy;
					break;
			}
			
			if (destroyParticle != null)
			{
				ParticleSystem particle = GameObject.Instantiate(destroyParticle) as ParticleSystem;
				
				// Apply the same light of the block that will be destroyed into the particles
				int x = Mathf.RoundToInt(_selectedBlockPosition.x);
				int y = Mathf.RoundToInt(_selectedBlockPosition.y);
				int z = Mathf.RoundToInt(_selectedBlockPosition.z);
				Block topBlock = _world[x, y+1, z];
				float particleColor = ((float)topBlock.Light)/255.0f;
				particle.renderer.material.color = new Color(particleColor, particleColor, particleColor);
				
				Vector3 particlePos = _selectedBlockPosition;
				particlePos.y += 0.75f;
				particle.transform.position = particlePos;
			}
			#endregion
			
			_selectedBlock.Destroy();
			StartCoroutine( _world.RefreshChunkMesh( new Vector3i(_selectedBlockPosition), false  ) );
		}			
	}
	
	Block GetBlock(bool getNearestNeighbor = false)
	{
		Ray ray = Camera.main.ScreenPointToRay( new Vector2(Screen.width/2f, Screen.height/2f) );
		RaycastHit hit = new RaycastHit();
		
		Block block = new Block(BlockType.Unknown);
    	if (Physics.Raycast(ray, out hit, 10.0f) == true)
		{
			_colisionPoint = hit.point;
            Vector3 hp = _colisionPoint + 0.0001f * ray.direction;

            int x = Mathf.CeilToInt(hp.x) - 1;
            int y = Mathf.CeilToInt(hp.y) - 1;
            int z = Mathf.CeilToInt(hp.z) - 1;

			_selectedBlockPosition = new Vector3(x,y,z);

			if (getNearestNeighbor == true)
			{
				#region GetNearestNeighbor
				Vector3 nearestBlock = _colisionPoint - _selectedBlockPosition;

				if (nearestBlock.x == 1.0f)
				{
					x++;
				}
				else if (nearestBlock.x == 0.0f)
				{
					x--;
				}
	
				if (nearestBlock.y == 1.0f)
				{
					y++;
				}
				else if (nearestBlock.y == 0.0f)
				{
					y--;
				}
	
				if (nearestBlock.z == 1.0f)
				{
					z++;
				}
				else if (nearestBlock.z == 0.0f)
				{
					z--;
				}
								
				_selectedBlockPosition.x = x;
				_selectedBlockPosition.y = y;
				_selectedBlockPosition.z = z;
				
				block = _world[x, y, z];
				#endregion
			}
			else
			{
				block = _world[x, y, z];	
			}
		}		
		
		return block;
	}
	
	void OnDrawGizmos() 
	{
		if (Application.isPlaying == true)
		{
			Gizmos.DrawRay(Camera.main.ScreenPointToRay( new Vector3(Screen.width/2f, Screen.height/2f, 0.0f) ) );
			
			if ( _selectedBlock != null && _selectedBlock.Type != BlockType.Unknown )
			{
				Vector3 cubePos = _selectedBlockPosition + Vector3.one/2;
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube( cubePos, Vector3.one*1.05f );
			}
		}
	}
}
