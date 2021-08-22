using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Leguar.TotalJSON;

namespace SWT
{
  namespace Analytics
  {
    public class AnalyticsES : Analytics
    {

      #region PrivateMembers

      private List<string> _queue = new List<string>();

      private List<string> _responseQueue = new List<string>();

      private Object _lock = new Object();

      private float _ms = 0.0f;

      #endregion

      #region MonobehaviorMethods

      private void Awake()
      {

        if (_instance != this && _instance != null) { Destroy(this.gameObject); }
        else
        {
          _instance = this;
          DontDestroyOnLoad(_instance);
        }
      }

      private void Start()
      {
        _queue = new List<string>();
      }

      private void Update()
      {
        _internalTime += Time.deltaTime;
        if (_internalTime >= maxQueueTime)
        {
          SendQueue();
          _internalTime = 0.0f;
        }
      }

      private void OnDestroy()
      {
        _instance = null;
      }

      #endregion

      #region Methods

      private void AddToQueue(JSON header, JSON body)
      {
        string headerStr = "";
        string bodyStr = "";

        headerStr = header.CreateString();
        bodyStr = body.CreateString();

        headerStr.Replace('\r', ' ');
        headerStr.Replace('\n', ' ');
        headerStr.Replace('\t', ' ');

        bodyStr.Replace('\r', ' ');
        bodyStr.Replace('\n', ' ');
        bodyStr.Replace('\t', ' ');

        string output = headerStr + "\n" + bodyStr;

        _queue.Add(output);

        SendQueue();
      }

      #endregion

      #region OverrideMethods

      [System.Obsolete("THIS IS EQUIVALENT TO GETTING DOCS, BUT BECAUSE FROM VERSION 5.0 " +
                       "THERE CAN ONLY EXIST ONE DOC PER INDEX, THIS IS NO LONGER USEFUL")]
      public override JSON
      GetTables()
      {
        return null;
      }

      public override void
      SetDatabase(string newDatabase)
      { _database = newDatabase; }

      protected override JSON
      CreateJSONHeader(string table)
      {
        if (table == string.Empty) { table = "_doc"; }

        JSON header = new JSON();
        header.Add("_index", database);
        header.Add("_type", table);

        JSON doc = new JSON();

        doc.Add("index", header);

        return doc;
      }


      protected override JSON
      CreateJSONBody(string eventID)
      {
        JSON body = new JSON();
        
        // Basic information
        body.Add("timestamp", GetTimeStamp());
        body.Add("os_name", SystemInfo.operatingSystem);
        body.Add("os_platform", System.Environment.OSVersion.Platform.ToString());
        body.Add("os_family", SystemInfo.operatingSystemFamily.ToString());
        body.Add("os_version", System.Environment.OSVersion.Platform.ToString());
        body.Add("os_type", System.Environment.Is64BitOperatingSystem ? "x64" : "x86");

        // Basic Hardware Information
        body.Add("device_name", SystemInfo.deviceName);
        body.Add("device_model", SystemInfo.deviceModel);
        body.Add("device_type", SystemInfo.deviceType);

        // CPU Information
        body.Add("cpu_name", SystemInfo.processorType);
        body.Add("cpu_type", System.Environment.Is64BitProcess ? "x64" : "x86");
        body.Add("cpu_memory_size", SystemInfo.systemMemorySize);
        body.Add("cpu_memory_used", System.GC.GetTotalMemory(true));
        body.Add("cpu_count", SystemInfo.processorCount);
        body.Add("cpu_frequency", SystemInfo.processorFrequency);

        // GPU Information
        body.Add("gpu_name", SystemInfo.graphicsDeviceName);
        body.Add("gpu_id", SystemInfo.graphicsDeviceID);
        body.Add("gpu_type", SystemInfo.graphicsDeviceType.ToString());
        body.Add("gpu_memory_size", SystemInfo.graphicsMemorySize);

        // Session information
        body.Add("session_id", sessionID);
        body.Add("user_id", userID);
        body.Add("username", username);
        body.Add("user_mail", mail);
        body.Add("event_id", eventID);

        return body;
      }

      public override void
      Initialize(string databaseURL)
      {
        base.Initialize(databaseURL);
        if (_database == string.Empty) { _database = "default"; }
        if (_url == string.Empty) { _url = "localhost:9200"; }
      }


      public override void
      Reset()
      {
        _queue.Clear();
      }

