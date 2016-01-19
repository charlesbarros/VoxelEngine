using System.Collections.Generic;
using UnityEngine;

class ChunkRenderer
{
	private static bool _smoothLight;
	public static bool SmoothLight { set { _smoothLight = value; } }
	
	private static List<Vector3> _lightningBlocks = new List<Vector3>();
	public static List<Vector3> LightningBlocks { get { return _lightningBlocks; } }
	
	private static float[,] _worldHeight;
	public static float[,] WorldHeight { get { return _worldHeight; } set { _worldHeight = value; } }
	
	private static List<Vector3> _vertices 	= new List<Vector3>();
	private static List<Color32> _colors 	= new List<Color32>();
    private static List<int> _triangles 	= new List<int>();
 	private static List<Vector2> _uvs 		= new List<Vector2>();

	// Create the chunk mesh only rendering the faces that can be seen.
    public static Mesh Render(Chunk chunk)
    {
		_vertices.Clear();
		_colors.Clear();
   		_triangles.Clear();
 		_uvs.Clear();
		
		for (int x = 0; x < Chunk.SizeX; x++) 
			for (int y = 0; y < Chunk.SizeY; y++) 
				for (int z = 0; z < Chunk.SizeZ; z++) 
				{
					// Get neighbours
					Block block = chunk [x, y, z];                    
                    
					// Check if the current block need to be rendered
					if (block != null && block.IsSolid ()) 
					{
						// The code bellow check the neighbour of each face and if needed create the mesh for it.
						Block top = chunk [x, y + 1, z];
						if (top.IsSolid () == false) 
						{
							CreateTopFace (top, block, chunk, x, y, z);
						}
						Block front = chunk [x, y, z + 1];
						if (front.IsSolid () == false) 
						{
							CreateFrontFace (front, block, chunk, x, y, z);
						}
						Block left = chunk [x + 1, y, z];
						if (left.IsSolid () == false) 
						{
							CreateLeftFace (left, block, chunk, x, y, z);
						}
						Block back = chunk [x, y, z - 1];
						if (back.IsSolid () == false) 
						{
							CreateBackFace (back, block, chunk, x, y, z);
						}
						Block right = chunk [x - 1, y, z];
						if (right.IsSolid () == false) 
						{
							CreateRightFace (right, block, chunk, x, y, z);
						}
						Block bottom = chunk [x, y - 1, z];
						if (bottom.IsSolid () == false) 
						{
							CreateBottomFace (bottom, block, chunk, x, y, z);
						}
					}
				}

        Mesh mesh = new Mesh();
        mesh.vertices = _vertices.ToArray();
		mesh.colors32 = _colors.ToArray();
        mesh.triangles = _triangles.ToArray();
 		mesh.uv = _uvs.ToArray();

        return mesh;
    }

	#region CreateFace
	static void CreateTopFace(Block top, Block block, Chunk chunk, int x, int y, int z)
	{
		// Flat light
		int c1 = top.Light, c2 = top.Light , c3 = top.Light, c4 = top.Light;

		if (_smoothLight)
		{
			Block frontTop = chunk[x, y+1, z +1];
			Block backTop = chunk[x, y+1, z -1];

			Block leftTop = chunk[x+1, y +1, z];
			Block rightTop = chunk[x-1, y +1, z];

			Block frontTopLeft = chunk[x+1, y+1, z +1];
			Block frontTopRight= chunk[x-1, y+1, z +1];

			Block backTopLeft = chunk[x+1, y+1, z -1];
			Block backTopRight= chunk[x-1, y+1, z -1];

			c1 = (top.Light+ backTop.Light + backTopRight.Light + rightTop.Light) / 4;  // Bottom Left
			c2 = (top.Light+ frontTop.Light + frontTopRight.Light + rightTop.Light) / 4;// Bottom Right 
			c3 = (top.Light+ frontTop.Light + frontTopLeft.Light + leftTop.Light) / 4;  // Top Right
			c4 = (top.Light+ backTop.Light + backTopLeft.Light + leftTop.Light) / 4;    // Top Left
		}

		Vector3 pos = new Vector3(x, y, z);

		CreateFace(block.TextureTopUV(), pos,
			new Vector3(0,1,0), new Vector3(0,1,1), new Vector3(1,1,1), new Vector3(1,1,0), 
			c1, c2, c3, c4 );
	}

