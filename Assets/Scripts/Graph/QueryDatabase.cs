using Dweiss;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS.RDF;
using VDS.RDF.Query;

public class QueryDatabase : MonoBehaviour
{
  SparqlRemoteEndpoint endPoint = null;
  IGraph internalEndPoint = null;
  public bool isRemote = true;

  private void Awake()
  {
    SetupSingelton();
    UseRemote();
  }

  public void UseRemote()
  {
    isRemote = true;
    CreateEndpoint();
  }

  public void UseInternalGraph(IGraph graph)
  {
    internalEndPoint = graph;
    isRemote = false;
  }

  public void QueryWithResultGraph(string query, GraphCallback callback)
  {
    if (isRemote)
    {
      endPoint.QueryWithResultGraph(query, callback, state: null);
    }
    else
    {
      object result = internalEndPoint.ExecuteQuery(query);
      callback(result as IGraph, null);
    }
  }

  public void QueryWithResultSet(string query, SparqlResultsCallback callback)
  {
    Debug.Log("QueryWithResultSet: " + isRemote);
    if (isRemote)
    {
      endPoint.QueryWithResultSet(query, callback, state: null);
    }
    else
    {
      Debug.Log(query);
      object result = internalEndPoint.ExecuteQuery(query);
      Debug.Log(result);
      Debug.Log((result as SparqlResultSet).Count);
      foreach (var r in result as SparqlResultSet)
      {
        Debug.Log(r.ToString());
      }
      callback(result as SparqlResultSet, null);
    }
  }

  private void CreateEndpoint()
  {
    endPoint = new SparqlRemoteEndpoint(new Uri(Settings.Instance.SparqlEndpoint), Settings.Instance.BaseURI);
  }

  #region  Singleton
  public static QueryDatabase _instance;
  public static QueryDatabase Instance { get { return _instance; } }
  private void SetupSingelton()
  {
    if (_instance != null)
    {
      Debug.LogError("Error in settings. Multiple singletons exists: " + _instance.name + " and now " + this.name);
    }
    else
    {
      _instance = this;
    }
  }
  #endregion
}
