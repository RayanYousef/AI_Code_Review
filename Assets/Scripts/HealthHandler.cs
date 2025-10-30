using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class HealthHandler : MonoBehaviour
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
    public KeyCode areaHealKey = KeyCode.J;

    public float defense = 5f;
    public float damageMultiplier = 1f;
    public bool isInvulnerable = false;
    public float invulnerabilityDuration = 1f;

    public float regenRate = 1f;
    public bool autoRegen = true;

    // Complex Stats System
    public int level = 1;
    public float experience = 0f;
    public float experienceToNextLevel = 100f;

    public float strength = 10f;
    public float agility = 10f;
    public float intelligence = 10f;
    public float vitality = 10f;
    public float luck = 5f;

    public float criticalChance = 0.05f;
    public float criticalMultiplier = 2f;
    public float dodgeChance = 0.05f;
    public float accuracy = 1f;

    public float attackSpeed = 1f;
    public float movementSpeed = 5f;
    public float castSpeed = 1f;

    public float mana = 100f;
    public float maxMana = 100f;
    public float manaRegenRate = 2f;

    // Damage Types System
    public enum DamageType { Physical, Magical, Fire, Ice, Poison, Lightning, Holy, Dark }
    public enum ElementalAffinity { Neutral, Strong, Weak, Immune }

    public ElementalAffinity[] resistances = new ElementalAffinity[8]; // One for each damage type
    public DamageType primaryDamageType = DamageType.Physical;
    public DamageType secondaryDamageType = DamageType.Physical;

    public float fireDamage = 0f;
    public float iceDamage = 0f;
    public float poisonDamage = 0f;
    public float lightningDamage = 0f;

    // Status Effects System
    public enum StatusEffect { Poison, Burn, Freeze, Stun, Haste, Shield, Regen, Curse, Bless, Invisible }
    public class StatusEffectInstance
    {
        public StatusEffect effect;
        public float duration;
        public float potency;
        public GameObject source;

        public StatusEffectInstance(StatusEffect eff, float dur, float pot, GameObject src = null)
        {
            effect = eff;
            duration = dur;
            potency = pot;
            source = src;
        }
    }

    public List<StatusEffectInstance> activeEffects = new List<StatusEffectInstance>();
    private Dictionary<StatusEffect, float> effectModifiers = new Dictionary<StatusEffect, float>();

    // Complex Healing System
    public float overhealAmount = 0f;
    public float overhealDecayRate = 1f;
    public float overhealMaxMultiplier = 1.5f;
    public bool canOverheal = true;

    public float regenBuffMultiplier = 1f;
    public float healEfficiency = 1f;

    public float areaHealRadius = 5f;
    public float areaHealAmount = 20f;
    public float areaHealCooldown = 10f;
    private float lastAreaHealTime;

    // Advanced AI System
    public enum AIType { Passive, Aggressive, Defensive, Support, Boss }
    public enum AIState { Idle, Patrolling, Chasing, Attacking, Fleeing, Healing, Buffing }

    public AIType aiType = AIType.Aggressive;
    public AIState currentAIState = AIState.Idle;

    public float detectionRange = 10f;
    public float chaseRange = 15f;
    public float fleeThreshold = 0.3f; // Flee when health below 30%

    public Vector3[] patrolPoints;
    private int currentPatrolIndex = 0;

    public float abilityCooldown = 5f;
    private float lastAbilityTime;

    public bool canCastSpells = false;
    public float spellRange = 8f;

    private Vector3 lastKnownPlayerPosition;
    private float lastSeenPlayerTime;
    private float memoryDuration = 5f;

    // Player Ability System
    public enum AbilityType { Fireball, IceBlast, LightningStrike, Heal, Shield, Teleport, Summon, Transform }
    public class Ability
    {
        public AbilityType type;
        public KeyCode keyBinding;
        public float cooldown;
        public float manaCost;
        public float range;
        public float potency;
        public bool unlocked = false;
        public int level = 1;

        private float lastUsedTime;

        public bool IsReady()
        {
            return Time.time - lastUsedTime >= cooldown;
        }

        public void Use()
        {
            lastUsedTime = Time.time;
        }

        public float CooldownRemaining()
        {
            return Mathf.Max(0, cooldown - (Time.time - lastUsedTime));
        }
    }

    public Ability[] abilities = new Ability[8];
    public int skillPoints = 0;
    public int maxSkillPoints = 50;

    public KeyCode ability1Key = KeyCode.Q;
    public KeyCode ability2Key = KeyCode.W;
    public KeyCode ability3Key = KeyCode.E;
    public KeyCode ability4Key = KeyCode.R;

    // Complex UI System
    public Slider manaBar;
    public TextMeshProUGUI manaText;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI experienceText;

    public TextMeshProUGUI statsText;
    public TextMeshProUGUI skillPointsText;

    public GameObject statusEffectPanel;
    public GameObject abilityCooldownPanel;

    public Image[] abilityIcons = new Image[8];
    public Image[] abilityCooldownOverlays = new Image[8];
    public TextMeshProUGUI[] abilityCooldownTexts = new TextMeshProUGUI[8];

    public GameObject miniMap;
    public RectTransform miniMapPlayerIcon;
    public RectTransform[] miniMapEnemyIcons;

    public GameObject damageNumbersPrefab;
    public GameObject statusEffectIconPrefab;

    // Complex Animation System
    public enum AnimationState { Idle, Walking, Running, Attacking, Casting, Hurt, Dying, Dead, Special }
    public AnimationState currentAnimationState = AnimationState.Idle;

    public AnimatorOverrideController[] animationControllers; // Different animation sets
    public int currentAnimationSet = 0;

    public float animationBlendSpeed = 0.1f;
    private float currentAnimationBlend = 0f;

    public string[] attackAnimationTriggers = { "Attack1", "Attack2", "Attack3", "Attack4" };
    private int currentAttackAnimation = 0;

    public string[] castAnimationTriggers = { "CastFire", "CastIce", "CastLightning", "CastHeal" };

    public bool useRootMotion = false;
    public float animationSpeedMultiplier = 1f;

    // Complex Audio System
    public AudioSource spatialAudioSource;
    public AudioSource voiceAudioSource;
    public AudioSource musicAudioSource;

    public AudioClip[] damageSounds;
    public AudioClip[] deathSounds;
    public AudioClip[] healSounds;
    public AudioClip[] abilitySounds;

    public AudioClip[] voiceLinesIdle;
    public AudioClip[] voiceLinesHurt;
    public AudioClip[] voiceLinesDeath;
    public AudioClip[] voiceLinesVictory;

    public AudioClip[] ambientMusic;
    public AudioClip[] combatMusic;
    public AudioClip[] bossMusic;

    public float masterVolume = 1f;
    public float sfxVolume = 1f;
    public float voiceVolume = 1f;
    public float musicVolume = 1f;

    public float audioRange = 20f;
    private float lastVoiceLineTime;
    public float voiceLineCooldown = 5f;

    private enum MusicState { Ambient, Combat, Boss, Victory }
    private MusicState currentMusicState = MusicState.Ambient;

    // Advanced Loot System
    public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }
    public enum ItemType { Weapon, Armor, Accessory, Consumable, Material, Quest }

    public class Item
    {
        public string name;
        public ItemType type;
        public ItemRarity rarity;
        public int level;
        public Dictionary<string, float> stats = new Dictionary<string, float>();
        public string description;
        public Sprite icon;
        public bool isEquipped = false;
        public int quantity = 1;

        public Item(string itemName, ItemType itemType, ItemRarity itemRarity, int itemLevel)
        {
            name = itemName;
            type = itemType;
            rarity = itemRarity;
            level = itemLevel;
        }
    }

    public class Inventory
    {
        public List<Item> items = new List<Item>();
        public int maxSlots = 50;

        public bool AddItem(Item item)
        {
            if (items.Count >= maxSlots) return false;

            // Stack consumables
            if (item.type == ItemType.Consumable || item.type == ItemType.Material)
            {
                Item existing = items.Find(i => i.name == item.name && i.level == item.level);
                if (existing != null)
                {
                    existing.quantity += item.quantity;
                    return true;
                }
            }

            items.Add(item);
            return true;
        }

        public bool RemoveItem(Item item, int quantity = 1)
        {
            if (item.quantity > quantity)
            {
                item.quantity -= quantity;
                return true;
            }
            else if (item.quantity == quantity)
            {
                items.Remove(item);
                return true;
            }
            return false;
        }
    }

    public Inventory inventory = new Inventory();
    public Item[] equippedItems = new Item[6]; // Weapon, Helmet, Chest, Gloves, Boots, Accessory

    public Dictionary<ItemRarity, float> dropRates = new Dictionary<ItemRarity, float>()
    {
        { ItemRarity.Common, 0.5f },
        { ItemRarity.Uncommon, 0.25f },
        { ItemRarity.Rare, 0.15f },
        { ItemRarity.Epic, 0.08f },
        { ItemRarity.Legendary, 0.015f },
        { ItemRarity.Mythic, 0.005f }
    };

    public string[] weaponNames = { "Sword", "Axe", "Bow", "Staff", "Dagger", "Hammer" };
    public string[] armorNames = { "Helmet", "Chestplate", "Gauntlets", "Boots", "Shield" };
    public string[] materialNames = { "Iron Ore", "Gold Nugget", "Mana Crystal", "Dragon Scale", "Phoenix Feather" };
    public string[] consumableNames = { "Health Potion", "Mana Potion", "Strength Elixir", "Speed Boost", "Invisibility Potion" };

    private float lootDropChance = 0.8f; // 80% chance to drop loot

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

        if (isPlayer)
        {
            InitializeAbilities();
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

        if (mana < maxMana)
        {
            RegenerateMana();
        }

        // Overheal decay
        if (overhealAmount > 0)
        {
            overhealAmount = Mathf.Max(0, overhealAmount - overhealDecayRate * Time.deltaTime);
        }

        if (isEnemy)
        {
            UpdateEnemyAI();
        }

        UpdateUI();
        UpdateMusicState();
        PlayFootstepSounds(Vector3.zero); // This would need actual movement vector
    }

    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, DamageType.Physical);
    }

    public virtual void TakeDamage(float damage, DamageType damageType)
    {
        if (isInvulnerable || !isAlive) return;

        // Dodge calculation
        if (Random.value < dodgeChance)
        {
            if (animator != null)
            {
                animator.SetTrigger("Dodge");
            }
            if (audioSource != null && damageSound != null)
            {
                audioSource.PlayOneShot(damageSound); // Play dodge sound
            }
            Debug.Log("Dodged attack!");
            return;
        }

        // Apply elemental resistances/weaknesses
        float resistanceMultiplier = 1f;
        switch (resistances[(int)damageType])
        {
            case ElementalAffinity.Strong:
                resistanceMultiplier = 0.5f; // Strong against this damage
                break;
            case ElementalAffinity.Weak:
                resistanceMultiplier = 2f; // Weak against this damage
                break;
            case ElementalAffinity.Immune:
                resistanceMultiplier = 0f; // Immune to this damage
                break;
        }

        float actualDamage = Mathf.Max(0, damage - defense) * damageMultiplier * resistanceMultiplier;

        currentHealth -= actualDamage;

        // Show damage numbers
        ShowDamageNumbers(actualDamage, damageType);

        // Apply elemental effects
        ApplyElementalEffects(damageType, actualDamage);

        if (bloodEffect != null)
        {
            bloodEffect.Play();
        }

        PlayHitReaction();

        PlayDamageSound(damageType);

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
        float effectiveHeal = amount * healEfficiency;

        if (canOverheal && currentHealth >= maxHealth)
        {
            // Apply overheal
            overhealAmount += effectiveHeal;
            overhealAmount = Mathf.Min(overhealAmount, maxHealth * overhealMaxMultiplier);
            Debug.Log($"Overheal applied: {overhealAmount}");
        }
        else
        {
            // Normal healing
            float healNeeded = maxHealth - currentHealth;
            float actualHeal = Mathf.Min(effectiveHeal, healNeeded);

            currentHealth += actualHeal;

            // Apply remaining as overheal if enabled
            if (canOverheal && effectiveHeal > actualHeal)
            {
                overhealAmount += (effectiveHeal - actualHeal);
                overhealAmount = Mathf.Min(overhealAmount, maxHealth * overhealMaxMultiplier);
            }
        }

        PlayHealSound();

        if (animator != null)
        {
            animator.SetTrigger("Heal");
        }

        UpdateUI();
    }

    public void AreaHeal()
    {
        if (Time.time - lastAreaHealTime < areaHealCooldown) return;
        if (mana < areaHealAmount * 0.5f) return; // Mana cost

        lastAreaHealTime = Time.time;
        mana -= areaHealAmount * 0.5f;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, areaHealRadius);
        foreach (Collider2D collider in hitColliders)
        {
            HealthHandler target = collider.GetComponent<HealthHandler>();
            if (target != null && (target.isPlayer || target.isEnemy == isEnemy)) // Heal allies
            {
                target.Heal(areaHealAmount);
                target.ApplyStatusEffect(StatusEffect.Regen, 5f, 2f, gameObject);
            }
        }

        if (animator != null)
        {
            animator.SetTrigger("AreaHeal");
        }

        Debug.Log("Area heal cast!");
    }

    public void ApplyRegenBuff(float multiplier, float duration)
    {
        regenBuffMultiplier *= multiplier;
        StartCoroutine(RemoveRegenBuff(multiplier, duration));
    }

    private IEnumerator RemoveRegenBuff(float multiplier, float duration)
    {
        yield return new WaitForSeconds(duration);
        regenBuffMultiplier /= multiplier;
    }

    private void Die()
    {
        isAlive = false;

        PlayDeathAnimation();

        PlayDeathSound();

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

        if (Input.GetKeyDown(areaHealKey))
        {
            AreaHeal();
        }

        HandleAbilityInput();
    }

    private void UpdateEnemyAI()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        bool canSeePlayer = distance <= detectionRange;
        HealthHandler playerHealth = player.GetComponent<HealthHandler>();

        // Update memory
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = player.transform.position;
            lastSeenPlayerTime = Time.time;
        }

        // Determine AI state based on conditions
        DetermineAIState(distance, canSeePlayer, playerHealth);

        // Execute behavior based on current state
        ExecuteAIBehavior(player, distance);
    }

    private void DetermineAIState(float distance, bool canSeePlayer, HealthHandler playerHealth)
    {
        float healthPercent = currentHealth / maxHealth;

        switch (aiType)
        {
            case AIType.Aggressive:
                if (canSeePlayer && distance <= chaseRange)
                {
                    if (distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
                    {
                        currentAIState = AIState.Attacking;
                    }
                    else
                    {
                        currentAIState = AIState.Chasing;
                    }
                }
                else if (Time.time - lastSeenPlayerTime < memoryDuration)
                {
                    currentAIState = AIState.Chasing; // Chase to last known position
                }
                else
                {
                    currentAIState = AIState.Patrolling;
                }
                break;

            case AIType.Defensive:
                if (healthPercent < fleeThreshold)
                {
                    currentAIState = AIState.Fleeing;
                }
                else if (canSeePlayer && distance <= attackRange)
                {
                    currentAIState = AIState.Attacking;
                }
                else if (canSeePlayer)
                {
                    currentAIState = AIState.Chasing;
                }
                else
                {
                    currentAIState = AIState.Idle;
                }
                break;

            case AIType.Support:
                if (canSeePlayer && distance <= spellRange && Time.time - lastAbilityTime >= abilityCooldown)
                {
                    currentAIState = AIState.Buffing;
                }
                else if (healthPercent < 0.5f)
                {
                    currentAIState = AIState.Healing;
                }
                else
                {
                    currentAIState = AIState.Idle;
                }
                break;

            case AIType.Boss:
                if (healthPercent < 0.3f && canCastSpells)
                {
                    currentAIState = AIState.Buffing; // Desperate buffs
                }
                else if (canSeePlayer)
                {
                    currentAIState = AIState.Chasing;
                }
                else
                {
                    currentAIState = AIState.Idle;
                }
                break;
        }
    }

    private void ExecuteAIBehavior(GameObject player, float distance)
    {
        switch (currentAIState)
        {
            case AIState.Idle:
                // Do nothing, maybe play idle animation
                break;

            case AIState.Patrolling:
                Patrol();
                break;

            case AIState.Chasing:
                ChasePlayer(player, distance);
                break;

            case AIState.Attacking:
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack(player);
                }
                break;

            case AIState.Fleeing:
                FleeFromPlayer(player);
                break;

            case AIState.Healing:
                if (Time.time - lastAbilityTime >= abilityCooldown)
                {
                    CastSelfHeal();
                }
                break;

            case AIState.Buffing:
                if (Time.time - lastAbilityTime >= abilityCooldown)
                {
                    CastBuff();
                }
                break;
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Vector3 targetPoint = patrolPoints[currentPatrolIndex];
        float distanceToPoint = Vector3.Distance(transform.position, targetPoint);

        if (distanceToPoint < 1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            targetPoint = patrolPoints[currentPatrolIndex];
        }

        Vector3 direction = (targetPoint - transform.position).normalized;
        transform.position += direction * movementSpeed * Time.deltaTime;
    }

    private void ChasePlayer(GameObject player, float distance)
    {
        Vector3 targetPosition = (Time.time - lastSeenPlayerTime < memoryDuration) ?
            lastKnownPlayerPosition : player.transform.position;

        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * movementSpeed * Time.deltaTime;

        // If close enough to attack, switch to attacking
        if (distance <= attackRange)
        {
            currentAIState = AIState.Attacking;
        }
    }

    private void FleeFromPlayer(GameObject player)
    {
        Vector3 direction = (transform.position - player.transform.position).normalized;
        transform.position += direction * movementSpeed * 1.5f * Time.deltaTime;

        // Try to heal if possible
        if (Time.time - lastAbilityTime >= abilityCooldown && canCastSpells)
        {
            CastSelfHeal();
        }
    }

    private void CastSelfHeal()
    {
        lastAbilityTime = Time.time;
        Heal(maxHealth * 0.3f);
        ApplyStatusEffect(StatusEffect.Regen, 10f, 3f, gameObject);
        Debug.Log("Enemy casts self-heal!");
    }

    private void CastBuff()
    {
        lastAbilityTime = Time.time;

        switch (aiType)
        {
            case AIType.Support:
                // Buff nearby allies
                Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 10f);
                foreach (Collider2D collider in nearbyEnemies)
                {
                    HealthHandler enemy = collider.GetComponent<HealthHandler>();
                    if (enemy != null && enemy.isEnemy && enemy != this)
                    {
                        enemy.ApplyStatusEffect(StatusEffect.Haste, 15f, 1.5f, gameObject);
                        enemy.ApplyStatusEffect(StatusEffect.Shield, 10f, 20f, gameObject);
                    }
                }
                break;

            case AIType.Boss:
                // Self-buff for desperation
                ApplyStatusEffect(StatusEffect.Haste, 20f, 2f, gameObject);
                ApplyStatusEffect(StatusEffect.Bless, 20f, 1f, gameObject);
                damageMultiplier *= 1.5f;
                break;
        }

        Debug.Log("Enemy casts buff!");
    }

    private void Attack(GameObject target)
    {
        lastAttackTime = Time.time;

        // Accuracy check
        if (Random.value > accuracy)
        {
            Debug.Log("Attack missed!");
            if (animator != null)
            {
                animator.SetTrigger("Miss");
            }
            return;
        }

        PlayAttackAnimation();

        float damage = attackDamage * (1 + strength * 0.1f); // Strength affects damage

        // Critical hit calculation
        bool isCritical = Random.value < (criticalChance + luck * 0.01f);
        if (isCritical)
        {
            damage *= criticalMultiplier;
            Debug.Log("Critical hit!");
            if (animator != null)
            {
                animator.SetTrigger("Critical");
            }
        }

        HealthHandler targetHealth = target.GetComponent<HealthHandler>();
        if (targetHealth != null)
        {
            // Deal primary damage type
            targetHealth.TakeDamage(damage, primaryDamageType);

            // Deal elemental damage if available
            if (fireDamage > 0) targetHealth.TakeDamage(fireDamage, DamageType.Fire);
            if (iceDamage > 0) targetHealth.TakeDamage(iceDamage, DamageType.Ice);
            if (poisonDamage > 0) targetHealth.TakeDamage(poisonDamage, DamageType.Poison);
            if (lightningDamage > 0) targetHealth.TakeDamage(lightningDamage, DamageType.Lightning);
        }

        // Experience gain for enemies
        if (isEnemy)
        {
            GainExperience(damage * 0.1f);
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
        float regenAmount = regenRate * regenBuffMultiplier * Time.deltaTime;

        if (overhealAmount > 0)
        {
            // Use overheal first
            float overhealUsed = Mathf.Min(overhealAmount, regenAmount);
            overhealAmount -= overhealUsed;
            regenAmount -= overhealUsed;
        }

        if (regenAmount > 0)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + regenAmount);
        }
    }

    private void RegenerateMana()
    {
        mana = Mathf.Min(maxMana, mana + manaRegenRate * Time.deltaTime);
    }

    public void GainExperience(float exp)
    {
        experience += exp;
        if (experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        experience -= experienceToNextLevel;
        experienceToNextLevel *= 1.2f; // Exponential scaling

        // Random stat increases
        strength += Random.Range(1, 4);
        agility += Random.Range(1, 4);
        intelligence += Random.Range(1, 4);
        vitality += Random.Range(1, 4);
        luck += Random.Range(0, 2);

        // Update derived stats
        maxHealth += vitality * 10;
        maxMana += intelligence * 5;
        currentHealth = maxHealth; // Full heal on level up
        mana = maxMana;

        criticalChance = 0.05f + (luck * 0.005f);
        dodgeChance = 0.05f + (agility * 0.005f);
        accuracy = 1f + (agility * 0.01f);

        skillPoints += 3; // Grant skill points on level up
        skillPoints = Mathf.Min(skillPoints, maxSkillPoints);

        Debug.Log($"Leveled up to {level}! Stats increased! Skill points: {skillPoints}");

        if (animator != null)
        {
            animator.SetTrigger("LevelUp");
        }
    }

    private void ApplyElementalEffects(DamageType damageType, float damage)
    {
        switch (damageType)
        {
            case DamageType.Fire:
                StartCoroutine(FireDamageOverTime(damage * 0.2f, 3f));
                break;
            case DamageType.Ice:
                StartCoroutine(FreezeEffect(1f));
                break;
            case DamageType.Poison:
                StartCoroutine(PoisonDamageOverTime(damage * 0.3f, 5f));
                break;
            case DamageType.Lightning:
                StartCoroutine(StunEffect(0.5f));
                break;
        }
    }

    private IEnumerator FireDamageOverTime(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && isAlive)
        {
            TakeDamage(damagePerSecond * Time.deltaTime, DamageType.Fire);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator PoisonDamageOverTime(float totalDamage, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && isAlive)
        {
            TakeDamage(totalDamage * Time.deltaTime / duration, DamageType.Poison);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FreezeEffect(float duration)
    {
        movementSpeed *= 0.3f;
        attackSpeed *= 0.5f;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
        }
        yield return new WaitForSeconds(duration);
        movementSpeed /= 0.3f;
        attackSpeed /= 0.5f;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private IEnumerator StunEffect(float duration)
    {
        isInvulnerable = true; // Can't attack while stunned
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void ApplyStatusEffect(StatusEffect effect, float duration, float potency = 1f, GameObject source = null)
    {
        // Check if effect already exists, refresh if so
        StatusEffectInstance existing = activeEffects.Find(e => e.effect == effect);
        if (existing != null)
        {
            existing.duration = Mathf.Max(existing.duration, duration);
            existing.potency = Mathf.Max(existing.potency, potency);
            return;
        }

        StatusEffectInstance newEffect = new StatusEffectInstance(effect, duration, potency, source);
        activeEffects.Add(newEffect);

        // Apply immediate effects
        switch (effect)
        {
            case StatusEffect.Shield:
                // Add shield amount to a temporary shield pool
                break;
            case StatusEffect.Haste:
                attackSpeed *= (1 + potency);
                movementSpeed *= (1 + potency);
                break;
            case StatusEffect.Curse:
                damageMultiplier *= (1 - potency * 0.2f);
                break;
            case StatusEffect.Bless:
                damageMultiplier *= (1 + potency * 0.2f);
                break;
            case StatusEffect.Invisible:
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(1, 1, 1, 0.3f);
                }
                break;
        }

        StartCoroutine(StatusEffectCoroutine(newEffect));
    }

    private IEnumerator StatusEffectCoroutine(StatusEffectInstance effect)
    {
        float tickTimer = 0f;
        while (effect.duration > 0 && isAlive)
        {
            effect.duration -= Time.deltaTime;
            tickTimer += Time.deltaTime;

            // Apply tick effects
            if (tickTimer >= 1f) // Tick every second
            {
                switch (effect.effect)
                {
                    case StatusEffect.Poison:
                        TakeDamage(effect.potency, DamageType.Poison);
                        break;
                    case StatusEffect.Burn:
                        TakeDamage(effect.potency, DamageType.Fire);
                        break;
                    case StatusEffect.Regen:
                        Heal(effect.potency);
                        break;
                }
                tickTimer = 0f;
            }

            yield return null;
        }

        // Remove effect
        RemoveStatusEffect(effect);
    }

    private void RemoveStatusEffect(StatusEffectInstance effect)
    {
        activeEffects.Remove(effect);

        // Remove stat modifications
        switch (effect.effect)
        {
            case StatusEffect.Haste:
                attackSpeed /= (1 + effect.potency);
                movementSpeed /= (1 + effect.potency);
                break;
            case StatusEffect.Curse:
                damageMultiplier /= (1 - effect.potency * 0.2f);
                break;
            case StatusEffect.Bless:
                damageMultiplier /= (1 + effect.potency * 0.2f);
                break;
            case StatusEffect.Invisible:
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
                break;
        }
    }

    public bool HasStatusEffect(StatusEffect effect)
    {
        return activeEffects.Exists(e => e.effect == effect);
    }

    public void DispelStatusEffect(StatusEffect effect)
    {
        StatusEffectInstance toRemove = activeEffects.Find(e => e.effect == effect);
        if (toRemove != null)
        {
            StopCoroutine(StatusEffectCoroutine(toRemove));
            RemoveStatusEffect(toRemove);
        }
    }

    private void InitializeAbilities()
    {
        // Initialize basic abilities
        abilities[0] = new Ability { type = AbilityType.Fireball, keyBinding = ability1Key, cooldown = 3f, manaCost = 15f, range = 10f, potency = 25f, unlocked = true };
        abilities[1] = new Ability { type = AbilityType.IceBlast, keyBinding = ability2Key, cooldown = 5f, manaCost = 20f, range = 8f, potency = 20f, unlocked = false };
        abilities[2] = new Ability { type = AbilityType.LightningStrike, keyBinding = ability3Key, cooldown = 8f, manaCost = 30f, range = 12f, potency = 40f, unlocked = false };
        abilities[3] = new Ability { type = AbilityType.Shield, keyBinding = ability4Key, cooldown = 15f, manaCost = 25f, range = 0f, potency = 50f, unlocked = false };

        // Advanced abilities
        abilities[4] = new Ability { type = AbilityType.Heal, keyBinding = KeyCode.Alpha1, cooldown = 10f, manaCost = 35f, range = 0f, potency = 40f, unlocked = false };
        abilities[5] = new Ability { type = AbilityType.Teleport, keyBinding = KeyCode.Alpha2, cooldown = 12f, manaCost = 20f, range = 15f, potency = 0f, unlocked = false };
        abilities[6] = new Ability { type = AbilityType.Summon, keyBinding = KeyCode.Alpha3, cooldown = 30f, manaCost = 50f, range = 5f, potency = 1f, unlocked = false };
        abilities[7] = new Ability { type = AbilityType.Transform, keyBinding = KeyCode.Alpha4, cooldown = 60f, manaCost = 100f, range = 0f, potency = 1f, unlocked = false };
    }

    private void HandleAbilityInput()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            if (abilities[i] != null && abilities[i].unlocked && Input.GetKeyDown(abilities[i].keyBinding))
            {
                UseAbility(i);
            }
        }
    }

    public void UseAbility(int abilityIndex)
    {
        Ability ability = abilities[abilityIndex];
        if (ability == null || !ability.unlocked || !ability.IsReady() || mana < ability.manaCost)
        {
            return;
        }

        mana -= ability.manaCost;
        ability.Use();

        PlayCastAnimation(ability.type);
        PlayAbilitySound(ability.type);

        switch (ability.type)
        {
            case AbilityType.Fireball:
                CastFireball(ability);
                break;
            case AbilityType.IceBlast:
                CastIceBlast(ability);
                break;
            case AbilityType.LightningStrike:
                CastLightningStrike(ability);
                break;
            case AbilityType.Shield:
                CastShield(ability);
                break;
            case AbilityType.Heal:
                CastAdvancedHeal(ability);
                break;
            case AbilityType.Teleport:
                CastTeleport(ability);
                break;
            case AbilityType.Summon:
                CastSummon(ability);
                break;
            case AbilityType.Transform:
                CastTransform(ability);
                break;
        }
    }

    private void CastFireball(Ability ability)
    {
        // Find nearest enemy within range
        GameObject target = FindNearestEnemy(ability.range);
        if (target != null)
        {
            HealthHandler targetHealth = target.GetComponent<HealthHandler>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(ability.potency, DamageType.Fire);
                targetHealth.ApplyStatusEffect(StatusEffect.Burn, 5f, ability.potency * 0.1f, gameObject);
            }
        }
        Debug.Log("Cast Fireball!");
    }

    private void CastIceBlast(Ability ability)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, ability.range);
        foreach (Collider2D collider in hitEnemies)
        {
            HealthHandler enemy = collider.GetComponent<HealthHandler>();
            if (enemy != null && enemy.isEnemy)
            {
                enemy.TakeDamage(ability.potency, DamageType.Ice);
                enemy.ApplyStatusEffect(StatusEffect.Freeze, 3f, 1f, gameObject);
            }
        }
        Debug.Log("Cast Ice Blast!");
    }

    private void CastLightningStrike(Ability ability)
    {
        GameObject target = FindNearestEnemy(ability.range);
        if (target != null)
        {
            HealthHandler targetHealth = target.GetComponent<HealthHandler>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(ability.potency, DamageType.Lightning);
                // Chain to nearby enemies
                ChainLightning(target.transform.position, ability.potency * 0.7f, ability.range, 3);
            }
        }
        Debug.Log("Cast Lightning Strike!");
    }

    private void ChainLightning(Vector3 position, float damage, float range, int chainsLeft)
    {
        if (chainsLeft <= 0) return;

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(position, range);
        foreach (Collider2D collider in nearbyEnemies)
        {
            HealthHandler enemy = collider.GetComponent<HealthHandler>();
            if (enemy != null && enemy.isEnemy)
            {
                enemy.TakeDamage(damage, DamageType.Lightning);
                ChainLightning(collider.transform.position, damage * 0.7f, range, chainsLeft - 1);
                break; // Only chain to first enemy found
            }
        }
    }

    private void CastShield(Ability ability)
    {
        ApplyStatusEffect(StatusEffect.Shield, 10f, ability.potency, gameObject);
        Debug.Log("Cast Shield!");
    }

    private void CastAdvancedHeal(Ability ability)
    {
        Heal(ability.potency);
        ApplyStatusEffect(StatusEffect.Regen, 15f, 5f, gameObject);
        Debug.Log("Cast Advanced Heal!");
    }

    private void CastTeleport(Ability ability)
    {
        // Teleport to mouse position or away from enemies
        Vector3 teleportTarget = transform.position + new Vector3(Random.Range(-ability.range, ability.range), Random.Range(-ability.range, ability.range), 0);
        transform.position = teleportTarget;
        Debug.Log("Cast Teleport!");
    }

    private void CastSummon(Ability ability)
    {
        // Summon a temporary ally
        GameObject summon = new GameObject("SummonedAlly");
        summon.transform.position = transform.position + new Vector3(2, 0, 0);
        HealthHandler summonHealth = summon.AddComponent<HealthHandler>();
        summonHealth.maxHealth = 50;
        summonHealth.currentHealth = 50;
        summonHealth.attackDamage = 15;
        summonHealth.isEnemy = false; // Ally
        summonHealth.aiType = AIType.Defensive;

        // Destroy after 30 seconds
        Destroy(summon, 30f);
        Debug.Log("Cast Summon!");
    }

    private void CastTransform(Ability ability)
    {
        // Temporary transformation with buffs
        strength *= 2;
        agility *= 2;
        ApplyStatusEffect(StatusEffect.Haste, 30f, 2f, gameObject);
        ApplyStatusEffect(StatusEffect.Bless, 30f, 1f, gameObject);

        StartCoroutine(EndTransformation(30f));
        Debug.Log("Cast Transform!");
    }

    private IEnumerator EndTransformation(float duration)
    {
        yield return new WaitForSeconds(duration);
        strength /= 2;
        agility /= 2;
    }

    private GameObject FindNearestEnemy(float range)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);
        GameObject nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            HealthHandler health = collider.GetComponent<HealthHandler>();
            if (health != null && health.isEnemy)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = collider.gameObject;
                }
            }
        }

        return nearestEnemy;
    }

    public void UpgradeAbility(AbilityType type)
    {
        if (skillPoints <= 0) return;

        Ability ability = System.Array.Find(abilities, a => a != null && a.type == type);
        if (ability != null)
        {
            ability.level++;
            ability.potency *= 1.2f;
            ability.manaCost *= 0.9f;
            ability.cooldown *= 0.95f;
            skillPoints--;
            Debug.Log($"Upgraded {type} to level {ability.level}!");
        }
    }

    public void UnlockAbility(AbilityType type)
    {
        if (skillPoints < 5) return; // Cost to unlock

        Ability ability = System.Array.Find(abilities, a => a != null && a.type == type);
        if (ability != null && !ability.unlocked)
        {
            ability.unlocked = true;
            skillPoints -= 5;
            Debug.Log($"Unlocked {type}!");
        }
    }

    private void UpdateUI()
    {
        // Health UI
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            if (overhealAmount > 0)
            {
                healthText.text += $" (+{overhealAmount:F0})";
            }
        }

        // Mana UI
        if (manaBar != null)
        {
            manaBar.value = mana / maxMana;
        }

        if (manaText != null)
        {
            manaText.text = $"{mana:F0}/{maxMana:F0}";
        }

        // Level and Experience UI
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }

        if (experienceText != null)
        {
            experienceText.text = $"{experience:F0}/{experienceToNextLevel:F0} XP";
        }

        // Stats UI
        if (statsText != null && isPlayer)
        {
            statsText.text = $"STR: {strength:F0} AGI: {agility:F0} INT: {intelligence:F0} VIT: {vitality:F0} LCK: {luck:F0}\n" +
                           $"Crit: {criticalChance:P1} Dodge: {dodgeChance:P1} Acc: {accuracy:P1}";
        }

        // Skill Points UI
        if (skillPointsText != null && isPlayer)
        {
            skillPointsText.text = $"Skill Points: {skillPoints}";
        }

        // Status Effects UI
        UpdateStatusEffectIcons();

        // Ability Cooldowns UI
        UpdateAbilityCooldowns();

        // Mini-map UI
        UpdateMiniMap();
    }

    private void UpdateStatusEffectIcons()
    {
        if (statusEffectPanel == null) return;

        // Clear existing icons
        foreach (Transform child in statusEffectPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Create icons for active effects
        foreach (StatusEffectInstance effect in activeEffects)
        {
            if (statusEffectIconPrefab != null)
            {
                GameObject icon = Instantiate(statusEffectIconPrefab, statusEffectPanel.transform);
                TextMeshProUGUI text = icon.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{effect.effect}\n{effect.duration:F1}s";
                }
            }
        }
    }

    private void UpdateAbilityCooldowns()
    {
        if (!isPlayer || abilities == null) return;

        for (int i = 0; i < abilities.Length && i < abilityIcons.Length; i++)
        {
            Ability ability = abilities[i];
            if (ability == null) continue;

            // Update ability icon (would need actual sprites)
            if (abilityIcons[i] != null)
            {
                abilityIcons[i].gameObject.SetActive(ability.unlocked);
            }

            // Update cooldown overlay
            if (abilityCooldownOverlays[i] != null && abilityCooldownTexts[i] != null)
            {
                float cooldownRemaining = ability.CooldownRemaining();
                if (cooldownRemaining > 0)
                {
                    abilityCooldownOverlays[i].fillAmount = cooldownRemaining / ability.cooldown;
                    abilityCooldownTexts[i].text = $"{cooldownRemaining:F1}";
                    abilityCooldownTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    abilityCooldownOverlays[i].fillAmount = 0;
                    abilityCooldownTexts[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateMiniMap()
    {
        if (miniMap == null || !isPlayer) return;

        // Update player position on mini-map
        if (miniMapPlayerIcon != null)
        {
            // This would need proper mini-map scaling and positioning logic
            // For now, just show it's active
            miniMapPlayerIcon.gameObject.SetActive(true);
        }

        // Update enemy positions on mini-map
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // Resize array if needed
        if (miniMapEnemyIcons.Length < enemies.Length)
        {
            System.Array.Resize(ref miniMapEnemyIcons, enemies.Length);
        }

        for (int i = 0; i < enemies.Length && i < miniMapEnemyIcons.Length; i++)
        {
            if (miniMapEnemyIcons[i] != null)
            {
                miniMapEnemyIcons[i].gameObject.SetActive(true);
                // Position relative to player would go here
            }
        }
    }

    public void ShowDamageNumbers(float damage, DamageType damageType)
    {
        if (damageNumbersPrefab == null) return;

        GameObject damageText = Instantiate(damageNumbersPrefab, transform.position + Vector3.up, Quaternion.identity);
        TextMeshPro textComponent = damageText.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = $"{damage:F0}";
            // Color based on damage type
            switch (damageType)
            {
                case DamageType.Fire:
                    textComponent.color = Color.red;
                    break;
                case DamageType.Ice:
                    textComponent.color = Color.cyan;
                    break;
                case DamageType.Lightning:
                    textComponent.color = Color.yellow;
                    break;
                case DamageType.Poison:
                    textComponent.color = Color.green;
                    break;
                default:
                    textComponent.color = Color.white;
                    break;
            }
        }

        // Animate and destroy
        StartCoroutine(AnimateDamageNumbers(damageText));
    }

    private IEnumerator AnimateDamageNumbers(GameObject damageText)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = damageText.transform.position;
        Vector3 endPos = startPos + Vector3.up * 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            damageText.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(damageText);
    }

    public void SetAnimationState(AnimationState newState)
    {
        if (currentAnimationState == newState) return;

        AnimationState previousState = currentAnimationState;
        currentAnimationState = newState;

        if (animator != null)
        {
            // Reset previous triggers
            ResetAnimationTriggers(previousState);

            // Set new animation parameters
            UpdateAnimationParameters(newState);
        }
    }

    private void ResetAnimationTriggers(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Attacking:
                foreach (string trigger in attackAnimationTriggers)
                {
                    animator.ResetTrigger(trigger);
                }
                break;
            case AnimationState.Casting:
                foreach (string trigger in castAnimationTriggers)
                {
                    animator.ResetTrigger(trigger);
                }
                break;
        }
    }

    private void UpdateAnimationParameters(AnimationState state)
    {
        // Set blend parameters
        animator.SetFloat("Blend", (float)state * animationBlendSpeed);

        // Set boolean parameters
        animator.SetBool("IsIdle", state == AnimationState.Idle);
        animator.SetBool("IsWalking", state == AnimationState.Walking);
        animator.SetBool("IsRunning", state == AnimationState.Running);
        animator.SetBool("IsAttacking", state == AnimationState.Attacking);
        animator.SetBool("IsCasting", state == AnimationState.Casting);
        animator.SetBool("IsHurt", state == AnimationState.Hurt);
        animator.SetBool("IsDying", state == AnimationState.Dying);
        animator.SetBool("IsDead", state == AnimationState.Dead);
        animator.SetBool("IsSpecial", state == AnimationState.Special);

        // Set speed multiplier
        animator.SetFloat("SpeedMultiplier", animationSpeedMultiplier);

        // Handle status effect animations
        if (HasStatusEffect(StatusEffect.Stun))
        {
            animator.SetTrigger("Stunned");
        }
        if (HasStatusEffect(StatusEffect.Freeze))
        {
            animator.SetTrigger("Frozen");
        }
        if (HasStatusEffect(StatusEffect.Haste))
        {
            animator.SetFloat("SpeedMultiplier", animationSpeedMultiplier * 1.5f);
        }
    }

    public void PlayAttackAnimation()
    {
        if (animator == null) return;

        SetAnimationState(AnimationState.Attacking);
        string attackTrigger = attackAnimationTriggers[currentAttackAnimation];
        animator.SetTrigger(attackTrigger);

        // Cycle through attack animations
        currentAttackAnimation = (currentAttackAnimation + 1) % attackAnimationTriggers.Length;
    }

    public void PlayCastAnimation(AbilityType abilityType)
    {
        if (animator == null) return;

        SetAnimationState(AnimationState.Casting);

        // Choose cast animation based on ability type
        string castTrigger = "Cast";
        switch (abilityType)
        {
            case AbilityType.Fireball:
            case AbilityType.IceBlast:
            case AbilityType.LightningStrike:
                castTrigger = "CastOffensive";
                break;
            case AbilityType.Heal:
            case AbilityType.Shield:
                castTrigger = "CastDefensive";
                break;
            case AbilityType.Teleport:
                castTrigger = "CastMovement";
                break;
            case AbilityType.Summon:
            case AbilityType.Transform:
                castTrigger = "CastSpecial";
                break;
        }

        animator.SetTrigger(castTrigger);
    }

    public void SwitchAnimationSet(int setIndex)
    {
        if (animationControllers == null || setIndex >= animationControllers.Length) return;

        currentAnimationSet = setIndex;
        if (animator != null && animationControllers[setIndex] != null)
        {
            animator.runtimeAnimatorController = animationControllers[setIndex];
        }
    }

    public void PlayHitReaction()
    {
        if (animator == null) return;

        SetAnimationState(AnimationState.Hurt);
        animator.SetTrigger("Hurt");

        // Different hurt animations based on damage type would go here
        // For now, just use a generic hurt animation
    }

    public void PlayDeathAnimation()
    {
        if (animator == null) return;

        SetAnimationState(AnimationState.Dying);
        animator.SetTrigger("Die");

        // Wait for animation to finish before setting dead state
        StartCoroutine(DeathAnimationCoroutine());
    }

    private IEnumerator DeathAnimationCoroutine()
    {
        // Wait for death animation to start
        yield return new WaitForSeconds(0.1f);

        // Wait for animation to finish (this would need proper animation event or length detection)
        yield return new WaitForSeconds(2f);

        SetAnimationState(AnimationState.Dead);
    }

    public void UpdateMovementAnimation(Vector3 movementDirection)
    {
        if (animator == null) return;

        float speed = movementDirection.magnitude;

        if (speed > 0.1f)
        {
            if (speed > movementSpeed * 0.8f)
            {
                SetAnimationState(AnimationState.Running);
            }
            else
            {
                SetAnimationState(AnimationState.Walking);
            }

            // Set movement direction for blend trees
            animator.SetFloat("MoveX", movementDirection.x);
            animator.SetFloat("MoveY", movementDirection.y);
        }
        else
        {
            SetAnimationState(AnimationState.Idle);
        }
    }

    public void PlaySpecialAnimation(string animationName)
    {
        if (animator == null) return;

        SetAnimationState(AnimationState.Special);
        animator.SetTrigger(animationName);
    }

    public void PlaySpatialAudio(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (spatialAudioSource == null || clip == null) return;

        spatialAudioSource.clip = clip;
        spatialAudioSource.volume = volume * sfxVolume * masterVolume;
        spatialAudioSource.pitch = pitch;
        spatialAudioSource.Play();
    }

    public void PlayVoiceLine(AudioClip clip, float volume = 1f)
    {
        if (voiceAudioSource == null || clip == null) return;
        if (Time.time - lastVoiceLineTime < voiceLineCooldown) return;

        voiceAudioSource.clip = clip;
        voiceAudioSource.volume = volume * voiceVolume * masterVolume;
        voiceAudioSource.Play();

        lastVoiceLineTime = Time.time;
    }

    public void PlayRandomVoiceLine(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        PlayVoiceLine(randomClip, volume);
    }

    public void PlayDamageSound(DamageType damageType)
    {
        if (damageSounds == null || damageSounds.Length == 0) return;

        // Choose sound based on damage type
        int soundIndex = 0;
        switch (damageType)
        {
            case DamageType.Fire:
                soundIndex = Mathf.Min(1, damageSounds.Length - 1);
                break;
            case DamageType.Ice:
                soundIndex = Mathf.Min(2, damageSounds.Length - 1);
                break;
            case DamageType.Lightning:
                soundIndex = Mathf.Min(3, damageSounds.Length - 1);
                break;
        }

        PlaySpatialAudio(damageSounds[soundIndex]);
        PlayRandomVoiceLine(voiceLinesHurt);
    }

    public void PlayDeathSound()
    {
        if (deathSounds != null && deathSounds.Length > 0)
        {
            PlaySpatialAudio(deathSounds[Random.Range(0, deathSounds.Length)]);
        }
        PlayRandomVoiceLine(voiceLinesDeath);
    }

    public void PlayHealSound()
    {
        if (healSounds != null && healSounds.Length > 0)
        {
            PlaySpatialAudio(healSounds[Random.Range(0, healSounds.Length)]);
        }
    }

    public void PlayAbilitySound(AbilityType abilityType)
    {
        if (abilitySounds == null || abilitySounds.Length == 0) return;

        // Choose sound based on ability type
        int soundIndex = (int)abilityType;
        soundIndex = Mathf.Min(soundIndex, abilitySounds.Length - 1);

        PlaySpatialAudio(abilitySounds[soundIndex]);
    }

    public void UpdateMusicState()
    {
        if (musicAudioSource == null) return;

        MusicState newState = MusicState.Ambient;

        if (isPlayer)
        {
            // Check for enemies in range
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            bool enemiesNearby = false;
            bool bossNearby = false;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= audioRange)
                {
                    enemiesNearby = true;
                    HealthHandler enemyHealth = enemy.GetComponent<HealthHandler>();
                    if (enemyHealth != null && enemyHealth.aiType == AIType.Boss)
                    {
                        bossNearby = true;
                        break;
                    }
                }
            }

            if (bossNearby)
            {
                newState = MusicState.Boss;
            }
            else if (enemiesNearby)
            {
                newState = MusicState.Combat;
            }
            else if (currentHealth <= 0)
            {
                newState = MusicState.Victory;
            }
        }

        if (newState != currentMusicState)
        {
            currentMusicState = newState;
            SwitchMusicTrack(newState);
        }
    }

    private void SwitchMusicTrack(MusicState state)
    {
        AudioClip[] musicArray = null;

        switch (state)
        {
            case MusicState.Ambient:
                musicArray = ambientMusic;
                break;
            case MusicState.Combat:
                musicArray = combatMusic;
                break;
            case MusicState.Boss:
                musicArray = bossMusic;
                break;
        }

        if (musicArray != null && musicArray.Length > 0)
        {
            AudioClip newTrack = musicArray[Random.Range(0, musicArray.Length)];
            if (musicAudioSource.clip != newTrack)
            {
                musicAudioSource.clip = newTrack;
                musicAudioSource.volume = musicVolume * masterVolume;
                musicAudioSource.loop = true;
                musicAudioSource.Play();
            }
        }
    }

    public void SetAudioVolumes(float master, float sfx, float voice, float music)
    {
        masterVolume = master;
        sfxVolume = sfx;
        voiceVolume = voice;
        musicVolume = music;

        // Update current playing audio
        if (spatialAudioSource != null)
            spatialAudioSource.volume = sfxVolume * masterVolume;
        if (voiceAudioSource != null)
            voiceAudioSource.volume = voiceVolume * masterVolume;
        if (musicAudioSource != null)
            musicAudioSource.volume = musicVolume * masterVolume;
    }

    public void PlayFootstepSounds(Vector3 movementDirection)
    {
        if (movementDirection.magnitude > 0.1f && spatialAudioSource != null)
        {
            // This would typically use a footstep audio clip
            // For now, just play a generic movement sound occasionally
            if (Random.value < 0.1f) // 10% chance per frame to play
            {
                // PlaySpatialAudio(footstepClip);
            }
        }
    }

    public void PlayEnvironmentalAudio()
    {
        // This would handle ambient environmental sounds
        // Like wind, water, crowd noises, etc.
        // Implementation would depend on the environment
    }

    private Item GenerateRandomItem()
    {
        // Determine rarity based on drop rates
        ItemRarity rarity = ItemRarity.Common;
        float random = Random.value;

        foreach (var rate in dropRates)
        {
            if (random <= rate.Value)
            {
                rarity = rate.Key;
                break;
            }
            random -= rate.Value;
        }

        // Determine item type based on enemy level and type
        ItemType itemType = GetRandomItemType();

        // Generate item based on type
        switch (itemType)
        {
            case ItemType.Weapon:
                return GenerateWeapon(rarity);
            case ItemType.Armor:
                return GenerateArmor(rarity);
            case ItemType.Consumable:
                return GenerateConsumable(rarity);
            case ItemType.Material:
                return GenerateMaterial(rarity);
            default:
                return GenerateAccessory(rarity);
        }
    }

    private ItemType GetRandomItemType()
    {
        float random = Random.value;

        if (random < 0.3f) return ItemType.Material;
        if (random < 0.5f) return ItemType.Consumable;
        if (random < 0.7f) return ItemType.Weapon;
        if (random < 0.9f) return ItemType.Armor;
        return ItemType.Accessory;
    }

    private Item GenerateWeapon(ItemRarity rarity)
    {
        string baseName = weaponNames[Random.Range(0, weaponNames.Length)];
        string rarityPrefix = GetRarityPrefix(rarity);
        int itemLevel = Mathf.Max(1, level + Random.Range(-2, 3));

        Item weapon = new Item($"{rarityPrefix} {baseName}", ItemType.Weapon, rarity, itemLevel);

        // Generate stats based on rarity and level
        float statMultiplier = GetRarityMultiplier(rarity) * itemLevel;

        weapon.stats["Damage"] = 10 + statMultiplier * 5;
        weapon.stats["AttackSpeed"] = 1 + statMultiplier * 0.1f;

        // Add elemental damage based on primary damage type
        if (primaryDamageType != DamageType.Physical)
        {
            string elementName = primaryDamageType.ToString();
            weapon.stats[$"{elementName}Damage"] = statMultiplier * 3;
        }

        weapon.description = $"A {rarity.ToString().ToLower()} weapon that deals {weapon.stats["Damage"]:F0} damage.";
        return weapon;
    }

    private Item GenerateArmor(ItemRarity rarity)
    {
        string baseName = armorNames[Random.Range(0, armorNames.Length)];
        string rarityPrefix = GetRarityPrefix(rarity);
        int itemLevel = Mathf.Max(1, level + Random.Range(-2, 3));

        Item armor = new Item($"{rarityPrefix} {baseName}", ItemType.Armor, rarity, itemLevel);

        float statMultiplier = GetRarityMultiplier(rarity) * itemLevel;

        armor.stats["Defense"] = 5 + statMultiplier * 2;
        armor.stats["MaxHealth"] = statMultiplier * 10;

        // Add resistances
        foreach (DamageType damageType in System.Enum.GetValues(typeof(DamageType)))
        {
            if (Random.value < 0.3f) // 30% chance for each resistance
            {
                armor.stats[$"{damageType}Resistance"] = statMultiplier * 0.5f;
            }
        }

        armor.description = $"Armor that provides {armor.stats["Defense"]:F0} defense.";
        return armor;
    }

    private Item GenerateConsumable(ItemRarity rarity)
    {
        string baseName = consumableNames[Random.Range(0, consumableNames.Length)];
        string rarityPrefix = GetRarityPrefix(rarity);
        int itemLevel = Mathf.Max(1, level + Random.Range(-1, 2));

        Item consumable = new Item($"{rarityPrefix} {baseName}", ItemType.Consumable, rarity, itemLevel);

        float statMultiplier = GetRarityMultiplier(rarity) * itemLevel;

        switch (baseName)
        {
            case "Health Potion":
                consumable.stats["HealAmount"] = 20 + statMultiplier * 10;
                break;
            case "Mana Potion":
                consumable.stats["ManaAmount"] = 25 + statMultiplier * 12;
                break;
            case "Strength Elixir":
                consumable.stats["StrengthBuff"] = 5 + statMultiplier * 3;
                consumable.stats["Duration"] = 30 + statMultiplier * 10;
                break;
            case "Speed Boost":
                consumable.stats["SpeedBuff"] = 2 + statMultiplier * 1;
                consumable.stats["Duration"] = 20 + statMultiplier * 8;
                break;
            case "Invisibility Potion":
                consumable.stats["Duration"] = 15 + statMultiplier * 5;
                break;
        }

        consumable.description = $"A {rarity.ToString().ToLower()} consumable item.";
        return consumable;
    }

    private Item GenerateMaterial(ItemRarity rarity)
    {
        string baseName = materialNames[Random.Range(0, materialNames.Length)];
        int itemLevel = Mathf.Max(1, level + Random.Range(-1, 2));
        int quantity = Random.Range(1, 4) * (int)rarity + 1;

        Item material = new Item(baseName, ItemType.Material, rarity, itemLevel);
        material.quantity = quantity;

        material.description = $"A crafting material. Quantity: {quantity}";
        return material;
    }

    private Item GenerateAccessory(ItemRarity rarity)
    {
        string[] accessoryTypes = { "Ring", "Amulet", "Bracelet", "Necklace", "Earring" };
        string baseName = accessoryTypes[Random.Range(0, accessoryTypes.Length)];
        string rarityPrefix = GetRarityPrefix(rarity);
        int itemLevel = Mathf.Max(1, level + Random.Range(-2, 3));

        Item accessory = new Item($"{rarityPrefix} {baseName}", ItemType.Accessory, rarity, itemLevel);

        float statMultiplier = GetRarityMultiplier(rarity) * itemLevel;

        // Random stat bonuses
        string[] possibleStats = { "Strength", "Agility", "Intelligence", "Vitality", "Luck", "CriticalChance", "DodgeChance" };
        int numStats = Random.Range(1, 4); // 1-3 random stats

        for (int i = 0; i < numStats; i++)
        {
            string stat = possibleStats[Random.Range(0, possibleStats.Length)];
            accessory.stats[stat] = statMultiplier * Random.Range(0.5f, 2f);
        }

        accessory.description = $"An accessory that provides various stat bonuses.";
        return accessory;
    }

    private string GetRarityPrefix(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "";
            case ItemRarity.Uncommon: return "Fine";
            case ItemRarity.Rare: return "Rare";
            case ItemRarity.Epic: return "Epic";
            case ItemRarity.Legendary: return "Legendary";
            case ItemRarity.Mythic: return "Mythic";
            default: return "";
        }
    }

    private float GetRarityMultiplier(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return 1f;
            case ItemRarity.Uncommon: return 1.5f;
            case ItemRarity.Rare: return 2.5f;
            case ItemRarity.Epic: return 4f;
            case ItemRarity.Legendary: return 6f;
            case ItemRarity.Mythic: return 10f;
            default: return 1f;
        }
    }

    private void SpawnLootItem(Item item)
    {
        // Create a loot object in the world
        GameObject lootObject = new GameObject($"Loot_{item.name}");
        lootObject.transform.position = transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);

        // Add components for pickup
        lootObject.AddComponent<LootPickup>().item = item;
        lootObject.AddComponent<SpriteRenderer>(); // Would need actual sprites
        lootObject.AddComponent<CircleCollider2D>().isTrigger = true;

        // Auto-destroy after 30 seconds
        Destroy(lootObject, 30f);
    }

    public void EquipItem(Item item)
    {
        if (item == null || item.isEquipped) return;

        int slotIndex = GetEquipmentSlot(item.type);
        if (slotIndex == -1) return;

        // Unequip current item if any
        if (equippedItems[slotIndex] != null)
        {
            UnequipItem(equippedItems[slotIndex]);
        }

        // Equip new item
        equippedItems[slotIndex] = item;
        item.isEquipped = true;

        // Apply stat bonuses
        ApplyEquipmentStats(item, true);

        Debug.Log($"Equipped {item.name}");
    }

    public void UnequipItem(Item item)
    {
        if (item == null || !item.isEquipped) return;

        int slotIndex = System.Array.IndexOf(equippedItems, item);
        if (slotIndex == -1) return;

        equippedItems[slotIndex] = null;
        item.isEquipped = false;

        // Remove stat bonuses
        ApplyEquipmentStats(item, false);

        Debug.Log($"Unequipped {item.name}");
    }

    private int GetEquipmentSlot(ItemType type)
    {
        switch (type)
        {
            case ItemType.Weapon: return 0;
            case ItemType.Armor:
                // Determine armor slot based on name
                string name = equippedItems[0]?.name.ToLower() ?? "";
                if (name.Contains("helmet")) return 1;
                if (name.Contains("chest")) return 2;
                if (name.Contains("gauntlet")) return 3;
                if (name.Contains("boot")) return 4;
                if (name.Contains("shield")) return 5;
                return Random.Range(1, 5); // Random armor slot
            case ItemType.Accessory: return 5;
            default: return -1;
        }
    }

    private void ApplyEquipmentStats(Item item, bool apply)
    {
        float multiplier = apply ? 1f : -1f;

        foreach (var stat in item.stats)
        {
            switch (stat.Key)
            {
                case "Damage": attackDamage += stat.Value * multiplier; break;
                case "Defense": defense += stat.Value * multiplier; break;
                case "MaxHealth": maxHealth += stat.Value * multiplier; break;
                case "Strength": strength += stat.Value * multiplier; break;
                case "Agility": agility += stat.Value * multiplier; break;
                case "Intelligence": intelligence += stat.Value * multiplier; break;
                case "Vitality": vitality += stat.Value * multiplier; break;
                case "Luck": luck += stat.Value * multiplier; break;
                case "CriticalChance": criticalChance += stat.Value * multiplier; break;
                case "DodgeChance": dodgeChance += stat.Value * multiplier; break;
                case "AttackSpeed": attackSpeed += stat.Value * multiplier; break;
                case "MovementSpeed": movementSpeed += stat.Value * multiplier; break;
            }
        }

        // Recalculate derived stats
        if (apply)
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            mana = Mathf.Min(mana, maxMana);
        }
    }

    public void UseConsumable(Item item)
    {
        if (item == null || item.type != ItemType.Consumable) return;

        foreach (var stat in item.stats)
        {
            switch (stat.Key)
            {
                case "HealAmount":
                    Heal(stat.Value);
                    break;
                case "ManaAmount":
                    mana = Mathf.Min(maxMana, mana + stat.Value);
                    break;
                case "StrengthBuff":
                    ApplyStatusEffect(StatusEffect.Bless, item.stats["Duration"], stat.Value, gameObject);
                    break;
                case "SpeedBuff":
                    ApplyStatusEffect(StatusEffect.Haste, item.stats["Duration"], stat.Value, gameObject);
                    break;
                case "Duration":
                    ApplyStatusEffect(StatusEffect.Invisible, stat.Value, 1f, gameObject);
                    break;
            }
        }

        inventory.RemoveItem(item, 1);
        Debug.Log($"Used {item.name}");
    }

    public Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.gray;
            case ItemRarity.Uncommon: return Color.green;
            case ItemRarity.Rare: return Color.blue;
            case ItemRarity.Epic: return Color.magenta;
            case ItemRarity.Legendary: return Color.yellow;
            case ItemRarity.Mythic: return new Color(1f, 0.5f, 0f); // Orange
            default: return Color.white;
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

// Simple loot pickup component
public class LootPickup : MonoBehaviour
{
    public HealthHandler.Item item;

    private void OnTriggerEnter2D(Collider2D other)
    {
        HealthHandler player = other.GetComponent<HealthHandler>();
        if (player != null && player.isPlayer)
        {
            if (player.inventory.AddItem(item))
            {
                Debug.Log($"Picked up {item.name}");
                Destroy(gameObject);
            }
        }
    }
}

public class AdvancedHealthSystem : HealthHandler
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