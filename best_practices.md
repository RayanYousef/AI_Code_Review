# Coding Guidelines for AI Code Review Project

## QODO Instructions (Automated Code Review)

**CRITICAL: QODO must review and enforce ALL guidelines in this document. Review EVERY line of code systematically.**

### Review Process
- Review all code changes against these guidelines
- Flag violations with specific inline code comments
- Point to the exact guideline violated with reference to this document
- Use the format: `// ❌ VIOLATION: [Guideline Name] - [Description] (See: best_practices.md#[section])`
- Be strict and comprehensive - do not miss violations
- For each violation, provide a concrete example of how to fix it

### Enforcement Rules
1. **Single Responsibility**: Ensure each function/class has ONE reason to change
2. **SOLID Principles**: Verify all five SOLID principles are followed
3. **Naming Conventions**: Enforce PascalCase, camelCase, _camelCase, and special rules strictly
4. **Code Structure**: Require explicit visibility modifiers, ban `var`, use null conditional operators
5. **Unity Specific**: Enforce SerializeField over GetComponent, zero garbage collection, proper method naming
6. **Command Query Separation**: Commands return void, Queries return data - never both
7. **DRY Principle**: Identify code duplication and suggest extraction
8. **KISS Principle**: Flag unnecessary complexity
9. **Performance**: Ban LINQ in loops, enforce NonAlloc methods, cache references

### Comment on Every Violation
- Add inline code comments for violations
- Suggest the correct approach
- Reference the specific example from this document

---

## Violation Checklist

Review this C# code STRICTLY against these rules. Check EVERY violation:

### 1. Type Declarations
- ❌ **VIOLATION**: Using `var` instead of explicit type
- ✅ **REQUIRED**: Always use explicit types (e.g., `int count`, not `var count`)

### 2. Naming Conventions
- ❌ **VIOLATION**: Wrong naming convention
- ✅ **REQUIRED**:
  - Classes, Methods, Public Fields, Public Properties, Namespaces: **PascalCase** (e.g., `PlayerController`, `MaxHealth`)
  - Private/Protected/Internal Fields: **_camelCase** (e.g., `_health`, `_rigidbody`)
  - Local Variables, Parameters: **camelCase** (e.g., `playerName`, `damageAmount`)
  - Boolean properties: **Is/Has/Did** prefix (e.g., `IsAlive`, `HasWeapon`, `DidComplete`)
  - Events: **On** prefix (e.g., `OnValueChanged`)
  - Event listeners: **Callback** suffix (e.g., `ValueChangedCallback`)
  - Button methods: **OnButtonClick_** prefix (e.g., `OnButtonClick_StartGame`)
  - Interfaces: **I** prefix (e.g., `IWeapon`)

### 3. Single Responsibility Principle
- ❌ **VIOLATION**: Function has multiple responsibilities
- ✅ **REQUIRED**: Each function must have only ONE responsibility. If a function does multiple things, split it.

### 4. DRY Principle (Don't Repeat Yourself)
- ❌ **VIOLATION**: Code duplication found
- ✅ **REQUIRED**: Extract duplicated code into reusable methods or classes

### 5. Command Query Separation (CQS)
- ❌ **VIOLATION**: Method both modifies state AND returns data
- ✅ **REQUIRED**: Methods must either:
  - **Command**: Modify state, return `void`
  - **Query**: Return data, do NOT modify state
  - Never both!

