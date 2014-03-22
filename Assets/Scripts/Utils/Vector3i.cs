using UnityEngine;
public struct Vector3i {
	
	public int x, y, z;
	
	public static readonly Vector3i zero = new Vector3i(0,0,0);
	public static readonly Vector3i one = new Vector3i(1, 1, 1);
	public static readonly Vector3i forward = new Vector3i(0, 0, 1);
	public static readonly Vector3i back = new Vector3i(0, 0, -1);
	public static readonly Vector3i up = new Vector3i(0, 1, 0);
	public static readonly Vector3i down = new Vector3i(0, -1, 0);
	public static readonly Vector3i left = new Vector3i(-1, 0, 0);
	public static readonly Vector3i right = new Vector3i(1, 0, 0);
	
	public static readonly Vector3i[] directions = new Vector3i[] {
		left, right,
		back, forward,
		down, up,
	};
	
	public Vector3i( Vector3 vector3) {
		this.x = Mathf.RoundToInt(vector3.x);
		this.y = Mathf.RoundToInt(vector3.y);
		this.z = Mathf.RoundToInt(vector3.z);
	}
	
	public Vector3i(int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}
	
	public Vector3i(int x, int y) {
		this.x = x;
		this.y = y;
		this.z = 0;
	}
	
	public static int DistanceSquared(Vector3i a, Vector3i b) {
		int dx = b.x-a.x;
		int dy = b.y-a.y;
		int dz = b.z-a.z;
		return dx*dx + dy*dy + dz*dz;
	}
	
	public int DistanceSquared(Vector3i v) {
		return DistanceSquared(this, v);
	}
	
	public override int GetHashCode () {
		return x.GetHashCode () ^ y.GetHashCode () << 2 ^ z.GetHashCode () >> 2;
	}
	
	public override bool Equals(object other) {
		if (!(other is Vector3i)) return false;
		Vector3i vector = (Vector3i) other;
		return x == vector.x && 
			   y == vector.y && 
			   z == vector.z;
	}
	
	public override string ToString() {
		return "Vector3i("+x+" "+y+" "+z+")";
	}
	
	public static bool operator == (Vector3i a, Vector3i b) {
		return a.x == b.x && 
			   a.y == b.y && 
			   a.z == b.z;
	}
	
	public static bool operator != (Vector3i a, Vector3i b) {
		return a.x != b.x ||
			   a.y != b.y ||
			   a.z != b.z;
	}
	
	public static Vector3i operator - (Vector3i a, Vector3i b) {
		return new Vector3i( a.x-b.x, a.y-b.y, a.z-b.z);
	}
	
	public static Vector3i operator + (Vector3i a, Vector3i b) {
		return new Vector3i( a.x+b.x, a.y+b.y, a.z+b.z);
	}
	
	public static explicit operator Vector3(Vector3i v) {
		return new Vector3(v.x, v.y, v.z);
	}
	
}
