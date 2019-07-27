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
    public LogManger(ILog log, string LogPath = "",int maxLength=5*1024*1024)
    {
        instance = this;
        this.log = log;
        this.LogPath = LogPath;
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
        LogItem item = new LogItem(msg);
        if (log != null)
        {
            log.Info(item.ToString());
        }
        WriteLog(item.ToString());
    }
    public void Warning(object msg)
    {
        LogItem item = new LogItem(msg, MsgType.Warning);
        if (log != null)
        {
            log.Warning(item.ToString());
        }
        WriteLog(item.ToString());
    }
    public void Error(object msg)
    {
        LogItem item = new LogItem(msg, MsgType.Error);
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
    string timeStr;

    public LogItem(object msg, MsgType type = MsgType.Info)
    {
        this.type = type;
        this.msg = msg;
        this.timeStr = DateTime.Now.ToLocalTime().ToString();
    }

    public override string ToString()
    {
        return string.Format("{0}----{1}----{2}", type.ToString(), msg, timeStr);
    }
}

public interface ILog
{
    void Info(string msg);
    void Warning(string msg);
    void Error(string msg);
}

