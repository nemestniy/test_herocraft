using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public string PathToDashboard = "Dashboard.json";

    public List<DashboardItem> ReadDashboard()
    {
        if (!File.Exists(Directory.GetCurrentDirectory() + "//" + PathToDashboard)) return null;

        var data = new List<DashboardItem>();

        using(StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "//" + PathToDashboard))
        {
            if (reader == null) return null;
            var json = reader.ReadToEnd();
            data = JsonUtility.FromJson<Dashboard>(json).DashboardItems;
        }

        return data;
    }

    public void SaveResults(DashboardItem item)
    {
        var list = ReadDashboard();

        if (list == null) list = new List<DashboardItem>();

        list.Add(item);

        var data = new Dashboard(list);

        try
        {
            using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "//" + PathToDashboard))
            {
                writer.WriteLine(JsonUtility.ToJson(data));
            }
        }catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
}
