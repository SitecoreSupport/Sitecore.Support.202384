// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectRenderingDatasourceForm.cs" company="Sitecore">
//   Copyright (c) Sitecore. All rights reserved.
// </copyright>
// <summary>
//   Defines the SelectRenderingDatasourceForm type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Buckets;

namespace Sitecore.Support.Buckets.Forms
{
  /// <summary>
  /// Select rendering data source dialog.
  /// </summary>
  public class SelectRenderingDatasourceForm : Shell.Applications.Dialogs.SelectRenderingDatasource.SelectRenderingDatasourceForm
  {
    #region Fields

    /// <summary>
    /// The item link.
    /// </summary>
    protected Edit ItemLink;

    /// <summary>
    /// The item link.
    /// </summary>
    protected Literal PathResolve;

    /// <summary>
    /// The search option.
    /// </summary>
    protected Border SearchOption;

    /// <summary>
    /// The search section.
    /// </summary>
    protected Border SearchSection;

    #endregion

    /// <summary>
    /// Raises the load event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
    protected override void OnLoad([NotNull] System.EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      base.OnLoad(e);

      if (Context.ClientPage.IsEvent)
      {
        return;
      }

      if (!ContentSearchManager.Locator.GetInstance<IContentSearchConfigurationSettings>().ItemBucketsEnabled())
      {
        this.SearchOption.Visible = false;
        this.SearchSection.Visible = false;
        return;
      }

      this.SearchOption.Click = "ChangeMode(\"Search\")";

      if (!string.IsNullOrEmpty(this.SelectDatasourceOptions.CurrentDatasource))
      {
        this.SetPathResolve();
      }

      this.SetSectionHeader();
    }

    /// <summary>
    /// Handles a click on the OK button.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The args.</param>
    /// <remarks>
    /// When the user clicks OK, the dialog is closed by calling
    /// the <see cref="M:Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow"><c>CloseWindow</c></see> method.
    /// </remarks>
    protected override void OnOK([NotNull] object sender, [NotNull] System.EventArgs args)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(args, "args");

      if (!ContentSearchManager.Locator.GetInstance<IContentSearchConfigurationSettings>().ItemBucketsEnabled())
      {
        base.OnOK(sender, args);
        return;
      }

      switch (this.CurrentMode)
      {
        case "Clone":
        case "Create":
          base.OnOK(sender, args);
          break;

        case "Select":
          {
            var selectionItem = this.Treeview.GetSelectionItem();
            if (selectionItem != null)
            {
              var pathResolve = this.PathResolve;
              if (pathResolve != null)
              {
                pathResolve.Text = selectionItem.Paths.FullPath;
              }

              this.SetDialogResult(selectionItem);
            }
            else
            {
              this.SetDialogDataSourceResult(this.ItemLink.Value);
            }

            SheerResponse.CloseWindow();
          }

          break;

        case "Search":
          var item = Context.ContentDatabase.GetItem(this.ItemLink.Value);
          if (item != null)
          {
            var pathResolve = this.PathResolve;
            if (pathResolve != null)
            {
              pathResolve.Text = item.Paths.FullPath;
            }

            var selectionItem = this.Treeview.GetSelectionItem();

            if (selectionItem.TemplateID == Sitecore.Buckets.Util.Constants.SavedSearchTemplateID) 
            {
                this.SetDialogDataSourceResult(selectionItem.Fields[Sitecore.Buckets.Util.Constants.DefaultQuery].Value);
            }

            this.SetDialogResult(item);
            SheerResponse.CloseWindow();
          }
          else
          {
            SheerResponse.Alert(Translate.Text(Sitecore.Buckets.Localization.Texts.PleaseSelectItemFromResults));
          }

          break;
      }
    }

    /// <summary>
    /// Sets the controls.
    /// </summary>
    protected override void SetControlsOnModeChange()
    {
      base.SetControlsOnModeChange();

      if (!ContentSearchManager.Locator.GetInstance<IContentSearchConfigurationSettings>().ItemBucketsEnabled())
      {
        return;
      }

      switch (this.CurrentMode)
      {
        case "Clone":
        case "Create":
        case "Select":
          this.SearchSection.Visible = false;
          this.SearchOption.Class = string.Empty;
          break;

        case "Search":
          this.SearchOption.Class = "selected";
          if (!this.CreateOption.Disabled)
          {
            this.CreateOption.Class = string.Empty;
          }

          this.CloneOption.Class = string.Empty;
          this.SelectOption.Class = string.Empty;
          this.SelectSection.Visible = false;
          this.SearchSection.Visible = true;
          this.CloneSection.Visible = false;
          this.CreateSection.Visible = false;
          this.SetControlsForSearching(this.CreateDestination.GetSelectionItem());
          SheerResponse.Eval(string.Format("selectItemName('{0}')", this.NewDatasourceName.ID));
          break;
      }

      this.SetSectionHeader();
    }

    /// <summary>
    /// Sets the path resolve.
    /// </summary>
    protected virtual void SetPathResolve()
    {
      var item = Context.Database.GetItem(this.SelectDatasourceOptions.CurrentDatasource);
      if (item != null)
      {
        var pathResolve = this.PathResolve;
        if (pathResolve != null)
        {
          pathResolve.Text = pathResolve.Text + " " + item.Paths.FullPath;
        }
      }
    }

    /// <summary>
    /// Sets the controls for searching.
    /// </summary>
    /// <param name="item">The item.</param>
    private void SetControlsForSearching([NotNull] Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      this.Warnings.Visible = false;


      this.RightContainer.Class = "rightColumn";

      string errorMessage;
      if (!this.CanCreateItem(item, out errorMessage))
      {
        this.OK.Disabled = true;
        this.Information.Text = Translate.Text(errorMessage);
        this.Warnings.Visible = true;
        this.RightContainer.Class = "rightColumn visibleWarning";
      }
      else
      {
        this.OK.Disabled = false;
      }
    }

    /// <summary>
    /// Sets the section header.
    /// </summary>
    private void SetSectionHeader()
    {
      if (this.CurrentMode == "Search")
      {
        this.SectionHeader.Text = Translate.Text(Sitecore.Buckets.Localization.Texts.SearchForContentItem);
      }
    }
  }
}