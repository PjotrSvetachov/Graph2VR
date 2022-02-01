﻿using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;

public class FruchtermanReingold : BaseLayoutAlgorithm
{
  // variables for the Fruchterman-Reingold algorithm
  public float Temperature = 0;

  public override void CalculateLayout()
  {
    Temperature = 0.05f;
  }

  public override void Stop()
  {
    Temperature = 0f;
  }

  void Update()
  {
    if (Temperature > 0.01f) {
      FruchtermanReingoldIteration();
    }
  }

  public float C_CONSTANT = 1.0f;
  public float AREA_CONSTANT = 1.0f;
  //Function for the Fruchterman-Reingold algorithm
  private float Fa(float x)
  {
    float k = (C_CONSTANT * Mathf.Sqrt(AREA_CONSTANT / graph.nodeList.Count));
    return 0.001f * (x * x * x * x) / k;
  }

  //Function for the Fruchterman-Reingold algorithm
  private float Fr(float x)
  {
    float k = (C_CONSTANT * Mathf.Sqrt(AREA_CONSTANT / graph.nodeList.Count));
    return 0.001f * (k * k) / (x * x * x);
  }

  struct CalculateForcesJob : IJobParallelFor
  {
    [ReadOnly]
    public NativeArray<Vector3> NodePositions;

    public NativeArray<Vector3> NodeDisplacements;

    public const float C_CONSTANT = 1.0f;
    public const float AREA_CONSTANT = 1.0f;
    //Function for the Fruchterman-Reingold algorithm
    private float Fa(float x)
    {
      float k = (C_CONSTANT * Mathf.Sqrt(AREA_CONSTANT / NodePositions.Length));
      return 0.001f * (x * x * x * x) / k;
    }

    //Function for the Fruchterman-Reingold algorithm
    private float Fr(float x)
    {
      float k = (C_CONSTANT * Mathf.Sqrt(AREA_CONSTANT / NodePositions.Length));
      return 0.001f * (k * k) / (x * x * x);
    }

    public void Execute(int index)
    {
      Vector3 displacement = Vector3.zero;
      Vector3 position = NodePositions[index];
      for(int i = 0; i < NodePositions.Length; i++)
      {
        if(i != index)
        {
          Vector3 delta = position - NodePositions[i];
          float FrForce = Fr(delta.magnitude);
          displacement += delta.normalized * FrForce;
        }
      }
      NodeDisplacements[index] = displacement;
    }
  }

  // Do one iteration fo the Fruchterman-Reingold algorithm
  // We only use localpositions so the algorithm stays stable when zooming in/out
  public void FruchtermanReingoldIteration()
  {
    // calculate repulsive forces
    bool useSingleThread = false;
    if (useSingleThread)
    {
      foreach (Node node in graph.nodeList) {
        node.displacement = Vector3.zero;
        foreach (Node neightbor in graph.nodeList) {
          if (node != neightbor) {
            Vector3 delta = node.transform.localPosition - neightbor.transform.localPosition;
            float FrForce = Fr(delta.magnitude);
            node.displacement += delta.normalized * FrForce;
          }
        }
      }
    }
    else
    {
      NativeArray<Vector3> NodePositions = new NativeArray<Vector3>(graph.nodeList.Count, Allocator.TempJob);
      NativeArray<Vector3> NodeDisplacements = new NativeArray<Vector3>(graph.nodeList.Count, Allocator.TempJob);

      // Copy the node positions to the arrays
      int i = 0;
      foreach (Node node in graph.nodeList)
      {
        NodePositions[i] = node.transform.localPosition;
        i++;
      }

      // Fire the jobs to calculate forces
      CalculateForcesJob forcesJob = new CalculateForcesJob()
      {
        NodePositions = NodePositions,
        NodeDisplacements = NodeDisplacements
      };

      JobHandle forcesJobHandle = forcesJob.Schedule(NodeDisplacements.Length, 16);
      forcesJobHandle.Complete();

      // Copy the new displacement back into the nodes
      i = 0;
      foreach (Node node in graph.nodeList)
      {
        node.displacement = forcesJob.NodeDisplacements[i];
        i++;
      }


      NodePositions.Dispose();
      NodeDisplacements.Dispose();
    }

    // calculate attractive forces
    foreach (Edge edge in graph.edgeList) {
      if (edge.displayObject != null && edge.displaySubject != null) {
        Vector3 delta = edge.displayObject.transform.localPosition - edge.displaySubject.transform.localPosition;
        float FaForce = Fa(delta.magnitude);
        Vector3 normal = delta.normalized;
        edge.displayObject.displacement -= normal * FaForce;
        edge.displaySubject.displacement += normal * FaForce;
      }
    }

    // Reposition the nodes, taking ionto account the temperature
    float TotalDisplacement = 0.0f;
    foreach (Node node in graph.nodeList) {
      if (node != null) {
        float DisplacementMagitude = node.displacement.magnitude;
        TotalDisplacement = Mathf.Max(DisplacementMagitude, TotalDisplacement);
        node.transform.localPosition += (node.displacement / DisplacementMagitude) * Mathf.Min(DisplacementMagitude, Temperature);
      }
    }

    // reduce the temperature
    Temperature -= 0.005f * Time.deltaTime;
  }
}
