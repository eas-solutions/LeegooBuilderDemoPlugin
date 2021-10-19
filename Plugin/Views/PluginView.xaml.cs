using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using EAS.LeegooBuilder.Common.CommonTypes.EventTypes;
using PrismCompatibility;
using PrismCompatibility.ServiceLocator;

namespace EAS.LeegooBuilder.Client.GUI.Modules.Plugin.Views
{
    /// <summary>
    /// Code behind of PluginView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class PluginView : UserControl
    {
        private IDisposable _configurationTreeSmartUpdateEvent;
        public PluginView()
        {
            InitializeComponent();

            /*this.Loaded += ConfigurationEditorView_Loaded;
            this.Unloaded += ConfigurationEditorView_Unloaded;*/
            ConfigurationEditorView_Loaded(this, null);
        }

        ~PluginView()
        {
            ConfigurationEditorView_Unloaded(this, null);
        }


        void ConfigurationEditorView_Unloaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _configurationTreeSmartUpdateEvent.Dispose();
        }

        void ConfigurationEditorView_Loaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _configurationTreeSmartUpdateEvent = eventAggregator.GetEvent<ExecuteConfigurationTreeSmartUpdateEvent>().Subscribe(ExecuteConfigurationTreeSmartUpdate);
        }

        private void ExecuteConfigurationTreeSmartUpdate(object p)
        {
            if (Application.Current.Dispatcher.CheckAccess()) ConfigurationTreeList.RefreshData();
            else Application.Current.Dispatcher.Invoke(new Action(() => { ConfigurationTreeList.RefreshData(); }));
        }

    }
}
