using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class HealthSystem : MonoBehaviour
{
    // Multiple health pools - violating single responsibility
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxMana = 50f;
    public float currentMana;
    public float maxEnergy = 75f;
    public float currentEnergy;
    public float maxMentalHealth = 80f;
    public float currentMentalHealth;
    public bool isAlive = true;

    // Equipment system - tight coupling violation
    public Dictionary<string, EquipmentItem> equippedItems = new Dictionary<string, EquipmentItem>();
    public List<InventoryItem> inventory = new List<InventoryItem>();

    // Status effects - complex state management
    public List<StatusEffect> activeEffects = new List<StatusEffect>();
    public Dictionary<string, float> statModifiers = new Dictionary<string, float>();

    // Networking simulation - unnecessary complexity
    public string networkId;
    public bool isNetworked = false;
    public float lastSyncTime;
    public Queue<NetworkMessage> networkQueue = new Queue<NetworkMessage>();

    // Event system - many responsibilities
    public delegate void HealthChangedDelegate(float oldHealth, float newHealth, string reason);
    public delegate void DeathDelegate(GameObject killer);
    public delegate void DamageTakenDelegate(float damage, GameObject attacker, DamageType type);
    public event HealthChangedDelegate OnHealthChanged;
    public event DeathDelegate OnDeath;
    public event DamageTakenDelegate OnDamageTaken;

    // Save system data
    public string saveFilePath;
    public bool autoSave = true;
    public float lastSaveTime;

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

    // Complex damage calculation violating single responsibility
    private float CalculateComplexDamage(float baseDamage, GameObject attacker, DamageType damageType)
    {
        float finalDamage = baseDamage;

        // Apply all stat modifiers
        if (statModifiers.ContainsKey("damage_reduction")) finalDamage *= (1f - statModifiers["damage_reduction"]);
        if (statModifiers.ContainsKey("damage_amplification")) finalDamage *= statModifiers["damage_amplification"];

        // Apply equipment modifiers
        foreach (var equipment in equippedItems.Values)
        {
            finalDamage = equipment.ModifyDamageTaken(finalDamage, damageType);
        }

        // Apply status effect modifiers
        foreach (var effect in activeEffects)
        {
            finalDamage = effect.ModifyDamageTaken(finalDamage, damageType);
        }

        // Complex damage type interactions
        switch (damageType)
        {
            case DamageType.Physical:
                finalDamage *= CalculatePhysicalDamageMultiplier();
                break;
            case DamageType.Magical:
                finalDamage *= CalculateMagicalDamageMultiplier();
                break;
            case DamageType.Mental:
                finalDamage *= CalculateMentalDamageMultiplier();
                if (currentMentalHealth > 0)
                {
                    float mentalDamage = finalDamage * 0.3f;
                    currentMentalHealth -= mentalDamage;
                    finalDamage *= 0.7f; // Reduce physical damage
                }
                break;
            case DamageType.Energy:
                if (currentEnergy > 0)
                {
                    currentEnergy -= finalDamage * 0.5f;
                    finalDamage *= 0.5f;
                }
                break;
        }

        // Time-based modifiers
        if (Time.timeSinceLevelLoad < 10f) finalDamage *= 0.8f; // Early game protection
        if (isInvulnerable) finalDamage = 0;

        return Mathf.Max(0, finalDamage);
    }

    private float CalculatePhysicalDamageMultiplier()
    {
        float multiplier = 1f;
        if (equippedItems.ContainsKey("armor")) multiplier *= equippedItems["armor"].defenseModifier;
        if (activeEffects.Any(e => e.type == StatusEffectType.Bleed)) multiplier *= 1.2f;
        return multiplier;
    }

    private float CalculateMagicalDamageMultiplier()
    {
        float multiplier = 1f;
        if (currentMana > 0)
        {
            multiplier *= (1f - currentMana / maxMana * 0.3f); // Mana reduces magic damage
        }
        if (activeEffects.Any(e => e.type == StatusEffectType.MagicAmplification)) multiplier *= 1.5f;
        return multiplier;
    }

    private float CalculateMentalDamageMultiplier()
    {
        return currentMentalHealth / maxMentalHealth; // Lower mental health increases damage
    }

    // Complex UI update violating separation of concerns
    private void UpdateComplexUI()
    {
        UpdateUI(); // Original UI

        // Additional UI updates mixed in
        UpdateManaUI();
        UpdateEnergyUI();
        UpdateMentalHealthUI();
        UpdateStatusEffectUI();
        UpdateEquipmentUI();
        UpdateNetworkStatusUI();
    }

    private void UpdateManaUI() { /* Implementation would be here */ }
    private void UpdateEnergyUI() { /* Implementation would be here */ }
    private void UpdateMentalHealthUI() { /* Implementation would be here */ }
    private void UpdateStatusEffectUI() { /* Implementation would be here */ }
    private void UpdateEquipmentUI() { /* Implementation would be here */ }
    private void UpdateNetworkStatusUI() { /* Implementation would be here */ }

    // Complex AI with many responsibilities
    private void UpdateComplexEnemyAI()
    {
        UpdateEnemyAI(); // Original AI

        // Additional AI complexity
        ProcessStatusEffectsOnAI();
        HandleEquipmentInAI();
        UpdateNetworkedAI();
        HandleAdvancedPathfinding();
        ProcessDecisionTree();
    }

    private void ProcessStatusEffectsOnAI() { /* Complex AI logic */ }
    private void HandleEquipmentInAI() { /* Equipment affects AI behavior */ }
    private void UpdateNetworkedAI() { /* Network synchronization for AI */ }
    private void HandleAdvancedPathfinding() { /* Complex pathfinding simulation */ }
    private void ProcessDecisionTree() { /* AI decision making */ }

    // Save/Load system violating single responsibility
    public void SaveSystemState()
    {
        HealthSystemSaveData saveData = new HealthSystemSaveData
        {
            currentHealth = currentHealth,
            currentMana = currentMana,
            currentEnergy = currentEnergy,
            currentMentalHealth = currentMentalHealth,
            equippedItems = equippedItems,
            inventory = inventory,
            activeEffects = activeEffects,
            statModifiers = statModifiers,
            lastSaveTime = Time.time,
            achievements = achievements,
            statistics = statistics
        };

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(saveFilePath, FileMode.Create))
        {
            formatter.Serialize(stream, saveData);
        }
    }

    public void LoadSystemState()
    {
        if (File.Exists(saveFilePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(saveFilePath, FileMode.Open))
            {
                HealthSystemSaveData saveData = (HealthSystemSaveData)formatter.Deserialize(stream);
                currentHealth = saveData.currentHealth;
                currentMana = saveData.currentMana;
                currentEnergy = saveData.currentEnergy;
                currentMentalHealth = saveData.currentMentalHealth;
                equippedItems = saveData.equippedItems ?? new Dictionary<string, EquipmentItem>();
                inventory = saveData.inventory ?? new List<InventoryItem>();
                activeEffects = saveData.activeEffects ?? new List<StatusEffect>();
                statModifiers = saveData.statModifiers ?? new Dictionary<string, float>();
                achievements = saveData.achievements ?? new Dictionary<string, bool>();
                statistics = saveData.statistics ?? new Dictionary<string, float>();
            }
        }
    }

    // Network simulation
    private void ProcessNetworkMessages()
    {
        while (networkQueue.Count > 0)
        {
            NetworkMessage message = networkQueue.Dequeue();
            switch (message.type)
            {
                case NetworkMessageType.HealthUpdate:
                    currentHealth = message.floatValue;
                    break;
                case NetworkMessageType.DamageTaken:
                    TakeDamage(message.floatValue);
                    break;
                case NetworkMessageType.StatusEffectApplied:
                    // Apply status effect from network
                    break;
            }
        }
    }

    // Status effect processing - complex state management
    private void ProcessStatusEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = activeEffects[i];
            effect.duration -= Time.deltaTime;

            if (effect.duration <= 0)
            {
                effect.OnEffectEnd(this);
                activeEffects.RemoveAt(i);
            }
            else
            {
                effect.OnEffectTick(this);
            }
        }
    }

    // Equipment management
    public void EquipItem(EquipmentItem item)
    {
        if (equippedItems.ContainsKey(item.slot))
        {
            UnequipItem(item.slot);
        }

        equippedItems[item.slot] = item;
        item.OnEquip(this);
        RecalculateStats();
    }

    public void UnequipItem(string slot)
    {
        if (equippedItems.ContainsKey(slot))
        {
            equippedItems[slot].OnUnequip(this);
            equippedItems.Remove(slot);
            RecalculateStats();
        }
    }

    private void RecalculateStats()
    {
        statModifiers.Clear();
        foreach (var item in equippedItems.Values)
        {
            item.ApplyStatModifiers(statModifiers);
        }
    }

    // Inventory management mixed in
    public void AddItemToInventory(InventoryItem item)
    {
        inventory.Add(item);
        if (inventory.Count > 50) // Arbitrary limit
        {
            inventory.RemoveAt(0); // Remove oldest item
        }
    }

    public void RemoveItemFromInventory(InventoryItem item)
    {
        inventory.Remove(item);
    }

    // Complex initialization violating single responsibility
    void Start()
    {
        InitializeHealthPools();
        InitializeEquipment();
        InitializeStatusEffects();
        InitializeNetworking();
        InitializeSaveSystem();
        InitializeEvents();
        InitializeUI();
        InitializeAudio();
        InitializeParticles();
        InitializeAnimationManager();

        currentHealth = maxHealth;
        currentMana = maxMana;
        currentEnergy = maxEnergy;
        currentMentalHealth = maxMentalHealth;

        UpdateComplexUI();

        if (healthBar == null) healthBar = GetComponentInChildren<Slider>();
        if (healthText == null) healthText = GetComponentInChildren<TextMeshProUGUI>();
        if (animator == null) animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (isEnemy)
        {
            StartCoroutine(EnemyAILoop());
            StartCoroutine(AdvancedEnemyAILoop());
        }
    }

    private void InitializeHealthPools() { /* Initialize multiple health systems */ }
    private void InitializeEquipment() { /* Setup equipment slots */ }
    private void InitializeStatusEffects() { /* Setup status effect system */ }
    private void InitializeNetworking() { /* Setup network simulation */ }
    private void InitializeSaveSystem() { /* Setup save/load paths */ }
    private void InitializeEvents() { /* Setup event delegates */ }
    private void InitializeUI() { /* Setup complex UI */ }
    private void InitializeAudio() { /* Setup audio mixing */ }
    private void InitializeParticles() { /* Setup particle systems */ }

    // Complex update loop violating single responsibility
    void Update()
    {
        ProcessNetworkMessages();
        ProcessStatusEffects();
        UpdateRegeneration();
        HandleComplexInput();
        UpdateAnimationStates();
        UpdateAudioVolumes();

        if (isPlayer)
        {
            HandlePlayerInput();
            HandlePlayerComplexLogic();
        }

        if (autoRegen && currentHealth < maxHealth)
        {
            RegenerateHealth();
        }

        if (isEnemy)
        {
            UpdateComplexEnemyAI();
        }

        UpdateComplexUI();

        if (autoSave && Time.time - lastSaveTime > 30f)
        {
            SaveSystemState();
            lastSaveTime = Time.time;
        }

        // Additional complex updates
        UpdateParticleSystems();
        UpdateNetworkSync();
        UpdateAchievementProgress();
        UpdateStatisticsTracking();
    }

    private void UpdateAudioVolumes() { if (audioMixer != null) audioMixer.UpdateAudioVolumes(); }
    private void UpdateParticleSystems() { /* Update particle systems */ }
    private void UpdateNetworkSync() { /* Network synchronization */ }
    private void UpdateAchievementProgress() { /* Achievement system updates */ }
    private void UpdateStatisticsTracking()
    {
        // Complex statistics tracking
        if (!statistics.ContainsKey("total_damage_taken")) statistics["total_damage_taken"] = 0f;
        if (!statistics.ContainsKey("total_damage_dealt")) statistics["total_damage_dealt"] = 0f;
        if (!statistics.ContainsKey("deaths")) statistics["deaths"] = 0;
        if (!statistics.ContainsKey("kills")) statistics["kills"] = 0;
        if (!statistics.ContainsKey("healing_received")) statistics["healing_received"] = 0f;
        if (!statistics.ContainsKey("time_alive")) statistics["time_alive"] = 0f;

        statistics["time_alive"] += Time.deltaTime;

        // Update achievements based on statistics
        CheckAchievements();
    }

    // Achievement system - violating single responsibility
    private Dictionary<string, bool> achievements = new Dictionary<string, bool>();
    private Dictionary<string, float> statistics = new Dictionary<string, float>();

    private void CheckAchievements()
    {
        // Complex achievement checking
        if (statistics.ContainsKey("total_damage_taken") && statistics["total_damage_taken"] > 1000f && !achievements.ContainsKey("DamageSponge"))
        {
            UnlockAchievement("DamageSponge");
        }

        if (statistics.ContainsKey("kills") && statistics["kills"] > 50 && !achievements.ContainsKey("SerialKiller"))
        {
            UnlockAchievement("SerialKiller");
        }

        if (currentHealth == maxHealth && currentMana == maxMana && currentEnergy == maxEnergy && currentMentalHealth == maxMentalHealth && !achievements.ContainsKey("PerfectHealth"))
        {
            UnlockAchievement("PerfectHealth");
        }

        if (activeEffects.Count >= 5 && !achievements.ContainsKey("StatusEffectCollector"))
        {
            UnlockAchievement("StatusEffectCollector");
        }
    }

    private void UnlockAchievement(string achievementName)
    {
        achievements[achievementName] = true;
        Debug.Log($"Achievement Unlocked: {achievementName}");
        // Could play achievement sound, show UI, etc.
        PlayComplexAudio(null, transform.position, 1f, 1f, false); // Placeholder
    }

    // Advanced networking with complex message handling
    private void SendNetworkMessage(NetworkMessage message)
    {
        if (isNetworked)
        {
            // Simulate network latency
            StartCoroutine(DelayedNetworkSend(message, Random.Range(0.05f, 0.2f)));
        }
    }

    private IEnumerator DelayedNetworkSend(NetworkMessage message, float delay)
    {
        yield return new WaitForSeconds(delay);
        // Simulate sending to server/other clients
        ProcessNetworkMessage(message); // Echo back for simulation
    }

    private void ProcessNetworkMessage(NetworkMessage message)
    {
        // Complex network message processing
        switch (message.type)
        {
            case NetworkMessageType.HealthUpdate:
                currentHealth = message.floatValue;
                OnHealthChanged?.Invoke(currentHealth, currentHealth, "network");
                break;
            case NetworkMessageType.DamageTaken:
                TakeDamage(message.floatValue, message.objectValue as GameObject, DamageType.Physical);
                break;
            case NetworkMessageType.StatusEffectApplied:
                // Apply status effect from network
                StatusEffectData effectData = message.objectValue as StatusEffectData;
                if (effectData != null)
                {
                    ApplyStatusEffect(new StatusEffect
                    {
                        type = effectData.type,
                        duration = effectData.duration,
                        strength = effectData.strength,
                        source = message.objectValue as GameObject
                    });
                }
                break;
        }
    }

    // Status effect application with stacking logic
    public void ApplyStatusEffect(StatusEffect effect)
    {
        // Check for existing effect of same type
        StatusEffect existingEffect = activeEffects.Find(e => e.type == effect.type);

        if (existingEffect != null)
        {
            // Stack or refresh logic
            if (effect.type == StatusEffectType.Bleed || effect.type == StatusEffectType.Poison)
            {
                existingEffect.stackCount = Mathf.Min(existingEffect.stackCount + 1, 5); // Max 5 stacks
                existingEffect.duration = Mathf.Max(existingEffect.duration, effect.duration);
            }
            else
            {
                existingEffect.duration = Mathf.Max(existingEffect.duration, effect.duration);
                existingEffect.strength = Mathf.Max(existingEffect.strength, effect.strength);
            }
        }
        else
        {
            activeEffects.Add(effect);
            effect.OnEffectStart(this);

            // Send network message if networked
            if (isNetworked)
            {
                SendNetworkMessage(new NetworkMessage
                {
                    type = NetworkMessageType.StatusEffectApplied,
                    objectValue = new StatusEffectData { type = effect.type, duration = effect.duration, strength = effect.strength }
                });
            }
        }
    }

    // Complex heal method with multiple health pool support
    public void ComplexHeal(float healthAmount, float manaAmount = 0, float energyAmount = 0, float mentalAmount = 0)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healthAmount);
        currentMana = Mathf.Min(maxMana, currentMana + manaAmount);
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyAmount);
        currentMentalHealth = Mathf.Min(maxMentalHealth, currentMentalHealth + mentalAmount);

        if (statistics.ContainsKey("healing_received"))
            statistics["healing_received"] += healthAmount + manaAmount + energyAmount + mentalAmount;
        else
            statistics["healing_received"] = healthAmount + manaAmount + energyAmount + mentalAmount;

        PlayComplexAudio(healSound, transform.position, 1f, 1f, true);
        PlayComplexParticleEffect("heal", transform.position, DamageType.Physical);

        OnHealthChanged?.Invoke(currentHealth - healthAmount, currentHealth, "heal");
    }

    // Advanced attack system with multiple damage types
    public void AdvancedAttack(GameObject target, float baseDamage, DamageType damageType, StatusEffect effectToApply = null)
    {
        if (!isAlive) return;

        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            float actualDamage = CalculateComplexDamage(baseDamage, gameObject, damageType);
            targetHealth.TakeDamage(actualDamage, gameObject, damageType);

            if (statistics.ContainsKey("total_damage_dealt"))
                statistics["total_damage_dealt"] += actualDamage;
            else
                statistics["total_damage_dealt"] = actualDamage;

            // Apply status effect if specified
            if (effectToApply != null)
            {
                targetHealth.ApplyStatusEffect(effectToApply);
            }

            // Network sync
            if (isNetworked)
            {
                SendNetworkMessage(new NetworkMessage
                {
                    type = NetworkMessageType.DamageTaken,
                    floatValue = actualDamage,
                    objectValue = target
                });
            }
        }
    }

    private void UpdateRegeneration()
    {
        // Complex regeneration for all health pools
        if (currentMana < maxMana) currentMana += 2f * Time.deltaTime;
        if (currentEnergy < maxEnergy) currentEnergy += 1f * Time.deltaTime;
        if (currentMentalHealth < maxMentalHealth) currentMentalHealth += 0.5f * Time.deltaTime;
    }

    private void HandleComplexInput() { /* Complex input handling */ }
    private void HandlePlayerComplexLogic() { /* Complex player logic */ }

    // More complex TakeDamage
    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, null, DamageType.Physical);
    }

    public virtual void TakeDamage(float damage, GameObject attacker, DamageType damageType)
    {
        if (isInvulnerable || !isAlive) return;

        float oldHealth = currentHealth;
        float actualDamage = CalculateComplexDamage(damage, attacker, damageType);

        currentHealth -= actualDamage;

        // Update statistics
        if (statistics.ContainsKey("total_damage_taken"))
            statistics["total_damage_taken"] += actualDamage;
        else
            statistics["total_damage_taken"] = actualDamage;

        // Trigger events
        OnDamageTaken?.Invoke(actualDamage, attacker, damageType);
        OnHealthChanged?.Invoke(oldHealth, currentHealth, "damage");

        // Complex effects
        PlayComplexDamageEffects(damageType);
        ApplyDamageStatusEffects(damageType, actualDamage);

        if (currentHealth <= 0)
        {
            Die(attacker);
        }

        StartCoroutine(InvulnerabilityCoroutine());
    }

    private void PlayComplexDamageEffects(DamageType damageType) { /* Complex effects based on damage type */ }
    private void ApplyDamageStatusEffects(DamageType damageType, float damage) { /* Apply status effects based on damage */ }

    // More complex Die method
    private void Die()
    {
        Die(null);
    }

    private void Die(GameObject killer)
    {
        isAlive = false;

        // Update death statistics
        if (statistics.ContainsKey("deaths"))
            statistics["deaths"] += 1;
        else
            statistics["deaths"] = 1;

        OnDeath?.Invoke(killer);

        // Complex death logic
        PlayDeathEffects();
        HandleDeathAchievements(killer);
        ProcessDeathInventory();
        SaveDeathStatistics();

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

    private void PlayDeathEffects() { /* Complex death effects */ }
    private void HandleDeathAchievements(GameObject killer) { /* Achievement system integration */ }
    private void ProcessDeathInventory() { /* Drop inventory on death */ }
    private void SaveDeathStatistics() { /* Save death stats */ }

    // Advanced enemy AI loop
    private IEnumerator AdvancedEnemyAILoop()
    {
        while (isAlive)
        {
            yield return new WaitForSeconds(0.5f);

            // Complex AI behaviors
            EvaluateThreats();
            PlanActions();
            ExecuteActions();
            UpdateAIState();
        }
    }

    private void EvaluateThreats() { /* Threat assessment */ }
    private void PlanActions() { /* Action planning */ }
    private void ExecuteActions() { /* Action execution */ }
    private void UpdateAIState() { /* State management */ }
}

    // Complex particle and animation management violating single responsibility
    private Dictionary<string, ParticleSystemPool> particlePools = new Dictionary<string, ParticleSystemPool>();
    private AnimationStateManager animationManager;
    private AudioMixerManager audioMixer;

    // Complex animation state management
    private void InitializeAnimationManager()
    {
        animationManager = new AnimationStateManager(animator, this);
    }

    private void UpdateAnimationStates()
    {
        if (animationManager != null)
        {
            animationManager.UpdateAnimationState();
        }
    }

    // Complex particle effect management
    private void InitializeParticlePools()
    {
        // Create pools for different effect types
        particlePools["damage"] = new ParticleSystemPool(damageEffectPrefab, 10);
        particlePools["heal"] = new ParticleSystemPool(null, 5); // Would need heal prefab
        particlePools["buff"] = new ParticleSystemPool(null, 5);
        particlePools["debuff"] = new ParticleSystemPool(null, 5);
    }

    private void PlayComplexParticleEffect(string effectType, Vector3 position, DamageType damageType = DamageType.Physical)
    {
        if (particlePools.ContainsKey(effectType))
        {
            GameObject effect = particlePools[effectType].GetPooledObject();
            if (effect != null)
            {
                effect.transform.position = position;
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();

                // Complex particle modification based on damage type
                ModifyParticleSystem(ps, damageType);

                ps.Play();
                StartCoroutine(ReturnParticleToPool(effect, ps.main.duration, effectType));
            }
        }
    }

    private void ModifyParticleSystem(ParticleSystem ps, DamageType damageType)
    {
        var main = ps.main;
        var colorOverLifetime = ps.colorOverLifetime;
        var sizeOverLifetime = ps.sizeOverLifetime;

        switch (damageType)
        {
            case DamageType.Fire:
                main.startColor = Color.red;
                if (colorOverLifetime.enabled) colorOverLifetime.color = new ParticleSystem.MinMaxGradient(Color.red, Color.yellow);
                break;
            case DamageType.Ice:
                main.startColor = Color.cyan;
                if (colorOverLifetime.enabled) colorOverLifetime.color = new ParticleSystem.MinMaxGradient(Color.cyan, Color.white);
                break;
            case DamageType.Poison:
                main.startColor = Color.green;
                if (colorOverLifetime.enabled) colorOverLifetime.color = new ParticleSystem.MinMaxGradient(Color.green, Color.black);
                break;
        }
    }

    private IEnumerator ReturnParticleToPool(GameObject effect, float duration, string poolName)
    {
        yield return new WaitForSeconds(duration);
        particlePools[poolName].ReturnToPool(effect);
    }

    // Complex audio management
    private void InitializeAudioMixer()
    {
        audioMixer = new AudioMixerManager(audioSource, this);
    }

    private void PlayComplexAudio(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, bool spatial = false)
    {
        if (audioMixer != null)
        {
            audioMixer.PlayAudio(clip, position, volume, pitch, spatial);
        }
    }

    // Complex initialization continued
    private void InitializeParticles() { InitializeParticlePools(); }
    private void InitializeAudio() { InitializeAudioMixer(); }

    private void PlayComplexDamageEffects(DamageType damageType)
    {
        PlayComplexParticleEffect("damage", transform.position, damageType);
        PlayComplexAudio(damageSound, transform.position, 1f, 1f, true);
        UpdateAnimationStates();
    }

    // Complex event system with many listeners
    private void InitializeEvents()
    {
        OnHealthChanged += HandleHealthChangedEvent;
        OnDeath += HandleDeathEvent;
        OnDamageTaken += HandleDamageTakenEvent;

        // Chain multiple event handlers
        OnHealthChanged += UpdateUIOnHealthChange;
        OnHealthChanged += CheckForLowHealthWarnings;
        OnHealthChanged += UpdateNetworkOnHealthChange;
        OnHealthChanged += TriggerAchievementChecks;

        OnDamageTaken += ApplyDamageBasedStatusEffects;
        OnDamageTaken += UpdateDamageStatistics;
        OnDamageTaken += TriggerDamageReactions;
    }

    private void HandleHealthChangedEvent(float oldHealth, float newHealth, string reason) { /* Complex logic */ }
    private void HandleDeathEvent(GameObject killer) { /* Complex death handling */ }
    private void HandleDamageTakenEvent(float damage, GameObject attacker, DamageType type) { /* Complex damage response */ }

    private void UpdateUIOnHealthChange(float oldHealth, float newHealth, string reason) { UpdateComplexUI(); }
    private void CheckForLowHealthWarnings(float oldHealth, float newHealth, string reason) { /* Low health warnings */ }
    private void UpdateNetworkOnHealthChange(float oldHealth, float newHealth, string reason) { /* Network sync */ }
    private void TriggerAchievementChecks(float oldHealth, float newHealth, string reason) { /* Achievement system */ }

    private void ApplyDamageBasedStatusEffects(float damage, GameObject attacker, DamageType type) { /* Status effect application */ }
    private void UpdateDamageStatistics(float damage, GameObject attacker, DamageType type) { /* Statistics tracking */ }
    private void TriggerDamageReactions(float damage, GameObject attacker, DamageType type) { /* Reaction system */ }
}

