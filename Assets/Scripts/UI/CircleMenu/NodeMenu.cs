﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;

public class NodeMenu : MonoBehaviour
{
    private CircleMenu cm = null;
    public bool isOutgoingLink = true;
    private Dictionary<string, System.Tuple<string, int>> set;
    private Node node = null;
    public GameObject controlerModel;
    public SteamVR_Action_Boolean clickAction = null;

    public void Start()
    {
        cm = GetComponent<CircleMenu>();
    }

    public void GetPredicats()
    {
        if (node != null) {
            if (isOutgoingLink) {
                set = Graph.instance.GetOutgoingPredicats(node.GetURIAsString());
            } else {
                set = Graph.instance.GetIncomingPredicats(node.GetURIAsString());
            }
        }
    }

    public void Update()
    {
        if (clickAction.GetStateDown(SteamVR_Input_Sources.LeftHand) == true) {
            Close();
        }
    }

    public void Populate(Object input)
    {
        node = input as Node;
        GetPredicats();
        if (set != null) {
            // Get uri of selected node
            // Get list of predicats 
            // call GetOutgoingPredicats()

            Close();
            controlerModel.SetActive(false);

            if (isOutgoingLink) {
                cm.AddButton("List incoming predicats", Color.blue / 2, () => {
                    isOutgoingLink = false;
                    Populate(input); 
                });
            } else {
                cm.AddButton("List outgoing predicats", Color.blue / 2, () => {
                    isOutgoingLink = true;
                    Populate(input);
                });
            }

            foreach (KeyValuePair<string, System.Tuple<string, int>> item in set) {
                //Debug.Log("k: " + item.Key + " v1: " + item.Value.Item1 + " v2: " + item.Value.Item2);
                Color color = Color.gray;
                string label = item.Value.Item1;
                if (label == "") {
                    label = item.Key;
                    color = Color.gray * 0.75f;
                }
                // TODO: add qname als alt.

                cm.AddButton(label, color, () => {
                    Graph.instance.ExpandGraph(node, item.Key, isOutgoingLink);
                    Close();
                }, item.Value.Item2);
            }
            cm.AddButton("Convert to Variable", Color.blue / 2, () => { });
            cm.AddButton("Convert to Constant", Color.cyan / 2, () => { });
            cm.AddButton("Collapse", Color.red / 2, () => {
                // Something smart
                Graph.instance.CollapseGraph(node);
            });
            cm.ReBuild();
        }
    }

    public void Close()
    {
        if (cm != null) {
            cm.Close();
            controlerModel.SetActive(true);
        }
    }
}
