using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leguar.TotalJSON;

namespace N4M2Q
{
  namespace Analytics
  {
    public class AnalyticsES : Analytics
    {


      public override void
      Initialize(string databaseURL)
      {
        base.Initialize(databaseURL);
      }

      public override JSON
      CreateJSONHeader(string table)
      {
        JSON doc = new JSON();
        doc.Add("index", database);
        doc.Add("type", table);
        return doc;
      }
    }
  }
}
