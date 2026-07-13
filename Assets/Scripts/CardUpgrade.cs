using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUpgrade : MonoBehaviour
{
    public TextMeshProUGUI cardText;
    private CardManager manager;
    private String[] upgrades = { "plop amount", "multi fish amount", "multi fish chance", "money gain", "xp gain", "fish rarity", "card rarity chance", "fishing range" };
    //private String[] upgrades = { "fish rarity"};
    float rarity;
    CardManager cardManager;
    void Start()
    {
        transform.Find("outline").gameObject.SetActive(false);
        rarity = getCardRarity();
        cardText.text = listOfUpgrades() + rarity.ToString() + "% increase";

    }

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
            return UnityEngine.Random.Range(1, 10);
        }
        if (transform.name.StartsWith("Uncommon"))
        {
            return UnityEngine.Random.Range(10, 20);
        }
        if (transform.name.StartsWith("Rare"))
        {
            return UnityEngine.Random.Range(20, 40);
        }
        if (transform.name.StartsWith("Legendary"))
        {
            return UnityEngine.Random.Range(40, 75);
        }
        if (transform.name.StartsWith("Chaotic"))
        {
            return UnityEngine.Random.Range(100, 250);
        }
        return 0;
    }

    void upgradeStat(String Stat)
    {
        if (Stat.StartsWith("plop amount"))
        {
            GlobalStats.plopAmount *= (1f + (rarity / 100f));
            //Debug.Log("current plopamount = " + GlobalStats.plopAmount);
        }
        if (Stat.StartsWith("multi fish amount"))
        {
            GlobalStats.multiFishAmount *= (1f + (rarity / 100f));
            //Debug.Log("current fish amount = " + GlobalStats.multiFishAmount);
        }
        if (Stat.StartsWith("multi fish chance"))
        {
            GlobalStats.multiFishChance += rarity / 100f;
            //Debug.Log("current multi fish chance = " + GlobalStats.multiFishChance);
        }
        if (Stat.StartsWith("xp gain"))
        {
            GlobalStats.xpGain *= (1f + (rarity / 100f));
            Debug.Log("current xp gain = " + GlobalStats.xpGain);
        }
        if (Stat.StartsWith("money gain"))
        {
            GlobalStats.moneyGain *= (1f + (rarity / 100f));
            Debug.Log("current money gain = " + GlobalStats.moneyGain);

        }
        if (Stat.StartsWith("fish rarity"))
        {
            GlobalStats.fishRarity *= (1f + (rarity / 100f));
            Debug.Log("current fish rarity = " + GlobalStats.fishRarity);

        }
        if (Stat.StartsWith("card rarity chance"))
        {
            GlobalStats.rarityChance *= (1f + (rarity / 100f));
            Debug.Log("current card rarity chance = " + GlobalStats.rarityChance);

        }
        if (Stat.StartsWith("fishing range"))
        {
            GlobalStats.fishingRange *= (1f + (rarity / 100f));
            Debug.Log("current fishing range = " + GlobalStats.fishingRange);
        }
        Debug.Log(GlobalStats.Level);
    }
    void OnMouseDown()
    {
        upgradeStat(cardText.text);

        if (manager != null)
        {
            manager.HideCards();
        }
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
    public void SetupCard(CardManager cardManagerInstance)
    {
        manager = cardManagerInstance;
    }
}
