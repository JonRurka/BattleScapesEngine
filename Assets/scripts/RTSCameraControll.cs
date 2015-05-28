using UnityEngine;
using System.Collections;

public class RTSCameraControll : MonoBehaviour
{
    public float moveSpeed = 10;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetMouseButton(0))
	    {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 movement = new Vector3(-mouseX * moveSpeed * Time.smoothDeltaTime, 0, -mouseY * moveSpeed * Time.smoothDeltaTime);
            transform.Translate(transform.InverseTransformDirection(movement));
	    }
	}
}
