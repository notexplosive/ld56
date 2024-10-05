using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Goal : Entity, IFocalPoint
{
    private readonly Worm _worm;
    private readonly List<Arm> _arms = new();
    private int _pendingArms;

    public Goal(Worm worm)
    {
        _worm = worm;

        for (int i = 0; i < 10; i++)
        {
            float f = i / 10f;
            var root = new Vector2(MathF.Sin(f * Single.Pi * 2f), MathF.Cos(f * Single.Pi * 2f)) * 15;
            _arms.Add(new Arm(root, 12));
        }
    }

    private void AddFullArm()
    {
        _arms.Add(new Arm(Vector2.Zero, 50));
    }

    public float FocalWeight()
    {
        return 3f;
    }

    public override void Draw(Painter painter)
    {
        foreach (var arm in _arms)
        {
            var index = 0;
            foreach (var node in arm.Chain.LinksFromHeadToTail())
            {
                index++;
                Constants.CircleImage.DrawAsRectangle(painter,
                    new RectangleF(Position + node.Position, new Vector2(20 + 5 * MathF.Sin(index / 10f))),
                    new DrawSettings
                    {
                        Origin = DrawOrigin.Center
                    });
            }
        }
    }

    public override void Update(float dt)
    {
        for (var index = 0; index < _arms.Count; index++)
        {
            var arm = _arms[index];
            var updateSpeed = 1f;
            if (_worm.HeldFood != null && Vector2.Distance(_worm.Position, Position) < 600 && index % 2 == 0)
            {
                arm.Destination = _worm.HeldFood.Position - Position;
                updateSpeed = 5;

                if (Vector2.Distance(_worm.HeldFood.Position, arm.Chain.Head.Position + Position) < 25)
                {
                    _worm.HeldFood.Destroy();
                    _worm.DeleteFood();
                    _pendingArms++;
                }
            }
            else
            {
                arm.Cooldown -= dt * 60;

                if (arm.Cooldown < 0)
                {
                    arm.Destination = arm.Chain.Head.Position + Vector2Extensions.Polar(
                        Client.Random.Dirty.NextFloat() * 500,
                        Client.Random.Dirty.NextFloat() * MathF.PI * 2f);

                    arm.Cooldown = 5f + 3 * Client.Random.Dirty.NextFloat();
                }
            }

            arm.Update(dt * updateSpeed);
        }

        for (int i = 0; i < _pendingArms; i++)
        {
            AddFullArm();

            foreach (var arm in _arms)
            {
                arm.Chain.AddLink(10);
            }
        }

        _pendingArms = 0;
    }
}

public class Arm
{
    public InverseKinematicChain Chain = new();

    public Arm(Vector2 relativeRoot, int numberOfSegments)
    {
        RelativeRoot = relativeRoot;
        for (var i = 0; i < numberOfSegments; i++)
        {
            Chain.AddLink(10);
        }
    }

    public Vector2 Destination { get; set; }
    public float Cooldown { get; set; }

    public void Update(float dt)
    {
        Chain.PutHeadAt(RelativeRoot,Vector2.Lerp(Chain.Head.Position, Destination, dt * 5));
    }

    public Vector2 RelativeRoot { get; }
}

public class InverseKinematicChain
{
    private readonly List<InverseKinematicLinkPair> _connections = new();

    public InverseKinematicChain()
    {
        Head = new InverseKinematicLink();
        Tail = Head;
    }

    public InverseKinematicLink Head { get; }
    public InverseKinematicLink Tail { get; private set; }

    public void AddLink(float distance)
    {
        var newTail = new InverseKinematicLink();
        _connections.Add(new InverseKinematicLinkPair(Tail, newTail, distance));

        Tail = newTail;
    }

    public void PullTowards(InverseKinematicLink link, Vector2 target)
    {
        if (!IsInChain(link))
        {
            return;
        }

        Client.Debug.LogWarning("todo!!");
    }

    public void PutHeadAt(Vector2 rootPosition, Vector2 target)
    {
        Head.Position = target;

        foreach (var connection in _connections)
        {
            SnapB(connection);
        }

        Tail.Position = rootPosition;
        for (var index = _connections.Count - 1; index >= 0; index--)
        {
            SnapA(_connections[index]);
        }
    }

    private void SnapA(InverseKinematicLinkPair connection)
    {
        var displacement = connection.B.Position - connection.A.Position;
        if (displacement.Length() > connection.Distance)
        {
            var newDisplacement = displacement.Normalized() * connection.Distance;
            connection.A.Position = connection.B.Position - newDisplacement;
        }
    }

    private void SnapB(InverseKinematicLinkPair connection)
    {
        var displacement = connection.A.Position - connection.B.Position;
        if (displacement.Length() > connection.Distance)
        {
            var newDisplacement = displacement.Normalized() * connection.Distance;
            connection.B.Position = connection.A.Position - newDisplacement;
        }
    }

    private bool IsInChain(InverseKinematicLink link)
    {
        foreach (var connection in _connections)
        {
            if (connection.Contains(link))
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerable<InverseKinematicLink> LinksFromHeadToTail()
    {
        yield return Head;

        foreach (var connection in _connections)
        {
            yield return connection.B;
        }

        yield return Tail;
    }
}

public record InverseKinematicLinkPair(InverseKinematicLink A, InverseKinematicLink B, float Distance)
{
    public bool Contains(InverseKinematicLink link)
    {
        return A == link || B == link;
    }
}

public class InverseKinematicLink
{
    public Vector2 Position { get; set; }
}
