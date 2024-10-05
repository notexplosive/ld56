using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class WormRenderer : Component
{
    private readonly Entity _entity;
    private readonly float _maxSegmentSize = 25;
    private readonly List<TailSegment> _tailSegments = new();
    private float _tailFurlPercent;

    public WormRenderer(Entity entity)
    {
        _entity = entity;
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
        _tailSegments.Add(new TailSegment());
    }

    public override void Draw(Painter painter)
    {
        DrawArrowHead(painter, _entity.Position, _tailSegments.First().Position, 60, -1f);

        for (var index = 0; index < _tailSegments.Count; index++)
        {
            var segment = _tailSegments[index];
            var previousSegment = _entity.Position;
            if (index > 0)
            {
                previousSegment = _tailSegments[index - 1].Position;
            }

            DrawArrowHead(painter, segment.Position, previousSegment, MathF.Abs(MathF.Sin(index / 2f)) * 50,
                1f - _tailFurlPercent + MathF.Sin(Client.TotalElapsedTime) * 0.3f);
        }
    }

    private static void DrawArrowHead(Painter painter, Vector2 currentSegment, Vector2 previousSegment,
        float segmentSize, float flapPercent)
    {
        var offset = currentSegment - previousSegment;
        var leftAngle = offset.GetAngleFromUnitX() + MathF.PI / 4f + MathF.PI / 8 * flapPercent;
        var rightAngle = offset.GetAngleFromUnitX() - MathF.PI / 4f - MathF.PI / 8 * flapPercent;
        var leftArm = Vector2Extensions.Polar(segmentSize, leftAngle);
        var rightArm = Vector2Extensions.Polar(segmentSize, rightAngle);

        painter.DrawLine(currentSegment, currentSegment + leftArm, new LineDrawSettings());
        painter.DrawLine(currentSegment, currentSegment + rightArm, new LineDrawSettings());
        painter.DrawLine(previousSegment, currentSegment + leftArm, new LineDrawSettings());
        painter.DrawLine(previousSegment, currentSegment + rightArm, new LineDrawSettings());
    }

    public override void Update(float dt)
    {
        for (var i = 0; i < _tailSegments.Count; i++)
        {
            var positionOfPreviousSegment = _entity.Position;

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
    }

    public void Furl()
    {
        _tailFurlPercent = 1f;
    }
}
