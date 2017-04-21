using System;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.MVVM;
using EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule.Helpers;
using EAS.LeegooBuilder.Client.ServerProxy.BusinessServiceClientBase;
using EAS.LeegooBuilder.Common.CommonTypes.EventTypes;
using EAS.LeegooBuilder.Common.CommonTypes.Models;
using EAS.LeegooBuilder.Server.DataAccess.Core;
using EAS.LeegooBuilder.Server.DataAccess.Core.Configuration;
using Microsoft.Practices.ServiceLocation;
using Prism.Regions;
using DemoPlugInViewModel = EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule.ViewModels.DemoPlugInViewModel;

namespace EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule
{
    public class DemoPlugInMainModuleController : ModuleControllerBase
    {
        private const string DemoPlugInRegionName = "DemoPlugInRegion";

        private IRegion DemoPlugInRegion { get; set; }

        /// <summary>
        /// Konstrutkor
        /// </summary>
        public DemoPlugInMainModuleController()
        {
            this.RegisterNavBarItem();
            this.InitRegion();
        }


        /// <summary>
        /// Registriert das zugehörige NavBarItem im Bereich "Proposals"
        /// </summary>
        private void RegisterNavBarItem()
        {
            // Position des NavigationBarItems innerhalb des Bereichs. 0 steht für ganz oben.
            const int position = 3;

            var demoPlugInNavigationBarItem = RegisterViewModel<DemoPlugInViewModel>(translator.Translate("Proposals"), position, GlyphHelper.GetGlyph("/Images/NavigationBar/plugin_32x32.png", this));
            //var demoPlugInNavigationBarItem = RegisterViewModel<DemoPlugInViewModel>(translator.Translate("ProductAdministration"), position, DemoPlugIn.Helpers.GlyphHelper.GetGlyph("/Images/NavigationBar/plugin_32x32.png", this));
            //var demoPlugInNavigationBarItem = RegisterViewModel<DemoPlugInViewModel>(translator.Translate("SystemAdministration"), position, DemoPlugIn.Helpers.GlyphHelper.GetGlyph("/Images/NavigationBar/plugin_32x32.png", this));
            //var demoPlugInNavigationBarItem = RegisterViewModel<DemoPlugInViewModel>(translator.Translate("SystemAdministration"), position, GlyphHelper.GetGlyph("/Images/NavigationBar/plugin_32x32.png", this), "Demo Plugin");

            demoPlugInNavigationBarItem.IsEnabled = false;

            // Beispiel: Das NavigationBarItem soll nur anwählbar sein, wenn ein Beleg angewählt ist
            var projectAndConfigurationModel = ServiceLocator.Current.GetInstance<ProjectAndConfigurationClientBase>();
            projectAndConfigurationModel.SelectedProposalChanged +=
            (sender, e) =>
            {
                bool bSelProposal = projectAndConfigurationModel.SelectedProposal != null;
                demoPlugInNavigationBarItem.IsEnabled = bSelProposal;
            };


            projectAndConfigurationModel.DynamicDataSaving += DynamicDataSaving;
            projectAndConfigurationModel.DynamicDataSaved += DynamicDataSaved;
            projectAndConfigurationModel.SelectedProjectInfoChanged += SelectedProjectInfoChanged;
            projectAndConfigurationModel.SelectedConfigurationItemChanged += SelectedConfigurationItemChanged;

            projectAndConfigurationModel.ConfigurationItemDeleting += ProjectAndConfigurationModelOnConfigurationItemDeleting;
            projectAndConfigurationModel.ConfigurationItemAdded += ProjectAndConfigurationModelOnConfigurationItemAdded;
            projectAndConfigurationModel.ConfigurationItemReplacing += ProjectAndConfigurationModelOnConfigurationItemReplacing;
        }

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




        /// <summary>
        /// Initialisiert die Region für das PlugIn
        /// </summary>
        private void InitRegion()
        {
            DemoPlugInRegion = shellService.GetRegion(DemoPlugInRegionName);
        }

        /// <summary>
        /// Liefert die zu diesem PlugIn gehörende Region zurück
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns></returns>
        protected override IRegion GetRegion(Type viewModelType)
        {
            if (viewModelType == typeof(DemoPlugInViewModel))
            {
                return DemoPlugInRegion;
            }

            return null;
        }
    }
}
