using UnityEditor;

// App UI (pulled in by Inference Engine) ships a prebuilt TextMate syntax highlighter for the web
// that was compiled against an older Emscripten: it calls embind symbols (_emval_as,
// _emval_take_value) this editor's toolchain no longer has, so linking it fails the web build.
// Nothing in the player uses it (its C# side only compiles with APPUI_ENABLE_SYNTAX_HIGHLIGHTING),
// and package files are read-only, so it is left out of builds here instead.
[InitializeOnLoad]
public static class ExcludeAppUiTextMateFromWeb
{
    private const string PluginPath = "Packages/com.unity.dt.app-ui/Runtime/Core/Platform/WebGL/Plugins/libAppUITextMateLib.a";

    static ExcludeAppUiTextMateFromWeb()
    {
        if (AssetImporter.GetAtPath(PluginPath) is PluginImporter importer)
            importer.SetIncludeInBuildDelegate(_ => false);
    }
}
