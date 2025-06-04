[System.Serializable]
public class PlayerStats
{
    public string playerId;
    public string playerName;

    public int kills;
    public int deaths;
    public int shotsFired;
    public int shotsHit;
    public int bombKills;

    public void Reset()
    {
        kills = deaths = shotsFired = shotsHit = bombKills = 0;
    }
}
