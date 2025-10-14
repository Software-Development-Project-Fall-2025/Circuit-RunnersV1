using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class UnityClientManager : MonoBehaviour
{
    public static UnityClientManager Instance { get; private set; }

    // WebSocket connection status
    public bool IsConnected { get; private set; }
    public string PlayerId { get; private set; }
    public string RoomId { get; private set; }
    public string PlayerName { get; private set; }

    // Player data
    public Dictionary<string, PlayerData> Players { get; private set; }

    [Serializable]
    public class PlayerData
    {
        public string id;
        public string name;
        public bool isHost;
        public Vector3 position;
        public Quaternion rotation;
        // Add other player properties as needed
    }

    // Game state
    [Serializable]
    public class GameState
    {
        public string roomId;
        public string state; // "waiting", "countdown", "playing", "finished"
        public float countdownTime;
        public Dictionary<string, PlayerData> players;
        // Add other game state properties as needed
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        Players = new Dictionary<string, PlayerData>();

        // Initialize WebSocket connection
#if UNITY_WEBGL && !UNITY_EDITOR
        // In WebGL, the connection is handled by JavaScript
#else
        // For testing in the Unity Editor
        // You might want to implement a native WebSocket client here for non-WebGL builds
#endif
    }

    // Called from JavaScript when WebSocket is ready
    public void OnWebSocketReady()
    {
        Debug.Log("WebSocket connection is ready");
        IsConnected = true;

        // Get player name from URL or use a default
        PlayerName = GetPlayerNameFromURL() ?? "Player" + UnityEngine.Random.Range(1000, 9999);

        // Notify the server that we're ready to join a game
        SendNetworkMessage("joinGame", new
        {
            playerName = PlayerName,
            roomId = GetRoomIdFromURL()
        });
    }

    // Called when a player joins the game
    public void OnPlayerJoined(string playerDataJson)
    {
        try
        {
            var playerData = JsonUtility.FromJson<PlayerData>(playerDataJson);
            Players[playerData.id] = playerData;

            Debug.Log($"Player joined: {playerData.name} (ID: {playerData.id})");

            // Update UI or game state as needed
            if (playerData.id == PlayerId)
            {
                // This is the local player
                Debug.Log("You have joined the game!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing player join: {e.Message}");
        }
    }

    // Called when a player leaves the game
    public void OnPlayerLeft(string playerDataJson)
    {
        try
        {
            var playerData = JsonUtility.FromJson<PlayerData>(playerDataJson);
            if (Players.ContainsKey(playerData.id))
            {
                Debug.Log($"Player left: {playerData.name} (ID: {playerData.id})");
                Players.Remove(playerData.id);

                // Update game state
                // (e.g., remove player's character from the game)
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing player leave: {e.Message}");
        }
    }

    // Called when the game state is updated
    public void OnGameStateUpdate(string gameStateJson)
    {
        try
        {
            // Parse the game state
            var gameState = JsonUtility.FromJson<GameState>(gameStateJson);

            // Update local game state
            // (e.g., update player positions, game timer, etc.)

            Debug.Log($"Game state updated: {gameState.state}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing game state update: {e.Message}");
        }
    }

    // Called when there's a network error
    public void OnNetworkError(string errorJson)
    {
        try
        {
            var error = JsonUtility.FromJson<NetworkError>(errorJson);
            Debug.LogError($"Network error: {error.message}");

            // Handle the error (e.g., show an error message to the player)
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing network error: {e.Message}");
        }
    }

    // Helper method to send messages to the server
    public void SendNetworkMessage(string messageType, object data)
    {
        string jsonData = JsonUtility.ToJson(data);

#if UNITY_WEBGL && !UNITY_EDITOR
        // Call JavaScript function to send the message
        SendNetworkMessageWebGL(messageType, jsonData);
#else
        // For non-WebGL builds, implement native WebSocket sending here
        Debug.Log($"Sending {messageType}: {jsonData}");
#endif
    }

    // Helper methods to get data from URL
    private string GetPlayerNameFromURL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GetURLParameter("name");
#else
        // For testing in the Unity Editor
        return "Player" + UnityEngine.Random.Range(1000, 9999);
#endif
    }

    private string GetRoomIdFromURL()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return GetURLParameter("room");
#else
        // For testing in the Unity Editor
        return "test-room";
#endif
    }

    // JavaScript interop for WebGL
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string GetURLParameter(string name);
    
    [DllImport("__Internal")]
    private static extern void SendNetworkMessageWebGL(string messageType, string data);
    #endif
    void Start()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        ConnectToServer("https://your-server-domain-or-ip:3000");
    #else
        Debug.Log("Local editor mode — no WebGL socket connection.");
    #endif
    }
}

// Helper class for network errors
[Serializable]
public class NetworkError
{
    public string message;
    public string code;
}
