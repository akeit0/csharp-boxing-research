using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ResearchEditor
{
    public static void Run() { ResearchPlayer.Run(); }

    public static void Configure()
    {
        PlayerSettings.allowUnsafeCode = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Standard);
        AssetDatabase.SaveAssets();
    }

    public static void Build()
    {
        Configure();
        string backend = ResearchPlayer.Argument("--research-backend", "Mono2x");
        var implementation = (ScriptingImplementation)Enum.Parse(typeof(ScriptingImplementation), backend);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, implementation);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, ManagedStrippingLevel.Minimal);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Research.unity");
        string output = ResearchPlayer.Argument("--research-build-output", "Build/Research.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Research.unity" },
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        Debug.Log("RESEARCH BUILD " + report.summary.result + "; backend=" + backend);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build failed: " + report.summary.result);
    }
}
