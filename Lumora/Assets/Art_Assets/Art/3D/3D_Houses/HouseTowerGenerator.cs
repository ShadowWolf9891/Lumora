using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HouseTowerGenerator : MonoBehaviour
{
    [Header("Floor Prefabs")]
    public GameObject[] housePrefabs;

    [Header("Tower Settings")]
    [Min(1)] public int floors = 10;
    public float baseHeight = 2.5f;

    [Header("Random Offsets")]
    public float maxHorizontalOffset = 1.2f;
    public float maxRotationY = 6f;

    [Header("Seed")]
    public bool useSeed = true;
    public int seed = 12345;

    [Header("Save Prefab")]
    public string saveFolder = "Assets/GeneratedPrefabs";
    public string prefabBaseName = "HouseTower";

    // Torre actualmente generada
    private GameObject currentTower;

#if UNITY_EDITOR
    // ===================== GENERATE =====================
    public void GenerateTower()
    {
        ClearTower();

        if (housePrefabs == null || housePrefabs.Length == 0)
        {
            Debug.LogWarning("No housePrefabs assigned.");
            return;
        }

        if (useSeed)
            Random.InitState(seed);

        currentTower = new GameObject($"{prefabBaseName}_Preview");
        currentTower.transform.position = transform.position;
        currentTower.transform.rotation = transform.rotation;

        float currentHeight = 0f;

        for (int i = 0; i < floors; i++)
        {
            var prefab = housePrefabs[Random.Range(0, housePrefabs.Length)];

            Vector3 offset = new Vector3(
                Random.Range(-maxHorizontalOffset, maxHorizontalOffset),
                currentHeight,
                Random.Range(-maxHorizontalOffset, maxHorizontalOffset)
            );

            Quaternion rot = Quaternion.Euler(
                0f,
                Random.Range(-maxRotationY, maxRotationY),
                0f
            );

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.SetParent(currentTower.transform, false);
            go.transform.position = currentTower.transform.position + offset;
            go.transform.rotation = rot;
            go.name = $"{prefab.name}_Floor{i:00}";

            currentHeight += baseHeight;
        }

        Selection.activeGameObject = currentTower;
    }

    // ===================== SAVE =====================
    public void SaveCurrentTowerAsPrefab()
    {
        if (currentTower == null)
        {
            Debug.LogWarning("No tower generated to save.");
            return;
        }

        EnsureFolderExists(saveFolder);

        string path = $"{saveFolder}/{prefabBaseName}.prefab";
        path = AssetDatabase.GenerateUniqueAssetPath(path);

        // Guarda como asset independiente (NO conecta)
        PrefabUtility.SaveAsPrefabAsset(currentTower, path);

        Debug.Log($"Saved independent prefab: {path}");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // ===================== CLEAR =====================
    public void ClearTower()
    {
        if (currentTower != null)
        {
            DestroyImmediate(currentTower);
            currentTower = null;
        }
    }

    // ===================== UTIL =====================
    private void EnsureFolderExists(string fullPath)
    {
        if (AssetDatabase.IsValidFolder(fullPath)) return;

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

        HouseTowerGenerator gen = (HouseTowerGenerator)target;

        GUILayout.Space(12);

        EditorGUILayout.HelpBox(
            "Generate creates a scene-only tower preview.\n" +
            "Save creates a UNIQUE prefab asset (not connected).",
            MessageType.Info
        );

        if (GUILayout.Button("Generate Tower (Scene Only)"))
        {
            gen.GenerateTower();
        }

        if (GUILayout.Button("Save Current Tower As Prefab"))
        {
            gen.SaveCurrentTowerAsPrefab();
        }

        if (GUILayout.Button("Clear Scene Tower"))
        {
            gen.ClearTower();
        }
    }
}
#endif
