using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Pipelines.GetPlaceholderRenderings;
using Sitecore.Pipelines.GetRenderingDatasource;
using Sitecore.Shell.Applications.WebEdit;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
  [Serializable]
  public class AddRendering : WebEditCommand
  {
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull((object)context, nameof(context));
      NameValueCollection parameters;
      if (context.Items.Length < 1)
      {
        Item itemFromUrl = AddRendering.GetItemFromUrl();
        if (itemFromUrl == null)
          return;
        parameters = AddRendering.CreateParameters(new CommandContext(itemFromUrl)
        {
          Parameters = {
            context.Parameters
          }
        });
      }
      else
        parameters = AddRendering.CreateParameters(context);
      Context.ClientPage.Start((object)this, "Run", parameters);
    }

    protected static void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      if (args.IsPostBack)
      {
        if (!args.HasResult)
          return;
        if (!AddRendering.IsSelectDatasourceDialogPostBack(args))
        {
          string result;
          bool flag;
          if (args.Result.IndexOf(',') >= 0)
          {
            string[] strArray = args.Result.Split(',');
            result = strArray[0];
            flag = strArray[2] == "1";
          }
          else
          {
            result = args.Result;
            flag = false;
          }
          Item itemNotNull = Sitecore.Client.GetItemNotNull(result);
          GetRenderingDatasourceArgs renderingDatasourceArgs = new GetRenderingDatasourceArgs(itemNotNull)
          {
            ContextItemPath = args.Parameters["contextitempath"],
            ContentLanguage = Sitecore.Web.WebEditUtil.GetClientContentLanguage()
          };
          #region FIX
          // add item id to the CustomData
          renderingDatasourceArgs.CustomData.Add("itemID", args.Parameters["itemID"]);
          #endregion
          string str = itemNotNull.ID.ToShortID().ToString();
          if (!AddRendering.IsMorphRenderingsRequest(args))
            CorePipeline.Run("getRenderingDatasource", (PipelineArgs)renderingDatasourceArgs);
          if (!string.IsNullOrEmpty(renderingDatasourceArgs.DialogUrl) && !AddRendering.IsMorphRenderingsRequest(args))
          {
            args.IsPostBack = false;
            args.Parameters["SelectedRendering"] = str;
            args.Parameters["OpenProperties"] = flag.ToString().ToLowerInvariant();
            SheerResponse.ShowModalDialog(renderingDatasourceArgs.DialogUrl, "1200px", "700px", string.Empty, true);
            args.WaitForPostBack();
          }
          else
            WebEditResponse.Eval(string.Format("Sitecore.PageModes.ChromeManager.handleMessage('{0}', {{ id: '{1}', openProperties: {2} }});", AddRendering.IsMorphRenderingsRequest(args) ? (object)"chrome:rendering:morphcompleted" : (object)"chrome:placeholder:controladded", (object)str, (object)flag.ToString().ToLowerInvariant()));
        }
        else
          WebEditResponse.Eval(string.Format("Sitecore.PageModes.ChromeManager.handleMessage('chrome:placeholder:controladded', {{ id: '{0}', openProperties: {1}, dataSource: '{2}' }});", (object)(args.Parameters["SelectedRendering"] ?? string.Empty), (object)(args.Parameters["OpenProperties"] ?? "false"), (object)args.Result));
      }
      else
      {
        List<Item> placeholderRenderings;
        string dialogUrl;
        AddRendering.RunGetPlaceholderRenderingsPipeline(args.Parameters, out placeholderRenderings, out dialogUrl);
        if (string.IsNullOrEmpty(dialogUrl))
          return;
        SheerResponse.ShowModalDialog(dialogUrl, "720px", "470px", string.Empty, true);
        args.WaitForPostBack();
      }
    }

    private static NameValueCollection CreateParameters(CommandContext context)
    {
      Assert.ArgumentNotNull((object)context, nameof(context));
      string xml = Sitecore.Web.WebEditUtil.ConvertJSONLayoutToXML(WebUtil.GetFormValue("scLayout"));
      Assert.IsNotNull((object)xml, "layout");
      NameValueCollection nameValueCollection = new NameValueCollection();
      nameValueCollection["placeholder"] = context.Parameters["placeholder"];
      nameValueCollection["layout"] = xml;
      nameValueCollection["renderingIds"] = context.Parameters["renderingIds"];
      nameValueCollection["contextitempath"] = context.Items[0].Paths.FullPath;
      #region FIX
      nameValueCollection["itemID"] = context.Items[0].ID.ToString();
      #endregion
      return nameValueCollection;
    }

    private static bool IsMorphRenderingsRequest(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      return args.Parameters["renderingIds"] != null;
    }

    private static bool IsSelectDatasourceDialogPostBack(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull((object)args, nameof(args));
      if (args.IsPostBack && args.Parameters["SelectedRendering"] != null)
        return args.Parameters["OpenProperties"] != null;
      return false;
    }

    private static void RunGetPlaceholderRenderingsPipeline(NameValueCollection context, out List<Item> placeholderRenderings, out string dialogUrl)
    {
      Assert.IsNotNull((object)context, nameof(context));
      ID deviceId = ShortID.DecodeID(WebUtil.GetFormValue("scDeviceID"));
      GetPlaceholderRenderingsArgs placeholderRenderingsArgs = new GetPlaceholderRenderingsArgs(context["placeholder"], context["layout"], Sitecore.Client.ContentDatabase, deviceId);
      placeholderRenderingsArgs.OmitNonEditableRenderings = true;
      if (!string.IsNullOrEmpty(context["renderingIds"]))
      {
        List<ID> list = ((IEnumerable<string>)context["renderingIds"].Split(new string[1]
        {
          "|"
        }, StringSplitOptions.RemoveEmptyEntries)).Select<string, ID>((Func<string, ID>)(id => ID.Parse(id))).ToList<ID>();
        placeholderRenderingsArgs.PredefinedRenderingIds = list;
        placeholderRenderingsArgs.Options.Title = "Select a replacement rendering";
        placeholderRenderingsArgs.Options.Icon = "ApplicationsV2/32x32/replace2.png";
      }
      CorePipeline.Run("getPlaceholderRenderings", (PipelineArgs)placeholderRenderingsArgs);
      placeholderRenderings = placeholderRenderingsArgs.PlaceholderRenderings;
      dialogUrl = placeholderRenderingsArgs.DialogURL;
    }

    private static Item GetItemFromUrl()
    {
      string queryString1 = WebUtil.GetQueryString("id");
      string queryString2 = WebUtil.GetQueryString("db");
      if (string.IsNullOrEmpty(queryString1) || string.IsNullOrEmpty(queryString2))
        return (Item)null;
      return Factory.GetDatabase(queryString2).GetItem(new ID(queryString1));
    }
  }
}
