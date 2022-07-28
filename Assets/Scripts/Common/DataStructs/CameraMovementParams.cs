using System;
using UnityEngine;
using Cinemachine;

[Serializable]
public struct CameraMovementParams
{
    public float TurnFollowSpeed;

    public float SmoothSpeed;

    public Transform Target;

    public AxisState CameraX;

    public AxisState CameraY;
}
