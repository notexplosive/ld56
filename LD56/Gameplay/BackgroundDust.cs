using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class BackgroundDust
{
    private readonly List<DustParticle> _particles = new();
    private Vector2 _savedCameraPosition;

    public BackgroundDust(Vector2 cameraSize)
    {
        for (int i = 0; i < 16; i++)
        {
            _particles.Add(new DustParticle()
            {
                Position = new Vector2(cameraSize.X * Client.Random.Dirty.NextFloat(),
                    cameraSize.Y * Client.Random.Dirty.NextFloat())
            });
        }
    }
    
    public void Draw(Painter painter, RectangleF cameraViewBounds)
    {
        var delta = _savedCameraPosition - cameraViewBounds.Center;
        
        foreach (var particle in _particles)
        {
            particle.Position += delta;
            var particleSize = 5;
            painter.DrawLine(particle.Position - new Vector2(particleSize, particleSize), particle.Position + new Vector2(particleSize, particleSize), new LineDrawSettings{Thickness = 2f});
            painter.DrawLine(particle.Position - new Vector2(particleSize, -particleSize), particle.Position + new Vector2(particleSize, -particleSize), new LineDrawSettings{Thickness = 2f});

            if (particle.Position.X < 0)
            {
                particle.Position += new Vector2(cameraViewBounds.Width, 0);
            }
            
            if (particle.Position.X > cameraViewBounds.Width)
            {
                particle.Position -= new Vector2(cameraViewBounds.Width, 0);
            }
            
            if (particle.Position.Y > cameraViewBounds.Height)
            {
                particle.Position -= new Vector2(0, cameraViewBounds.Height);
            }
            
            if (particle.Position.Y < 0)
            {
                particle.Position += new Vector2(0, cameraViewBounds.Height);
            }
        }
        
        _savedCameraPosition = cameraViewBounds.Center;
    }
}

internal class DustParticle
{
    public Vector2 Position { get; set; }
}
