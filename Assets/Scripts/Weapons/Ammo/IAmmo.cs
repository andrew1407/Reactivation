using UnityEngine;

public interface IAmmo
{
    void Use(Vector3 force);

    void BindState(bool interractive, Transform parent = null);
}
