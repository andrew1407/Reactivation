using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

// Player builds from the Build menu, or from the command line (build.ps1 drives this), e.g.
// Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod BuildScript.Build -buildPath Builds/WebGL
public static class BuildScript
{
    private const string BuildsDirectory = "Builds";

    [MenuItem("Build/Windows")]
    public static void BuildWindows() => BuildPlayer(BuildTarget.StandaloneWindows64, $"{BuildsDirectory}/Windows");

    [MenuItem("Build/Linux")]
    public static void BuildLinux() => BuildPlayer(BuildTarget.StandaloneLinux64, $"{BuildsDirectory}/Linux");

    [MenuItem("Build/macOS")]
    public static void BuildMacOS() => BuildPlayer(BuildTarget.StandaloneOSX, $"{BuildsDirectory}/macOS");

    [MenuItem("Build/WebGL")]
    public static void BuildWebGL() => BuildPlayer(BuildTarget.WebGL, $"{BuildsDirectory}/WebGL");

    // Command line entry: builds the target picked with -buildTarget into -buildPath
    public static void Build()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        bool succeeded = BuildPlayer(target, GetArgument("-buildPath") ?? $"{BuildsDirectory}/{target}");
        if (!succeeded) EditorApplication.Exit(1);
    }

    public static bool BuildPlayer(BuildTarget target, string directory)
    {
        string path = target switch
        {
            BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => $"{directory}/Reactivation.exe",
            BuildTarget.StandaloneLinux64 => $"{directory}/Reactivation.x86_64",
            BuildTarget.StandaloneOSX => $"{directory}/Reactivation.app",
            _ => directory,
        };
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

    private static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
