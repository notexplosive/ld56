using System;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class WanderState : WorkerState
{
    private readonly Worker _worker;
    private float _waitTimer;
    private bool _isMoving;
    private readonly NoiseBasedRng _random;

    public WanderState(Worker worker, Entity entity)
    {
        _worker = worker;
        _random = new NoiseBasedRng(Client.Random.Clean.NextNoise());
        _waitTimer = NextWaitTimer();
    }

    private float NextWaitTimer()
    {
        return _random.NextFloat(1, 3);
    }

    public override void Update(float dt)
    {
        if (_waitTimer < 0)
        {
            if (_isMoving)
            {
                _worker.Velocity = Vector2.Zero;
            }
            else
            {
                _worker.Velocity = Vector2Extensions.Polar(1f, _random.NextFloat() * MathF.PI * 2f) * 0.25f;
            }

            _isMoving = !_isMoving;
            _waitTimer = NextWaitTimer();
        }
        
        if (_worker.Velocity.LengthSquared() > 0)
        {
            _worker.SpriteRenderer.SetAnimation(Constants.WorkerWalkAnimation);
        }
        else
        {
            _worker.SpriteRenderer.SetAnimation(Constants.WorkerIdleAnimation);
        }

        _waitTimer -= dt;
    }
}
