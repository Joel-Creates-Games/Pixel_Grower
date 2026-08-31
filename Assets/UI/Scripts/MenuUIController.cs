using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

#if !UNITY_WEBGL || UNITY_EDITOR
using SFB;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuUIController : MonoBehaviour
{
    // Reference to the UIDocument in the scene
    private UIDocument uiDocument;

    // Containers
    private VisualElement mainMenu;
    private VisualElement uploadMenu;
    private VisualElement optionsMenu;

    // Main menu buttons
    private Button startButton;
    private Button uploadButton;
    private Button optionsButton;
    private Button exitButton;

    // Upload menu elements
    private Button chooseFileButton;
    private VisualElement previewImage;
    private Button backFromUpload;

    // Options menu elements
    private DropdownField resolutionDropdown;
    private Slider volumeSlider;
    private Toggle fullscreenToggle;
    private Toggle highContrastToggle;
    private Button backFromOptions;

    // Available resolutions
    private Resolution[] resolutions;

    [SerializeField] private ImagePacket imagePacket;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void OpenWebGlFilePicker(string extensions, string gameObjectName, string callbackMethodName);
#endif

    private void OnEnable()
    {
        // Grab the UIDocument and root
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Cache panels
        mainMenu = root.Q<VisualElement>("main-menu");
        uploadMenu = root.Q<VisualElement>("upload-menu");
        optionsMenu = root.Q<VisualElement>("options-menu");

        // Main menu buttons
        startButton = root.Q<Button>("start-button");
        uploadButton = root.Q<Button>("upload-button");
        optionsButton = root.Q<Button>("options-button");
        exitButton = root.Q<Button>("exit-button");

        startButton.clicked += OnStartClicked;
        uploadButton.clicked += () => ShowScreen(uploadMenu);
        optionsButton.clicked += () => ShowScreen(optionsMenu);
        exitButton.clicked += OnExitClicked;

        // Upload menu
        chooseFileButton = root.Q<Button>("choose-file-button");
        previewImage = root.Q<VisualElement>("preview-image");
        backFromUpload = root.Q<Button>("back-from-upload");

        chooseFileButton.clicked += OnChooseFileClicked;
        backFromUpload.clicked += () => ShowScreen(mainMenu);

        // Options menu
        resolutionDropdown = root.Q<DropdownField>("resolution-dropdown");
        volumeSlider = root.Q<Slider>("volume-slider");
        fullscreenToggle = root.Q<Toggle>("fullscreen-toggle");
        highContrastToggle = root.Q<Toggle>("high-contrast-toggle");
        backFromOptions = root.Q<Button>("back-from-options");

        volumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(evt.newValue));
        fullscreenToggle.RegisterValueChangedCallback(evt => OnFullscreenChanged(evt.newValue));
        highContrastToggle.RegisterValueChangedCallback(evt => OnHighContrastChanged(evt.newValue));
        backFromOptions.clicked += () => ShowScreen(mainMenu);

        PopulateResolutions();
        ApplySavedSettings();

        ShowScreen(mainMenu);
    }

    private void ShowScreen(VisualElement screen)
    {
        mainMenu.style.display = DisplayStyle.None;
        uploadMenu.style.display = DisplayStyle.None;
        optionsMenu.style.display = DisplayStyle.None;

        screen.style.display = DisplayStyle.Flex;
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void OnExitClicked()
    {
        Application.Quit();
    }

    private void OnChooseFileClicked()
    {
#if UNITY_EDITOR
        string editorPath = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(editorPath) && File.Exists(editorPath))
        {
            LoadImageFromPath(editorPath);
        }
#elif UNITY_WEBGL
        // WebGL uses native browser file picker via JS bridge
        OpenWebGlFilePicker(".png,.jpg,.jpeg", gameObject.name, nameof(OnFileUploadedFromWebGL));
#elif UNITY_STANDALONE
        var filters = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", filters, false);
        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            LoadImageFromPath(paths[0]);
        }
