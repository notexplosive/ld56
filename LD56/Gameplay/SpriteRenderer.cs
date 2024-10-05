using System;
using ExplogineCore.Data;
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

    public SpriteRenderer(Entity entity, SpriteSheet sheet)
    {
        _entity = entity;
        _sheet = sheet;
        _currentAnimation = _sheet.DefaultAnimation;
    }

    public float FramesPerSecond { get; set; } = 10;
    public int Frame => _currentAnimation.GetFrame(_time * FramesPerSecond);
    
    public bool FlipX { get; set; }

    public void SetAnimation(IFrameAnimation animation)
    {
        if (_currentAnimation == animation)
        {
            return;
        }
        
        _currentAnimation = animation;
        _time = 0f;
    }

    public override void Draw(Painter painter)
    {
        _sheet.DrawFrameAtPosition(painter, Frame, _entity.Position, _entity.Scale,
            new DrawSettings {Depth = Math.Clamp(_entity.Depth - (int) _entity.Position.Y, 0, Depth.MaxAsInt), Origin = DrawOrigin.Center, Flip = new XyBool(FlipX, false)});
    }

    public override void Update(float dt)
    {
        _time += dt;
    }
}
