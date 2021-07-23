using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SWT.Analytics;

public enum ESTestType : int
{
  [Tooltip("No tests will run")]
  NONE = 0,
  [Tooltip("This tests a batch of test data ")]
  TESTA,
  TESTB,
  TESTC,
  ALL
}

public class ESTest : MonoBehaviour
{
  // Public properties
  #region Properties

  public ESTestType testToRun;

  #endregion

  // Mono behavior Methods
  #region MonobehaviorMethods

  // Start is called before the first frame update
  void Start()
  {
    switch(testToRun)
    {
      case ESTestType.TESTA:
        StartCoroutine(TestA());
        break;
      case ESTestType.TESTB:
        StartCoroutine(TestB());
        break;
      case ESTestType.TESTC:
        StartCoroutine(TestC());
        break;
      case ESTestType.ALL:
        StartCoroutine(TestA());
        StartCoroutine(TestB());
        StartCoroutine(TestC());
        break;
      default:
        Analytics.Get.Ping();
        break;
    }
  }

  // Update is called once per frame
  void Update()
  {
    
  }

  #endregion

  // Methods
  #region Methods

  private IEnumerator TestA()
  {
    Analytics.Get.RequestSentEvent.AddListener(TestFinishMessage);
    Analytics.Get.RequestCompleteEvent.AddListener(TestCompleteMessage);

    yield return new WaitForSeconds(1);
    RunBatch("test_event_a_1", Analytics.Get.maxQueueSize);
    Debug.Log("Sending first Batch");
    yield return new WaitForSeconds(5);
    RunBatch("test_event_a_2", Analytics.Get.maxQueueSize);
    Debug.Log("Sending second Batch");
    yield return new WaitForSeconds(5);
    RunBatch("test_event_a_3", Analytics.Get.maxQueueSize);
    Debug.Log("Sending second Batch");
    yield return new WaitForSeconds(5);

    Analytics.Get.RequestSentEvent.RemoveListener(TestFinishMessage);
    Analytics.Get.RequestCompleteEvent.AddListener(TestCompleteMessage);

    yield return null;

  }

  private IEnumerator TestB()
  {
    Analytics.Get.RequestSentEvent.AddListener(TestFinishMessage);
    Analytics.Get.RequestCompleteEvent.AddListener(TestCompleteMessage);
    RunBatch("test_event_b_1", Analytics.Get.maxQueueSize);
    RunBatch("test_event_b_2", Analytics.Get.maxQueueSize);
    Debug.Log("Forcing another forced update to check for locks and threading");

    Analytics.Get.QueueDesignEvent(string.Empty, "test_event_b_2", $"Forced Event 1");
    Analytics.Get.QueueDesignEvent(string.Empty, "test_event_b_2", $"Forced Event 2");
    Analytics.Get.ForceQueue();
    yield return new WaitForSeconds(5);

    Analytics.Get.RequestSentEvent.RemoveListener(TestFinishMessage);
    Analytics.Get.RequestCompleteEvent.AddListener(TestCompleteMessage);

    yield return null;
  }

  private IEnumerator TestC()
  {
    yield return null;
  }

  void TestFinishMessage()
  {
    Debug.Log("Finished sending...");
  }

  void TestCompleteMessage(AnalyticsResponse response)
  {
    Debug.Log($"Response returned with message: {response.message}");
  }

  void RunBatch(string eventID, uint size)
  {
    for (int i = 0; i < size; ++i)
    {
      Analytics.Get.QueueDesignEvent(string.Empty, $"{eventID}", $"Design Event {i}");
    }
  }

  #endregion

}
