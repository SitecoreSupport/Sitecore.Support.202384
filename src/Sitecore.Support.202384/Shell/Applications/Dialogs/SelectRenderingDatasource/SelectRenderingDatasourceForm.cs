// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectRenderingDatasource.xml.cs" company="Sitecore">
//   Copyright (c) Sitecore. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Support.Shell.Applications.Dialogs.SelectRenderingDatasource
{
  using System;
  using System.Collections.Generic;
  using Diagnostics;
  using Resources;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Globalization;
  using Sitecore.Text;
  using Sitecore.Web.UI;
  using Sitecore.Web.UI.HtmlControls;
  using Sitecore.Web.UI.Sheer;
  using Sitecore.Web.UI.WebControls;
  using StringExtensions;
  using Sitecore.Shell.Applications.Dialogs.SelectCreateItem;
  using Sitecore.Shell.Applications.Dialogs;
  using Sitecore.Shell.Applications.Dialogs.ItemLister;


  /// <summary>Select rendering datasource form</summary>
  public class SelectRenderingDatasourceForm : SelectCreateItemForm
  {
    #region Constants and Fields

    /// <summary>
    /// The options
    /// </summary>
    private SelectDatasourceOptions options;

    /// <summary>
    /// The Clone mode
    /// </summary>
    protected const string CloneMode = "Clone";

    /// <summary>
    /// The clone desitnation
    /// </summary>
    protected TreeviewEx CloneDestination;

    /// <summary>
    /// The clone name
    /// </summary>
    protected Edit CloneName;

    /// <summary>
    /// The clone option
    /// </summary>
    protected Border CloneOption;

    /// <summary>
    /// The clone parent data context
    /// </summary>
    [Obsolete("Deprecated.")]
    protected DataContext CloneParentDataContext;

    /// <summary>
    /// The Clone Section
    /// </summary>
    protected Border CloneSection;

    /// <summary>
    /// The create destination
    /// </summary>
    protected TreeviewEx CreateDestination;

    /// <summary>
    /// The create icon
    /// </summary>
    protected ThemedImage CreateIcon;

    /// <summary>
    /// The create option
    /// </summary>
    protected Border CreateOption;

    /// <summary>
    /// The create parent datacontext
    /// </summary>
    [Obsolete("Deprecated.")]
    protected DataContext CreateParentDataContext;

    /// <summary>
    /// The Create section
    /// </summary>
    protected Border CreateSection;

    /// <summary>
    /// Information
    /// </summary>
    protected Literal Information;

    /// <summary>
    /// The new datasource name
    /// </summary>
    protected Edit NewDatasourceName;

    /// <summary>
    /// The select option
    /// </summary>
    protected Border SelectOption;

    /// <summary>
    /// The Select Section
    /// </summary>
    protected Scrollbox SelectSection;

    /// <summary>
    /// The warnings
    /// </summary>
    protected Border Warnings;

    /// <summary>
    /// The right container
    /// </summary>
    protected Border RightContainer;

    /// <summary>
    /// The Section Header
    /// </summary>
    protected Literal SectionHeader;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the create option control.
    /// </summary>
    /// <value>The create option control.</value>
    protected override Control CreateOptionControl
    {
      get { return this.CreateOption; }
    }

    /// <summary>
    /// Gets the select option control.
    /// </summary>
    /// <value>The select option control.</value>
    protected override Control SelectOptionControl
    {
      get { return this.SelectOption; }
    }

    /// <summary>
    /// Gets or sets the select datasource options.
    /// </summary>
    /// <value>
    /// The select datasource options.
    /// </value>
    [NotNull]
    protected SelectDatasourceOptions SelectDatasourceOptions
    {
      get
      {
        if (this.options == null)
        {
          this.options = SelectItemOptions.Parse<SelectDatasourceOptions>();
        }

        return this.options;
      }

      set
      {
        Assert.ArgumentNotNull(value, "value");
        this.options = value;
      }
    }

    /// <summary>
    /// Gets or sets the content language.
    /// </summary>
    /// <value>The content language.</value>
    [CanBeNull]
    private Language ContentLanguage
    {
      get
      {
        return this.ServerProperties["cont_language"] as Language;
      }

      set
      {
        this.ServerProperties["cont_language"] = value;
      }
    }

    /// <summary>
    /// Gets the current datasource item.
    /// </summary>
    /// <value>The current datasource item.</value>
    [CanBeNull]
    private Item CurrentDatasourceItem
    {
      get
      {
        string path = this.CurrentDatasourcePath;
        if (!string.IsNullOrEmpty(path))
        {
          return Client.ContentDatabase.GetItem(path);
        }

        return null;
      }
    }

    /// <summary>
    /// Gets or sets the current datasource path.
    /// </summary>
    /// <value>The current datasource path.</value>
    [CanBeNull]
    private string CurrentDatasourcePath
    {
      get
      {
        return this.ServerProperties["current_datasource"] as string;
      }

      set
      {
        this.ServerProperties["current_datasource"] = value;
      }
    }

    /// <summary>
    /// Gets or sets Prototype.
    /// </summary>
    [CanBeNull]
    private Item Prototype
    {
      [CanBeNull]
      get
      {
        ItemUri uri = this.PrototypeUri;
        if (uri != null)
        {
          return Database.GetItem(uri);
        }

        return null;
      }

      [NotNull]
      set
      {
        Assert.IsNotNull(value, "value");
        this.ServerProperties["template_item"] = value.Uri;
      }
    }

    /// <summary>
    /// Gets PrototypeUri.
    /// </summary>
    [CanBeNull]
    private ItemUri PrototypeUri
    {
      [CanBeNull]
      get
      {
        return this.ServerProperties["template_item"] as ItemUri;
      }
    }

    private bool DataContextInitilized
    {
      get
      {
        return MainUtil.GetBool(this.ServerProperties["datacontex_initilized"], false);
      }

      set
      {
        this.ServerProperties["datacontex_initilized"] = value;
      }
    }


    #endregion

    #region Public Methods

    /// <summary>
    /// Handles the message.
    /// </summary>
    /// <param name="message">The message.</param>
    public override void HandleMessage(Message message)
    {
      if (message.Name == "datacontext:changed" && !this.DataContextInitilized)
      {
        message.CancelBubble = true;
        message.CancelDispatch = true;
        return;
      }

      base.HandleMessage(message);
    }

    /// <summary>
    /// Changes the mode.
    /// </summary>
    /// <param name="mode">The mode.</param>
    protected override void ChangeMode([NotNull] string mode)
    {
      Assert.ArgumentNotNull(mode, "mode");

      base.ChangeMode(mode);
      if (!UIUtil.IsIE())
      {
        SheerResponse.Eval("scForm.browser.initializeFixsizeElements();");
      }
      else
      {
        SheerResponse.Eval("if (window.Flexie) Flexie.updateInstance();");
      }
    }

    #endregion

    #region Methods

    /// <summary>Clones the destination_ change.</summary>   
    protected void CloneDestination_Change()
    {
      var selectedItem = this.CloneDestination.GetSelectionItem();
      this.SetControlsForCloning(selectedItem);
    }

    /// <summary>Creates the destination_ change.</summary>   
    protected void CreateDestination_Change()
    {
      var selectedItem = this.CreateDestination.GetSelectionItem();
      this.SetControlsForCreating(selectedItem);
    }

    /// <summary>Raises the load event.</summary>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
      Assert.ArgumentNotNull(e, "e");
      base.OnLoad(e);
      if (Context.ClientPage.IsEvent)
      {
        return;
      }

      this.SelectOption.Click = string.Format("ChangeMode(\"{0}\")", SelectMode);
      this.CreateOption.Click = string.Format("ChangeMode(\"{0}\")", CreateMode);
      this.CloneOption.Click = string.Format("ChangeMode(\"{0}\")", CloneMode);

      //this.SelectDatasourceOptions.Root=new Item();

      if (this.SelectDatasourceOptions.DatasourcePrototype == null)
      {
        this.DisableCreateOption();
      }
      else
      {
        this.Prototype = this.SelectDatasourceOptions.DatasourcePrototype;
      }

      if (!string.IsNullOrEmpty(this.SelectDatasourceOptions.DatasourceItemDefaultName))
      {
        this.NewDatasourceName.Value = this.GetNewItemDefaultName(
          this.SelectDatasourceOptions.Root, this.SelectDatasourceOptions.DatasourceItemDefaultName);
      }

      if (this.SelectDatasourceOptions.ContentLanguage != null)
      {
        this.ContentLanguage = this.SelectDatasourceOptions.ContentLanguage;
      }

      if (Settings.ItemCloning.Enabled && !string.IsNullOrEmpty(this.SelectDatasourceOptions.CurrentDatasource))
      {
        this.CurrentDatasourcePath = this.SelectDatasourceOptions.CurrentDatasource;
        if (this.SelectDatasourceOptions.Root != null)
        {
          var name = string.Empty;
          if (!string.IsNullOrEmpty(this.SelectDatasourceOptions.DatasourceItemDefaultName))
          {
            name = ItemUtil.GetCopyOfName(this.SelectDatasourceOptions.Root, this.SelectDatasourceOptions.DatasourceItemDefaultName);
          }

          if (this.CurrentDatasourceItem != null)
          {
            name =
              this.CloneName.Value = ItemUtil.GetCopyOfName(this.SelectDatasourceOptions.Root, this.CurrentDatasourceItem.Name);
          }

          this.CloneName.Value = name;
        }
      }
      else
      {
        this.CloneOption.Visible = false;
      }

      this.SetDataContexts(this.SelectDatasourceOptions.DatasourceRoots);
      this.DataContextInitilized = true;
      this.SetControlsForSelection(this.CurrentDatasourceItem ?? this.DataContext.GetFolder());
      this.SetSectionHeader();
    }

    /// <summary>
    /// Handles a click on the OK button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <remarks>When the user clicks OK, the dialog is closed by calling
    /// the <see cref="Sitecore.Web.UI.Sheer.ClientResponse.CloseWindow"><c>CloseWindow</c></see> method.</remarks>
    protected override void OnOK(object sender, EventArgs args)
    {
      Assert.ArgumentNotNull(sender, "sender");
      Assert.ArgumentNotNull(args, "args");

      switch (this.CurrentMode)
      {
        case SelectMode:
          var item = this.Treeview.GetSelectionItem();
          if (item != null)
          {
            this.SetDialogResult(item);
          }

          SheerResponse.CloseWindow();
          break;
        case CloneMode:
          this.CloneDatasource();
          break;
        case CreateMode:
          this.CreateDatasource();
          break;
      }
    }

    /// <summary>Handles the Treeview_ click event.</summary>
    protected void Treeview_Click()
    {
      this.SetControlsForSelection(this.Treeview.GetSelectionItem());
    }

    /// <summary>
    /// Disables the create option.
    /// </summary>
    private void DisableCreateOption()
    {
      this.CreateOption.Disabled = true;
      this.CreateOption.Class = "option-disabled";
      this.CreateOption.Click = "javascript:void(0);";
      this.CreateIcon.Src = Images.GetThemedImageSource(this.CreateIcon.Src, ImageDimension.id32x32, true);
    }

    /// <summary>The clone datasource.</summary>
    private void CloneDatasource()
    {
      Item target = this.CloneDestination.GetSelectionItem();
      if (target == null)
      {
        SheerResponse.Alert(Texts.PARENT_NOT_FOUND);
        return;
      }

      string name = this.CloneName.Value;
      string errorMessage;
      if (!this.ValidateNewItemName(name, out errorMessage))
      {
        SheerResponse.Alert(errorMessage);
        return;
      }

      Item currentDatasource = this.CurrentDatasourceItem;
      Assert.IsNotNull(currentDatasource, "currentDatasource");

      if (target.Paths.LongID.StartsWith(currentDatasource.Paths.LongID, StringComparison.InvariantCulture))
      {
        SheerResponse.Alert(Texts.AN_ITEM_CANNOT_BE_COPIED_BELOW_ITSELF);
        return;
      }

      name = ItemUtil.GetCopyOfName(target, name);
      Item clone = currentDatasource.CloneTo(target, name, true);
      if (clone != null)
      {
        this.SetDialogResult(clone);
      }

      SheerResponse.CloseWindow();
    }

    /// <summary>The create datasource.</summary>
    private void CreateDatasource()
    {
      Item item = this.CreateDestination.GetSelectionItem();
      if (item == null)
      {
        SheerResponse.Alert(Texts.PLEASE_SELECT_AN_ITEM_FIRST);
        return;
      }

      string newItemName = this.NewDatasourceName.Value;
      string errorMessage;
      if (!this.ValidateNewItemName(newItemName, out errorMessage))
      {
        SheerResponse.Alert(errorMessage);
        return;
      }

      var contentLang = this.ContentLanguage;
      if (contentLang != null && contentLang != item.Language)
      {
        item = item.Database.GetItem(item.ID, contentLang) ?? item;
      }

      Item newDatasourceItem;
      if (this.Prototype != null && this.Prototype.TemplateID == TemplateIDs.BranchTemplate)
      {
        newDatasourceItem = item.Add(newItemName, (BranchItem)this.Prototype);
      }
      else
      {
        newDatasourceItem = item.Add(newItemName, (TemplateItem)this.Prototype);
      }


      if (newDatasourceItem != null)
      {
        this.SetDialogResult(newDatasourceItem);
      }

      SheerResponse.CloseWindow();
    }

    /// <summary>Sets the controls.</summary>
    protected override void SetControlsOnModeChange()
    {
      base.SetControlsOnModeChange();
      switch (this.CurrentMode)
      {
        case SelectMode:
          this.CloneOption.Class = string.Empty;
          this.SelectSection.Visible = true;
          this.CloneSection.Visible = false;
          this.CreateSection.Visible = false;
          this.SetControlsForSelection(this.Treeview.GetSelectionItem());
          break;

        case CloneMode:
          this.CloneOption.Class = SelectedOptionCssClass;
          if (!this.CreateOption.Disabled)
          {
            this.CreateOption.Class = string.Empty;
          }

          this.SelectOption.Class = string.Empty;
          this.SelectSection.Visible = false;
          this.CloneSection.Visible = true;
          this.CreateSection.Visible = false;
          this.SetControlsForCloning(this.CloneDestination.GetSelectionItem());
          SheerResponse.Eval(string.Format("selectItemName('{0}')", this.CloneName.ID));
          break;

        case CreateMode:
          this.CloneOption.Class = string.Empty;
          this.SelectSection.Visible = false;
          this.CloneSection.Visible = false;
          this.CreateSection.Visible = true;
          this.SetControlsForCreating(this.CreateDestination.GetSelectionItem());
          SheerResponse.Eval(string.Format("selectItemName('{0}')", this.NewDatasourceName.ID));
          break;
      }

      this.SetSectionHeader();
    }

    /// <summary>The set controls for cloning.</summary>
    /// <param name="item">The item.</param>
    private void SetControlsForCloning([CanBeNull] Item item)
    {
      this.SetControlsForCreating(item);
    }

    /// <summary>The set controls for creating.</summary>
    /// <param name="item">The item.</param>
    private void SetControlsForCreating([CanBeNull] Item item)
    {
      this.Warnings.Visible = false;
      SheerResponse.SetAttribute(this.Warnings.ID, "title", string.Empty);
      this.RightContainer.Class = "rightColumn";
      string errorMessage;
      if (!this.CanCreateItem(item, out errorMessage))
      {
        this.OK.Disabled = true;
        this.Information.Text = Translate.Text(errorMessage);
        this.Warnings.Visible = true;
        this.RightContainer.Class = "rightColumn visibleWarning";
        return;
      }

      this.OK.Disabled = false;
    }

    /// <summary>The set controls for selection.</summary>
    /// <param name="item">The item.</param>
    private void SetControlsForSelection([CanBeNull] Item item)
    {
      this.Warnings.Visible = false;
      SheerResponse.SetAttribute(this.Warnings.ID, "title", string.Empty);
      this.RightContainer.Class = "rightColumn";
      if (item == null)
      {
        return;
      }

      if (!this.IsSelectable(item))
      {
        this.OK.Disabled = true;
        var itemName = StringUtil.Clip(item.GetUIDisplayName(), 20, true);
        string text = Translate.Text(Texts.THIS_ITEM_CANNOT_BE_SELECTED_AS_A_DATASOURCE).FormatWith(itemName);
        this.Information.Text = text;
        this.Warnings.Visible = true;
        this.RightContainer.Class = "rightColumn visibleWarning";
        SheerResponse.SetAttribute(this.Warnings.ID, "title", Translate.Text(Texts.Thedatasourcemustbea1Item).FormatWith(this.TemplateNamesString));
        return;
      }

      this.Information.Text = string.Empty;
      this.OK.Disabled = false;
    }

    /// <summary>
    /// The set data contexts.
    /// </summary>
    /// <param name="roots">The roots.</param>
    private void SetDataContexts([NotNull] IEnumerable<Item> roots)
    {
      Assert.ArgumentNotNull(roots, "roots");

      // We keep the default DataContext (width ID DataContext) for backward compatibility
      // with Select Item dialog. Actual datacontexts are created as copies of default one.
      var currentDatasource = this.CurrentDatasourceItem;
      var counter = 0;
      var selectDataContextIds = new ListString();
      var createDataContextIds = new ListString();
      var cloneDataContextIds = new ListString();
      foreach (var root in roots)
      {
        var dataContext = this.CopyDataContext(this.DataContext, "SelectDataContext" + counter);
        #region FIX
        // FIX. Change from root.Paths.FullPath
        dataContext.Root = root.Paths.LongID;
        // End of FIX
        #endregion
        bool isCurrentRoot = currentDatasource != null && (currentDatasource.ID == root.ID || currentDatasource.Paths.IsDescendantOf(root));
        if (isCurrentRoot)
        {
          dataContext.Folder = currentDatasource.ID.ToString();
          var multiRootTreeview = this.Treeview as MultiRootTreeview;
          if (multiRootTreeview != null)
          {
            multiRootTreeview.CurrentDataContext = dataContext.ID;
          }
        }


        Context.ClientPage.AddControl(this.Dialog, dataContext);
        selectDataContextIds.Add(dataContext.ID);
        dataContext = this.CopyDataContext(this.DataContext, "CreateDataContext" + counter);
        #region FIX
        dataContext.Root = root.Paths.LongID;
        #endregion
        Context.ClientPage.AddControl(this.Dialog, dataContext);
        createDataContextIds.Add(dataContext.ID);

        dataContext = this.CopyDataContext(this.DataContext, "CloneDataContext" + counter);
        #region FIX
        dataContext.Root = root.Paths.LongID;
        #endregion
        Context.ClientPage.AddControl(this.Dialog, dataContext);
        cloneDataContextIds.Add(dataContext.ID);
        counter++;
      }

      this.Treeview.DataContext = selectDataContextIds.ToString();
      this.CreateDestination.DataContext = createDataContextIds.ToString();
      this.CloneDestination.DataContext = cloneDataContextIds.ToString();
    }

    /// <summary>
    /// Copies the data context.
    /// </summary>
    /// <param name="dataContext">The data context.</param>
    /// <param name="id">The id.</param>
    /// <returns>The data context.</returns>
    [NotNull]
    private DataContext CopyDataContext([NotNull]DataContext dataContext, [NotNull]string id)
    {
      Assert.ArgumentNotNull(dataContext, "dataContext");
      Assert.ArgumentNotNull(id, "id");

      var copy = new DataContext();
      copy.Filter = dataContext.Filter;
      copy.DataViewName = dataContext.DataViewName;
      copy.ID = id;
      return copy;
    }

    /// <summary>
    /// Sets the section header.
    /// </summary>
    private void SetSectionHeader()
    {
      switch (CurrentMode)
      {
        case SelectMode:
          this.SectionHeader.Text = Translate.Text(Texts.Selecttheexistingcontent);
          break;
        case CreateMode:
          this.SectionHeader.Text = Translate.Text(Texts.Createanewcontent);
          break;
        case CloneMode:
          this.SectionHeader.Text = Translate.Text(Texts.Clonethecurrentcontent);
          break;
      }
    }

    #endregion
  }
}