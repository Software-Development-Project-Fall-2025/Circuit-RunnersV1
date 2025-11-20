using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceController : MonoBehaviour
{
    [Header("Core")]
    public CheckpointManager checkpointManager;      // CPContainer
    public int targetLaps = 3;

    [Header("Use your existing RaceHud Canvas")]
    public Canvas raceHudCanvas;                     // assign your RaceHud (Canvas)
    public TMP_Text winText;                         // big "WIN!!"
    public TMP_Text winSubText;                      // "Winner: Name"
    public Button restartButton;                     // Restart button

    [Header("Lock controls when PLAYER wins")]
    public string playerTag = "Car";
    public List<Behaviour> componentsToDisableOnWin = new();   // drag PlayerInput/CarController/etc

    bool raceOver = false;
    readonly List<Behaviour> _disabled = new();

    void Start()
    {
        if (!checkpointManager) checkpointManager = FindObjectOfType<CheckpointManager>();
        if (!raceHudCanvas) raceHudCanvas = FindObjectOfType<Canvas>();  // best effort

        // EnsureOverlay();
        SetOverlayVisible(false);
    }

    void Update()
    {
        if (!checkpointManager) return;

        if (!raceOver && checkpointManager.GetRacers().Count == 0)
        {
            foreach (var car in FindObjectsOfType<CarProgress>())
                if (car.checkpointManager == checkpointManager)
                    checkpointManager.RegisterRacer(car);
        }
        if (raceOver) return;

        foreach (var r in checkpointManager.GetRacers())
        {
            if (r.lapsCompleted >= targetLaps)
            {
                raceOver = true;
                ShowWin(r);
                break;
            }
        }
    }

    void ShowWin(CarProgress winner)
    {
        SetOverlayVisible(true);
        if (winText)    winText.text = "WIN!!";
        if (winSubText) winSubText.text = $"Winner: {winner.name}";

        if (winner.CompareTag(playerTag))
        {
            if (componentsToDisableOnWin.Count == 0)
            {
                // try to find obvious control behaviours on winner the first time
                foreach (var b in winner.GetComponentsInChildren<Behaviour>(true))
                {
                    if (!b.enabled) continue;
                    string n = b.GetType().Name.ToLowerInvariant();
                    if (n.Contains("playerinput") || n.Contains("controller") || n.Contains("vehicle") || n.Contains("input"))
                        componentsToDisableOnWin.Add(b);
                }
            }
            foreach (var b in componentsToDisableOnWin)
            {
                if (!b) continue;
                b.enabled = false;
                _disabled.Add(b);
            }
        }
    }

    public void RestartRace()
    {
        // re-enable controls
        foreach (var b in _disabled)
            if (b) b.enabled = true;
        _disabled.Clear();

        // reset racers
        foreach (var r in checkpointManager.GetRacers())
            r.ResetProgress();

        // re-enable rings (CP0 stays gone if its trigger has restoreOnEnable=false)
        foreach (var trig in checkpointManager.AllTriggers())
        {
            if (trig.restoreOnEnable)
            {
                trig.gameObject.SetActive(false);
                trig.gameObject.SetActive(true);
            }
        }

        SetOverlayVisible(false);
        raceOver = false;
    }

    // ------- UI helpers -------
    // void EnsureOverlay()
    // {
    //     if (raceHudCanvas && winText && restartButton) { HookRestartButton(); return; }

    //     // Build a small overlay under the RaceHud canvas
    //     Canvas targetCanvas = raceHudCanvas ? raceHudCanvas : FindObjectOfType<Canvas>();
    //     if (!targetCanvas)
    //     {
    //         // fallback: make a canvas
    //         var go = new GameObject("RaceHud", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
    //         raceHudCanvas = targetCanvas = go.GetComponent<Canvas>();
    //         targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
    //         go.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
    //     }

    //     var panel = new GameObject("WinOverlay", typeof(Image));
    //     panel.transform.SetParent(targetCanvas.transform, false);
    //     panel.SetActive(false);
    //     var img = panel.GetComponent<Image>();
    //     img.color = new Color(0, 0, 0, 0.55f);
    //     var rt = panel.GetComponent<RectTransform>();
    //     rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 1);
    //     rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

    //     // var t = new GameObject("WinText", typeof(TMP_Text));
    //     // t.transform.SetParent(panel.transform, false);
    //     // winText = t.GetComponent<TMP_Text>();
    //     // winText.alignment = TextAlignmentOptions.Center;
    //     // winText.fontSize = 96; winText.fontStyle = FontStyles.Bold;
    //     // var tr = t.GetComponent<RectTransform>();
    //     // tr.anchorMin = new Vector2(0.2f, 0.56f); tr.anchorMax = new Vector2(0.8f, 0.86f);
    //     // tr.offsetMin = tr.offsetMax = Vector2.zero;

    //     // var st = new GameObject("WinSubText", typeof(TMP_Text));
    //     // st.transform.SetParent(panel.transform, false);
    //     // winSubText = st.GetComponent<TMP_Text>();
    //     // winSubText.alignment = TextAlignmentOptions.Center;
    //     // winSubText.fontSize = 36;
    //     // var srt = st.GetComponent<RectTransform>();
    //     // srt.anchorMin = new Vector2(0.2f, 0.45f); srt.anchorMax = new Vector2(0.8f, 0.53f);
    //     // srt.offsetMin = srt.offsetMax = Vector2.zero;

    //     var b = new GameObject("RestartButton", typeof(Button), typeof(Image));
    //     b.transform.SetParent(panel.transform, false);
    //     restartButton = b.GetComponent<Button>();
    //     b.GetComponent<Image>().color = new Color(1,1,1,0.9f);
    //     var br = b.GetComponent<RectTransform>();
    //     br.anchorMin = new Vector2(0.42f, 0.23f); br.anchorMax = new Vector2(0.58f, 0.35f);
    //     br.offsetMin = br.offsetMax = Vector2.zero;

    //     var label = new GameObject("Label", typeof(TMP_Text));
    //     label.transform.SetParent(b.transform, false);
    //     var lt = label.GetComponent<TMP_Text>();
    //     lt.text = "Restart";
    //     lt.alignment = TextAlignmentOptions.Center;
    //     lt.fontSize = 36;
    //     var lrt = label.GetComponent<RectTransform>();
    //     lrt.anchorMin = new Vector2(0, 0); lrt.anchorMax = new Vector2(1, 1);
    //     lrt.offsetMin = lrt.offsetMax = Vector2.zero;

    //     HookRestartButton();
    // }

    void HookRestartButton()
    {
        if (restartButton == null) return;
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartRace);
    }

    void SetOverlayVisible(bool v)
    {
        if (!winText) return;
        var root = winText.transform.parent; // panel
        // if (root) root.gameObject.SetActive(v);
        root.transform.GetChild(0).gameObject.SetActive(!v);
        winText.gameObject.SetActive(v);
    }
}
