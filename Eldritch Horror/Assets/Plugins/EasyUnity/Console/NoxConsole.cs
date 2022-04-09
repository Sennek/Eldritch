using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

public sealed class NoxConsole : SingletonMB<NoxConsole>
{
    [Header("Settings_NoxConsole")]
    [Tooltip("Key used to open/close the console")]
    [SerializeField] private KeyCode activationKey = KeyCode.Backslash;
    [Tooltip("Time in seconds it takes for the full open/close animation")]
    [SerializeField] private float animationTime = 1;
    [Tooltip("How many lines can the window fit?")]
    [SerializeField] private int linesMax = 13;
    [SerializeField] private Color selectedCandidateColor = Color.white;
    [SerializeField] private Color unselectedCandidateColor = Color.gray;

    [Header("References_NoxConsole")]
    [SerializeField] private RectTransform BGRect;
    [SerializeField] private RectTransform candidatesTransform;
    [SerializeField] private GameObject candidateTemplate;
    [SerializeField] private Text textField;
    [SerializeField] private InputField inputField;

    public enum State { Open, Closed, Opening, Closing, Locked }
    private State state = State.Closed;

    private int selectedCandidate = -1;
    private int caretPos;

    private List<Text> candidatesTexts;
    private List<string> lines;
    private Dictionary<string, MethodInfo> commands;
    private Dictionary<string, string> descriptions;
    private List<string> candidates;

    public static bool ShowUnityLog { get; set; } = false;

    private void Start()
    {
        RegisterEvents();
        LoadLists();
    }

    private void RegisterEvents()
    {
        Application.logMessageReceived += LogCallback;
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void LoadLists()
    {
        candidatesTexts = new List<Text>();
        lines = new List<string>(linesMax);

        List<MethodInfo> methods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.CustomAttributes.OfType<NoxCommand>().Any())
            .ToList();

        commands = methods.ToDictionary(m => m.GetCustomAttribute<NoxCommand>(false).Alias);
        descriptions = new Dictionary<string, string>(methods.Count);
        methods.ForEach(m =>
        {
            NoxCommand nc = m.GetCustomAttribute<NoxCommand>(false);
            descriptions.Add(nc.Alias, nc.Description);
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(activationKey)) Activate();

        if (Input.GetKeyDown(KeyCode.Return)) OnSubmit();

        if (candidates != null)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SelectCandidate(Mathf.Clamp(selectedCandidate + 1, -1, candidates.Count - 1));
                inputField.caretPosition = caretPos;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SelectCandidate(Mathf.Clamp(selectedCandidate - 1, -1, candidates.Count - 1));
                inputField.caretPosition = caretPos;
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (selectedCandidate >= 0)
                {
                    inputField.text = candidates[selectedCandidate];
                    inputField.caretPosition = inputField.text.Length;
                }
            }
        }

        switch (state)
        {
            case State.Opening:
            case State.Closing:
                Animate();
                break;
        }
    }

    private void Activate()
    {
        switch (state)
        {
            case State.Open:
            case State.Opening:
                ChangeState(State.Closing);
                break;
            case State.Closed:
            case State.Closing:
                ChangeState(State.Opening);
                break;
        }
    }

    private void Animate()
    {
        Vector2 v = BGRect.localScale;
        if (state == State.Opening)
        {
            v.y += Time.unscaledDeltaTime / animationTime;
            if (v.y > 1)
            {
                v.y = 1;
                ChangeState(State.Open);
            }
        }
        else if (state == State.Closing)
        {
            v.y -= Time.unscaledDeltaTime / animationTime;
            if (v.y < 0)
            {
                v.y = 0;
                ChangeState(State.Closed);
            }
        }
        BGRect.localScale = v;
    }

    private void ChangeState(State newState) => OnChangeState(state, state = newState);

    private void OnChangeState(State oldState, State newState)
    {
        if (newState == oldState) return;

        if (newState == State.Open)
            inputField.ActivateInputField();
        else
        {
            inputField.DeactivateInputField();
            inputField.text = "";
        }
    }

    private void OnValueChanged(string text)
    {
        candidates = text.Length > 0 ? commands.Keys.Where(k => k.StartsWith(text)).ToList() : null;

        for (int i = 0; i < candidatesTransform.childCount; i++)
            SetCandidateObject(i);

        caretPos = inputField.caretPosition;
    }

    private void SetCandidateObject(int index)
    {
        while (candidatesTransform.childCount < index)
            InstantiateCandidateObject();

        if (index < candidates?.Count)
        {
            candidatesTexts[index].gameObject.SetActive(true);
            candidatesTexts[index].text = $"{candidates[index]} ({descriptions[candidates[index]]})";
        }
        else candidatesTexts[index].gameObject.SetActive(false);
    }

    private void InstantiateCandidateObject()
    {
        GameObject obj = Instantiate(candidateTemplate, candidatesTransform);
        obj.transform.localPosition = new Vector3(0, candidateTemplate.GetRectTransform().rect.height * candidatesTransform.childCount, 0);
        candidatesTexts.Add(obj.GetComponentInChildren<Text>(true));
    }

    private void SelectCandidate(int value)
    {
        if (selectedCandidate >= 0)
            candidatesTexts[selectedCandidate].color = unselectedCandidateColor;
        selectedCandidate = value;
        candidatesTexts[selectedCandidate].color = selectedCandidateColor;
    }

    private void OnSubmit()
    {
        try
        {
            string[] args = inputField.text.Split(' ');
            if (commands.TryGetValue(args[0], out MethodInfo method))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 0 && args.Length == 1)
                    method.Invoke(null, null);
                else if (parameters.Length == args.Length - 1)
                {
                    object[] objs = new object[parameters.Length];
                    for (int i = 0; i < objs.Length; i++)
                        objs[i] = TypeDescriptor.GetConverter(parameters[i].ParameterType).ConvertFromString(args[i + 1]);
                    method.Invoke(null, objs);
                }
                else
                {
                    StringBuilder sb = new StringBuilder("Error: expected parameters: (");
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(parameters[i].ParameterType.Name);
                    }
                    Write(sb.Append(")").ToString());
                }
            }
        }
        catch (Exception e) { Log.Exception(e); Write(e.Message); }

        inputField.text = "";
        inputField.ActivateInputField();
    }

    private void LogCallback(string log, string stackTrace, LogType type)
    {
        if (ShowUnityLog)
            Write(log);
    }

    private void UpdateText()
    {
        StringBuilder sb = new StringBuilder(lines.Sum(s => s.Length));
        for (int i = 0; i < lines.Count; i++)
            sb.AppendLine(lines[i]);
        textField.text = sb.ToString();
    }

    public static void Write(string text)
    {
        while (Instance.lines.Count >= Instance.lines.Capacity) Instance.lines.RemoveAt(0);
        Instance.lines.Add(text);
        Instance.UpdateText();
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class NoxCommand : Attribute
{
    public string Alias { get; set; }
    public string Description { get; set; }
}
