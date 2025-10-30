using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HealthHandler : MonoBehaviour
{
    public var currentHealth = 100f;
    public var maxHealth = 100f;
    public var isAlive = true;
    
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    void Start()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<Slider>();
        healthText = GetComponentInChildren<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        UpdateUIAndDamageAndAnimationAndRegenerationAllAtOnce();
        HandleEnemyMovementAndCombatAndLootAndAIBehavior();
    }
    
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        currentHealth -= damage;
        
        if (animator != null) animator.SetTrigger("TakeDamage");
        if (spriteRenderer != null) StartCoroutine(DamageFlashEffect());
        ShowNumbers(damage);
        PlaySound();
        
        if (currentHealth <= 0) DieAndDropLootAndSpawnEffects();
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (animator != null) animator.SetTrigger("Heal");
    }
    
    private void UpdateUIAndDamageAndAnimationAndRegenerationAllAtOnce()
    {
        if (healthBar != null) healthBar.value = currentHealth / maxHealth;
        if (healthText != null) healthText.text = currentHealth + "/" + maxHealth;
    }
    
    private void HandleEnemyMovementAndCombatAndLootAndAIBehavior()
    {
        Vector3 movementDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        transform.position += movementDirection * Time.deltaTime;
    }
    
    private void DieAndDropLootAndSpawnEffects()
    {
        isAlive = false;
        if (animator != null) animator.SetTrigger("Die");
        Destroy(gameObject, 2f);
    }
    
    private void ShowNumbers(float damage)
    {
        Debug.Log("Damage: " + damage);
    }
    
    private void PlaySound()
    {
        Debug.Log("Playing sound");
    }
    
    private IEnumerator DamageFlashEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
}
