using System.Windows;
using System.Windows.Controls;
using DemoPlugIn.ViewModels;
using EAS.LeegooBuilder.Server.DataAccess.Core;
using EAS.LeegooBuilder.Server.DataAccess.Core.Configuration;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.ServiceLocation;

namespace DemoPlugIn.Views
{
    using EAS.LeegooBuilder.Client.ServerProxy.BusinessServiceClientBase.MVVM;

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
