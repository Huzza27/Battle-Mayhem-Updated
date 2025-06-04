using System.Collections.Generic;
using UnityEngine;

public class MatchStatsManager : MonoBehaviour
{
    public static MatchStatsManager Instance;

    private Dictionary<string, PlayerStats> playerStats = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(string id, string name)
    {
        if (!playerStats.ContainsKey(id))
        {
            playerStats[id] = new PlayerStats
            {
                playerId = id,
                playerName = name
            };
        }
    }

    public void RecordKill(string killerId)
    {
        if (playerStats.ContainsKey(killerId))
            playerStats[killerId].kills++;
    }

    public void RecordDeath(string playerId)
    {
        if (playerStats.ContainsKey(playerId))
            playerStats[playerId].deaths++;
    }

    public void RecordShotFired(string playerId)
    {
        if (playerStats.ContainsKey(playerId))
            playerStats[playerId].shotsFired++;
    }

    public void RecordShotHit(string playerId)
    {
        if (playerStats.ContainsKey(playerId))
            playerStats[playerId].shotsHit++;
    }

    public void RecordBombKill(string playerId)
    {
        if (playerStats.ContainsKey(playerId))
            playerStats[playerId].bombKills++;
    }

    public List<PlayerStats> GetAllStats()
    {
        return new List<PlayerStats>(playerStats.Values);
    }

    public void ResetAllStats()
    {
        foreach (var stats in playerStats.Values)
            stats.Reset();
    }
}
