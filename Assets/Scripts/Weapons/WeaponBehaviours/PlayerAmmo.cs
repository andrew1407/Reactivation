using UnityEngine;
using Zenject;

public abstract class PlayerAmmo : MonoBehaviour, IAmmo
{
    protected enum ItemState
    {
        FREE,
        USED,
        IDLE,
        CLEANUP_AWAIT,
    }

    [Header("Physics")]
    
    [SerializeField] protected Rigidbody _rigidBody;

    [SerializeField] protected Collider _collider;

    [SerializeField] protected GameObject _ammoView;

    [Inject] protected readonly AmmoParams _ammoParams;

    [Inject] protected readonly CharacterComponents _characterComponents;

    [Inject] private readonly PlayerAmmoObserver _ammoObserver;

    [Inject] private readonly UiController _uiController;

    protected IAmmoManager _ammoManager;

    protected string _label;

    private float _cleanupPeriodAccumulator = 0;

    protected ItemState _state = ItemState.IDLE;

    public void Use(Vector3 force)
    {
        _state = ItemState.FREE;
        setComponentsState(state: true);
        _rigidBody.AddForce(force, ForceMode.Impulse);
        transform.SetParent(null);
    }

    public void BindState(bool interractive, Transform parent = null)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        _ammoView.SetActive(interractive);
        setComponentsState(state: false);
        _state = ItemState.IDLE;
        _cleanupPeriodAccumulator = 0;
    }
    
    protected void setComponentsState(bool state)
    {
        _collider.enabled = state;
        _rigidBody.isKinematic = !state;
    }

    private void Update()
    {
        if (_state == ItemState.CLEANUP_AWAIT) awaitCleanpup();
    }

    private void awaitCleanpup()
    {
        if (_cleanupPeriodAccumulator < _ammoParams.RefillDelay)
        {
            _cleanupPeriodAccumulator += Time.deltaTime;
            return;
        }
        _state = ItemState.IDLE;
        if (_ammoManager.ReturnToPool(this)) _ammoObserver.OnAmmoChange(_label, _uiController.GetAmmo(_label) + 1);
        _cleanupPeriodAccumulator = 0;
    }
}

