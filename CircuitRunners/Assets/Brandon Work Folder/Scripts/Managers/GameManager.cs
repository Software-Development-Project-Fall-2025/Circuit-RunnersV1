using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject playerPrefab;
    private Dictionary<string, GameObject> spawnedPlayers = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdatePlayerPosition(PlayerPositionData posData)
    {
        if (!spawnedPlayers.ContainsKey(posData.id))
        {
            var newPlayer = Instantiate(playerPrefab, new Vector3(posData.x, posData.y, posData.z), Quaternion.identity);
            spawnedPlayers[posData.id] = newPlayer;
        }
        else
        {
            spawnedPlayers[posData.id].transform.position = new Vector3(posData.x, posData.y, posData.z);
        }
    }

    public void RemovePlayer(string id)
    {
        if (spawnedPlayers.ContainsKey(id))
        {
            Destroy(spawnedPlayers[id]);
            spawnedPlayers.Remove(id);
        }
    }
}
