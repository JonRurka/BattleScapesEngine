using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LibNoise;

public class TerrainController : MonoBehaviour, IPageController
{
    public static string WorldThreadName = "GenerationThread";
    public static string setBlockThreadName = "SetBlockThread";
    public static bool ThereIsAnError = false;
    public static TerrainController Instance;
    public Texture2D textureAtlas;
    public Rect[] AtlasUvs;
    public Material chunkMaterial;
    public int seed = 0;
    public Vector3Int newPlayerChunkPos;
    public int chunksInQueue;
    public int chunksGenerated;
    public int VoxelSize;
    public int MeshSize;

    public delegate void RenderComplete();
    public static event RenderComplete OnRenderComplete;


    public static Dictionary<byte, BlockType> blockTypes = new Dictionary<byte, BlockType>();

    // Temparary simple 3D array. Will use dictionary when I switch over to unlimited terrain gen.
    public static SafeDictionary<Vector3Int, Chunk> Chunks = new SafeDictionary<Vector3Int, Chunk>();

    public GameObject player;
    public GameObject chunkPrefab;
    public Texture2D[] AllCubeTextures;
    public BlockType[] BlocksArray;
    public Vector3Int generateArroundChunk = new Vector3Int(0, 0, 0);

    List<Vector3Int> _tmpChunkList = new List<Vector3Int>();

