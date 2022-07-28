using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

public sealed class GameUtils
{
    #region Animations

    public static bool IsAnimationPlaying(Animator animator, int layer = 0)
    {
        return animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1;
    }

    public static IEnumerator WaitCurrentAnimationFinish(Animator animator, int layer = 0)
    {
        do yield return null;
        while (IsAnimationPlaying(animator, layer));
    }

    public static IEnumerator PlayAnimation(Animator animator, int labelHash, int layer = 0)
    {
        animator.Play(labelHash);
        return WaitCurrentAnimationFinish(animator, layer);
    }

    #endregion

    #region Gameplay

    public static bool PlayerDefeated(WaveState state) => state == WaveState.DIED || state == WaveState.TIME_IS_OVER;

    public static Dictionary<DamageType, float> DefaultModifications() => new() {
        {DamageType.FIST, 1},
        {DamageType.MACHETE, 1},
        {DamageType.ARROW, 1},
        {DamageType.GRENADE, 1},
    };

    #endregion

    #region Math

    public static float Mean(List<float> data) => data.Sum() / (float)data.Count;

    public static float Variance(List<float> data, float ddof = 0)
    {
        float mean = Mean(data);
        float sum = data.Select(x => MathF.Pow(x - mean, 2)).Sum();
        return sum / ((float)data.Count - ddof);
    }

    public static float Stdev(List<float> data)
    {
        float varience = Variance(data);
        return MathF.Sqrt(varience);
    }

    public static float FormatModifierValue(float value)
    {
        float rounded = MathF.Round(value, digits: 2);
        return Mathf.Clamp(rounded, min: 0.01f, max: 1);
    }
    
    #endregion

    #region Camera

    public static float ClampAxisValue(ref AxisState axisState, float value)
    {
        float max = axisState.m_MaxValue;
        float min = axisState.m_MinValue;
        return Mathf.Clamp(value, min, max);
    }

    public static float RoundClampAxisValue(ref AxisState axisState, float value)
    {
        float max = axisState.m_MaxValue;
        float min = axisState.m_MinValue;
        if (value > max) return value - max + min;
        if (value < min) return value + max - min;
        return value;
    }
    
    #endregion
}
