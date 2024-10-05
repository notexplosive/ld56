using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Food : Entity, IFocalPoint
{
    private float _shineTime;
    private float _flame = 1f;
    private float _flash = 0f;

    public bool IsEaten { get; private set; }

    public override void Draw(Painter painter)
    {
        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(Position, new Vector2(1000f) * Ease.CubicFastSlow(_flash)),
            new DrawSettings {Origin = DrawOrigin.Center, Color = Color.White.WithMultipliedOpacity(0.25f * _flash)});
        
        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(Position, new Vector2(30)),
            new DrawSettings {Origin = DrawOrigin.Center});

        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(Position, new Vector2(100, 100) *
                (1 + MathF.Sin(_shineTime * 10) / 100) * _flame),
            new DrawSettings {Origin = DrawOrigin.Center, Color = Color.White.WithMultipliedOpacity(0.5f)});

        var lightNarrow = 50 * _flame;
        var lightWideBase = 400 * _flame;
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

        if (IsEaten)
        {
            _flame -= dt;
            if (_flame < 0.1f)
            {
                _flame = 0.1f;
            }
        }

        if (_flash > 0)
        {
            _flash -= dt * 5f;
            if (_flash < 0)
            {
                _flash = 0f;
            }
        }
    }

    public void Eat()
    {
        IsEaten = true;
        _flash = 1f;
    }

    public float FocalWeight()
    {
        if (IsEaten)
        {
            return 0;
        }
        return 2f;
    }
}
