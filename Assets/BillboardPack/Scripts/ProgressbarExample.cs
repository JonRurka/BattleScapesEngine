using UnityEngine;
using System.Collections;

public class ProgressbarExample : MonoBehaviour {
	public float progressPercent = 100f;
	// Update is called once per frame
	void Update () {
		float f =Mathf.Clamp01(progressPercent * 0.01f);
		GetComponent<Renderer>().material.SetFloat("_Progress", f);
	}
}
