using Godot;
using System;

public struct Date
{
    public Date()
    {
        day = DateTime.Now.Day;
        month = DateTime.Now.Month;
        year = DateTime.Now.Year;

        hour = DateTime.Now.Hour;
        minute = DateTime.Now.Minute;
        second = DateTime.Now.Second;
    }

    public int day;
    public int month;
    public int year;


    public int hour;
    public int minute;
    public int second;

    public string ExportAsString()
    {
        string date = "";

        date += day + ",";
        date += month + ",";
        date += year + ",";
        
        date += hour + ",";
        date += minute + ",";
        date += second;

        return date;
    }

    public void ImportFromString(string date)
    {
        string[] dateSplit = date.Split(',');

        day = int.Parse(dateSplit[0]);
        month = int.Parse(dateSplit[1]);
        year = int.Parse(dateSplit[2]);

        hour = int.Parse(dateSplit[3]);
        minute = int.Parse(dateSplit[4]);
        second = int.Parse(dateSplit[5]);
    }
}