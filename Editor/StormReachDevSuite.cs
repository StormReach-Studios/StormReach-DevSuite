// © 2025 StormReach Studios

using UnityEngine;
using UnityEditor;
using StormReachStudios.DevSuite.UnityToolbarExtender;

namespace StormReachStudios.DevSuite
{
    [InitializeOnLoad]
    public static class StormReachDevSuite
    {
        const string ICON_PATH = "Packages/com.stormreachstudios.stormreachdevsuite/Editor/Icons/";

        static Texture2D ICON_COMPILELOCK_FORCE;
        static Texture2D ICON_COMPILELOCK_START;
        static Texture2D ICON_COMPILELOCK_STOP;
        static Texture2D ICON_COMPILELOCK;

        static bool IS_TOGGLE_LOCK
        {
            get => EditorPrefs.GetBool("StormReachDevSuite_ToggleLock", false);
            set => EditorPrefs.SetBool("StormReachDevSuite_ToggleLock", value);
        }

        static StormReachDevSuite()
        {
            // LOAD ICONS
            ICON_COMPILELOCK_FORCE = EditorGUIUtility.IconContent(ICON_PATH + "T_Icon_CompileLock_Force.png").image as Texture2D;
            ICON_COMPILELOCK_START = EditorGUIUtility.IconContent(ICON_PATH + "T_Icon_CompileLock_Start.png").image as Texture2D;
            ICON_COMPILELOCK_STOP = EditorGUIUtility.IconContent(ICON_PATH + "T_Icon_CompileLock_Stop.png").image as Texture2D;
            ICON_COMPILELOCK = IS_TOGGLE_LOCK ? ICON_COMPILELOCK_START : ICON_COMPILELOCK_STOP;

            // ADD BUTTONS
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        static void OnToolbarGUI()
        {
            GUILayout.Space(6);

            // USE GUIContent TO SETUP THE ICONS
            if (GUILayout.Button(new GUIContent(ICON_COMPILELOCK_FORCE, "Force Compile Now"), GUILayout.Width(30), GUILayout.Height(21)))
            {
                CompileLock.ForceCompile();
            }

            if (GUILayout.Button(new GUIContent(ICON_COMPILELOCK, "Toggle Compile Lock"), GUILayout.Width(30), GUILayout.Height(21)))
            {
                IS_TOGGLE_LOCK = CompileLock.ToggleLock();
                ICON_COMPILELOCK = IS_TOGGLE_LOCK ? ICON_COMPILELOCK_START : ICON_COMPILELOCK_STOP;
            }
        }
    }
}