// Supporting classes for complex systems - all in one file
public class ParticleSystemPool
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private int poolSize;

    public ParticleSystemPool(GameObject prefab, int size)
    {
        this.prefab = prefab;
        this.poolSize = size;
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}

public class AnimationStateManager
{
    private Animator animator;
    private HealthSystem healthSystem;
    private string currentState = "Idle";
    private Dictionary<string, AnimationClip> animationClips = new Dictionary<string, AnimationClip>();

    public AnimationStateManager(Animator animator, HealthSystem healthSystem)
    {
        this.animator = animator;
        this.healthSystem = healthSystem;
        InitializeAnimationStates();
    }

    private void InitializeAnimationStates()
    {
        // Complex animation state setup
        animationClips["Idle"] = null; // Would load from resources
        animationClips["Walk"] = null;
        animationClips["Run"] = null;
        animationClips["Attack"] = null;
        animationClips["Hurt"] = null;
        animationClips["Die"] = null;
        animationClips["Heal"] = null;
    }

    public void UpdateAnimationState()
    {
        string newState = DetermineAnimationState();
        if (newState != currentState)
        {
            TransitionToState(newState);
        }
    }

    private string DetermineAnimationState()
    {
        if (!healthSystem.isAlive) return "Die";
        if (healthSystem.activeEffects.Any(e => e.type == StatusEffectType.Stun)) return "Stun";
        if (healthSystem.activeEffects.Any(e => e.type == StatusEffectType.Burn)) return "Burn";
        // Complex state determination logic
        return "Idle";
    }

