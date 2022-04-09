using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SetParameterOnEvent))]
public class SetParameterOnEvent_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        SetParameterOnEvent me = (SetParameterOnEvent)target;

        int size = EditorGUILayout.IntField("Size", me.Parameters.Count);
        while (size > me.Parameters.Count)
            me.Parameters.Add(new SetParameterOnEvent.Parameter());
        while (size < me.Parameters.Count)
            me.Parameters.RemoveAt(me.Parameters.Count - 1);

        for (int i = 0; i < me.Parameters.Count; i++)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            me.Parameters[i].Event = (SetParameterOnEvent.Event)EditorGUILayout.EnumPopup("Event", me.Parameters[i].Event);
            me.Parameters[i].Type = (SetParameterOnEvent.Type)EditorGUILayout.EnumPopup("Type", me.Parameters[i].Type);
            me.Parameters[i].Name = EditorGUILayout.TextField("Name", me.Parameters[i].Name);
            switch (me.Parameters[i].Type)
            {
                case SetParameterOnEvent.Type.Float:
                    me.Parameters[i].Value = EditorGUILayout.FloatField("Value", (float)me.Parameters[i].Value); break;
                case SetParameterOnEvent.Type.Int:
                    me.Parameters[i].Value = EditorGUILayout.IntField("Value", (int)me.Parameters[i].Value); break;
                case SetParameterOnEvent.Type.Bool:
                case SetParameterOnEvent.Type.Trigger:
                    me.Parameters[i].Value = EditorGUILayout.Toggle("Value", (bool)me.Parameters[i].Value); break;
            }
            if (me.Parameters[i].Event == SetParameterOnEvent.Event.Enter)
                me.Parameters[i].Delay = EditorGUILayout.FloatField("Delay", me.Parameters[i].Delay);
        }
    }
}
#endif

public class SetParameterOnEvent : StateMachineBehaviour
{
    [System.Serializable]
    public class Parameter
    {
        public Event Event;
        public Type Type;
        public string Name;
        public object Value;
        public float Delay;
    }
    public enum Event { Enter, Update, Exit, Move, IK }
    public enum Type { Float, Int, Bool, Trigger }
    public List<Parameter> Parameters = new List<Parameter>();

    protected virtual void HandleParameter(Animator a, Parameter p)
    {
        switch (p.Type)
        {
            case Type.Float:
                a.SetFloat(p.Name, (float)p.Value);
                break;
            case Type.Int:
                a.SetInteger(p.Name, (int)p.Value);
                break;
            case Type.Bool:
                a.SetBool(p.Name, (bool)p.Value);
                break;
            case Type.Trigger:
                if ((bool)p.Value) a.SetTrigger(p.Name);
                else a.ResetTrigger(p.Name);
                break;
        }
    }

    /// <summary>
    /// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < Parameters.Count; i++)
            if (Parameters[i].Event == Event.Enter && Parameters[i].Delay <= 0)
            {
                HandleParameter(animator, Parameters[i]);
                Parameters.RemoveAt(i--);
            }
    }

    /// <summary>
    /// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < Parameters.Count; i++)
        {
            if (Parameters[i].Event == Event.Update)
                HandleParameter(animator, Parameters[i]);
            else if (Parameters[i].Event == Event.Enter && (Parameters[i].Delay -= Time.deltaTime) <= 0)
            {
                HandleParameter(animator, Parameters[i]);
                Parameters.RemoveAt(i--);
            }
        }
    }

    /// <summary>
    /// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < Parameters.Count; i++)
            if (Parameters[i].Event == Event.Exit)
                HandleParameter(animator, Parameters[i]);
    }

    /// <summary>
    /// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < Parameters.Count; i++)
            if (Parameters[i].Event == Event.Move)
                HandleParameter(animator, Parameters[i]);
    }

    /// <summary>
    /// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="stateInfo"></param>
    /// <param name="layerIndex"></param>
    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        for (int i = 0; i < Parameters.Count; i++)
            if (Parameters[i].Event == Event.IK)
                HandleParameter(animator, Parameters[i]);
    }
}
