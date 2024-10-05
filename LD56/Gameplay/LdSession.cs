using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;
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
    private readonly World _world = new();
    private readonly Worm _worm;
    private int _buttonInput;

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

        var food = new Food();
        food.Position = new Vector2(50, 50);
        _world.Entities.Add(food);

        var food2 = new Food();
        food2.Position = new Vector2(-1650, -1650);
        _world.Entities.Add(food2);

        var goal = new Goal(_worm);
        goal.Position = new Vector2(800, 800);
        _world.Entities.Add(goal);
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
            _worm.Jet();
        }
    }

    public void Update(float dt)
    {
        foreach (var entity in _world.Entities)
        {
            entity.Update(dt);
        }

        HandleCamera();

        _world.Entities.RemoveAll(e=>e.FlaggedForDestroy);
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
        HandleCamera(painter);
        painter.EndSpriteBatch();
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
