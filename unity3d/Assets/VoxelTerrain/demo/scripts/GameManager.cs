using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public enum EndGameState
    {
        Win,
        Loose
    }
    public static GameManager Instance;
    public static string Status;
    private static string TimeStr;
    private static float timeCount = 0.0f;

    public Vector3 playerSpawn;
    public GameObject playerPrefab;
    //public GameObject npcPrefab;
    public GameObject playerObj;
    //public GameObject npcObj;
    public GameObject UIcam;
    public bool enablePlayer;
    public bool enableNPC;
    public bool unlimitedAmmo;
    public bool randomSeed;
    public Texture2D[] ammoTexture;
    public Texture2D[] targetTexture;
    public bool mapLoaded;
    public float score;
    public int targetMultiplier = 100;
    public int ammoMultiplier = 1;
    public float secondsMultiplier = 0.1f; 
    public bool showSuccessMsg;
    public float destroyMessageTime = 2;
    public int maxNpcs = 12;
    //public List<NPCControll> NPClist;
    public GUIStyle labelStyle;
    public bool ShowStatus = true;
    public bool terrainPresent;
    public bool CreateTerrainOnLoad;
    public bool enableUI = true;
    public Texture2D[] splashImages;
    public int splashIndex;

    public PlayerControll playerScript;
    //private NPCControll npcScript;
    private int Sec = 0;
    private int min = 0;
    private string secZeroFillStr;
    private string minZeroFillStr;

    public int ChunkSizeX = 20;
    public int ChunkSizeY = 20;
    public int ChunkSizeZ = 20;

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Only one terrain controller allowed per scene. Destroying duplicate.");
            Destroy(this);
        }
    }

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
        EnableSplash();
        if (Application.loadedLevelName == "Main")
            StartTerrain();

        /*System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        watch.Stop();
        Debug.Log(watch.Elapsed.ToString());
        watch.Reset();*/
	}

    void FixedUpdate()
    {
        if (playerScript != null)
        {
            secZeroFillStr = string.Empty;
            minZeroFillStr = string.Empty;

            timeCount += Time.deltaTime;
            Sec = Mathf.FloorToInt(timeCount) % 60;
            min = (int)timeCount / 60;

            if (Sec < 10)
                secZeroFillStr = "0";

            if (min < 10)
                minZeroFillStr = "0";

            TimeStr = string.Format("{0} min {1} sec.", minZeroFillStr + min, secZeroFillStr + Sec);
            //print(TimeStr);
        }
    }
	
	// Update is called once per frame
	void Update () {
       /* if (playerScript != null){
            int ammoFired = playerScript.maxAmmo - playerScript.ammo;
            int targetsHit = maxNpcs - NPClist.Count;
            int ET = Mathf.FloorToInt(timeCount);
            score = Mathf.Max(0, (targetsHit * targetMultiplier) - (ammoFired * ammoMultiplier) - (ET * secondsMultiplier));
        }*/
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(0, 100, 0), Vector3.down);
        if (Physics.Raycast(ray, out hit, 10000) && !terrainPresent) 
        {
            terrainPresent = true;
            playerSpawn = new Vector3(hit.point.x, hit.point.y + 1, hit.point.z);
            OnRenderComplete();
        }
        //Debug.DrawRay(new Vector3(0, 100, 0), Vector3.down * 10000, Color.red);

        if (Input.GetKeyDown(KeyCode.F1))
            enableUI = !enableUI;
	}

    void OnGUI()
    {
        if (enableUI)
        {
            if (playerScript != null && enableUI)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 15;
                int currentAmmo = playerScript.ammo;
                int maxAmmo = playerScript.maxAmmo;
                labelStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(10, 10, 50, 20), "Ammo: ", labelStyle);
                for (int i = 0; i < maxAmmo; i++)
                    GUI.DrawTexture(new Rect(10 + 50 + i * 20, 13, 20, 18), ammoTexture[i >= currentAmmo ? 0 : 1]);
                //GUI.Label(new Rect(10, 20 + 10, 50, 20), "NPCs: ", labelStyle);
                /*for (int i = 0; i < maxNpcs; i++)
                {
                    bool npcTargetEmpty = i >= NPClist.Count;
                    GUI.DrawTexture(new Rect(10 + 50 + i * 20, 20 + 10 + 3, 20, 18), targetTexture[npcTargetEmpty ? 0 : 1]);
                    if (!npcTargetEmpty)
                        GUI.Label(new Rect(10 + 50 + i * 20, 20 + 10 + 3 + 10 + 2, 20, 20), (i + 1).ToString(), labelStyle);
                }*/
                //if (NPClist.Count == 0)
                //    GUI.Label(new Rect(10 + 50 + 1 * 20, 20 + 10 + 3 + 10, 20, 18), "0", labelStyle);
                /*GUI.Label(new Rect(10, 20 + 10 + 30, 50, 20), "Score: ", labelStyle);
                labelStyle.normal.textColor = Color.red;
                GUI.Label(new Rect(10 + 50, 20 + 10 + 30, 200, 20), score.ToString(), labelStyle);
                labelStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(10, 20 + 10 + 30 + 20, 50, 20), "Time: ");
                labelStyle.normal.textColor = Color.red;
                GUI.Label(new Rect(10 + 50, 20 + 10 + 30 + 20, 200, 20), TimeStr, labelStyle);*/
            }

            if (showSuccessMsg)
            {
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = Color.green;
                labelStyle.fontSize = 30;
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height * 1 / 4 - 25, 300, 50), "Enemy Destroyed!", labelStyle);
            }

            if (ShowStatus && enableUI)
                GUI.Label(new Rect(10, Screen.height - 30, Screen.width, 20), Status);
            if (TerrainController.Instance != null)
            {
                double voxelSize = ConvertBytesToMegabytes(TerrainController.Instance.VoxelSize);
                double meshSize = ConvertBytesToMegabytes(TerrainController.Instance.MeshSize);
                double totalSize = voxelSize + meshSize;
                GUI.Label(new Rect(10, Screen.height - 90, Screen.width, 20), "Voxel Size: " + voxelSize + " MB");
                GUI.Label(new Rect(10, Screen.height - 70, Screen.width, 20), "Mesh Size: " + meshSize + " MB");
                GUI.Label(new Rect(10, Screen.height - 50, Screen.width, 20), "Total Size: " + totalSize + " MB");
            }
        }

        if (Application.loadedLevelName == "Start")
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 50), "Start"))
                Application.LoadLevel("Main");
            Loom.DebugMode  = GUI.Toggle(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 60, 150, 20), Loom.DebugMode, "Show thread info");
            //enableNPC       = GUI.Toggle(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 80, 150, 20), enableNPC, "Spawn NPCs");
            unlimitedAmmo   = GUI.Toggle(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 100, 150, 20), unlimitedAmmo, "Unlimited ammo");
            randomSeed      = GUI.Toggle(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 120, 150, 20), randomSeed, "Random terrain");
            VoxelSettings.randomSeed = randomSeed;
        }
        else if (Application.loadedLevelName == "Win" || Application.loadedLevelName == "Loose")
        {
            if (GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 50), "Return to start"))
                Restart();
            GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 + 35, 100, 20), "Score: " + score.ToString());
        }

        if (TerrainController.ThereIsAnError)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.red;
            labelStyle.fontSize = 30;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height * 1 / 4 - 25, 300, 50), "Error in terrain!", labelStyle);
        }
    }

    void OnLevelWasLoaded(int level)
    {
        EnableSplash();
        if (level == 1)
        {
            StartTerrain();
        }
        else
            ReleaseMouse();
    }

    public void EnableSplash()
    {
        GameObject splash = GameObject.Find("BlackBackground");
        if (splash != null)
        {
            splashIndex = Random.Range(0, splashImages.Length);
            splash.GetComponent<Renderer>().material.SetTexture("_MainTex", splashImages[splashIndex]);
        }
        else
            Debug.LogError("Splash object is null!");
    }

    public void StartTerrain()
    {
        Debug.Log("Initializing terrain.");
        //TerrainController.OnRenderComplete += OnRenderComplete;
        //NPCControll.OnNpcDestroyed += OnNpcDestroyed;
        TerrainController.Init();
        UIcam = GameObject.Find("BlackBackground");
    }

    public void OnRenderComplete()
    {
        mapLoaded = true;
        if (enablePlayer)
        {
            SpawnPlayer(playerSpawn);
            /*if (enableNPC)
            {
                for (int i = 0; i < maxNpcs; i++)
                {
                    float maxDistanceX = VoxelSettings.MeterSizeX * VoxelSettings.radius;
                    float maxDistanceZ = VoxelSettings.MeterSizeZ * VoxelSettings.radius;
                    float xLoc = Random.Range(playerSpawn.x - maxDistanceX, playerSpawn.x + maxDistanceX);
                    float zLoc = Random.Range(playerSpawn.z - maxDistanceZ, playerSpawn.z + maxDistanceZ);
                    SpawnNPC(new Vector3(xLoc, NPCControll.yHeight, zLoc));
                }
            }*/
        }
    }

    public void OnNpcDestroyed(object npc)
    {
        //score += scoreIncreaseValue;
        showSuccessMsg = true;
        //NPClist.Remove((NPCControll)npc);
        playerScript.FillAmmo();
        Invoke("disableSuccessMsg", destroyMessageTime);
    }

    public void SpawnNPC(Vector3 Spawn)
    {
        if (mapLoaded)
        {
            //npcObj = (GameObject)Instantiate(npcPrefab, Spawn, Quaternion.identity);
            //npcObj.name = "NPC";
            //npcScript = npcObj.GetComponent<NPCControll>();
            //NPClist.Add(npcScript);
        }
    }

    public void SpawnPlayer(Vector3 Spawn)
    {
        if (terrainPresent)
        {
            playerObj = (GameObject)Instantiate(playerPrefab, playerSpawn, Quaternion.identity);
            playerObj.name = "Player";
            TerrainController.Instance.player = playerObj;
            playerScript = playerObj.GetComponentInChildren<PlayerControll>();
            playerScript.unlimitedAmmo = unlimitedAmmo;
            Destroy(UIcam);
        }
        else
            GameManager.Status = "Terrain not found.";
    }

    public void EndGame(EndGameState state)
    {
        playerScript.FillAmmo();
        //Application.LoadLevel(state.ToString());
        //ReleaseMouse();
    }

    public void Restart()
    {
        Instance = null;
        TerrainController.OnRenderComplete -= OnRenderComplete;
        //NPCControll.OnNpcDestroyed -= OnNpcDestroyed;
        TerrainController.Instance.ClearChunks();
        TerrainController.blockTypes.Clear();
        TerrainController.Chunks.Clear();
        TerrainController.Instance = null;
        timeCount = 0.0f;
        playerScript = null;
        Destroy(gameObject);
        Application.LoadLevel("Start");
    }

    public void ReleaseMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void disableSuccessMsg()
    {
        showSuccessMsg = false;
        //if (NPClist.Count <= 0)
        //    EndGame(EndGameState.Win);
        //if (EnableNPC)
        //    SpawnNPC(new Vector3(Random.Range(0, VoxelSettings.MeterSizeX * VoxelSettings.maxChunksX), 65, Random.Range(0, VoxelSettings.MeterSizeZ * VoxelSettings.maxChunksZ)));
    }

    private double ConvertBytesToMegabytes(long bytes)
    {
        return System.Math.Round((bytes / 1024.0) / 1024.0, 3);
    }

    private bool IsInBounds(int x, int y, int z)
    {
        return ((x <= ChunkSizeX - 1) && x >= 0) && ((y <= ChunkSizeY - 1) && y >= 0) && ((z <= ChunkSizeZ - 1) && z >= 0);
    }
}
