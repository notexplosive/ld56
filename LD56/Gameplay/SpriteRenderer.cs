using System;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;

namespace LD56.Gameplay;

public class SpriteRenderer : Component
{
    private readonly Entity _entity;
    private readonly SpriteSheet _sheet;
    private IFrameAnimation _currentAnimation;
    private float _time;
    private float _framesPerSecond = 10;

    public SpriteRenderer(Entity entity, SpriteSheet sheet)
    {
        _entity = entity;
        _sheet = sheet;
        _currentAnimation = _sheet.DefaultAnimation;
    }

    public int Frame => _currentAnimation.GetFrame(_time * _framesPerSecond);

    public void SetAnimation(IFrameAnimation animation)
    {
        _currentAnimation = animation;
    }

    public override void Draw(Painter painter)
    {
        _sheet.DrawFrameAtPosition(painter, Frame, _entity.Position, _entity.Scale,
            new DrawSettings {Depth = _entity.Depth});
    }

    public override void Update(float dt)
    {
        _time += dt;
    }
}
