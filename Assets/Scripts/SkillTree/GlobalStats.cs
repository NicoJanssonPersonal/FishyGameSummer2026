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

    // === PLOPS ===
    private static float _minRadius = 5f;
    public static float minRadius { get => _minRadius; set => _minRadius = value; }
    private static float _maxRadius = 15f;
    public static float maxRadius { get => _maxRadius; set => _maxRadius = value; }
    private static float _spawnIntervall = 2f;
    public static float spawnIntervall { get => _spawnIntervall; set => _spawnIntervall = value; }
    private static float _plopAmount = 1;
    public static float plopAmount { get => _plopAmount; set => _plopAmount = value; }

    // === Levelup System ===
    private static float _experince = 0f;
    public static float Experince { get => _experince; set => _experince = value; }
    public static int _level = 0;
    public static int Level { get => _level; set => _level = value; }
    private static float _expTonNextLevel = 2;
    public static float expTonNextLevel { get => _expTonNextLevel; set => _expTonNextLevel = value; }
    private static float _rarityChance = 1f;
    public static float rarityChance { get => _rarityChance; set => _rarityChance = value; }

    // === Fishing ===
    // multifish chance
    private static float _multiFishChance = 0f;
    public static float multiFishChance { get => _multiFishChance; set => _multiFishChance = value; }
    // multi fish
    private static float _multiFishAmount = 1f;
    public static float multiFishAmount { get => _multiFishAmount; set => _multiFishAmount = value; }
    // xp and money gain
    private static float _xpGain = 0f;
    public static float xpGain { get => _xpGain; set => _xpGain = value; }
    private static float _moneyGain = 0f;
    public static float moneyGain { get => _moneyGain; set => _moneyGain = value; }
    // fish rarity chance
    private static float _fishRarity = 0f;
    public static float fishRarity { get => _fishRarity; set => _fishRarity = value; }

    // === Character specific ===
}