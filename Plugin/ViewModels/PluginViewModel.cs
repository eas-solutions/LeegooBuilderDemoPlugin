using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.DevExpressHelper;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Views.Helpers;
using EAS.LeegooBuilder.Client.GUI.Modules.MainModule.Models;
using EAS.LeegooBuilder.Client.GUI.Modules.MainModule.ViewModels;
using EAS.LeegooBuilder.ServiceClient;
using EAS.LeegooBuilder.ServiceClient.MVVM;
using EAS.LeegooBuilder.Common.CommonTypes.Constants;
using EAS.LeegooBuilder.Common.CommonTypes.EventTypes;
using EAS.LeegooBuilder.Common.CommonTypes.Extensions;
using EAS.LeegooBuilder.Common.CommonTypes.Helpers;
using EAS.LeegooBuilder.Common.CommonTypes.Interfaces;
using EAS.LeegooBuilder.Server.DataAccess.Core;
using EAS.LeegooBuilder.Server.DataAccess.Core.Configuration;
using EAS.LeegooBuilder.Server.DataAccess.Core.Global;
using PrismCompatibility;
using PrismCompatibility.ServiceLocator;
using MessageBox = EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.ViewModels.MessageBox;

namespace EAS.LeegooBuilder.Client.GUI.Modules.Plugin.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
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

        /// <summary>   Caption der Region (oben) </summary>
        public override string Caption => "DemoPlugIn header"; // displayed in client area (upper left corner)

        #endregion

        #region SelectedConfigurationTreeItem

        private TreeStructureItem<ConfigurationItem> _selectedConfigurationTreeItem;

        /// <summary>   Ausgewähltes Configuration-TreeItem. </summary>
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
        /// Steuert die Sichtbarkeit der Spalte 'EditStateIndication' in der Konfiguration (links)
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
        /// Steuert die Sichtbarkeit der Spalte 'HasAnySpecializedDescription' in der Konfiguration (links)
        /// Wenn keine spezialisierten Benennungen existieren, wird die Spalte ausgeblendet.
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
        /// Steuert die Sichtbarkeit der Spalte 'HasAnySpecializedLongText' in der Konfiguration (links)
        /// Wenn keine spezialisierten Langtexte existieren, wird die Spalte ausgeblendet.
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
        /// Steuert die Sichtbarkeit der Spalte 'Anchor' in der Konfiguration (links)
        /// </summary>
        public bool VisibilityOfAnchorColumnInConfigurationTree
        {
            get => _visibilityOfAnchorColumnInConfigurationTree;
            set => SetProperty(ref _visibilityOfAnchorColumnInConfigurationTree, value);
        }

        #endregion

        #region ListOfProjects

        /// <summary>
        /// Liste der Projekte
        /// Diese wird beispielhaft im View in einer ListBox angezeigt
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

        [ImportingConstructor]
        public PluginViewModel(IEventAggregator eventAggregator,
                                  ProjectAndConfigurationClient projectAndConfigurationModel,
                                  ITranslator translator,
                                  IUserSettingsService userSettingsService)
            : base(eventAggregator, projectAndConfigurationModel, translator, userSettingsService)
        {
            TraceLogHelper.Log("DemoPlugInViewModel: Initialization (start)");

            // Initialisierung des ViewModels
            serviceLocator = ServiceLocator.Current;
            
            TraceLogHelper.Log("DemoPlugInViewModel: Initialization (end)");

        }

        #endregion

        #region Overrides

        #region OnNavigatedTo

        /// <summary>
        /// Der User hat gerade unser PlugIn links in der NavigationBar aufgerufen
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            MouseHelper.WaitIdle();

            // Sicherstellen, dass die Konfiguration geladen ist (für die Darstellung im TreeListControl)
            if (ProjectAndConfigurationModel.SelectedProposal.Configuration == null)
            {
                Task.Factory.ExecuteAndWaitNonBlocking(() =>
                {
                    // 2019-01-11, vi: ProgressBar einschalten, wenn es sich lohnt. Siehe Schwellwert in den SE.
                    var threshold = Convert.ToInt16("0" + ProjectAndConfigurationModel?.GetSysSettingsParameterValue(SysSettingsParameter.ShowBusyIndicatorWhenLoadingConfigurationThreshold));
                    if (ProjectAndConfigurationModel?.SelectedProposal?.ComponentCount >= threshold)
                        StartProgressBar("LoadingConfiguration");

                    ProjectAndConfigurationModel.LoadConfiguration(ProjectAndConfigurationModel.SelectedProposal);
                    ProjectAndConfigurationModel.LoadCalculationDataForProposal(User.CurrentUser.CurrentCalculationSystemView.ViewName,
                        Translator.EAS_Language, User.CurrentUser.LBUser.UserID);

                    ProjectAndConfigurationModel.SelectedProposal.PropertyChanged += SelectedProposalOnPropertyChanged;
                    ProjectAndConfigurationModel.SelectedProposal.Configuration.Root.OnTreeItemChanged += RootOnOnTreeItemChanged;
                    ProjectAndConfigurationModel.SelectedProposal.OnConfigured += OnConfigured;
                    ProjectAndConfigurationModel.SelectedProposal.OnCalculated += OnCalculated;

                    // Hinweis:
                    // Neue Events müssen in ViewClosed() wieder entfernt werden.

                    EndProgressBar();
                });
            }

            SetVisibilityOfEditStateIndicationColumnInConfigurationTree();
            SetVisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree();
            SetVisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree();


            // Programmereignisskript OnNavigatedTo() feuern
            ProjectAndConfigurationModel.FireOnNavigatedTo(ProjectAndConfigurationModel.SelectedProposal, NavigationTargets.PlugIn, Name);


            base.OnNavigatedTo(navigationContext);
        }

        #endregion
        
        #region OnNavigatedFrom

        /// <summary>
        /// Der User hat gerade unser PlugIn verlassen und ein anderes Module geöffnet
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

        /// <summary>   Text auf dem Tabpage (unten) </summary>
        /// <remarks>   M Fries, 04.05.2021. </remarks>
        protected override void SetUpUI()
        {
            title = "TestTitel"; // displayed on page (bottom left)
        }

        #endregion

        #region SetUpRibbonViewModel

        protected override void SetUpRibbonViewModel()
        {
            base.SetUpRibbonViewModel();

            // Initialisierung des Ribbons für unser PlugIn
            var page = new PageModel { Name = "DemoPlugIn Page" };
            var category = new CategoryModel();
            RibbonViewModel.Categories.Add(category);
            category.Pages.Add(page);

            var doSomethingGroup = new PageGroupModel { Name = "Do Something Group" };
            page.Groups.Add(doSomethingGroup);

            AddCommand(doSomethingGroup, "Do Something", ExecuteDoSomething, largeGlyph: "pass_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Click here to do something", canExecuteDelegate: CanExecuteDoSomething);
            AddCommand(doSomethingGroup, "Do Something Else", ExecuteDoSomethingElse, smallGlyph: "fail_32x32.png", hint: "Click here to do something else", canExecuteDelegate: CanExecuteDoSomethingElse);
            AddCommand(doSomethingGroup, "Filter Elements", ExecuteDoFilterElements, smallGlyph: "find_32x32.png", hint: "Shows how to select some elements", canExecuteDelegate: CanExecuteDoFilterElements);
            AddCommand(doSomethingGroup, "Show BusiInicator", ExecuteShowBusiIndicator, smallGlyph: "process_32x32.png", hint: "Shows the busy indicator vor 5 seconds", canExecuteDelegate: CanExecuteShowBusiIndicator);
            AddCommand(doSomethingGroup, "Execute Script", ExecuteExecuteScript, largeGlyph: "ScriptingEditor_32x32.png", hint: "Executes the Script 'HelloWorld'", canExecuteDelegate: CanExecuteExecuteScript);
            AddCommand(doSomethingGroup, "Load Custom Definition Values", ExecuteGetCustomDefinitionsOfCompany2, largeGlyph: "find_32x32.png", hint: "Reads custom definition values of company2", canExecuteDelegate: CanExecuteGetCustomDefinitionsOfCompany2);
            AddCommand(doSomethingGroup, "Load global attributes", ExecuteLoadGlobalAttributes, largeGlyph: "find_32x32.png", hint: "Loads the global attributes", canExecuteDelegate: CanExecuteLoadGlobalAttributes);
            AddCommand(doSomethingGroup, "Get proposal custom definition values", ExecuteGetProposalCustomDefinitionValues, smallGlyph: "find_32x32.png", hint: "Reads custom definition values of the proposal", canExecuteDelegate: CanExecuteGetProposalCustomDefinitionValues);
            AddCommand(doSomethingGroup, "Set local attributes", ExecuteSetLocalAttributes, smallGlyph: "upgrade_32x32.png", hint: "Sets some local attributes", canExecuteDelegate: CanExecuteSetLocalAttributes);
            

            var proposalGroup = new PageGroupModel { Name = "Proposal Group" };
            page.Groups.Add(proposalGroup);

            AddCommand(proposalGroup, "Show Proposal Id", ExecuteShowProposalId, largeGlyph: "check_32x32.png", hint: "Shows the Id of the selected proposal", canExecuteDelegate: CanExecuteShowProposalId);
            AddCommand(proposalGroup, "Create Proposal", ExecuteCreateProposal, largeGlyph: "CreateProposal_32x32.png", hint: "Creates a new proposal", canExecuteDelegate: CanExecuteCreateProposal);
            AddCommand(proposalGroup, "Set Custom Property", ExecuteSetProposalCustomProperty, largeGlyph: "CustomProperty_32x32.png", hint: "Sets a custom definition value", canExecuteDelegate: CanExecuteSetProposalCustomProperty);



            _lockProposalToggleButtonCommand = new DXToggleButtonCommand(ExecuteToggleLockProposal, CanExecuteToggleLockProposal)
            {
                Caption = Translator.Translate("LockProposal"),
                LargeGlyph = GlyphHelper.GetGlyph("/Images/Ribbon/locked_32x32.png", this),
                Hint = Translator.Translate("LockProposalHint"),
                GroupIndex = 53,
                IsToggleChecked = false
            };
            proposalGroup.Commands.Add(_lockProposalToggleButtonCommand);



            var sqlGroup = new PageGroupModel { Name = "Database Group" };
            AddCommand(sqlGroup, "Select Something", ExecuteSelectSomething, largeGlyph: "sql_32x32.png", hint: "Demonstrates a database select", canExecuteDelegate: CanExecuteSelectSomething);
            page.Groups.Add(sqlGroup);

            
            var pricingGroup = new PageGroupModel { Name = "Pricing" };
            AddCommand(pricingGroup, "Read", ExecuteReadPricingField, smallGlyph: "Dollar_16x16.png", hint: "Demonstrates reading a pricing field", canExecuteDelegate: CanExecuteReadPricingField);
            AddCommand(pricingGroup, "Write", ExecuteWritePricingField, smallGlyph: "Dollar_16x16.png", hint: "Demonstrates writing a pricing field", canExecuteDelegate: CanExecuteWritePricingField);
            page.Groups.Add(pricingGroup);

            
            
            // 2nd page
            var dynamicDataPage = new PageModel { Name = "Dynamic Data" };
            category.Pages.Add(dynamicDataPage);
            
            var configurationItemGroup = new PageGroupModel { Name = "ConfigurationItem" };
            AddCommand(configurationItemGroup, "Insert", ExecuteInsertElement, largeGlyph: "Add_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Click here to do something", canExecuteDelegate: CanExecuteInsertElement);
            AddCommand(configurationItemGroup, "Update", ExecuteUpdateConfigurationItem, largeGlyph: "Edit_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Click here to do something", canExecuteDelegate: CanExecuteUpdateConfigurationItem);
            AddCommand(configurationItemGroup, "Delete", ExecuteDeleteConfigurationItem, largeGlyph: "Remove_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Click here to do something", canExecuteDelegate: CanExecuteDeleteConfigurationItem);
            AddCommand(configurationItemGroup, "Move", ExecuteMoveConfigurationItem, largeGlyph: "MoveDown_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Click here to do something", canExecuteDelegate: CanExecuteMoveConfigurationItem);
            AddCommand(configurationItemGroup, "Clone", ExecuteCloneConfigurationItem, largeGlyph: "Copy_32x32.png", keyGesture: new KeyGesture(Key.F2), hint: "Click here to do something", canExecuteDelegate: CanExecuteCloneConfigurationItem);
            dynamicDataPage.Groups.Add(configurationItemGroup);

            // local Attribute
            // global Attribute
            // Pricingfield
            
            // Document
            
            // Execute Script
            // Calculate
            
        }

        #endregion

        #endregion

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
