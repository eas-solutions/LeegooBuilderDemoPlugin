using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.DevExpressHelper;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Extensions;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Views.Helpers;
using EAS.LeegooBuilder.Client.GUI.Modules.MainModule.Models;
using EAS.LeegooBuilder.Client.ServerProxy.BusinessServiceClientBase;
using EAS.LeegooBuilder.Client.ServerProxy.BusinessServiceClientBase.MVVM;
using EAS.LeegooBuilder.Common.CommonTypes.Interfaces;
using EAS.LeegooBuilder.Server.DataAccess.Core;
using EAS.LeegooBuilder.Server.DataAccess.Core.Configuration;
using Prism.Events;
using Prism.Regions;
using GlyphHelper = EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule.Helpers.GlyphHelper;

namespace EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule.ViewModels
{
    public class ExecuteConfigurationTreeSmartUpdateEvent : PubSubEvent<object>
    {
    }

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class DemoPlugInViewModel : UserSettingsAwareClientViewModelBase
    {
        private DXToggleButtonCommand _lockProposalToggleButtonCommand;


        #region RibbonHelpers

        protected new CommandModel AddCommand(PageGroupModel pageGroup, string caption, Action action, string smallGlyph = null,
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

        #region TreeListControl-Stuff


        /// <summary>
        /// Ausgewähltes Configuration-TreeItem
        /// </summary>
        /// <remarks>
        /// <para>BR 04.10.2014: Prism 5-Umstellung: RaisePropertyChanged() -> OnPropertyChanged(value)</para>
        /// </remarks>
        public TreeStructureItem<ConfigurationItem> SelectedConfigurationTreeItem
        {
            get { return selectedConfigurationTreeItem; }
            set
            {
                if (selectedConfigurationTreeItem == value) return;

                selectedConfigurationTreeItem = value;
                this.OnPropertyChanged(() => this.SelectedConfigurationTreeItem);


                if (selectedConfigurationTreeItem == null)
                    return;

                if ((selectedConfigurationTreeItem.Value.LocalAttributes == null) ||
                    (selectedConfigurationTreeItem.Value.HasConfigurator && (selectedConfigurationTreeItem.Value.GlobalAttributes == null)))
                {
                    ProjectAndConfigurationModel.LoadAttributes(selectedConfigurationTreeItem);
                }
            }
        }

        private TreeStructureItem<ConfigurationItem> selectedConfigurationTreeItem;


        /// <summary>
        /// Steuert die Sichtbarkeit der Spalte 'EditStateIndication' in der Konfiguration (links)
        /// </summary>
        /// <remarks>
        /// <para>BR 04.10.2014: Prism 5-Umstellung: RaisePropertyChanged() -> SetProperty(ref, value)</para>
        /// </remarks>
        public bool VisibilityOfEditStateIndicationColumnInConfigurationTree
        {
            get { return this.visibilityOfEditStateIndicationColumnInConfigurationTree; }
            set { this.SetProperty(ref this.visibilityOfEditStateIndicationColumnInConfigurationTree, value); }
        }

        private bool visibilityOfEditStateIndicationColumnInConfigurationTree;


        private void SetVisibilityOfEditStateIndicationColumnInConfigurationTree()
        {
            if (ProjectAndConfigurationModel.SelectedProposal.Configuration != null)
                VisibilityOfEditStateIndicationColumnInConfigurationTree = ProjectAndConfigurationModel.SelectedProposal.Configuration.EditStateCount > 0;
        }


        /// <summary>
        /// Steuert die Sichtbarkeit der Spalte 'HasAnySpecializedDescription' in der Konfiguration (links)
        /// Wenn keine spezialisierten Benennungen existieren, wird die Spalte ausgeblendet.
        /// </summary>
        public bool VisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree
        {
            get { return this.visibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree; }
            set { this.SetProperty(ref this.visibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree, value); }
        }

        private bool visibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree;

        private void SetVisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree()
        {
            VisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree = ProjectAndConfigurationModel.SelectedProposal.Configuration != null && ProjectAndConfigurationModel.SelectedProposal.Configuration.FindInBreadth(x => x.HasAnySpecializedDescription) != null;
        }


        /// <summary>
        /// Steuert die Sichtbarkeit der Spalte 'HasAnySpecializedLongText' in der Konfiguration (links)
        /// Wenn keine spezialisierten Langtexte existieren, wird die Spalte ausgeblendet.
        /// </summary>
        public bool VisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree
        {
            get { return this.visibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree; }
            set { this.SetProperty(ref this.visibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree, value); }
        }

        private bool visibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree;

        private void SetVisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree()
        {
            VisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree = ProjectAndConfigurationModel.SelectedProposal != null && ProjectAndConfigurationModel.SelectedProposal.Configuration != null && ProjectAndConfigurationModel.SelectedProposal.Configuration.FindInBreadth(x => x.HasAnySpecializedLongText) != null;
        }


        /// <summary>
        /// Steuert die Sichtbarkeit der Spalte 'Anchor' in der Konfiguration (links)
        /// </summary>
        public bool VisibilityOfAnchorColumnInConfigurationTree
        {
            get { return this.visibilityOfAnchorColumnInConfigurationTree; }
            set { this.SetProperty(ref this.visibilityOfAnchorColumnInConfigurationTree, value); }
        }

        private bool visibilityOfAnchorColumnInConfigurationTree;


        #endregion TreeListControl-Stuff


        [ImportingConstructor]
        public DemoPlugInViewModel(IEventAggregator eventAggregator,
                                  ProjectAndConfigurationClientBase projectAndConfigurationModel,
                                  ITranslator translator,
                                  IUserSettingsService userSettingsService)
            : base(eventAggregator, projectAndConfigurationModel, translator, userSettingsService)
        {
            // Initialisierung des ViewModels
        }




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
                                                           StartProgressBar("LoadingConfiguration");
                                                           ProjectAndConfigurationModel.LoadConfiguration(ProjectAndConfigurationModel.SelectedProposal);
                                                           ProjectAndConfigurationModel.LoadCalculationDataForProposal(User.CurrentUser.CurrentCalculationSystemView.ViewName,
                                                               Translator.EAS_Language, User.CurrentUser.LBUser.UserID);

                                                           ProjectAndConfigurationModel.SelectedProposal.PropertyChanged += SelectedProposalOnPropertyChanged;
                                                           ProjectAndConfigurationModel.SelectedProposal.Configuration.Root.OnTreeItemChanged += RootOnOnTreeItemChanged;


                                                           EndProgressBar();
                                                       });
            }

            SetVisibilityOfEditStateIndicationColumnInConfigurationTree();
            SetVisibilityOfHasSpecializedDescriptionIndicationColumnInConfigurationTree();
            SetVisibilityOfHasSpecializedLongTextIndicationColumnInConfigurationTree();

            base.OnNavigatedTo(navigationContext);
        }


        /// <summary>
        /// Der User hat gerade unser PlugIn verlassen und ein anderes Module geöffnet
        /// </summary>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
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


        // Caption der Region (oben)
        public override string Caption
        {
            get
            {
                return "DemoPlugIn blablabla";
            }
        }

        // Text auf dem Tabpage (unten)
        protected override void SetUpUI()
        {
            title = "TestTitel";
        }


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
            AddCommand(doSomethingGroup, "Set local attributes", ExecuteSetLocalAttributes, smallGlyph: "upgrade_32x32.png", hint: "Sets some local attributesl", canExecuteDelegate: CanExecuteSetLocalAttributes);



            var proposalGroup = new PageGroupModel { Name = "Proposal Group" };
            page.Groups.Add(proposalGroup);

            AddCommand(proposalGroup, "Show Proposal Id", ExecuteShowProposalId, largeGlyph: "check_32x32.png", hint: "Shows the Id of the selected proposal", canExecuteDelegate: CanExecuteShowProposalId);



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
            page.Groups.Add(sqlGroup);

            AddCommand(sqlGroup, "Select Something", ExecuteSelectSomething, largeGlyph: "sql_32x32.png", hint: "Demonstrates a database select", canExecuteDelegate: CanExecuteSelectSomething);

        }


        /// <summary>
        /// Liste der Projekte
        /// Diese wird beispielhaft im View in einer ListBox angezeigt
        /// </summary>
        public List<string> ListOfProjects
        {
            get
            {
                if (this.listOfProjects == null)
                {
                    this.listOfProjects = new List<string>();

                    var projects = ProjectAndConfigurationModel.GetProjectInfos();
                    foreach (var project in projects)
                    {
                        this.listOfProjects.Add(project.Description);
                    }
                }
                return this.listOfProjects;
            }
        }

        private List<string> listOfProjects;


        #region IDisposable

        private bool disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Release managed resources.
                }
                // Release unmanaged resources.
                // Set large fields to null.
                // Call Dispose on your base class.
                disposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposable

    }
}
