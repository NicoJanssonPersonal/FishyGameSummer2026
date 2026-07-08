using TMPro;
using UnityEngine;

public class wheelDescriber : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    void Start()
    {
        title.text = "";
        description.text = "";
    }
    void OnMouseEnter()
    {
        Vector3 upDirection = transform.up;
        Vector3 forwardDirection = transform.forward;

        float angleRad = Mathf.Atan2(Vector3.Dot(upDirection, Vector3.forward), Vector3.Dot(upDirection, Vector3.up));
        float cleanAngle = angleRad * Mathf.Rad2Deg;

        if (cleanAngle < 0) cleanAngle += 360f;

        int finalAngle = Mathf.RoundToInt(cleanAngle / 45f) * 45;
        if (finalAngle >= 360) finalAngle = 0;

        UpdateText(finalAngle);
    }

    void UpdateText(int angle)
    {
        if (angle == 0)
        {
            title.text = "Jackpot";
            description.text = "Jackpot, 100x your bet in coins";
        }
        else if (angle == 45)
        {
            title.text = "Flaskpost";
            description.text = "MISC";
        }
        else if (angle == 90)
        {
            title.text = "Skillpoint";
            description.text = "gain one skillpoint, can be spent in skilltree";
        }
        else if (angle == 135)
        {
            title.text = "3 Coins";
            description.text = "3x your bet";
        }
        else if (angle == 180)
        {
            title.text = "Anchor";
            description.text = "destorys slot machine. repaired next day";
        }
        else if (angle == 225)
        {
            title.text = "1 coin";
            description.text = "win back your bet";
        }
        else if (angle == 270)
        {
            title.text = "skillpoints";
            description.text = "Gain bet 2 skillpoints";
        }
        else if (angle == 315)
        {
            title.text = "Crab";
            description.text = "CrabCrabCrab";
        }
        else
        {
            ClearText();
        }
    }

    void OnMouseExit()
    {
        ClearText();
    }

    void ClearText()
    {
        title.text = "";
        description.text = "";
    }
}