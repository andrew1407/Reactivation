using System.Collections;

public interface IWeapon
{
    string Label { get; }

    IEnumerator Action();
}

public interface IAmmoContainer
{
    void Setup();

    IEnumerator Reload();

    void RefillAmmo();
}
