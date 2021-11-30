﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerLine : MonoBehaviour
{
  public int maxDistance = 10;
  public LineRenderer line;
  public Transform endPoint;
  void Start()
  {
  }

  void Update()
  {
    RaycastHit hit;
    Vector3 endpoint = transform.position + (transform.forward * maxDistance);
    if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance)) {
      endpoint = hit.point;
    }
    line.SetPosition(0, transform.position);
    line.SetPosition(1, endpoint);
    endPoint.transform.position = endpoint;
  }
}