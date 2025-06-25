using UnityEditor;
using UnityEngine;

namespace NonPlayable.Goap
{
    /// <summary>
    /// Editor utility for flattening LODGroup children into combined meshes on the root GameObject.
    /// </summary>
    public static class LODFlattener
    {
        /// <summary>
        /// Flattens LODGroups on all selected GameObjects by combining child meshes into single meshes per LOD level and updating the LODGroup.
        /// </summary>
        [MenuItem("Tools/NonPlayable/Flatten Selected LOD Prefabs")]
        public static void FlattenSelectedLODPrefabs()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning("No GameObjects selected for LOD flattening.");
                return;
            }

            foreach (GameObject go in selectedObjects)
            {
                FlattenLOD(go);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Flattens the LODGroup on the specified GameObject by merging all renderer meshes in each LOD level into one combined mesh, then removes the original child renderers.
        /// </summary>
        /// <param name="root">Root GameObject containing the LODGroup.</param>
        private static void FlattenLOD(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            LODGroup lodGroup = root.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                Debug.LogWarning("GameObject '" + root.name + "' does not have a LODGroup component.");
                return;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(root))
            {
                Debug.LogWarning("Please open the prefab '" + root.name + "' in Prefab Mode to flatten LOD.");
                return;
            }

            LOD[] lods = lodGroup.GetLODs();
            LOD[] newLODs = new LOD[lods.Length];

            for (int i = 0; i < lods.Length; i++)
            {
                Renderer[] renderers = lods[i].renderers;
                if (renderers == null || renderers.Length == 0)
                {
                    newLODs[i] = lods[i];
                    continue;
                }

                CombineInstance[] combineInstances = new CombineInstance[renderers.Length];
                Material[] materials = new Material[renderers.Length];

                for (int j = 0; j < renderers.Length; j++)
                {
                    MeshFilter meshFilter = renderers[j].GetComponent<MeshFilter>();
                    MeshRenderer meshRenderer = renderers[j].GetComponent<MeshRenderer>();

                    if (meshFilter != null && meshFilter.sharedMesh != null && meshRenderer != null)
                    {
                        combineInstances[j].mesh = meshFilter.sharedMesh;
                        combineInstances[j].transform = meshFilter.transform.localToWorldMatrix;
                        materials[j] = meshRenderer.sharedMaterial;
                    }
                }

                Mesh combinedMesh = new Mesh();
                combinedMesh.name = root.name + "_LOD" + i;
                combinedMesh.CombineMeshes(combineInstances, false, true);

                MeshFilter rootMeshFilter = root.GetComponent<MeshFilter>();
                if (rootMeshFilter == null)
                {
                    rootMeshFilter = root.AddComponent<MeshFilter>();
                }
                rootMeshFilter.sharedMesh = combinedMesh;

                MeshRenderer rootMeshRenderer = root.GetComponent<MeshRenderer>();
                if (rootMeshRenderer == null)
                {
                    rootMeshRenderer = root.AddComponent<MeshRenderer>();
                }
                rootMeshRenderer.sharedMaterials = materials;

                newLODs[i] = new LOD(lods[i].screenRelativeTransitionHeight, new Renderer[] { rootMeshRenderer });

                foreach (Renderer r in renderers)
                {
                    GameObject child = r.gameObject;
                    Object.DestroyImmediate(r);
                    Object.DestroyImmediate(child);
                }
            }

            lodGroup.SetLODs(newLODs);
            lodGroup.RecalculateBounds();
            EditorUtility.SetDirty(root);
        }
    }
}
