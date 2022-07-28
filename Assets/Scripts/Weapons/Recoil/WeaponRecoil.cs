using UnityEngine;
using Zenject;

public class WeaponRecoil : IRecoilable, ITickable
{
    [Inject] private readonly CameraMovement _cameraMovement;

    private float _timeLeft = 0;

    private RecoilParams _currentRecoil;

    private float _verticalRecoil { get => _currentRecoil.VerticalRecoil * Time.deltaTime / _currentRecoil.DurationSeconds; }

    public void Activate(RecoilParams recoilParams)
    {
        _currentRecoil = recoilParams;
        _timeLeft = _currentRecoil.DurationSeconds;
    }

    public void Tick()
    {
        if (_timeLeft < 0) _timeLeft = 0;
        if (_timeLeft == 0)
        {
            if (!_currentRecoil.Equals(default)) _currentRecoil = default;
            return;
        }
        _cameraMovement.CameraValues -= new Vector2(0,  _verticalRecoil);
        _timeLeft -= Time.deltaTime;
    }
}
