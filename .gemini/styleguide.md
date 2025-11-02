# AI Code Review - C# & Unity Coding Style Guide

This style guide defines the coding standards for the AI Code Review project. All code must strictly adhere to these guidelines.

## Core Design Principles

### Single Responsibility Principle (SRP)
- **Every function must have exactly one responsibility**
- If a function handles multiple tasks, split it into smaller, focused functions
- Each function should be focused on a single purpose
- Methods should either do something OR return something, not both (Command Query Separation)

### Open/Closed Principle (OCP)
- Classes should be open for extension but closed for modification
- Avoid switch/if-else chains for type checking
- Use polymorphism and abstraction instead

### Liskov Substitution Principle (LSP)
- Subtypes must be substitutable for their base types without breaking functionality
- Derived classes must not change the expected behavior of base classes

### Interface Segregation Principle (ISP)
- Clients should not be forced to depend on interfaces they don't use
- Split fat interfaces into smaller, focused ones

### Dependency Inversion Principle (DIP)
- Depend on abstractions, not concretions
- Use VContainer for dependency injection in Unity projects

### KISS Principle (Keep It Simple, Stupid)
- Write simple, clear code that is easy to understand
- Avoid unnecessary complexity
- Prefer straightforward solutions over clever ones

### DRY Principle (Don't Repeat Yourself)
- Avoid code duplication
- Extract common functionality into reusable methods or classes
- Use inheritance, composition, or utility classes when appropriate

### Command and Query Separation (CQS)
- **Commands change state and return void**
- **Queries return data and do not change state**
- Methods should either do something OR return something, not both

## C# Coding Style

### Naming Conventions

#### Class, Method, Enum, Public Members
- Use **PascalCase** for: Classes, Methods, Enumerations, Public Fields, Public Properties, Namespaces
- Example: `public class PlayerController`, `public void UpdateHealth()`, `public enum OperationType`

#### Variables and Parameters
- Use **camelCase** for local variables and parameters
- Example: `int playerHealth`, `string weaponName`

#### Private and Protected Members
- Use **_camelCase** (underscore prefix) for private, protected, internal fields and properties
- Example: `private int _maxHealth`, `protected bool _isInitialized`

#### Boolean Members
- Boolean properties should start with: `Is`, `Has`, `Did`, `Can`, `Should`
- Example: `public bool IsEnabled`, `public bool HasValue`, `private bool _didComplete`

#### Events and Callbacks
- Events should be prefixed with "On": `public event Action OnValueChanged`
- Event listener methods end with "Callback": `private void ValueChangedCallback()`

#### Constants
- Use **PascalCase** for constants
- Example: `private const int MaxHealth = 100`

#### Acronyms
- Treat acronyms as single words: `MyRpc` (not MyRPC)

#### Interfaces
- Interface names start with 'I': `public interface ICalculator`

### Code Structure Rules

- **ALWAYS specify visibility modifiers** (private, public, protected, internal), even when it's the default
- Visibility modifier should be first: `public static` not `static public`
- **Avoid `this.` unless necessary for clarity**
- **Never use `var` - always use explicit types**
- Use null conditional operator for delegates/events: `SomeDelegate?.Invoke()`

## Unity Specific Guidelines

### Component References

**Prefer SerializeField over GetComponent()**
- Serialize component references in the inspector whenever possible
- Only use `GetComponent()` for components created at runtime or cannot be serialized
- **Cache GetComponent() references in Awake() or Start(), NEVER in Update()**
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

- **Avoid LINQ** - use for-loops instead (LINQ creates garbage)
- Use NonAlloc methods: `Physics.RaycastNonAlloc` instead of `Physics.Raycast`
- Cache strings and avoid string concatenation in loops
- Reuse objects instead of creating new ones in Update() methods
- Reference: https://docs.unity3d.com/Manual/performance-garbage-collection-best-practices.html

### MonoBehaviour Methods

- Don't use public access modifiers for Unity-specific methods (Start, Update, Awake, etc.)
- Keep these methods private or use no access modifier

