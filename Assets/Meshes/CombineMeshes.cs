using UnityEditor;
using UnityEngine;

public class CombineMeshes : MonoBehaviour
{
    [SerializeField] private MeshFilter TargetMesh;
    [SerializeField] bool OptimizeMesh;
    void Start()
    {
        MeshFilter newMeshFilter = gameObject.AddComponent<MeshFilter>();
        TargetMesh = newMeshFilter;

        CombineInstance[] meshes = new CombineInstance[transform.childCount];

        for (int i = 0; i < meshes.Length; i++)
        {
            Transform child = transform.GetChild(i);
            
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();
            meshes[i].mesh = meshFilter.sharedMesh;
            meshes[i].transform = child.localToWorldMatrix;
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(meshes);

        TargetMesh.mesh = combinedMesh;

        if (OptimizeMesh) MeshUtility.Optimize(combinedMesh);

        string path = EditorUtility.SaveFilePanel("Save Combined Mesh", "Assets/Meshes", "", "asset");
        path = FileUtil.GetProjectRelativePath(path);

        AssetDatabase.CreateAsset(combinedMesh, path);
        AssetDatabase.SaveAssets();

        Debug.Log("<color=#00ffff>Combined mesh saved.</color>");
    }
}
