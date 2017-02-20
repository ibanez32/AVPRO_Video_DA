using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMP;

public class UMPPreference
{
    private static UMPPreference _instance;
    private static PreloadedSettings _preloadedSettings;

    private static bool _preloaded = false;
    private static string _additionalLibsPath = string.Empty;
    private static bool _nativeLibraryPrepare = false;

    public static UMPPreference Instance
    {
        get
        {
            if (_instance == null)
                _instance = new UMPPreference();
            return _instance;
        }
    }

    public bool UseExternalLibs
    {
        get { return _preloadedSettings.UseExternalLibs; }
    }

    public bool UseAndroidNative
    {
        get { return _preloadedSettings.UseAndroidNative; }
    }

    private UMPPreference()
    {
        _preloadedSettings = PreloadedSettings.Instance;
    }

    [PreferenceItem("UMP")]
    public static void UMPGUI()
    {
        if (!_preloaded)
        {
            _preloadedSettings = PreloadedSettings.Instance;
            _preloaded = true;
        }

        EditorGUILayout.LabelField("Editor/Desktop platforms:", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        _preloadedSettings.UseAudioSource = EditorGUILayout.Toggle(new GUIContent("Use Unity 'Audio Source' component", "Will be using Unity 'Audio Source' component for audio output for all UMP instances (global) by default."), _preloadedSettings.UseAudioSource);

        EditorGUILayout.Space();

        _preloadedSettings.UseDebugMode = EditorGUILayout.Toggle(new GUIContent("Debug mode", "Allows you to use debug mode in your IDE with UMP asset in Unity Editor (it's still experimental and will work only in Unity Editor, also some default UMP functions will not work in this mode, because it's conflicting with IDE debug mode)."), _preloadedSettings.UseDebugMode);

        EditorGUILayout.Space();

        _preloadedSettings.UseExternalLibs = EditorGUILayout.Toggle(new GUIContent("Use installed VLC libraries", "Will be using external/installed VLC player libraries for all UMP instances (global). Path to install VLC directory will be obtained automatically (you can also setup your custom path)."), _preloadedSettings.UseExternalLibs);

        var chachedLabelColor = EditorStyles.label.normal.textColor;
        EditorStyles.label.wordWrap = true;
        EditorStyles.label.normal.textColor = Color.red;

        if (MediaPlayerHelper.GetLibsPath(false).Equals(string.Empty))
        {
            EditorGUILayout.LabelField("Please correctly import UMP (Win, Mac, Linux) package to use internal VLC libraries.");
            _preloadedSettings.UseExternalLibs = true;
        }

        EditorGUILayout.Space();

        if (_preloadedSettings.UseExternalLibs)
        {
            string externalLibsPath = MediaPlayerHelper.GetLibsPath(true);
            if (externalLibsPath.Equals(string.Empty))
            {
                EditorGUILayout.LabelField("Did you install VLC player software correctly? Please make sure that:");
                EditorGUILayout.LabelField("1. Your installed VLC player bit application == Unity Editor bit application (VLC player 64-bit == Unity 64-bit Editor);");
                EditorGUILayout.LabelField("2. Use last version installer from official site: ");

                var link = "https://www.videolan.org/vlc/index.ru.html";
                EditorStyles.label.normal.textColor = Color.blue;
                EditorGUILayout.LabelField(link);

                Rect linkRect = GUILayoutUtility.GetLastRect();

                if (Event.current.type == EventType.MouseUp && linkRect.Contains(Event.current.mousePosition))
                    Application.OpenURL(link);

                EditorStyles.label.normal.textColor = Color.red;
                EditorGUILayout.LabelField("Or you can try to use custom additional path to your VLC libraries.");

                EditorGUILayout.Space();
            }

            EditorStyles.label.normal.textColor = chachedLabelColor;

            EditorGUILayout.LabelField(new GUIContent("External/installed VLC libraries path:", "Default path to installed VLC player libraries. Example: '" + @"C:\Program Files\VideoLAN\VLC'."));
            GUIStyle pathLabel = EditorStyles.textField;
            pathLabel.wordWrap = true;
            EditorGUILayout.LabelField(externalLibsPath, pathLabel);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(new GUIContent("Additional external/installed VLC libraries path:", "Additional path to installed VLC player libraries. Will be used if path to libraries can't be automatically obtained. Example: '" + @"C:\Program Files\VideoLAN\VLC'."));
            GUIStyle additionalLabel = EditorStyles.textField;
            additionalLabel.wordWrap = true;

            _preloadedSettings.AdditionalLibsPath = EditorGUILayout.TextField(_additionalLibsPath, _preloadedSettings.AdditionalLibsPath);
        }

        EditorStyles.label.normal.textColor = chachedLabelColor;

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Mobile platforms:", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        _preloadedSettings.UseAndroidNative = EditorGUILayout.Toggle(new GUIContent("Use Android native player", "Will be using Android native media player for all UMP instances (global)."), _preloadedSettings.UseAndroidNative);

        if (_nativeLibraryPrepare != _preloadedSettings.UseAndroidNative)
        {
            List<string> libs = new List<string>();
            libs.AddRange(Directory.GetFiles(Application.dataPath + "/UniversalMediaPlayer/Plugins/Android/libs/armeabi-v7a"));
            libs.AddRange(Directory.GetFiles(Application.dataPath + "/UniversalMediaPlayer/Plugins/Android/libs/x86"));

            foreach (var lib in libs)
            {
                if (lib.Contains(".meta") && !lib.Contains("libUniversalMediaPlayer"))
                {
                    File.SetAttributes(lib, FileAttributes.Normal);
                    string metaData = File.ReadAllText(lib);
                    var match = Regex.Match(metaData, @"Android.*\s*enabled:.");

                    if (match.Success)
                    {
                        metaData = Regex.Replace(metaData, @"Android.*\s*enabled:." + (!_nativeLibraryPrepare ? 1 : 0), match.Value + (_nativeLibraryPrepare ? 1 : 0));
                        File.WriteAllText(lib, metaData);
                    }
                }
            }
            libs.Clear();
        }

        _nativeLibraryPrepare = _preloadedSettings.UseAndroidNative;

        if (GUI.changed)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
