using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UIElements;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// Handles smooth scene transitions with fade effects and checkpoint loading.
/// Uses DOTween for fade animations and maintains transition state across scenes.
/// </summary>
public class SceneTransitioner : Singleton<SceneTransitioner>
{
    [Header("Transition Settings")]
    [Tooltip("Duration of the fade transition in seconds")]
    [SerializeField] private float fadeDuration = 0.5f;

    VisualElement faderElement;

    public UnityEvent<string> NewSceneLoaded = new UnityEvent<string>();
    public static bool Transitioning { get; private set; } = true;

    private string cachedNameForReset;

    /// <summary>
    /// Ensures only one instance exists and persists across scenes
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        if (creationFailed)
            return;
        
        CreateFaderVisualElement();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected void Start()
    {
        // Since we're just starting we should let everyone know the current scene is loaded.
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void CreateFaderVisualElement()
    {
        faderElement = (new VisualElement());
        faderElement.style.opacity = 1.0f;
        faderElement.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        faderElement.style.height = faderElement.style.width;
        faderElement.style.position = new StyleEnum<Position>(Position.Absolute);
        faderElement.style.backgroundColor = Color.black;
        faderElement.DoFade(0.0f, 1.2f).SetDelay(0.5f);
        GameManager.Instance.MenuHelper.uiDocument.rootVisualElement.Add(faderElement);
    }


    private void OnDestroy()
    {
        if (Instance == this) {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// Initiates a scene transition with fade effect
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public static void LoadScene(string sceneName)
    {        
        Instance.StartTransition(sceneName);
    }

    /// <summary>
    /// Performs the fade out, scene load, and fade in sequence
    /// </summary>
    private void StartTransition(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            cachedNameForReset = sceneName;
            sceneName = "TransitionScene";
        }
        Transitioning = true;
        faderElement.DoFade(1.0f, fadeDuration).OnComplete(() => {
            SceneManager.LoadScene(sceneName);
        });
    }

    /// <summary>
    /// Handles the fade in after a new scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "TransitionScene")
        {
            SceneManager.LoadScene(cachedNameForReset);
            return;
        }
        Transitioning = false;
        faderElement.DoFade(0.0f, fadeDuration).OnComplete(() => { Transitioning = false;
            faderElement.pickingMode = PickingMode.Ignore;
        });
        NewSceneLoaded?.Invoke(scene.name);

        // If we have a player remove the camera from the scene.
        DisableOtherSceneCameras();
    }

    public static void DisableOtherSceneCameras(bool force = false)
    {
        for (int i = 0; i < Camera.allCameras.Length; i++)
        {
            if (Camera.allCameras[i].TryGetComponent(out AudioListener audioListener))
                GameObject.Destroy(audioListener);
        }
        if (PlayerController.players.Count > 0 || force)
        {
            List<Camera> camerasToDisable = new List<Camera>();
            for (int i = 0; i < Camera.allCameras.Length; i++)
            {
                if (!Camera.allCameras[i].TryGetComponent(out ThirdPersonCamera thirdPersonCam))
                    camerasToDisable.Add(Camera.allCameras[i]);
            }
            foreach (var cam in camerasToDisable)
            {
                cam.gameObject.SetActive(false);
            }
        }
    }
} 