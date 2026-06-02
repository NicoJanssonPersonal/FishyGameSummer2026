using UnityEngine;
using UnityEngine.UI;

public class SkillButtonUI : MonoBehaviour
{
    [Header("Skill Asset")]
    public Skills skillData;

    [Header("UI Components")]
    public Button button;
    public Image backgroundImage;
    
    private SkillTreeManager uiManager;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();

        uiManager = FindAnyObjectByType<SkillTreeManager>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnButtonClick);
        UpdateVisuals();
    }

    private void OnButtonClick()
    {
        if (skillData == null) return;

        if (skillData.CanUnlock())
        {
            skillData.Unlock();
            
            if (uiManager != null) uiManager.RefreshAllButtons();
        }
        else
        {
            Debug.Log($"Can't unlock {skillData.skillName} yet!");
        }
    }

    public void UpdateVisuals()
    {
        if (skillData == null) return;

        if (skillData.isUnlocked)
        {
            backgroundImage.color = Color.green;
            button.interactable = true;
        }
        else if (skillData.CanUnlock())
        {
            backgroundImage.color = Color.white;
            button.interactable = true;
        }
        else
        {
            backgroundImage.color = Color.gray;
            button.interactable = false; 
        }
    }
}