using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Text;
using System;
using System.Diagnostics.Eventing.Reader;
using Sitecore.Pipelines.GetRenderingDatasource;
using Sitecore.Data;
using Sitecore.Web.UI.XamlSharp.Xaml;

namespace Sitecore.Support.Pipelines.GetRenderingDatasource
{
  /// <summary>Defines the datasource location root item</summary>
  public class GetDatasourceLocation
  {
    /// <summary>Runs the processor.</summary>
    /// <param name="args">The arguments.</param>
    public void Process(GetRenderingDatasourceArgs args)
    {
      Assert.IsNotNull((object)args, nameof(args));
      foreach (string str in new ListString(args.RenderingItem["Datasource Location"]))
      {
        if (str.StartsWith("query:", StringComparison.InvariantCulture))
        {
          this.AddRootsFromQuery(str.Substring("query:".Length), args);
        }
        else
        {
          string path = str;
          Item obj = null;
          if (str.StartsWith("./", StringComparison.InvariantCulture) && !string.IsNullOrEmpty(args.ContextItemPath))
          {
            path = args.ContextItemPath + str.Remove(0, 1);
            #region FIX
            if (args.CustomData["itemID"] != null)
            {
              string itemId = args.CustomData["itemID"].ToString();
              Item contextItem = args.ContentDatabase.GetItem(new ID(itemId));
              if (contextItem != null && contextItem.Children != null && contextItem.Children.InnerChildren != null)
              {
                foreach (var innerChild in contextItem.Children.InnerChildren)
                {
                  // sstr.Remove(0,2) -> remove "./" symbols from "str"
                  if (innerChild.Name.ToString().ToLower().Equals(str.Remove(0, 2).ToString().ToLower()))
                  {
                    obj = args.ContentDatabase.GetItem(innerChild.ID);
                  }
                }
              }
            }
            else
            {
              #endregion
              obj = args.ContentDatabase.GetItem(path);
            }
          }
          if (obj != null)
            args.DatasourceRoots.Add(obj);
        }
      }
    }

    /// <summary>Find and add roots from passed query.</summary>
    /// <param name="query">The query.</param>
    /// <param name="args">The arguments of processor.</param>
    protected virtual void AddRootsFromQuery(string query, GetRenderingDatasourceArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      Assert.ArgumentNotNullOrEmpty(query, nameof(query));
      Item[] objArray = (Item[])null;
      if (query.StartsWith("./", StringComparison.InvariantCulture) && !string.IsNullOrEmpty(args.ContextItemPath))
      {
        Item obj = args.ContentDatabase.GetItem(args.ContextItemPath);
        if (obj != null)
          objArray = obj.Axes.SelectItems(query);
      }
      else
        objArray = args.ContentDatabase.SelectItems(query);
      if (objArray == null)
        return;
      foreach (Item obj in objArray)
        args.DatasourceRoots.Add(obj);
    }
  }
}
