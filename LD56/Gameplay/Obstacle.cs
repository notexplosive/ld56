using System.Reflection.Metadata;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class Obstacle : Entity
{
    public Obstacle()
    {
        Radius = 250;
    }

    public int Radius { get; set; }

    public override void Draw(Painter painter)
    {
        Constants.CircleImage.DrawAsRectangle(painter, new RectangleF(Position, new Vector2(Radius * 2f)), new DrawSettings{Origin = DrawOrigin.Center, Color = Color.White.DimmedBy(0.2f)});
    }

    public override void Update(float dt)
    {
    }
}
