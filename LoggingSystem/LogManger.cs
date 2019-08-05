using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


public class LogManger
{
    ILog log;
    public static LogManger Instance
    {
        get
        {
            if (instance == null)
            {
                throw new Exception("LogManger没有生成");
            }
            return instance;
        }
    }
    static LogManger instance;
    public string LogPath { get; private set; }
    string selfName;
    public LogManger(ILog log, string LogDirPath = "",string LogFileName="",bool isShowData=true,string selfName="",int maxLength=5*1024*1024)
    {
        instance = this;
        this.log = log;
        this.LogPath = string.Format("{0}/{1}_{2}_{3}_{4}", LogDirPath, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, LogFileName);// LogPath;
        this.selfName = selfName;
        if (this.selfName=="")
        {
            this.selfName= Environment.MachineName;
        }
        if (LogPath != ""&&LogFileName!="")
        {
            if (File.Exists(LogPath))
            {
                FileInfo fi = new FileInfo(LogPath);
                if (fi.Length > maxLength)//5M
                {
                    File.Delete(LogPath);
                    sw = File.CreateText(LogPath);
                }
                sw = new StreamWriter(LogPath, true, Encoding.UTF8);
            }
            else
            {
                sw  = File.CreateText(LogPath);
            }
            WriteLog("=====" + DateTime.Now.ToLocalTime().ToString() + "=====");



        }
    }

    public void Info(object msg,bool isPrint=true)
    {
        if (log != null && isPrint)
        {
            log.Info(msg);
        }
        HandLogItem(msg, MsgType.Info);
    }

    public void Warning(object msg, bool isPrint = true)
    {
        if (log != null && isPrint)
        {
            log.Warning(msg);
        }
        HandLogItem(msg, MsgType.Warning);
    }
    public void Error(object msg, bool isPrint = true)
    {
        if (log != null && isPrint)
        {
            log.Error(msg);
        }
        HandLogItem(msg,MsgType.Error);
    }

    void HandLogItem(object msg,MsgType mt)
    {
        LogItem item = new LogItem(msg, selfName, MsgType.Error);
        WriteLog(item.ToString());
    }


    StreamWriter sw;
    public void WriteLog(string msg)
    {
        try
        {
            if (sw != null)
            {
                sw.WriteLine(msg);
            }
        }
        catch (Exception e)
        {

        }

    }
    public void SaveLog()
    {
        if (sw != null)
        {
            sw.Dispose();
            sw.Close();
            sw = null;
        }
    }
}

public enum MsgType
{
    Info,
    Warning,
    Error,
}
public struct LogItem
{


    MsgType type;
    object msg;
    DateTime time;
    string selfName;

    public LogItem(object msg,string selfName, MsgType type = MsgType.Info)
    {
        this.type = type;
        this.msg = msg;
        this.selfName = selfName;
        this.time = DateTime.Now.ToLocalTime();
    }
    public LogItem(string str)
    {
        str = str.Replace("----", "-");
        string[] arr = str.Split('-');
        selfName = arr[0];
        type = (MsgType)Enum.Parse(typeof(MsgType), arr[1]);
        this.msg = arr[2];
        time = DateTime.Parse(arr[3]);
    }

    public override string ToString()
    {
        return string.Format("{0}----{1}----{2}----{3}", selfName, type.ToString(), msg, time);
    }
}

public interface ILog
{
    void Info(object msg);
    void Warning(object msg);
    void Error(object msg);
}

