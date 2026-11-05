using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

// Player builds from the Build menu, or from the command line (build.ps1 drives this), e.g.
// Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod BuildScript.Build -buildPath Builds/WebGL
public sealed class BuildScript
{
    private const string _buildsDirectory = "Builds";

    // The file a target builds into; the others fill the build's directory
    private static readonly Dictionary<BuildTarget, string> _playerFiles = new() {
        {BuildTarget.StandaloneWindows, "Reactivation.exe"},
        {BuildTarget.StandaloneWindows64, "Reactivation.exe"},
        {BuildTarget.StandaloneLinux64, "Reactivation.x86_64"},
        {BuildTarget.StandaloneOSX, "Reactivation.app"},
        {BuildTarget.Android, "Reactivation.apk"},
    };

    [MenuItem("Build/Windows")]
    public static void BuildWindows() => BuildPlayer(BuildTarget.StandaloneWindows64, $"{_buildsDirectory}/Windows");

    [MenuItem("Build/Linux")]
    public static void BuildLinux() => BuildPlayer(BuildTarget.StandaloneLinux64, $"{_buildsDirectory}/Linux");

    [MenuItem("Build/macOS")]
    public static void BuildMacOS() => BuildPlayer(BuildTarget.StandaloneOSX, $"{_buildsDirectory}/macOS");

    [MenuItem("Build/WebGL")]
    public static void BuildWebGL() => BuildPlayer(BuildTarget.WebGL, $"{_buildsDirectory}/WebGL");

    [MenuItem("Build/Android")]
    public static void BuildAndroid() => BuildPlayer(BuildTarget.Android, $"{_buildsDirectory}/Android");

    // An Xcode project, which takes a Mac to turn into the app
    [MenuItem("Build/iOS")]
    public static void BuildIOS() => BuildPlayer(BuildTarget.iOS, $"{_buildsDirectory}/iOS");

    // Command line entry: builds the target picked with -buildTarget into -buildPath
    public static void Build()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        bool wasBuilt = BuildPlayer(target, getArgument("-buildPath") ?? $"{_buildsDirectory}/{target}");
        if (!wasBuilt) EditorApplication.Exit(1);
    }

    public static bool BuildPlayer(BuildTarget target, string directory)
    {
        string path = _playerFiles.TryGetValue(target, out string file) ? $"{directory}/{file}" : directory;
        BuildPlayerOptions options = new()
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = path,
            target = target,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Console.WriteLine($"Build {report.summary.result}: {path} ({report.summary.totalSize / (1024 * 1024)} MB)");
        return report.summary.result == BuildResult.Succeeded;
    }

    private static string getArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
