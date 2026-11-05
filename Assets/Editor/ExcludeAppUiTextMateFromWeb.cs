using UnityEditor;

// App UI (pulled in by Inference Engine) ships a prebuilt TextMate syntax highlighter for the web
// that was compiled against an older Emscripten: it calls embind symbols (_emval_as,
// _emval_take_value) this editor's toolchain no longer has, so linking it fails the web build.
// Nothing in the player uses it (its C# side only compiles with APPUI_ENABLE_SYNTAX_HIGHLIGHTING),
// and package files are read-only, so it is left out of builds here instead.
[InitializeOnLoad]
public sealed class ExcludeAppUiTextMateFromWeb
{
    private const string _pluginPath = "Packages/com.unity.dt.app-ui/Runtime/Core/Platform/WebGL/Plugins/libAppUITextMateLib.a";

    static ExcludeAppUiTextMateFromWeb()
    {
        if (AssetImporter.GetAtPath(_pluginPath) is PluginImporter importer)
            importer.SetIncludeInBuildDelegate(_ => false);
    }
}
