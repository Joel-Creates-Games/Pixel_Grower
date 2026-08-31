using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class GameHudController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Icons")]
    [SerializeField] private Texture2D goldIconTexture;
    [SerializeField] private Texture2D seedIconTexture;

    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Events")]
    public UnityEvent onMenuButtonClicked;

    // UI references
    private Label goldLabel;
    private Label seedLabel;
    private VisualElement goldIcon;
    private VisualElement seedIcon;
    private Button menuButton;
    private VisualElement menuPopup;
    private Button continueBtn;
    private Button mainMenuBtn;

    // Value cache to avoid GC garbage in Update
    private int lastGold = -1;
    private int lastSeedCount = -1;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;
        var root = uiDocument.rootVisualElement;

        // Popup container setup
        menuPopup = root.Q<VisualElement>("menuPopup");
        if (menuPopup != null)
            menuPopup.style.display = DisplayStyle.None;

        // Query instances
        var goldCounter = root.Q<VisualElement>("goldCounter");
        var seedCounter = root.Q<VisualElement>("seedCounter");
        var menuButtonContainer = root.Q<VisualElement>("menu-button");

        // Popup buttons
        continueBtn = root.Q<Button>("continue-button");
        mainMenuBtn = root.Q<Button>("main-menu-button");

        if (continueBtn != null) continueBtn.clicked += ToggleMenuPopup;
        if (mainMenuBtn != null) mainMenuBtn.clicked += HandleReturnToMain;

        // Gold UI setup
        if (goldCounter != null)
        {
            goldIcon = goldCounter.Q<VisualElement>("gold-icon");
            goldLabel = goldCounter.Q<Label>("gold-label");
            if (goldIcon != null && goldIconTexture != null)
                goldIcon.style.backgroundImage = new StyleBackground(goldIconTexture);
        }

        // Seed UI setup
        if (seedCounter != null)
        {
            seedIcon = seedCounter.Q<VisualElement>("seed-icon");
            seedLabel = seedCounter.Q<Label>("seed-label");
            if (seedIcon != null && seedIconTexture != null)
                seedIcon.style.backgroundImage = new StyleBackground(seedIconTexture);
        }

        // Main Menu trigger button
        if (menuButtonContainer != null)
        {
            menuButton = menuButtonContainer.Q<Button>("menu-button");
            if (menuButton != null)
                menuButton.clicked += ToggleMenuPopup;
        }

        ForceRefreshUI();
    }

    private void OnDisable()
    {
        if (menuButton != null) menuButton.clicked -= ToggleMenuPopup;
        if (continueBtn != null) continueBtn.clicked -= ToggleMenuPopup;
        if (mainMenuBtn != null) mainMenuBtn.clicked -= HandleReturnToMain;
    }

    private void Update()
    {
        if (playerInventory == null) return;

        // Only update labels if data changes
        if (playerInventory.gold != lastGold || playerInventory.seedCount != lastSeedCount)
        {
            ForceRefreshUI();
        }
    }

    private void ForceRefreshUI()
    {
        if (playerInventory == null) return;

        lastGold = playerInventory.gold;
        lastSeedCount = playerInventory.seedCount;

        if (goldLabel != null) goldLabel.text = $"Gold: {lastGold}";
        if (seedLabel != null) seedLabel.text = $"Seeds: {lastSeedCount}";
    }

    private void HandleReturnToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void ToggleMenuPopup()
    {
        if (menuPopup == null) return;

        bool isHidden = menuPopup.style.display == DisplayStyle.None;
        menuPopup.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;

        onMenuButtonClicked?.Invoke();
    }
}