      public override void
      SendQueue()
      {
        // Lock to prevent data repetition, race problems and such
        System.Threading.Monitor.Enter(_lock);
        try
        {
          if (_queue.Count >= maxQueueSize || (_queue.Count > 0 && _internalTime >= maxQueueTime))
          {
            string output = "";
            foreach (string data in _queue)
            {
              output += (data + "\n");
            }

            System.Net.WebClient request = new System.Net.WebClient();
            System.Uri urlToPost = new System.Uri(url + "/_bulk?pretty");
            request.Headers.Add("Content-Type", "application/json");

            if (needsCredentials)
            {
              request.Credentials = new System.Net.NetworkCredential(_urlUsername, _urlPassword);
            }

            request.UploadStringCompleted += Request_UploadStringCompleted;
            request.UploadStringAsync(urlToPost, "POST", output);
            OnRequestSent();
          }
        }
        catch
        {
          Debug.LogWarning("Someone is trying or forcing to send information while the request is" +
                           "being still processed. Waiting for next opportunity");
        }
      }

      private void Request_UploadStringCompleted(object sender, System.Net.UploadStringCompletedEventArgs e)
      {
        AnalyticsResponse response = new AnalyticsResponse();
        response.SetData(e.Cancelled, e.Error, e.Result);
        System.Threading.Monitor.Exit(_lock);
        OnRequestComplete(response);
      }

      public override void
      ForceQueue()
      { 
        if (_queue.Count == 0) { return; }

        string output = "";
        foreach (string data in _queue)
        {
          output += (data + "\n");
        }

        System.Net.WebClient request = new System.Net.WebClient();
        System.Uri urlToPost = new System.Uri(url + "/_bulk?pretty");
        request.Headers.Add("Content-Type", "application/json");
        if (needsCredentials)
        {
          request.Credentials = new System.Net.NetworkCredential(_urlUsername, _urlPassword);
        }
        request.UploadStringCompleted += Request_UploadStringCompleted;
        request.UploadStringAsync(urlToPost, "POST", output);
        OnRequestSent();
      }

      public override void
      OnRequestSent()
      {
        _queue.Clear();
        base.OnRequestSent();
      }

      public override void
      OnRequestComplete(AnalyticsResponse response)
      {
        if (response.cancelled)
        {
          Debug.LogError("The response was cancelled.\nThe reasons are: " + 
                         response.message);
        }
        if (response.error != null)
        {
          Debug.LogError("The response returned with an error.\nThe reasons are:" +
                         response.error.Message);
        }
        base.OnRequestComplete(response);
      }

      public override void
      SendDesignEvent(string table,
                      string eventID)
      {
        var body = CreateJSONBody(eventID);

        var bodyStr = body.CreateString();
        bodyStr.Replace('\r', ' ');
        bodyStr.Replace('\n', ' ');
        bodyStr.Replace('\t', ' ');

        System.Net.WebClient request = new System.Net.WebClient();
        System.Uri urlToPost = new System.Uri(url + $"/{database}/{table}/message?pretty");
        request.Headers.Add("Content-Type", "application/json");
        if (needsCredentials)
        {
          request.Credentials = new System.Net.NetworkCredential(_urlUsername, _urlPassword);
        }
        request.UploadStringCompleted += Request_UploadStringCompleted;
        request.UploadStringAsync(urlToPost, "POST", bodyStr);
        OnRequestSent();
      }

      public override void
      SendDesignEvent(string table,
                      string eventID,
                      float floatValue)
      {
        if (table == string.Empty) { table = "_doc"; }
        var body = CreateJSONBody(eventID);
        body.Add("float_value", floatValue);

        var bodyStr = body.CreateString();
        bodyStr.Replace('\r', ' ');
        bodyStr.Replace('\n', ' ');
        bodyStr.Replace('\t', ' ');

        System.Net.WebClient request = new System.Net.WebClient();
        System.Uri urlToPost = new System.Uri(url + $"/{database}/{table}/message?pretty");
        request.Headers.Add("Content-Type", "application/json");
        if (needsCredentials)
        {
          request.Credentials = new System.Net.NetworkCredential(_urlUsername, _urlPassword);
        }
        request.UploadStringCompleted += Request_UploadStringCompleted;
        request.UploadStringAsync(urlToPost, "POST", bodyStr);
        OnRequestSent();
      }

