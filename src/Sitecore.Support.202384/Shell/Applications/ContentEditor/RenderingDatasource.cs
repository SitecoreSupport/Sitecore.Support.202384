namespace Sitecore.Support.Shell.Applications.ContentEditor
{
    using Sitecore;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.StringExtensions;
    using Sitecore.Pipelines;
    using Sitecore.Shell.Applications.ContentEditor;
    using Sitecore.Pipelines.GetRenderingDatasource;
    using Sitecore.Web.UI.HtmlControls;
    using Sitecore.Web.UI.Sheer;
    using System;
    using System.Collections.Generic;

    public class RenderingDatasource : Edit, Sitecore.Shell.Applications.ContentEditor.IContentField
    {
        private const string BrowseDatasourceCommand = "contentrenderingdatasource:browse";
        private const string ClearDatasourceCommand = "contentrenderingdatasource:clear";

        protected string ValueItemId {
            get {
                return base.GetViewStateString("ValueItemId");
            }
            set {
                base.SetViewStateString("ValueItemId", value);
            }
        }

        public RenderingDatasource()
        {
            this.Class = "scContentControl";
            base.Activation = true;
        }

        public string GetValue()
        {
            Item item = this.ContentDatabase.GetItem(this.ValueItemId);
            return item?.ID.ToString();
        }

        public override void HandleMessage(Message message)
        {
            string str;
            Assert.ArgumentNotNull(message, "message");
            if ((message["id"] == this.ID) && ((str = message.Name) != null))
            {
                if (str != "contentrenderingdatasource:browse")
                {
                    if (str != "contentrenderingdatasource:clear")
                    {
                        return;
                    }
                }
                else
                {
                    Sitecore.Context.ClientPage.Start(this, "SelectDatasource");
                    return;
                }
                if (this.ValueItemId.Length > 0)
                {
                    this.SetModified();
                }
                this.SetValue(string.Empty);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnPreRender(e);
            base.ServerProperties["Value"] = base.ServerProperties["Value"];
            base.ServerProperties["ItemID"] = base.ServerProperties["ItemID"];
        }

        protected void SelectDatasource(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!args.IsPostBack)
            {
                this.ShowDialog(args);
            }
            else if (args.HasResult)
            {
                this.SetResultValue(args.Result);
            }
        }

        protected override void SetModified()
        {
            base.SetModified();
            if (base.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        protected virtual void SetResultValue(string result)
        {
            Assert.ArgumentNotNullOrEmpty(result, "result");
            Item item = this.ContentDatabase.GetItem(result);
            if (item == null)
            {
                if (!string.IsNullOrEmpty(this.ValueItemId))
                {
                    this.SetModified();
                }
                SheerResponse.Alert("Item not found.", new string[0]);
                this.SetValue(string.Empty);
            }
            else
            {
                if (this.ValueItemId != item.ID.ToString())
                {
                    this.SetModified();
                }
                this.SetValue(item.ID.ToString());
            }
        }

        public void SetValue(string value)
        {
            Item item = this.ContentDatabase.GetItem(value);
            if ( item == null )
            {
                this.Value = value;
            } else {
                this.Value = "{0} [ID:'{1}']".FormatWith(item.Paths.FullPath, item.ID.ToString());
            }
            this.ValueItemId = (item != null) ? item.ID.ToString() : value;
        }

        private void ShowDialog(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item renderingItem = this.RenderingItem;
            if (renderingItem == null)
            {
                SheerResponse.Alert("Rendering not found.", new string[0]);
            }
            else
            {
                Item contentItem = this.ContentItem;
                GetRenderingDatasourceArgs args3 = new GetRenderingDatasourceArgs(renderingItem);
                List<Item> list = new List<Item> {
                    this.ContentDatabase.GetRootItem()
                };
                args3.FallbackDatasourceRoots = list;
                args3.ContentLanguage = contentItem?.Language;
                args3.ContextItemPath = (contentItem != null) ? contentItem.Paths.LongID : string.Empty;
                args3.ShowDialogIfDatasourceSetOnRenderingItem = true;
                args3.CurrentDatasource = this.ValueItemId;
                GetRenderingDatasourceArgs args2 = args3;
                CorePipeline.Run("getRenderingDatasource", args2);
                if (string.IsNullOrEmpty(args2.DialogUrl))
                {
                    SheerResponse.Alert("An error ocurred.", new string[0]);
                }
                else
                {
                    SheerResponse.ShowModalDialog(args2.DialogUrl, "1200px", "700px", string.Empty, true);
                    args.WaitForPostBack();
                }
            }
        }

        private Database ContentDatabase
        {
            get
            {
                return Client.ContentDatabase;
            }
        }

        private Item ContentItem
        {
            get
            {
                Language language;
                FieldEditorParameters persistedParameters = FieldEditorParameters.GetPersistedParameters();
                if ((persistedParameters != null) && !string.IsNullOrEmpty(persistedParameters["contentitem"]))
                {
                    return Sitecore.Data.Database.GetItem(new ItemUri(persistedParameters["contentitem"]));
                }
                string itemID = this.ItemID;
                if (!Sitecore.Data.ID.IsID(itemID))
                {
                    return null;
                }
                if (!Language.TryParse(this.ItemLanguage, out language))
                {
                    language = Sitecore.Context.Language;
                }
                return this.ContentDatabase.GetItem(itemID, language);
            }
        }

        public string ItemID
        {
            get
            {
                return base.GetViewStateString("ItemID");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.SetViewStateString("ItemID", value);
            }
        }

        public string ItemLanguage
        {
            get
            {
                return base.GetViewStateString("ItemLanguage");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.SetViewStateString("ItemLanguage", value);
            }
        }

        private Item RenderingItem
        {
            get
            {
                FieldEditorParameters persistedParameters = FieldEditorParameters.GetPersistedParameters();
                if ((persistedParameters != null) && !string.IsNullOrEmpty(persistedParameters["rendering"]))
                {
                    return Sitecore.Data.Database.GetItem(new ItemUri(persistedParameters["rendering"]));
                }
                if (!string.IsNullOrEmpty(this.ItemID))
                {
                    Item item = this.ContentDatabase.GetItem(this.ItemID);
                    if ((item != null) && ItemUtil.IsRenderingItem(item))
                    {
                        return item;
                    }
                }
                return null;
            }
        }
    }
}
