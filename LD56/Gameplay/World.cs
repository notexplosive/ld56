using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LD56.Gameplay;

public class World
{
    public Goal Goal { get; private set; }
    public Player Player { get; private set; }

    public World()
    {
        Player = new Player(this);
        Goal = new Goal(Player);
    }

    public List<Entity> Entities { get; } = new();

    public void LoadLevel(Level level)
    {
        Entities.Clear();

        CreatePlayerAndGoal(level.GoalSpawnPosition.ToVector2());
        
        LoadLevelWithOffset(level, Vector2.Zero);
    }

    private void CreatePlayerAndGoal(Vector2 spawnPosition)
    {
        Player = new Player(this);
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
    }
}
