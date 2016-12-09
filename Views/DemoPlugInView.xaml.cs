using System.Windows;
using EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule.ViewModels;
using EAS.LeegooBuilder.Client.ServerProxy.BusinessServiceClientBase.MVVM;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;

namespace EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule.Views
{
    /// <summary>
    /// Interaktionslogik für DemoPlugInView.xaml
    /// </summary>
    public partial class DemoPlugInView : UserSettingsAwareView
    {
        public DemoPlugInView()
        {
            InitializeComponent();

            this.Loaded += ConfigurationEditorView_Loaded;
            this.Unloaded += ConfigurationEditorView_Unloaded;

        }

        void ConfigurationEditorView_Unloaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<ExecuteConfigurationTreeSmartUpdateEvent>().Unsubscribe(ExecuteConfigurationTreeSmartUpdate);

        }

        void ConfigurationEditorView_Loaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<ExecuteConfigurationTreeSmartUpdateEvent>().Subscribe(ExecuteConfigurationTreeSmartUpdate);
        }

        private void ExecuteConfigurationTreeSmartUpdate(object p)
        {
            // Baum aktualisieren
            ConfigurationTreeList.BeginDataUpdate();
            ConfigurationTreeList.RefreshData();
            ConfigurationTreeList.EndDataUpdate();

            /*var treeItem = p as TreeStructureItem<ConfigurationItem>;
            if (treeItem != null)
            {
                ConfigurationTreeList.SelectedItem = treeItem;
            }

            // Aktuelle Node aufklappen
            var focusedNode = ConfigurationTreeList.GetSelectedNodes();
            if (focusedNode.Length > 0) focusedNode[0].ExpandAll();*/
        }

    }
}
