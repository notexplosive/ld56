﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Debugging;
using ExTween;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

namespace LD56.Gameplay;

public class LdSession : ISession
{
    private readonly Camera _camera;
    private readonly SoundEffectInstance _coinSound;
    private readonly List<BackgroundDust> _dustLayers = new();
    private readonly SoundEffectInstance _goalSound;
    private readonly SoundEffectInstance _enemySound;
    private readonly SoundEffectInstance _enemySound2;

    private int _buttonInput;
    private bool _leftIsDown;
    private Food? _nearestFood;
    private bool _rightIsDown;
    private bool _gameIsOver;
    private float _startTimer;

    public LdSession(RealWindow runtimeWindow, ClientFileSystem runtimeFileSystem)
    {
        _camera = new Camera(runtimeWindow.RenderResolution.ToVector2());

        _dustLayers.Add(new BackgroundDust(_camera, 0.9f));
        _dustLayers.Add(new BackgroundDust(_camera, 4f));
        _dustLayers.Add(new BackgroundDust(_camera, 8f));

        _coinSound = LdResourceAssets.Instance.SoundInstances["coin_near"];
        _coinSound.Volume = 0f;
        _coinSound.IsLooped = true;
        _coinSound.Play();

        _goalSound = LdResourceAssets.Instance.SoundInstances["monster_breath"];
        _goalSound.Volume = 0f;
        _goalSound.IsLooped = true;
        _goalSound.Play();
        
        _enemySound = LdResourceAssets.Instance.SoundInstances["enemy_near"];
        _enemySound.Volume = 0f;
        _enemySound.IsLooped = true;
        _enemySound.Play();
        
        _enemySound2 = LdResourceAssets.Instance.SoundInstances["spider_move"];
        _enemySound2.Volume = 0f;
        _enemySound2.IsLooped = true;
        _enemySound2.Play();
    }

    public World World { get; } = new();

    public void OnHotReload()
    {
    }

    public void UpdateInput(ConsumableInput input, HitTestStack hitTestStack)
    {
        if (input.Keyboard.GetButton(Keys.P).WasPressed)
        {
            World.SkipLevel();
        }
        
        _leftIsDown = input.Keyboard.GetButton(Keys.Left).IsDown || input.Keyboard.GetButton(Keys.A).IsDown;
        var leftInput = _leftIsDown ? -1 : 0;
        _rightIsDown = input.Keyboard.GetButton(Keys.Right).IsDown || input.Keyboard.GetButton(Keys.D).IsDown;
        var rightInput = _rightIsDown ? 1 : 0;

        _buttonInput = leftInput + rightInput;
        if (World.Player != null)
        {
            World.Player.DirectionalInput = _buttonInput;
        }

        if (input.Keyboard.GetButton(Keys.Space).WasPressed)
        {
        }

        if (input.Keyboard.GetButton(Keys.F4).WasPressed)
        {
            RequestEditor?.Invoke();
            StopAllSounds();
        }
    }

    private void StopAllSounds()
    {
        _coinSound.Stop();
        _goalSound.Stop();
        _enemySound.Stop();
        _enemySound2.Stop();
    }

    public void PlayAllSounds()
    {
        _startTimer = 0f;
        _coinSound.Play();
        _goalSound.Play();
        _enemySound.Play();
        _enemySound2.Play();
    }

    public void Update(float dt)
    {
        _startTimer += dt;
        foreach (var entity in World.Entities)
        {
            entity.Update(dt);
        }

        if (_gameIsOver)
        {
            World.Goal.IncreaseAuraLevel();
        }

        if (World.Player != null)
        {
            HandleFood(World.Player);
        }
        
        if (World.Player == null)
        {
            World.Goal.SpawningInput(_leftIsDown, _rightIsDown, dt);
        }

        HandleCamera();

        World.Entities.RemoveAll(e => e.FlaggedForDestroy);

        var monsterBreathVolume = 0.5f;

        if (World.Player != null)
        {
            monsterBreathVolume =
                Math.Clamp(1 - Vector2.Distance(World.Player.Position, World.Goal.Position) / 2000, 0, 0.5f);

            if (_nearestFood != null && World.Player.HeldFood == null)
            {
                var coinVolume =
                    Math.Clamp(1 - Vector2.Distance(World.Player.Position, _nearestFood.Position) / 2000, 0, 1) * 0.1f;
                _coinSound.Volume = coinVolume;
            }
            else
            {
                _coinSound.Volume = 0f;
            }
        }
        else
        {
            _coinSound.Volume = 0f;
        }

        _goalSound.Volume = monsterBreathVolume;
        _goalSound.Pitch = World.Goal.ConsumeTimer;

        if (World.HasRequestedReload)
        {
            World.LoadCurrentLevel(false);
            World.HasRequestedReload = false;
        }

        if (IsAnyEnemyAlert())
        {
            _enemySound.Volume = 0.4f;
            _enemySound2.Volume = Math.Clamp(World.Entities.Where(a=>a is Enemy).Cast<Enemy>().Sum(a=>a.Velocity.Length() / a.MaxSpeed), 0, 0.9f);
        }
        else
        {
            _enemySound.Volume = 0f;
            _enemySound2.Volume = 0f;
        }
    }

