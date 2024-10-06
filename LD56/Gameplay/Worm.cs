using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Worm : Entity
{
    private readonly float _maximumSpeed;
    private readonly float _maxSegmentSize = 30;
    private readonly float _minimumSpeed;
    private readonly float _steeringPower;
    private readonly List<TailSegment> _tailSegments = new();
    private readonly World _world;
    private float _bankCooldown;
    private float _bankPercent;
    private float _facingAngle;
    private float _forwardSpeed;
    public Food? HeldFood { get; private set; }
    private float _tailFurlPercent;

    public Worm(World world)
    {
        _world = world;
        _facingAngle = 0f;
        _minimumSpeed = 200f;
        _maximumSpeed = 800f;
        _steeringPower = 1.5f;
        _forwardSpeed = _minimumSpeed;

        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
    }

    public float BankPercent
    {
        set
        {
            _bankPercent = Math.Clamp(value, -1, 1);
            _bankCooldown = 0.1f;
        }
        get => _bankPercent;
    }

    public int DirectionalInput { get; set; }

    private void DrawArrowHead(Painter painter, Vector2 currentSegment, Vector2 previousSegment,
        float segmentSize, float flapPercent)
    {
        var offset = currentSegment - previousSegment;
        var leftAngle = offset.GetAngleFromUnitX() + MathF.PI / 4f + MathF.PI / 8 * flapPercent;
        var rightAngle = offset.GetAngleFromUnitX() - MathF.PI / 4f - MathF.PI / 8 * flapPercent;
        var leftArm = Vector2Extensions.Polar(segmentSize, leftAngle + BankPercent / 2f);
        var rightArm = Vector2Extensions.Polar(segmentSize, rightAngle + BankPercent / 2f);

        painter.DrawLine(currentSegment, currentSegment + leftArm, new LineDrawSettings(){Thickness =  2});
        painter.DrawLine(currentSegment, currentSegment + rightArm, new LineDrawSettings(){Thickness =  2});
        painter.DrawLine(previousSegment, currentSegment + leftArm, new LineDrawSettings(){Thickness =  3});
        painter.DrawLine(previousSegment, currentSegment + rightArm, new LineDrawSettings(){Thickness =  3});
    }

    public override void Draw(Painter painter)
    {
        DrawArrowHead(painter, Position, _tailSegments.First().Position, 60,
            HeldFood != null ? -1f : 0f);

        for (var index = 0; index < _tailSegments.Count; index++)
        {
            var segment = _tailSegments[index];
            var previousSegment = Position;
            if (index > 0)
            {
                previousSegment = _tailSegments[index - 1].Position;
            }

            DrawArrowHead(painter, segment.Position, previousSegment, MathF.Abs(MathF.Sin(index / 2f)) * 50,
                1f - Ease.CubicSlowFast(_tailFurlPercent) * 2f + MathF.Sin(Client.TotalElapsedTime) * 0.3f);
        }
    }

    public override void Update(float dt)
    {
        var speedPercent = _forwardSpeed / _maximumSpeed;
        _facingAngle += DirectionalInput * _steeringPower * (1 + speedPercent) * dt;
        var direction = Vector2Extensions.Polar(1f, _facingAngle);
        Position += direction * _forwardSpeed * dt;


        if (DirectionalInput == 0)
        {
            _forwardSpeed += dt * 200f;
        }
        else
        {
            _forwardSpeed -= dt * 100f;
        }
        _forwardSpeed = Math.Clamp(_forwardSpeed, _minimumSpeed, _maximumSpeed);
        
        if(DirectionalInput != 0)
        {
            BankPercent += DirectionalInput * dt * _forwardSpeed / 100f;
        }


        if (HeldFood != null)
        {
            var angle = Vector2Extensions.GetAngleFromUnitX(Position - _tailSegments.First().Position);
            var headDirection = Vector2Extensions.Polar(50, angle + BankPercent /2f);
            HeldFood.Position = Position + headDirection;
        }

        foreach (var entity in _world.Entities)
        {
            if (entity is Food food)
            {
                if (Vector2.Distance(food.Position, Position) < 100 && !food.IsEaten && HeldFood == null)
                {
                    HeldFood = food;
                    food.Eat();
                }
            }
        }
        
        foreach (var entity in _world.Entities)
        {
            if (entity is Obstacle obstacle)
            {
                var radius = obstacle.Radius;
                if (Vector2.Distance(Position, obstacle.Position) < radius)
                {
                    var displacement = Position - obstacle.Position;
                    Position = obstacle.Position + displacement.Normalized() * radius;
                    _facingAngle = displacement.GetAngleFromUnitX();
                }
            }
        }

        UpdateDrawingRelatedStuff(dt);
    }

    private void UpdateDrawingRelatedStuff(float dt)
    {
        for (var i = 0; i < _tailSegments.Count; i++)
        {
            var positionOfPreviousSegment = Position;

            if (i > 0)
            {
                positionOfPreviousSegment = _tailSegments[i - 1].Position;
            }

            var currentSegment = _tailSegments[i];

            if (Vector2.Distance(currentSegment.Position, positionOfPreviousSegment) > _maxSegmentSize)
            {
                var displacement = currentSegment.Position - positionOfPreviousSegment;
                currentSegment.Position = positionOfPreviousSegment + displacement.Normalized() * _maxSegmentSize;
            }
        }

        _tailFurlPercent = Math.Clamp(_tailFurlPercent - dt * 2, 0, 1f);

        if (_bankCooldown > 0)
        {
            _bankCooldown -= dt;
        }
        else
        {
            _bankPercent *= 0.999f;
        }
    }

    public void Jet()
    {
    }

    public void DeleteFood()
    {
        HeldFood = null;
    }
}
