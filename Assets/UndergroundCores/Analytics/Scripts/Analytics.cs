
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leguar.TotalJSON;

using MySql;
using MySql.Data.MySqlClient;

namespace N4M2Q
{

  namespace Analytics
  {


    enum eErrorLevel : uint
    {
      NONE = 0,
      LOW,
      MEDIUM,
      HIGH,
      CRITICAL
    }

    public class Analytics : MonoBehaviour
    {

      // Singleton
      public static Analytics Get { get => _instance; }

      // Public members
      #region PublicMembers
      public static bool created { get => _instance ?? false; } 

      public uint maxQueueTime { get => _maxQueueTime; }

      public uint maxQueueSize { get => _maxQueueSize; }

      public string username { get => _username; }

      public string mail { get => _mail; }

      public uint sessionID { get => _sessionID; }

      public uint userID { get => _userID; }

      public string sessionTag { get => _sessionTag; }

      public string database { get => _database; }

      public string url { get => _url; }

      #endregion

      // Private members
      #region ProtectedMethods

      protected static Analytics _instance = null;

      protected uint _maxQueueTime = 30;

      protected uint _maxQueueSize = 1000;

      protected string _username;

      protected string _mail;

      protected uint _sessionID;

      protected uint _userID;

      protected string _sessionTag;

      protected string _url;

      protected string _urlUsername;

      protected string _urlPassword;

      protected string _database;

      protected float _internalTime;

      protected bool _needsCredentials;

      #endregion

      // Methods
      #region Methods

      public static bool
      IsStarted()
      { return _instance != null; }

      public static void
      StartUp(Analytics instance)
      {
        if (_instance == null && instance != null)
        {
          _instance = instance;
          DontDestroyOnLoad(_instance);
        }
        else
        {
          Debug.LogError($"Trying to initialize Analytics Module " +
                         $"when it is already initialized");
        }
      }

      public void
      SetMaxQueueTime(uint newMaxQueueTime)
      { _maxQueueTime = newMaxQueueTime; }

      public void
      SetMaxQueueSize(uint newMaxQueueSize)
      { _maxQueueSize = newMaxQueueSize; }

      public void
      SetUserName(string newUsername)
      { _username = newUsername; }

      public void
      SetMail(string newMail)
      { _mail = newMail; }

      public void
      SetSessionID(uint newSessionID)
      { _sessionID = newSessionID; }

      public void
      SetUserID(uint newUserID)
      { _userID = newUserID; }

      public void
      SetSessionTag(string newSessionTag)
      { _sessionTag = newSessionTag; }

      public void
      SetDatabaseURL(string newURL)
      { _url = newURL; }

      public void
      SetCredentials(string newUsername, string newPassword)
      {
        _urlUsername = newUsername;
        _urlPassword = newPassword;
      }

      #endregion

      // Virtual Methods
      #region VirtualMethods

      public virtual JSON
      GetTables()
      {
        return null;
      }

      public virtual void
      SetDatabase(string newDatabase)
      { _database = newDatabase; }

      public virtual JSON
      CreateJSONHeader(string table)
      { return null; }

      public virtual JSON
      CreateJSONBody(string eventID)
      { return null; }

      public virtual void
      Initialize(string databaseURL)
      { _url = databaseURL; }

      public virtual void
      Reset()
      { }

      public virtual void
      SendQueue()
      { }

      public virtual void
      ForceQueue()
      { }

      public virtual void
      OnRequestSent()
      { }

      public virtual void
      OnRequestComplete()
      { }

      public virtual void
      SendDesignEvent(string table,
                      string eventID)
      {

      }

      public virtual void
      SendDesignEvent(string table,
                      string eventID,
                      float floatValue)
      {

      }

      public virtual void
      SendDesignEvent(string table,
                      string eventID,
                      string stringValue)
      {

      }

      public virtual void
      SendDesignEvent(string table,
                      string eventID,
                      float floatValue,
                      string stringValue)
      {

      }

      public virtual void
      QueueDesignEvent(string table,
                       string eventID)
      {

      }

      public virtual void
      QueueDesignEvent(string table,
                       string eventID,
                       float floatValue)
      {

      }

      public virtual void
      QueueDesignEvent(string table,
                       string eventID,
                       string stringValue)
      {

      }

      public virtual void
      QueueDesignEvent(string table,
                       string eventID,
                       float floatValue,
                       string stringValue)
      {

      }

      public virtual JSON
      QueryData(string table, string eventID)
      {
        return null;
      }

      public virtual JSON
      QueryData(string table, string eventID, float floatValue)
      {
        return null;
      }

      public virtual JSON
      QueryData(string table, string eventID, string stringValue)
      {
        return null;
      }

      public virtual JSON
      QueryData(string table, string eventID, float floatValue, string stringValue)
      {
        return null;
      }

      public virtual JSON
      QueryData(string table, uint sessionID)
      {
        return null;
      }

      public virtual JSON
      QueryData(string table, uint sessionID, string eventID)
      {
        return null;
      }

      #endregion

    }

  }
  
}