### 6. Explicit Visibility Modifiers
- ❌ **VIOLATION**: Missing visibility modifier
- ✅ **REQUIRED**: Always specify `private`, `public`, `protected`, etc. (even when it's the default)
- ✅ **REQUIRED**: Visibility modifier must be first: `public static` not `static public`

### 7. Boolean Naming
- ❌ **VIOLATION**: Boolean doesn't start with Is/Has/Did
- ✅ **REQUIRED**: Boolean properties/methods must start with `Is`, `Has`, or `Did` (e.g., `IsEnabled`, `HasValue`, `DidComplete`)

### 8. Event Naming
- ❌ **VIOLATION**: Event doesn't follow naming convention
- ✅ **REQUIRED**: Events must have **On** prefix (e.g., `OnValueChanged`)
- ✅ **REQUIRED**: Event listener methods must end with **Callback** (e.g., `ValueChangedCallback`)

### 9. Button Methods
- ❌ **VIOLATION**: Button method doesn't start with OnButtonClick_
- ✅ **REQUIRED**: Editor button methods must start with `OnButtonClick_` (e.g., `OnButtonClick_StartGame`)

### 10. Unity MonoBehaviour Methods
- ❌ **VIOLATION**: Unity methods (Awake, Start, Update) are public
- ✅ **REQUIRED**: Unity lifecycle methods (Awake, Start, Update, etc.) must be `private` or have no modifier (NOT `public`)

### 11. LINQ in Update Loops
- ❌ **VIOLATION**: LINQ used in Update() or frequently called methods
- ✅ **REQUIRED**: Use `for` loops instead of LINQ in Update() or frequently called methods (LINQ creates garbage)
- ✅ **REQUIRED**: Use `Physics.RaycastNonAlloc` instead of `Physics.Raycast`

### 12. GetComponent Caching
- ❌ **VIOLATION**: GetComponent called in Update() or frequently called methods
- ✅ **REQUIRED**: Cache GetComponent references in `Awake()` or `Start()`, never in `Update()`
- ✅ **REQUIRED**: Prefer `[SerializeField]` over `GetComponent()` for editor-time components

### 13. Serialization
- ❌ **VIOLATION**: Incorrect SerializeField usage
- ✅ **REQUIRED**: Use `[SerializeField] private` format: `[SerializeField] private int _health;`
- ✅ **REQUIRED**: Always write `private` with `[SerializeField]`

### 14. SOLID Principles

#### Single Responsibility Principle (SRP)
- ❌ **VIOLATION**: Class/function has multiple responsibilities
- ✅ **REQUIRED**: Each class/function must have ONE reason to change

#### Open/Closed Principle (OCP)
- ❌ **VIOLATION**: Using switch/if-else chains for type checking
- ✅ **REQUIRED**: Use polymorphism and abstraction instead of switch/if-else chains

#### Liskov Substitution Principle (LSP)
- ❌ **VIOLATION**: Derived class changes expected behavior of base class
- ✅ **REQUIRED**: Subtypes must be substitutable for their base types without breaking functionality

#### Interface Segregation Principle (ISP)
- ❌ **VIOLATION**: Fat interface forces unused implementation
- ✅ **REQUIRED**: Split large interfaces into smaller, focused ones

#### Dependency Inversion Principle (DIP)
- ❌ **VIOLATION**: Depend on concrete classes instead of abstractions
- ✅ **REQUIRED**: Depend on abstractions (interfaces), not concretions. Use VContainer for dependency injection

### 15. Performance (Zero Garbage Collection)
- ❌ **VIOLATION**: Creating garbage in Update() or frequently called methods
- ✅ **REQUIRED**: 
  - Avoid LINQ - use for-loops
  - Use NonAlloc methods: `Physics.RaycastNonAlloc` instead of `Physics.Raycast`
  - Cache strings and avoid string concatenation in loops
  - Reuse Vector3/Vector2 objects instead of creating new ones

### 16. Code Structure
- ❌ **VIOLATION**: Using `this.` unnecessarily
- ✅ **REQUIRED**: Avoid `this.` unless necessary for clarity

- ❌ **VIOLATION**: Not using null conditional operator for delegates/events
- ✅ **REQUIRED**: Use `SomeDelegate?.Invoke()` for delegates/events

### 17. Method Naming
- ❌ **VIOLATION**: Simple one-word method name
- ✅ **REQUIRED**: Use descriptive names (e.g., `StartInteraction` instead of `Interact`)

### 18. Component References
- ❌ **VIOLATION**: Using GetComponent for editor-time components
- ✅ **REQUIRED**: Prefer `[SerializeField]` over `GetComponent()` for components that exist in editor
- ✅ **REQUIRED**: Only use `GetComponent()` for runtime-instantiated objects

### 19. Variable Naming by Type
- ❌ **VIOLATION**: Variable name doesn't reflect its type
- ✅ **REQUIRED**: Name variables based on their type: `_levelManager`, `_rigidbody`, `_animator`

### 20. KISS Principle
- ❌ **VIOLATION**: Unnecessary complexity
- ✅ **REQUIRED**: Write simple, clear code. Prefer straightforward solutions over clever ones

---

## Output Format for Violations

For each violation found, output:

```
VIOLATION: [Category Name]
LINE(S): [line numbers]
ISSUE: [explanation of the violation]
SOLUTION: [corrected code example]
GUIDELINE: best_practices.md#[section-name]
```

---

## Important Reminders

- Always follow these guidelines to maintain code quality
- If you find code that violates these rules, please fix it
- Focus on writing clean, maintainable, and performant code
- When in doubt, ask for clarification rather than guessing

These guidelines help us maintain a professional, consistent codebase that is easy to understand and maintain for all team members.
