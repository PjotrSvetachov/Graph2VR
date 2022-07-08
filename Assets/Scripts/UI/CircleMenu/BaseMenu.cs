using System.Collections.Generic;
using UnityEngine;

public class BaseMenu : MonoBehaviour
{
   public Graph graph;
   protected CircleMenu cm = null;
   public GameObject controlerModel;
   public string subMenu = "";
   public GameObject limitSlider;
   protected Node node = null;
   protected Edge edge = null;

   protected enum PopulateMenuState { unloaded, loading, loaded };
   protected PopulateMenuState populateMenuState = PopulateMenuState.unloaded;

   public void Start()
   {
      cm = GetComponent<CircleMenu>();
   }

   public virtual void Update()
   {
      if (ControlerInput.instance.triggerLeft)
      {
         Close();
      }
   }

   public void Close()
   {
      if (node != null) node.IsActiveInMenu = false;
      if (edge != null) edge.IsActiveInMenu = false;
      populateMenuState = PopulateMenuState.unloaded;
      limitSlider.SetActive(false);
      node = null;
      subMenu = "";
      edge = null;
      graph = null;
      if (cm != null)
      {
         cm.Close();
         controlerModel.SetActive(true);
      }
   }

   protected bool GraphHasSelectedVariable()
   {
      return graph.selection.Find((edge) => edge.IsVariable || edge.displayObject.IsVariable || edge.displaySubject.IsVariable) != null;
   }

   protected HashSet<string> SelectedVariableNames()
   {
      HashSet<string> variables = new HashSet<string>();
      List<Edge> selected = graph.selection.FindAll((edge) => edge.IsVariable || edge.displayObject.IsVariable || edge.displaySubject.IsVariable);
      foreach (Edge edge in selected)
      {
         if (edge.IsVariable) variables.Add(edge.variableName);
         if (edge.displayObject.IsVariable) variables.Add(edge.displayObject.label);
         if (edge.displaySubject.IsVariable) variables.Add(edge.displaySubject.label);
      }
      return variables;
   }

   public void PopulateGraphMenu()
   {
      cm.AddButton("Layout: Force Directed 3D", Color.green / 2, () =>
      {
         graph.SwitchLayout<FruchtermanReingold>();
         graph.layout.CalculateLayout();
      });

      cm.AddButton("Layout: Force Directed 2D", Color.green / 2, () =>
      {
         graph.SwitchLayout<SpatialGrid2D>();
         graph.layout.CalculateLayout();
      });


      cm.AddButton("Layout: Hierarchical View", Color.green / 2, () =>
      {
         graph.SwitchLayout<HierarchicalView>();
         graph.layout.CalculateLayout();
      });

      cm.AddButton("Layout: Class Hierarchy", Color.green / 2, () =>
      {
         graph.SwitchLayout<ClassHierarchy>();
         graph.layout.CalculateLayout();
      });


      cm.AddButton("Auto layout", Color.yellow / 2, () =>
      {
         graph.layout.CalculateLayout();
      });

      cm.AddButton("Close Graph", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
         graph.Remove();
         Close();
      });

      cm.AddButton("Save this Graph (experimental)", new Color(1, 0.5f, 0.5f) / 2, () =>
      {
         SaveLoad.Save(graph, "graph");
         Close();
      });

      if (graph.subGraphs.Count > 0)
      {
         cm.AddButton("Close all child graphs", new Color(1, 0.5f, 0.5f) / 2, () =>
         {
            graph.RemoveSubGraphs();
            Close();
         });
      }

      if (graph.parentGraph != null && graph.creationQuery != "")
      {
         cm.AddButton("Close sibling graphs", new Color(1, 0.5f, 0.5f) / 2, () =>
         {
            graph.RemoveGraphsOfSameQuery();
            Close();
         });
      }
   }
}