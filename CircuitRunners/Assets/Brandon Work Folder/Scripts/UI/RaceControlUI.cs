// Hypothetical (IP) logic for starting the race 
using UnityEngine;
using UnityEngine.UI;

public class RaceControlUI : MonoBehaviour
{
    [SerializeField] private Button startRaceButton;
    [SerializeField] private Text countdownText;

    private void Start()
    {
        startRaceButton.gameObject.SetActive(false);
        startRaceButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        NetworkManager.Instance.SendStartRace();
    }

    public void ShowHostButton(bool isHost)
    {
        startRaceButton.gameObject.SetActive(isHost);
    }

    public void UpdateCountdown(int time)
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = (time > 0) ? time.ToString() : "GO!";
        if (time < 0) countdownText.gameObject.SetActive(false);
    }
}
