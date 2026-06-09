using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUpgrade : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI cardText;
    private String[] upgrades = {"plop amount", "fish amount"};
    float rarity;
    CardManager cardManager;
    void Start()
    {
        transform.Find("outline").gameObject.SetActive(false);
        rarity = getCardRarity();
        cardText.text = listOfUpgrades() + rarity.ToString() + "% increase";
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    string listOfUpgrades()
    {
        if (upgrades.Length == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, upgrades.Length);

        string chosenItem = upgrades[randomIndex];

        return chosenItem;
    }
    float getCardRarity()
    {
        if (transform.name.StartsWith("Common"))
        {
            return UnityEngine.Random.Range(1,10);
        }
        if (transform.name.StartsWith("Uncommon"))
        {
            return UnityEngine.Random.Range(10,20);
        }
        if (transform.name.StartsWith("Rare"))
        {
            return UnityEngine.Random.Range(20,40);
        }
        if (transform.name.StartsWith("Legendary"))
        {
            return  UnityEngine.Random.Range(40,75);
        }
        if (transform.name.StartsWith("Chaotic"))
        {
            return UnityEngine.Random.Range(100,250);
        }
        return 0;
    }

    void upgradeStat(String Stat)
    {
        if (Stat.StartsWith("plop amount"))
        {
            //Debug.Log("plop amount was upgrade by " + rarity + " %");
            GlobalStats.plopAmount *= (1f + (rarity / 100f));
            Debug.Log("current plopamount = " + GlobalStats.plopAmount);
            GlobalStats.Experince = 0;
            Debug.Log("current xp " + GlobalStats.Experince);
            GlobalStats.Level = GlobalStats.Level + 1;
        }
        if (Stat.StartsWith("fish amount"))
        {
            //Debug.Log("plop amount was upgrade by " + rarity + " %");
            GlobalStats.multiFishAmount *= (1f + (rarity / 100f));
            Debug.Log("current fish amount = " + GlobalStats.multiFishAmount);
            GlobalStats.Experince =  0;
            Debug.Log("current xp " + GlobalStats.Experince);
            GlobalStats.Level = GlobalStats.Level + 1;
        }
        
    }
    void OnMouseDown()
    {
       upgradeStat(cardText.text);
    }
    void OnMouseEnter()
    {
        transform.localScale *= 1.2f;
        transform.Find("outline").gameObject.SetActive(true);
    }
    void OnMouseExit()
    {
        transform.localScale /= 1.2f;
        transform.Find("outline").gameObject.SetActive(false);
    }
}
