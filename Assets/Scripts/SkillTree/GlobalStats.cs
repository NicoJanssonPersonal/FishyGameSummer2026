using UnityEngine;
public static class GlobalStats
{
    // === MOTOR ===
    private static float _thrustForce = 50f; // Acceleration
    public static float thrustForce { get => _thrustForce; set => _thrustForce = value; }
    private static float _maxSpeed = 5f;
    public static float maxSpeed { get => _maxSpeed; set => _maxSpeed = value; }

    private static float _reverseSpeedDebuff = 0.5f; // Reversing speed decrease (50% by default)
    public static float reverseSpeedDebuff { get => _reverseSpeedDebuff; set => _reverseSpeedDebuff = value; }
    // === STEERING ===
    private static float _maxTurnTorque = 50f; // Caps the raw force applied
    public static float maxTurnTorque { get => _maxTurnTorque; set => _maxTurnTorque = value; }
    private static float _turnTorque = 15f; // Turn acceleration
    public static float turnTorque { get => _turnTorque; set => _turnTorque = value; }
    private static float _minTurningRadius = 10f; // The tightest circle the boat can make
    public static float minTurningRadius { get => _minTurningRadius; set => _minTurningRadius = value; }
    private static float _maxAngularVelocity = 2f; // Caps how fast the boat can spin (rad/s)
    public static float maxAngularVelocity { get => _maxAngularVelocity; set => _maxAngularVelocity = value; }
    private static float _rudderTurnSpeed = 5f;
    public static float rudderTurnSpeed { get => _rudderTurnSpeed; set => _rudderTurnSpeed = value; }

    // === MINIGAME ===
    private static float _constantSpeed = 50f;
    public static float constantSpeed { get => _constantSpeed; set => _constantSpeed = value; }
    private static int _fishDifficulty = 1;
    public static int fishDifficulty { get => _fishDifficulty; set => _fishDifficulty = value; }
    private static float _fishingRange = 5f;
    public static float fishingRange  { get => _fishingRange ; set => _fishingRange  = value; }
    private static float _fishingStrength = 1f;
    public static float fishingStrength  { get => _fishingStrength ; set => _fishingStrength  = value; }
    

    // === PLOPS ===
    private static float _minRadius = 5f;
    public static float minRadius { get => _minRadius; set => _minRadius = value; }
    private static float _maxRadius = 100f;
    public static float maxRadius { get => _maxRadius; set => _maxRadius = value; }
    private static float _spawnIntervall = 2f;
    public static float spawnIntervall { get => _spawnIntervall; set => _spawnIntervall = value; }
    private static float _plopAmount = 2;
    public static float plopAmount { get => _plopAmount; set => _plopAmount = value; }

    // === Levelup System ===
    private static float _experince = 0f;
    public static float Experince { get => _experince; set => _experince = value; }
    public static int _level = 0;
    public static int Level { get => _level; set => _level = value; }
    private static float _expTonNextLevel = 2;
    public static float expTonNextLevel { get => _expTonNextLevel; set => _expTonNextLevel = value; }
    private static float _rarityChance = 2f;
    public static float rarityChance { get => _rarityChance; set => _rarityChance = value; }

    // === Currency System ===
    public static int _money = 1000;
    public static int money { get => _money; set => _money = value; }
    public static int _skillpoints = 300;
    public static int skillpoints { get => _skillpoints; set => _skillpoints = value; }
    public static int _nodesUnlocked = 0;
    public static int nodesUnlocked { get => _nodesUnlocked; set => _nodesUnlocked = value; }

    // === Fishing ===
    // multifish chance
    private static float _multiFishChance = 0f;
    public static float multiFishChance { get => _multiFishChance; set => _multiFishChance = value; }
    // multi fish
    private static float _multiFishAmount = 1f;
    public static float multiFishAmount { get => _multiFishAmount; set => _multiFishAmount = value; }

    // xp and money gain
    private static float _xpGain = 1f;
    public static float xpGain { get => _xpGain; set => _xpGain = value; }
    private static float _moneyGain = 1f;
    public static float moneyGain { get => _moneyGain; set => _moneyGain = value; }

    // fish rarity chance
    private static float _fishRarity = 50f;
    public static float fishRarity { get => _fishRarity; set => _fishRarity = value; }

    // === SkillTree ===
    private static bool _cannon = false;
    public static bool cannon { get => _cannon; set => _cannon = value; }

    private static float _maxHealth = 100;
    public static float maxHealth { get => _maxHealth; set => _maxHealth = value; }
    private static float _currentHealth = 100;
    public static float currentHealth { get => _currentHealth; set => _currentHealth = value; }

