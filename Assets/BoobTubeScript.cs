using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class BoobTubeScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;
    public TextMesh[] buttonTexts;

    private string[] words = new string[] { "Shittah", "Dik-Dik", "Aktashite", "Tetheradick", "Sack-Butt", "Nobber", "Knobstick", "Jerkinhead", "Haboob", "Fanny-Blower", "Assapanick", "Fuksheet", "Clatterfart", "Humpenscrump", "Cock-Bell", "Slagger", "Pakapoo", "Wankapin", "Lobcocked", "Poonga", "Sexagesm", "Tit-Bore", "Pershitte", "Invagination", "Bumfiddler", "Nestle-Cock", "Gullgroper", "Boob Tube", "Boobyalla", "Dreamhole" };
    private string[] chosenWords = new string[6];
    private int[] correctOrder = new int[6] { 1, 2, 3, 4, 5, 6 };
    private int curIndex;
    private bool activated;
    private bool animating;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void Start () {
        for (int i = 0; i < 6; i++)
        {
            if (!activated)
                buttonTexts[i].text = "";
            string word = words[UnityEngine.Random.Range(0, words.Length)];
            while (chosenWords.Contains(word))
                word = words[UnityEngine.Random.Range(0, words.Length)];
            chosenWords[i] = word;
            if (activated)
                buttonTexts[i].text = chosenWords[i];
        }
        List<int> usedVals = new List<int>();
        List<int> vals = new List<int>() { Array.IndexOf(words, chosenWords[0]), Array.IndexOf(words, chosenWords[1]), Array.IndexOf(words, chosenWords[2]), Array.IndexOf(words, chosenWords[3]), Array.IndexOf(words, chosenWords[4]), Array.IndexOf(words, chosenWords[5]) };
        for (int j = 0; j < 6; j++)
        {
            int small = 99;
            for (int i = 0; i < vals.Count; i++)
            {
                if (vals[i] < small)
                    small = vals[i];
            }
            usedVals.Add(small);
            vals.Remove(small);
            correctOrder[j] = Array.IndexOf(chosenWords, words[usedVals[j]]);
        }
        Debug.LogFormat("[Boob Tube #{0}] The displayed words are:", moduleId);
        Debug.LogFormat("[Boob Tube #{0}] {1} {2}", moduleId, chosenWords[0], chosenWords[3]);
        Debug.LogFormat("[Boob Tube #{0}] {1} {2}", moduleId, chosenWords[1], chosenWords[4]);
        Debug.LogFormat("[Boob Tube #{0}] {1} {2}", moduleId, chosenWords[2], chosenWords[5]);
        Debug.LogFormat("[Boob Tube #{0}] The order to press the displayed words in is: {1}, {2}, {3}, {4}, {5}, and {6}", moduleId, chosenWords[correctOrder[0]], chosenWords[correctOrder[1]], chosenWords[correctOrder[2]], chosenWords[correctOrder[3]], chosenWords[correctOrder[4]], chosenWords[correctOrder[5]]);
    }

    void OnActivate()
    {
        for (int i = 0; i < 6; i++)
            buttonTexts[i].text = chosenWords[i];
        activated = true;
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true && activated && !animating)
        {
            pressed.AddInteractionPunch(0.5f);
            audio.PlaySoundAtTransform("tick", pressed.transform);
            if (correctOrder[curIndex] == Array.IndexOf(buttons, pressed))
            {
                curIndex++;
                buttonTexts[Array.IndexOf(buttons, pressed)].color = new Color32(0, 255, 0, 255);
                Debug.LogFormat("[Boob Tube #{0}] Pressing {1} was correct", moduleId, buttonTexts[Array.IndexOf(buttons, pressed)].text);
                if (curIndex == 6)
                {
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                    Debug.LogFormat("[Boob Tube #{0}] Module disarmed", moduleId);
                }
            }
            else
            {
                curIndex = 0;
                StartCoroutine(StrikeAnim(Array.IndexOf(buttons, pressed)));
                GetComponent<KMBombModule>().HandleStrike();
                Debug.LogFormat("[Boob Tube #{0}] Pressing {1} was incorrect. Strike! Resetting module...", moduleId, buttonTexts[Array.IndexOf(buttons, pressed)].text);
            }
        }
    }

    private IEnumerator StrikeAnim(int pos)
    {
        animating = true;
        for (int i = 0; i < 6; i++)
        {
            buttonTexts[pos].color = new Color32(255, 0, 0, 255);
            yield return new WaitForSeconds(0.1f);
            buttonTexts[pos].color = new Color32(255, 255, 255, 255);
            yield return new WaitForSeconds(0.1f);
        }
        for (int i = 0; i < 6; i++)
            buttonTexts[i].color = new Color32(255, 255, 255, 255);
        Start();
        animating = false;
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press <pos1> (pos2)... [Press the word in the specified position (optionally include multiple positions)] | Valid positions are 1-6 in reading order";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        int[] positions = { 0, 3, 1, 4, 2, 5 };
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the position(s) of the word(s) you wish to press!";
            }
            else
            {
                for (int i = 1; i < parameters.Length; i++)
                {
                    int temp = 0;
                    if (!int.TryParse(parameters[i], out temp))
                    {
                        yield return "sendtochaterror!f The specified position '" + parameters[i] + "' is invalid!";
                        yield break;
                    }
                    if (temp < 1 || temp > 6)
                    {
                        yield return "sendtochaterror!f The specified position '" + parameters[i] + "' is out of range 1-6!";
                        yield break;
                    }
                }
                for (int i = 1; i < parameters.Length; i++)
                {
                    buttons[positions[int.Parse(parameters[i]) - 1]].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!activated || animating) { yield return true; }
        int start = curIndex;
        for (int i = start; i < 6; i++)
        {
            buttons[correctOrder[i]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