#else
        Debug.LogWarning("File picker is not supported on this platform.");
#endif
    }

#if UNITY_WEBGL
    // Callback automatically invoked by the JavaScript SendMessage hook
    public void OnFileUploadedFromWebGL(string blobUrl)
    {
        if (!string.IsNullOrEmpty(blobUrl))
        {
            StartCoroutine(LoadImageFromWebGL(blobUrl));
        }
    }

    private IEnumerator LoadImageFromWebGL(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                ApplyLoadedTexture(tex, url);
            }
            else
            {
                Debug.LogError($"Failed to load WebGL image: {uwr.error}");
            }
        }
    }
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
    private void LoadImageFromPath(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"File does not exist at path: {path}");
            return;
        }

        byte[] bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2);

        if (tex.LoadImage(bytes))
        {
            ApplyLoadedTexture(tex, path);
        }
        else
        {
            Debug.LogError("Failed to decode image data.");
        }
    }
#endif

    private void ApplyLoadedTexture(Texture2D tex, string sourceIdentifier)
    {
        if (previewImage != null)
        {
            previewImage.style.backgroundImage = new StyleBackground(tex);
        }

        if (imagePacket != null)
        {
            imagePacket.SetImage(tex);
        }

        Debug.Log($"Applied texture successfully from: {sourceIdentifier}");
    }

    private void PopulateResolutions()
    {
        resolutions = Screen.resolutions;
        var options = new string[resolutions.Length];
        for (int i = 0; i < resolutions.Length; i++)
        {
            var r = resolutions[i];
            options[i] = $"{r.width} x {r.height}";
        }
        resolutionDropdown.choices = new List<string>(options);
        resolutionDropdown.RegisterValueChangedCallback(evt => OnResolutionChanged(evt.newValue));
    }

    private void OnResolutionChanged(string choice)
    {
#if !UNITY_WEBGL
        int idx = resolutionDropdown.choices.IndexOf(choice);
        if (idx >= 0 && resolutions != null && idx < resolutions.Length)
        {
            var res = resolutions[idx];
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
            PlayerPrefs.SetInt("resolutionIndex", idx);
        }
#endif
    }

    private void OnVolumeChanged(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("masterVolume", volume);
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
#if !UNITY_WEBGL
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
#else
        Screen.fullScreen = isFullscreen;
#endif
    }

    private void OnHighContrastChanged(bool enabled)
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            var root = uiDocument.rootVisualElement;
            root.EnableInClassList("high-contrast", enabled);
            PlayerPrefs.SetInt("highContrast", enabled ? 1 : 0);
        }
    }

    private void ApplySavedSettings()
    {
        float vol = PlayerPrefs.GetFloat("masterVolume", 1f);
        volumeSlider.value = vol;
        AudioListener.volume = vol;

        bool hc = PlayerPrefs.GetInt("highContrast", 0) == 1;
        highContrastToggle.value = hc;
        uiDocument.rootVisualElement.EnableInClassList("high-contrast", hc);

#if !UNITY_WEBGL
        bool fs = PlayerPrefs.GetInt("fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        fullscreenToggle.value = fs;
        Screen.fullScreen = fs;

        if (resolutions != null && resolutions.Length > 0)
        {
            int idx = PlayerPrefs.GetInt("resolutionIndex", resolutions.Length - 1);
            idx = Mathf.Clamp(idx, 0, resolutions.Length - 1);

            if (resolutionDropdown.choices != null && resolutionDropdown.choices.Count > idx)
            {
                resolutionDropdown.value = resolutionDropdown.choices[idx];
            }

            var r = resolutions[idx];
            Screen.SetResolution(r.width, r.height, fs);
        }
#else
        if (resolutionDropdown != null) resolutionDropdown.style.display = DisplayStyle.None;
        if (fullscreenToggle != null) fullscreenToggle.style.display = DisplayStyle.None;
#endif
    }
}