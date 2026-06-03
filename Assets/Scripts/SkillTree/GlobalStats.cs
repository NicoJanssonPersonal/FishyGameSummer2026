public static class GlobalStats
{
    // === MOTOR ===
    public static float thrustForce = 50f; // Acceleration
    public static float maxSpeed = 5f; // Top speed
    public static float reverseSpeedDebuff = 0.5f; // Reversing speed decrease (50% by default)

    // === STEERING ===
    public static float maxTurnTorque = 50f; // Caps the raw force applied
    public static float turnTorque = 15f; // Turn acceleration
    public static float minTurningRadius = 10f; // The tightest circle the boat can make
    public static float maxAngularVelocity = 2f; // Caps how fast the boat can spin (rad/s)
    public static float rudderTurnSpeed = 5f;

    // === MINIGAME ===
    public static float constantSpeed = 50f;
    public static int fishDifficulty = 1;

    // === PLOPS ===
    public static float minRadius = 5f;
    public static float maxRadius = 15f;
    public static float spawnIntervall = 2f;
    public static int plopAmount = 1;
}