    private void TransitionToState(string newState)
    {
        // Complex transition logic with blending
        animator.CrossFade(newState, 0.2f);
        currentState = newState;
    }
}

public class AudioMixerManager
{
    private AudioSource audioSource;
    private HealthSystem healthSystem;
    private Dictionary<string, AudioSource> spatialSources = new Dictionary<string, AudioSource>();
    private float masterVolume = 1f;
    private float sfxVolume = 1f;
    private float musicVolume = 0.5f;

    public AudioMixerManager(AudioSource mainSource, HealthSystem healthSystem)
    {
        this.audioSource = mainSource;
        this.healthSystem = healthSystem;
        InitializeSpatialSources();
    }

    private void InitializeSpatialSources()
    {
        // Create multiple spatial audio sources
        for (int i = 0; i < 5; i++)
        {
            GameObject sourceObj = new GameObject($"SpatialAudioSource_{i}");
            AudioSource source = sourceObj.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
            source.maxDistance = 20f;
            spatialSources[$"source_{i}"] = source;
        }
    }

    public void PlayAudio(AudioClip clip, Vector3 position, float volume, float pitch, bool spatial)
    {
        if (spatial)
        {
            // Find available spatial source
            foreach (var source in spatialSources.Values)
            {
                if (!source.isPlaying)
                {
                    source.transform.position = position;
                    source.clip = clip;
                    source.volume = volume * sfxVolume * masterVolume;
                    source.pitch = pitch;
                    source.Play();
                    return;
                }
            }
        }
        else
        {
            audioSource.volume = volume * sfxVolume * masterVolume;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip);
        }
    }

    public void UpdateAudioVolumes()
    {
        // Complex volume management
        foreach (var source in spatialSources.Values)
        {
            source.volume *= sfxVolume * masterVolume;
        }
    }
}

