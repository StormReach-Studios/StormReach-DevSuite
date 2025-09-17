// © 2025 StormReach Studios

using UnityEditor;
using UnityEngine;

namespace StormReachStudios.DevSuite
{
    [InitializeOnLoad]
    public static class CompileLock
    {
        // "EditorPrefs" KEYS (PERSISTED ACROSS EDITOR RESTARTS / DOMAIN RELOADS)
        const string PREF_LOCKED = "CompileLock_Pref_IsLocked";
        const string PREF_PENDING_RELOCK = "CompileLock_Pref_PendingRelock";

        static CompileLock()
        {
            // ON LOAD, IF A RELOCK WAS PENDING, WAIT AND RE-LOCK AFTER COMPILATION FINISHES.
            if (IsPendingRelock())
            {
                EditorApplication.update += OnEditorUpdate_WaitPendingRelock;
            }
            else
            {
                // IF THE USER WANTED THE LOCK TO BE ACTIVE, REAPPLY IT (UNLESS UNITY IS CURRENTLY COMPILING).
                if (IsDesiredLocked())
                {
                    if (!EditorApplication.isCompiling)
                    {
                        EditorApplication.LockReloadAssemblies();
                        Debug.Log("[StormReachDevSuite] LOCK REAPPLIED ON EDITOR LOAD.");
                    }
                    else
                    {
                        // IF COMPILING ON LOAD, WAIT AND APPLY WHEN DONE
                        Debug.Log("[StormReachDevSuite] DESIRED LOCK IS TRUE BUT COMPILE IS RUNNING — WILL REAPPLY WHEN FINISHED.");
                        EditorApplication.update += OnEditorUpdate_ApplyDesiredLock;
                    }
                }
            }
        }

        public static bool ToggleLock()
        {
            if (IsDesiredLocked())
            {
                EditorApplication.UnlockReloadAssemblies();
                SetDesiredLocked(false);
                Debug.Log("[StormReachDevSuite] ✅ COMPILATION UNLOCKED.");
                return true;
            }
            else
            {
                EditorApplication.LockReloadAssemblies();
                SetDesiredLocked(true);
                Debug.Log("[StormReachDevSuite] ⛔ COMPILATION LOCKED.");
                return false;
            }
        }

        public static void ForceCompile()
        {
            if (!IsDesiredLocked())
            {
                // NOT LOCKED: JUST REFRESH AND LET UNITY COMPILE
                Debug.Log("[StormReachDevSuite] FORCE COMPILE (UNLOCKED)...");
                AssetDatabase.Refresh();
                return;
            }

            // LOCKED: SET PENDING RELOCK, UNLOCK SO UNITY CAN COMPILE, THEN TRIGGER A REFRESH.
            SetPendingRelock(true);
            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.Refresh();

            EditorApplication.update += OnEditorUpdate_WaitPendingRelock;
        }

        // -------------------------
        // UPDATE CALLBACKS (WATCH FOR END OF COMPILATION)
        // -------------------------

        private static void OnEditorUpdate_WaitPendingRelock()
        {
            if (!IsPendingRelock())
            {
                // NOTHING TO DO
                EditorApplication.update -= OnEditorUpdate_WaitPendingRelock;
                return;
            }

            // WAIT UNTIL UNITY IS NOT COMPILING ANYMORE
            if (!EditorApplication.isCompiling)
            {
                // RE-LOCK, CLEAR PENDING FLAG
                EditorApplication.LockReloadAssemblies();
                SetDesiredLocked(true);
                SetPendingRelock(false);
                Debug.Log("[StormReachDevSuite] 🔒 COMPILE FINISHED — LOCK REAPPLIED.");
                EditorApplication.update -= OnEditorUpdate_WaitPendingRelock;
            }
        }

        // IF ON LOAD DESIRED LOCKED BUT A COMPILE WAS RUNNING, APPLY LOCK WHEN COMPILE FINISHES
        private static void OnEditorUpdate_ApplyDesiredLock()
        {
            if (!EditorApplication.isCompiling)
            {
                EditorApplication.LockReloadAssemblies();
                Debug.Log("[StormReachDevSuite] 🔒 APPLIED DESIRED LOCK (COMPILE FINISHED ON LOAD).");
                EditorApplication.update -= OnEditorUpdate_ApplyDesiredLock;
            }
        }

        // -------------------------
        // EDITORPREFS HELPERS
        // -------------------------

        private static bool IsDesiredLocked() { return EditorPrefs.GetInt(PREF_LOCKED, 0) == 1; }
        private static void SetDesiredLocked(bool v) { EditorPrefs.SetInt(PREF_LOCKED, v ? 1 : 0); }
        private static bool IsPendingRelock() { return EditorPrefs.GetInt(PREF_PENDING_RELOCK, 0) == 1; }
        private static void SetPendingRelock(bool v) { EditorPrefs.SetInt(PREF_PENDING_RELOCK, v ? 1 : 0); }
    }
}