## File Organization

### File Naming

- Files and directories use **PascalCase**: `MyClass.cs`, `MyFolder/`
- Prefer one main class per file
- File names should match the main class name

### Using Statements

- All using statements go at the top, before namespace
- System imports come first, then in alphabetical order

### Class Member Ordering

Group members in this order:

1. Nested classes, enums, delegates, events
2. Static, const, readonly fields
3. Fields and properties
4. Constructors and destructors
5. Methods

Within each group, order by accessibility: Public → Internal → Protected Internal → Protected → Private

## Critical Rules Violations to Flag

### MUST NOT:
- ❌ Use `var` - always explicit type
- ❌ Use GetComponent() in Update() or loops
- ❌ Create functions with multiple responsibilities
- ❌ Use `public` for MonoBehaviour Unity methods (Start, Update, Awake, etc.)
- ❌ Mix commands and queries in the same method
- ❌ Create LINQ expressions (garbage collection)
- ❌ Use public fields without properties
- ❌ Use incorrect naming conventions (camelCase for types, PascalCase for locals)
- ❌ Skip visibility modifiers
- ❌ Use switch/if-else for type checking (violates OCP)
- ❌ Throw NotImplementedException for forced implementations (violates ISP)
- ❌ Break Liskov Substitution by changing base behavior

### MUST DO:
- ✅ Use explicit types everywhere
- ✅ Specify all visibility modifiers
- ✅ Follow PascalCase for types/methods, camelCase for locals/parameters, _camelCase for private
- ✅ One responsibility per function
- ✅ Cache GetComponent() in Awake/Start
- ✅ Use SerializeField for editor components
- ✅ Use interfaces and dependency injection
- ✅ Separate commands (void) from queries (return value)
- ✅ Use for-loops instead of LINQ
- ✅ Use NonAlloc physics methods

## Examples

### ❌ Wrong - Multiple Violations

```csharp
public class DataProcessor
{
    // VIOLATIONS: var, multiple responsibilities, public Update, GetComponent in Update
    public void Update()
    {
        var rb = GetComponent<Rigidbody>();
        var health = 100;
        
        if (health > 0)
        {
            rb.velocity = Vector3.forward;
        }
        
        // Multiple responsibilities: validation, calculation, logging
        ProcessData(5.0f);
    }
    
    public int ProcessData(float value)
    {
        if (value <= 0) Console.WriteLine("Invalid");
        int result = (int)(value * 2);
        File.WriteAllText("log.txt", result.ToString());
        return result;
    }
}
```

### ✅ Correct - All Principles Applied

```csharp
public class DataProcessor : MonoBehaviour
{
    [SerializeField] private float _multiplier;
    [SerializeField] private Rigidbody _rigidbody;
    
    private int _currentHealth;
    private const int MaxHealth = 100;
    
    private void Awake()
    {
        InitializeHealth();
    }
    
    private void Update()
    {
        HandleMovement();
    }
    
    private void InitializeHealth()
    {
        _currentHealth = MaxHealth;
    }
    
    private void HandleMovement()
    {
        if (IsAlive())
        {
            ApplyMovement();
        }
    }
    
    private void ApplyMovement()
    {
        _rigidbody.velocity = Vector3.forward * _multiplier;
    }
    
    private bool IsAlive()
    {
        return _currentHealth > 0;
    }
    
    private int CalculateResult(float value)
    {
        return (int)(value * _multiplier);
    }
    
    public void TakeDamageCallback(int damageAmount)
    {
        ApplyDamage(damageAmount);
    }
    
    private void ApplyDamage(int damageAmount)
    {
        _currentHealth -= damageAmount;
        CheckDeath();
    }
    
    private void CheckDeath()
    {
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        gameObject.SetActive(false);
    }
}
```

## Enforcement

Gemini Code Assist will:
1. Flag ALL violations of these rules
2. Provide specific line numbers and explanations
3. Suggest corrected code following these guidelines
4. Reference the specific guideline section violated
5. Require LOW severity threshold to catch all issues
