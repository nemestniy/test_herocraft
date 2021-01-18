using System;
using System.Collections.Generic;

[Serializable]
public struct DashboardItem
{
    public string CarName;
    public float Time;
    public int Score;

    public DashboardItem(string name, float time, int score)
    {
        CarName = name;
        Time = time;
        Score = score;
    }
}

[Serializable]
public struct Dashboard
{
    public List<DashboardItem> DashboardItems;

    public Dashboard(List<DashboardItem> items)
    {
        DashboardItems = items;
    }
}

