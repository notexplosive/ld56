using System;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Enemy : Entity, IFocalPoint
{
    private readonly float _maxSpeed = 1700f;
    private readonly float _speed = 50f;
    private readonly World _world;
    private LineDrawSettings _lineStyle;
    private float _motionTime;
    private Player? _target;
    private Vector2 _velocity;

    public Enemy(World world)
    {
        _world = world;
    }

    public bool IsAlert => _target != null;

    public float FocalWeight()
    {
        return 3f;
    }

    public override void Draw(Painter painter)
    {
        var motion = MathF.Sin(_motionTime);
        Constants.DrawCircle(painter, Position + new Vector2(0, motion * 5), 25f);
        Constants.DrawCircle(painter, Position - new Vector2(0, 25) + new Vector2(0, motion * 5), 20f);

        _lineStyle = new LineDrawSettings {Thickness = 10};

        DrawLeg(painter, Knee1(motion), Foot1(motion));
        DrawLeg(painter, Knee2(motion), Foot2(motion));

        DrawLeg(painter, Knee1(-motion, true), Foot1(-motion, true));
        DrawLeg(painter, Knee2(-motion, true), Foot2(-motion, true));
    }

    private static Vector2 Foot2(float motion, bool mirrorX = false)
    {
        return Mirrored(new Vector2(-120 - motion * 10, 40 - motion * 20), mirrorX);
    }

    private static Vector2 Knee2(float motion, bool mirrorX = false)
    {
        return Mirrored(new Vector2(-75 + motion, -80 - motion * 20), mirrorX);
    }

    private static Vector2 Foot1(float motion, bool mirrorX = false)
    {
        return Mirrored(new Vector2(-75 + motion * 10, 50 - motion * 20), mirrorX);
    }

    private static Vector2 Knee1(float motion, bool mirrorX = false)
    {
        return Mirrored(new Vector2(-50 + motion, -50 + motion * 20), mirrorX);
    }

    private static Vector2 Mirrored(Vector2 original, bool shouldMirror)
    {
        return shouldMirror ? new Vector2(-original.X, original.Y) : original;
    }

    private void DrawLeg(Painter painter, Vector2 knee1, Vector2 foot1)
    {
        painter.DrawLine(Position - new Vector2(0, 15f), Position + knee1, _lineStyle);
        Constants.DrawCircle(painter, Position + knee1, 5f);
        painter.DrawLine(Position + knee1, Position + foot1, _lineStyle);
        Constants.DrawCircle(painter, Position + foot1, 5f);
    }

    public override void Update(float dt)
    {
        foreach (var entity in _world.Entities)
        {
            if (entity is Obstacle obstacle)
            {
                var radius = obstacle.Radius;
                if (Vector2.Distance(Position, obstacle.Position) < radius)
                {
                    var displacement = Position - obstacle.Position;
                    Position = obstacle.Position + displacement.Normalized() * radius;
                    _velocity = Vector2Extensions.Polar(_velocity.Length() / 2f, displacement.GetAngleFromUnitX());
                }
            }
        }

        if (_target == null)
        {
            foreach (var entity in _world.Entities)
            {
                if (entity is Player player)
                {
                    if (TestDistance(entity.Position) && TestLineOfSightTo(entity.Position))
                    {
                        _target = player;
                    }
                }
            }
        }
        else
        {
            var displacement = _target.Position - Position;
            _velocity += displacement.Normalized() * _speed * dt * 40f;

            if (_target.IsHurtAt(Position))
            {
                _target.TakeDamage();

                // speed boost after chomping player (should help player get away
                _velocity += displacement.Normalized() * _speed * dt * 80f;
            }

            if (_velocity.Length() > _maxSpeed)
            {
                _velocity = _velocity.Normalized() * _maxSpeed;
            }
        }

        if (_target != null && (!TestDistance(_target.Position) || !TestLineOfSightTo(_target.Position)))
        {
            _target = null;
            _velocity = Vector2.Zero;
        }

        Position += _velocity * dt;

        _motionTime += _velocity.Length() * dt / 100f;
    }

    private bool TestDistance(Vector2 entityPosition)
    {
        return (Position - entityPosition).Length() < 1920;
    }

    private bool TestLineOfSightTo(Vector2 targetPosition)
    {
        var obstacles = _world.Entities.Where(a => a is Obstacle).Cast<Obstacle>().ToList();

        for (var i = 5; i < 100; i++)
        {
            var probe = Vector2.Lerp(Position, targetPosition, i / 100f);

            foreach (var obstacle in obstacles)
            {
                var radius = obstacle.Radius;
                if (Vector2.Distance(probe, obstacle.Position) < radius)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public override bool EditorHitTest(Vector2 mousePosition)
    {
        return (mousePosition - Position).Length() < 100;
    }
}
