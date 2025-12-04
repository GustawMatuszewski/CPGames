using UnityEngine;

public class StatsController : MonoBehaviour
{
    public float currentCalories;
    public float baseNeededCalories;

    [Header("References")]
    public KCC playerController;

    [Header("States and Stats")]
    public KCC.State currentState;

    public float maxStamina = 100.0f;
    public float maxTiredness = 100.0f; 

    public float currentStamina;
    public float currentTiredness;

    [Header("Energy Costs (kcal/sec)")]
    public float idleCost;
    public float walkCost;
    public float runCost;
    public float sprintCost;
    public float crouchCost;
    public float climbCost;
    public float passiveBurnRate;

    [Header("Recovery/Scaling")]
    public float sleepRecoveryRate; 
    public float staminaRecoveryRate; 

    public float calorieScaleFactor = 0.0001f; 

    void Start()
    {
        currentStamina = maxStamina;
        currentTiredness = 0f;
    }

    void FixedUpdate()
    {
        currentState = playerController.state;
        
        float calorieDeficit = baseNeededCalories - currentCalories;
        
        float calorieEffect = calorieDeficit * calorieScaleFactor * Time.fixedDeltaTime; 

        float staminaChange = 0f;
        float energyCostKcal = 0f;

        energyCostKcal += passiveBurnRate; 

        switch (currentState)
        {
            case KCC.State.Idle:
                // Recovery is buffed by having a calorie surplus
                staminaChange = staminaRecoveryRate * (1f - Mathf.Clamp(calorieEffect, -0.5f, 0.5f)); 
                energyCostKcal += idleCost;
                break;
            case KCC.State.Walk:
                staminaChange = -walkCost * Time.fixedDeltaTime;
                energyCostKcal += walkCost;
                break;
            case KCC.State.Run:
                staminaChange = -runCost * Time.fixedDeltaTime;
                energyCostKcal += runCost;
                break;
            case KCC.State.Sprint:
                staminaChange = -sprintCost * Time.fixedDeltaTime;
                energyCostKcal += sprintCost;
                break;
            case KCC.State.Crouch:
                staminaChange = -crouchCost * Time.fixedDeltaTime;
                energyCostKcal += crouchCost;
                break;
            case KCC.State.Climbing:
                staminaChange = -climbCost * Time.fixedDeltaTime;
                energyCostKcal += climbCost;
                break;
            default:
                staminaChange = staminaRecoveryRate * Time.fixedDeltaTime;
                energyCostKcal += idleCost;
                break;
        }

        float tirednessMultiplier = 1f + (currentTiredness / maxTiredness);
        if (staminaChange < 0)
        {
            currentStamina += staminaChange * tirednessMultiplier;
        }
        else
        {
            currentStamina += staminaChange;
        }

        currentCalories -= energyCostKcal * Time.fixedDeltaTime; 

        float baseTirednessRate = 0.05f; // Constant drain for being awake
        float deficitTirednessIncrease = Mathf.Max(0, calorieDeficit) * calorieScaleFactor * 100f; // Scale deficit into tiredness gain
        
        currentTiredness += (baseTirednessRate + deficitTirednessIncrease) * Time.fixedDeltaTime;

        //Clamping
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        currentTiredness = Mathf.Clamp(currentTiredness, 0f, maxTiredness);
    }

    public void Sleep()
    {
        // Sleeping reduces tiredness significantly
        currentTiredness -= sleepRecoveryRate * Time.deltaTime;
    }
    
    // Call this function when the player eats food
    public void ConsumeCalories(float amount)
    {
        currentCalories += amount;
    }
}