using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;

namespace LD56.Gameplay;

public class WormBehavior : Component
{
    private readonly Entity _entity;
    private readonly WormRenderer _wormRenderer;
    private float _facingAngle;
    private float _forwardSpeed;
    private readonly float _minimumSpeed;
    private readonly float _maximumSpeed;
    private readonly float _steeringPower;

    public WormBehavior(Entity entity, WormRenderer wormRenderer)
    {
        _entity = entity;
        _wormRenderer = wormRenderer;
        _facingAngle = 0f;
        _minimumSpeed = 200f;
        _maximumSpeed = 800f;
        _steeringPower = 1.5f;
        _forwardSpeed = _minimumSpeed;
    }

    public int DirectionalInput { get; set; }

    public override void Draw(Painter painter)
    {
    }

    public override void Update(float dt)
    {
        var speedPercent = _forwardSpeed / _maximumSpeed;
        _facingAngle += DirectionalInput * _steeringPower * (1 + speedPercent) * dt;
        var direction = Vector2Extensions.Polar(1f, _facingAngle);
        _entity.Position += direction * _forwardSpeed * dt;
        
        _forwardSpeed -= dt * 100f;
        _forwardSpeed = Math.Clamp(_forwardSpeed, _minimumSpeed, _maximumSpeed);

        if (DirectionalInput != 0)
        {
            _wormRenderer.BankPercent += DirectionalInput * dt * _forwardSpeed / 100f;
        }
    }

    public void Jet()
    {
        _forwardSpeed += _maximumSpeed / 8f;
        _wormRenderer.Furl();
    }
}
