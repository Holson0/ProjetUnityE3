using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Component")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private MoveBehaviour playerMovementScript;

    [Header("Health")]
    
    [SerializeField]
    private float maxHealth = 100f;
    public float currentHealth;

    [SerializeField]
    private Image healthBarFill;

    [SerializeField]
    private float healthDecreaseRateForHungerAndThirst;

    public float currentArmorPoints;

    [HideInInspector]
    public bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, bool overTime = false)
    {
        if(overTime)
        {
            currentHealth -= damage * Time.deltaTime;
        } else
        {
            currentHealth -= damage * (1 - (currentArmorPoints / 100));
        }

        if(currentHealth <= 0 && !isDead)
        {
            Die();
        }
        
        UpdateHealthBarFill();
    }

    private void Die()
    {
        Debug.Log("Player died !");
        isDead = true;
        playerMovementScript.canMove = false;

        animator.SetTrigger("Die");
    }

    public void ConsumeItem(float health, float hunger, float thirst)
    {
        currentHealth += health;

        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        UpdateHealthBarFill();
    }

    public void UpdateHealthBarFill()
    {
        healthBarFill.fillAmount = currentHealth / maxHealth;
    }
}
