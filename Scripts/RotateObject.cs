using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour {

	public Vector3 rotateAxis;
	public float rotateSpeed = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.RotateAround( gameObject.GetComponent<Renderer>().bounds.center, rotateAxis, Time.deltaTime * rotateSpeed);
	}
}
