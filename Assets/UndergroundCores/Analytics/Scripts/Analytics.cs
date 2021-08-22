
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using Leguar.TotalJSON;

// using MySql;
// using MySql.Data.MySqlClient;

namespace SWT
{

  namespace Analytics
  {
    [System.Serializable]
    public class AnalyticsEvent : UnityEvent<AnalyticsResponse> { }


    [System.Serializable]
    public enum eErrorLevel : uint
    {
      [Tooltip("Default value")]
      NONE = 0,
      [Tooltip("Low level errors. This could be used for warnings")]
      LOW,
      [Tooltip("Medium level errors. This could be used for errors that don't break or change behaviors")]
      MEDIUM,
      [Tooltip("High level errors. This could be used for errors that need to be changed ASAP")]
      HIGH,
      [Tooltip("Critical level errors. This errors could break the game")]
      CRITICAL
    }

    [System.Serializable]
    public class AnalyticsResponse
    {
      /// <summary>
      /// Public getter for _cancelled
      /// </summary>
      [SerializeField]
      public bool cancelled { get => _cancelled; }

      /// <summary>
      /// Public getter for _error
      /// </summary>
      [SerializeField]
      public System.Exception error { get => _error; }

      /// <summary>
      /// Public getter for _message
      /// </summary>
      [SerializeField]
      public string message { get => _message; }

      /// <summary>
      /// A bool set if there was any cancellation on the response
      /// </summary>
      [Tooltip("True if the response was cancelled")]
      protected bool _cancelled = false;

      /// <summary>
      /// The Exception that contains the information of why and how an error appears
      /// </summary>
      [Tooltip("A C# exception filled with information if the response failed")]
      protected System.Exception _error = null;

      /// <summary>
      /// The message sent by the response
      /// </summary>
      [Tooltip("The response sent by the callback")]
      protected string _message = "";

      /// <summary>
      /// Sets the internal _cancelled value
      /// </summary>
      /// <param name="c">true if there was any cancellation</param>
      public void SetCancelled(bool c) { _cancelled = c; }

      /// <summary>
      /// Sets the internal _error value
      /// </summary>
      /// <param name="e">the error if there was any, if not, you can set this with null</param>
      public void SetError(System.Exception e) { _error = e; }

      /// <summary>
      /// Sets the internal _message value
      /// </summary>
      /// <param name="msg">the string of the response</param>
      public void SetMessage(string msg) { _message = msg; }

      /// <summary>
      /// Sets all the information of a response
      /// </summary>
      /// <param name="ifCancelled">true if there was any cancellation</param>
      /// <param name="ifException">the error if there was any, if not, you can set this with null</param>
      /// <param name="ifMsg">the string of the response</param>
      public void SetData(bool ifCancelled, System.Exception ifException, string ifMsg = "")
      {
        _cancelled = ifCancelled;
        _error = ifException;
        _message = ifMsg;
      }

    }

    /// <summary>
    /// The Analytics base class. It contains the base definitions for Analytics inheritance
    /// </summary>
    public class Analytics : MonoBehaviour
    {

      // Singleton/Module
      /// <summary>
      /// Returns the instance of the Analytics Object
      /// </summary>
      public static Analytics Get { get => _instance; }

      // Public members
      #region PublicMembers
      /// <summary>
      /// Checks whether is the analytics object already created or not
      /// </summary>
      public static bool created { get => _instance ?? false; }

      /// <summary>
      /// The max time between queues
      /// </summary>
      public uint maxQueueTime { get => _maxQueueTime; }

      /// <summary>
      /// The max size of the queue before it automatically sends the data
      /// </summary>

      public uint maxQueueSize { get => _maxQueueSize; }

      /// <summary>
      /// The Username of the player to set in the analytics database
      /// </summary>
      public string username { get => _username; }

      /// <summary>
      /// mail of the user used in the game
      /// </summary>
      public string mail { get => _mail; }

      /// <summary>
      /// Session ID set by the time the session is started
      /// </summary>
      public uint sessionID { get => _sessionID; }

      /// <summary>
      /// The ID of the user in the database
      /// </summary>
      public uint userID { get => _userID; }

