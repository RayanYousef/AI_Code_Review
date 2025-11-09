# Coding Guidelines for AI Code Review Project

This document contains the coding standards and best practices for our Unity project. Please follow these guidelines to ensure code quality, consistency, and maintainability.

## Table of Contents

1. [General Principles](#general-principles)
2. [C# Coding Style](#c-coding-style)
3. [Unity Specific Guidelines](#unity-specific-guidelines)
4. [File Organization](#file-organization)
5. [Code Examples](#code-examples)

## General Principles

### SOLID Principles

- **Single Responsibility**: Each class should have only one reason to change. One job, one responsibility.
- **Open/Closed**: Classes should be open for extension but closed for modification. Avoid switch/if-else chains for type checking.
- **Liskov Substitution**: Subtypes must be substitutable for their base types without breaking functionality.
- **Interface Segregation**: Clients should not be forced to depend on interfaces they don't use. Split fat interfaces into smaller ones.
- **Dependency Inversion**: Depend on abstractions, not concretions. Use VContainer for dependency injection.

### KISS Principle (Keep It Simple, Stupid)

- Write simple, clear code that is easy to understand
- Avoid unnecessary complexity
- Prefer straightforward solutions over clever ones

### Command and Query Separation (CQS)

- Commands change state and return void
- Queries return data and do not change state
- Methods should either do something OR return something, not both

### DRY Principle (Don't Repeat Yourself)

- Avoid code duplication
- Extract common functionality into reusable methods or classes
- Use inheritance, composition, or utility classes when appropriate

### Function Design

- Functions should have only one responsibility
- If a function handles multiple tasks, split it into smaller functions
- Each function should be focused on a single purpose

## C# Coding Style

Based on:

- Google C# Style Guide: <https://google.github.io/styleguide/csharp-style.html>
- .NET Runtime Coding Guidelines: <https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md>

### Naming Conventions

#### General Rules

- **Classes, Methods, Enumerations, Public Fields, Public Properties, Namespaces**: Use PascalCase
- **Local Variables, Parameters**: Use camelCase
- **Private, Protected, Internal Fields and Properties**: Use _camelCase (underscore prefix)
- Naming is the same regardless of modifiers (const, static, readonly, etc.)
- Acronyms are treated as single words: MyRpc (not MyRPC)
- Interface names start with 'I': IInterface

#### Special Cases

- Boolean properties should start with questions: IsEnabled, HasValue, DidComplete
- Events should be prefixed with "On": OnValueChanged
- Event listener methods end with "Callback": ValueChangedCallback

### Code Structure

- Always specify visibility modifiers (private, public, etc.), even when it's the default
- Visibility modifier should be first: `public static` not `static public`
- Avoid `this.` unless necessary for clarity
- Avoid `var` - always use explicit types
- Use null conditional operator for delegates/events: `SomeDelegate?.Invoke()`

## Unity Specific Guidelines

### Component References

- **Prefer `[SerializeField]` over `GetComponent()`**: Serialize component references in the inspector whenever possible
- Only use `GetComponent()` for components that are created at runtime or cannot be serialized in the editor
- If you must use `GetComponent()`, cache the reference in Awake() or Start(), never in Update()
- Never call `GetComponent()` in Update() or frequently called methods
- Name variables based on their type: `_levelManager`, `_rigidbody`, `_animator`

### Serialization

- Use `[SerializeField]` for private fields that need Unity editor access
- Always write `private` with `[SerializeField]`: `[SerializeField] private int _health;`
- Prefer auto-implemented properties when possible: `public int PageNumber { get; set; }`

### Method Naming

- Editor button methods start with "OnButtonClick_": `OnButtonClick_StartGame`
- These methods should handle button feedback and call other methods for logic
- Avoid having other methods call OnButtonClick_ methods directly
- Avoid simple one-word method names: Use `StartInteraction` instead of `Interact`

### Performance (Zero Garbage Collection)

- Avoid LINQ - use for-loops instead (LINQ creates garbage)
- Use NonAlloc methods: `Physics.RaycastNonAlloc` instead of `Physics.Raycast`
- Cache strings and avoid string concatenation in loops
- Reference: <https://docs.unity3d.com/Manual/performance-garbage-collection-best-practices.html>

### MonoBehaviour Methods

- Don't use public access modifiers for Unity-specific methods (Start, Update, etc.)
- Keep these methods private or remove access modifier

## File Organization

### File Naming

- Files and directories use PascalCase: MyClass.cs, MyFolder/
- Prefer one main class per file
- File names should match the main class name

### Using Statements

- All using statements go at the top, before namespace
- System imports come first, then alphabetical order

### Class Member Ordering

Group members in this order:

1. Nested classes, enums, delegates, events
2. Static, const, readonly fields
3. Fields and properties
4. Constructors and destructors
5. Methods

### Access Modifier Ordering

Within each group, order by accessibility:

1. Public
2. Internal
3. Protected internal
4. Protected
5. Private

## Code Examples

### 1. Naming Conventions

#### ❌ Don't

```csharp
using System;

namespace myNamespace // Wrong: should be PascalCase
{
    public interface Calculator // Wrong: interface should start with 'I'
    {
        int calculate(float Value, float Exponent); // Wrong: method camelCase, params PascalCase
    }

    public enum operationType // Wrong: should be PascalCase
    {
        add, subtract // Wrong: should be PascalCase
    }

    public class Calculator : Calculator
    {
        public int lastResult; // Wrong: field should be PascalCase or use property
        private bool isCalculating; // Wrong: should be _isCalculating
        public static int times_used; // Wrong: should be TimesUsed
        private const int MAX_VALUE = 100; // Wrong: should be _maxValue

        public bool Enabled() // Wrong: boolean should start with Is/Has/Did
        {
            return true;
        }
    }
}
```

#### ✅ Do

```csharp
using System;

namespace MyNamespace // PascalCase namespace
{
    public interface ICalculator // Interface starts with I
    {
        int Calculate(float value, float exponent); // PascalCase method, camelCase params
    }

    public enum OperationType // PascalCase enum
    {
        Add, Subtract, Multiply // PascalCase enum values
    }

    public class Calculator : ICalculator
    {
        public int LastResult { get; private set; } // PascalCase property
        private bool _isCalculating; // _camelCase private field
        public static int TimesUsed; // PascalCase static field
        private const int _maxValue = 100; // _camelCase const field

        public bool IsEnabled() // Boolean starts with Is
        {
            return true;
        }
    }
}
```

### 2. Single Responsibility Principle (SRP)

#### ❌ Don't - Method with Multiple Responsibilities

```csharp
public class DataProcessor
{
    // Wrong: one method doing everything (calculate, validate, log, save, notify)
    public int ProcessData(float value, float exponent)
    {
        if (value <= 0)
        {
            Console.WriteLine("Invalid value");
            return -1;
        }

        int result = (int)(value * Math.Pow(value, exponent));
        Console.WriteLine($"Calculated: {result}");

        File.WriteAllText("result.txt", result.ToString());

        OnDataProcessed?.Invoke(result);

        return result;
    }

    public event Action<int> OnDataProcessed;
}
```

#### ✅ Do - Each Method Has Single Responsibility

```csharp
public class DataProcessor
{
    public event Action<int> OnDataProcessed;

    // Main method - orchestrates the process
    public int ProcessData(float value, float exponent)
    {
        if (!IsValidInput(value))
        {
            return -1;
        }

        int result = Calculate(value, exponent);
        SaveResult(result);
        NotifyProcessed(result);

        return result;
    }

    // Single responsibility: validates input
    private bool IsValidInput(float value)
    {
        bool isValid = value > 0;
        if (!isValid)
        {
            LogError("Invalid value");
        }
        return isValid;
    }

    // Single responsibility: performs calculation
    private int Calculate(float value, float exponent)
    {
        return (int)(value * Math.Pow(value, exponent));
    }

    // Single responsibility: saves to file
    private void SaveResult(int result)
    {
        File.WriteAllText("result.txt", result.ToString());
    }

    // Single responsibility: notifies listeners
    private void NotifyProcessed(int result)
    {
        OnDataProcessed?.Invoke(result);
    }

    // Single responsibility: logs errors
    private void LogError(string message)
    {
        Console.WriteLine(message);
    }
}
```

### 3. Open/Closed Principle (OCP)

#### ❌ Don't - Using Switch/If-Else Chains

```csharp
public class DamageCalculator
{
    // Wrong: violates OCP - must modify this method to add new weapon types
    public int CalculateDamage(string weaponType, int baseDamage)
    {
        switch (weaponType)
        {
            case "Sword":
                return baseDamage * 2;
            case "Axe":
                return baseDamage * 3;
            case "Bow":
                return baseDamage + 10;
            default:
                return baseDamage;
        }
    }

    // Wrong: same problem with if-else
    public float GetMovementSpeed(string enemyType)
    {
        if (enemyType == "Zombie")
        {
            return 2.0f;
        }
        else if (enemyType == "Runner")
        {
            return 5.0f;
        }
        else if (enemyType == "Tank")
        {
            return 1.0f;
        }
        else
        {
            return 3.0f;
        }
    }
}
```

#### ✅ Do - Use Polymorphism and Abstraction

```csharp
public interface IWeapon
{
    int CalculateDamage(int baseDamage);
}

public class Sword : IWeapon
{
    public int CalculateDamage(int baseDamage)
    {
        return baseDamage * 2;
    }
}

public class Axe : IWeapon
{
    public int CalculateDamage(int baseDamage)
    {
        return baseDamage * 3;
    }
}

public class Bow : IWeapon
{
    public int CalculateDamage(int baseDamage)
    {
        return baseDamage + 10;
    }
}

public class DamageCalculator
{
    // Open for extension (add new weapon classes), closed for modification
    public int CalculateDamage(IWeapon weapon, int baseDamage)
    {
        return weapon.CalculateDamage(baseDamage);
    }
}
```

```csharp
public abstract class Enemy
{
    public abstract float GetMovementSpeed();
}

public class Zombie : Enemy
{
    public override float GetMovementSpeed()
    {
        return 2.0f;
    }
}

public class Runner : Enemy
{
    public override float GetMovementSpeed()
    {
        return 5.0f;
    }
}

public class Tank : Enemy
{
    public override float GetMovementSpeed()
    {
        return 1.0f;
    }
}
```

### 4. Liskov Substitution Principle (LSP)

#### ❌ Don't - Derived Class Changes Expected Behavior

```csharp
public class Bird
{
    public virtual void Fly()
    {
        Debug.Log("Flying");
    }
}

public class Penguin : Bird
{
    // Wrong: violates LSP - penguin can't fly but inherits from Bird
    public override void Fly()
    {
        throw new NotSupportedException("Penguins can't fly!");
    }
}

public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }

    public int GetArea()
    {
        return Width * Height;
    }
}

public class Square : Rectangle
{
    // Wrong: violates LSP - changes behavior of base class
    public override int Width
    {
        get => base.Width;
        set
        {
            base.Width = value;
            base.Height = value; // Unexpected side effect
        }
    }

    public override int Height
    {
        get => base.Height;
        set
        {
            base.Width = value; // Unexpected side effect
            base.Height = value;
        }
    }
}
```

#### ✅ Do - Proper Inheritance Hierarchy

```csharp
public abstract class Bird
{
    public abstract void Move();
}

public class FlyingBird : Bird
{
    public override void Move()
    {
        Fly();
    }

    protected virtual void Fly()
    {
        Debug.Log("Flying");
    }
}

public class Eagle : FlyingBird
{
    protected override void Fly()
    {
        Debug.Log("Eagle soaring high");
    }
}

public class Penguin : Bird
{
    public override void Move()
    {
        Swim();
    }

    private void Swim()
    {
        Debug.Log("Penguin swimming");
    }
}
```

```csharp
public interface IShape
{
    int GetArea();
}

public class Rectangle : IShape
{
    public int Width { get; set; }
    public int Height { get; set; }

    public int GetArea()
    {
        return Width * Height;
    }
}

public class Square : IShape
{
    public int SideLength { get; set; }

    public int GetArea()
    {
        return SideLength * SideLength;
    }
}
```

### 5. Interface Segregation Principle (ISP)

#### ❌ Don't - Fat Interface Forces Unused Implementation

```csharp
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
    void GetPaid();
}

public class Robot : IWorker
{
    public void Work()
    {
        Debug.Log("Robot working");
    }

    // Wrong: Robot forced to implement methods it doesn't need
    public void Eat()
    {
        throw new NotImplementedException("Robots don't eat");
    }

    public void Sleep()
    {
        throw new NotImplementedException("Robots don't sleep");
    }

    public void GetPaid()
    {
        throw new NotImplementedException("Robots don't get paid");
    }
}

public interface IUnitActions
{
    void Move();
    void Attack();
    void Defend();
    void Heal();
    void Cast();
}

public class Warrior : IUnitActions
{
    public void Move() { }
    public void Attack() { }
    public void Defend() { }
    
    // Wrong: Warrior forced to implement abilities it doesn't have
    public void Heal()
    {
        throw new NotImplementedException();
    }

    public void Cast()
    {
        throw new NotImplementedException();
    }
}
```

#### ✅ Do - Split Into Smaller, Focused Interfaces

```csharp
public interface IWorkable
{
    void Work();
}

public interface IPayable
{
    void GetPaid();
}

public interface IBiologicalNeeds
{
    void Eat();
    void Sleep();
}

public class Human : IWorkable, IPayable, IBiologicalNeeds
{
    public void Work()
    {
        Debug.Log("Human working");
    }

    public void GetPaid()
    {
        Debug.Log("Human getting paid");
    }

    public void Eat()
    {
        Debug.Log("Human eating");
    }

    public void Sleep()
    {
        Debug.Log("Human sleeping");
    }
}

public class Robot : IWorkable
{
    public void Work()
    {
        Debug.Log("Robot working");
    }
}
```

```csharp
public interface IMovable
{
    void Move();
}

public interface ICombatant
{
    void Attack();
    void Defend();
}

public interface IHealer
{
    void Heal();
}

public interface ISpellCaster
{
    void Cast();
}

public class Warrior : IMovable, ICombatant
{
    public void Move() { }
    public void Attack() { }
    public void Defend() { }
}

public class Mage : IMovable, ICombatant, ISpellCaster
{
    public void Move() { }
    public void Attack() { }
    public void Defend() { }
    public void Cast() { }
}

public class Cleric : IMovable, ICombatant, IHealer, ISpellCaster
{
    public void Move() { }
    public void Attack() { }
    public void Defend() { }
    public void Heal() { }
    public void Cast() { }
}
```

### 6. Dependency Inversion Principle (DIP) with VContainer

#### ❌ Don't - Depend on Concrete Classes

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Wrong: direct dependency on concrete class
    private FileLogger _logger;
    private MySqlDatabase _database;
    private AudioManager _audioManager;

    void Awake()
    {
        // Wrong: creating dependencies inside the class
        _logger = new FileLogger();
        _database = new MySqlDatabase();
        _audioManager = FindObjectOfType<AudioManager>();
    }

    public void SavePlayer()
    {
        _logger.Log("Saving player");
        _database.Save("player_data");
        _audioManager.PlaySound("save");
    }
}

public class FileLogger
{
    public void Log(string message)
    {
        Debug.Log(message);
    }
}

public class MySqlDatabase
{
    public void Save(string data)
    {
        Debug.Log($"Saving to MySQL: {data}");
    }
}
```

#### ✅ Do - Depend on Abstractions with VContainer

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;

public interface ILogger
{
    void Log(string message);
}

public interface IDatabase
{
    void Save(string data);
}

public interface IAudioService
{
    void PlaySound(string soundName);
}

public class FileLogger : ILogger
{
    public void Log(string message)
    {
        Debug.Log(message);
    }
}

public class MySqlDatabase : IDatabase
{
    public void Save(string data)
    {
        Debug.Log($"Saving to MySQL: {data}");
    }
}

public class AudioService : IAudioService
{
    public void PlaySound(string soundName)
    {
        Debug.Log($"Playing sound: {soundName}");
    }
}

public class PlayerController : MonoBehaviour
{
    private readonly ILogger _logger;
    private readonly IDatabase _database;
    private readonly IAudioService _audioService;

    [Inject]
    public PlayerController(ILogger logger, IDatabase database, IAudioService audioService)
    {
        _logger = logger;
        _database = database;
        _audioService = audioService;
    }

    public void SavePlayer()
    {
        _logger.Log("Saving player");
        _database.Save("player_data");
        _audioService.PlaySound("save");
    }
}

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ILogger, FileLogger>(Lifetime.Singleton);
        builder.Register<IDatabase, MySqlDatabase>(Lifetime.Singleton);
        builder.Register<IAudioService, AudioService>(Lifetime.Singleton);
        
        builder.RegisterComponentInHierarchy<PlayerController>();
    }
}
```

### 7. Command Query Separation (CQS)

#### ❌ Don't - Method Both Changes State AND Returns Data

```csharp
public class UserManager
{
    private int _loginCount = 0;

    // Wrong: changes state (_loginCount) AND returns data (violates CQS)
    public bool Login(string username, string password)
    {
        _loginCount++; // Changes state
        bool isValid = ValidateCredentials(username, password);
        return isValid; // Returns data
    }

    private bool ValidateCredentials(string username, string password)
    {
        return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
    }
}
```

#### ✅ Do - Separate Commands and Queries

```csharp
public class UserManager
{
    private int _loginCount = 0;

    // Command - changes state, returns void
    public void Login(string username, string password)
    {
        if (IsValidCredentials(username, password))
        {
            IncrementLoginCount();
        }
    }

    // Query - returns data, doesn't change state
    public bool IsValidCredentials(string username, string password)
    {
        return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
    }

    // Query - returns data, doesn't change state
    public int GetLoginCount()
    {
        return _loginCount;
    }

    // Command - changes state, returns void
    private void IncrementLoginCount()
    {
        _loginCount++;
    }
}
```

### 8. Unity: Component References - SerializeField vs GetComponent

#### ❌ Don't - GetComponent for Editor-Time Components

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed;

    private Rigidbody _rigidbody;
    private Animator _animator;

    void Awake()
    {
        // Wrong: using GetComponent for components that exist in editor
        // These should be serialized instead
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        _rigidbody.velocity = Vector3.forward * _speed;
        _animator.SetFloat("Speed", _speed);
    }
}
```

#### ✅ Do - Use SerializeField for Editor Components

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Animator _animator;

    void Update()
    {
        MovePlayer();
        UpdateAnimation();
    }

    private void MovePlayer()
    {
        _rigidbody.velocity = Vector3.forward * _speed;
    }

    private void UpdateAnimation()
    {
        _animator.SetFloat("Speed", _speed);
    }
}
```

#### ✅ Do - GetComponent Only for Runtime Components

```csharp
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private GameObject _weaponPrefab;
    [SerializeField] private Transform _weaponHolder;

    private Weapon _currentWeapon;

    public void EquipWeapon()
    {
        GameObject weaponObject = Instantiate(_weaponPrefab, _weaponHolder);
        
        // Correct: GetComponent for runtime-instantiated object
        _currentWeapon = weaponObject.GetComponent<Weapon>();
    }
}
```

### 9. Unity: Zero Garbage Collection

#### ❌ Don't - Creates Garbage Every Frame

```csharp
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private Rigidbody _rigidbody;

    void Update()
    {
        // Wrong: creates new Vector3 every frame (garbage)
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _rigidbody.velocity = movement * _speed;

        // Wrong: using LINQ (creates garbage)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = enemies.OrderBy(e => Vector3.Distance(transform.position, e.transform.position)).FirstOrDefault();
    }
}
```

#### ✅ Do - Reuse Objects, Avoid LINQ

```csharp
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private Rigidbody _rigidbody;
    
    private Vector3 _movement;

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        _movement.Set(horizontal, 0, vertical);
        _rigidbody.velocity = _movement * _speed;
    }

    private GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < enemies.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, enemies[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemies[i];
            }
        }

        return closest;
    }
}
```

### 10. Unity: Complete Example with All Principles

#### ❌ Don't

```csharp
using UnityEngine;

public class Player : MonoBehaviour
{
    public int health = 100; // Wrong: public field
    [SerializeField] float speed; // Wrong: missing private

    public void Update() // Wrong: public modifier
    {
        // Wrong: GetComponent in Update, new Vector3, multiple responsibilities
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        GetComponent<Rigidbody>().velocity = move * speed;
    }

    // Wrong: multiple responsibilities (damage, animation, death check, destruction)
    public void Hit(int damage)
    {
        health -= damage;
        GetComponent<Animator>().SetTrigger("Hit");

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
```

#### ✅ Do

```csharp
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private int _maxHealth;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Animator _animator;

    [field: SerializeField] public int CurrentHealth { get; private set; }

    private Vector3 _movement;

    void Awake()
    {
        InitializeHealth();
    }

    void Update()
    {
        HandleMovement();
    }

    public void OnButtonClick_Attack()
    {
        PerformAttack();
    }

    public void DamageTakenCallback(int damageAmount)
    {
        ApplyDamage(damageAmount);
    }

    private void InitializeHealth()
    {
        CurrentHealth = _maxHealth;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        UpdateMovement(horizontal, vertical);
    }

    private void UpdateMovement(float horizontal, float vertical)
    {
        _movement.Set(horizontal, 0, vertical);
        _rigidbody.velocity = _movement * _moveSpeed;
    }

    private void ApplyDamage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
        TriggerHitAnimation();
        CheckDeath();
    }

    private void TriggerHitAnimation()
    {
        _animator.SetTrigger("Hit");
    }

    private void CheckDeath()
    {
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void PerformAttack()
    {
    }

    private void Die()
    {
    }

    public bool IsAlive => CurrentHealth > 0;
}
```

## Important Reminders

- Always follow these guidelines to maintain code quality
- If you find code that violates these rules, please fix it
- Focus on writing clean, maintainable, and performant code
- When in doubt, ask for clarification rather than guessing

These guidelines help us maintain a professional, consistent codebase that is easy to understand and maintain for all team members.
