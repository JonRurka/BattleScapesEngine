using UnityEngine;
using System.Collections;

public class ProjectileScript : MonoBehaviour {
    public enum ProjectileType
    {
        Player,
        NPC,
        None
    }
    public GameObject partical;
    public int explosionPower;
    public GameObject audioPrefab;
    public ProjectileType type = ProjectileType.None;

    Rigidbody rigidBody;
    bool exploded = false;

	// Use this for initialization
	void Start () {
        Destroy(gameObject, 10);
        rigidBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("enter " + other.gameObject.name);
        if (type == ProjectileType.Player && other.gameObject.tag == "Player")
            return;

        if (type == ProjectileType.NPC && other.gameObject.tag == "NPC")
            return;

        if (!exploded)
        {
            //Debug.Log("explode " + other.gameObject.name);
            exploded = true;
            TerrainController.Instance.GenerateExplosion(rigidBody.position, explosionPower);
            GameObject audio = (GameObject)Instantiate(audioPrefab, rigidBody.position, Quaternion.identity);
            GameObject particalObj = (GameObject)Instantiate(partical, rigidBody.position, Quaternion.identity);
            audio.GetComponent<AudioSource>().Play();
            Destroy(particalObj, 5);
            Destroy(audio, 5);
            Destroy(gameObject);
            //if (type == ProjectileType.Player && other.gameObject.tag == "NPC")
            //    other.GetComponent<NPCControll>().Break();
        }
    }

    void OnTriggerStay(Collider other)
    {
        /*Debug.Log("stay " + other.gameObject.name);
        if (type == ProjectileType.Player && other.gameObject.tag == "Player")
            return;

        if (type == ProjectileType.NPC && other.gameObject.tag == "NPC")
            return;

        if (!exploded)
        {
            Debug.Log("explode " + other.gameObject.name);
            exploded = true;
            TerrainController.Instance.GenerateExplosion(rigidBody.position, explosionPower);
            GameObject audio = (GameObject)Instantiate(audioPrefab, rigidBody.position, Quaternion.identity);
            GameObject particalObj = (GameObject)Instantiate(partical, rigidBody.position, Quaternion.identity);
            audio.GetComponent<AudioSource>().Play();
            Destroy(particalObj, 5);
            Destroy(audio, 5);
            Destroy(gameObject);
            if (type == ProjectileType.Player && other.gameObject.tag == "NPC")
                other.GetComponent<NPCControll>().Break();
        }*/
    }
}
