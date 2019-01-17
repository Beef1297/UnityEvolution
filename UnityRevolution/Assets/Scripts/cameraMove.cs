using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour {

    [SerializeField]
    private float speed = 0.2f;

    [SerializeField]
    [Range(10f, 100f)]
    private float radius = 30f;

    [SerializeField]
    private float height = 10.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 camPosition = new Vector3(Mathf.Sin(Time.time*speed) * radius, height, Mathf.Cos(Time.time*speed) * radius);
        this.transform.position = camPosition;
        this.transform.rotation = Quaternion.LookRotation(-camPosition + new Vector3(0, -10f, 0f));
	}
}
