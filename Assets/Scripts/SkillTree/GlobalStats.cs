public static class GlobalStats
{
    // === MOTOR ===
    public static float thrustForce = 50f; // Acceleration
    public static float maxSpeed = 5f; // Top speed
    public static float reverseSpeedDebuff = 0.5f; // Reversing speed decrease (50% by default)
    public static float sidewaysGrip = 2.5f; // ??? (De säger PANG om den e för låg)
    public static float forwardDrag = 0.5f; // Water resistance

    // === STEERING ===
    public static float maxTurnTorque = 50f; // Caps the raw force applied
    public static float turnTorque = 15f; // Turn acceleration
    public static float minTurningRadius = 10f; // The tightest circle the boat can make
    public static float maxAngularVelocity = 2f; // Caps how fast the boat can spin (rad/s)
    public static float maxRudderAngle = 35f;
    public static float rudderTurnSpeed = 5f;

    // === CAMERA ===
    public static float maxCamFOV = 80f;
    public static float minCamFOV = 70f;

    // === REVERSE ===
    public static float currentXRotation = 40f;
    public static float reverseTiltAngle = 45f;

    // === MINIGAME ===
    public static float constantSpeed = 50f;
    public static int fishDifficulty = 1;

    // === PLOPS ===
    public static float minRadius = 5f;
    public static float maxRadius = 15f;
    public static float spawnIntervall = 2f;
}