using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewCardController : MonoBehaviour
{
    private NewCardManager newCardManager;

    public TextMeshProUGUI cardText;
    public Image cardTextPlace;
    public Image cardSymbolPlace;
    public Image cardSymbolBoxPlace;
    public Image cardDecorationsPlace;
    public Image cardRarityIndicatorPlace;
    public Image cardRarityBannerPlace;
    public Image cardBackground;


    public Sprite[] cardImages;
    public Sprite[] cardRarityBanner;
    public Sprite[] cardRarityIndicator;

    public Color[] rarityColors = { Color.lightBlue, Color.limeGreen, Color.purple, Color.gold };

    private float upgradeAmount = 0;
    private int cardRarity = 0;

    private String upgradeType = "";
    void Start()
    {
        newCardManager = GetComponentInParent<NewCardManager>();
        Sprite cardImg = cardImages[UnityEngine.Random.Range(0, cardImages.Length)];
        cardRarity = weightedRarityCalculation();
        populateCard(cardImg.name, cardImg, cardRarity);
    }
    private int weightedRarityCalculation()
    {
        // Calculate luck-adjusted weights (0 = Common, 1 = Rare, 2 = Epic, 3 = Legendary)
        float commonWeight = Mathf.Max(10f, 70f - (GlobalStats.rarityChance * 4.0f));
        float rareWeight = 20f + (GlobalStats.rarityChance * 2.0f);
        float epicWeight = 8f + (GlobalStats.rarityChance * 1.5f);
        float legendaryWeight = 2f + (GlobalStats.rarityChance * 0.5f);

        float totalWeight = commonWeight + rareWeight + epicWeight + legendaryWeight;
        float randomRoll = UnityEngine.Random.Range(0f, totalWeight);

        if (randomRoll < commonWeight) return 0; // common
        randomRoll -= commonWeight;

        if (randomRoll < rareWeight) return 1; // uncommon
        randomRoll -= rareWeight;

        if (randomRoll < epicWeight) return 2; // rare

        return 3; // Legendary
    }

    public void populateCard(String Text, Sprite cardSymbol, int cardRarity) // , Image cardRarity, Image cardDecorations
    {

        cardSymbolPlace.sprite = cardSymbol;
        cardSymbolPlace.preserveAspect = true;

        cardRarityIndicatorPlace.sprite = cardRarityIndicator[cardRarity];
        cardRarityIndicatorPlace.preserveAspect = true;
        cardRarityBannerPlace.sprite = cardRarityBanner[cardRarity];
        cardRarityBannerPlace.preserveAspect = true;

        changeColor(rarityColors[cardRarity]);
        changeText(Text);

    }
    private void changeColor(Color color)
    {
        //cardRarityIndicatorPlace.color = color;
        //cardRarityBannerPlace.color = color;
        //cardBackground.color = color;
        //cardTextPlace.color = color;
        cardSymbolBoxPlace.color = color;
    }
    private void changeText(string text)
    {
        string realText = "";

        switch (text)
        {
            case "cardsIconCardRarity":
                upgradeType = "rarity";
                realText = "card rarity upgraded by ";
                break;
            case "cardsIconFishRarity":
                upgradeType = "rarity";
                realText = "fish rarity upgraded by ";
                break;
            case "cardsIconMoreCoins":
                upgradeType = "economy";
                realText = "money gain upgraded by ";
                break;
            case "cardsIconPlopAmount":
                upgradeType = "fishing";
                realText = "plop amount spawn upgraded by ";
                break;
            case "cardsIconFishingRange":
                upgradeType = "fishing";
                realText = "fishing range upgraded by ";
                break;
            case "cardsIconMoreXP":
                upgradeType = "economy";
                realText = "xp gain upgraded by ";
                break;
            default:
                realText = "error";
                break;
        }

        cardText.text = realText + determineUpgradeamount().ToString() + " %";
    }
    private float determineUpgradeamount()
    {
        float baseValue = 0;
        switch (upgradeType)
        {
            case "rarity":
                baseValue = 5f;
                break;
            case "fishing":
                baseValue = 15f;
                break;
            case "economy":
                baseValue = 10f;
                break;
            default:
                baseValue = 0;
                break;
        }
        upgradeAmount = baseValue * (cardRarity + 1);
        return upgradeAmount;
    }
    public void cardClicked()
    {
        Debug.Log(cardSymbolPlace.sprite.name);
        upgradeStat(cardText.text);
        newCardManager.destroyCards();
    }
    void upgradeStat(String Stat)
    {
        if (Stat.StartsWith("plop amount"))
        {
            GlobalStats.plopAmount *= 1f + upgradeAmount/100f;
            Debug.Log("current plopamount = " + GlobalStats.plopAmount);
        }
        if (Stat.StartsWith("xp gain"))
        {
            GlobalStats.xpGain *= 1f + upgradeAmount/100f;
            Debug.Log("current xp gain = " + GlobalStats.xpGain);
        }
        if (Stat.StartsWith("money gain"))
        {
            GlobalStats.moneyGain *= 1f + upgradeAmount/100f;
            Debug.Log("current money gain = " + GlobalStats.moneyGain);

        }
        if (Stat.StartsWith("fish rarity"))
        {
            GlobalStats.fishRarity *= 1f + upgradeAmount/100f;
            Debug.Log("current fish rarity = " + GlobalStats.fishRarity);

        }
        if (Stat.StartsWith("card rarity"))
        {
            GlobalStats.rarityChance *= 1f + upgradeAmount/100f;
            Debug.Log("current card rarity chance = " + GlobalStats.rarityChance);

        }
        if (Stat.StartsWith("fishing range"))
        {
            GlobalStats.fishingRange *= 1f + upgradeAmount/100f;
            Debug.Log("current fishing range = " + GlobalStats.fishingRange);
        }
        //Debug.Log(GlobalStats.Level);
    }
}
