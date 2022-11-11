using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.DevExpressHelper;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Extensions;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Views.Helpers;
using EAS.LeegooBuilder.Client.GUI.Modules.MainModule.Models;
using EAS.LeegooBuilder.ServiceClient;
using EAS.LeegooBuilder.ServiceClient.MVVM;
using EAS.LeegooBuilder.Common.CommonTypes.Constants;
using EAS.LeegooBuilder.Common.CommonTypes.Definitions;
using EAS.LeegooBuilder.Common.CommonTypes.EventTypes;
using EAS.LeegooBuilder.Common.CommonTypes.Helpers;
using EAS.LeegooBuilder.Common.CommonTypes.Interfaces;
using EAS.LeegooBuilder.Server.DataAccess.Core;
using EAS.LeegooBuilder.Server.DataAccess.Core.Configuration;
using EAS.LeegooBuilder.Server.DataAccess.Core.Global;
using EAS.LeegooBuilder.Server.DataAccess.Core.Proposals;
using PrismCompatibility;
using PrismCompatibility.ServiceLocator;
using MessageBox = EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.ViewModels.MessageBox;

namespace EAS.LeegooBuilder.Client.GUI.Modules.Plugin.ViewModels
{
    [Export]
    [PartCreationPolicyAttributeNonShared]
    public partial class PluginViewModel : UserSettingsAwareClientViewModelBase
    {
        private DXToggleButtonCommand _lockProposalToggleButtonCommand;
        
        #region RibbonHelpers

        private new CommandModel AddCommand(PageGroupModel pageGroup, string caption, Action action, string smallGlyph = null,
            string largeGlyph = null, bool isEnabled = false, string hint = null, KeyGesture keyGesture = null,
            CheckConditionsDelegate canExecuteDelegate = null)
        {
            ImageSource sg = null;
            if (smallGlyph != null)
                sg = GlyphHelper.GetGlyph("/Images/Ribbon/" + smallGlyph, this);
            ImageSource lg = null;
            if (largeGlyph != null)
                lg = GlyphHelper.GetGlyph("/Images/Ribbon/" + largeGlyph, this);

            CommandModel command = new CommandModel(action, canExecuteDelegate)
            {
                Caption = caption,
                IsEnabled = isEnabled,
                SmallGlyph = sg,
                LargeGlyph = lg,
                Hint = hint,
                KeyGesture = keyGesture
            };
            pageGroup.Commands.Add(command);
            return command;
        }

        #endregion RibbonHelpers

        #region Properties

        #region Caption

        /// <summary>   caption of region (top) </summary>
        public override string Caption => "DemoPlugIn header"; // displayed in client area (upper left corner)

        #endregion

        #region SelectedConfigurationTreeItem

        private TreeStructureItem<ConfigurationItem> _selectedConfigurationTreeItem;

        /// <summary>   selected Configuration-TreeItem. </summary>
        public TreeStructureItem<ConfigurationItem> SelectedConfigurationTreeItem
        {
            get => _selectedConfigurationTreeItem;
            set
            {
                SetProperty(ref _selectedConfigurationTreeItem, value);
                if (_selectedConfigurationTreeItem == null) return;

                if (_selectedConfigurationTreeItem.Value.LocalAttributes == null ||
                    _selectedConfigurationTreeItem.Value.HasConfigurator && _selectedConfigurationTreeItem.Value.GlobalAttributes == null)
                {
                    ProjectAndConfigurationModel.LoadAttributes(_selectedConfigurationTreeItem);
                }
            }
        }

        #endregion

        #region VisibilityOfEditStateIndicationColumnInConfigurationTree

        private bool _visibilityOfEditStateIndicationColumnInConfigurationTree;

        /// <summary>
        /// Visibility of column 'EditStateIndication' in ConfigurationTree (left)
        /// If there are no edit states, the column is hidden.
        /// </summary>
        public bool VisibilityOfEditStateIndicationColumnInConfigurationTree
        {
            get => _visibilityOfEditStateIndicationColumnInConfigurationTree;
            set => SetProperty(ref _visibilityOfEditStateIndicationColumnInConfigurationTree, value);
        }

