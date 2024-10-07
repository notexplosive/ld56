using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class ObstacleRenderer : Entity
{
    private readonly List<LineSegment> _lines;

    public ObstacleRenderer(World world)
    {
        Depth = Depth.Middle - 10;
        _lines = new List<LineSegment>();

        foreach (var obstacle in world.Entities.Where(a => a is Obstacle).Cast<Obstacle>())
        {
            var increment = MathF.PI / 16;
            for (float angle = 0; angle < MathF.PI * 2f; angle += increment)
            {
                var pointA = Vector2Extensions.Polar(obstacle.Radius + 2f, angle) + obstacle.Position;
                var pointB = Vector2Extensions.Polar(obstacle.Radius + 2f, angle + increment) + obstacle.Position;

                _lines.Add(new LineSegment(pointA, pointB));
            }
        }

        foreach (var obstacle in world.Entities.Where(a => a is Obstacle).Cast<Obstacle>())
        {
            var linesToRemove = new List<LineSegment>();
            foreach (var line in _lines)
            {
                if ((obstacle.Position - line.A).Length() < obstacle.Radius &&
                    (obstacle.Position - line.B).Length() < obstacle.Radius)
                {
                    linesToRemove.Add(line);
                }
            }

            foreach (var lineToRemove in linesToRemove)
            {
                _lines.Remove(lineToRemove);
            }
        }
    }

    public override void Draw(Painter painter)
    {
        foreach (var line in _lines)
        {
            painter.DrawLine(line.A, line.B, new LineDrawSettings
            {
                Thickness = 5f,
                Color = Color.White.DimmedBy(0.25f)
            });
        }
    }

    public override void Update(float dt)
    {
    }

    public override bool EditorHitTest(Vector2 mousePosition)
    {
        return false;
    }
}

public record LineSegment(Vector2 A, Vector2 B);
