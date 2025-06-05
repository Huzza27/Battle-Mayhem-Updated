[System.Serializable]
public class PlayerStats
{
    public int playerId;
    public string playerName;

    public int kills;
    public int deaths;
    public int shotsFired;
    public int shotsHit;
    public int bombKills;

    public MatchStatTitle title;

    public void Reset()
    {
        kills = deaths = shotsFired = shotsHit = bombKills = 0;
    }

    public void SetTitle(MatchStatTitle newTitle)
    {
        title = newTitle;
    }
}