	static void CreateBottomFace(Block bottom, Block block, Chunk chunk, int x, int y, int z)
	{
		// Flat light
		int c1 = bottom.Light, c2 = bottom.Light, c3 = bottom.Light, c4 = bottom.Light;

		if (_smoothLight)
		{	
			Block frontBottom = chunk[x, y-1, z +1];
			Block backBottom = chunk[x, y-1, z -1];

			Block leftBottom = chunk[x+1, y -1, z];
			Block rightBottom = chunk[x-1, y -1, z];

			Block frontBottomLeft = chunk[x+1, y-1, z +1];
			Block frontBottomRight= chunk[x-1, y-1, z +1];

			Block backBottomLeft = chunk[x+1, y-1, z -1];
			Block backBottomRight= chunk[x-1, y-1, z -1];

			c1 = (bottom.Light+ backBottom.Light + backBottomRight.Light + rightBottom.Light) / 4;   // Bottom Left
			c4 = (bottom.Light+ frontBottom.Light + frontBottomRight.Light + rightBottom.Light) / 4; // Bottom Right 
			c3 = (bottom.Light+ frontBottom.Light + frontBottomLeft.Light + leftBottom.Light) / 4;   // Top Right
			c2 = (bottom.Light+ backBottom.Light + backBottomLeft.Light + leftBottom.Light) / 4;     // Top Left
		}

		Vector3 pos = new Vector3(x, y, z);

		CreateFace(block.TextureBottomUV(), pos,
			new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,0,1), new Vector3(0,0,1), 
			c1, c2, c3, c4 );
	}


	static void CreateLeftFace(Block left, Block block, Chunk chunk, int x, int y, int z)
	{
		// Flat light
		int c1  = left.Light, c2  = left.Light, c3  = left.Light, c4 = left.Light;

		if (_smoothLight)
		{
			Block leftTop = chunk[x+1, y +1, z];
			Block leftBottom = chunk[x+1, y -1, z];

			Block frontLeft = chunk[x+1, y, z +1];
			Block backLeft = chunk[x+1, y, z -1];

			Block frontTopLeft = chunk[x+1, y+1, z +1];
			Block backTopLeft= chunk[x+1, y+1, z -1];

			Block frontBottomLeft = chunk[x+1, y-1, z +1];
			Block backBottomLeft = chunk[x+1, y-1, z -1];

			c1 = (left.Light+ leftBottom.Light + backBottomLeft.Light + backLeft.Light) / 4;   // Bottom Left
			c2 = (left.Light+ leftTop.Light + backTopLeft.Light + backLeft.Light) / 4;         // Top Left
			c3 = (left.Light+ leftTop.Light + frontTopLeft.Light + frontLeft.Light) / 4;       // Top Right
			c4 = (left.Light+ leftBottom.Light + frontBottomLeft.Light + frontLeft.Light) / 4; // Bottom Right
		}

		Vector3 pos = new Vector3(x, y, z);

		CreateFace(block.TextureUV(), pos,
			new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(1,1,1), new Vector3(1,0,1), 
			c1, c2, c3, c4 );
	}


	static void CreateRightFace(Block right, Block block, Chunk chunk, int x, int y, int z)
	{
		// Flat light
		int c1 = right.Light, c2 = right.Light, c3 = right.Light, c4 = right.Light;
		if (_smoothLight)
		{	
			Block rightTop = chunk[x-1, y +1, z];
			Block rightBottom = chunk[x-1, y -1, z];

			Block frontRight = chunk[x-1, y, z +1];
			Block backRight = chunk[x-1, y, z -1];

			Block frontTopRight = chunk[x-1, y+1, z +1];
			Block backTopRight= chunk[x-1, y+1, z -1];

			Block frontBottomRight = chunk[x-1, y-1, z +1];
			Block backBottomRight = chunk[x-1, y-1, z -1];

			c1 = (right.Light+ rightBottom.Light + frontBottomRight.Light + frontRight.Light) / 4; // Bottom Left
			c2 = (right.Light+ rightTop.Light + frontTopRight.Light + frontRight.Light) / 4;       // Top Left
			c3 = (right.Light+ rightTop.Light + backTopRight.Light + backRight.Light) / 4;         // Top Right
			c4 = (right.Light+ rightBottom.Light + backBottomRight.Light + backRight.Light) / 4;   // Bottom Right
		}

		Vector3 pos = new Vector3(x, y, z);

		CreateFace(block.TextureUV(), pos,
			new Vector3(0,0,1), new Vector3(0,1,1), new Vector3(0,1,0), new Vector3(0,0,0), 
			c1, c2, c3, c4 );
	}


	static void CreateFrontFace(Block front, Block block, Chunk chunk, int x, int y, int z)
	{
		// Flat light
		int c1 = front.Light, c2 = front.Light, c3 = front.Light, c4 = front.Light;

		if (_smoothLight)
		{
			Block frontTop = chunk[x, y+1, z +1];
			Block frontBottom = chunk[x, y-1, z +1];

			Block frontLeft = chunk[x+1, y, z +1];
			Block frontRight = chunk[x-1, y, z +1];

			Block frontTopLeft = chunk[x+1, y+1, z +1];
			Block frontTopRight= chunk[x-1, y+1, z +1];

			Block frontBottomLeft = chunk[x+1, y-1, z +1];
			Block frontBottomRight= chunk[x-1, y-1, z +1];

			c1 = (front.Light+ frontBottom.Light + frontBottomRight.Light + frontRight.Light) / 4; // Bottom Left
			c2 = (front.Light+ frontBottom.Light + frontBottomLeft.Light + frontLeft.Light) / 4;   // Bottom Right
			c3 = (front.Light+ frontTop.Light + frontTopLeft.Light + frontLeft.Light) / 4;         // Top Right
			c4 = (front.Light+ frontTop.Light + frontTopRight.Light + frontRight.Light) / 4;       // Top Left
		}

		Vector3 pos = new Vector3(x, y, z);

		CreateFace(block.TextureSideUV(), pos,
			new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1), 
			c1, c2, c3, c4 );
	}


	static void CreateBackFace(Block back, Block block, Chunk chunk, int x, int y, int z)
	{
		// Flat light
		int c1 = back.Light, c2 = back.Light, c3  = back.Light, c4 = back.Light;

		if (_smoothLight)
		{
			Block backTop = chunk[x, y+1, z -1];
			Block backBottom = chunk[x, y-1, z -1];

			Block backLeft = chunk[x+1, y, z -1];
			Block backRight = chunk[x-1, y, z -1];

			Block backTopLeft = chunk[x+1, y+1, z -1];
			Block backTopRight= chunk[x-1, y+1, z -1];

			Block backBottomLeft = chunk[x+1, y-1, z -1];
			Block backBottomRight= chunk[x-1, y-1, z -1];

			c1 = (back.Light+ backBottom.Light + backBottomRight.Light + backRight.Light) / 4; // Bottom Left
			c2 = (back.Light+ backTop.Light + backTopRight.Light + backRight.Light) / 4;       // Top Left
			c3 = (back.Light+ backTop.Light + backTopLeft.Light + backLeft.Light) / 4;         // Top Right
			c4 = (back.Light+ backBottom.Light + backBottomLeft.Light + backLeft.Light) / 4;   // Bottom Right
		}

		Vector3 pos = new Vector3(x, y, z);

		CreateFace(block.TextureUV(), pos,
			new Vector3(0,0,0), new Vector3(0,1,0), new Vector3(1,1,0), new Vector3(1,0,0), 
			c1, c2, c3, c4 );
	}

	static void CreateFace(Vector2 texUV, Vector3 pos, 
								  Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, 
							      int c1, int c2, int c3, int c4)
	{
        int vertexIndex = _vertices.Count;
		
        _vertices.Add( pos + v1);
        _vertices.Add( pos + v2);
        _vertices.Add( pos + v3);
        _vertices.Add( pos + v4);

        // Create the triangles using the winding order to avoid calculate the normals
		// first triangle 
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex+1);
        _triangles.Add(vertexIndex+2);
         
        // second triangle 
        _triangles.Add(vertexIndex+2);
        _triangles.Add(vertexIndex+3);
        _triangles.Add(vertexIndex);
	
		// Light
		byte c1Byte = (byte)c1;
		byte c2Byte = (byte)c2;
		byte c3Byte = (byte)c3;
		byte c4Byte = (byte)c4;
		
		_colors.Add ( new Color32(c1Byte, c1Byte, c1Byte, 0) );
		_colors.Add ( new Color32(c2Byte, c2Byte, c2Byte, 0) );
		_colors.Add ( new Color32(c3Byte, c3Byte, c3Byte, 0) );
		_colors.Add ( new Color32(c4Byte, c4Byte, c4Byte, 0) );
		
		// UV
		float _uvsize = 0.25f;
		_uvs.Add(new Vector2(texUV.x, texUV.y));
		_uvs.Add(new Vector2(texUV.x, texUV.y+_uvsize));
		_uvs.Add(new Vector2(texUV.x+_uvsize, texUV.y+_uvsize));
		_uvs.Add(new Vector2(texUV.x+_uvsize, texUV.y));	
		
	}
	#endregion
	
	#region Lightning
	// Clear the light data of the neighbours of a given block, and recalculate the 
	// highest block of the collumn
	public static void UpdateHeightMap(World world, Vector3i worldPos, int worldVisibleSizeY)
	{
		int xMin = worldPos.x-3;
		int xMax = worldPos.x+3;
		int zMin = worldPos.z-3;
		int zMax = worldPos.z+3;		
		int yMin = worldVisibleSizeY-1;
		
		for (int x = xMin; x < xMax; x++)
		{
			for (int z = zMin; z < zMax; z++)
			{
				bool heightBlockFound = false;
				
				for (int y = yMin; y >= 0; y--)
				{
					Block block = world[x, y, z];

					if (block != null)
					{
						block.Light = 0;	
						
						if (block.IsSolid() == true)
						{
							Block top = world[x, y + 1, z];
							
							if (top != null && top.IsSolid() == false)
							{
								if (heightBlockFound == false)
								{
									top.Type = BlockType.Height;
									top.Light = Block.MaxLight;
									heightBlockFound = true;
									_worldHeight[top._wx, top._wz] = top._wy;									
								}
								
								if (block.Type == BlockType.Lava)
								{
									block.Light = Block.MaxLight;
									top.Light = Block.MaxLight;									
								}	
							}
						}
						else
						{
							if (heightBlockFound == false)
								block.Light = Block.MaxLight;	
							
							if (block.Type == BlockType.Height)
							{
								block.Type = BlockType.Air;	
							}
						}
					}
				}
			}
		}
	}
	
	// Spread the block's light for a given area
	public static void LightningFloodArea(World world, Vector3i worldPos, int worldVisibleSizeY)
	{
		_lightningBlocks.Clear();	
		
		int xMin = worldPos.x-5;
		int xMax = worldPos.x+5;
		int zMin = worldPos.z-5;
		int zMax = worldPos.z+5;		
		int yMin = worldVisibleSizeY-1;

		for (int x = xMin; x < xMax; x++)
		{
			for (int z = zMin; z < zMax; z++)
			{
				for (int y = yMin; y >= 0; y--)
				{
					Block block = world[x, y, z];
					if (block != null)
						LightningFlood(world, x, y, z, block.Light, block);
				}
			}
		}
	}
	
	private static void LightningFlood(World world, int x, int y, int z, int light, Block firstStepBlock = null)
	{
		Block block = world[x, y, z]; 
		if (firstStepBlock != null)
		{
			block = firstStepBlock;	
		}
		else
		{
			block = world[x, y, z]; 
		}

		if (block == null || block.IsSolid() == true || block.Type == BlockType.Unknown || block.Type == BlockType.WorldBound)
			return;

		int blockLight = block.Light;
		
		// Skip decay if it's the first flood iteration
		if (firstStepBlock == null)
		{
			// Light Decay 
			// Note: if you want to create more lightning steps, increase the value bellow,
			// but remember to increase the number of affected neighbours on UpdateHeightMap (ln 267) and LightningFloodArea (ln 319)
			light -= Block.MinLight;
			
			if (light <= Block.MinLight)
			{
				return;	
			}

			if (blockLight >= light)
			{
				return;	
			}
			
			block.Light = light;
			_lightningBlocks.Add( new Vector3(x,y,z) );	
		}
		
		// Note: there is more optimized ways to do that. But I think that the recursive function
		// is the easiest way to understand the light flood concept
		LightningFlood(world, x, y + 1, z, light);
		LightningFlood(world, x, y - 1, z, blockLight);
		LightningFlood(world, x, y, z +1, light);
		LightningFlood(world, x, y, z -1, light);
		LightningFlood(world, x + 1, y, z, light);
		LightningFlood(world, x - 1, y, z, light);	
	}	
	#endregion
}