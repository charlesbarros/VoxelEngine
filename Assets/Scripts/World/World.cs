/// <summary>
/// This class is heavily based on this tutorial: http://www.blockstory.net/node/58
/// As the tutorial suggests, there is many optimizations that could be applied, 
/// but for the sake of simplicity I'm distributing the source without it.
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World
{
    // How far the player can see
    const int _visibleX = 12;
	const int _visibleY = 1;
	const int _visibleZ = 12;
	
	public int VisibleX { get { return _visibleX; } }
	public int VisibleY { get { return _visibleY; } }
	public int VisibleZ { get { return _visibleZ; } }
	
	private int _worldVisibleSizeX;
	private int _worldVisibleSizeY;
	private int _worldVisibleSizeZ;

	public int WorldVisibleSizeX { get { return _worldVisibleSizeX; } }
	public int WorldVisibleSizeY { get { return _worldVisibleSizeY; } }
	public int WorldVisibleSizeZ { get { return _worldVisibleSizeZ; } }
	
    private Chunk[,,] _chunks;
	public Chunk[,,] Chunks { get { return _chunks; } }

	private static bool _lightAttenuation;
	public static bool LightAttenuation { set { _lightAttenuation = value; } }
	
	public World()
	{
		_chunks = new Chunk[_visibleX, _visibleY, _visibleZ];

		for(int x = 0; x < _visibleX; x++)
			for(int y = 0; y < _visibleY; y++)
				for(int z = 0; z < _visibleZ; z++)
					_chunks[x,y,z] = new Chunk(this,x,y,z);
		
		_worldVisibleSizeX = _visibleX * Chunk.SizeX;
		_worldVisibleSizeY = _visibleY * Chunk.SizeY;
		_worldVisibleSizeZ = _visibleZ * Chunk.SizeZ;
	}

	// Create a chunk
	public void CreateChunkMesh(int x, int y, int z, ChunkObject chunkObject)
	{
		chunkObject.ChunkMesh.mesh = ChunkRenderer.Render(_chunks[x,y,z]);
		chunkObject.ChunkCollider.sharedMesh = chunkObject.ChunkMesh.mesh;
		_chunks[x,y,z].ChunkObject = chunkObject;
	}
	
	// Refresh chunk and neighbors mesh by a given world position
	public IEnumerator RefreshChunkMesh(Vector3i worldPos, bool async = false)
	{
		ChunkRenderer.UpdateHeightMap(this, worldPos, _worldVisibleSizeY);
		
		if (_lightAttenuation == true)
			ChunkRenderer.LightningFloodArea(this, worldPos, _worldVisibleSizeY);
	
		Vector3i blockPosInsideChunk = WorldToChunkPosition(worldPos);
		
		// Refresh Main Chunk (the chunk that contains the worldPosition)
		Vector3i chunkIndex = GetChunkIndex(worldPos);
		RefreshChunkMeshByIndex( chunkIndex );
		if (async) yield return null;

		// Refresh chunk neighbors if needed
		#region RefreshChunkNeighbors
		if (blockPosInsideChunk.y >= Chunk.SizeY-1)
		{
			Vector3i neighborChunkIndex = chunkIndex;
			neighborChunkIndex.y++;
			RefreshChunkMeshByIndex(neighborChunkIndex);
			if (async) yield return null;
		}
		if (blockPosInsideChunk.y <= 0)
		{
			Vector3i neighborChunkIndex = chunkIndex;
			neighborChunkIndex.y--;
			RefreshChunkMeshByIndex(neighborChunkIndex);
			if (async) yield return null;
		}
		
		if (blockPosInsideChunk.x >= Chunk.SizeX-1)
		{
			Vector3i neighborChunkIndex = chunkIndex;
			neighborChunkIndex.x++;
			RefreshChunkMeshByIndex(neighborChunkIndex);
			if (async) yield return null;
		}
		if (blockPosInsideChunk.x <= 0)
		{
			Vector3i neighborChunkIndex = chunkIndex;
			neighborChunkIndex.x--;
			RefreshChunkMeshByIndex(neighborChunkIndex);	
			if (async) yield return null;
		}
	
		if (blockPosInsideChunk.z >= Chunk.SizeZ-1)
		{
			Vector3i neighborChunkIndex = chunkIndex;
			neighborChunkIndex.z++;
			RefreshChunkMeshByIndex(neighborChunkIndex);
			if (async) yield return null;
		}
		if (blockPosInsideChunk.z <= 0)
		{
			Vector3i neighborChunkIndex = chunkIndex;
			neighborChunkIndex.z--;
			RefreshChunkMeshByIndex(neighborChunkIndex);	
			if (async) yield return null;
		}
		#endregion
	}
	
	// Refresh chunk mesh by chunk index
	public void RefreshChunkMeshByIndex(Vector3i chunkIndex)
	{
		int x = chunkIndex.x;
		int y = chunkIndex.y;
		int z = chunkIndex.z;
		
		if ( (x >= 0 && x < _visibleX) && (y >= 0 && y < _visibleY) && (z >= 0 && z < _visibleZ) )
		{
			_chunks[x,y,z].SetDirty();
		}
	}
	
	// Convert world to chunk position
	public Vector3i WorldToChunkPosition(Vector3i worldPos)
    {
		Vector3i cPos = GetChunkIndex(worldPos);
		
		cPos.x = worldPos.x % Chunk.SizeX;
        cPos.y = worldPos.y % Chunk.SizeY;
        cPos.z = worldPos.z % Chunk.SizeZ;
			
		return cPos;
    }    
	
	// Get chunk index based on world position
	public Vector3i GetChunkIndex(Vector3i worldPos)
    {
		int wx = worldPos.x;
		int wy = worldPos.y;
		int wz = worldPos.z;
		
		if (wx < 0 || wy < 0 || wz < 0)
			return Vector3i.zero;

		if (wx >= _worldVisibleSizeX || wy >= _worldVisibleSizeY || wz >= _worldVisibleSizeZ)
			return Vector3i.zero;
		
        // first calculate which chunk we are talking about:
		int cx = (wx / Chunk.SizeX);
		int cy = (wy / Chunk.SizeY);
		int cz = (wz / Chunk.SizeZ);
		
        // request can be out of range, then return a Unknown block type
        if (cx < 0 || cy < 0 || cz < 0)
            return Vector3i.zero;
        if (cx >= _visibleX || cy >= _visibleY || cz >= _visibleZ)
            return Vector3i.zero;

        return new Vector3i(cx, cy, cz);
    }    

    // Get the block at position (wx,wy,wz) where these are the world coordinates
    public Block this[int wx, int wy, int wz]
    {
        get {
			if (wx < 0 || wy < 0 || wz < 0)
				return new Block(BlockType.WorldBound);

			if (wx >= _worldVisibleSizeX || wy >= _worldVisibleSizeY || wz >= _worldVisibleSizeZ)
				return new Block(BlockType.WorldBound);
			
            // first calculate which chunk we are talking about:
			int cx = (wx / Chunk.SizeX);
			int cy = (wy / Chunk.SizeY);
			int cz = (wz / Chunk.SizeZ);
			
            // request can be out of range, then return a Unknown block type
            if (cx < 0 || cy < 0 || cz < 0)
                return new Block(BlockType.Unknown);
            if (cx >= _visibleX || cy >= _visibleY || cz >= _visibleZ)
                return new Block(BlockType.Unknown);

            Chunk chunk = _chunks[cx,cy,cz];
 
            // this figures out the coordinate of the block relative to chunk origin.
            int lx = wx % Chunk.SizeX;
            int ly = wy % Chunk.SizeY;
            int lz = wz % Chunk.SizeZ;
 
            return chunk[lx, ly, lz];
        }               
        set {
            // first calculate which chunk we are talking about:
			int cx = (wx / Chunk.SizeX);
			int cy = (wy / Chunk.SizeY);
			int cz = (wz / Chunk.SizeZ);

            // cannot modify _chunks that are not within the visible area
			bool insideVisibleBounds = true;
			
            if (cx < 0 || cx >= _visibleX)
               insideVisibleBounds = false;
            if (cy < 0 || cy >= _visibleY)
               insideVisibleBounds = false;
            if (cz < 0 || cz >= _visibleZ)
               insideVisibleBounds = false;
			
			if (insideVisibleBounds == true)
			{
	            Chunk chunk = _chunks[cx,cy,cz];
	 
	            // this figures out the coordinate of the block relative to chunk origin.
				int lx = wx % Chunk.SizeX;
				int ly = wy % Chunk.SizeY;
				int lz = wz % Chunk.SizeZ;
	
	            chunk[lx,ly,lz] = value;
			}
        }
    }
	

}