using ExitGames.Client.Photon;

public static class LockerData
{
    public static int HatID;
    public static int BodyID;

    public static Hashtable WholeOutfit
    {
        get
        {
            var table = new ExitGames.Client.Photon.Hashtable();
            table.Add("HatID", HatID);
            table.Add("BodyID", BodyID);
            return table;
        }
    }
}
