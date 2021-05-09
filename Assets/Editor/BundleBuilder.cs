using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleBuilder : Editor
{
    [MenuItem("Assets/Build AssetBundles")]

    static void BuildAsset()
    {
        BuildPipeline.BuildAssetBundles("Assets", BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
    }
}