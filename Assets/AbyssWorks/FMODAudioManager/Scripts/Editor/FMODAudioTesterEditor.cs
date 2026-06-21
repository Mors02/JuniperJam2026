#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AbyssWorks.FMODAudioManager
{
    [CustomEditor(typeof(FMODAudioTester))]
    public class FMODAudioTesterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector first (shows serialized fields)
            DrawDefaultInspector();

            // Reference to the target component
            FMODAudioTester tester = (FMODAudioTester)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Control Buttons", EditorStyles.boldLabel);

            // Fade duration control
            //fadeDuration = EditorGUILayout.FloatField("Fade Duration (s)", fadeDuration);

            EditorGUILayout.BeginHorizontal();
            //playMode = (FadeMode)EditorGUILayout.EnumPopup("Play Mode", playMode);
            if (GUILayout.Button("Play", GUILayout.Height(30)))
            {
                if (tester.audioScriptable && FMODAudioManager.Instance)
                {
                    if (tester.playMode == FMODAudioTester.FadeMode.Instant)
                        FMODAudioManager.Instance.PlayAudio(tester.audioScriptable);
                    else
                        FMODAudioManager.Instance.PlayAudioLinear(
                            tester.audioScriptable, tester.fadeDuration, null, tester.allowReverse
                        );
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //stopMode = (FadeMode)EditorGUILayout.EnumPopup("Stop Mode", stopMode);
            if (GUILayout.Button("Stop", GUILayout.Height(30)))
            {
                if (tester.audioScriptable && FMODAudioManager.Instance)
                {
                    if (tester.stopMode == FMODAudioTester.FadeMode.Instant)
                        FMODAudioManager.Instance.StopAudio(tester.audioScriptable);
                    else
                        FMODAudioManager.Instance.StopAudioLinear(
                            tester.audioScriptable,
                            tester.fadeDuration,
                            null,
                            tester.allowReverse
                        );
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("SFX", EditorStyles.boldLabel);

            if (GUILayout.Button("PlaySFX", GUILayout.Height(30)))
            {
                tester.PlaySFX();
            }
        }
    }
}

#endif