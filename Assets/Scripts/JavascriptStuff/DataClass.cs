using System;

[Serializable]
public class DataClass
{
    public string _id;

    public string playerName;
    public int playerWins;
    public int gamesPlayed;
}

[Serializable]
public class ListUser
{
    public DataClass[] game;
}

[Serializable]
public class deleteClass
{
    public string id;
    public string holder;
    public int score;
}