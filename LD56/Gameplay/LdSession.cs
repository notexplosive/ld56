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

public class World
{
    public List<Entity> Entities { get; } = new();
}

public class LdSession : ISession
{
    private readonly Camera _camera;
    private readonly List<BackgroundDust> _dustLayers = new();
    private readonly Goal _goal;
    private readonly World _world = new();
    private readonly Worm _worm;
    private int _buttonInput;
    private Food? _food;
    private readonly List<Entity> _pendingEntities = new();
    private readonly SoundEffectInstance _coinSound;
    private readonly SoundEffectInstance _monsterSound;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _camera = new Camera(runtimeWindow.RenderResolution.ToVector2());
        _worm = new Worm(_world);
        var screenRectangle = runtimeWindow.RenderResolution.ToRectangleF();
        _worm.Position = screenRectangle.Center + new Vector2(0, screenRectangle.Height / 4f);

        _dustLayers.Add(new BackgroundDust(_camera, 0.9f));
        _dustLayers.Add(new BackgroundDust(_camera, 4f));
        _dustLayers.Add(new BackgroundDust(_camera, 8f));

        _world.Entities.Add(_worm);

        _goal = new Goal(_worm);
        _goal.WasFed += SpawnFood;
        _goal.Position = new Vector2(800, 800);
        _world.Entities.Add(_goal);

        var obstacle = new Obstacle();
        obstacle.Position = new Vector2(-600, -600);
        _world.Entities.Add(obstacle);
        
        SpawnFood();

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
        _worm.DirectionalInput = _buttonInput;

        if (input.Keyboard.GetButton(Keys.Space).WasPressed)
        {
        }
    }

    public void Update(float dt)
    {
        foreach (var entity in _world.Entities)
        {
            entity.Update(dt);
        }

        HandleCamera();

        _world.Entities.RemoveAll(e => e.FlaggedForDestroy);
        _world.Entities.AddRange(_pendingEntities);
        _pendingEntities.Clear();

        var monsterBreathVolume = Math.Clamp(1 - Vector2.Distance(_worm.Position, _goal.Position) / 2000, 0, 1);
        _monsterSound.Volume = monsterBreathVolume;

        if (_food != null && _worm.HeldFood == null)
        {
            var coinVolume = Math.Clamp(1 - Vector2.Distance(_worm.Position, _food.Position) / 2000, 0, 1) * 0.1f;
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

        foreach (var entity in _world.Entities)
        {
            entity.Draw(painter);
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);
        // debug camera
        // HandleCamera(painter);
        painter.EndSpriteBatch();

        painter.BeginSpriteBatch(_camera.CanvasToScreen);

        var target = _goal.Position;
        if (_food != null && _worm.HeldFood == null)
        {
            target = _food.Position;
        }

        if (!_camera.ViewBounds.Contains(target))
        {
            var compassDirection = (target - _worm.Position).Normalized();

            var arrowBase = _worm.Position + compassDirection * 300f;
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

    private void SpawnFood()
    {
        _food = new Food();
        _food.Position = _goal.Position +
                         Vector2Extensions.Polar(1920 * 2f, Client.Random.Clean.NextFloat() * MathF.Tau);

        _pendingEntities.Add(_food);
    }

    private void HandleCamera(Painter? debugPainter = null)
    {
        var player = _world.Entities.Find(a => a is Worm);

        var focalEntities = _world.Entities.Where(a => a is IFocalPoint).ToList();

        var entitiesInFocus = new List<Entity>();

        foreach (var entity in focalEntities)
        {
            // if (Vector2.Distance(_worm.Position, entity.Position) < 2000)
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