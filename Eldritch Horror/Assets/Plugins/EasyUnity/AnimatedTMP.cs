using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;
using NoxLibrary;
using UnityEngine.UI;
using System.Linq;
using MoreLinq;
using System;

public class AnimatedTMP : MonoBehaviour
{
    public event Action OnLetterPlaced = null;
    public bool IsRunning => running;

    [Header("Settings_AnimatedTMP")]
    [SerializeField] [Min(float.Epsilon)] protected float timeBetweenLetters = 0.02f;
    [SerializeField] protected AnimationCurve redCurve;
    [SerializeField] protected AnimationCurve greenCurve;
    [SerializeField] protected AnimationCurve blueCurve;
    [SerializeField] protected AnimationCurve alphaCurve;
    [SerializeField] protected AnimationCurve sizeCurve;

    [Header("References_AnimatedTMP")]
    [SerializeField] protected TMP_Text textMeshPro;
    [SerializeField] protected LayoutElement layoutElement;

    [Header("DEBUG_AnimatedTMP")]
    [SerializeField] protected float elapsedTime;
    [SerializeField] protected bool running;
    [SerializeField] protected bool useColorCurve;
    [SerializeField] protected bool useAlphaCurve;
    [SerializeField] protected bool useSizeCurve;

    protected float highestLetterSize = 1;
    protected StringBuilder sb = new StringBuilder();
    char[] letters;
    public int VisibleLetters { get; protected set; }


    private void Awake()
    {
        if (!textMeshPro)
            textMeshPro = GetComponentInChildren<TMP_Text>();
        if (!layoutElement)
            layoutElement = GetComponentInChildren<LayoutElement>();
    }

    public void Display(string text)
    {
        letters = text.ToCharArray();
        useColorCurve = redCurve.keys.Length > 0 || greenCurve.keys.Length > 0 || blueCurve.keys.Length > 0;
        useAlphaCurve = alphaCurve.keys.Length > 0;
        useSizeCurve = sizeCurve.keys.Length > 0;

        if (useSizeCurve)
            highestLetterSize = sizeCurve.keys.Max(key => key.value);

        UpdateText(float.MaxValue, false, false, false);
        layoutElement.preferredWidth = textMeshPro.preferredWidth;
        elapsedTime = 0;
        this.InvokeDelayed(new WaitForEndOfFrame(), DelayedPreparations);
    }

    public float GetTotalTime(string txt)
    {
        float f = timeBetweenLetters * txt.Length;
        return f;
    }
    public float GetRemainingTime()
    {
        return timeBetweenLetters* (letters.Length - VisibleLetters);
    }

    protected void DelayedPreparations()
    {
        running = true;
        layoutElement.preferredHeight = textMeshPro.preferredHeight;
    }

    protected void Update()
    {
        if (!running)
        {
            return;
        }
        UpdateText(elapsedTime += Time.deltaTime, useColorCurve, useAlphaCurve, useSizeCurve);
        running = elapsedTime < letters.Length * timeBetweenLetters * 2;

        bool isPlacingLetters = elapsedTime < letters.Length * timeBetweenLetters;
        if(!isPlacingLetters)
        {
            return;
        }

        while (alphaCurve.Evaluate(elapsedTime - timeBetweenLetters * VisibleLetters) > 0)
        {
            VisibleLetters++;
            OnLetterPlaced?.Invoke();
        }
        if(VisibleLetters>=letters.Length)
        {
            VisibleLetters = 0;
        }
    }

    protected void UpdateText(float elapsedTime, bool useColorCurve, bool useAlphaCurve, bool useSizeCurve)
    {
        sb.Clear();
        sb.Append("<alpha=#00><size=").Append((highestLetterSize * 100).RoundToInt().ToString("##0")).Append("%>");
        sb.Append("<line-height=").Append((highestLetterSize * 100).RoundToInt().ToString("##0")).Append("%>");
        foreach (char c in letters)
        {
            if (Environment.NewLine.Contains(c))
                sb.Append("<alpha=#00><size=").Append((highestLetterSize * 100).RoundToInt().ToString("##0")).Append("%>|<alpha=#FF><size=100%>")
                    .Append(c)
                    .Append("<line-height=").Append((highestLetterSize * 100).RoundToInt().ToString("##0")).Append("%>");
            else
                AppendLetter(c, elapsedTime, useColorCurve, useAlphaCurve, useSizeCurve);
            elapsedTime -= timeBetweenLetters;
        }
        sb.Append("<alpha=#00><size=").Append((highestLetterSize * 100).RoundToInt().ToString("##0")).Append("%>|");
        textMeshPro.text = sb.ToString();
    }

    protected void AppendLetter(char c, float elapsedTime, bool useColorCurve, bool useAlphaCurve, bool useSizeCurve)
    {
        if (useColorCurve)
            sb
            .Append("<color=#")
            .Append(redCurve.Evaluate(elapsedTime).RoundToInt().ToString("X2"))
            .Append(greenCurve.Evaluate(elapsedTime).RoundToInt().ToString("X2"))
            .Append(blueCurve.Evaluate(elapsedTime).RoundToInt().ToString("X2"))
            .Append(">");

        if (useAlphaCurve)
            sb.Append("<alpha=#")
            .Append(alphaCurve.Evaluate(elapsedTime).RoundToInt().ToString("X2"))
            .Append(">");

        if (useSizeCurve)
            sb.Append("<size=")
            .Append((sizeCurve.Evaluate(elapsedTime) * 100).RoundToInt().ToString("##0"))
            .Append("%>");

        sb.Append(c);
    }

}