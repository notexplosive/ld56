using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Credits : Entity
{
    private float _fader;

    public override void Draw(Painter painter)
    {
    }

    public override void Update(float dt)
    {
        _fader += dt;
    }

    public override bool EditorHitTest(Vector2 mousePosition)
    {
        return false;
    }

    public void DrawUi(Painter painter, RectangleF cameraRectangle)
    {
        painter.DrawStringWithinRectangle(Client.Assets.GetFont("fishbone/font", 120), "fishbone", cameraRectangle.Moved(new Vector2(0, -300)),
            Alignment.Center, new DrawSettings{Color = Color.White.WithMultipliedOpacity(CalculateFade(1f))});
        
        painter.DrawStringWithinRectangle(Client.Assets.GetFont("fishbone/font", 60), "Made in 72 Hours\nby NotExplosive", cameraRectangle.Moved(new Vector2(0, 300)),
            Alignment.Center, new DrawSettings{Color = Color.White.WithMultipliedOpacity(CalculateFade(4f))});
        
        painter.DrawStringWithinRectangle(Client.Assets.GetFont("fishbone/font", 30), "Programmed in MonoGame and .NET\nSFX & Music made in FuncSynth\nnotexplosive.net", cameraRectangle.Inflated(0, -20),
            Alignment.BottomCenter, new DrawSettings{Color = Color.White.WithMultipliedOpacity(CalculateFade(6f))});
    }

    private float CalculateFade(float delay)
    {
        return Math.Clamp(_fader - delay, 0, 1);
    }
}
