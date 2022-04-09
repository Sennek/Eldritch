using UnityEngine;
using NoxLibrary;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(EasyCharacterVOInfo))]
public class EasyCharacterVOInfoEditor : Editor
{
    SerializedProperty pitch;
    SerializedProperty amplitude;

    private void OnEnable()
    {
        pitch = serializedObject.FindProperty("pitch");
        amplitude = serializedObject.FindProperty("amplitude");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.Slider(pitch, 0, 3);
        amplitude.floatValue = amplitude.floatValue.Clamp(0, 1.5f - Mathf.Abs(1.5f - pitch.floatValue));
        EditorGUILayout.Slider(amplitude, 0, 1.5f - Mathf.Abs(1.5f - pitch.floatValue));
        EditorGUILayout.CurveField((target as EasyCharacterVOInfo).Editor_GetCurve(), Color.green, new Rect(0, 0, Mathf.PI, 3));
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

[CreateAssetMenu(fileName = "CharacterVOInfo", menuName = "Scriptable Objects/CharacterInfo")]
public class EasyCharacterVOInfo : ScriptableObject
{
    [SerializeField] protected float pitch;
    [SerializeField] protected float amplitude;

    public float FauxPitch => pitch;
    public float FauxAmplitude => amplitude;

    protected virtual float EvaluateFaux(float elapsedTime) => pitch + (amplitude * Mathf.Sin(elapsedTime));

#if UNITY_EDITOR
    public virtual AnimationCurve Editor_GetCurve()
    {
        AnimationCurve fauxCurve = new AnimationCurve
        {
            preWrapMode = WrapMode.Loop,
            postWrapMode = WrapMode.Loop,
        };

        for (int i = 0; i < 100; i++)
        {
            fauxCurve.AddKey(i, EvaluateFaux(i));
            AnimationUtility.SetKeyLeftTangentMode(fauxCurve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(fauxCurve, i, AnimationUtility.TangentMode.Auto);
        }
        return fauxCurve;
    }
#endif
}
