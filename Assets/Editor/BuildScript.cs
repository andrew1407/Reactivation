using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

// Command-line builds for local checks, e.g.
// Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod BuildScript.Build -buildPath Builds/WebGL
public static class BuildScript
{
    public static void Build()
    {
        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        string path = GetArgument("-buildPath") ?? $"Builds/{target}";
        if (target is BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64) path += "/Reactivation.exe";
        else if (target is BuildTarget.StandaloneLinux64) path += "/Reactivation.x86_64";
        else if (target is BuildTarget.StandaloneOSX) path += "/Reactivation.app";

        BuildPlayerOptions options = new()
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = path,
            target = target,
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Console.WriteLine($"Build {report.summary.result}: {path} ({report.summary.totalSize / (1024 * 1024)} MB)");
        if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }

    private static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }
}
