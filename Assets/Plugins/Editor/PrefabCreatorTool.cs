using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class PrefabCreatorTool : EditorWindow
{
    private List<GameObject> sourceModels = new List<GameObject>();
    private Material selectedMaterial;
    private string outputPath = "Assets/Prefabs";
    private Vector2 scrollPosition;

    [MenuItem("Tools/Prefab Creator")]
    public static void ShowWindow()
    {
        GetWindow<PrefabCreatorTool>("Prefab Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Prefab Creator Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Material selection
        selectedMaterial = (Material)EditorGUILayout.ObjectField("Material", selectedMaterial, typeof(Material), false);

        // Output path selection
        EditorGUILayout.BeginHorizontal();
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                outputPath = "Assets" + path.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Drag and drop model files here:", EditorStyles.boldLabel);

        // Model list area with scroll view
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Drag and drop area
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag Models Here");

        // Display list of added models
        for (int i = 0; i < sourceModels.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            sourceModels[i] = (GameObject)EditorGUILayout.ObjectField(sourceModels[i], typeof(GameObject), false);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                sourceModels.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // Handle drag and drop
        if (Event.current.type == EventType.DragUpdated && dropArea.Contains(Event.current.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            Event.current.Use();
        }
        else if (Event.current.type == EventType.DragPerform && dropArea.Contains(Event.current.mousePosition))
        {
            DragAndDrop.AcceptDrag();
            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is GameObject)
                {
                    sourceModels.Add((GameObject)draggedObject);
                }
            }
            Event.current.Use();
        }

        EditorGUILayout.Space();

        GUI.enabled = sourceModels.Count > 0;
        if (GUILayout.Button("Create Prefabs"))
        {
            CreatePrefabs();
        }
        GUI.enabled = true;
    }

    private void CreatePrefabs()
    {
        // Ensure output directory exists
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        int amountPrefabs = 0;
        foreach (GameObject sourceModel in sourceModels)
        {
            // Create new instance of the model
            GameObject instance = Instantiate(sourceModel);
            if (instance.transform.childCount == 1)
            {
                var child = instance.transform.GetChild(0);
                child.SetParent(instance.transform.parent);
                DestroyImmediate(instance);
                instance = child.gameObject;
            }

            if (instance.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
                HandleMeshRenderer(mr);
            // Add mesh colliders to all mesh renderers
            foreach (MeshRenderer meshRenderer in instance.GetComponentsInChildren<MeshRenderer>())
            {
                HandleMeshRenderer(meshRenderer);
            }

            // Create prefab
            string prefabPath = Path.Combine(outputPath, sourceModel.name + ".prefab");
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            amountPrefabs++;

            // Clean up instance
            DestroyImmediate(instance);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", $"{amountPrefabs} prefabs created successfully!", "OK");
    }

    private void HandleMeshRenderer(MeshRenderer meshRenderer)
    {
        // Get the mesh filter attached to the same object as the mesh renderer
        MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            // Add mesh collider if it doesn't exist
            if (!meshRenderer.gameObject.GetComponent<MeshCollider>())
            {
                MeshCollider collider = meshRenderer.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilter.sharedMesh;
            }

            // Set material
            if (selectedMaterial != null)
                meshRenderer.sharedMaterial = selectedMaterial;
        }
    }
}
