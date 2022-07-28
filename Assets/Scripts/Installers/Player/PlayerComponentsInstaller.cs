using UnityEngine;
using Zenject;

public class PlayerComponentsInstaller : MonoInstaller
{
    [SerializeField] private CharacterController _characterController;

    [SerializeField] private CharacterComponents _characterComponents;

    [SerializeField] private MotionParams _motionParams;

    [SerializeField] private CameraMovementParams _cameraMovementParams;

    public override void InstallBindings()
    {
        Container.BindInstance(_characterComponents);

        Container.Bind<PlayerMotionControl>()
            .AsSingle()
            .WithArguments(_characterComponents.BodyAnimator, _characterController, _motionParams);

        Container.Bind<LocomotionStateMachine>().AsSingle();

        Container.BindInterfacesAndSelfTo<CameraMovement>()
            .FromSubContainerResolve()
            .ByMethod(installCameraMovement)
            .AsSingle();
    }

    private void installCameraMovement(DiContainer subContainer)
    {
        subContainer.Bind<CameraMovement>().AsSingle();
        subContainer.BindInstance(_cameraMovementParams);
    }
}