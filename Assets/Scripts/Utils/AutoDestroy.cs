using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour 
{
	public float _time;
	
	IEnumerator Start () 
	{
		yield return new WaitForSeconds(_time);
		GameObject.DestroyImmediate(gameObject);
	}
}
