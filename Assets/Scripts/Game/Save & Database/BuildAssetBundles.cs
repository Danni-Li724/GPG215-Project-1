#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine;

public static class BuildAssetBundles
{
    [MenuItem("Assets/Build Node Particle Bundles")]
    public static void Build()
    {
        string outputPath = Path.Combine(Application.streamingAssetsPath, "NodeBundles");

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        BuildPipeline.BuildAssetBundles(
            outputPath,
            BuildAssetBundleOptions.None,
            BuildTarget.Android
        );

        AssetDatabase.Refresh();
        Debug.Log($"Asset bundles built to: {outputPath}");
    }
}
#endif