      public override void
      SendDesignEvent(string table,
                      string eventID,
                      string stringValue)
      {
        if (table == string.Empty) { table = "_doc"; }
        var body = CreateJSONBody(eventID);
        body.Add("string_value", stringValue);

        var bodyStr = body.CreateString();
        bodyStr.Replace('\r', ' ');
        bodyStr.Replace('\n', ' ');
        bodyStr.Replace('\t', ' ');

        System.Net.WebClient request = new System.Net.WebClient();
        System.Uri urlToPost = new System.Uri(url + $"/{database}/{table}/message?pretty");
        request.Headers.Add("Content-Type", "application/json");
        if (needsCredentials)
        {
          request.Credentials = new System.Net.NetworkCredential(_urlUsername, _urlPassword);
        }
        request.UploadStringCompleted += Request_UploadStringCompleted;
        request.UploadStringAsync(urlToPost, "POST", bodyStr);
        OnRequestSent();
      }

      public override void
      SendDesignEvent(string table,
                      string eventID,
                      float floatValue,
                      string stringValue)
      {
        if (table == string.Empty) { table = "_doc"; }
        var body = CreateJSONBody(eventID);
        body.Add("float_value", floatValue);
        body.Add("string_value", stringValue);

        var bodyStr = body.CreateString();
        bodyStr.Replace('\r', ' ');
        bodyStr.Replace('\n', ' ');
        bodyStr.Replace('\t', ' ');

        System.Net.WebClient request = new System.Net.WebClient();
        System.Uri urlToPost = new System.Uri(url + $"/{database}/{table}/message?pretty");
        request.Headers.Add("Content-Type", "application/json");
        if (needsCredentials)
        {
          request.Credentials = new System.Net.NetworkCredential(_urlUsername, _urlPassword);
        }
        request.UploadStringCompleted += Request_UploadStringCompleted;
        request.UploadStringAsync(urlToPost, "POST", bodyStr);
        OnRequestSent();
      }

      public override void
      QueueDesignEvent(string table,
                       string eventID)
      {
        var header = CreateJSONHeader(table);
        var body = CreateJSONBody(eventID);

        AddToQueue(header, body);

      }

      public override void
      QueueDesignEvent(string table,
                       string eventID,
                       float floatValue)
      {
        var header = CreateJSONHeader(table);
        var body = CreateJSONBody(eventID);
        body.Add("float_value", floatValue);

        AddToQueue(header, body);
      }

      public override void
      QueueDesignEvent(string table,
                       string eventID,
                       string stringValue)
      {
        var header = CreateJSONHeader(table);
        var body = CreateJSONBody(eventID);
        body.Add("string_value", stringValue);

        AddToQueue(header, body);
      }

      public override void
      QueueDesignEvent(string table,
                       string eventID,
                       float floatValue,
                       string stringValue)
      {
        var header = CreateJSONHeader(table);
        var body = CreateJSONBody(eventID);
        body.Add("float_value", floatValue);
        body.Add("string_value", stringValue);

        AddToQueue(header, body);
      }

      public override void QueueErrorEvent(string table, 
                                           eErrorLevel severity, 
                                           string message)
      {
        var header = CreateJSONHeader(table);
        var body = CreateJSONBody(severity.ToString());
        body.Add("string_value", message);

        AddToQueue(header, body);
      }

      public override JSON
      QueryData(string table, string eventID)
      {
        return null;
      }

      public override JSON
      QueryData(string table, string eventID, float floatValue)
      {
        return null;
      }

      public override JSON
      QueryData(string table, string eventID, string stringValue)
      {
        return null;
      }

      public override JSON
      QueryData(string table, string eventID, float floatValue, string stringValue)
      {
        return null;
      }

      public override JSON
      QueryData(string table, uint sessionID)
      {
        return null;
      }

      public override JSON
      QueryData(string table, uint sessionID, string eventID)
      {
        return null;
      }

      public override void
      Ping()
      {
        System.Net.WebClient request = new System.Net.WebClient();
        System.Uri urlToPost = new System.Uri(url);
        request.Headers.Add("Content-Type", "application/json");

        request.DownloadStringCompleted +=
          new System.Net.DownloadStringCompletedEventHandler(
            delegate (object sender,
                      System.Net.DownloadStringCompletedEventArgs e)
          {
            Debug.Log(e.Result);
          });
        request.DownloadStringAsync(urlToPost);
        OnRequestSent();
      }

      #endregion

    }
  }
}
