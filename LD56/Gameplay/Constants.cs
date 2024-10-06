using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD56.Gameplay;

public static class Constants
{
    public static ImageAsset CircleImage => LdResourceAssets.Instance.Sheets["circle"].GetImageAtFrame(0);

    public static void DrawCircle(Painter painter, Vector2 position, float radius)
    {
        Constants.CircleImage.DrawAsRectangle(painter,
            new RectangleF(position, new Vector2(radius * 2)),
            new DrawSettings
            {
                Origin = DrawOrigin.Center
            });
    }
}