    private bool IsAnyEnemyAlert()
    {
        foreach (var enemy in World.Entities.Where(a => a is Enemy).Cast<Enemy>())
        {
            if (enemy.IsAlert)
            {
                return true;
            }
        }

        return false;
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

        if (World.Player != null)
        {
            var target = World.Goal.Position;
            if (_nearestFood != null && World.Player.HeldFood == null)
            {
                target = _nearestFood.Position;
            }

            if (!_gameIsOver)
            {
                if (!_camera.ViewBounds.Contains(target))
                {
                    var compassDirection = (target - World.Player.Position).Normalized();

                    var arrowBase = World.Player.Position + compassDirection * 300f;
                    arrowBase = arrowBase.ConstrainedTo(_camera.ViewBounds.Inflated(0, -200));
                    var arrowHead = arrowBase + compassDirection * 200f;

                    painter.DrawLine(arrowBase, arrowHead,
                        new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
                    painter.DrawLine(arrowHead,
                        arrowHead + Vector2Extensions.Polar(20,
                            compassDirection.GetAngleFromUnitX() + MathF.PI * 5 / 6f),
                        new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
                    painter.DrawLine(arrowHead,
                        arrowHead + Vector2Extensions.Polar(20,
                            compassDirection.GetAngleFromUnitX() - MathF.PI * 5 / 6f),
                        new LineDrawSettings {Thickness = 5, Color = Color.White.WithMultipliedOpacity(0.5f)});
                }
            }
        }

        painter.EndSpriteBatch();

        painter.BeginSpriteBatch();

        if (World.Player == null)
        {
            var timer = Ease.QuadFastSlow(Math.Clamp(1 - _startTimer, 0, 1));
            painter.DrawStringWithinRectangle(Client.Assets.GetFont("fishbone/font", 100), $"Hold A and D to {(World.HasSpawnedAtLeastOnce? "revive" : "start")}",
                _camera.OutputResolution.ToRectangleF().Inflated(-300f, -150 + timer * 250f), Alignment.BottomCenter, new DrawSettings());

            var leftNoise = _leftIsDown ? Client.Random.Dirty.NextNormalVector2() * 5 : new Vector2();
            var rightNoise = _rightIsDown ? Client.Random.Dirty.NextNormalVector2() * 5 : new Vector2();
            
            painter.DrawStringWithinRectangle(Client.Assets.GetFont("fishbone/font", 250), "A",
                _camera.OutputResolution.ToRectangleF().Inflated(-300f + 500f * timer, 0).Moved(leftNoise), Alignment.CenterLeft, new DrawSettings());
            
            painter.DrawStringWithinRectangle(Client.Assets.GetFont("fishbone/font", 250), "D",
                _camera.OutputResolution.ToRectangleF().Inflated(-300f + 500f * timer, 0).Moved(rightNoise), Alignment.CenterRight, new DrawSettings());

            if (!World.HasSpawnedAtLeastOnce)
            {
                var contentWarningText = "Hold Space to see Content Warnings";

                if (Client.Input.Keyboard.GetButton(Keys.Space).IsDown)
                {
                    contentWarningText = "Content Warnings: Arachnids, Tentacles";
                }

                painter.DrawStringWithinRectangle(Client.Assets.GetFont("fishbone/font", 50), contentWarningText,
                    _camera.OutputResolution.ToRectangleF().Inflated(-300f, -100 + timer * 250f),
                    Alignment.BottomCenter, new DrawSettings());
            }
        }

        painter.EndSpriteBatch();
        
        painter.BeginSpriteBatch();
        foreach (var creditsEntity in World.Entities.Where(a => a is Credits).Cast<Credits>())
        {
            creditsEntity.DrawUi(painter, _camera.OutputResolution.ToRectangleF());
        }
        painter.EndSpriteBatch();
    }

    public event Action? RequestEditor;

    private void HandleFood(Player player)
    {
        if (_nearestFood?.FlaggedForDestroy == true)
        {
            _nearestFood = null;
        }

        var food = CalculateNearestFood(player);

        _nearestFood = food.FirstOrDefault();

        if (_nearestFood == null && player.HeldFood == null)
        {
            if (!_gameIsOver)
            {
                World.LoadNextLevel();
            }

            if (CalculateNearestFood(player).Count == 0 && !_gameIsOver)
            {
                _gameIsOver = true;
                World.Entities.Add(new Credits());
                Client.Debug.Log($"Game Over!");
            }
        }
    }

    private List<Food> CalculateNearestFood(Player player)
    {
        var food = World.Entities.Where(a => a is Food food && food.IsEaten == false).Cast<Food>().ToList();
        food.Sort((a, b) =>
            (a.Position - player.Position).Length().CompareTo((b.Position - player.Position).Length()));
        return food;
    }

    private void HandleCamera(Painter? debugPainter = null)
    {
        var player = World.Entities.Find(a => a is Player);

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
        var playerPosition = player?.Position ?? World.Goal.Position;
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