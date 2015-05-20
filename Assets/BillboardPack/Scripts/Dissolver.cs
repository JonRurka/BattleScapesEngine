using UnityEngine;
using System.Collections;

public class Dissolver : MonoBehaviour {
	
	void Update () {
		GetComponent<Renderer>().material.SetFloat("_Cutoff", Mathf.Clamp01( Mathf.PingPong( Time.time * 0.5f, 1.6f)));
	}
}
