using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public class Chunk : MonoBehaviour {
    [Serializable]
    public struct BlockChange
    {
        public Vector3Int position;
        public byte type;
        public BlockChange(Vector3Int position, byte type)
        {
            this.position = position;
            this.type = type;
        }
    }

    public Vector3Int ChunkPosition;
    public IVoxelBuilder builder;
    public SmoothVoxelBuilder BuilderInstance;
    public IPageController pageController;
    public List<BlockChange> EditQueue;
    public int disappearDistance = VoxelSettings.radius;
    public int size = 0;
    public int vertSize = 0;
    public int triSize = 0;

    public bool Generated {
        get { return generated; }
    }

    MeshFilter _filter;
    MeshRenderer _renderer;
    MeshCollider _collider;
    GameObject player;

    object lockObj;

    ManualResetEvent resetEvent = new ManualResetEvent(false);

    bool enableTest = false;
    bool generated = false;
    bool rendered = false;

	// Use this for initialization
	void Start () {
        EditQueue = new List<BlockChange>();
        lockObj = new object();
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(TerrainController.Instance.newPlayerChunkPos, ChunkPosition) > disappearDistance)
        {
            Destroy(gameObject, 1);
            Loom.QueueAsyncTask(TerrainController.WorldThreadName, () =>
            {
                TerrainController.Chunks.Remove(ChunkPosition);
                Close();
            });
            return;
        }

        if (EditQueue.Count > 0)
        {
            List<BlockChange> EditQueueCopy = new List<BlockChange>(EditQueue);
            EditQueue.Clear();
            Loom.QueueAsyncTask(TerrainController.setBlockThreadName, () =>
            {

                lock (lockObj)
                {
                    List<Vector3Int> updateChunks = new List<Vector3Int>();
                    foreach (BlockChange change in EditQueueCopy)
                    {
                        Vector3Int position = change.position;
                        byte type = change.type;
                        if (position.x == 0)
                        {
                            if (!updateChunks.Contains(new Vector3Int(ChunkPosition.x - 1, ChunkPosition.y, ChunkPosition.z)))
                                updateChunks.Add(new Vector3Int(ChunkPosition.x - 1, ChunkPosition.y, ChunkPosition.z));
                        }
                        if (position.x == VoxelSettings.ChunkSizeX - 1)
                        {
                            if (!updateChunks.Contains(new Vector3Int(ChunkPosition.x + 1, ChunkPosition.y, ChunkPosition.z)))
                                updateChunks.Add(new Vector3Int(ChunkPosition.x + 1, ChunkPosition.y, ChunkPosition.z));
                        }

                        if (position.y == 0)
                        {
                            if (!updateChunks.Contains(new Vector3Int(ChunkPosition.x, ChunkPosition.y - 1, ChunkPosition.z)))
                                updateChunks.Add(new Vector3Int(ChunkPosition.x, ChunkPosition.y - 1, ChunkPosition.z));

                        }
                        if (position.y == VoxelSettings.ChunkSizeY - 1)
                        {
                            if (!updateChunks.Contains(new Vector3Int(ChunkPosition.x, ChunkPosition.y + 1, ChunkPosition.z)))
                                updateChunks.Add(new Vector3Int(ChunkPosition.x, ChunkPosition.y + 1, ChunkPosition.z));
                        }

                        if (position.z == 0)
                        {
                            if (!updateChunks.Contains(new Vector3Int(ChunkPosition.x, ChunkPosition.y, ChunkPosition.z - 1)))
                                updateChunks.Add(new Vector3Int(ChunkPosition.x, ChunkPosition.y, ChunkPosition.z - 1));
                        }
                        if (position.z == VoxelSettings.ChunkSizeZ - 1)
                        {
                            if (!updateChunks.Contains(new Vector3Int(ChunkPosition.x, ChunkPosition.y, ChunkPosition.z + 1)))
                                updateChunks.Add(new Vector3Int(ChunkPosition.x, ChunkPosition.y, ChunkPosition.z + 1));
                        }
                        builder.SetBlock(position.x, position.y, position.z, new Block(type));
                    }
                    Render(true);
                    foreach (Vector3Int chunk in updateChunks)
                    {
                        pageController.UpdateChunk(chunk.x, chunk.y, chunk.z);
                    }
                }
            });
        }
	}

    public void EditNextFrame(BlockChange[] changes)
    {
        EditQueue.AddRange(changes);
    }

    public void EditNextFrame(BlockChange change)
    {
        Vector3Int position = change.position;
        byte type = change.type;
        if (position.x >= 0 && position.x < VoxelSettings.ChunkSizeX && position.y >= 0 && position.y < VoxelSettings.ChunkSizeY && position.z >= 0 && position.z < VoxelSettings.ChunkSizeZ)
        {
            EditQueue.Add(new BlockChange(position, type));
        }
        else
        {
            SafeDebug.LogError(string.Format("Out of Bounds: chunk: {0}, localVoxel: {1}, Function: EditNextFrame", ChunkPosition, position));
        }
    }

    public void Init(Vector3Int chunkPos, IPageController pageController) {
        ChunkPosition = chunkPos;
        this.pageController = pageController;
        transform.position = VoxelConversions.ChunkCoordToWorld(chunkPos);
        _renderer = gameObject.GetComponent<MeshRenderer>();
        _filter = gameObject.GetComponent<MeshFilter>();
        _collider = gameObject.GetComponent<MeshCollider>();
        _renderer.material.SetTexture("_MainTex", TerrainController.Instance.textureAtlas);
        player = TerrainController.Instance.player;
        createChunkBuilder();
    }

    public void createChunkBuilder() {
        builder = new SmoothVoxelBuilder(TerrainController.Instance,
                                    ChunkPosition,
                                    VoxelSettings.voxelsPerMeter,
                                    VoxelSettings.MeterSizeX,
                                    VoxelSettings.MeterSizeY,
                                    VoxelSettings.MeterSizeZ);
        builder.SetBlockTypes(TerrainController.Instance.BlocksArray, TerrainController.Instance.AtlasUvs);
        BuilderInstance = (SmoothVoxelBuilder)builder;
    }

    public void DebugFill(byte type)
    {
        for(int x = 0; x < VoxelSettings.ChunkSizeX; x++)
            for(int y = 0; y < VoxelSettings.ChunkSizeY; y++)
                for(int z = 0; z < VoxelSettings.ChunkSizeZ; z++)
                {
                    if (y == 0)
                        builder.SetBlock(x, y, z, new Block(type));
                }
    }

    public void DebugColor(Color color)
    {
        _renderer.material.SetColor("_color", color);
    }

    public float[,] GenerateChunk(LibNoise.IModule module)
    {
        float[,] result = null;
        result = ((SmoothVoxelBuilder)builder).Generate(module,
                                                 VoxelSettings.seed,
                                                 VoxelSettings.enableCaves,
                                                 VoxelSettings.amplitude,
                                                 VoxelSettings.caveDensity,
                                                 VoxelSettings.groundOffset,
                                                 VoxelSettings.grassOffset);
        generated = true;
        return result;
    }

    public float[,] GenerateChunk() {
        float[,] result = null;
        result = builder.Generate( VoxelSettings.seed,
                                 VoxelSettings.enableCaves,
                                 VoxelSettings.amplitude,
                                 VoxelSettings.caveDensity,
                                 VoxelSettings.groundOffset,
                                 VoxelSettings.grassOffset );
        generated = true;
        return result;
    }

    public void Render(bool renderOnly) {
        MeshData meshData = RenderChunk(renderOnly);
        Loom.QueueOnMainThread(() => {
            if (_filter != null && _collider != null && _renderer != null)
            {
                Mesh mesh = new Mesh();
                mesh.vertices = meshData.vertices;
                mesh.triangles = meshData.triangles;
                //mesh.uv = meshData.UVs;
                
                mesh.RecalculateNormals();

                _filter.sharedMesh = mesh;
                _collider.sharedMesh = mesh;
                //_renderer.material.SetTexture("_MainTex", TerrainController.Instance.textureAtlas);

                size = meshData.GetSize();
                vertSize = meshData.vertices.Length;
                triSize = meshData.triangles.Length;
                meshData.vertices = null;
                meshData.triangles = null;
                meshData.UVs = null;
                
                rendered = true;
            }
        });
    }

    public byte GetBlock(int x, int y, int z)
    {
        byte result = 0;
        if (builder != null)
        {
            result = builder.GetBlock(x, y, z).type;
        }
        return result;
    }

    public void Close()
    {
        builder.Dispose();
        builder = null;
        BuilderInstance = null;
    }

    private MeshData RenderChunk(bool renderOnly) {
        return builder.Render(renderOnly);
    }
}
