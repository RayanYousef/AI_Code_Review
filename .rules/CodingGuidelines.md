# Coding Guidelines for AI Code Review Project

This document contains the coding standards and best practices for our Unity project. Please follow these guidelines to ensure code quality, consistency, and maintainability.

## Table of Contents
1. [General Principles](#general-principles)
2. [C# Coding Style](#csharp-coding-style)
3. [Unity Specific Guidelines](#unity-specific-guidelines)
4. [File Organization](#file-organization)
5. [Code Examples](#code-examples)

## General Principles

### SOLID Principles
- **Single Responsibility**: Each class should have only one reason to change. One job, one responsibility.
- **Open/Closed**: Classes should be open for extension but closed for modification.
- **Liskov Substitution**: Subtypes must be substitutable for their base types.
- **Interface Segregation**: Clients should not be forced to depend on interfaces they don't use.
- **Dependency Inversion**: Depend on abstractions, not concretions.

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
- Google C# Style Guide: https://google.github.io/styleguide/csharp-style.html
- .NET Runtime Coding Guidelines: https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md

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

### Tools
- Use CodeMaid extension to beautify and clean code
- Download from: https://marketplace.visualstudio.com/items?itemName=SteveCadwallader.CodeMaid

## Unity Specific Guidelines

### Component References
- Always cache component references at Start() or Awake()
- Avoid GetComponent() calls in Update() or frequently called methods
- Use lazy initialization when appropriate (get component once when first needed)
- Name variables based on their type: `_levelManager`, `transformRef`, `rigidbodyComponent`

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
- Reference: https://docs.unity3d.com/Manual/performance-garbage-collection-best-practices.html

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

### Basic C# Class
```csharp
using System; // using statements at top

namespace MyNamespace // PascalCase namespace
{
    public interface ICalculator // Interface starts with I
    {
        int Calculate(float value, float exponent); // PascalCase method
    }

    public enum OperationType // PascalCase enum
    {
        Add,    // PascalCase enum values
        Subtract,
        Multiply
    }

    public class Calculator : ICalculator // PascalCase class
    {
        // Events first
        public event Action<int> OnResultCalculated;

        // Static fields
        public static int TimesUsed = 0;

        // Fields and properties
        public int LastResult { get; private set; } // Auto-implemented property
        private bool _isCalculating = false; // _camelCase private field
        private const int _maxValue = 1000; // const doesn't change naming

        // Constructor
        public Calculator()
        {
            TimesUsed++;
        }

        // Methods
        public int Calculate(float value, float exponent)
        {
            int result = (int)(value * Mathf.Pow(value, exponent)); // camelCase local variables
            LastResult = result;
            OnResultCalculated?.Invoke(result); // Null conditional for events
            return result;
        }

        public bool IsValidOperation(float input) // Boolean starts with Is
        {
            return input > 0 && input <= _maxValue;
        }

        private void LogCalculation(int result) // Always specify private
        {
            Debug.Log($"Calculation result: {result}");
        }

        // Long method parameters - align with first parameter
        private void ProcessData(int firstParameter, int secondParameter,
                                int thirdParameter)
        {
            // Method body
        }

        // If alignment is difficult, use 4-space indent
        private void AnotherLongMethod(
            int veryLongParameterName, int anotherLongName,
            int yetAnotherParameter)
        {
            // Method body
        }
    }
}
```

### Unity Class Example
```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Serialized fields
    [SerializeField] private float _moveSpeed;
    [SerializeField] private int _maxHealth;

    // Auto-implemented property with serialization
    [field: SerializeField] public int CurrentHealth { get; private set; }

    // Cached components (lazy initialization example)
    private Rigidbody _rigidbody;
    private Animator _animator;

    // Unity methods - no public modifier
    void Awake()
    {
        // Cache components at start
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        CurrentHealth = _maxHealth;
    }

    void Update()
    {
        // Use cached components, don't call GetComponent here
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        _rigidbody.velocity = movement * _moveSpeed;
    }

    // Button click method
    public void OnButtonClick_Attack()
    {
        // Handle button feedback here
        Debug.Log("Attack button pressed");

        // Call separate method for logic
        PerformAttack();
    }

    // Event callback method
    public void DamageTakenCallback(int damageAmount)
    {
        CurrentHealth -= damageAmount;
        _animator.SetTrigger("TakeDamage");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    // Private methods for logic
    private void PerformAttack()
    {
        // Attack logic here
        Debug.Log("Performing attack");
    }

    private void Die()
    {
        // Death logic here
        Debug.Log("Player died");
    }

    // Property for external access
    public bool IsAlive => CurrentHealth > 0;
}
```

## Important Reminders

- Always follow these guidelines to maintain code quality
- If you find code that violates these rules, please fix it
- Use the CodeMaid extension to help format your code
- Focus on writing clean, maintainable, and performant code
- When in doubt, ask for clarification rather than guessing

These guidelines help us maintain a professional, consistent codebase that is easy to understand and maintain for all team members.
