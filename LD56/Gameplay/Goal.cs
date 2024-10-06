using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Goal : Entity, IFocalPoint
{
    private readonly World _world;
    private readonly List<Arm> _arms = new();
    public Player? Player { get; private set; }
    private int _pendingArms;
    private float _spawnTimer;
    private int _auraLevel;
    public float ConsumeTimer { get; private set; }
    public float AuraRadius => (_auraLevel + 1) * 200 + MutantSineFunction() * 15;

    private float MutantSineFunction()
    {
        var function = (float x) => MathF.Sin(x * MathF.PI * 2f);
        var function2 = (float x) => function(function(x));
        var function3 = (float x) => function2(function2(x));

        return function3(Client.TotalElapsedTime / 10);
    }

    public Goal(World world)
    {
        _world = world;
        for (var i = 0; i < 10; i++)
        {
            var f = i / 10f;
            var root = new Vector2(MathF.Sin(f * float.Pi * 2f), MathF.Cos(f * float.Pi * 2f)) * 15;
            _arms.Add(new Arm(root, 12));
        }
    }

    public void IncreaseAuraLevel()
    {
        _auraLevel++;
    }

    public void CreatePlayer()
    {
        LdResourceAssets.Instance.PlaySound("player_spawn", new SoundEffectSettings());
        Player = new Player(_world);
        Player.Position = Position;
        Player.MoveAllTailSegmentsToHead();
        Player.Boost();
        _world.Entities.Add(Player);
    }

    public float FocalWeight()
    {
        return 3f;
    }

    public event Action? WasFed;

    private void AddFullArm()
    {
        ConsumeTimer = 1f;
        LdResourceAssets.Instance.PlaySound("grow_tendril", new SoundEffectSettings{Pitch = 1f});
        _arms.Add(new Arm(Vector2.Zero, 50));
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
        
        Constants.DrawCircle(painter, Position, AuraRadius, Color.White.WithMultipliedOpacity(0.15f));
    }

    public override void Update(float dt)
    {
        if (ConsumeTimer > 0)
        {
            ConsumeTimer -= dt;
        }
        else
        {
            ConsumeTimer = 0f;
        }
        
        for (var index = 0; index < _arms.Count; index++)
        {
            var arm = _arms[index];
            var updateSpeed = 1f;
            if (Player?.HeldFood != null && Vector2.Distance(Player.Position, Position) < 600 && index % 2 == 0)
            {
                arm.Destination = Player.HeldFood.Position - Position;
                updateSpeed = 5;

                if (Vector2.Distance(Player.HeldFood.Position, arm.Chain.Head.Position + Position) < 25)
                {
                    Player.HeldFood.Destroy();
                    Player.DeleteFood();
                    _pendingArms++;
                    WasFed?.Invoke();
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

        for (var i = 0; i < _pendingArms; i++)
        {
            AddFullArm();

            foreach (var arm in _arms)
            {
                arm.Chain.AddLink(10);
            }
        }

        _pendingArms = 0;
    }

    public override bool EditorHitTest(Vector2 mousePosition)
    {
        // fails always
        return false;
    }

    public void SpawningInput(bool leftIsDown, bool rightIsDown, float dt)
    {
        if (Player == null)
        {
            for (var index = 0; index < _arms.Count; index++)
            {
                if ((index % 2 == 0 && leftIsDown) || (index % 2 == 1 && rightIsDown))
                {
                    var arm = _arms[index];
                    arm.Destination = new Vector2((Client.Random.Dirty.NextFloat() - 0.5f) * 1600, -800);
                }
            }

            if (leftIsDown && rightIsDown)
            {
                _spawnTimer += dt;
                if (_spawnTimer > 1)
                {
                    CreatePlayer();
                }
            }
            else
            {
                _spawnTimer -= dt;
                if (_spawnTimer < 0)
                {
                    _spawnTimer = 0;
                }
            }
        }
    }

    public void KillPlayer()
    {
        if (Player != null)
        {
            Player.Destroy();
            Player = null;

            _world.RequestReload();
        }
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

    public Vector2 RelativeRoot { get; }

    public void Update(float dt)
    {
        Chain.PutHeadAt(RelativeRoot, Vector2.Lerp(Chain.Head.Position, Destination, dt * 5));
    }
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
