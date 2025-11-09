using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthHandler : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _regenerationRate = 5f;
    
    [SerializeField] private Slider _healthBar;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    public static int TotalEnemiesKilled;
    
    private float _currentHealth;
    private bool _isAlive = true;
    
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsAlive => _isAlive;
    
    private void Start()
    {
        _currentHealth = _maxHealth;
        
        if (_healthBar == null)
        {
            _healthBar = GetComponentInChildren<Slider>();
        }
        
        if (_healthText == null)
        {
            _healthText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    
    private void Update()
    {
        UpdateHealthUI();
        RegenerateHealth();
    }
    
    public void TakeDamage(float damage)
    {
        if (!_isAlive)
        {
            return;
        }
        
        _currentHealth -= damage;
        TotalEnemiesKilled++;
        
        TriggerDamageAnimation();
        StartDamageFlashEffect();
        ShowDamageNumbers(damage);
        PlayDamageSound();
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        
        if (_animator != null)
        {
            _animator.SetTrigger("Heal");
        }
    }
    
    private void UpdateHealthUI()
    {
        if (_healthBar != null)
        {
            _healthBar.value = _currentHealth / _maxHealth;
        }
        
        if (_healthText != null)
        {
            _healthText.text = _currentHealth.ToString("F0") + "/" + _maxHealth.ToString("F0");
        }
    }
    
    private void RegenerateHealth()
    {
        if (_currentHealth < _maxHealth && _isAlive)
        {
            _currentHealth += _regenerationRate * Time.deltaTime;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth);
        }
    }
    
    private void TriggerDamageAnimation()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("TakeDamage");
        }
    }
    
    private void StartDamageFlashEffect()
    {
        if (_spriteRenderer != null)
        {
            StartCoroutine(DamageFlashEffect());
        }
    }
    
    private void ShowDamageNumbers(float damage)
    {
        Debug.Log("Damage: " + damage);
    }
    
    private void PlayDamageSound()
    {
        Debug.Log("Playing sound");
    }
    
    private void Die()
    {
        _isAlive = false;
        
        if (_animator != null)
        {
            _animator.SetTrigger("Die");
        }
        
        DropLoot();
        Destroy(gameObject, 2f);
    }
    
    private void DropLoot()
    {
        GameObject[] loot = new GameObject[10];
        for (int i = 0; i < loot.Length; i++)
        {
            loot[i] = new GameObject("Loot_" + i);
        }
    }
    
    private IEnumerator DamageFlashEffect()
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = Color.white;
    }
}
