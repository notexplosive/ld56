using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework.Graphics;

namespace LD56.Gameplay;

public static class Constants
{
    public static ImageAsset CircleImage => LdResourceAssets.Instance.Sheets["circle"].GetImageAtFrame(0);
}