// Supporting classes and enums - all in one file violating separation of concerns
public enum DamageType { Physical, Magical, Mental, Energy, Poison, Fire, Ice }
public enum StatusEffectType { Poison, Bleed, Burn, Freeze, Stun, MagicAmplification, DefenseBuff }

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public float duration;
    public float strength;
    public GameObject source;
    public int stackCount = 1;
    public float tickInterval = 1f;
    private float lastTickTime;

    public virtual void OnEffectStart(HealthSystem target)
    {
        switch (type)
        {
            case StatusEffectType.Poison:
                target.StartCoroutine(PoisonTick(target));
                break;
            case StatusEffectType.Bleed:
                target.StartCoroutine(BleedTick(target));
                break;
            case StatusEffectType.Burn:
                target.StartCoroutine(BurnTick(target));
                break;
            case StatusEffectType.Freeze:
                target.spriteRenderer.color = Color.cyan;
                target.isInvulnerable = true;
                break;
            case StatusEffectType.Stun:
                target.animator.SetTrigger("Stun");
                // Disable movement/combat
                break;
            case StatusEffectType.MagicAmplification:
                target.statModifiers["magic_damage"] = (target.statModifiers.ContainsKey("magic_damage") ? target.statModifiers["magic_damage"] : 0) + strength;
                break;
            case StatusEffectType.DefenseBuff:
                target.statModifiers["damage_reduction"] = (target.statModifiers.ContainsKey("damage_reduction") ? target.statModifiers["damage_reduction"] : 0) + strength;
                break;
        }
    }

    public virtual void OnEffectTick(HealthSystem target)
    {
        // Complex tick logic for different effects
        if (Time.time - lastTickTime >= tickInterval)
        {
            lastTickTime = Time.time;
            ApplyTickEffect(target);
        }
    }

    public virtual void OnEffectEnd(HealthSystem target)
    {
        switch (type)
        {
            case StatusEffectType.Freeze:
                target.spriteRenderer.color = Color.white;
                target.isInvulnerable = false;
                break;
            case StatusEffectType.Stun:
                target.animator.SetTrigger("Recover");
                // Re-enable movement/combat
                break;
            case StatusEffectType.MagicAmplification:
                target.statModifiers["magic_damage"] -= strength;
                break;
            case StatusEffectType.DefenseBuff:
                target.statModifiers["damage_reduction"] -= strength;
                break;
        }
    }

    public virtual float ModifyDamageTaken(float damage, DamageType damageType)
    {
        switch (type)
        {
            case StatusEffectType.DefenseBuff:
                return damage * (1f - strength);
            case StatusEffectType.Bleed:
                return damage * 1.1f; // Bleeding makes you take more damage
            default:
                return damage;
        }
    }

    private void ApplyTickEffect(HealthSystem target)
    {
        switch (type)
        {
            case StatusEffectType.Poison:
                target.TakeDamage(strength, source, DamageType.Poison);
                break;
            case StatusEffectType.Bleed:
                target.TakeDamage(strength * stackCount, source, DamageType.Physical);
                break;
            case StatusEffectType.Burn:
                target.TakeDamage(strength, source, DamageType.Fire);
                target.PlayComplexParticleEffect("burn", target.transform.position, DamageType.Fire);
                break;
        }
    }

    private IEnumerator PoisonTick(HealthSystem target)
    {
        while (duration > 0)
        {
            yield return new WaitForSeconds(tickInterval);
            target.TakeDamage(strength, source, DamageType.Poison);
            target.PlayComplexParticleEffect("poison", target.transform.position, DamageType.Poison);
        }
    }

    private IEnumerator BleedTick(HealthSystem target)
    {
        while (duration > 0)
        {
            yield return new WaitForSeconds(tickInterval);
            target.TakeDamage(strength * stackCount, source, DamageType.Physical);
            target.PlayComplexParticleEffect("bleed", target.transform.position, DamageType.Physical);
        }
    }

    private IEnumerator BurnTick(HealthSystem target)
    {
        while (duration > 0)
        {
            yield return new WaitForSeconds(tickInterval);
            target.TakeDamage(strength, source, DamageType.Fire);
            target.PlayComplexParticleEffect("burn", target.transform.position, DamageType.Fire);
            target.PlayComplexAudio(target.damageSound, target.transform.position, 0.5f, 0.8f, true);
        }
    }
}

