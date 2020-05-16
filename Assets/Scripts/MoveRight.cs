using UnityEngine;

public class MoveRight : Move, ICommand
{
    public void excute()
    {
        _agent.transform.position += Vector3.right;
    }

    public void undo()
    {
        _agent.transform.position -= Vector3.right;
    }

    public MoveRight(GameObject agent) { _agent = agent; }
}
