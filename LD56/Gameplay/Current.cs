using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Debugging;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Current : Entity
{
    private readonly List<Vector2> _particles;
    private readonly World _world;

    public Current(World world, float radius, float angle)
    {
        _world = world;
        Radius = radius;
        Angle = angle;

        var random = Client.Random.Dirty;

        _particles = new List<Vector2>();
        for (var i = 0; i < 25; i++)
        {
            _particles.Add(random.NextNormalVector2() * radius * random.NextFloat());
        }
    }

    public float Angle { get; set; }

    public float Radius { get; set; }

    public override void Draw(Painter painter)
    {
        foreach (var particle in _particles)
        {
            Constants.DrawCircle(painter, particle + Position, 10, Color.White.DimmedBy(0.2f));
        }

        if (Client.Debug.IsPassiveOrActive)
        {
            var arrowBase = Position;
            var direction = Vector2Extensions.Polar(1f, Angle);
            var arrowHead = arrowBase + direction * 200;

            painter.DrawLine(arrowBase, arrowHead,
                new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
            painter.DrawLine(arrowHead,
                arrowHead + Vector2Extensions.Polar(20,
                    direction.GetAngleFromUnitX() + MathF.PI * 5 / 6f),
                new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
            painter.DrawLine(arrowHead,
                arrowHead + Vector2Extensions.Polar(20,
                    direction.GetAngleFromUnitX() - MathF.PI * 5 / 6f),
                new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
        }
    }

    public override void Update(float dt)
    {
        foreach (var entity in _world.Entities)
        {
            if ((entity.Position - Position).Length() < Radius)
            {
                if (entity is Player player)
                {
                    player.PushInDirection(Angle, 500 * dt, dt);
                }
            }
        }

        for (var index = 0; index < _particles.Count; index++)
        {
            _particles[index] += Vector2Extensions.Polar(dt * 500, Angle);

            
            if ((_particles[index]).Length() > Radius)
            {
                var relativeAngle = _particles[index].GetAngleFromUnitX();
                relativeAngle += MathF.PI * Client.Random.Dirty.NextSign();
                _particles[index] = Vector2Extensions.Polar(Radius,relativeAngle);
            }
        }
    }

    public override bool EditorHitTest(Vector2 mousePosition)
    {
        return (mousePosition - Position).Length() < Radius;
    }
}
