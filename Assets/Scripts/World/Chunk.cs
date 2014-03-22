using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
	public const int _sizeX = 16;
	public const int _sizeY = 24;
	public const int _sizeZ = 16;

	public static int SizeX { get { return _sizeX; } }
	public static int SizeY { get { return _sizeY; } }
	public static int SizeZ { get { return _sizeZ; } }
	
	private World _worldRef;
	
    private Block[,,] _blocks;
 	private int _wx = 0, _wy = 0, _wz = 0;
	
	private ChunkObject _chunkObject;
	public ChunkObject ChunkObject 
	{ 
		get { return _chunkObject; } 
		set { _chunkObject = value; _chunkObject.SetChunk(this); } 
	}
	
	public Chunk(World world, int x, int y, int z)
	{
		_worldRef = world;
		_blocks = new Block[_sizeX, _sizeY, _sizeZ];
		_wx = x * _sizeX;
		_wy = y * _sizeY;
		_wz = z * _sizeZ;
	}
	
	public void SetDirty()
	{
		if (_chunkObject != null)
			_chunkObject.SetDirty();
	}
	
	public void RefreshChunkMesh()
	{
		_chunkObject.ChunkMesh.mesh.Clear();
		_chunkObject.ChunkMesh.mesh = ChunkRenderer.Render(this);
		_chunkObject.ChunkCollider.sharedMesh = _chunkObject.ChunkMesh.mesh;
	}

    // Get the block using chunk coordinates. 0,0,0 is the first block in the chunk
    public Block this[int lx, int ly, int lz]
    {
        get 
		{
			if (lx >= 0 && lx < _sizeX && ly >= 0 && ly < _sizeY && lz >= 0 && lz < _sizeZ)
			{
				if (_blocks[lx,ly,lz] != null)
					_blocks[lx,ly,lz].Chunk = this;
            	return _blocks[lx,ly,lz];
			}
			else
			{
				// Fallback if the block is not inside this chunk
				return _worldRef[_wx+lx, _wy+ly, _wz+lz];
			}
        }               
        set 
		{
			if (lx < _sizeX && ly < _sizeY && lz < _sizeZ)
			{
            	_blocks[lx,ly,lz] = value;
				_blocks[lx,ly,lz].Chunk = this;
			}
        }
    }
 
}
