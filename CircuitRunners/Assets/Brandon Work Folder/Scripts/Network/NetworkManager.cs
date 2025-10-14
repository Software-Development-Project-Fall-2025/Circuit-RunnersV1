using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    // Connection / identity
    public bool IsConnected { get; private set; }
    public string RoomId { get; private set; }
    public string PlayerName { get; private set; }

    public Dictionary<string, PlayerData> Players = new();

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ConnectToServer(string url);
    [DllImport("__Internal")] private static extern void SendNetworkMessageWebGL(string type, string data);
#else
    // Editor-safe stubs (no errors if testing locally)
    private static void ConnectToServer(string url) => Debug.Log($"[Editor] Would connect to {url}");
    private static void SendNetworkMessageWebGL(string type, string data) =>
        Debug.Log($"[Editor] Emit: {type} → {data}");
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ConnectToServer("http://localhost:3000"); // change for production
#else
        Debug.Log("[Editor] Running without live WebSocket connection.");
#endif
    }

    // Called from JS when connection succeeds
    public void OnWebSocketReady()
    {
        IsConnected = true;
        Debug.Log("✅ WebSocket connection ready");
    }

    // ======================
    // Outgoing messages
    // ======================

    public void JoinRoom(string playerName, string roomId)
    {
        PlayerName = string.IsNullOrWhiteSpace(playerName)
            ? "Player" + Random.Range(100, 999)
            : playerName;

        var payload = JsonUtility.ToJson(new { playerName = PlayerName, roomId });
        SendNetworkMessageWebGL("joinGame", payload);
    }

    public void SendPosition(Vector3 pos)
    {
        var payload = JsonUtility.ToJson(new { x = pos.x, y = pos.y, z = pos.z });
        SendNetworkMessageWebGL("positionUpdate", payload);
    }

    public void CheckpointReached(int checkpointIndex)
    {
        var payload = JsonUtility.ToJson(new { checkpointIndex });
        SendNetworkMessageWebGL("checkpointReached", payload);
    }

    public void SendStartRace()
    {
        var payload = JsonUtility.ToJson(new { }); // empty payload
        SendNetworkMessageWebGL("startRace", payload);
    }


    // ======================
    // Incoming events (called from JS)
    // ======================

    public void OnRoomUpdate(string json)
    {
        var players = JsonUtility.FromJson<PlayerListWrapper>(json);
        LobbyUIManager.Instance?.UpdatePlayerList(players.list);
    }

    public void OnRankUpdate(string json)
    {
        var ranks = JsonUtility.FromJson<RankListWrapper>(json);
        LeaderboardUIManager.Instance?.UpdateLeaderboard(ranks.list);
    }

    public void OnPlayerPositions(string json)
    {
        var posData = JsonUtility.FromJson<PlayerPositionData>(json);
        GameManager.Instance?.UpdatePlayerPosition(posData);
    }

    public void OnNetworkError(string json)
    {
        Debug.LogError($"Network error: {json}");
    }

    // Wrapper structs for simple JSON array parsing
    [System.Serializable] private class PlayerListWrapper { public List<PlayerData> list; }
    [System.Serializable] private class RankListWrapper { public List<RankData> list; }
}

// ======================
// Shared data models
// ======================
[System.Serializable]
public class PlayerData
{
    public string id;
    public string name;
    public bool isHost;
}

[System.Serializable]
public class PlayerPositionData
{
    public string id;
    public float x, y, z;
}

[System.Serializable]
public class RankData
{
    public string id;
    public int rank;
}
