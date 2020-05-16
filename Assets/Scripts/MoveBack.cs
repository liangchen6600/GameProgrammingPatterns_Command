using UnityEngine;

public class MoveBack : Move, ICommand
{
    public void excute()
    {
        _agent.transform.position += Vector3.back;
    }

    public void undo()
    {
        _agent.transform.position -= Vector3.back;
    }

    public MoveBack(GameObject agent) { _agent = agent; }
}
