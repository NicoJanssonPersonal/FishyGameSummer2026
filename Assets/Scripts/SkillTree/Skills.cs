using UnityEngine;

[CreateAssetMenu(fileName = "Skills", menuName = "Scriptable Objects/Skills")]
public class Skills : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public bool isUnlocked;
    public Skills prerequisiteSkill;

    public enum StatType
    {
        // === MOTOR ===
        ThrustForce,
        MaxSpeed,
        ReverseSpeedDebuff,

        // === STEERING ===
        MaxTurnTorque,
        TurnTorque,
        MinTurningRadius,
        MaxAngularVelocity,
        RudderTurnSpeed,

        // === MINIGAME ===
        ConstantSpeed,
        FishDifficulty,

        // === PLOPS ===
        MinRadius,
        MaxRadius,
        SpawnInterval,
        plopAmount
    }

    public StatType targetStat;
    public float upgradeAmount;

    public bool CanUnlock()
    {
        if (isUnlocked) return false;
        if (prerequisiteSkill != null && !prerequisiteSkill.isUnlocked) return false;
        return true;
    }

    private void OnEnable()
    {
        // remove when play testing
        isUnlocked = false;
    }
    public void Unlock()
    {
        isUnlocked = true;

        switch (targetStat)
        {
            // === MOTOR ===
            case StatType.ThrustForce:
                GlobalStats.thrustForce += upgradeAmount;
                break;
            case StatType.MaxSpeed:
                GlobalStats.maxSpeed += upgradeAmount;
                break;
            case StatType.ReverseSpeedDebuff:
                GlobalStats.reverseSpeedDebuff += upgradeAmount;
                break;

            // === STEERING ===
            case StatType.MaxTurnTorque:
                GlobalStats.maxTurnTorque += upgradeAmount;
                break;
            case StatType.TurnTorque:
                GlobalStats.turnTorque += upgradeAmount;
                break;
            case StatType.MinTurningRadius:
                GlobalStats.minTurningRadius += upgradeAmount;
                break;
            case StatType.MaxAngularVelocity:
                GlobalStats.maxAngularVelocity += upgradeAmount;
                break;
            case StatType.RudderTurnSpeed:
                GlobalStats.rudderTurnSpeed += upgradeAmount;
                break;

            // === MINIGAME ===
            case StatType.ConstantSpeed:
                GlobalStats.constantSpeed += upgradeAmount;
                break;
            case StatType.FishDifficulty:
                GlobalStats.fishDifficulty += (int)upgradeAmount;
                break;

            // === PLOPS ===
            case StatType.MinRadius:
                GlobalStats.minRadius += upgradeAmount;
                break;
            case StatType.MaxRadius:
                GlobalStats.maxRadius += upgradeAmount;
                break;
            case StatType.SpawnInterval:
                GlobalStats.spawnIntervall += upgradeAmount;
                break;
            case StatType.plopAmount:
                GlobalStats.plopAmount += (int)upgradeAmount;
                break;
        }

        Debug.Log($"{skillName} upgraded! New global value: " + GetCurrentValueString());
    }

    private string GetCurrentValueString()
    {
        return targetStat switch
        {
            // Motor
            StatType.ThrustForce => GlobalStats.thrustForce.ToString(),
            StatType.MaxSpeed => GlobalStats.maxSpeed.ToString(),
            StatType.ReverseSpeedDebuff => GlobalStats.reverseSpeedDebuff.ToString(),

            // Steering
            StatType.MaxTurnTorque => GlobalStats.maxTurnTorque.ToString(),
            StatType.TurnTorque => GlobalStats.turnTorque.ToString(),
            StatType.MinTurningRadius => GlobalStats.minTurningRadius.ToString(),
            StatType.MaxAngularVelocity => GlobalStats.maxAngularVelocity.ToString(),
            StatType.RudderTurnSpeed => GlobalStats.rudderTurnSpeed.ToString(),

            // Minigame & Plops
            StatType.ConstantSpeed => GlobalStats.constantSpeed.ToString(),
            StatType.FishDifficulty => GlobalStats.fishDifficulty.ToString(),
            StatType.MinRadius => GlobalStats.minRadius.ToString(),
            StatType.MaxRadius => GlobalStats.maxRadius.ToString(),
            StatType.SpawnInterval => GlobalStats.spawnIntervall.ToString(),
            StatType.plopAmount => GlobalStats.plopAmount.ToString(),

            _ => "Unknown"
        };
    }
}