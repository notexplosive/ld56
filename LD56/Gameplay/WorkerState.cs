namespace LD56.Gameplay;

public abstract class WorkerState
{
    public abstract void Update(float dt);
    
    public bool IsFinished { get; private set; }

    protected void MarkAsFinished()
    {
        IsFinished = true;
    }
    
    
}
