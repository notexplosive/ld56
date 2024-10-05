using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class FrogRenderer : Component
{
    private readonly Entity _entity;
    private readonly Vector2 _floorPosition;
    private float _breathAmount;
    private float _eyelidOpenAmount;
    private float _legExtent;
    private float _verticalVelocity;
    private bool _isExtending;
    private float _legWiggle;
    private float _legWiggleElapsed;
    private readonly SequenceTween _blinkTween;
    private float? _waitUntilNextBlink;
    private bool _isRebounding;
    private float _lilyPadWobble;
    private readonly List<float> _splashRings;

    public FrogRenderer(Entity entity)
    {
        _entity = entity;

        _legExtent = 0f;
        _breathAmount = 0f;
        _eyelidOpenAmount = 0f;
        _floorPosition = entity.Position;
        _eyelidOpenAmount = 0.6f;

        var eyelidTweenable = new TweenableFloat(() => _eyelidOpenAmount, a => _eyelidOpenAmount = a);
        
        _blinkTween = new SequenceTween()
                .Add(eyelidTweenable.TweenTo(0.9f, 0.15f, Ease.CubicFastSlow))
                .Add(eyelidTweenable.TweenTo(0f, 0.25f, Ease.CubicSlowFast))
                .Add(new WaitSecondsTween(0.15f))
                .Add(eyelidTweenable.TweenTo(0.6f, 0.25f, Ease.CubicFastSlow))
            ;

        _splashRings = new List<float>();
    }

    public override void Draw(Painter painter)
    {
        var offsetFromLegExtent = _legExtent * 1.5f;
        var bodyOffsetFromLegs = new Vector2(0, offsetFromLegExtent) * 20;
        var breathOffset = new Vector2(0, -5 * _breathAmount / 2) - bodyOffsetFromLegs * 2f;

        // body
        DrawSegment(painter, Vector2.Zero + breathOffset, new Vector2(80, 75 + 5 * _breathAmount), Color.Green, 0);

        // eyes
        var eyeHeight = -35;
        DrawSegment(painter, new Vector2(30, eyeHeight) + breathOffset, new Vector2(40, 40), Color.Green, -1, true);
        DrawSegment(painter, new Vector2(30, eyeHeight) + breathOffset, new Vector2(30, 30 * _eyelidOpenAmount),
            Color.Black, -2, true);

        // feet
        DrawSegment(painter, new Vector2(25 + 5 * _legWiggle / 4f, 10 - offsetFromLegExtent * 20), new Vector2(40, 30),
            Color.DarkGreen, +1, true);
        DrawSegment(painter, new Vector2(30 + 5 * _legWiggle / 2f, 20 - offsetFromLegExtent * 10), new Vector2(40, 30),
            Color.DarkGreen, +1, true);
        DrawSegment(painter, new Vector2(30 + 5 * _legWiggle, 25), new Vector2(50, 30),
            Color.DarkOrange, +2, true);
        
        // hands and fingers
        var handPosition = new Vector2(30 - 5 * _legWiggle,  35 - 100 * offsetFromLegExtent / 2f);
        var handColor = Color.DarkOrange.BrightenedBy(0.1f);
        DrawSegment(painter, handPosition, new Vector2(30, 30),
            handColor, -3, true);
        DrawSegment(painter, handPosition + new Vector2(13,5), new Vector2(10, 10),
            handColor, -3, true);
        DrawSegment(painter, handPosition + new Vector2(10,0), new Vector2(10, 10),
            handColor, -3, true);
        DrawSegment(painter, handPosition + new Vector2(-13,5), new Vector2(10, 10),
            handColor, -3, true);

        // lilly pad
        var offset = new Vector2(0, 55);
        var size = new Vector2(300 + 10 * _lilyPadWobble, 80);
        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(_floorPosition + offset, size),
            new DrawSettings
            {
                Origin = DrawOrigin.Center,
                Color = Color.ForestGreen,
                Depth = _entity.Depth + (Depth) (+3)
            });

        foreach (var ring in _splashRings)
        {
            Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(_floorPosition + offset, size + new Vector2(size.X * ring * 2f, size.Y * ring / 4f)),
                new DrawSettings
                {
                    Origin = DrawOrigin.Center,
                    Color = Color.LightBlue.WithMultipliedOpacity(Math.Clamp(0.5f - ring, 0f, 1f)),
                    Depth = _entity.Depth + (Depth) (+4)
                });
        }
    }

    private void DrawSegment(Painter painter, Vector2 offset, Vector2 size, Color color, Depth depthOffset,
        bool isMirrored = false)
    {
        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(_entity.Position + offset, size),
            new DrawSettings
            {
                Origin = DrawOrigin.Center,
                Color = color,
                Depth = _entity.Depth + depthOffset
            });

        if (isMirrored)
        {
            Constants.CircleImage.DrawAsRectangle(painter,
                new RectangleF(_entity.Position + offset - offset.JustX() * 2, size),
                new DrawSettings
                {
                    Origin = DrawOrigin.Center,
                    Color = color,
                    Depth = _entity.Depth + depthOffset
                });
        }
    }

    public override void Update(float dt)
    {
        _blinkTween.Update(dt);

        if (_blinkTween.IsDone())
        {
            if (_waitUntilNextBlink == null)
            {
                _waitUntilNextBlink = Client.Random.Dirty.NextFloat() * 3 + 3f;
            }
            else
            {
                _waitUntilNextBlink -= dt;

                if (_waitUntilNextBlink < 0)
                {
                    _blinkTween.Reset();
                    _waitUntilNextBlink = null;
                }
            }
        }
        
        if (Math.Abs(_entity.Position.Y - _floorPosition.Y) > float.Epsilon)
        {
            _verticalVelocity += dt * 500;
            _legWiggleElapsed += dt * 5;
        }
        else
        {
            // on lilly pad
            _breathAmount = MathF.Sin(Client.TotalElapsedTime * 5);
            _legWiggleElapsed += dt * 5 * Client.Random.Clean.NextFloat();
        }

        _legWiggle = MathF.Sin(_legWiggleElapsed) / 2f;
        
        if (_entity.Position.Y > _floorPosition.Y)
        {
            // impact
            _entity.Position = _entity.Position with {Y = _floorPosition.Y};
            _legExtent = -0.25f;
            _isRebounding = true;
            Splash(5);
            _verticalVelocity = 0;
        }

        if (_lilyPadWobble > 0)
        {
            _lilyPadWobble -= dt * 60;
        }
        
        if (_isRebounding)
        {
            _legExtent += dt * 4f;

            if (_legExtent > 0)
            {
                _legExtent = 0;
                _isRebounding = false;
            }
        }
        
        _entity.Position += new Vector2(0, _verticalVelocity * dt);

        for (var index = 0; index < _splashRings.Count; index++)
        {
            _splashRings[index] += dt;
        }

        _splashRings.RemoveAll(a => a >= 1f);

        if (_isExtending)
        {
            _legExtent += dt * 25;

            if (_legExtent > 1)
            {
                _legExtent = 1f;
                _verticalVelocity = -500;
                _isExtending = false;
            }
        }
        
        if (_legExtent > 0)
        {
            _legExtent -= dt / 2f;

            if (_legExtent < 0)
            {
                _legExtent = 0;
            }
        }
    }

    private void Splash(float intensity)
    {
        _lilyPadWobble = intensity;
        _splashRings.Add(0f);
    }

    public void Jump()
    {
        _isExtending = true;
        Splash(2);
    }
}