        private void SetVisibilityOfEditStateIndicationColumnInConfigurationTree()
        {
            if (ProjectAndConfigurationModel.SelectedProposal.Configuration != null)
                VisibilityOfEditStateIndicationColumnInConfigurationTree = ProjectAndConfigurationModel.SelectedProposal.Configuration.EditStateCount > 0;
        }

        #endregion

        #region VisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree

        /// <summary>
        /// Visibility of column 'HasAnySpecializedDescription' in ConfigurationTree (left)
        /// If there are no specialized descriptions, the column is hidden.
        /// </summary>
        public bool VisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree
        {
            get => _visibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree;
            set => SetProperty(ref _visibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree, value);
        }

        private bool _visibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree;

        private void SetVisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree()
        {
            VisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree = ProjectAndConfigurationModel.SelectedProposal.Configuration != null && ProjectAndConfigurationModel.SelectedProposal.Configuration.FindInBreadth(x => x.HasAnySpecializedDescription) != null;
        }

        #endregion
        
        #region VisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree

        /// <summary>
        /// Visibility of column 'HasAnySpecializedLongText' in ConfigurationTree (left)
        /// If there are no specialized longtexts, the column is hidden.
        /// </summary>
        public bool VisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree
        {
            get => _visibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree;
            set => SetProperty(ref _visibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree, value);
        }

        private bool _visibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree;

        private void SetVisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree()
        {
            VisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree = ProjectAndConfigurationModel.SelectedProposal != null && ProjectAndConfigurationModel.SelectedProposal.Configuration != null && ProjectAndConfigurationModel.SelectedProposal.Configuration.FindInBreadth(x => x.HasAnySpecializedLongText) != null;
        }

        #endregion
        
        #region VisibilityOfAnchorColumnInConfigurationTree

        private bool _visibilityOfAnchorColumnInConfigurationTree;

        /// <summary>
        /// Visibility of column 'Anchor' in ConfigurationTree (left)
        /// If there are no anchores, the column is hidden.
        /// </summary>
        public bool VisibilityOfAnchorColumnInConfigurationTree
        {
            get => _visibilityOfAnchorColumnInConfigurationTree;
            set => SetProperty(ref _visibilityOfAnchorColumnInConfigurationTree, value);
        }

        #endregion

        #region ListOfProjects

        /// <summary>
        /// List of projects
        /// These will be viewed in a ListBox
        /// </summary>
        public List<string> ListOfProjects
        {
            get
            {
                if (_listOfProjects == null)
                {
                    _listOfProjects = new List<string>();

                    var projects = ProjectAndConfigurationModel.GetProjectInfos();
                    foreach (var project in projects)
                    {
                        _listOfProjects.Add(project.Description);
                    }
                }
                return _listOfProjects;
            }
        }

        private List<string> _listOfProjects;

        #endregion

        #endregion

        #region Constructors

        //[ImportingConstructor]
        public PluginViewModel(IEventAggregator eventAggregator,
                                  ProjectAndConfigurationClient projectAndConfigurationModel,
                                  ITranslator translator,
                                  IUserSettingsService userSettingsService)
            : base(eventAggregator, projectAndConfigurationModel, translator, userSettingsService)
        {
            TraceLogHelper.Log("DemoPlugInViewModel: Initialization (start)");

            // Initialization of ViewModel
            serviceLocator = ServiceLocator.Current;
            
            TraceLogHelper.Log("DemoPlugInViewModel: Initialization (end)");

        }

        #endregion

        #region Overrides

        #region OnNavigatedTo

        /// <summary>
        /// EventHandler: The user just selected our PlugIn in the  NavigationBar (left)
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            MouseHelper.WaitIdle();

