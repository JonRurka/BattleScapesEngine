using UnityEngine;
using System.Collections;

public class CutoffModifier : MonoBehaviour {
	public float cutoff = 0f;
	void Update () {
		GetComponent<Renderer>().material.SetFloat("_Cutoff", cutoff);
	}
}
