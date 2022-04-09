#if UNITY_EDITOR
using System.IO;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneViewWindow : EditorWindow
{
    protected Vector2 scrollPosition;
    protected OpenSceneMode openSceneMode = OpenSceneMode.Single;

    [MenuItem("Tools/Scenes View #&s")]
    public static void Init()
    {
        var window = GetWindow<SceneViewWindow>("Scenes View");
        window.minSize = new Vector2(250f, 200f);
        window.Show();
    }

    protected virtual void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        EditorGUILayout.EndHorizontal();

        openSceneMode = (OpenSceneMode)EditorGUILayout.EnumPopup("Open Scene Mode", openSceneMode);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.BeginVertical();
        ScenesTabGUI();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

        GUILayout.Label("Credits YJ and Google", EditorStyles.centeredGreyMiniLabel);
    }

    protected virtual void ScenesTabGUI()
    {
        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

        for (int i = 0, buildIndex = 0; i < buildScenes.Length; i++)
        {
            EditorGUI.BeginDisabledGroup(GetSceneLoaded(buildScenes[i].path));
            if (buildScenes[i].enabled)
            {
                GUI.contentColor = Color.white;
                if (GUILayout.Button($"{buildIndex++}: {Path.GetFileNameWithoutExtension(buildScenes[i].path)}"))
                    Open(buildScenes[i].path);
            }
            else
            {
                GUI.contentColor = Color.yellow;
                if (GUILayout.Button(Path.GetFileNameWithoutExtension(buildScenes[i].path)))
                    Open(buildScenes[i].path);
            }
            EditorGUI.EndDisabledGroup();
        }

        GUI.contentColor = Color.cyan;
        if (GUILayout.Button("Create New Scene"))
            EditorSceneManager.SaveScene(EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single));
        GUI.contentColor = Color.white;
    }

    protected virtual bool GetSceneLoaded(string path)
    {
        Scene scene = SceneManager.GetSceneByPath(path);
        return scene.IsValid() && scene.isLoaded;
    }

    public virtual void Open(string path)
    {
        if (EditorSceneManager.EnsureUntitledSceneHasBeenSaved("You haven't saved the Untitled Scene, do you want to leave?"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(path, openSceneMode);
        }
    }
}
#endif