    bool _generating = false;

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
        
	}
	
	// Update is called once per frame
	void Update () {
        
        if (player != null)
        {
            Vector3Int voxel = VoxelConversions.WorldToVoxel(player.transform.position);
            newPlayerChunkPos = VoxelConversions.VoxelToChunk(voxel);
            //Debug.Log(generateArroundChunk + ", " + newPlayerChunkPos + ", " + Vector3.Distance(generateArroundChunk, newPlayerChunkPos));
            if (Vector3.Distance(generateArroundChunk, newPlayerChunkPos) > 0 && !_generating)
            {
                // generate around point.
                //Debug.Log("Debug filling " + newPlayerChunkPos + ".");
                generateArroundChunk = newPlayerChunkPos;
                GenerateSpherical(generateArroundChunk, null);
            }
        }
        
        if (chunksInQueue > 0)
            GameManager.Status = string.Format("Generating chunk {0}/{1}.", chunksGenerated, chunksInQueue);
	}

    public static void Init()
    {
        Instance.init();
    }

    public void init()
    {
        Loom.AddAsyncThread(WorldThreadName);
        Loom.AddAsyncThread(setBlockThreadName);
        textureAtlas = new Texture2D(0, 0);
        AtlasUvs = textureAtlas.PackTextures(AllCubeTextures, 1);
        AddBlockType(BaseType.air, "Air", new int[] { -1, -1, -1, -1, -1, -1 }, null);
        AddBlockType(BaseType.solid, "Grass", new int[] { 0, 0, 0, 0, 0, 0 }, null);
        AddBlockType(BaseType.solid, "Rock", new int[] { 1, 1, 1, 1, 1, 1 }, null);
        AddBlockType(BaseType.solid, "Dirt", new int[] { 2, 2, 2, 2, 2, 2 }, null);
        AddBlockType(BaseType.solid, "Brick", new int[] { 3, 3, 3, 3, 3, 3 }, null);
        if (VoxelSettings.randomSeed)
            VoxelSettings.seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        else
            VoxelSettings.seed = seed;
        //TestSmooth();
        //SpawnChunksLinear();
        GameManager.Status = "Loading...";
        //Action action = new Action(OnRenderComplete);
        GenerateSpherical(generateArroundChunk, null);
    }

    public void TestSmooth()
    {
        Vector3Int chunkPos = new Vector3Int(0, 0, 0);
        SpawnChunk(chunkPos);
        GenerateChunk(chunkPos, null);
    }

    public void GenerateSpherical(Vector3Int center, Action onComplete)
    {
        Loom.QueueAsyncTask(WorldThreadName, () =>
        {
            Vector3Int[] chunkPositions = GetChunkLocationsAroundPoint(center);
            chunksInQueue = chunkPositions.Length;
            Loom.QueueOnMainThread(() =>
            {
                SpawnChunks(chunkPositions);
                GenerateChunks(chunkPositions, onComplete);
            });
        });
    }

    public Vector3Int[] GetChunkLocationsAroundPoint(Vector3Int center)
    {
        List<Vector3Int> chunksInSphere = new List<Vector3Int>();
        for (int i = 0; i < VoxelSettings.radius + 1; i++)
        {
            for (int x = center.x - i; x < center.x + i; x++)
            {
                for (int z = center.z - i; z < center.z + i; z++)
                {
                    for (int y = center.y + VoxelSettings.maxChunksY_M; y > center.y - VoxelSettings.maxChunksY_P; y--)
                    {
                        if (!BuilderExists(x, y, z) && IsInSphere(center, VoxelSettings.radius, new Vector3Int(x, center.y, z)) && !chunksInSphere.Contains(new Vector3Int(x, y, z)))
                            chunksInSphere.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }
        return chunksInSphere.ToArray();
    }

    public void SpawnDebugChunks()
    {
        SpawnChunkFeild();

        Loom.QueueAsyncTask(WorldThreadName, () => 
        {
            /*Chunks[1, 0, 0].DebugFill(1);
            SafeDebug.Log("Filling 1, 0.");
            Chunks[1, 0, 0].Render(true);
            SafeDebug.Log("Rendering 1, 0.");

            Chunks[1, 1, 0].DebugFill(2);
            Chunks[1, 1, 0].Render(true);
            SafeDebug.Log("Rendering 1, 1.");

            Chunks[2, 0, 0].DebugFill(3);
            Chunks[2, 0, 0].Render(true);
            SafeDebug.Log("Rendering 2, 0.");

            Chunks[2, 1, 0].DebugFill(4);
            Chunks[2, 1, 0].Render(true);
            SafeDebug.Log("Rendering 2, 1.");

            SafeDebug.Log("Finished Debug rendering.");*/
        });

    }

    public void SpawnChunksLinear()
    {
        SpawnChunkFeild();
        Loom.QueueAsyncTask(WorldThreadName, () =>
        {
            TerrainModule module = new TerrainModule(VoxelSettings.seed);
            for (int x = 0; x < VoxelSettings.maxChunksX; x++)
            {
                for (int z = 0; z < VoxelSettings.maxChunksZ; z++)
                {
                    for (int y = 0; y < VoxelSettings.maxChunksY_M; y++)
                    {
                        Vector3Int location3D = new Vector3Int(x, y, z);
                        GenerateChunk(location3D, module);
                    }
                }
            }
            SafeDebug.Log("Finished rendering.");
            Loom.QueueOnMainThread(() =>
            {
                _generating = false;
                OnRenderComplete();
            });
        });
    }

    public void GenerateChunks(Vector3Int[] chunkLocations, Action onComplete)
    {
        Loom.QueueAsyncTask(WorldThreadName, () =>
        {
            //SafeDebug.Log("Generating " + chunkLocations.Length + " chunks.");
            TerrainModule module = new TerrainModule(VoxelSettings.seed);
            foreach (Vector3Int location3D in chunkLocations)
            {
                GenerateChunk(location3D, module);
                chunksGenerated++;
            }
            SetVoxelSize();
            setMeshSize();
            Loom.QueueOnMainThread(() =>
            {
                _generating = false;
                chunksInQueue = 0;
                chunksGenerated = 0;
                GameManager.Status = "";
                if (onComplete != null)
                    onComplete();
            });
        });
    }

    public void GenerateChunk(Vector3Int location3D, IModule module)
    {
        try
        {
            if (BuilderExists(location3D.x, location3D.y, location3D.z) && !Chunks[new Vector3Int(location3D.x, location3D.y, location3D.z)].Generated)
            {
                _generating = true;
                Chunks[new Vector3Int(location3D.x, location3D.y, location3D.z)].GenerateChunk(module);
                Chunks[new Vector3Int(location3D.x, location3D.y, location3D.z)].Render(false);
            }
        }
        catch (Exception e)
        {
            SafeDebug.LogError(e.Message + "\nFunction: GenerateChunks" + "\n" + location3D.x + "," + location3D.y + "," + location3D.y, e);
        }
    }

    public void AddBlockType(BaseType _baseType, string _name, int[] _textures, GameObject _prefab)
    {
        byte index = (byte)blockTypes.Count;
        blockTypes.Add(index, new BlockType(_baseType, index, _name, _textures, _prefab));
        BlocksArray = GetBlockTypeArray(blockTypes.Values);
    }

    public bool BuilderExists(int x, int y, int z)
    {
        if (Chunks.ContainsKey(new Vector3Int(x, y, z)))
        {
            return (Chunks[new Vector3Int(x, y, z)] != null);
        } 
        return false;
    }

    public bool BuilderGenerated(int x, int y, int z)
    {
        if (BuilderExists(x, y, z))
        {
            return Chunks[new Vector3Int(x, y, z)].Generated;
        }
        return false;
    }

    public IVoxelBuilder GetBuilder(int x, int y, int z)
    {
        IVoxelBuilder result = null;
        Vector3Int location = new Vector3Int(x, y, z);
        if (BuilderExists(x, y, z))
        {
            result = Chunks[new Vector3Int(x, y, z)].builder;
        }
        return result;
    }

    public static BlockType[] GetBlockTypeArray(Dictionary<byte, BlockType>.ValueCollection collection)
    {
        BlockType[] types = new BlockType[collection.Count];
        int i = 0;
        foreach (BlockType _type in collection)
        {
            types[i++] = _type;
        }
        return types;
    }

    public byte GetBlock(int x, int y, int z)
    {
        Vector3Int chunk = VoxelConversions.VoxelToChunk(new Vector3Int(x, y, x));
        Vector3Int localVoxel = VoxelConversions.GlobalVoxToLocalChunkVoxCoord(chunk, new Vector3Int(x, y, z));
        byte result = 1;
        if (x >= 0 && y >= 0 && z >= 0 && BuilderExists(chunk.x, chunk.y, chunk.z))
        {
            result = Chunks[new Vector3Int(chunk.x, chunk.y, chunk.z)].GetBlock(x, y, z);
        }
        return result;
    }

    public void SetBlockAtLocation(Vector3 position, byte type)
    {
        Vector3Int voxelPos = VoxelConversions.WorldToVoxel(position);
        Vector3Int chunk = VoxelConversions.VoxelToChunk(voxelPos);
        Vector3Int localVoxel = VoxelConversions.GlobalVoxToLocalChunkVoxCoord(chunk, voxelPos);
        if (BuilderExists(chunk.x, chunk.y, chunk.z))
        {
            Chunks[new Vector3Int(chunk.x, chunk.y, chunk.z)].EditNextFrame(new Chunk.BlockChange[] { new Chunk.BlockChange(position, type) });
        }
    }

    public void UpdateChunk(int x, int y, int z)
    {
        if (BuilderExists(x, y, z))
        {
            Chunks[new Vector3Int(x, y, z)].Render(true);
        }
    }

    public void GenerateExplosion(Vector3 postion, int radius)
    {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        Loom.AddAsyncThread("Explosion");
        Loom.QueueAsyncTask("Explosion", () =>
        {        
            Dictionary<Vector3Int, List<Chunk.BlockChange>> changes = new Dictionary<Vector3Int, List<Chunk.BlockChange>>();
            Vector3Int voxelPos = VoxelConversions.WorldToVoxel(postion);
            for (int x = voxelPos.x - radius; x <= voxelPos.x + radius; x++)
                for (int y = voxelPos.y - radius; y <= voxelPos.y + radius; y++)
                    for (int z = voxelPos.z - radius; z <= voxelPos.z + radius; z++)
                    {
                        Vector3Int voxel = new Vector3Int(x, y, z);
                        Vector3Int chunk = VoxelConversions.VoxelToChunk(voxel);
                        if (IsInSphere(voxelPos, radius, voxel))
                        {
                            if (!changes.ContainsKey(chunk))
                                changes.Add(chunk, new List<Chunk.BlockChange>());
                            changes[chunk].Add(new Chunk.BlockChange(VoxelConversions.GlobalVoxToLocalChunkVoxCoord(chunk, voxel), 0));
                            //ChangeBlock(new Chunk.BlockChange(voxel, 0));
                        }
                    }
            //Debug.Log("Iterated through exploded blocks: " + watch.Elapsed.ToString());
            Loom.QueueOnMainThread(() =>
            {
                foreach (Vector3Int chunkPos in changes.Keys)
                {
                    ChangeBlock(chunkPos, changes[chunkPos].ToArray());
                }
                watch.Stop();
                //Debug.Log("Blocks changes sent to chunk: " + watch.Elapsed.ToString());
            });
        });
    }

    public void ChangeBlock(Vector3 globalPosition, byte type, Vector3 normals, bool invertNormal)
    {
        Vector3 modifiedNormal = new Vector3();
        if (invertNormal)
            modifiedNormal = -(normals * VoxelSettings.half);
        else
            modifiedNormal = (normals * VoxelSettings.half);
        ChangeBlock(globalPosition + modifiedNormal, type);
    }

    public void ChangeBlock(Vector3 globalPosition, byte type)
    {
        Vector3Int voxelPos = VoxelConversions.WorldToVoxel(globalPosition);
        //Debug.LogFormat("globalPostion: {0}", globalPosition);
        ChangeBlock(voxelPos, type);
    }

    public void ChangeBlock(Vector3Int voxel, byte type)
    {
        ChangeBlock(new Chunk.BlockChange(voxel, type));
    }

    public void ChangeBlock(Chunk.BlockChange change)
    {
        Vector3Int chunk = VoxelConversions.VoxelToChunk(change.position);
        Vector3Int localVoxel = VoxelConversions.GlobalVoxToLocalChunkVoxCoord(chunk, change.position);
        //Debug.LogFormat("voxel: {0}, localVoxel: {1}, chunk: {2}", voxel, localVoxel, chunk);
        if (BuilderExists(chunk.x, chunk.y, chunk.z))
        {
            if (localVoxel.x >= 0 && localVoxel.x < VoxelSettings.ChunkSizeX && localVoxel.y >= 0 && localVoxel.y < VoxelSettings.ChunkSizeY && localVoxel.z >= 0 && localVoxel.z < VoxelSettings.ChunkSizeZ)
            {
                Chunks[chunk].EditNextFrame(new Chunk.BlockChange(localVoxel, change.type));
            }
            else
            {
                SafeDebug.LogError(string.Format("Out of Bounds: chunk: {0}, globalVoxel:{1}, localVoxel: {2}, Function: GenerateExplosion",
                    chunk, change.position, localVoxel));
            }
        }
    }

    public void ChangeBlock(Vector3Int chunk, Chunk.BlockChange change)
    {
        Vector3Int localVoxel = change.position;
        if (BuilderExists(chunk.x, chunk.y, chunk.z))
        {
            if (localVoxel.x >= 0 && localVoxel.x < VoxelSettings.ChunkSizeX && localVoxel.y >= 0 && localVoxel.y < VoxelSettings.ChunkSizeY && localVoxel.z >= 0 && localVoxel.z < VoxelSettings.ChunkSizeZ)
            {
                Chunks[chunk].EditNextFrame(change);
            }
            else
            {
                SafeDebug.LogError(string.Format("Out of Bounds: chunk: {0}, localVoxel: {1}, Function: GenerateExplosion",
                    chunk, localVoxel));
            }
        }
    }

    public void ChangeBlock(Vector3Int chunk, Chunk.BlockChange[] changes)
    {
        if (BuilderExists(chunk.x, chunk.y, chunk.z))
        {
            Chunks[new Vector3Int(chunk.x, chunk.y, chunk.z)].EditNextFrame(changes);
        }
    }

    public void ChangeBlock(Vector3Int[] voxels, byte type)
    {
        foreach(Vector3Int block in voxels)
        {
            Vector3Int chunk = VoxelConversions.VoxelToChunk(block);
            Vector3Int localVoxel = VoxelConversions.GlobalVoxToLocalChunkVoxCoord(chunk, block);
            //Debug.LogFormat("voxel: {0}, localVoxel: {1}, chunk: {2}", voxel, localVoxel, chunk);
            if (BuilderExists(chunk.x, chunk.y, chunk.z))
            {
                if (localVoxel.x >= 0 && localVoxel.x < VoxelSettings.ChunkSizeX && localVoxel.y >= 0 && localVoxel.y < VoxelSettings.ChunkSizeY && localVoxel.z >= 0 && localVoxel.z < VoxelSettings.ChunkSizeZ)
                {
                    Chunks[chunk].EditNextFrame(new Chunk.BlockChange(localVoxel, type));
                }
                else
                {
                    SafeDebug.LogError(string.Format("Out of Bounds: chunk: {0}, globalVoxel:{1}, localVoxel: {2}, Function: GenerateExplosion",
                        chunk, block, localVoxel));
                }
            }
        }
    }

    public void SetVoxelSize()
    {
        VoxelSize = ChunkSize() * Chunks.Count;
    }

    public void setMeshSize()
    {
        int totalSize = 0;
        foreach (Vector3Int chunk in Chunks.Keys)
            if (Chunks.ContainsKey(chunk))
                totalSize += Chunks[chunk].size;
        MeshSize = totalSize;
    }

    public void ClearChunks()
    {
        foreach (Vector3Int chunkPos in Chunks.Keys)
            DestroyChunk(chunkPos);
    }

    public void DestroyChunk(Vector3Int chunk)
    {
        Loom.QueueAsyncTask(WorldThreadName, () =>
        {
            if (BuilderExists(chunk.x, chunk.y, chunk.z))
            {
                Chunks[chunk].Close();
                Loom.QueueOnMainThread(() =>
                {
                    Destroy(Chunks[chunk].gameObject);
                    Chunks.Remove(chunk);
                });
            }
        });
    }

    private void SpawnChunkFeild()
    {
        for (int x = 0; x < VoxelSettings.maxChunksX; x++)
        {
            for (int z = 0; z < VoxelSettings.maxChunksZ; z++)
            {
                for (int y = 0; y < VoxelSettings.maxChunksY_M; y++)
                {
                    SpawnChunk(new Vector3Int(x, y, z));
                }
            }
        }
    }

    private void SpawnChunks(Vector3Int[] locations)
    {
        foreach (Vector3Int chunkPos in locations)
            SpawnChunk(chunkPos);
    }

    private void SpawnChunk(Vector3Int location)
    {
        if (!Chunks.ContainsKey(new Vector3Int(location.x, location.y, location.z)))
        {
            Chunk chunk = ((GameObject)Instantiate(chunkPrefab)).AddComponent<Chunk>();
            chunk.transform.parent = transform;
            chunk.name = string.Format("Chunk_{0}.{1}.{2}", location.x, location.y, location.z);
            chunk.Init(new Vector3Int(location.x, location.y, location.z), this);
            Chunks.Add(new Vector3Int(location.x, location.y, location.z), chunk);
        }
    }

    private bool ChunkIsInBounds(int x, int y, int z)
    {
        //return ((x <= Chunks.GetLength(0) - 1) && x >= 0) && ((y <= Chunks.GetLength(1) - 1) && y >= 0) && ((z <= Chunks.GetLength(2) - 1) && z >= 0);
        return true;
    }

    private bool IsInSphere(Vector3Int center, int radius, Vector3Int testPosition)
    {
        float distance = Mathf.Pow(center.x - testPosition.x, 2) + Mathf.Pow(center.y - testPosition.y, 2) + Mathf.Pow(center.z - testPosition.z, 2);
        return distance <= Mathf.Pow(radius, 2);
    }

    private int ChunkSize()
    {
        int blockStructSize = 5; //Marshal.SizeOf(typeof(Block));
        int blockArraySize = VoxelSettings.ChunkSizeX * VoxelSettings.ChunkSizeY * VoxelSettings.ChunkSizeZ * blockStructSize;
        int heightMapArraySize = VoxelSettings.ChunkSizeX * VoxelSettings.ChunkSizeZ * sizeof(float);
        return blockArraySize + heightMapArraySize;
    }
}