            // check, if the configuration has been loaded (fpr viewing in a TreeListControl)
            if (ProjectAndConfigurationModel.SelectedProposal.Configuration == null)
            {
                Task.Factory.ExecuteAndWaitNonBlocking(() =>
                {
                    // 2019-01-11, vi: Display ProgressBar if threashold is reached.
                    var threshold = Convert.ToInt16("0" + ProjectAndConfigurationModel?.GetSysSettingsParameterValue(SysSettingsParameter.ShowBusyIndicatorWhenLoadingConfigurationThreshold));
                    if (ProjectAndConfigurationModel?.SelectedProposal?.ComponentCount >= threshold)
                        StartProgressBar("LoadingConfiguration");

                    ProjectAndConfigurationModel.LoadConfiguration(ProjectAndConfigurationModel.SelectedProposal);
                    ProjectAndConfigurationModel.LoadCalculationDataForProposal(User.CurrentUser.CurrentCalculationSystemView?.ViewName,
                        Translator.EAS_Language, User.CurrentUser.LBUser.UserID);

                    LoadCompaniesAndPersons(ProjectAndConfigurationModel.SelectedProposal);
                    
                    ProjectAndConfigurationModel.SelectedProposal.PropertyChanged += SelectedProposalOnPropertyChanged;
                    ProjectAndConfigurationModel.SelectedProposal.Configuration.Root.OnTreeItemChanged += RootOnOnTreeItemChanged;
                    ProjectAndConfigurationModel.SelectedProposal.OnConfigured += OnConfigured;
                    ProjectAndConfigurationModel.SelectedProposal.OnCalculated += OnCalculated;

                    // Hint:
                    // Do not forget to remove used events in ViewClosed().

                    EndProgressBar();
                });
            }

            SetVisibilityOfEditStateIndicationColumnInConfigurationTree();
            SetVisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree();
            SetVisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree();


            // Invoke OnNavigatedTo() in host application
            ProjectAndConfigurationModel.FireOnNavigatedTo(ProjectAndConfigurationModel.SelectedProposal, NavigationTargets.PlugIn, Name);


            base.OnNavigatedTo(navigationContext);
        }

        #endregion
        
        #region OnNavigatedFrom

