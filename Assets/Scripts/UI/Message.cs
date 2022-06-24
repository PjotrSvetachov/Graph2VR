using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message : MonoBehaviour
{
  public GameObject messagePrefab;
  public Transform messageContainer;
  public Transform panel;

  public void AddMessage(string message)
  {
    panel.gameObject.SetActive(true);
    GameObject clone = Instantiate(messagePrefab, messageContainer);
    clone.transform.localScale = Vector3.one;
    clone.transform.Find("Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().text = message;
  }

  static public Message instance;
  private void Awake()
  {
    instance = this;
  }

}
