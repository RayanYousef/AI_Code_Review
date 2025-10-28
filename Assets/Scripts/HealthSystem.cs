using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isAlive = true;

    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public GameObject damageEffectPrefab;

    public Animator animator;
    public ParticleSystem bloodEffect;
    public SpriteRenderer spriteRenderer;

    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip deathSound;
    public AudioClip healSound;

    public bool isEnemy = false;
    public float attackRange = 5f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    public bool isPlayer = false;
    public KeyCode healKey = KeyCode.H;
    public float healAmount = 20f;

    public float defense = 5f;
    public float damageMultiplier = 1f;
    public bool isInvulnerable = false;
    public float invulnerabilityDuration = 1f;

    public float regenRate = 1f;
    public bool autoRegen = true;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        if (healthBar == null) healthBar = GetComponentInChildren<Slider>();
        if (healthText == null) healthText = GetComponentInChildren<TextMeshProUGUI>();
        if (animator == null) animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (isEnemy)
        {
            StartCoroutine(EnemyAILoop());
        }
    }

    void Update()
    {
        if (isPlayer)
        {
            HandlePlayerInput();
        }

        if (autoRegen && currentHealth < maxHealth)
        {
            RegenerateHealth();
        }

        if (isEnemy)
        {
            UpdateEnemyAI();
        }

        UpdateUI();
    }

    public virtual void TakeDamage(float damage)
    {
        if (isInvulnerable || !isAlive) return;

        float actualDamage = Mathf.Max(0, damage - defense) * damageMultiplier;

        currentHealth -= actualDamage;

        if (bloodEffect != null)
        {
            bloodEffect.Play();
        }

        if (animator != null)
        {
            animator.SetTrigger("Hurt");
        }

        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        if (spriteRenderer != null)
        {
            StartCoroutine(DamageFlash());
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(InvulnerabilityCoroutine());
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (audioSource != null && healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }

        if (animator != null)
        {
            animator.SetTrigger("Heal");
        }

        UpdateUI();
    }

    private void Die()
    {
        isAlive = false;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }

        if (isPlayer)
        {
            HandlePlayerDeath();
        }
        else if (isEnemy)
        {
            HandleEnemyDeath();
        }
    }

    private void HandlePlayerInput()
    {
        if (Input.GetKeyDown(healKey))
        {
            Heal(healAmount);
        }
    }

    private void UpdateEnemyAI()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
            {
                Attack(player);
            }
        }
    }

    private void Attack(GameObject target)
    {
        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);
        }
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("Player died! Game Over!");
        Time.timeScale = 0; // Freeze game
    }

    private void HandleEnemyDeath()
    {
        DropLoot();

        Destroy(gameObject, 2f); // Delay for death animation
    }

    private void DropLoot()
    {
        int lootType = Random.Range(0, 3);
        switch (lootType)
        {
            case 0:
                Debug.Log("Dropped health potion");
                break;
            case 1:
                Debug.Log("Dropped coin");
                break;
            case 2:
                Debug.Log("Dropped weapon upgrade");
                break;
        }
    }

    private void RegenerateHealth()
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + regenRate * Time.deltaTime);
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
        }
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    private IEnumerator EnemyAILoop()
    {
        while (isAlive)
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0).normalized;
            transform.position += randomDirection * 2f * Time.deltaTime;

            yield return new WaitForSeconds(1f);
        }
    }
}

public class AdvancedHealthSystem : HealthSystem
{
    public float shield = 50f;
    public float maxShield = 50f;

    public override void TakeDamage(float damage)
    {
        if (shield > 0)
        {
            float shieldDamage = Mathf.Min(shield, damage);
            shield -= shieldDamage;
            damage -= shieldDamage;
        }

        if (damage > 0)
        {
            currentHealth -= damage;
        }
    }
}