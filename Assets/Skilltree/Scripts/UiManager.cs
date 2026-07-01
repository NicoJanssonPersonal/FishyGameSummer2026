using TMPro;
using UnityEngine;

public class UiManagerSkilltree : MonoBehaviour
{
    public TextMeshProUGUI skillPoints;
    public TextMeshProUGUI money;
    void Start()
    {
        skillPoints.text = "skillpoints " + GlobalStats.skillpoints.ToString();
        money.text = "money " + GlobalStats.money.ToString();
    }

    void Update()
    {
        skillPoints.text = GlobalStats.skillpoints.ToString();
        money.text = GlobalStats.money.ToString();
    }
}
