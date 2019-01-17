using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attractor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = new Vector3(Mathf.Sin(Time.time * 0.3f) * 20f, Mathf.Sin(Time.time * 1.33f * 0.3f) * 20f, Mathf.Cos(Time.time * 0.3f) * 20f);
	}
}
