using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(Goal))]
public class GoalEditor : Editor
{
    SerializedProperty nextSceneName;

    private void OnEnable()
    {
        nextSceneName = serializedObject.FindProperty("nextSceneName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default inspector
        DrawDefaultInspector();

        // Add a dropdown for scene selection
        if (GUILayout.Button("Select Scene", GUILayout.ExpandWidth(true)))
        {
            ShowSceneDropdown();
        }

        // Check if the scene name is valid
        if (string.IsNullOrEmpty(nextSceneName.stringValue))
        {
            EditorGUILayout.HelpBox("No next scene specified! This will cause an error when transitioning.", MessageType.Warning);
        }
        else if (!IsSceneInBuildSettings(nextSceneName.stringValue))
        {
            EditorGUILayout.HelpBox("Specified scene is not added in the build settings!", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowSceneDropdown()
    {
        GenericMenu menu = new GenericMenu();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
            menu.AddItem(new GUIContent(sceneName), false, OnSceneSelected, sceneName);
        }
        menu.ShowAsContext();
    }

    private void OnSceneSelected(object sceneName)
    {
        nextSceneName.stringValue = sceneName.ToString();
        serializedObject.ApplyModifiedProperties();
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (System.IO.Path.GetFileNameWithoutExtension(scene.path) == sceneName)
            {
                return true;
            }
        }
        return false;
    }
}
