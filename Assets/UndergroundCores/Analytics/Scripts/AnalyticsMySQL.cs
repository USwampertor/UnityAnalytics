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
    public class AnalyticsMySQL : Analytics
    {
      // Private members
      #region PrivateMembers

      private MySqlConnection _connection;

      private MySqlCommand _command;
      
      private MySqlDataReader _dataReader;
      
      private string _connectionString;
      
      private Dictionary<string, Dictionary<string, List<string>>> _dataMap;
      
      private uint _dataCounter = 0;

      private const string _defaultColumns = "TIMESTAMP," +
                                              "SESSION_ID," +
                                              "USER_ID," +
                                              "SESSION_TAG," +
                                              "USERNAME," +
                                              "MAIL," +
                                              "EVENT_ID";

      private const string _allColumns = "TIMESTAMP," +
                                          " SESSION_ID," +
                                          " USER_ID," +
                                          " SESSION_TAG," +
                                          " USERNAME," +
                                          " MAIL," +
                                          " EVENT_ID," +
                                          " FLOATVALUE," +
                                          " STRINGVALUE";

      #endregion

      // Monobehavior Methods
      #region MonobehaviorMethods

      private void Update()
      {
        _internalTime += Time.deltaTime;
        SendQueue();
      }

      private void
      OnDestroy()
      {
        if (_connection != null)
        {
          if (_connection.State != System.Data.ConnectionState.Closed)
          {
            _connection.Close();
          }
        }
      }

      #endregion

      // Methods
      #region Methods

      private void
      ReadResponse(MySqlDataReader response)
      {
        MySqlDataReader reader = response;
        while (reader.Read())
        {
          Debug.Log($"{reader[0]}--{reader[1]}");
        }
        reader.Close();

      }

      private string ToRow(JSON doc)
      {
        string output = null;



        output = $" '{doc.GetString("timestamp")}'," +
                 $" '{doc.GetJNumber("session_id").AsUInt()}'," +
                 $" '{doc.GetJNumber("user_id").AsUInt()}'," +
                 $" '{doc.GetString("session_tag")}'," +
                 $" '{doc.GetString("username")}'," +
                 $" '{doc.GetString("mail")}'," +
                 $" '{doc.GetString("event_id")}'," +

                 (doc.Get("floatvalue").GetType() == typeof(JNull) ?
                 $"NULL," : $"{doc.GetFloat("floatvalue")},") +


                (doc.Get("stringvalue").GetType() == typeof(JNull) ?
                 $"NULL" : $"'{doc.GetString("stringvalue")}'");


        return output;
      }

      private JSON
      QueryResponse(MySqlDataReader response)
      {
        JSON output = new JSON();
        MySqlDataReader reader = response;
        int i = 0;
        while (reader.Read())
        {

          JSON doc = new JSON();

          doc.Add("timestamp", reader[1] != System.DBNull.Value ? reader[1] : null);
          doc.Add("session_id", reader[2] != System.DBNull.Value ? reader[2] : null);
          doc.Add("user_id", reader[3] != System.DBNull.Value ? reader[3] : null);
          doc.Add("session_tag", reader[4] != System.DBNull.Value ? reader[4] : null);
          doc.Add("username", reader[5] != System.DBNull.Value ? reader[5] : null);
          doc.Add("mail", reader[6] != System.DBNull.Value ? reader[6] : null);
          doc.Add("event_id", reader[7] != System.DBNull.Value ? reader[7] : null);
          doc.Add("floatvalue", reader[8] != System.DBNull.Value ? reader[8] : null);
          doc.Add("stringvalue", reader[9] != System.DBNull.Value ? reader[9] : null);

          output.Add($"value_{i}", doc);
          ++i;
        }
        reader.Close();

        return output;
      }

      private void
      SetTable(string table)
      {
        if (_connection != null)
        {
          if (_connection.State != System.Data.ConnectionState.Closed)
          {

            string sqlString;

            sqlString = $"CREATE TABLE IF NOT EXISTS {table} " +
                        $"(ID INT UNSIGNED AUTO_INCREMENT NOT NULL," +
                        $" TIMESTAMP    VARCHAR(255) NOT NULL," +
                        $" SESSION_ID   INT UNSIGNED," +
                        $" USER_ID      INT UNSIGNED," +
                        $" SESSION_TAG  VARCHAR(255)," +
                        $" USERNAME     VARCHAR(255)," +
                        $" MAIL         VARCHAR(255)," +
                        $" EVENT_ID     VARCHAR(255)," +
                        $" FLOATVALUE   FLOAT," +
                        $" STRINGVALUE  VARCHAR(255)," +
                        $" PRIMARY KEY(ID) );";
            _command = new MySqlCommand(sqlString, _connection);
            ReadResponse(_command.ExecuteReader());
            if (!_dataMap[database].ContainsKey(table)) { _dataMap[database].Add(table, new List<string>()); }
          }
        }
      }
      #endregion

      // Override Methods
      #region OverrideMethods

      public override JSON
      GetTables()
      {
        string sqlString = $"SHOW TABLES";

        _command = new MySqlCommand(sqlString, _connection);

        MySqlDataReader reader = _command.ExecuteReader();

        JSON tables = new JSON();

        while (reader.Read())
        {
          for (int i = 0; i < reader.FieldCount; ++i)
          {
            tables.Add(reader.GetString(i), reader.GetString(i));
          }
        }
        reader.Close();


        return tables;
      }


      public override void
      SetDatabase(string newDatabase)
      {
        base.SetDatabase(newDatabase);
        if (_connection != null)
        {
          if (_connection.State != System.Data.ConnectionState.Closed)
          {
            string sqlString = $"CREATE DATABASE IF NOT EXISTS {database};";
            _command = new MySqlCommand(sqlString, _connection);
            ReadResponse(_command.ExecuteReader());
            _connection.ChangeDatabase(database);
            if (!_dataMap.ContainsKey(database)) { _dataMap.Add(database, new Dictionary<string, List<string>>()); }

            /* This is to test
            sqlString = $"DROP TABLE IF EXISTS testTable;";
            _command = new MySqlCommand(sqlString, _connection);
            ReadResponse(_command.ExecuteReader());

            //*/
          }
        }

      }


      public override void
      Initialize(string databaseURL)
      {
        base.Initialize(databaseURL);
        _dataMap = new Dictionary<string, Dictionary<string, List<string>>>();
        _connectionString = $"Server={url};port=3306;User={_urlUsername};Password={_urlPassword};Pooling=False";
        bool ping = false;
        try
        {
          _connection = new MySqlConnection(_connectionString);
          _connection.Open();
          ping = _connection.Ping();
        }
        catch (System.Exception e)
        {
          Debug.Log($"Exception found: {e}");
        }
        Debug.Log($"Ping returned: {ping} \n" + (ping ? "Connection succesful" : "Connection failed"));

      }

      public override void
      Reset()
      {
        if (_connection != null)
        {
          if (_connection.State != System.Data.ConnectionState.Closed)
          {
            _connection.Close();
          }
        }
        _connectionString = $"Server={url};port=3306;Database={database};User={_urlUsername};Password={_urlPassword};Pooling=False";

        bool ping = false;
        try
        {
          _connection = new MySqlConnection(_connectionString);
          _connection.Open();
          ping = _connection.Ping();
        }
        catch (System.Exception e)
        {
          Debug.Log($"Exception found: {e}");
        }
        Debug.Log($"Ping returned: {ping} \n" + (ping ? "Connection succesful" : "Connection failed"));
      }

      public override JSON
      CreateJSONHeader(string table)
      {
        JSON doc = new JSON();
        doc.Add("database", database);
        doc.Add("table", table);
        return doc;
      }

      public override JSON
      CreateJSONBody(string eventID)
      {
        JSON doc = new JSON();
        doc.Add("timestamp", System.DateTime.UtcNow.ToString("dd/MM/yyyy HH::mm:ss"));
        doc.Add("session_id", sessionID);
        doc.Add("user_id", userID);
        doc.Add("session_tag", sessionTag);
        doc.Add("username", username);
        doc.Add("mail", mail);
        doc.Add("event_id", eventID);
        doc.Add("floatvalue", null);
        doc.Add("stringvalue", null);
        return doc;
      }


      public override void
      SendQueue()
      {
        if (_dataCounter >= maxQueueSize || _internalTime >= maxQueueTime)
        {

          foreach (var d in _dataMap)
          {
            if (database != d.Key) { SetDatabase(d.Key); }
            foreach (var t in d.Value)
            {
              SetTable(t.Key);
              string queue = "";

              foreach (var entry in t.Value)
              {
                queue += $"({entry}),";
              }
              queue = queue.Remove(queue.Length - 1);

              string sqlString = $"INSERT INTO {t.Key} " +
                           $"({_allColumns})" +
                           $" VALUES" +
                           $"{queue}";

              _command = new MySqlCommand(sqlString, _connection);
              ReadResponse(_command.ExecuteReader());
            }
          }




          if (_internalTime >= maxQueueTime) { _internalTime = 0; }
        }
      }

      public override void
      ForceQueue()
      {
        foreach (var d in _dataMap)
        {
          if (database != d.Key) { SetDatabase(d.Key); }
          foreach (var t in d.Value)
          {
            SetTable(t.Key);
            string queue = "";

            foreach (var entry in t.Value)
            {
              queue += $"({entry}),";
            }
            queue = queue.Remove(queue.Length - 1);

            string sqlString = $"INSERT INTO {t.Key} " +
                         $"({_allColumns})" +
                         $" VALUES" +
                         $"{queue}";

            _command = new MySqlCommand(sqlString, _connection);
            ReadResponse(_command.ExecuteReader());
          }
        }
      }


      private void
      AddToQueue(string table, string json)
      {
        _dataMap[database][table].Add(json);
        ++_dataCounter;
      }

      public override void
      SendDesignEvent(string table,
                      string eventID)
      {

        SetTable(table);

        JSON doc = CreateJSONBody(eventID);

        string sqlString = $"INSERT INTO {table} " +
                           $"({_allColumns})" +
                           $" VALUES" +
                           $"({ToRow(doc)})";

        _command = new MySqlCommand(sqlString, _connection);
        ReadResponse(_command.ExecuteReader());

      }

      public override void
      SendDesignEvent(string table,
                      string eventID,
                      float floatValue)
      {
        SetTable(table);

        JSON doc = CreateJSONBody(eventID);
        doc.AddOrReplace("floatvalue", floatValue);

        string sqlString = $"INSERT INTO {table} " +
                           $"({_allColumns})" +
                           $" VALUES" +
                           $"({ToRow(doc)})";

        _command = new MySqlCommand(sqlString, _connection);
        ReadResponse(_command.ExecuteReader());
      }

      public override void
      SendDesignEvent(string table,
                      string eventID,
                      string stringValue)
      {
        SetTable(table);

        JSON doc = CreateJSONBody(eventID);
        doc.AddOrReplace("stringvalue", stringValue);

        string sqlString = $"INSERT INTO {table} " +
                           $"({_allColumns})" +
                           $" VALUES" +
                           $"({ToRow(doc)})";

        _command = new MySqlCommand(sqlString, _connection);
        ReadResponse(_command.ExecuteReader());
      }

      public override void
      SendDesignEvent(string table,
                      string eventID,
                      float floatValue,
                      string stringValue)
      {

        SetTable(table);

        JSON doc = CreateJSONBody(eventID);
        doc.AddOrReplace("floatvalue", floatValue);
        doc.AddOrReplace("stringvalue", stringValue);

        string sqlString = $"INSERT INTO {table} " +
                           $"({_allColumns})" +
                           $" VALUES" +
                           $"({ToRow(doc)})";

        _command = new MySqlCommand(sqlString, _connection);
        ReadResponse(_command.ExecuteReader());
      }

      public override void
      QueueDesignEvent(string table,
                       string eventID)
      {
        SetTable(table);
        JSON doc = CreateJSONBody(eventID);
        AddToQueue(table, ToRow(doc));
      }

      public override void
      QueueDesignEvent(string table,
                       string eventID,
                       float floatValue)
      {
        SetTable(table);
        JSON doc = CreateJSONBody(eventID);
        doc.AddOrReplace("floatvalue", floatValue);
        AddToQueue(table, ToRow(doc));
      }

      public override void
      QueueDesignEvent(string table,
                       string eventID,
                       string stringValue)
      {
        SetTable(table);
        JSON doc = CreateJSONBody(eventID);
        doc.AddOrReplace("stringvalue", stringValue);
        AddToQueue(table, ToRow(doc));
      }

      public override void
      QueueDesignEvent(string table,
                       string eventID,
                       float floatValue,
                       string stringValue)
      {
        SetTable(table);
        JSON doc = CreateJSONBody(eventID);
        doc.AddOrReplace("floatvalue", floatValue);
        doc.AddOrReplace("stringvalue", stringValue);
        AddToQueue(table, ToRow(doc));
      }

      public override JSON
      QueryData(string table, string eventID)
      {

        string sql = $"SELECT * FROM {table} WHERE EVENT_ID = '{eventID}'";
        _command = new MySqlCommand(sql, _connection);
        return QueryResponse(_command.ExecuteReader());
      }

      public override JSON
      QueryData(string table, string eventID, float floatValue)
      {
        string sql = $"SELECT * FROM {table} WHERE " +
                     $"EVENT_ID = '{eventID}' AND " +
                     $"FLOATVALUE = {floatValue}";
        _command = new MySqlCommand(sql, _connection);
        return QueryResponse(_command.ExecuteReader());
      }

      public override JSON
      QueryData(string table, string eventID, string stringValue)
      {
        string sql = $"SELECT * FROM {table} WHERE " +
                     $"EVENT_ID = '{eventID}' AND " +
                     $"STRINGVALUE = '{stringValue}'";
        _command = new MySqlCommand(sql, _connection);
        return QueryResponse(_command.ExecuteReader());
      }

      public override JSON
      QueryData(string table, string eventID, float floatValue, string stringValue)
      {
        string sql = $"SELECT * FROM {table} WHERE " +
                     $"EVENT_ID = '{eventID}' AND " +
                     $"FLOATVALUE = {floatValue} AND " +
                     $"STRINGVALUE = '{stringValue}'";
        _command = new MySqlCommand(sql, _connection);
        return QueryResponse(_command.ExecuteReader());
      }

      public override JSON
      QueryData(string table, uint sessionID)
      {
        string sql = $"SELECT * FROM {table} WHERE " +
                     $"SESSION_ID = {sessionID}";
        _command = new MySqlCommand(sql, _connection);
        return QueryResponse(_command.ExecuteReader());
      }

      public override JSON
      QueryData(string table, uint sessionID, string eventID)
      {
        string sql = $"SELECT * FROM {table} WHERE " +
                     $"EVENT_ID = '{eventID}' AND " +
                     $"SESSION_ID = {sessionID} ";
        _command = new MySqlCommand(sql, _connection);
        return QueryResponse(_command.ExecuteReader());
      }

      #endregion
      
    }
  }
}