        /// <summary>
        /// EventHandler: The user just leaves our PlugIn by selecting another module in the  NavigationBar (left)
        /// </summary>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // DoSomething()
            base.OnNavigatedFrom(navigationContext);
        }

        #endregion
        
        #region ViewClosed

        public override void ViewClosed()
        {
            ProjectAndConfigurationModel.SelectedProposal.PropertyChanged -= SelectedProposalOnPropertyChanged;
            ProjectAndConfigurationModel.SelectedProposal.Configuration.Root.OnTreeItemChanged -= RootOnOnTreeItemChanged;
            ProjectAndConfigurationModel.SelectedProposal.OnConfigured -= OnConfigured;
            ProjectAndConfigurationModel.SelectedProposal.OnCalculated -= OnCalculated;

            base.ViewClosed();
        }

        #endregion
        
        #region SetUpUI

        /// <summary>   Text on PageControl-Tab (bottom) </summary>
        /// <remarks>   M Fries, 04.05.2021. </remarks>
        protected override void SetUpUI()
        {
            title = "TestTitel"; // displayed on page (bottom left)
        }

        #endregion

        #region SetUpRibbonViewModel

        /// <summary>
        /// Create all ribbon controls
        /// </summary>
        protected override void SetUpRibbonViewModel()
        {
            base.SetUpRibbonViewModel();

            var category = new CategoryModel();
            RibbonViewModel.Categories.Add(category);

            

            
            
            // Dynamic data page
            #region DynamicData Page

            var dynamicDataPage = new PageModel { Name = "Dynamic Data" };
            category.Pages.Add(dynamicDataPage);

            
            // IO page
            var ioGroup = new PageGroupModel { Name = "I/O" };
            AddCommand(ioGroup, "Save", ExecuteSaveConfigurationItem, largeGlyph: "SaveAll_32x32.png", hint: "Save all data", canExecuteDelegate: CanExecuteSaveConfigurationItem);
            dynamicDataPage.Groups.Add(ioGroup);
            
            
            // Proposal page
            var proposalGroup = new PageGroupModel { Name = "Proposal" };
            AddCommand(proposalGroup, "Create", ExecuteCreateProposal, largeGlyph: "Add_32x32.png", hint: "Create a new proposal", canExecuteDelegate: CanExecuteCreateProposal);
            AddCommand(proposalGroup, "Show Proposal Id", ExecuteShowProposalId, largeGlyph: "check_32x32.png", hint: "Show the Id of the selected proposal", canExecuteDelegate: CanExecuteShowProposalId);

            _lockProposalToggleButtonCommand = new DXToggleButtonCommand(ExecuteToggleLockProposal, CanExecuteToggleLockProposal)
            {
                Caption = Translator.Translate("LockProposal"),
                LargeGlyph = GlyphHelper.GetGlyph("/Images/Ribbon/locked_32x32.png", this),
                Hint = Translator.Translate("LockProposalHint"),
                GroupIndex = 53,
                IsToggleChecked = false
            };
            proposalGroup.Commands.Add(_lockProposalToggleButtonCommand);

            AddCommand(proposalGroup, "Update Proposal List", ExecuteUpdateProposalList, largeGlyph: "Update_32x32.png", hint: "Updates the proposal list in View 'Projects and Proposals'", canExecuteDelegate: CanExecuteUpdateProposalList);

            dynamicDataPage.Groups.Add(proposalGroup);
            
            
            // ConfigurationItem page
            var configurationItemGroup = new PageGroupModel { Name = "ConfigurationItem" };
            AddCommand(configurationItemGroup, "Insert", ExecuteInsertElement, largeGlyph: "Add_32x32.png", hint: "Insert a new element", canExecuteDelegate: CanExecuteInsertElement);
            AddCommand(configurationItemGroup, "Update", ExecuteUpdateConfigurationItem, largeGlyph: "Edit_32x32.png", hint: "Increase the quantity by 1", canExecuteDelegate: CanExecuteUpdateConfigurationItem);
            AddCommand(configurationItemGroup, "Delete", ExecuteDeleteConfigurationItem, largeGlyph: "Remove_32x32.png",  hint: "Delete a configuration item", canExecuteDelegate: CanExecuteDeleteConfigurationItem);
            AddCommand(configurationItemGroup, "Move down", () => ExecuteMoveConfigurationItem(MoveDirection.Down), largeGlyph: "MoveDown_32x32.png", hint: "Move the selected configuration item down", canExecuteDelegate: CanExecuteMoveConfigurationItem);
            AddCommand(configurationItemGroup, "Move up", () => ExecuteMoveConfigurationItem(MoveDirection.Up), largeGlyph: "MoveUp_32x32.png",  hint: "Move the selected configuration item up", canExecuteDelegate: CanExecuteMoveConfigurationItem);
            AddCommand(configurationItemGroup, "Clone", ExecuteCloneConfigurationItem, largeGlyph: "Copy_32x32.png", hint: "Create a 1:1 copy", canExecuteDelegate: CanExecuteCloneConfigurationItem);
            dynamicDataPage.Groups.Add(configurationItemGroup);

            #endregion DynamicData Page
            
            // ToDo:
            // local Attribute
            // global Attribute
            // Pricingfield
            // VKFields (VkTextFields)
            // Document
            
            // Execute Script
            // Calculate ausführen (Programmereignisse allgemein)

            
            // Documents page
            #region Documents Page

            var documentsGroup = new PageGroupModel { Name = "Documents" };
            AddCommand(documentsGroup, "GetDocument PDF", ExecuteGetDocumentAsPdf, largeGlyph: "InsertHeader_32x32.png", hint: "Get an open a document in PDF format", canExecuteDelegate: CanExecuteGetDocumentAsPdf);
            dynamicDataPage.Groups.Add(documentsGroup);
            
            #endregion Documents Page
            
            
            #region Playground Page
            
            // Another PageModel
            var page = new PageModel { Name = "Playground" };

            var doSomethingGroup = new PageGroupModel { Name = "Do Something Group" };
            page.Groups.Add(doSomethingGroup);

            AddCommand(doSomethingGroup, "Do Something", ExecuteDoSomething, largeGlyph: "pass_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Click here to do something", canExecuteDelegate: CanExecuteDoSomething);
            AddCommand(doSomethingGroup, "GenericDialog LoadingBar", ExecuteFakeLoadingBar, largeGlyph: "pass_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Display the GenericDialogs LoadingBar", canExecuteDelegate: CanExecuteDoSomething);
            AddCommand(doSomethingGroup, "Generic Dialogs Popup", ExecuteGenericDialogsPopup, largeGlyph: "pass_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Display a Generic Dialogs Popup", canExecuteDelegate: CanExecuteDoSomething);
            AddCommand(doSomethingGroup, "Do Something Else", ExecuteDoSomethingElse, smallGlyph: "fail_32x32.png", hint: "Click here to do something else", canExecuteDelegate: CanExecuteDoSomethingElse);
            AddCommand(doSomethingGroup, "Filter Elements", ExecuteDoFilterElements, smallGlyph: "find_32x32.png", hint: "Shows how to select some elements", canExecuteDelegate: CanExecuteDoFilterElements);
            AddCommand(doSomethingGroup, "Show BusiInicator", ExecuteShowBusiIndicator, smallGlyph: "process_32x32.png", hint: "Shows the busy indicator vor 5 seconds", canExecuteDelegate: CanExecuteShowBusiIndicator);
            AddCommand(doSomethingGroup, "Execute Script", ExecuteExecuteScript, largeGlyph: "ScriptingEditor_32x32.png", hint: "Executes the Script 'HelloWorld'", canExecuteDelegate: CanExecuteExecuteScript);
            AddCommand(doSomethingGroup, "Load Custom Definition Values", ExecuteGetCustomDefinitionsOfCompany2, largeGlyph: "find_32x32.png", hint: "Reads custom definition values of company2", canExecuteDelegate: CanExecuteGetCustomDefinitionsOfCompany2);
            AddCommand(doSomethingGroup, "Load global attributes", ExecuteLoadGlobalAttributes, largeGlyph: "find_32x32.png", hint: "Loads the global attributes", canExecuteDelegate: CanExecuteLoadGlobalAttributes);
            AddCommand(doSomethingGroup, "Get proposal custom definition values", ExecuteGetProposalCustomDefinitionValues, smallGlyph: "find_32x32.png", hint: "Reads custom definition values of the proposal", canExecuteDelegate: CanExecuteGetProposalCustomDefinitionValues);
            AddCommand(doSomethingGroup, "Set local attributes", ExecuteSetLocalAttributes, smallGlyph: "upgrade_32x32.png", hint: "Sets some local attributes", canExecuteDelegate: CanExecuteSetLocalAttributes);
            

            var proposalGroup2 = new PageGroupModel { Name = "Proposal Group" };
            page.Groups.Add(proposalGroup2);

            //AddCommand(proposalGroup2, "Show Proposal Id", ExecuteShowProposalId, largeGlyph: "check_32x32.png", hint: "Shows the Id of the selected proposal", canExecuteDelegate: CanExecuteShowProposalId);
            //AddCommand(proposalGroup2, "Create Proposal", ExecuteCreateProposal, largeGlyph: "CreateProposal_32x32.png", hint: "Creates a new proposal", canExecuteDelegate: CanExecuteCreateProposal);
            AddCommand(proposalGroup2, "Set Custom Property", ExecuteSetProposalCustomProperty, largeGlyph: "CustomProperty_32x32.png", hint: "Sets a custom definition value", canExecuteDelegate: CanExecuteSetProposalCustomProperty);



            var sqlGroup = new PageGroupModel { Name = "Database Group" };
            AddCommand(sqlGroup, "Select Something", ExecuteSelectSomething, largeGlyph: "sql_32x32.png", hint: "Demonstrates a database select", canExecuteDelegate: CanExecuteSelectSomething);
            page.Groups.Add(sqlGroup);

            
            var pricingGroup = new PageGroupModel { Name = "Pricing" };
            AddCommand(pricingGroup, "Read", ExecuteReadPricingField, smallGlyph: "Dollar_16x16.png", hint: "Demonstrates reading a pricing field", canExecuteDelegate: CanExecuteReadPricingField);
            AddCommand(pricingGroup, "Write", ExecuteWritePricingField, smallGlyph: "Dollar_16x16.png", hint: "Demonstrates writing a pricing field", canExecuteDelegate: CanExecuteWritePricingField);
            page.Groups.Add(pricingGroup);

            category.Pages.Add(page);

            #endregion Playground Page
            
        }

        #endregion

        #endregion

        
        #region LoadCompaniesAndPersons

        /// <summary>
        /// Shows, how to load several company- and person data
        /// </summary>
        /// <param name="proposal"></param>
        private void LoadCompaniesAndPersons(Proposal proposal)
        {
            proposal.BeginUpdate();

            // Anbieter laden
            if ((proposal.Company1ID != null) && (proposal.Company1 == null))
                proposal.Company1 = ProjectAndConfigurationModel.GetCompanyByPrimaryKey((Guid)proposal.Company1ID);
            
            // Kunde laden
            if ((proposal.Company2ID != null) && (proposal.Company2 == null))
                proposal.Company2 = ProjectAndConfigurationModel.GetCompanyByPrimaryKey((Guid)proposal.Company2ID);

            // Person1 laden
            if ((proposal.Person1ID != null) && (proposal.Person1 == null))
                proposal.Person1 = ProjectAndConfigurationModel.GetPersonByPrimaryKey((Guid)proposal.Person1ID);

            // Person2 laden
            if ((proposal.Person2ID != null) && (proposal.Person2 == null))
                proposal.Person2 = ProjectAndConfigurationModel.GetPersonByPrimaryKey((Guid)proposal.Person2ID);
            
            // Person3 laden
            if ((proposal.Person3ID != null) && (proposal.Person3 == null))
                proposal.Person3 = ProjectAndConfigurationModel.GetPersonByPrimaryKey((Guid)proposal.Person3ID);
            
            // Person4 laden
            if ((proposal.Person4ID != null) && (proposal.Person4 == null))
                proposal.Person4 = ProjectAndConfigurationModel.GetPersonByPrimaryKey((Guid)proposal.Person4ID);

            // Owner1 laden
            if ((proposal.Owner1 == null) && (proposal.Owner1ID != null))
            {
                proposal.Owner1 = ProjectAndConfigurationModel.GetUserPrimaryKey((Guid)proposal.Owner1ID);
                if ((proposal.Owner1 != null) && (proposal.Owner1.PersonID != null)) proposal.Owner1.Person = ProjectAndConfigurationModel.GetPersonByPrimaryKey((Guid)proposal.Owner1.PersonID);
            }

            // Owner2 laden
            if ((proposal.Owner2 == null) && (proposal.Owner2ID != null))
            {
                proposal.Owner2 = ProjectAndConfigurationModel.GetUserPrimaryKey((Guid)proposal.Owner2ID);
                if ((proposal.Owner2 != null) && (proposal.Owner2.PersonID != null)) proposal.Owner2.Person = ProjectAndConfigurationModel.GetPersonByPrimaryKey((Guid)proposal.Owner2.PersonID);
            }

            // Owner3 laden
            if ((proposal.Owner3 == null) && (proposal.Owner3ID != null))
            {
                proposal.Owner3 = ProjectAndConfigurationModel.GetUserPrimaryKey((Guid)proposal.Owner3ID);
                if ((proposal.Owner3 != null) && (proposal.Owner3.PersonID != null)) proposal.Owner3.Person = ProjectAndConfigurationModel.GetPersonByPrimaryKey((Guid)proposal.Owner3.PersonID);
            }

            if (proposal.RepresentativePerson == null && proposal.RepresentativePersonID != null)
                proposal.RepresentativePerson = ProjectAndConfigurationModel.GetPersonByPrimaryKey(proposal.RepresentativePersonID.Value);

            if ((proposal.RepresentativeCompanyID != null) && (proposal.RepresentativeCompany == null))
                proposal.RepresentativeCompany = ProjectAndConfigurationModel.GetCompanyByPrimaryKey(proposal.RepresentativeCompanyID.Value);

            
            proposal.EndUpdate();
        }
        
        #endregion LoadCompaniesAndPersons
        

        private void OnConfigured(TreeStructureItem<ConfigurationItem> configurationItem)
        {
            MessageBox.Show("Configuration has been configured.");
        }

        private void OnCalculated()
        {
            MessageBox.Show("Configuration has been calculated.");
        }

        private void RootOnOnTreeItemChanged(object sender, TreeStructureEventArgs<ConfigurationItem> treeStructureEventArgs)
        {
            //MessageBox.Show("ConfigurationHasBeenChanged");
            //this.OnPropertyChanged(() => this.SelectedConfigurationTreeItem);
            eventAggregator.GetEvent<ExecuteConfigurationTreeSmartUpdateEvent>().Publish(null);
        }

        private void SelectedProposalOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            //MessageBox.Show("SelectedProposalOnPropertyChanged");
        }

        #region IDisposable

        private bool _disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Release managed resources.
                }
                // Release unmanaged resources.
                // Set large fields to null.
                // Call Dispose on your base class.
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable
    }
}
