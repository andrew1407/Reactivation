using System;
using UnityEngine;

// Whether the player is on the on-screen controls: they're the default on mobile, using a gamepad
// (or a keyboard) hides them, and touching the screen again brings them back
public class TouchMode
{
    private readonly bool _isSupported;

    private readonly bool _isMobile;

    public event Action<bool> Changed;

    public bool IsActive { get; private set; }

    // A phone's screen stays small with a gamepad in hand, so its text outlives the on-screen controls
    public bool IsTextEnlarged { get => _isMobile || IsActive; }

    // The web player counts as supported on desktop too: an iPad's browser presents itself as a desktop one,
    // so there it takes the first touch to find out. Desktop players stay out of it altogether
    public static TouchMode ForCurrentPlatform()
    {
        bool isMobile = UnityEngine.Device.Application.isMobilePlatform;
        bool isWeb = UnityEngine.Device.Application.platform == RuntimePlatform.WebGLPlayer;
        return new TouchMode(isSupported: isMobile || isWeb, isActive: isMobile, isMobile: isMobile);
    }

    public TouchMode(bool isSupported, bool isActive, bool isMobile = false)
    {
        _isSupported = isSupported;
        _isMobile = isMobile;
        IsActive = isActive;
    }

    public void OnTouch() => setActive(true);

    public void OnOtherDevice() => setActive(false);

    private void setActive(bool isActive)
    {
        if (!_isSupported || isActive == IsActive) return;
        IsActive = isActive;
        Changed?.Invoke(isActive);
    }
}
