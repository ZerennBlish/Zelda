using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    // --- Action Definitions ---
    private InputAction moveAction;
    private InputAction aimAction;
    private InputAction attackAction;
    private InputAction shootAction;
    private InputAction useSubWeaponAction;
    private InputAction cycleWeaponAction;
    private InputAction scrollWeaponAction;
    private InputAction mountToggleAction;
    private InputAction interactAction;
    private InputAction blockAction;
    private InputAction toggleMinimapAction;
    private InputAction pauseAction;
    private InputAction escapeAction;
    private InputAction shopBuy1Action;
    private InputAction shopBuy2Action;
    private InputAction shopBuy3Action;
    private InputAction debugRefillAction;
    private InputAction debugResetAction;
    private InputAction debugCycleClassAction;

    // --- Public Read Properties ---

    // Vector2 — read every frame
    public Vector2 Move => moveAction.ReadValue<Vector2>();
    public Vector2 AimScreenPosition => aimAction.ReadValue<Vector2>();

    // Buttons — single frame press
    public bool AttackPressed => attackAction.WasPressedThisFrame();
    public bool UseSubWeaponPressed => useSubWeaponAction.WasPressedThisFrame();
    public bool CycleWeaponPressed => cycleWeaponAction.WasPressedThisFrame();
    public bool MountTogglePressed => mountToggleAction.WasPressedThisFrame();
    public bool InteractPressed => interactAction.WasPressedThisFrame();
    public bool ToggleMinimapPressed => toggleMinimapAction.WasPressedThisFrame();
    public bool PausePressed => pauseAction.WasPressedThisFrame();
    public bool EscapePressed => escapeAction.WasPressedThisFrame();
    public bool ShopBuy1Pressed => shopBuy1Action.WasPressedThisFrame();
    public bool ShopBuy2Pressed => shopBuy2Action.WasPressedThisFrame();
    public bool ShopBuy3Pressed => shopBuy3Action.WasPressedThisFrame();
    public bool DebugRefillPressed => debugRefillAction.WasPressedThisFrame();
    public bool DebugResetPressed => debugResetAction.WasPressedThisFrame();
    public bool DebugCycleClassPressed => debugCycleClassAction.WasPressedThisFrame();

    // Buttons — held (true every frame while pressed)
    public bool ShootHeld => shootAction.IsPressed();
    public bool BlockHeld => blockAction.IsPressed();

    // Axis — scroll wheel (returns float, positive = up, negative = down)
    public float ScrollWeapon => scrollWeaponAction.ReadValue<float>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CreateActions();
        EnableAllActions();
    }

    void CreateActions()
    {
        // Movement — WASD and Arrow Keys (composite), Left Stick
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick");

        // Aim — Mouse screen position
        aimAction = new InputAction("Aim", InputActionType.Value, "<Mouse>/position");

        // Attack — Space, Left Mouse Button, Gamepad West (X on Xbox)
        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Keyboard>/space");
        attackAction.AddBinding("<Mouse>/leftButton");
        attackAction.AddBinding("<Gamepad>/buttonWest");

        // Shoot (held for rapid fire) — F, Middle Mouse, Gamepad Right Shoulder
        shootAction = new InputAction("Shoot", InputActionType.Button);
        shootAction.AddBinding("<Keyboard>/f");
        shootAction.AddBinding("<Mouse>/middleButton");
        shootAction.AddBinding("<Gamepad>/rightShoulder");

        // Use Sub-Weapon — Right Mouse Button, Gamepad North (Y on Xbox)
        useSubWeaponAction = new InputAction("UseSubWeapon", InputActionType.Button);
        useSubWeaponAction.AddBinding("<Mouse>/rightButton");
        useSubWeaponAction.AddBinding("<Gamepad>/buttonNorth");

        // Cycle Weapon — I key
        cycleWeaponAction = new InputAction("CycleWeapon", InputActionType.Button, "<Keyboard>/i");

        // Scroll Weapon — Mouse Scroll Y
        scrollWeaponAction = new InputAction("ScrollWeapon", InputActionType.Value, "<Mouse>/scroll/y");

        // Mount Toggle — M, Gamepad Select/Back
        mountToggleAction = new InputAction("MountToggle", InputActionType.Button);
        mountToggleAction.AddBinding("<Keyboard>/m");
        mountToggleAction.AddBinding("<Gamepad>/select");

        // Interact — E key
        interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");

        // Block (held) — Left Shift, Gamepad East (B on Xbox)
        blockAction = new InputAction("Block", InputActionType.Button);
        blockAction.AddBinding("<Keyboard>/leftShift");
        blockAction.AddBinding("<Gamepad>/buttonEast");

        // Toggle Minimap — Tab
        toggleMinimapAction = new InputAction("ToggleMinimap", InputActionType.Button, "<Keyboard>/tab");

        // Pause — P, Gamepad Start
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/p");
        pauseAction.AddBinding("<Gamepad>/start");

        // Escape — Escape key
        escapeAction = new InputAction("Escape", InputActionType.Button, "<Keyboard>/escape");

        // Shop buy keys — 1, 2, 3
        shopBuy1Action = new InputAction("ShopBuy1", InputActionType.Button, "<Keyboard>/1");
        shopBuy2Action = new InputAction("ShopBuy2", InputActionType.Button, "<Keyboard>/2");
        shopBuy3Action = new InputAction("ShopBuy3", InputActionType.Button, "<Keyboard>/3");

        // Debug keys — O, R, T
        debugRefillAction = new InputAction("DebugRefill", InputActionType.Button, "<Keyboard>/o");
        debugResetAction = new InputAction("DebugReset", InputActionType.Button, "<Keyboard>/r");
        debugCycleClassAction = new InputAction("DebugCycleClass", InputActionType.Button, "<Keyboard>/t");
    }

    void EnableAllActions()
    {
        moveAction.Enable();
        aimAction.Enable();
        attackAction.Enable();
        shootAction.Enable();
        useSubWeaponAction.Enable();
        cycleWeaponAction.Enable();
        scrollWeaponAction.Enable();
        mountToggleAction.Enable();
        interactAction.Enable();
        blockAction.Enable();
        toggleMinimapAction.Enable();
        pauseAction.Enable();
        escapeAction.Enable();
        shopBuy1Action.Enable();
        shopBuy2Action.Enable();
        shopBuy3Action.Enable();
        debugRefillAction.Enable();
        debugResetAction.Enable();
        debugCycleClassAction.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        aimAction?.Disable();
        attackAction?.Disable();
        shootAction?.Disable();
        useSubWeaponAction?.Disable();
        cycleWeaponAction?.Disable();
        scrollWeaponAction?.Disable();
        mountToggleAction?.Disable();
        interactAction?.Disable();
        blockAction?.Disable();
        toggleMinimapAction?.Disable();
        pauseAction?.Disable();
        escapeAction?.Disable();
        shopBuy1Action?.Disable();
        shopBuy2Action?.Disable();
        shopBuy3Action?.Disable();
        debugRefillAction?.Disable();
        debugResetAction?.Disable();
        debugCycleClassAction?.Disable();
    }

    void OnDestroy()
    {
        moveAction?.Dispose();
        aimAction?.Dispose();
        attackAction?.Dispose();
        shootAction?.Dispose();
        useSubWeaponAction?.Dispose();
        cycleWeaponAction?.Dispose();
        scrollWeaponAction?.Dispose();
        mountToggleAction?.Dispose();
        interactAction?.Dispose();
        blockAction?.Dispose();
        toggleMinimapAction?.Dispose();
        pauseAction?.Dispose();
        escapeAction?.Dispose();
        shopBuy1Action?.Dispose();
        shopBuy2Action?.Dispose();
        shopBuy3Action?.Dispose();
        debugRefillAction?.Dispose();
        debugResetAction?.Dispose();
        debugCycleClassAction?.Dispose();

        if (Instance == this) Instance = null;
    }
}
