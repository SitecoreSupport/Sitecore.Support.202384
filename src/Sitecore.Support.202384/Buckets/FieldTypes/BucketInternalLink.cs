namespace Sitecore.Support.Buckets.FieldTypes
{
    using Sitecore;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Text;
    using Sitecore.Web.UI.Sheer;
    using System;
    using System.Web.UI;

    public class BucketInternalLink : Sitecore.Support.Shell.Applications.ContentEditor.RenderingDatasource
    {
        protected virtual Database GetContentDatabase()
        {
           return Client.ContentDatabase;
        }

        public override void HandleMessage(Message message)
        {
            string str;
            base.HandleMessage(message);
            if ((message["id"] != this.ID) || ((str = message.Name) == null))
            {
                return;
            }
            if (str != "contentExtension:pastequery")
            {
                if (str != "contentExtension:buildquery")
                {
                    return;
                }
            }
            else
            {
                if (Sitecore.Context.ClientData.GetValue("CurrentPasteDatasource") != null)
                {
                    this.SetValue(Sitecore.Context.ClientData.GetValue("CurrentPasteDatasource").ToString());
                }
                if (this.Value.Length > 0)
                {
                    this.SetModified();
                }
                return;
            }
            Sitecore.Context.ClientPage.Start(this, "OpenSearch");
        }

        protected void OpenSearch(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && (args.Result != "undefined"))
                {
                    Item item = this.GetContentDatabase().Items[args.Result];
                    if (item != null)
                    {
                        if (this.ValueItemId != item.ID.ToString())
                        {
                            this.SetModified();
                        }
                        this.SetValue(item.ID.ToString());
                    }
                    else
                    {
                        this.SetModified();
                        this.SetValue(args.Result);
                    }
                }
            }
            else
            {
                UrlString str = new UrlString("/sitecore/shell/Applications/Dialogs/Bucket Datasource Link.aspx");
                string str3 = this.ValueItemId;
                Item item2 = this.GetContentDatabase().Items[this.ValueItemId];
                if (item2 != null)
                {
                    str3 = item2.ID.ToString();
                }
                str.Append("db", this.GetContentDatabase().Name);
                str.Append("id", str3);
                str.Append("fo", str3);
                if (!string.IsNullOrEmpty(this.Source))
                {
                    str.Append("ro", this.Source);
                }
                str.Append("sc_content", Sitecore.Context.ContentDatabase.Name);
                SheerResponse.ShowModalDialog(str.ToString(), "1200", "600", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        public string Source
        {
            get
            {
                return base.GetViewStateString("Source");
            }
            set
            {
                string str = MainUtil.UnmapPath(value);
                if (str.EndsWith("/"))
                {
                    str = str.Substring(0, str.Length - 1);
                }
                base.SetViewStateString("Source", str);
            }
        }
    }
}
