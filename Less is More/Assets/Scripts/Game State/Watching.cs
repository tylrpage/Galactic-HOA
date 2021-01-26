
using System.Collections;

public class Watching : State
{
    public Watching(StateMachine stateMachine) : base(stateMachine)
    {
    }
    
    public override IEnumerator Start()
    {
        // set status text
        yield break;
    }

    public override void Update()
    {
        // if players in circle >= 2, start timer to end
        return;
    }
}