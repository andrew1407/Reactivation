using System;
using UnityEngine;

[Serializable]
public struct AmmoParams
{
    public uint Count;

    public float RefillDelay;

    public Transform Holder;

    public GameObject Prebab;
}

 [Serializable]
public struct AmmoAppliance
{
    public float LaunchForce;

    public float RetakePeriod;
}
