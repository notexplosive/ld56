using ExplogineMonoGame.AssetManagement;
using LD56.CartridgeManagement;

namespace LD56.Gameplay;

public static class Constants
{
    public static SpriteSheet OreSheet => LdResourceAssets.Instance.Sheets["ore"];

    public static SpriteSheet WorkerBodySheet => LdResourceAssets.Instance.Sheets["worker (body)"];
    public static SpriteSheet WorkerPickaxeSheet => LdResourceAssets.Instance.Sheets["worker (pickaxe)"];
    public static SpriteSheet WorkerNuggetSheet => LdResourceAssets.Instance.Sheets["worker (nugget)"];

    public static IFrameAnimation WorkerIdleAnimation => LdResourceAssets.Instance.Animations["idle"];
    public static IFrameAnimation WorkerWalkAnimation => LdResourceAssets.Instance.Animations["walk"];
    public static IFrameAnimation WorkerHurtAnimation => LdResourceAssets.Instance.Animations["hurt"];
}
