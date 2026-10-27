using System;
using UnityEngine;
using Unity.Cinemachine;

[Serializable]
public struct CameraMovementParams
{
    public float TurnFollowSpeed;

    public float SmoothSpeed;

    public Transform Target;

    public AxisState CameraX;

    public AxisState CameraY;
}
