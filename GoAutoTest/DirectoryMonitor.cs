using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GoAutoTest
{
  public delegate void FileSystemEvent(string path);

  public class DirectoryMonitor
  { 
    private readonly FileSystemWatcher watcher =  new FileSystemWatcher(); 
    private readonly Dictionary<string, DateTime> pendingEvents =  new Dictionary<string, DateTime>(); 
    private readonly Timer timer; 
    private bool timerStarted = false;

    public DirectoryMonitor(string dirPath, string filter)
    {
      watcher.Path = Path.GetFullPath(dirPath); 
      watcher.IncludeSubdirectories = true;
      watcher.Filter = filter;
      watcher.Created += new FileSystemEventHandler(OnChange); 
      watcher.Changed += new FileSystemEventHandler(OnChange);  
      timer = new Timer(OnTimeout, null, Timeout.Infinite, Timeout.Infinite);
      watcher.EnableRaisingEvents = true;
      Console.WriteLine("Watching for changes to {0} files in: {1}", filter, Path.GetFullPath(dirPath));
    }  
    
    public event FileSystemEvent Change;

    private void OnChange(object sender, FileSystemEventArgs e) 
    {
      lock (pendingEvents) 
      { 
        pendingEvents[Path.GetDirectoryName(e.FullPath)] = DateTime.Now;  
        
        if (!timerStarted)
        {
          timer.Change(250, 250); 
          timerStarted = true;
        }  
      } 
    }  
    
    private void OnTimeout(object state) 
    { 
      List<string> paths;  
      
      lock (pendingEvents) 
      { 
        paths = FindReadyPaths(pendingEvents);  
        paths.ForEach(i => pendingEvents.Remove(i));  

        if (pendingEvents.Count == 0)
        {
          timer.Change(Timeout.Infinite, Timeout.Infinite); 
          timerStarted = false;
        } 
      }  
      
      paths.ForEach(FireEvent); 
    }  
    
    private List<string> FindReadyPaths(Dictionary<string, DateTime> events) 
    { 
      var results = new List<string>(); 
      var now = DateTime.Now;  
      
      foreach (var item in events)
      {
        if (now.Subtract(item.Value).TotalMilliseconds >= 140) 
          results.Add(item.Key); 
      }  
      return results; 
    }

    private void FireEvent(string path)
    {
      Console.WriteLine("Change detected at: {0}", path);
      FileSystemEvent evt = Change;
      if (evt != null)
      {
        evt(path);
      }
    } 
  } 
}
