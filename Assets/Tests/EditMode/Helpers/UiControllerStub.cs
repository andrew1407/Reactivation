using System.Reflection;
using TMPro;
using UnityEngine;

// A UiController with its serialized text fields wired to fresh components, so the logic that
// writes to the HUD can run outside the scene and the written text can be read back
public class UiControllerStub
{
    public readonly UiController Controller;

    private readonly GameObject _gameObject;

    public UiControllerStub()
    {
        _gameObject = new GameObject(nameof(UiControllerStub));
        Controller = _gameObject.AddComponent<UiController>();
        foreach (var field in typeof(UiController).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (field.FieldType != typeof(TextMeshProUGUI)) continue;
            var child = new GameObject(field.Name);
            child.transform.SetParent(_gameObject.transform);
            field.SetValue(Controller, child.AddComponent<TextMeshProUGUI>());
        }
    }

    public string Text(string fieldName)
    {
        var field = typeof(UiController).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return ((TextMeshProUGUI)field.GetValue(Controller)).text;
    }

    public void Destroy() => Object.DestroyImmediate(_gameObject);
}