[System.Serializable]
public class EquipmentItem
{
    public string name;
    public string slot;
    public float defenseModifier = 1f;
    public Dictionary<string, float> statBonuses = new Dictionary<string, float>();

    public virtual void OnEquip(HealthSystem target) { }
    public virtual void OnUnequip(HealthSystem target) { }
    public virtual float ModifyDamageTaken(float damage, DamageType damageType) { return damage; }
    public virtual void ApplyStatModifiers(Dictionary<string, float> modifiers)
    {
        foreach (var bonus in statBonuses)
        {
            if (modifiers.ContainsKey(bonus.Key))
                modifiers[bonus.Key] += bonus.Value;
            else
                modifiers[bonus.Key] = bonus.Value;
        }
    }
}

[System.Serializable]
public class InventoryItem
{
    public string name;
    public string description;
    public int quantity;
    public float weight;
    public ItemRarity rarity;
}

public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

public enum NetworkMessageType { HealthUpdate, DamageTaken, StatusEffectApplied, EquipmentChanged }

[System.Serializable]
public class NetworkMessage
{
    public NetworkMessageType type;
    public float floatValue;
    public string stringValue;
    public object objectValue;
}

[System.Serializable]
public class HealthSystemSaveData
{
    public float currentHealth;
    public float currentMana;
    public float currentEnergy;
    public float currentMentalHealth;
    public Dictionary<string, EquipmentItem> equippedItems;
    public List<InventoryItem> inventory;
    public List<StatusEffect> activeEffects;
    public Dictionary<string, float> statModifiers;
    public float lastSaveTime;
    public Dictionary<string, bool> achievements;
    public Dictionary<string, float> statistics;
}

[System.Serializable]
public class StatusEffectData
{
    public StatusEffectType type;
    public float duration;
    public float strength;
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