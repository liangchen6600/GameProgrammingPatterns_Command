using UnityEngine;

public class MoveLeft : Move, ICommand
{
    public void excute()
    {
        _agent.transform.position += Vector3.left;
    }

    public void undo()
    {
        _agent.transform.position -= Vector3.left;
    }

    public MoveLeft(GameObject agent) { _agent = agent; }
}
