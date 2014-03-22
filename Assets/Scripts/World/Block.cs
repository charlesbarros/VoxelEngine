using System.Collections.Generic;
using UnityEngine;

public enum BlockType 
{
	Grass=0,
	Dirt,
	Stone,
	Lava,
	Air,
	Height,
	Unknown,
	WorldBound 
};
// Height-Type: is the first non-solid block over the highest block of its collumn
// Unknown-Type: is used as return type of some methods instead of just return Null
// WorldBound-Type: are the edge of the world. You know, there is no such thing like REAL infinite world...


public static class BlockTypes
{
    private static BlockBase[] _types = new BlockBase[] { 
		new GrassType(), new DirtType(), new StoneType(), new LavaType(), new AirType(), new HeightType(), new UnknownType(), new WorldBoundType() };
     
    public static BlockBase GetBlockType(BlockType type)
    {
         return _types[(int)type];
    }
}

// Note: This class should be a struct to use less memory per block, like the following article points out:
// http://www.blockstory.net/node/59
// But unfortunatly some bad decisions during this project made me change back to a class
public class Block
{
	private static int _minLight = 51;
	public static int MinLight { get { return _minLight; } }
	
	private static int _maxLight = 255;
	public static int MaxLight { get { return _maxLight; } }
	
	private BlockType _type; 
	public BlockType Type { get { return _type; } set { _type = value; } }
	
	private int _light;
	public int Light 
	{ 
		get { return _light; }
		set 
		{ 
			int light =  Mathf.Clamp(value, _minLight, _maxLight);
			if (_light != light)
			{
				_light = light;
				if (_chunk != null)
					_chunk.SetDirty();
			}
		} 
	}
	
	private Chunk _chunk;
	public Chunk Chunk { get { return _chunk; } set { _chunk = value; } }
	
	public int _wx = 0;
	public int _wy = 0;
	public int _wz = 0;

	public Block(BlockType type, int wx = 0, int wy = 0, int wz = 0)
	{
		_wx = wx;
		_wy = wy;
		_wz = wz;
		_type = type;
		
		if (_type == BlockType.Height)
		{
			_light = _maxLight;	
		}
		else
		{
			_light = _minLight;
		}	
	}
	
    // Those methods looks up what class should serve this request, and forwards it
	// Note: remember this was supposed  to be a struct...
    public bool IsSolid() 
	{
       return BlockTypes.GetBlockType(_type).IsSolid(this);
    }
	
	public Vector2 TextureBottomUV()
	{
		return BlockTypes.GetBlockType(_type).TextureBottomUV(this);
	}
	
	public Vector2 TextureTopUV()
	{
		return BlockTypes.GetBlockType(_type).TextureTopUV(this);
	}

	public Vector2 TextureSideUV()
	{
		return BlockTypes.GetBlockType(_type).TextureSideUV(this);
	}
	
	public Vector2 TextureUV()
	{
		return BlockTypes.GetBlockType(_type).TextureUV(this);
	}
	
	public bool Destroy()
	{
		return BlockTypes.GetBlockType(_type).Destroy(this);
	}
	
	public bool Create(BlockType type)
	{
		return BlockTypes.GetBlockType(_type).Create(this, type);
	}
}
 
// Base class for all type of blocks
public class BlockBase
{
    public virtual bool IsSolid(Block block)
	{
		return false;
	}

	public virtual Vector2 TextureBottomUV(Block block)
	{
		 return Vector2.zero;
	}
	
	public virtual Vector2 TextureTopUV(Block block)
	{
		 return Vector2.zero;
	}
	
	public virtual Vector2 TextureSideUV(Block block)
	{
		 return Vector2.zero;
	}
	
	public virtual Vector2 TextureUV(Block block)
	{
		 return Vector2.zero;
	}
	
	public virtual bool Destroy(Block block)
	{
		if (IsSolid(block) == false)
		{
			return false;
		}

		if (block.Chunk != null)
		{
			block.Chunk.SetDirty();
		}
		block.Type = BlockType.Air;
		return true;
	}
	
	public virtual bool Create(Block block, BlockType type)
	{
		if (IsSolid(block) == true)
		{
			return false;
		}
		
		if (block.Chunk != null)
		{
			block.Chunk.SetDirty();
		}
		block.Type = type;
		return true;
	}
}
 
#region DirtBlock
public class DirtType : BlockBase
{
    public override bool IsSolid(Block block)
    {
        return true;
    }
	
    public override Vector2 TextureBottomUV(Block block)
    {
        return new Vector2(0.00f, 0.25f);
    }
	
    public override Vector2 TextureTopUV(Block block)
    {
        return new Vector2(0.00f, 0.75f);
    }

    public override Vector2 TextureSideUV(Block block)
    {
        return new Vector2(0.00f, 0.0f);
    }
	
    public override Vector2 TextureUV(Block block)
    {
        return new Vector2(0.00f, 0.50f);
    }
}
#endregion

#region StoneBlock
public class StoneType : BlockBase
{
    public override bool IsSolid(Block block)
    {
        return true;
    }

    public override Vector2 TextureBottomUV(Block block)
    {
        return new Vector2(0.50f, 0.25f);
    }
	
    public override Vector2 TextureTopUV(Block block)
    {
        return new Vector2(0.50f, 0.75f);
    }

    public override Vector2 TextureSideUV(Block block)
    {
        return new Vector2(0.50f, 0.0f);
    }
	
    public override Vector2 TextureUV(Block block)
    {
        return new Vector2(0.50f, 0.50f);
    }
}
#endregion

#region GrassBlock
class GrassType : BlockBase
{
    public override bool IsSolid(Block block)
    {
        return true;
    }
	
    public override Vector2 TextureBottomUV(Block block)
    {
         return new Vector2(0.25f, 0.25f);
    }
	
    public override Vector2 TextureTopUV(Block block)
    {
         return new Vector2(0.25f, 0.75f);
    }

    public override Vector2 TextureSideUV(Block block)
    {
         return new Vector2(0.25f, 0.0f);
	}
    
    public override Vector2 TextureUV(Block block)
    {
         return new Vector2(0.25f, 0.50f);
    }
}
#endregion

#region LavaBlock
public class LavaType : BlockBase
{
    public override bool IsSolid(Block block)
    {
        return true;
    }

    public override Vector2 TextureBottomUV(Block block)
    {
		return new Vector2(0.75f, 0.25f);
    }
	
    public override Vector2 TextureTopUV(Block block)
    {
		return new Vector2(0.75f, 0.75f);
    }

    public override Vector2 TextureSideUV(Block block)
    {
		return new Vector2(0.75f, 0.0f);
    }
	
    public override Vector2 TextureUV(Block block)
    {
		return new Vector2(0.75f, 0.50f);
    }
}
#endregion

#region Non-solidBlocks
public class AirType : BlockBase
{
    public override bool IsSolid(Block block)
    {
        return false;
    }
}
 
public class HeightType : BlockBase
{
    public override bool IsSolid(Block block)
    {
        return false;
    }
}

public class WorldBoundType : BlockBase
{
    public override bool IsSolid(Block block)
    {
        return false;
    }
}

public class UnknownType : BlockBase
{
    public override bool IsSolid(Block block)
    {
        return false;
    }
}
#endregion
 