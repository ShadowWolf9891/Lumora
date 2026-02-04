using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HouseTowerGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] housePrefabs;

    [Header("Tower Settings")]
    public int floors = 10;
    public float baseHeight = 2.5f;

    [Header("Random Offsets")]
    public float maxHorizontalOffset = 1.2f;
    public float maxRotationY = 6f;

    [Header("Seed (keep same results)")]
    public bool useSeed = true;
    public int seed = 12345;

    [Header("Save Prefab")]
    public string saveFolder = "Assets/GeneratedPrefabs";
    public string prefabName = "HouseTower_01";

    public void GenerateTower()
    {
        ClearChildren();

        if (housePrefabs == null || housePrefabs.Length == 0)
        {
            Debug.LogWarning("No housePrefabs assigned.");
            return;
        }

        if (useSeed) Random.InitState(seed);

        float currentHeight = 0f;

        for (int i = 0; i < floors; i++)
        {
            var prefab = housePrefabs[Random.Range(0, housePrefabs.Length)];

            Vector3 offset = new Vector3(
                Random.Range(-maxHorizontalOffset, maxHorizontalOffset),
                currentHeight,
                Random.Range(-maxHorizontalOffset, maxHorizontalOffset)
            );

            Quaternion rot = Quaternion.Euler(0f, Random.Range(-maxRotationY, maxRotationY), 0f);

            var go = Instantiate(prefab, transform.position + offset, rot, transform);
            go.name = $"{prefab.name}_Floor{i:00}";

            currentHeight += baseHeight;
        }
    }

    public void ClearChildren()
    {
        
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(transform.GetChild(i).gameObject);
            else Destroy(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject);
#endif
        }
    }

#if UNITY_EDITOR
    public void SaveAsPrefab()
    {
        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            // folder
            CreateFoldersRecursively(saveFolder);
        }

        string path = $"{saveFolder}/{prefabName}.prefab";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        //GameObject root prefab
        PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.UserAction);

        Debug.Log($"Saved Prefab: {path}");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CreateFoldersRecursively(string fullPath)
    {
        // Ej: Assets/GeneratedPrefabs/Sub
        string[] parts = fullPath.Split('/');
        if (parts.Length == 0 || parts[0] != "Assets") return;

        string current = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(HouseTowerGenerator))]
public class HouseTowerGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var gen = (HouseTowerGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Tower"))
        {
            gen.GenerateTower();
        }

        if (GUILayout.Button("Clear"))
        {
            gen.ClearChildren();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Save As Prefab"))
        {
            gen.SaveAsPrefab();
        }
    }
}
#endif
