using UnityEngine;

public class MoveForward : Move, ICommand
{
    public void excute()
    {
        _agent.transform.position += Vector3.forward;
    }

    public void undo()
    {
        _agent.transform.position -= Vector3.forward;
    }

    public MoveForward(GameObject agent) { _agent = agent; }
}