      /// <summary>
      /// The tag or tags of the session. This can be useful to separate data
      /// </summary>
      public string sessionTag { get => _sessionTag; }

      /// <summary>
      /// The database name
      /// </summary>
      public string database { get => _database; }

      /// <summary>
      /// The URL of the database
      /// </summary>
      public string url { get => _url; }

      /// <summary>
      /// If the user needs credentials to access database
      /// </summary>
      public bool needsCredentials { get => _needsCredentials; }

      /// <summary>
      /// Event to be sent once the request of queue is sent
      /// </summary>
      [Tooltip("The events to be sent when the request is sent")]
      public UnityEvent RequestSentEvent = new UnityEvent();

      /// <summary>
      /// The event to be sent once the request of queue is completed
      /// </summary>
      [Tooltip("The events to be sent when the request is complete")]
      public AnalyticsEvent RequestCompleteEvent = new AnalyticsEvent();

      #endregion

      // Private members
      #region ProtectedMethods

      /// <summary>
      /// Private instance of Analytics
      /// </summary>
      protected static Analytics _instance = null;

      /// <summary>
      /// Private value of maxQueueTime
      /// </summary>
      [SerializeField, Tooltip("The time to wait until analytics sends the batch")]
      protected uint _maxQueueTime = 30;

      /// <summary>
      /// Private value of maxQueueSize
      /// </summary>
      [SerializeField, Tooltip("The max amount of objects to hold in the queue before batching it")]
      protected uint _maxQueueSize = 1000;

      /// <summary>
      /// Protected value of username
      /// </summary>
      [SerializeField, Tooltip("Username of the player")]
      protected string _username = "anonymous";

      /// <summary>
      /// Protected value of mail
      /// </summary>
      [SerializeField, Tooltip("use registered mail")]
      protected string _mail = "foo@bar.com";

      /// <summary>
      /// Protected value of sessionID
      /// </summary>
      protected uint _sessionID = 0;

      /// <summary>
      /// Protected value of userID
      /// </summary>
      protected uint _userID = 0;

      /// <summary>
      /// Protected value of sessionTag
      /// </summary>
      [SerializeField, Tooltip("The tag or tags of the session. This lets you organize and query")]
      protected string _sessionTag = "";

      /// <summary>
      /// Protected value of URL
      /// </summary>
      [SerializeField, Tooltip("The url of the database")]
      protected string _url = "";

      /// <summary>
      /// The username of the database. This to access the database
      /// </summary>
      protected string _urlUsername = "admin";

      /// <summary>
      /// The password requested in the database
      /// </summary>
      protected string _urlPassword = "admin";

      /// <summary>
      /// Protected value of database
      /// </summary>
      protected string _database = "default";

      /// <summary>
      /// Internal timer used to count when to send information
      /// </summary>
      protected float _internalTime = 0.0f;

      /// <summary>
      /// Whether the url needs credentials to store information in the database
      /// </summary>
      [SerializeField, Tooltip("Check this if there is any need for credentials")]
      protected bool _needsCredentials = false;

      #endregion

      // Methods
      #region Methods

      /// <summary>
      /// This function checks if the module is started properly
      /// </summary>
      /// <returns>  </returns>
      public static bool
      IsStarted()
      { return _instance != null; }

