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
    string LogPath;
    string selfName;
    public LogManger(ILog log, string LogPath = "",string selfName="",int maxLength=5*1024*1024)
    {
        instance = this;
        this.log = log;
        this.LogPath = LogPath;
        this.selfName = selfName;
        if (this.selfName=="")
        {
            this.selfName= Environment.MachineName;
        }
        if (LogPath != "")
        {
            if (File.Exists(LogPath))
            {
                FileInfo fi = new FileInfo(LogPath);
                if (fi.Length > maxLength)//5M
                {
                    File.Delete(LogPath);
                }
            }
            sw = new StreamWriter(LogPath, true, Encoding.UTF8);
            WriteLog("-----" + DateTime.Now.ToLocalTime().ToString() + "-----");
        }
    }

    public void Info(object msg)
    {
        LogItem item = new LogItem(msg, selfName, MsgType.Error);
        if (log != null)
        {
            log.Info(item.ToString());
        }
        WriteLog(item.ToString());
    }
    public void Warning(object msg)
    {
        LogItem item = new LogItem(msg, selfName, MsgType.Error);
        if (log != null)
        {
            log.Warning(item.ToString());
        }
        WriteLog(item.ToString());
    }
    public void Error(object msg)
    {
        LogItem item = new LogItem(msg, selfName, MsgType.Error);
        if (log != null)
        {
            log.Error(item.ToString());
        }
        WriteLog(item.ToString());
    }

    StreamWriter sw;
    void WriteLog(string msg)
    {
        if (sw != null)
        {
            sw.WriteLine(msg);
        }
    }
    public void SaveLog()
    {
        if (sw != null)
        {
            sw.Dispose();
            sw.Close();
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
    void Info(string msg);
    void Warning(string msg);
    void Error(string msg);
}

