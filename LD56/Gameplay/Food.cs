using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Food : Entity
{
    private float _shineTime;

    public bool IsEaten { get; private set; }

    public override void Draw(Painter painter)
    {
        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(Position, new Vector2(30)),
            new DrawSettings {Origin = DrawOrigin.Center});

        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(Position, new Vector2(100, 100) *
                (1 + MathF.Sin(_shineTime * 10) / 100)),
            new DrawSettings {Origin = DrawOrigin.Center, Color = Color.White.WithMultipliedOpacity(0.5f)});

        var lightNarrow = 50;
        var lightWideBase = 400;
        var shimmerTime = _shineTime * 25;
        var shimmerScaler = 30;
        Constants.CircleImage.DrawAsRectangle(painter,
            new RectangleF(Position,
                new Vector2(lightWideBase + MathF.Sin(shimmerTime) * shimmerScaler, lightNarrow)),
            new DrawSettings {Origin = DrawOrigin.Center, Color = Color.White.WithMultipliedOpacity(0.25f)});

        Constants.CircleImage.DrawAsRectangle(painter,
            new RectangleF(Position,
                new Vector2(lightNarrow, lightWideBase + MathF.Cos(shimmerTime) * shimmerScaler)),
            new DrawSettings {Origin = DrawOrigin.Center, Color = Color.White.WithMultipliedOpacity(0.25f)});
    }

    public override void Update(float dt)
    {
        _shineTime += dt;
    }

    public void Eat()
    {
        IsEaten = true;
    }
}
