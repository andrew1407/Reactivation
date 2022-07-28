public interface IAmmoManager
{
    int Count { get; }

    int Left { get; }

    IAmmo GetFromPool();

    bool ReturnToPool(IAmmo ammo);

    void Refill();
}
