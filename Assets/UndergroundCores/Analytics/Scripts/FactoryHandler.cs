using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SWT
{
  public class FactoryHandler
  {
    /// <summary>
    /// By code you can create an object of an inherited class of Analytics.
    /// IF C# Would let you have members inside Interfaces, this would have
    /// been event better but sometimes you gotta work around.
    /// </summary>
    /// <typeparam name="T">Analytics.Analytics inherited class</typeparam>
    public static void CreateAnalytics<T>() where T : Analytics.Analytics
    {
      // Create GameObject and assign the type of Analytics child to use
      GameObject module = new GameObject("Analytics");
      Analytics.Analytics.StartUp(module.AddComponent<T>());
    }
  }
}