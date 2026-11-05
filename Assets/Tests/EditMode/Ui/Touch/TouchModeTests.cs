using NUnit.Framework;

public class TouchModeTests
{
    [Test]
    public void Should_Start_Active_On_Mobile()
    {
        Assert.IsTrue(new TouchMode(isSupported: true, isActive: true).IsActive, "Mobile should start on the touch controls");
    }

    [Test]
    public void Should_Hide_On_Gamepad_And_Come_Back_On_Touch()
    {
        var touchMode = new TouchMode(isSupported: true, isActive: true);

        touchMode.OnOtherDevice();
        Assert.IsFalse(touchMode.IsActive, "A gamepad should hide the controls");

        touchMode.OnTouch();
        Assert.IsTrue(touchMode.IsActive, "A touch should bring the controls back");
    }

    [Test]
    public void Should_Find_Out_About_Touch_Web_Devices_On_First_Touch()
    {
        var touchMode = new TouchMode(isSupported: true, isActive: false);

        touchMode.OnTouch();

        Assert.IsTrue(touchMode.IsActive, "The first touch should turn the controls on");
    }

    [Test]
    public void Should_Stay_Off_On_Desktop_Players()
    {
        var touchMode = new TouchMode(isSupported: false, isActive: false);

        touchMode.OnTouch();

        Assert.IsFalse(touchMode.IsActive, "Desktop players should never show the controls");
    }

    [Test]
    public void Should_Report_Only_Real_Changes()
    {
        var touchMode = new TouchMode(isSupported: true, isActive: true);
        int changes = 0;
        touchMode.Changed += _ => ++changes;

        touchMode.OnTouch();
        touchMode.OnOtherDevice();
        touchMode.OnOtherDevice();

        Assert.AreEqual(1, changes, "Only the actual switch should be reported");
    }

    [Test]
    public void Should_Keep_Text_Enlarged_On_Mobile_With_Gamepad()
    {
        var touchMode = new TouchMode(isSupported: true, isActive: true, isMobile: true);

        touchMode.OnOtherDevice();

        Assert.IsFalse(touchMode.IsActive, "A gamepad should hide the controls");
        Assert.IsTrue(touchMode.IsTextEnlarged, "Mobile text should stay large with a gamepad");
    }

    [Test]
    public void Should_Enlarge_Text_Only_While_On_Touch_On_Desktop_Web()
    {
        var touchMode = new TouchMode(isSupported: true, isActive: false);
        Assert.IsFalse(touchMode.IsTextEnlarged, "Text should start small before any touch");

        touchMode.OnTouch();
        Assert.IsTrue(touchMode.IsTextEnlarged, "A touch should enlarge the text");

        touchMode.OnOtherDevice();
        Assert.IsFalse(touchMode.IsTextEnlarged, "Leaving touch should shrink the text on desktop web");
    }

    [Test]
    public void Should_Not_Enlarge_Text_On_Desktop_Players()
    {
        var touchMode = new TouchMode(isSupported: false, isActive: false);

        touchMode.OnTouch();

        Assert.IsFalse(touchMode.IsTextEnlarged, "Desktop players should never enlarge the text");
    }
}
