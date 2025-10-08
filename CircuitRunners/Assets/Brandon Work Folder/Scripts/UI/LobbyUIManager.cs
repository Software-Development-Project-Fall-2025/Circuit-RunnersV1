using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance { get; private set; }

    [Header("Lobby UI Elements")]
    [SerializeField] private InputField nameInput;
    [SerializeField] private InputField roomCodeInput;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Text roomCodeDisplay;
    [SerializeField] private Text playersListText;

    private void Awake()
    {
        Instance = this;
        createButton.onClick.AddListener(OnCreateClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnCreateClicked()
    {
        string name = nameInput.text.Trim();
        if (string.IsNullOrEmpty(name))
            name = "Player" + Random.Range(100, 999);

        // Blank roomId triggers server-generated code
        NetworkManager.Instance.JoinRoom(name, "");
    }

    private void OnJoinClicked()
    {
        string name = nameInput.text.Trim();
        string roomId = roomCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(name))
            name = "Player" + Random.Range(100, 999);

        NetworkManager.Instance.JoinRoom(name, roomId);
    }

    public void UpdateRoomCode(string code)
    {
        if (roomCodeDisplay != null)
            roomCodeDisplay.text = $"Room Code: {code}";
    }

    public void UpdatePlayerList(List<PlayerData> players)
    {
        playersListText.text = "Players in Room:\n";
        foreach (var p in players)
        {
            playersListText.text += $"{p.name}{(p.isHost ? " 👑" : "")}\n";
        }
    }
}
