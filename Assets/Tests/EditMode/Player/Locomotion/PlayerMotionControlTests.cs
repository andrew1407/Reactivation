using NUnit.Framework;
using UnityEngine;

public class PlayerMotionControlTests
{
    [Test]
    public void Should_Push_Along_Horizontal_Move_Direction()
    {
        Vector3 direction = PlayerMotionControl.ToPushDirection(new Vector3(0.6f, -0.5f, 0.8f));

        Assert.AreEqual(new Vector3(0.6f, 0, 0.8f), direction, "A push should keep x and z and stay level");
    }

    [Test]
    public void Should_Push_Forward_When_Moving_Forward()
    {
        Vector3 direction = PlayerMotionControl.ToPushDirection(Vector3.forward);

        Assert.AreEqual(Vector3.forward, direction, "Walking straight into an obstacle should push it");
    }
}