      /// <summary>
      /// Starts up the analytics object by assigning it by code.  
      /// This function is used by the factory handler
      /// </summary>
      /// <param name="instance">Analytics.Analytics inherited object</param>
      public static void
      StartUp(Analytics instance)
      {
        // Checks up if the instance is already set up
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

      /// <summary>
      /// Sets the max time in between data uploading
      /// </summary> 
      /// <param name="newMaxQueueTime">The time in seconds to set </param>
      public void
      SetMaxQueueTime(uint newMaxQueueTime)
      { _maxQueueTime = newMaxQueueTime; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="newMaxQueueSize">the new size of the queue</param>
      public void
      SetMaxQueueSize(uint newMaxQueueSize)
      { _maxQueueSize = newMaxQueueSize; }

      /// <summary>
      /// Sets the username
      /// </summary>
      /// <param name="newUsername">the username</param>
      public void
      SetUserName(string newUsername)
      { _username = newUsername; }

      /// <summary>
      /// Sets the mail of the user
      /// </summary>
      /// <param name="newMail">the new mail to set</param>
      public void
      SetMail(string newMail)
      { _mail = newMail; }

      /// <summary>
      /// Sets the session ID. This should be normally a value set by the time
      /// so it is always different, but can be set with any unsigned int
      /// </summary>
      /// <param name="newSessionID">the session id</param>
      public void
      SetSessionID(uint newSessionID)
      { _sessionID = newSessionID; }

      /// <summary>
      /// Sets internally the user ID
      /// </summary>
      /// <param name="newUserID">the new user ID to store</param>
      public void
      SetUserID(uint newUserID)
      { _userID = newUserID; }

      /// <summary>
      /// Sets a tag or series of tags in the module
      /// </summary>
      /// <param name="newSessionTag">
      /// tag or tags separated by a comma (i.e: tag, another tag)
      /// </param>
      public void
      SetSessionTag(string newSessionTag)
      { _sessionTag = newSessionTag; }

      /// <summary>
      /// Sets the URL where the database is stored
      /// </summary>
      /// <param name="newURL">the URL the database is stored</param>
      public void
      SetDatabaseURL(string newURL)
      { _url = newURL; }

      /// <summary>
      /// Sets the credentials needed for the module to access and store info
      /// </summary>
      /// <param name="newUsername">the username</param>
      /// <param name="newPassword">the password</param>
      public void
      SetCredentials(string newUsername, string newPassword)
      {
        _urlUsername = newUsername;
        _urlPassword = newPassword;
      }

      public static string GetTimeStamp()
      {
        string currentDate = "";

        System.DateTime now = System.DateTime.UtcNow;

        currentDate = now.ToString("yyyy/MM/dd HH:mm:ss");

        return currentDate;
      }

      #endregion

      // Virtual Methods
      #region VirtualMethods

      /// <summary>
      /// Returns a list with the names of the tables/indices/ whatever you want
      /// to call them that have the information stored in. This is useful
      /// for databases such as SQL or MYSQL
      /// </summary>
      /// <returns></returns>
      public virtual JSON
      GetTables()
      {
        return null;
      }

      /// <summary>
      /// Sets the database where the information will be stored. The developer 
      /// might separate data in different databases or yadda yadda
      /// </summary>
      /// <param name="newDatabase"></param>
      public virtual void
      SetDatabase(string newDatabase)
      { _database = newDatabase; }

      /// <summary>
      /// Creates the header of the data being sent, useful for batch sending 
      /// information in ES
      /// </summary>
      /// <param name="table">the name of the table to insert the information</param>
      /// <returns>the JSON that holds the information</returns>
      protected virtual JSON
      CreateJSONHeader(string table)
      { return null; }

      /// <summary>
      /// Creates a JSON body, useful for batch sending information in ES
      /// </summary>
      /// <param name="eventID">the id of the event</param>
      /// <returns>the JSON that holds the information of the analytics</returns>
      protected virtual JSON
      CreateJSONBody(string eventID)
      { return null; }

      /// <summary>
      /// Initializes the module with a default database
      /// </summary>
      /// <param name="databaseURL">the database that will hold the information</param>
      public virtual void
      Initialize(string databaseURL)
      { _url = databaseURL; }

      /// <summary>
      /// Resets the connection of the database
      /// </summary>
      public virtual void
      Reset()
      {}

      /// <summary>
      /// Sends the information stored in the queue in a big batch so there is no
      /// lagging problems of any kind.
      /// When the queue is sent, the RequestSent Event will be triggered.
      /// When the queue returns, the RequestComplete Event will be triggered.
      /// </summary>
      public virtual void
      SendQueue()
      {}

      /// <summary>
      /// Forces the queue to be sent, regardless of the time reaching maxTime or
      /// time reaching maxTime
      /// </summary>
      public virtual void
      ForceQueue()
      {}

      /// <summary>
      /// The event sent when the request is uploading
      /// </summary>
      public virtual void
      OnRequestSent()
      {
        RequestSentEvent.Invoke();
      }

      /// <summary>
      /// The event sent when the request is completed and recovered a response
      /// </summary>
      public virtual void
      OnRequestComplete(AnalyticsResponse response)
      {
        RequestCompleteEvent.Invoke(response);
      }

      /// <summary>
      /// Sends a single Event to the database
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      public virtual void
      SendDesignEvent(string table,
                      string eventID)
      {}

      /// <summary>
      /// Sends a single Event to the database
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="floatValue">the float value to save</param>
      public virtual void
      SendDesignEvent(string table,
                      string eventID,
                      float floatValue)
      {}

      /// <summary>
      /// Sends a single Event to the database
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="stringValue">The string value to save</param>
      public virtual void
      SendDesignEvent(string table,
                      string eventID,
                      string stringValue)
      {}

      /// <summary>
      /// Sends a single Event to the database
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="floatValue">the float value to save</param>
      /// <param name="stringValue">the string value to save</param>
      public virtual void
      SendDesignEvent(string table,
                      string eventID,
                      float floatValue,
                      string stringValue)
      {}

      /// <summary>
      /// Adds an event to the Queue, when time runs out or queue gets to max size
      /// It will be sent as bulk along other design events
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      public virtual void
      QueueDesignEvent(string table,
                       string eventID)
      {}

      /// <summary>
      /// Adds an event to the Queue, when time runs out or queue gets to max size
      /// It will be sent as bulk along other design events
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="floatValue">the float value to save</param>
      public virtual void
      QueueDesignEvent(string table,
                       string eventID,
                       float floatValue)
      {}

      /// <summary>
      /// Adds an event to the Queue, when time runs out or queue gets to max size
      /// It will be sent as bulk along other design events
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="stringValue">the string value to save</param>
      public virtual void
      QueueDesignEvent(string table,
                       string eventID,
                       string stringValue)
      {}

      /// <summary>
      /// Adds an event to the Queue, when time runs out or queue gets to max size
      /// It will be sent as bulk along other design events
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="floatValue">the float value to save</param>
      /// <param name="stringValue">the string value to save</param>
      public virtual void
      QueueDesignEvent(string table,
                       string eventID,
                       float floatValue,
                       string stringValue)
      {}

      /// <summary>
      /// Sends an error type event, useful for unexpected or unwanted events
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="severity">level of severity</param>
      /// <param name="message">The message that informs what happened</param>
      public virtual void
      QueueErrorEvent(string table,
                      eErrorLevel severity,
                      string message)
      {}

      /// <summary>
      /// Queries all data available with the given parameters
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <returns>a JSON with all results matching the values</returns>
      public virtual JSON
      QueryData(string table, string eventID)
      {
        return null;
      }

      /// <summary>
      /// Queries all data available with the given parameters
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="floatValue">the float value to search</param>
      /// <returns>a JSON with all results matching the values</returns>
      public virtual JSON
      QueryData(string table, string eventID, float floatValue)
      {
        return null;
      }

      /// <summary>
      /// Queries all data available with the given parameters
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="stringValue">the string value to search</param>
      /// <returns>a JSON with all results matching the values</returns>
      public virtual JSON
      QueryData(string table, string eventID, string stringValue)
      {
        return null;
      }

      /// <summary>
      /// Queries all data available with the given parameters
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="eventID">the id to identify the data</param>
      /// <param name="floatValue">the float value to search</param>
      /// <param name="stringValue">the string value to search</param>
      /// <returns>a JSON with all results matching the values</returns>
      public virtual JSON
      QueryData(string table, string eventID, float floatValue, string stringValue)
      {
        return null;
      }

      /// <summary>
      /// Queries all data available with the given parameters
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="sessionID">the session ID</param>
      /// <returns>a JSON with all results matching the values</returns>
      public virtual JSON
      QueryData(string table, uint sessionID)
      {
        return null;
      }

      /// <summary>
      /// Queries all data available with the given parameters
      /// </summary>
      /// <param name="table">table/doc/subdatabase</param>
      /// <param name="sessionID">the session ID</param>
      /// <param name="eventID"></param>
      /// <returns>a JSON with all results matching the values</returns>
      public virtual JSON
      QueryData(string table, uint sessionID, string eventID)
      {
        return null;
      }

      /// <summary>
      /// Pings the database
      /// </summary>
      public virtual void 
      Ping()
      {}

      #endregion

    }

  }
  
}