using System.Collections.Generic;
using UnityEngine;

public class MatchStatsManager : MonoBehaviour
{
    public static MatchStatsManager Instance;

    public Dictionary<string, PlayerStats> playerStats = new();
    public List<MatchStatTitle> defaultTitles = new List<MatchStatTitle>();
    public Dictionary<string, MatchStatTitle> titles;

    public List<MatchStatTitle> titleAssets; // assign in Inspector

    PlayerStats currentPlayer;

    void Awake()
    {
        Instance = this;
        titles = new Dictionary<string, MatchStatTitle>();

        foreach (var statTitle in titleAssets)
        {
            switch (statTitle.registeredName)
            {
                case "Sharpshooter":
                    statTitle.statEvaluator = p => p.kills;
                    break;
                case "BoomGod":
                    statTitle.statEvaluator = p => p.bombKills;
                    break;
                case "RespawnChamp":
                    statTitle.statEvaluator = p => p.deaths;
                    break;
                case "PixelPerfect":
                    statTitle.statEvaluator = p =>
                        (p.shotsFired >= 5 ? (float)p.shotsHit / p.shotsFired : -1f);
                    break;
            }

            titles[statTitle.title] = statTitle;
        }
    }

    public void RegisterPlayer(int id, string name)
    {
        Debug.Log("Registering Player" + id);
        if (!playerStats.ContainsKey(id.ToString()))
        {
            playerStats[id.ToString()] = new PlayerStats
            {
                playerId = id,
                playerName = name
            };
        }
    }

    public PlayerStats GetTitleEarnedByPlayer(int actorNumber)
    {
        
        foreach (var titleEntry in titles)
        {
            MatchStatTitle title = titleEntry.Value;

            PlayerStats bestPlayer = null;
            float bestValue = float.MinValue;

            foreach (var stats in playerStats.Values)
            {
                if(stats.playerId == actorNumber)
                {
                    currentPlayer = stats;
                }

                if (title.statEvaluator == null)
                {
                    Debug.LogError($"statEvaluator is null for title {title.name}");
                    continue;
                }
                float value = title.statEvaluator(stats);
                if (value > bestValue)
                {
                    bestValue = value;
                    bestPlayer = stats;
                }
            }

            // If this title has a valid winner and it's the player we're asking about
            if (bestPlayer != null && bestValue > 0f && bestPlayer.playerId == actorNumber)
            {
                bestPlayer.SetTitle(title);
                return bestPlayer;
            }
        }

        currentPlayer.SetTitle(defaultTitles[0]);
        return currentPlayer;
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
