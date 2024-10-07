using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Obstacle : Entity
{
    public Obstacle(float radius)
    {
        Radius = radius;
    }

    public float Radius { get; }

    public override void Draw(Painter painter)
    {
        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(Position, new Vector2(Radius * 2f)),
            new DrawSettings {Origin = DrawOrigin.Center, Color = ColorExtensions.FromRgbHex(0x060608), Depth = Depth + 10});
    }

    public override void Update(float dt)
    {
    }

    public override bool EditorHitTest(Vector2 mousePosition)
    {
        return (mousePosition - Position).Length() < Radius;
    }
}