    public static void SaveStats()
    {
        PlayerPrefs.SetFloat("GS_thrustForce", thrustForce);
        PlayerPrefs.SetFloat("GS_maxSpeed", maxSpeed);
        PlayerPrefs.SetFloat("GS_reverseSpeedDebuff", reverseSpeedDebuff);
        PlayerPrefs.SetFloat("GS_maxTurnTorque", maxTurnTorque);
        PlayerPrefs.SetFloat("GS_turnTorque", turnTorque);
        PlayerPrefs.SetFloat("GS_minTurningRadius", minTurningRadius);
        PlayerPrefs.SetFloat("GS_maxAngularVelocity", maxAngularVelocity);
        PlayerPrefs.SetFloat("GS_rudderTurnSpeed", rudderTurnSpeed);

        PlayerPrefs.SetFloat("GS_constantSpeed", constantSpeed);
        PlayerPrefs.SetInt("GS_fishDifficulty", fishDifficulty);
        PlayerPrefs.SetFloat("GS_fishingRange", fishingRange);
        PlayerPrefs.SetFloat("GS_fishingStrength", fishingStrength);
        PlayerPrefs.SetFloat("GS_minRadius", minRadius);
        PlayerPrefs.SetFloat("GS_maxRadius", maxRadius);
        PlayerPrefs.SetFloat("GS_spawnIntervall", spawnIntervall);
        PlayerPrefs.SetFloat("GS_plopAmount", plopAmount);

        PlayerPrefs.SetFloat("GS_Experince", Experince);
        PlayerPrefs.SetInt("GS_Level", Level);
        PlayerPrefs.SetFloat("GS_expTonNextLevel", expTonNextLevel);
        PlayerPrefs.SetFloat("GS_rarityChance", rarityChance);
        PlayerPrefs.SetInt("GS_money", money);
        PlayerPrefs.SetInt("GS_skillpoints", skillpoints);
        PlayerPrefs.SetInt("GS_nodesUnlocked", nodesUnlocked);

        PlayerPrefs.SetFloat("GS_multiFishChance", multiFishChance);
        PlayerPrefs.SetFloat("GS_multiFishAmount", multiFishAmount);
        PlayerPrefs.SetFloat("GS_xpGain", xpGain);
        PlayerPrefs.SetFloat("GS_moneyGain", moneyGain);
        PlayerPrefs.SetFloat("GS_fishRarity", fishRarity);

        PlayerPrefs.SetFloat("GS_maxHealth", maxHealth);

        PlayerPrefs.SetInt("GS_cannon", cannon ? 1 : 0);

        PlayerPrefs.Save();
        Debug.Log("GlobalStats successfully saved!");
    }
    public static void SaveMoneyAndSkillpoints()
    {
        PlayerPrefs.SetInt("GS_money", money);
        PlayerPrefs.SetInt("GS_skillpoints", skillpoints);

        PlayerPrefs.Save();
        Debug.Log("money and skillpoints successfully saved!");
    }
    public static void LoadStats()
    {
        if (!PlayerPrefs.HasKey("GS_thrustForce")) 
        {
            Debug.Log("No GlobalStats save data found. Using class defaults.");
            return; 
        }

        thrustForce = PlayerPrefs.GetFloat("GS_thrustForce");
        maxSpeed = PlayerPrefs.GetFloat("GS_maxSpeed");
        reverseSpeedDebuff = PlayerPrefs.GetFloat("GS_reverseSpeedDebuff");
        maxTurnTorque = PlayerPrefs.GetFloat("GS_maxTurnTorque");
        turnTorque = PlayerPrefs.GetFloat("GS_turnTorque");
        minTurningRadius = PlayerPrefs.GetFloat("GS_minTurningRadius");
        maxAngularVelocity = PlayerPrefs.GetFloat("GS_maxAngularVelocity");
        rudderTurnSpeed = PlayerPrefs.GetFloat("GS_rudderTurnSpeed");

        constantSpeed = PlayerPrefs.GetFloat("GS_constantSpeed");
        fishDifficulty = PlayerPrefs.GetInt("GS_fishDifficulty");
        fishingRange = PlayerPrefs.GetFloat("GS_fishingRange");
        fishingStrength = PlayerPrefs.GetFloat("GS_fishingStrength");
        minRadius = PlayerPrefs.GetFloat("GS_minRadius");
        maxRadius = PlayerPrefs.GetFloat("GS_maxRadius");
        spawnIntervall = PlayerPrefs.GetFloat("GS_spawnIntervall");
        plopAmount = PlayerPrefs.GetFloat("GS_plopAmount");

        Experince = PlayerPrefs.GetFloat("GS_Experince");
        Level = PlayerPrefs.GetInt("GS_Level");
        expTonNextLevel = PlayerPrefs.GetFloat("GS_expTonNextLevel");
        rarityChance = PlayerPrefs.GetFloat("GS_rarityChance");
        money = PlayerPrefs.GetInt("GS_money");
        skillpoints = PlayerPrefs.GetInt("GS_skillpoints");
        nodesUnlocked = PlayerPrefs.GetInt("GS_nodesUnlocked");

        multiFishChance = PlayerPrefs.GetFloat("GS_multiFishChance");
        multiFishAmount = PlayerPrefs.GetFloat("GS_multiFishAmount");
        xpGain = PlayerPrefs.GetFloat("GS_xpGain");
        moneyGain = PlayerPrefs.GetFloat("GS_moneyGain");
        fishRarity = PlayerPrefs.GetFloat("GS_fishRarity");

        PlayerPrefs.GetFloat("GS_maxHealth", maxHealth);


        cannon = PlayerPrefs.GetInt("GS_cannon") == 1;

        Debug.Log("GlobalStats successfully loaded!");
    }
}