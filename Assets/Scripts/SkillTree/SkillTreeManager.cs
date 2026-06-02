using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    public void RefreshAllButtons()
    {
        SkillButtonUI[] buttons = FindObjectsByType<SkillButtonUI>(FindObjectsSortMode.None);
        
        foreach (SkillButtonUI btn in buttons)
        {
            btn.UpdateVisuals();
        }
    }
}
