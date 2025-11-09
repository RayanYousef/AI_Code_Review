# Coding Guidelines - Compact AI Version

## Instructions for AI Reviewers
Enforce ALL rules in this document. Check every section. Flag all violations with line numbers, explanations, corrected code, and section references.

---

## General Principles

### SOLID
- **SRP**: One class = one responsibility
- **OCP**: Open for extension, closed for modification. Avoid switch/if-else chains.
- **LSP**: Subtypes must be substitutable without breaking behavior
- **ISP**: Split fat interfaces into smaller ones
- **DIP**: Depend on abstractions (interfaces), use VContainer

### Other Principles
- **KISS**: Simple, clear code. Avoid complexity.
- **CQS**: Commands change state (return void). Queries return data (no state change). Never both.
- **DRY**: No duplication. Extract common functionality.
- **Function Design**: One responsibility per function. Split if multiple tasks.

## C# Coding Style

### Naming
- **PascalCase**: Classes, Methods, Enums, Public Fields/Properties, Namespaces
- **camelCase**: Local variables, Parameters
- **_camelCase**: Private/Protected/Internal Fields/Properties
- **Interfaces**: Start with 'I' (ICalculator)
- **Booleans**: IsEnabled, HasValue, DidComplete
- **Events**: OnValueChanged
- **Callbacks**: ValueChangedCallback
- **Acronyms**: MyRpc (not MyRPC)

### Code Structure
- Always specify visibility modifiers
- Visibility first: `public static` not `static public`
- Avoid `this.` unless needed
- Avoid `var` - use explicit types
- Use `?.Invoke()` for delegates/events

## Unity Guidelines

### Component References
- **Prefer `[SerializeField]` over `GetComponent()`** for editor-time components
- `GetComponent()` only for runtime-created components
- Cache `GetComponent()` in Awake/Start, never in Update
- Name by type: `_rigidbody`, `_animator`, `_levelManager`

### Serialization
- `[SerializeField] private int _health;` (always write `private`)
- Prefer auto-properties: `public int PageNumber { get; set; }`

### Method Naming
- Editor buttons: `OnButtonClick_StartGame`
- Avoid one-word methods: Use `StartInteraction` not `Interact`

### Performance (Zero GC)
- **No LINQ** - use for-loops
- Use NonAlloc methods: `Physics.RaycastNonAlloc`
- Cache strings, avoid concatenation in loops

### MonoBehaviour
- No `public` on Start/Update/etc. Use `private` or no modifier.

## File Organization

### Naming
- Files/Directories: PascalCase (MyClass.cs, MyFolder/)
- One main class per file
- File name = class name

### Using Statements
- Top of file, before namespace
- System first, then alphabetical

### Member Ordering
1. Nested classes, enums, delegates, events
2. Static, const, readonly fields
3. Fields and properties
4. Constructors/destructors
5. Methods

### Access Modifier Ordering (within groups)
1. Public 2. Internal 3. Protected internal 4. Protected 5. Private

## Code Examples - Violation Patterns

### Naming Violations
❌ `namespace myNamespace`, `public interface Calculator`, `int calculate(float Value)`, `private bool isCalculating`, `public static int times_used`, `private const int MAX_VALUE`, `public bool Enabled()`
✅ `namespace MyNamespace`, `public interface ICalculator`, `int Calculate(float value)`, `private bool _isCalculating`, `public static int TimesUsed`, `private const int _maxValue`, `public bool IsEnabled()`

### SRP Violation
❌ Method does multiple things (validate, calculate, log, save, notify)
✅ Split into separate methods: `IsValidInput()`, `Calculate()`, `SaveResult()`, `NotifyProcessed()`, `LogError()`

### OCP Violation
❌ Switch/if-else chains: `switch (weaponType) { case "Sword": ... }`
✅ Use interfaces/polymorphism: `IWeapon` interface, `Sword : IWeapon`, `CalculateDamage(IWeapon weapon)`

### LSP Violation
❌ `Penguin : Bird` with `Fly()` throwing exception, or `Square : Rectangle` with side effects
✅ Proper hierarchy: `FlyingBird : Bird`, `Penguin : Bird` with `Swim()`, or separate `IShape` interface

### ISP Violation
❌ Fat interface: `IWorker` with `Work()`, `Eat()`, `Sleep()`, `GetPaid()` forcing `Robot` to implement unused methods
✅ Split: `IWorkable`, `IPayable`, `IBiologicalNeeds` - implement only needed

### DIP Violation
❌ Direct concrete dependencies: `new FileLogger()`, `FindObjectOfType<AudioManager>()`
✅ Interfaces + VContainer: `ILogger`, `IDatabase`, `[Inject]` constructor

### CQS Violation
❌ `public bool Login(...)` that changes state AND returns bool
✅ Separate: `public void Login(...)` (command) and `public bool IsValidCredentials(...)` (query)

### Unity Violations
❌ `GetComponent<Rigidbody>()` in Awake for editor components
✅ `[SerializeField] private Rigidbody _rigidbody;`

❌ `new Vector3(...)` every frame, LINQ in Update
✅ Reuse `_movement` field, use for-loops instead of LINQ

❌ `public void Update()`, `public int health`
✅ `void Update()`, `[SerializeField] private int _maxHealth;` with property

---

**Enforce all rules above. Check every violation pattern. Reference specific sections when flagging issues.**
