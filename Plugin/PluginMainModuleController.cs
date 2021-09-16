using System;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.MVVM;
using EAS.LeegooBuilder.Client.GUI.Modules.Plugin.ViewModels;
using EAS.LeegooBuilder.ServiceClient;
using EAS.LeegooBuilder.Common.CommonTypes.EventTypes;
using EAS.LeegooBuilder.Common.CommonTypes.Models;
using EAS.LeegooBuilder.Server.DataAccess.Core;
using EAS.LeegooBuilder.Server.DataAccess.Core.Configuration;
using Plugin.Images.Helpers;
using PrismCompatibility;
using PrismCompatibility.ServiceLocator;

namespace EAS.LeegooBuilder.Client.GUI.Modules.Plugin
{
    public class PluginMainModuleController : ModuleControllerBase
    {
        #region Region

        private const string RegionName = "DemoPluginRegion";
        private IRegion PluginRegion { get; set; }
        protected override IRegion GetRegion(Type viewModelType) => viewModelType == typeof(PluginViewModel) ? PluginRegion : null;

        #endregion

        #region Constructors

        /// <summary>   Default constructor. </summary>
        /// <remarks>   M Fries, 04.05.2021. </remarks>
        public PluginMainModuleController()
        {
            PluginRegion = shellService.GetRegion(RegionName);
            RegisterNavBarItem();
        }

        #endregion

        #region RegisterNavBarItem

        /// <summary>   Registriert das Plugin als Button in der Navigationsleiste. </summary>
        /// <remarks>   M Fries, 04.05.2021. </remarks>
        private void RegisterNavBarItem()
        {
            // Position des NavigationBarItems innerhalb des Bereichs. 0 steht für ganz oben.
            // Diese Einstellung kann in Systemeinstellungen überschrieben werden
            const int position = 0;

            // Hier muss eine der drei möglichen Gruppen in der Navbar ausgewählt werden.
            var navBarItem = RegisterViewModel<PluginViewModel>(translator.Translate("Proposals"), position, GlyphHelper.GetGlyph("/Images/NavigationBar/plugin_32x32.png", this), "Demo Plugin");
            
            navBarItem.IsEnabled = false;

            // Beispiel: Das NavigationBarItem soll nur anwählbar sein, wenn ein Beleg angewählt ist
            var projectAndConfigurationModel = ServiceLocator.Current.GetInstance<ProjectAndConfigurationClient>();
            projectAndConfigurationModel.SelectedProposalChanged += (sender, e) =>
            {
                var isSelected = projectAndConfigurationModel.SelectedProposal != null;
                navBarItem.IsEnabled = isSelected;
            };

            projectAndConfigurationModel.DynamicDataSaving += DynamicDataSaving;
            projectAndConfigurationModel.DynamicDataSaved += DynamicDataSaved;
            projectAndConfigurationModel.SelectedProjectInfoChanged += SelectedProjectInfoChanged;
            projectAndConfigurationModel.SelectedConfigurationItemChanged += SelectedConfigurationItemChanged;

            projectAndConfigurationModel.ConfigurationItemDeleting += ProjectAndConfigurationModelOnConfigurationItemDeleting;
            projectAndConfigurationModel.ConfigurationItemAdded += ProjectAndConfigurationModelOnConfigurationItemAdded;
            projectAndConfigurationModel.ConfigurationItemReplacing += ProjectAndConfigurationModelOnConfigurationItemReplacing;
        }

        #endregion

        #region Event Handlers

        private void ProjectAndConfigurationModelOnConfigurationItemAdded(TreeStructureItem<ConfigurationItem> configurationItem)
        {

        }

        private void ProjectAndConfigurationModelOnConfigurationItemDeleting(TreeStructureItem<ConfigurationItem> configurationItem, out bool canDelete)
        {
            canDelete = true;
        }

        private void ProjectAndConfigurationModelOnConfigurationItemReplacing(TreeStructureItem<ConfigurationItem> configurationItemToBeInserted, TreeStructureItem<ConfigurationItem> configurationItemToBeReplaced, out bool canReplace)
        {
            canReplace = true;
        }


        /// <summary>
        /// Eventhandler: The user wants to save the dynamic data in LEEGOO BUILDER.NET
        /// You can prevent saving when setting e.Cancel to true. Don't forget to set 
        /// e.Message to inform the user about the reason.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DynamicDataSaving(object sender, SavingEventArgs e)
        {
            //e.Cancel = true;
            //e.Message = "Saving not possible, due to...";
        }


        /// <summary>
        /// Eventhandler: The user has saved the dynamic data in LEEGOO BUILDER.NET
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DynamicDataSaved(object sender, EventArgs e)
        {
            // Insert Code to save your dynamic data

        }

        /// <summary>
        /// Eventhandler: The user has selected another ProjectInfo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newProjectInfo"></param>
        private void SelectedProjectInfoChanged(object sender, ProjectInfo newProjectInfo)
        {
            //MessageBox.Show(string.Format("Selected project has changed. New project is {0}", newProjectInfo.Description));

        }

        /// <summary>
        /// Eventhandler: The user has selected another ConfigurationItem (in ConfigurationEditor or Configurator)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newConfigurationItem"></param>
        private void SelectedConfigurationItemChanged(object sender, TreeStructureItem<ConfigurationItem> newConfigurationItem)
        {
            //MessageBox.Show(string.Format("Selected configurationitem has changed. New configurationitem is {0}", newConfigurationItem.Value.Description));

        }

        #endregion
    }
}
