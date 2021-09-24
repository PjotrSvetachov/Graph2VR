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
        KeyboardHandler.instance.Close();
        node = input as Node;
        if (node.isVariable) {
            Close();
            controlerModel.SetActive(false);
            cm.AddButton("Convert to Constant", Color.blue / 2, () => {
                node.MakeConstant();
                Populate(input);
            });
            cm.AddButton("Rename", Color.red / 2, () => { KeyboardHandler.instance.Open(node); });
            cm.ReBuild();
        } else {
            GetPredicats();
            Close();
            controlerModel.SetActive(false);

            if (set != null) {
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
            }

            if (!node.isVariable) {
                cm.AddButton("Convert to Variable", Color.blue / 2, () => {
                    node.MakeVariable();
                    Populate(input);
                });
            }

            if (node.uri != "") {
                cm.AddButton("Collapse Incomming", new Color(1, 0.5f, 0.5f) / 2, () => {
                    Graph.instance.CollapseIncomingGraph(node);
                });
                cm.AddButton("Collapse Outgoing", new Color(1, 0.5f, 0.5f) / 2, () => {
                    Graph.instance.CollapseOutgoingGraph(node);
                });
                cm.AddButton("Collapse All", new Color(1, 0.5f, 0.5f) / 2, () => {
                    Graph.instance.CollapseGraph(node);
                });
            }

            cm.AddButton("Close", Color.red / 2, () => {
                Graph.instance.RemoveNode(node);
                Close();
            });

            cm.ReBuild();
        }
    }

    public void Close()
    {
        if (cm != null) {
            cm.Close();
            KeyboardHandler.instance.Close();
            controlerModel.SetActive(true);
        }
    }
}
