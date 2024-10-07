using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using LD56.CartridgeManagement;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace LD56.Gameplay;

public class World
{
    private int _levelIndex;
    public bool HasRequestedReload { get; set; }
    public bool HasSpawnedAtLeastOnce { get; set; }

    public Goal Goal { get; private set; }
    public Player? Player => Goal.Player;

    public World()
    {
        Goal = new Goal(this);
    }

    public List<Entity> Entities { get; } = new();

    public void LoadLevel(Level level)
    {
        Entities.Clear();

        CreatePlayerAndGoal(level.GoalSpawnPosition.ToVector2());
        
        LoadLevelWithOffset(level, Vector2.Zero);
    }

    public void CreatePlayerAndGoal(Vector2 spawnPosition)
    {
        Goal = new Goal(this);
        Goal.Position = spawnPosition;
        
        Entities.Add(Goal);
    }

    private Entity CreateWall(Vector2 wallPosition, float wallRadius)
    {
        var obstacle = new Obstacle(wallRadius);
        obstacle.Position = wallPosition;
        return obstacle;
    }
    
    private Entity CreateCurrent(Vector2 position, float wallRadius, float angle)
    {
        var obstacle = new Current(this, wallRadius, angle);
        obstacle.Position = position;
        return obstacle;
    }

    public void LoadCurrentLevel(bool spawnNewPlayer)
    {
        var level = JsonConvert.DeserializeObject<Level>(
                        Client.Debug.RepoFileSystem.ReadFile($"Levels/level{_levelIndex}.json")) ??
                    new Level();
        
        if (spawnNewPlayer)
        {
            CreatePlayerAndGoal(level.GoalSpawnPosition.ToVector2());
        }
        
        LoadLevelSeamless(level);
    }
    
    public void LoadLevelSeamless(Level level)
    {
        Entities.RemoveAll(a => a is not Gameplay.Player && a is not LD56.Gameplay.Goal);

        var offset = Goal.Position - level.GoalSpawnPosition.ToVector2();
        LoadLevelWithOffset(level, offset);
    }

    private void LoadLevelWithOffset(Level level, Vector2 offset)
    {
        foreach (var wall in level.Obstacles)
        {
            Entities.Add(CreateWall(wall.Position.ToVector2() + offset, wall.Radius));
        }
        
        foreach (var currentDAta in level.Currents)
        {
            Entities.Add(CreateCurrent(currentDAta.Position.ToVector2() + offset, currentDAta.Radius, currentDAta.Angle));
        }

        foreach (var coin in level.Foods)
        {
            var food = new Food();
            food.Position = coin.ToVector2() + offset;
            Entities.Add(food);
        }

        foreach (var enemySpawn in level.Enemies)
        {
            var enemy = new Enemy(this);
            enemy.Position = enemySpawn.ToVector2() + offset;
            Entities.Add(enemy);
        }
        
        Entities.Add(new ObstacleRenderer(this));
    }

    public void PlayerDied()
    {
        LdResourceAssets.Instance.PlaySound("player_die", new SoundEffectSettings());
        Goal.KillPlayer();
    }

    public void LoadNextLevel()
    {
        _levelIndex++;
        Goal.IncreaseAuraLevel();
        LoadCurrentLevel(false);
    }

    public void RequestReload()
    {
        HasRequestedReload = true;
    }

    public void SkipLevel()
    {
        _levelIndex++;
        LoadCurrentLevel(false);
        Client.Debug.Log($"Skipped to level: {_levelIndex}");
    }
}