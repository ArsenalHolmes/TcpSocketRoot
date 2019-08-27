using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

public class LogManger
{
    ILog log;
    public static LogManger Instance
    {
        get
        {
            if (instance == null)
            {
                new LogManger();
            }
            return instance;
        }
    }
    static LogManger instance;
    public string LogPath { get; private set; }
    string selfName;


    Thread FileThread;
    public Queue<LogItem> LogQueue;

    public LogManger(ILog log=null, string LogDirPath = "",string LogFileName="",string selfName="",int maxLength=5*1024*1024)
    {
        instance = this;
        this.log = log;
        this.selfName = selfName;
        if (string.IsNullOrEmpty(selfName))
        {
            this.selfName = Environment.MachineName;
        }

        LogDirPath = LogDirPath == "" ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) : LogDirPath;
        LogFileName = LogFileName == "" ? "Log.txt" : LogFileName;

        this.LogPath = string.Format(@"{0}\{1}_{2}_{3}_{4}_{5}", LogDirPath,this.selfName, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, LogFileName);// LogPath;
        LogQueue = new Queue<LogItem>();


        FileThread = new Thread(FileThreadFunc);
        FileThread.Start(Thread.CurrentThread);

    }
    void FileThreadFunc(object obj)
    {
        sw = new StreamWriter(LogPath, true, Encoding.UTF8);
        WriteLog("===============" + DateTime.Now.ToString() + "===============");
        Thread main = (Thread)obj;
        while (main.IsAlive)
        {
            while (LogQueue.Count>0)
            {
                WriteLog(LogQueue.Dequeue().ToString());
            }
            sw.Flush();
            Thread.Sleep(100);
        }

        while (LogQueue.Count > 0)
        {
            WriteLog(LogQueue.Dequeue().ToString());
        }
        sw.Flush();

        SaveLog();
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
        LogItem item = new LogItem(msg, selfName,mt);
        LogQueue.Enqueue(item);
    }


    StreamWriter sw;
    public void WriteLog(string msg)
    {
        try
        {
            sw.WriteLine(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void SaveLog()
    {
        sw.Flush();
        sw.Close();
        sw.Dispose();
        sw = null;
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

