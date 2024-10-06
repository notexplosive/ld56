using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace LD56.Gameplay;

public class LdSession : ISession
{
    private readonly Camera _camera;
    private readonly SoundEffectInstance _coinSound;
    private readonly List<BackgroundDust> _dustLayers = new();
    private readonly SoundEffectInstance _monsterSound;
    public World World { get; } = new();
    public event Action? RequestEditor;

    private int _buttonInput;
    private Food? _nearestFood;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _camera = new Camera(runtimeWindow.RenderResolution.ToVector2());
        
        _dustLayers.Add(new BackgroundDust(_camera, 0.9f));
        _dustLayers.Add(new BackgroundDust(_camera, 4f));
        _dustLayers.Add(new BackgroundDust(_camera, 8f));

        World.Entities.Add(World.Player);

        _coinSound = LdResourceAssets.Instance.SoundInstances["coin_near"];
        _coinSound.Volume = 0f;
        _coinSound.IsLooped = true;
        LdResourceAssets.Instance.SoundInstances["coin_near"].Play();

        _monsterSound = LdResourceAssets.Instance.SoundInstances["monster_breath"];
        _monsterSound.Volume = 0f;
        _monsterSound.IsLooped = true;
        LdResourceAssets.Instance.SoundInstances["monster_breath"].Play();
    }

    public void OnHotReload()
    {
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        var leftInput = input.Keyboard.GetButton(Keys.Left).IsDown || input.Keyboard.GetButton(Keys.A).IsDown ? -1 : 0;
        var rightInput = input.Keyboard.GetButton(Keys.Right).IsDown || input.Keyboard.GetButton(Keys.D).IsDown ? 1 : 0;
        _buttonInput = leftInput + rightInput;
        World.Player.DirectionalInput = _buttonInput;

        if (input.Keyboard.GetButton(Keys.Space).WasPressed)
        {
        }
        
        if (input.Keyboard.GetButton(Keys.F4).WasPressed)
        {
            RequestEditor?.Invoke();
            _coinSound.Stop();
            _monsterSound.Stop();
        }
    }

    public void Update(float dt)
    {
        foreach (var entity in World.Entities)
        {
            entity.Update(dt);
        }

        HandleCamera();

        World.Entities.RemoveAll(e => e.FlaggedForDestroy);

        var monsterBreathVolume = Math.Clamp(1 - Vector2.Distance(World.Player.Position, World.Goal.Position) / 2000, 0, 1);
        _monsterSound.Volume = monsterBreathVolume;

        if (_nearestFood != null && World.Player.HeldFood == null)
        {
            var coinVolume = Math.Clamp(1 - Vector2.Distance(World.Player.Position, _nearestFood.Position) / 2000, 0, 1) * 0.1f;
            _coinSound.Volume = coinVolume;
        }
        else
        {
            _coinSound.Volume = 0f;
        }
    }

    public void Draw(Painter painter)
    {
        painter.Clear(ColorExtensions.FromRgbHex(0x060608));

        var canvasToScreen = _camera.CanvasToScreen;

        foreach (var dustLayer in _dustLayers)
        {
            painter.BeginSpriteBatch(dustLayer.Matrix);
            dustLayer.Draw(painter);
            painter.EndSpriteBatch();
        }

        painter.BeginSpriteBatch(canvasToScreen);

        foreach (var entity in World.Entities)
        {
            entity.Draw(painter);
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        // debug camera
        // HandleCamera(painter);
        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        var target = World.Goal.Position;
        if (_nearestFood != null && World.Player.HeldFood == null)
        {
            target = _nearestFood.Position;
        }

        if (!_camera.ViewBounds.Contains(target))
        {
            var compassDirection = (target - World.Player.Position).Normalized();

            var arrowBase = World.Player.Position + compassDirection * 300f;
            arrowBase = arrowBase.ConstrainedTo(_camera.ViewBounds.Inflated(0, -200));
            var arrowHead = arrowBase + compassDirection * 200f;

            painter.DrawLine(arrowBase, arrowHead,
                new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
            painter.DrawLine(arrowHead,
                arrowHead + Vector2Extensions.Polar(20, compassDirection.GetAngleFromUnitX() + MathF.PI * 5 / 6f),
                new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
            painter.DrawLine(arrowHead,
                arrowHead + Vector2Extensions.Polar(20, compassDirection.GetAngleFromUnitX() - MathF.PI * 5 / 6f),
                new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
        }

        painter.EndSpriteBatch();
    }

    private void HandleCamera(Painter? debugPainter = null)
    {
        var player = World.Entities.Find(a => a is Worm);

        var focalEntities = World.Entities.Where(a => a is IFocalPoint).ToList();

        var entitiesInFocus = new List<Entity>();

        foreach (var entity in focalEntities)
        {
            // if (Vector2.Distance(_world.Worm.Position, entity.Position) < 2000)
            {
                entitiesInFocus.Add(entity);
            }
        }

        var initialTotalPosition = Vector2.Zero;
        var playerPosition = player?.Position ?? Vector2.Zero;
        foreach (var entity in entitiesInFocus)
        {
            initialTotalPosition += entity.Position;
        }

        var initialCenter = initialTotalPosition / entitiesInFocus.Count;

        var topLeft = playerPosition;
        var bottomRight = playerPosition;
        foreach (var entity in entitiesInFocus)
        {
            var offsetFromPlayer = entity.Position - playerPosition;

            var distanceLimit = 1920;
            var influence = (distanceLimit - offsetFromPlayer.Length()) / distanceLimit;

            var focalPoint = (entity as IFocalPoint)!;
            var position = playerPosition + offsetFromPlayer * focalPoint.FocalWeight() * influence;

            if (influence > 0)
            {
                topLeft = Vector2Extensions.MinAcross(position, topLeft);
                bottomRight = Vector2Extensions.MaxAcross(position, bottomRight);
            }

            if (debugPainter != null)
            {
                /*
                Constants.CircleImage.DrawAtPosition(debugPainter, position, Scale2D.One, new DrawSettings
                {
                    Origin = DrawOrigin.Center, Color = Color.White.WithMultipliedOpacity(Math.Clamp(
                        influence, 0, 1))
                });
                */
            }
        }

        var allFociRectangle = RectangleF.FromCorners(topLeft, bottomRight);

        var cameraRectangle = RectangleF.FromCenterAndSize(allFociRectangle.Center,
            new Vector2(16 / 9f, 1f) * MathF.Max(allFociRectangle.LongSide, 1920));

        if (debugPainter != null)
        {
            /*
            debugPainter.DrawRectangle(allFociRectangle,
                new DrawSettings {Color = Color.LightBlue.WithMultipliedOpacity(0.25f)});
            debugPainter.DrawRectangle(
                cameraRectangle,
                new DrawSettings {Color = Color.Orange.WithMultipliedOpacity(0.25f)});
                */
        }

        //var debugZoom = 2;
        _camera.ViewBounds =
            cameraRectangle;
        //new RectangleF(-1920 * debugZoom, -1080 * debugZoom, 1920 * 2 * debugZoom, 1080 * 2 * debugZoom);
    }
}
