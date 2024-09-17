using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.Callbacks;

using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

public class PostProcess : MonoBehaviour {

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath) {
        if (buildTarget == BuildTarget.iOS) {
            Debug.Log("Start OnPostprocessBuild");
            var projectPath = PBXProject.GetPBXProjectPath(buildPath);

            UpdateProject(buildTarget, projectPath);
            UpdateProjectPlist(buildTarget, buildPath + "/Info.plist");

            Debug.Log(Application.dataPath);
            //string srcFilePath2 = Application.dataPath + "/_Projects/Editor/IOSRequiredFiles/LaunchImage.launchimage";
            //DataCore.Debug.Log("src: " + srcFilePath2);
            //string dstFilePath2 = buildPath + "/Unity-iPhone/Images.xcassets/LaunchImage.launchimage";

            //FileUtil.DeleteFileOrDirectory(dstFilePath2);
            //FileUtil.CopyFileOrDirectory(srcFilePath2, dstFilePath2);
            UpdateCapabilities(buildPath);


            //LocalizeUserTrackingDescription("This only uses device info for less annoying, more relevant ads", "en", buildPath);
            //LocalizeUserTrackingDescription("下の [許可] をタップして広告体験をパーソナライズしてください", "ja", buildPath);
            //LocalizeUserTrackingDescription("Нажмите \\\"Разрешить\\\" ниже, чтобы персонализировать рекламу.", "ru", buildPath);
            //LocalizeUserTrackingDescription("点击下面的“允许”以个性化您的广告体验", "zh-Hans", buildPath);
            //LocalizeUserTrackingDescription("Appuyez sur \\\"Autoriser\\\" ci-dessous pour personnaliser votre expérience publicitaire", "fr", buildPath);
            //LocalizeUserTrackingDescription("Tippen Sie unten auf \\\"Zulassen\\\", um Ihre Anzeigenerfahrung zu personalisieren", "de", buildPath);
            Debug.Log("End OnPostprocessBuild");
        }
    }

    private static void LocalizeUserTrackingDescription(string localizedUserTrackingDescription, string localeCode, string buildPath)
    {
        var projectPath = PBXProject.GetPBXProjectPath(buildPath);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);
        string targetId = project.GetUnityMainTargetGuid();

        const string resourcesDirectoryName = "Resources";
        var resourcesDirectoryPath = Path.Combine(buildPath, resourcesDirectoryName);
        var localeSpecificDirectoryName = localeCode + ".lproj";
        var localeSpecificDirectoryPath = Path.Combine(resourcesDirectoryPath, localeSpecificDirectoryName);
        var infoPlistStringsFilePath = Path.Combine(localeSpecificDirectoryPath, "InfoPlist.strings");


        // Create intermediate directories as needed.
        if (!Directory.Exists(resourcesDirectoryPath))
        {
            Directory.CreateDirectory(resourcesDirectoryPath);
        }

        if (!Directory.Exists(localeSpecificDirectoryPath))
        {
            Directory.CreateDirectory(localeSpecificDirectoryPath);
        }

        var localizedDescriptionLine = "\"NSUserTrackingUsageDescription\" = \"" + localizedUserTrackingDescription + "\";\n";
        // File already exists, update it in case the value changed between builds.
        if (File.Exists(infoPlistStringsFilePath))
        {
            var output = new List<string>();
            var lines = File.ReadAllLines(infoPlistStringsFilePath);
            var keyUpdated = false;
            foreach (var line in lines)
            {
                if (line.Contains("NSUserTrackingUsageDescription"))
                {
                    output.Add(localizedDescriptionLine);
                    keyUpdated = true;
                }
                else
                {
                    output.Add(line);
                }
            }

            if (!keyUpdated)
            {
                output.Add(localizedDescriptionLine);
            }

            File.WriteAllText(infoPlistStringsFilePath, string.Join("\n", output.ToArray()) + "\n");
        }
        // File doesn't exist, create one.
        else
        {
            File.WriteAllText(infoPlistStringsFilePath, "/* Localized versions of Info.plist keys */\n" + localizedDescriptionLine);
        }
        DataCore.Debug.Log($"infoPlistStringsFilePath {infoPlistStringsFilePath}");
        var guid = project.AddFolderReference(localeSpecificDirectoryPath, Path.Combine(resourcesDirectoryName, localeSpecificDirectoryName));
        project.AddFileToBuild(targetId, guid);
        project.WriteToFile(projectPath);
    }

    private static void UpdateProject(BuildTarget buildTarget, string projectPath) {
        PBXProject project = new PBXProject();
        project.ReadFromString(File.ReadAllText(projectPath));

        string targetId = project.GetUnityMainTargetGuid();
        string frameworkGUID = project.GetUnityFrameworkTargetGuid();
        //Add here
        project.AddBuildProperty(targetId, "OTHER_LDFLAGS", "-ObjC");
        project.AddBuildProperty(targetId, "OTHER_LDFLAGS", "-l\"sqlite3\"");
        project.AddBuildProperty(targetId, "OTHER_LDFLAGS", "-l\"c++\"");
        project.AddFrameworkToProject(targetId, "AdSupport.Framework", false);
        project.AddFrameworkToProject(targetId, "iAd.Framework", false);
        project.AddFrameworkToProject(targetId, "UserNotifications.framework", false);
        project.AddFrameworkToProject(targetId, "WebKit.framework", false);

        project.SetBuildProperty(targetId, "ENABLE_BITCODE", "false");
        project.SetBuildProperty(targetId, "CLANG_ALLOW_NON_MODULAR_INCLUDES_IN_FRAMEWORK_MODULES", "YES");
        foreach (var targetGuid in new[] { targetId, project.GetUnityFrameworkTargetGuid() })
        {
            project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
        }
        project.SetBuildProperty(targetId, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

        File.WriteAllText(projectPath, project.WriteToString());
    }

    private static void UpdateProjectPlist(BuildTarget buildTarget, string plistPath) {
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // Get root
        PlistElementDict rootDict = plist.root;
        //Add here
        rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        //rootDict.SetBoolean("FirebaseMessagingAutoInitEnabled", false);
        rootDict.SetString("gad_preferred_webview", "wkwebview");
        rootDict.SetString("FirebaseMessagingAutoInitEnabled", "NO");
        rootDict.SetString("FirebaseAutomaticScreenReportingEnabled", "NO");
        rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://tenjin-skan.com");
        string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
        if (rootDict.values.ContainsKey(exitsOnSuspendKey)) {
            rootDict.values.Remove(exitsOnSuspendKey);
        }
        rootDict.SetString("NSCalendarsUsageDescription", "Used to deliver better advertising experience");
        //string userTrackingUsageDescription = "This only uses device info for less annoying, more relevant ads";
        //rootDict.SetString("NSUserTrackingUsageDescription", userTrackingUsageDescription);
        File.WriteAllText(plistPath, plist.WriteToString());
    }

    private static void UpdateCapabilities(string buildPath) {
        var projectPath = PBXProject.GetPBXProjectPath(buildPath);
        var project = new PBXProject();
        project.ReadFromFile(projectPath);
        var proCapabilitiesManager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", targetGuid: project.GetUnityMainTargetGuid());
        proCapabilitiesManager.AddInAppPurchase();
        proCapabilitiesManager.AddGameCenter();
        proCapabilitiesManager.AddPushNotifications(false);
        proCapabilitiesManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        proCapabilitiesManager.WriteToFile();
    }


}
#endif
