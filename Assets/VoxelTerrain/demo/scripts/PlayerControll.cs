using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerControll : MonoBehaviour 
{
    public GameObject ProjectilePrefab;
    public GameObject PopSound;
    public GameObject lightObj;
    public FirstPersonController controller;
    public int explosivePower = 2;
    public int maxAmmo = 25;
    public int ammo;
    public Texture2D crossHair;
    public bool unlimitedAmmo = true;
    public Camera cam;
    public LayerMask mask;

    RaycastHit _hit;
    Quaternion _pauseRotation;

	// Use this for initialization
	void Start () {
        cam = GetComponent<Camera>();
        _pauseRotation = transform.rotation;
        ammo = maxAmmo;
	}
	
	// Update is called once per frame
	void Update () {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (Input.GetMouseButtonDown(0) && ammo > 0)
            {
                Shoot();
            }
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out _hit, 100, mask))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    TerrainController.Instance.ChangeBlock(_hit.point, 4, _hit.normal, false);
                }
                Debug.DrawLine(transform.position, _hit.point, Color.red);
            }
            _pauseRotation = transform.rotation;
            lightObj.transform.rotation = transform.rotation;
        }
        else
        {
            transform.rotation = _pauseRotation;
            lightObj.transform.rotation = _pauseRotation;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            controller.enabled = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReleaseMouse();
        }
	}

    void OnGUI()
    {
        if (GameManager.Instance.enableUI)
            GUI.DrawTexture(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200), crossHair);
    }

    public void Shoot()
    {
        GameObject projectileObj = (GameObject)Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);
        GameObject popSoundObj = (GameObject)Instantiate(PopSound, transform.position, Quaternion.identity);
        popSoundObj.transform.parent = transform;
        projectileObj.GetComponent<Rigidbody>().AddForce(transform.TransformDirection(0, 0, 2000));
        projectileObj.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        projectileObj.GetComponent<ProjectileScript>().type = ProjectileScript.ProjectileType.Player;
        projectileObj.GetComponent<ProjectileScript>().explosionPower = explosivePower;
        Destroy(popSoundObj, 1);
        if (!unlimitedAmmo)
            ammo--;
        if (ammo <= 0)
            GameManager.Instance.EndGame(GameManager.EndGameState.Loose);
    }

    public void ReleaseMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        controller.enabled = false;
    }

    public void FillAmmo()
    {
        ammo = maxAmmo;
    }
}
