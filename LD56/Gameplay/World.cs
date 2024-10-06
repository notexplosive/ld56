﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class World
{
    public Goal Goal { get; private set; }
    public Worm Player { get; private set; }

    public World()
    {
        Player = new Worm(this);
        Goal = new Goal(Player);
    }

    public List<Entity> Entities { get; } = new();

    public void LoadLevel(Level level)
    {
        Entities.Clear();

        foreach (var wall in level.Obstacles)
        {
            Entities.Add(CreateWall(wall.Position.ToVector2(), wall.Radius));
        }

        foreach (var coin in level.Foods)
        {
            var food = new Food();
            food.Position = coin.ToVector2();
            Entities.Add(food);
        }

        CreatePlayerAndGoal(level.GoalSpawnPosition.ToVector2());
    }

    private void CreatePlayerAndGoal(Vector2 spawnPosition)
    {
        Player = new Worm(this);
        Player.Position = spawnPosition;
        Player.MoveAllTailSegmentsToHead();
        
        Goal = new Goal(Player);
        Goal.Position = spawnPosition;
        
        Entities.Add(Goal);
        Entities.Add(Player);
    }

    private Entity CreateWall(Vector2 wallPosition, float wallRadius)
    {
        var obstacle = new Obstacle(wallRadius);
        obstacle.Position = wallPosition;
        return obstacle;
    }

    public void LoadLevelSeamless(Level level)
    {
        Entities.RemoveAll(a => a is Obstacle);
        Entities.RemoveAll(a => a is Food);

        var offset = Goal.Position - level.GoalSpawnPosition.ToVector2();
        foreach (var wall in level.Obstacles)
        {
            Entities.Add(CreateWall(wall.Position.ToVector2() + offset, wall.Radius));
        }

        foreach (var coin in level.Foods)
        {
            var food = new Food();
            food.Position = coin.ToVector2() + offset;
            Entities.Add(food);
        }
    }
}
