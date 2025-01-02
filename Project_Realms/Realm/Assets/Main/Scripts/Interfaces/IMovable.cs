using UnityEngine;

public interface IMovable
{
    float MoveSpeed { get; }
    bool IsMoving { get; }
    void MoveTo(Vector3 destination);
    void StopMoving();
}