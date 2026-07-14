using TMPro;
using UnityEngine;

public class slotmachineMoneyHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI money;
    public TextMeshProUGUI skillPoint;
    
    void Start()
    {
        money.text = GlobalStats.money.ToString();
        skillPoint.text = GlobalStats.skillpoints.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        money.text = GlobalStats.money.ToString();
        skillPoint.text = GlobalStats.skillpoints.ToString();
    }
}
