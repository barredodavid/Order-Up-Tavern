/// <summary>
/// Constantes du jeu pour éviter les "magic numbers"
/// Utilisation : GameConstants.TAG_PLAYER au lieu de "Player"
/// </summary>
public static class GameConstants
{
    // ═══════════════════════════════════════════
    // 🏷️ TAGS
    // ═══════════════════════════════════════════
    public const string TAG_PLAYER = "Player";
    public const string TAG_CUSTOMER = "Customer";
    public const string TAG_INTERACTABLE = "Interactable";
    
    // ═══════════════════════════════════════════
    // 📊 LAYERS
    // ═══════════════════════════════════════════
    public const int LAYER_DEFAULT = 0;
    public const int LAYER_IGNORE_RAYCAST = 2;
    public const int LAYER_UI = 5;
    public const int LAYER_INTERACTION = 8;
    
    // ═══════════════════════════════════════════
    // 📏 DISTANCES
    // ═══════════════════════════════════════════
    public const float INTERACTION_DISTANCE = 3.0f;
    public const float CLEANING_RADIUS = 1.5f;
    public const float DOOR_OPEN_DISTANCE = 3.0f;
    public const float SWEEP_RADIUS = 1.0f;
    
    // ═══════════════════════════════════════════
    // ⏱️ DURÉES (en secondes)
    // ═══════════════════════════════════════════
    public const float BEER_FILL_DURATION = 2.0f;
    public const float WASH_DURATION = 1.5f;
    public const float DRINKING_DURATION = 4.0f;
    public const float SWEEP_COOLDOWN = 1.0f;
    
    // ═══════════════════════════════════════════
    // 🎮 GAMEPLAY
    // ═══════════════════════════════════════════
    public const int DEFAULT_DRINK_PRICE = 5;
    public const float DEFAULT_CUSTOMER_PATIENCE = 20.0f;
    public const int STARTING_MONEY = 0;
    
    // ═══════════════════════════════════════════
    // 🎨 ANIMATIONS
    // ═══════════════════════════════════════════
    public const string ANIM_SPEED = "Speed";
    public const string ANIM_IS_WALKING = "IsWalking";
    public const string ANIM_IS_DRINKING = "IsDrinking";
    public const string ANIM_HOLD_BROOM = "HoldBroom";
    public const string ANIM_HOLD_GLASS = "HoldGlass";
    public const string ANIM_SWEEP = "Sweep";
    public const string ANIM_WASH = "Wash";
    
    // ═══════════════════════════════════════════
    // 📦 PREFAB PATHS (si tu utilises Resources)
    // ═══════════════════════════════════════════
    public const string PATH_GLASS_PREFAB = "Prefabs/Items/Glass";
    public const string PATH_DIRTY_GLASS_PREFAB = "Prefabs/Items/DirtyGlass";
    public const string PATH_BUBBLE_EFFECT = "Prefabs/Effects/BubbleEffect";
    
    // ═══════════════════════════════════════════
    // 🎯 TUTORIEL
    // ═══════════════════════════════════════════
    public const string TUTORIAL_STEP_CLEANING = "Cleaning";
    public const string TUTORIAL_STEP_TAKE_GLASS = "TakeGlass";
    public const string TUTORIAL_STEP_FILL_BEER = "FillBeer";
    public const string TUTORIAL_STEP_SERVE = "Serve";
    public const string TUTORIAL_STEP_WASH = "Wash";
    
    // ═══════════════════════════════════════════
    // 🎨 UI
    // ═══════════════════════════════════════════
    public const float UI_MESSAGE_DURATION = 3.0f;
    public const float MONEY_ANIM_DURATION = 0.15f;
    public const float FLOATING_TEXT_DURATION = 1.5f;
    
    // ═══════════════════════════════════════════
    // 🎵 AUDIO
    // ═══════════════════════════════════════════
    public const float AUDIO_SPATIAL_BLEND_3D = 1.0f;
    public const float AUDIO_SPATIAL_BLEND_2D = 0.0f;
}