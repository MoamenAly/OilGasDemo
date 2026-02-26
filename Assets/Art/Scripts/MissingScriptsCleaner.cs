using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class MissingScriptsCleaner
{
    // =========================
    // Helpers
    // =========================

    // Clean one GameObject hierarchy (itself + all children)
    private static int CleanGameObjectHierarchy(GameObject root)
    {
        int removed = 0;
        var transforms = root.GetComponentsInChildren<Transform>(true); // true = include inactive

        foreach (var t in transforms)
        {
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        }

        return removed;
    }

    // Clean one Scene (all root objects)
    private static int CleanScene(Scene scene)
    {
        int removed = 0;
        var roots = scene.GetRootGameObjects();

        foreach (var go in roots)
        {
            removed += CleanGameObjectHierarchy(go);
        }

        return removed;
    }

    // Clean one prefab asset
    private static int CleanPrefab(GameObject prefab)
    {
        int removed = 0;
        var transforms = prefab.GetComponentsInChildren<Transform>(true);

        foreach (var t in transforms)
        {
            removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        }

        return removed;
    }

    // =========================
    // MENU ITEMS
    // =========================

    [MenuItem("Tools/Missing Scripts/Clean Active Scene (Deep)")]
    private static void CleanActiveScene()
    {
        var scene = SceneManager.GetActiveScene();
        int removed = CleanScene(scene);

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log($"[MissingScriptsCleaner] Removed {removed} missing scripts in active scene '{scene.name}'.");
    }

    [MenuItem("Tools/Missing Scripts/Clean All Scenes In Project")]
    private static void CleanAllScenesInProject()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        int totalRemoved = 0;

        // Remember current scene so we can reopen it
        var currentScene = SceneManager.GetActiveScene();
        string currentScenePath = currentScene.path;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            int removed = CleanScene(scene);
            if (removed > 0)
            {
                totalRemoved += removed;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        // Re-open previously active scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
        }

        Debug.Log($"[MissingScriptsCleaner] Removed {totalRemoved} missing scripts from all scenes in project.");
    }

    [MenuItem("Tools/Missing Scripts/Clean All Prefabs In Project")]
    private static void CleanAllPrefabsInProject()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int totalRemoved = 0;

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                continue;

            int removed = CleanPrefab(prefab);
            if (removed > 0)
            {
                totalRemoved += removed;
                EditorUtility.SetDirty(prefab);
            }
        }

        if (totalRemoved > 0)
        {
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"[MissingScriptsCleaner] Removed {totalRemoved} missing scripts from all prefabs in project.");
    }

    [MenuItem("Tools/Missing Scripts/Clean All Scenes + Prefabs")]
    private static void CleanAllScenesAndPrefabs()
    {
        CleanAllScenesInProject();
        CleanAllPrefabsInProject();
        Debug.Log("[MissingScriptsCleaner] Finished cleaning all scenes and prefabs.");
    }
}
