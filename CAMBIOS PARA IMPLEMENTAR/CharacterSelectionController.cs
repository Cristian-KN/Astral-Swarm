using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CharacterSelectionController : MonoBehaviour
{
    [System.Serializable]
    public class AnimationData
    {
        public Sprite[] frames;
    }

    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;
    
    [Header("References to Hide")]
    public GameObject[] objectsToHide;

    [Header("Animation Settings")]
    [SerializeField] private float frameDuration = 0.1f;

    [Header("Character Animations")]
    public List<AnimationData> warriorAnims = new List<AnimationData>();
    public List<AnimationData> archerAnims = new List<AnimationData>();
    public List<AnimationData> lancerAnims = new List<AnimationData>();

    // Estado actual
    private string selectedClass = "warrior";
    private int selectedColorIndex = 0;

    private readonly string[] colorNames = { "blue", "yellow", "red", "purple", "black" };

    // Referencias UI
    private VisualElement root;
    private VisualElement[] classCards = new VisualElement[3];
    private VisualElement[] colorCards = new VisualElement[5];
    private VisualElement[] colorPreviews = new VisualElement[5];
    private VisualElement[] classIcons = new VisualElement[3];

    // Animación
    private int currentAnimationFrame = 0;
    private IVisualElementScheduledItem animationTask;

    void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

        InitializeUI();
        
        // Iniciar loop de animación
        if (animationTask == null)
        {
            animationTask = root.schedule.Execute(AnimateStep).Every((long)(frameDuration * 1000));
        }
        else
        {
            animationTask.Resume();
        }
        
        UpdateUI();
    }

    void OnDisable()
    {
        animationTask?.Pause();
    }

    void InitializeUI()
    {
        if (root == null) return;

        classCards[0] = root.Q<VisualElement>("class-warrior");
        classCards[1] = root.Q<VisualElement>("class-archer");
        classCards[2] = root.Q<VisualElement>("class-lancer");

        classIcons[0] = root.Q<VisualElement>("class-icon-warrior");
        classIcons[1] = root.Q<VisualElement>("class-icon-archer");
        classIcons[2] = root.Q<VisualElement>("class-icon-lancer");

        for (int i = 0; i < 5; i++)
        {
            colorCards[i] = root.Q<VisualElement>($"color-{colorNames[i]}");
            colorPreviews[i] = root.Q<VisualElement>($"color-preview-{i}");
        }

        classCards[0]?.RegisterCallback<ClickEvent>(evt => SelectClass("warrior"));
        classCards[1]?.RegisterCallback<ClickEvent>(evt => SelectClass("archer"));
        classCards[2]?.RegisterCallback<ClickEvent>(evt => SelectClass("lancer"));

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            colorCards[i]?.RegisterCallback<ClickEvent>(evt => SelectColor(index));
        }

        root.Q<VisualElement>("btn-back")?.RegisterCallback<ClickEvent>(evt => CloseSelection());
        root.Q<VisualElement>("btn-play")?.RegisterCallback<ClickEvent>(evt => PlayGame());
    }

    public void OpenSelection()
    {
        if (objectsToHide != null)
        {
            foreach (var obj in objectsToHide)
                if (obj != null) obj.SetActive(false);
        }
        gameObject.SetActive(true);
        UpdateUI();
    }

    public void CloseSelection()
    {
        gameObject.SetActive(false);
        if (objectsToHide != null)
        {
            foreach (var obj in objectsToHide)
                if (obj != null) obj.SetActive(true);
        }
    }

    void SelectClass(string className)
    {
        selectedClass = className;
        UpdateUI();
    }

    void SelectColor(int index)
    {
        selectedColorIndex = index;
        UpdateUI();
    }

    void AnimateStep()
    {
        currentAnimationFrame++;
        
        // Iconos de clase (siempre blue)
        if (warriorAnims != null && warriorAnims.Count > 0) UpdateElementAnim(classIcons[0], warriorAnims[0].frames);
        if (archerAnims != null && archerAnims.Count > 0) UpdateElementAnim(classIcons[1], archerAnims[0].frames);
        if (lancerAnims != null && lancerAnims.Count > 0) UpdateElementAnim(classIcons[2], lancerAnims[0].frames);

        // Previews de color
        List<AnimationData> currentAnims = warriorAnims;
        if (selectedClass == "archer") currentAnims = archerAnims;
        else if (selectedClass == "lancer") currentAnims = lancerAnims;

        if (currentAnims != null)
        {
            for (int i = 0; i < colorPreviews.Length; i++)
            {
                if (i < currentAnims.Count && currentAnims[i] != null)
                {
                    UpdateElementAnim(colorPreviews[i], currentAnims[i].frames);
                }
            }
        }
    }

    void UpdateElementAnim(VisualElement el, Sprite[] frames)
    {
        if (el == null || frames == null || frames.Length == 0) return;
        int frame = currentAnimationFrame % frames.Length;
        el.style.backgroundImage = new StyleBackground(frames[frame]);
    }

    void UpdateUI()
    {
        if (root == null) return;

        classCards[0]?.EnableInClassList("class-card--active", selectedClass == "warrior");
        classCards[1]?.EnableInClassList("class-card--active", selectedClass == "archer");
        classCards[2]?.EnableInClassList("class-card--active", selectedClass == "lancer");

        for (int i = 0; i < colorCards.Length; i++)
        {
            if (colorCards[i] != null)
                colorCards[i].EnableInClassList("color-card--active", i == selectedColorIndex);
        }
    }

    public void PlayGame()
    {
        Debug.Log($"[CharSelect] Iniciando juego con: {selectedClass} color {colorNames[selectedColorIndex]}");
        PlayerPrefs.SetString("SelectedClass", selectedClass);
        PlayerPrefs.SetString("SelectedColor", colorNames[selectedColorIndex]);
        SceneManager.LoadScene("Game");
    }
}