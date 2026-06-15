using System.Collections.Generic;
using UnityEngine;

public class meshCombiner : MonoBehaviour
{
    public void CombineSelectedMeshes()
    {
        // 1. Find all MeshFilters in the children (the spawned tiles)
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        
        // Dictionary to group our mesh data by Material
        Dictionary<Material, List<CombineInstance>> materialToMeshMap = new Dictionary<Material, List<CombineInstance>>();

        foreach (MeshFilter filter in meshFilters)
        {
            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            if (renderer == null || filter.sharedMesh == null) continue;

            // Get the material of this specific tile
            Material mat = renderer.sharedMaterial;

            // If we haven't seen this material yet, create a new list for it
            if (!materialToMeshMap.ContainsKey(mat))
            {
                materialToMeshMap[mat] = new List<CombineInstance>();
            }

            // Create a combine instance for this tile's mesh
            CombineInstance combine = new CombineInstance();
            combine.mesh = filter.sharedMesh;
            combine.transform = filter.transform.localToWorldMatrix; // Keeps its position/rotation intact
            
            materialToMeshMap[mat].Add(combine);

            // Deactivate the original tile visual so it doesn't render twice
            renderer.enabled = false; 
        }

        List<GameObject> objectsToDestroy = new List<GameObject>();

        foreach (KeyValuePair<Material, List<CombineInstance>> entry in materialToMeshMap)
        {
            Material currentMat = entry.Key;
            List<CombineInstance> combineList = entry.Value;

            GameObject combinedObj = new GameObject("CombinedMesh_" + currentMat.name);
            combinedObj.transform.parent = this.transform;
            combinedObj.transform.localPosition = Vector3.zero;
            combinedObj.transform.localRotation = Quaternion.identity;

            MeshFilter newMeshFilter = combinedObj.AddComponent<MeshFilter>();
            MeshRenderer newMeshRenderer = combinedObj.AddComponent<MeshRenderer>();

            newMeshRenderer.sharedMaterial = currentMat;

            Mesh finalMesh = new Mesh();
            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 
            
            finalMesh.CombineMeshes(combineList.ToArray(), true, true);
            newMeshFilter.sharedMesh = finalMesh;
            combinedObj.AddComponent<MeshCollider>().sharedMesh = finalMesh;
        }

        foreach (MeshFilter filter in meshFilters)
        {
            if (filter.gameObject != this.gameObject)
            {
                Destroy(filter.gameObject);
            }
        }
    }
}