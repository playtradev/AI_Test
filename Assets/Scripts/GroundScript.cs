using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GroundScript : MonoBehaviour {

	public static GroundScript Instance;

	public NavMeshSurface NMSurface;

	private void Awake()
	{
		Instance = this;
		NMSurface = GetComponent<NavMeshSurface>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void RunTimeBake()
	{
		NMSurface.BuildNavMesh();
	}
}
