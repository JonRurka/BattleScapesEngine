﻿using UnityEngine;
using System.Collections;

public static class VoxelConversions {
    public static Vector3Int ChunkCoordToSuperCoord(Vector3Int location) {
        int x = Mathf.RoundToInt(location.x / VoxelSettings.maxChunksX) + Negative1IfNegative(location.x, 0);
        int y = Mathf.RoundToInt(location.y / VoxelSettings.maxChunksY_M) + Negative1IfNegative(location.y, 0);
        int z = Mathf.RoundToInt(location.z / VoxelSettings.maxChunksZ) + Negative1IfNegative(location.z, 0);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int SuperCoordToChunkCoord(Vector3Int location) {
        int x = location.x * VoxelSettings.maxChunksX;
        int y = location.y * VoxelSettings.maxChunksY_M;
        int z = location.z * VoxelSettings.maxChunksZ;
        return new Vector3Int(x, y, z);
    }

    public static Vector3 SuperChunkCoordToWorldPos(Vector3Int location) {
        float x = location.x * ((VoxelSettings.ChunkSizeX / VoxelSettings.voxelsPerMeter) * VoxelSettings.maxChunksX) - VoxelSettings.half;
        float y = location.y * ((VoxelSettings.ChunkSizeY / VoxelSettings.voxelsPerMeter) * VoxelSettings.maxChunksY_M) - VoxelSettings.half;
        float z = location.z * ((VoxelSettings.ChunkSizeZ / VoxelSettings.voxelsPerMeter) * VoxelSettings.maxChunksZ) - VoxelSettings.half;
        return new Vector3(x, y, z);
    }

    public static Vector3Int WorldPosToSuperChunkCoord(Vector3 location) {
        int x = Mathf.RoundToInt((location.x - (VoxelSettings.MeterSizeX * VoxelSettings.maxChunksX / 2 + VoxelSettings.half)) / ((VoxelSettings.ChunkSizeX / VoxelSettings.voxelsPerMeter) * VoxelSettings.maxChunksX));
        int y = Mathf.RoundToInt((location.y - (VoxelSettings.MeterSizeY * VoxelSettings.maxChunksY_M / 2 + VoxelSettings.half)) / ((VoxelSettings.ChunkSizeY / VoxelSettings.voxelsPerMeter) * VoxelSettings.maxChunksY_M));
        int z = Mathf.RoundToInt((location.z - (VoxelSettings.MeterSizeZ * VoxelSettings.maxChunksZ / 2 + VoxelSettings.half)) / ((VoxelSettings.ChunkSizeZ / VoxelSettings.voxelsPerMeter) * VoxelSettings.maxChunksZ));
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int[] GetCoords(Vector3 location) {
        Vector3Int[] result = new Vector3Int[3];
        result[0] = WorldPosToChunkCoord(location);
        result[1] = ChunkCoordToSuperCoord(result[0]);
        result[2] = GlobalToLocalChunkCoords(result[0]);
        return result;
    }

    public static Vector3 ChunkCoordToWorld(Vector3Int location) {
        return new Vector3(location.x * VoxelSettings.ChunkSizeX / VoxelSettings.voxelsPerMeter - VoxelSettings.half, location.y * VoxelSettings.ChunkSizeY / VoxelSettings.voxelsPerMeter - VoxelSettings.half, location.z * VoxelSettings.ChunkSizeZ / VoxelSettings.voxelsPerMeter - VoxelSettings.half);
    }

    public static Vector3Int WorldPosToChunkCoord(Vector3 location) {
        return new Vector3Int(Mathf.RoundToInt(location.x / VoxelSettings.ChunkSizeX * VoxelSettings.voxelsPerMeter + VoxelSettings.half), Mathf.RoundToInt(location.y / VoxelSettings.ChunkSizeY * VoxelSettings.voxelsPerMeter + VoxelSettings.half), Mathf.RoundToInt(location.y / VoxelSettings.ChunkSizeY * VoxelSettings.voxelsPerMeter + VoxelSettings.half));
    }

    public static Vector3Int GlobalToLocalChunkCoords(Vector3Int location) {
        Vector3Int super = ChunkCoordToSuperCoord(location);
        return GlobalToLocalChunkCoords(super, location);
    }

    public static Vector3Int GlobalToLocalChunkCoords(Vector3Int super, Vector3Int location) {
        int x = (location.x - (super.x * VoxelSettings.maxChunksX));
        int y = (location.y - (super.y * VoxelSettings.maxChunksY_M));
        int z = (location.z - (super.z * VoxelSettings.maxChunksZ));
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int LocalToGlobalChunkCoords(Vector3Int super, Vector3Int chunk) {
        int x = chunk.x + (super.x * VoxelSettings.maxChunksX);
        int y = chunk.y + (super.y * VoxelSettings.maxChunksY_M);
        int z = chunk.z + (super.z * VoxelSettings.maxChunksZ);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int GlobalVoxToSuperVoxCoord(Vector3Int location) {
        Vector3Int SuperCoord = VoxelToSuper(location);
        int x = (location.x - Mathf.Abs(SuperCoord.x * VoxelSettings.SuperSizeX));
        int y = (location.y - Mathf.Abs(SuperCoord.y * VoxelSettings.SuperSizeY));
        int z = (location.z - Mathf.Abs(SuperCoord.z * VoxelSettings.SuperSizeZ));
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int GlobalVoxToSuperVoxCoord(Vector3Int SuperLocation ,Vector3Int location) {
        int x = location.x - Mathf.Abs(SuperLocation.x * VoxelSettings.SuperSizeX);
        int y = location.y - Mathf.Abs(SuperLocation.y * VoxelSettings.SuperSizeY);
        int z = location.z - Mathf.Abs(SuperLocation.z * VoxelSettings.SuperSizeZ);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int SuperVoxToGlobalVoxCoord(Vector3Int SuperChunk, Vector3Int location) {
        int x = location.x + (SuperChunk.x / VoxelSettings.SuperSizeX);
        int y = location.y + (SuperChunk.y / VoxelSettings.SuperSizeY);
        int z = location.z + (SuperChunk.z / VoxelSettings.SuperSizeZ);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int GlobalVoxToLocalChunkVoxCoord(Vector3Int location) {
        Vector3Int ChunkCoord = VoxelToChunk(location);
        return GlobalVoxToLocalChunkVoxCoord(ChunkCoord, location);
    }

    public static Vector3Int GlobalVoxToLocalChunkVoxCoord(Vector3Int ChunkCoord, Vector3Int location) {
        int x = location.x - (ChunkCoord.x * VoxelSettings.ChunkSizeX) + Negative1IfNegative(0, 0);
        int y = location.y - (ChunkCoord.y * VoxelSettings.ChunkSizeY) + Negative1IfNegative(0, 0);
        int z = location.z - (ChunkCoord.z * VoxelSettings.ChunkSizeZ) + Negative1IfNegative(0, 0);
        //return new Vector3Int(location.x >= 0 ? x : x - 1, location.y >= 0 ? y : y - 1, location.z >= 0 ? z : z - 1);
        int offsetX = location.x == -1 ? VoxelSettings.ChunkSizeX - 1 : x;
        int offsetY = location.y == -1 ? VoxelSettings.ChunkSizeY - 1 : y;
        int offsetZ = location.z == -1 ? VoxelSettings.ChunkSizeZ - 1 : z;
        return new Vector3Int(offsetX == 20 ? 0 : offsetX, offsetY == 20 ? 0 : offsetY, offsetZ == 20 ? 0 : offsetZ);
    }

    public static Vector3Int LocalChunkVoxToGlobalVoxCoord(Vector3Int Chunk, Vector3Int location) {
        int x = location.x + (Chunk.x / VoxelSettings.ChunkSizeX);
        int y = location.y + (Chunk.y / VoxelSettings.ChunkSizeY);
        int z = location.z + (Chunk.z / VoxelSettings.ChunkSizeZ);
        return new Vector3Int(x, y, z);
    }

    // Same as GlobalVoxToLocalChunkVoxCoord. Only here to show you can use it in this context.
    public static Vector3Int SuperLocalVoxToLocalChunkVox(Vector3Int location) {
        return GlobalVoxToLocalChunkVoxCoord(location);
    }

    // Same as GlobalVoxToLocalChunkVoxCoord. Only here to show you can use it in this context.
    public static Vector3Int SuperLocalVoxToLocalChunkVox(Vector3Int chunk, Vector3Int location) {
        return GlobalVoxToLocalChunkVoxCoord(chunk, location);
    }

    // Same as LocalChunkVoxToGlobalVoxCoord. Only here to show you can use it in this context.
    public static Vector3Int LocalChunkVoxToSuperLocalVox(Vector3Int chunk, Vector3Int location) {
        return LocalChunkVoxToGlobalVoxCoord(chunk, location);
    }

    public static Vector3Int VoxelToSuper(Vector3Int location) {
        int x = Mathf.FloorToInt(location.x / VoxelSettings.SuperSizeX);
        int y = Mathf.FloorToInt(location.y / VoxelSettings.SuperSizeY);
        int z = Mathf.FloorToInt(location.z / VoxelSettings.SuperSizeZ);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int SuperToVoxel(Vector3Int location) {
        int x = (int)(location.x * VoxelSettings.SuperSizeX);
        int y = (int)(location.y * VoxelSettings.SuperSizeY);
        int z = (int)(location.z * VoxelSettings.SuperSizeZ);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int VoxelToChunk(Vector3Int location) {
        int x = (int)(((location.x - Negative1IfNegative(location.x, 0)) / VoxelSettings.ChunkSizeX)) + Negative1IfNegative(location.x, 0);
        int y = (int)(((location.y - Negative1IfNegative(location.y, 0)) / VoxelSettings.ChunkSizeY)) + Negative1IfNegative(location.y, 0);
        int z = (int)(((location.z - Negative1IfNegative(location.z, 0)) / VoxelSettings.ChunkSizeZ)) + Negative1IfNegative(location.z, 0);
        return new Vector3Int(x, y, z);
    }

    public static Vector3Int ChunkToVoxel(Vector3Int location) {
        return new Vector3Int(Mathf.RoundToInt(location.x * VoxelSettings.ChunkSizeX), Mathf.RoundToInt(location.y * VoxelSettings.ChunkSizeY), Mathf.RoundToInt(location.z * VoxelSettings.ChunkSizeZ));
    }

    public static Vector3 VoxelToWorld(Vector3Int location)
    {
        return VoxelToWorld(location.x, location.x, location.z);
    }

    public static Vector3 VoxelToWorld(int x, int y, int z) {
        float newX = (x / VoxelSettings.voxelsPerMeter) - VoxelSettings.half;
        float newY = (y / VoxelSettings.voxelsPerMeter) - VoxelSettings.half;
        float newZ = (z / VoxelSettings.voxelsPerMeter) - VoxelSettings.half;
        return new Vector3(newX, newY, newZ);
    }

    public static Vector3Int WorldToVoxel(Vector3 worldPos) {
        float xOffset = worldPos.x + VoxelSettings.half * VoxelSettings.voxelsPerMeter;
        float yOffset = worldPos.y + VoxelSettings.half * VoxelSettings.voxelsPerMeter;
        float zOffset = worldPos.z + VoxelSettings.half * VoxelSettings.voxelsPerMeter;
        int x = xOffset > 0 ? (int)(xOffset) : (int)(xOffset) - 1;
        int y = yOffset > 0 ? (int)(yOffset) : (int)(yOffset) - 1;
        int z = zOffset > 0 ? (int)(zOffset) : (int)(zOffset) - 1;
        //int x = (int)((worldPos.x + (VoxelSettings.half * GetEditValue(worldPos.x))) * VoxelSettings.voxelsPerMeter);
        //int y = (int)((worldPos.y + (VoxelSettings.half * GetEditValue(worldPos.y))) * VoxelSettings.voxelsPerMeter);
        //int z = (int)((worldPos.z + (VoxelSettings.half * GetEditValue(worldPos.z))) * VoxelSettings.voxelsPerMeter);
        return new Vector3Int(x, y, z);
    }

    private static int GetEditValue(float input)
    {
        if (input < 0)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    public static int Negative1IfNegative(int input, int defaultValue) {
        if (input < 0) {
            return -1;
        }
        else return defaultValue;
    }

    public static int Negative1IfZeroOrLess(int input, int defaultValue)
    {
        if (input <= 0)
        {
            return -1;
        }
        else return defaultValue;
    }
}

