using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class BackgroundDust
{
    private readonly Camera _camera;
    private readonly float _scaler;
    private readonly List<DustParticle> _particles = new();

    public BackgroundDust(Camera camera, float scaler)
    {
        _camera = camera;
        _scaler = scaler;
        for (int i = 0; i < 10; i++)
        {
            _particles.Add(new DustParticle()
            {
                Position = new Vector2(camera.ViewBounds.Width * 2 * Client.Random.Dirty.NextFloat(),
                    camera.ViewBounds.Height * 2 * Client.Random.Dirty.NextFloat())
            });
        }
    }

    public Matrix Matrix => _camera.ViewBounds.MovedToZero().Moved(_camera.CenterPosition / (_scaler)).CanvasToScreen();

    public void Draw(Painter painter)
    {
        var topLeft = Vector2.Transform(_camera.OutputResolution.ToRectangleF().TopLeft, Matrix.Invert(Matrix));
        var bottomRight = Vector2.Transform(_camera.OutputResolution.ToRectangleF().BottomRight, Matrix.Invert(Matrix));
        foreach (var particle in _particles)
        {
            var particleSize = 5;
            painter.DrawLine(particle.Position - new Vector2(particleSize, particleSize), particle.Position + new Vector2(particleSize, particleSize), new LineDrawSettings{Thickness = 2f, Color = Color.White.WithMultipliedOpacity(1 / _scaler)});
            painter.DrawLine(particle.Position - new Vector2(particleSize, -particleSize), particle.Position + new Vector2(particleSize, -particleSize), new LineDrawSettings{Thickness = 2f, Color = Color.White.WithMultipliedOpacity(1 / _scaler)});
            
            var newPosition = particle.Position;
            if (particle.Position.X < topLeft.X)
            {
                newPosition.X += bottomRight.X - topLeft.X;
            }
            
            if (particle.Position.X > bottomRight.X)
            {
                newPosition.X -= bottomRight.X - topLeft.X;
            }

            
            if (particle.Position.Y < topLeft.Y)
            {
                newPosition.Y += bottomRight.Y - topLeft.Y;
            }
            
            if (particle.Position.Y > bottomRight.Y)
            {
                newPosition.Y -= bottomRight.Y - topLeft.Y;
            }
            
            particle.Position = newPosition;
        }
    }
}

internal class DustParticle
{
    public Vector2 Position { get; set; }
}
