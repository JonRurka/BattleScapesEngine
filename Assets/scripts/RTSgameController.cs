using UnityEngine;
using System.Collections;

public class RTSgameController : MonoBehaviour
{
    public static RTSgameController Instance;
    public GameObject cameraObj;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Only one game controller allowed per scene. Destroying duplicate.");
            Destroy(this);
        }
    }

	// Use this for initialization
	void Start ()
	{
        GameObject cameraObj = GameObject.Find("Main Camera");
	    if (cameraObj != null)
	    {
	        RTSterrainControll.Instance.cameraObj = cameraObj;
            RTSterrainControll.Init();
	    }
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
