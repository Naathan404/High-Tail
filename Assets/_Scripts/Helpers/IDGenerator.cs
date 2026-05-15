using System;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;
public static class IDGenerator
{
    public static string GenerateUniqueID(string sceneName, string objectName, float posX, float posY, float posZ)
    {
        return $"{sceneName}_{objectName}_{posX:F1}_{posY:F1}_{posZ:F1}";
    }

    public static string GenerateUniqueID(GameObject gameObject)
    {
        string sceneName = gameObject.scene.name;
        string objectName = gameObject.name;
        Vector3 position = gameObject.transform.position;
        return GenerateUniqueID(sceneName, objectName, position.x, position.y, position.z);
    }

    public static string GenerateUniqueID(string prefix)
    {
        return $"{prefix}_{Guid.NewGuid().ToString().Substring(0, 8)}";
    }
}