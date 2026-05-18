using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor helper to automatically assign the FoodTree prefab to the FoodTreeSpawner.
/// Right-click the FoodTreeSpawner in the hierarchy and select "Assign Food Tree Prefab".
/// </summary>
public class FoodTreeSpawnerEditor
{
    [MenuItem("GameObject/Food Trees/Assign FoodTree Prefab to Spawner", priority = 30)]
    public static void AssignFoodTreePrefab()
    {
        // Get the currently selected GameObject (should be the spawner)
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select the FoodTreeSpawner object first.", "OK");
            return;
        }

        // Get the FoodTreeSpawner component
        FoodTreeSpawner spawner = selectedObject.GetComponent<FoodTreeSpawner>();
        if (spawner == null)
        {
            EditorUtility.DisplayDialog("Error", "Selected object does not have a FoodTreeSpawner component.", "OK");
            return;
        }

        // Find the FoodTree prefab
        string[] guids = AssetDatabase.FindAssets("FoodTree t:prefab");
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "Could not find FoodTree.prefab in the project.", "OK");
            return;
        }

        string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject foodTreePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (foodTreePrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to load FoodTree prefab.", "OK");
            return;
        }

        // Use reflection to set the private field (if needed) or use SerializedObject
        SerializedObject serializedSpawner = new SerializedObject(spawner);
        SerializedProperty prefabProperty = serializedSpawner.FindProperty("foodTreePrefab");

        if (prefabProperty != null)
        {
            prefabProperty.objectReferenceValue = foodTreePrefab;
            serializedSpawner.ApplyModifiedProperties();
            EditorUtility.DisplayDialog("Success", $"Assigned FoodTree prefab from: {prefabPath}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Could not find foodTreePrefab property on FoodTreeSpawner.", "OK");
        }
    }
}
