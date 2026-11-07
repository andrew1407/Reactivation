using UnityEngine;

public sealed class TouchUtils
{
    #region Stick

    public static Vector2 ToStickValue(Vector2 offset, float radius) => Vector2.ClampMagnitude(offset / radius, 1);

    #endregion

    #region LookArea

    public static bool IsPinch(float gapChange, float pinchDistance) => Mathf.Abs(gapChange) >= pinchDistance;

    // Both fingers have to move, against each other: a resting one or two swiping alongside aren't a pinch
    public static bool IsPinchStart(Vector2 firstTravel, Vector2 secondTravel, float minTravel) =>
        firstTravel.magnitude >= minTravel && secondTravel.magnitude >= minTravel && Vector2.Dot(firstTravel, secondTravel) < 0;

    #endregion

    #region Layout

    // The scale that keeps controls reaching left of a point (its distance from the right edge) out of the stick's zone
    public static float ToStickZoneFit(float width, float reach, float pointFromRight, float stickZoneShare) =>
        Mathf.Clamp01((width * (1 - stickZoneShare) - pointFromRight) / reach);

    #endregion

    #region WeaponCross

    public static Vector2 ToWeaponSlot(Vector2 direction) => Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
        ? new Vector2(Mathf.Sign(direction.x), 0)
        : new Vector2(0, Mathf.Sign(direction.y));

    public static bool IsSwipe(Vector2 travel, float swipeDistance) => travel.magnitude >= swipeDistance;

    public static bool IsOnRing(Vector2 position, float radius) => position.magnitude <= radius;

    #endregion
}
