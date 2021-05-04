using System.ComponentModel.Composition;
using PrismCompatibility.Module;
using PrismCompatibility.ServiceLocator;

namespace EAS.LeegooBuilder.Client.GUI.Modules.Plugin
{
    public class PluginModuleInit : IModule
    {
        private readonly IServiceLocator _serviceLocator;
        private PluginMainModuleController _pluginMainModuleController;

        [ImportingConstructor]
        public PluginModuleInit(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public void Initialize()
        {
            _pluginMainModuleController = new PluginMainModuleController();
        }
    }
}
