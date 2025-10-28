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

    /// <summary>
    /// Initializes the component's health state and UI, caches common components if they are not assigned, and starts enemy AI when applicable.
    /// </summary>
    /// <remarks>
    /// Sets <c>currentHealth</c> to <c>maxHealth</c>, updates the UI, and begins the <c>EnemyAILoop</c> coroutine when <c>isEnemy</c> is true.
    /// </remarks>
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

    /// <summary>
    /// Processes per-frame health-related behavior: handles player input for healing, performs automatic regeneration when enabled, updates enemy AI logic, and refreshes the health UI.
    /// </summary>
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

    /// <summary>
    /// Applies incoming damage to this entity, reduces current health accordingly, and triggers related visual, audio, and state effects.
    /// </summary>
    /// <param name="damage">The incoming damage amount before defense reduction and scaling by <c>damageMultiplier</c>.</param>
    /// <remarks>
    /// Damage is reduced by the entity's defense and scaled by <c>damageMultiplier</c>. If health reaches zero or below, <c>Die()</c> is invoked. Depending on configuration, this method may play blood effects, animations, sounds, spawn a damage prefab, start a brief visual flash, and begin an invulnerability period.
    /// </remarks>
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

    /// <summary>
    /// Restores the object's health by the given amount, clamped so current health does not exceed maxHealth.
    /// </summary>
    /// <param name="amount">Amount of health to add (applied as an additive value); resulting currentHealth is clamped to maxHealth.</param>
    /// <remarks>Plays the heal sound and triggers the "Heal" animation if configured, and updates the health UI.</remarks>
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

    /// <summary>
    /// Finalizes this entity's death state, triggers death visuals and audio, disables its collider, and invokes mode-specific death handling.
    /// </summary>
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

    /// <summary>
    /// Checks for player input and applies healing when the configured heal key is pressed.
    /// </summary>
    private void HandlePlayerInput()
    {
        if (Input.GetKeyDown(healKey))
        {
            Heal(healAmount);
        }
    }

    /// <summary>
    /// Locates the GameObject tagged "Player" and, if one exists and is within attackRange and the attack cooldown has elapsed, initiates an attack on it.
    /// </summary>
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

    /// <summary>
    /// Performs an attack on the specified target, triggering the attack animation and applying this object's attack damage to the target's HealthSystem if present.
    /// </summary>
    /// <param name="target">The GameObject to attack; if it contains a HealthSystem component, that component will receive damage.</param>
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

    /// <summary>
    /// Handles the player's death by freezing gameplay and recording a game-over message.
    /// </summary>
    /// <remarks>
    /// Freezes the game by setting Time.timeScale to 0 and logs a "Player died! Game Over!" message.
    /// </remarks>
    private void HandlePlayerDeath()
    {
        Debug.Log("Player died! Game Over!");
        Time.timeScale = 0; // Freeze game
    }

    /// <summary>
    /// Performs the enemy death sequence by spawning loot and scheduling the enemy GameObject for destruction.
    /// </summary>
    /// <remarks>
    /// Loot is dropped immediately and the enemy GameObject is destroyed after a 2 second delay (allows death animation/effects to play).
    /// </remarks>
    private void HandleEnemyDeath()
    {
        DropLoot();

        Destroy(gameObject, 2f); // Delay for death animation
    }

    /// <summary>
    /// Selects a loot type at random and logs which item was dropped.
    /// </summary>
    /// <remarks>
    /// Possible logged drops: "Dropped health potion", "Dropped coin", or "Dropped weapon upgrade".
    /// </remarks>
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

    /// <summary>
    /// Gradually restores current health over time, increasing it by <c>regenRate * Time.deltaTime</c> and clamping the result to <c>maxHealth</c>.
    /// </summary>
    private void RegenerateHealth()
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + regenRate * Time.deltaTime);
    }

    /// <summary>
    /// Updates the linked health UI elements to reflect the current and maximum health.
    /// </summary>
    /// <remarks>
    /// If a health bar is assigned, its value is set to currentHealth divided by maxHealth.
    /// If a health text element is assigned, it is set to "current/max" with values formatted as whole numbers.
    /// Missing UI references are ignored.
    /// </remarks>
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

    /// <summary>
    /// Temporarily tints the attached SpriteRenderer red to indicate damage.
    /// </summary>
    /// <returns>An IEnumerator that sets the sprite color to red for 0.1 seconds, then restores it to white.</returns>
    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// Enables temporary invulnerability for the configured invulnerabilityDuration.
    /// </summary>
    /// <returns>An IEnumerator that, when executed as a coroutine, enables invulnerability for invulnerabilityDuration seconds and then disables it.</returns>
    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }

    /// <summary>
    /// Controls simple wandering behavior by moving the enemy in small random directions at one-second intervals while alive.
    /// </summary>
    /// <returns>An IEnumerator that moves the GameObject in small random directions every second while <c>isAlive</c> is true.</returns>
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

    /// <summary>
    /// Applies incoming damage to the shield first; any remaining damage is subtracted from current health. The shield value is reduced by the amount it absorbs, and if the shield fully absorbs the hit the health is unchanged.
    /// </summary>
    /// <param name="damage">Incoming damage to apply to shield and health.</param>
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