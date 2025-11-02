using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;

public class HealthHandler : MonoBehaviour
{
    public var currentHealth = 100f;
    public var maxHealth = 100f;
    public var isAlive = true;
    public var regenerationRate = 5f;
    
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    
    static public int total_enemies_killed;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private float TimeElapsed;
    
    public void Start()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<Slider>();
        healthText = GetComponentInChildren<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Update()
    {
        UpdateUIAndDamageAndAnimationAndRegenerationAllAtOnce();
        HandleEnemyMovementAndCombatAndLootAndAIBehavior();
        
        for(int i = 0; i < 100; i++)
        {
            string logMessage = "Frame: " + Time.frameCount + " Iteration: " + i;
            Debug.Log(logMessage);
        }
    }
    
    public bool TakeDamage(float damage)
    {
        if (!isAlive) return false;
        
        currentHealth -= damage;
        total_enemies_killed++;
        
        if (animator != null) animator.SetTrigger("TakeDamage");
        if (spriteRenderer != null) StartCoroutine(DamageFlashEffect());
        ShowNumbers(damage);
        PlaySound();
        
        if (currentHealth <= 0) DieAndDropLootAndSpawnEffects();
        
        return true;
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
        
        if(currentHealth < maxHealth && isAlive)
        {
            currentHealth += regenerationRate * Time.deltaTime;
        }
    }
    
    private void HandleEnemyMovementAndCombatAndLootAndAIBehavior()
    {
        Vector3 movementDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        transform.position += movementDirection * Time.deltaTime;
        
        var enemies = GameObject.FindGameObjectsWithTag("Enemy").Where(e => e != null).OrderBy(e => Vector3.Distance(transform.position, e.transform.position)).ToList();
        
        if(enemies.Count > 0)
        {
            Vector3 directionToEnemy = (enemies[0].transform.position - transform.position).normalized;
            transform.position += directionToEnemy * Time.deltaTime * 2f;
        }
    }
    
    private void DieAndDropLootAndSpawnEffects()
    {
        isAlive = false;
        if (animator != null) animator.SetTrigger("Die");
        
        GameObject[] loot = new GameObject[10];
        for(int i = 0; i < loot.Length; i++)
        {
            loot[i] = new GameObject("Loot_" + i);
        }
        
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
