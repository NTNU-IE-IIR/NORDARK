using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particle : MonoBehaviour {

	private GameObject Controller;
	public float localBestfittValue;
	public Vector3 localBestPosition;
	public Vector3 particleVelocity;
	//public Vector3 myposition;
	//public Vector3 mylocalPosition;
	public float fittValue;
	// Use this for initialization
	void Start () {

		
	}
	
	// Update is called once per frame
	void Update () {
		//myposition = this.transform.position;
		//mylocalPosition = this.transform.localPosition;
		
	}
	public void SetController (GameObject theController)
	{
		Controller = theController;

	}
}
