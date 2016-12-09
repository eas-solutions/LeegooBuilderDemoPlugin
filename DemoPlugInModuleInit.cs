using System.ComponentModel.Composition;
using EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule;
using EAS.LeegooBuilder.Common.CommonTypes.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Prism.Mef.Modularity;
using Prism.Modularity;

namespace EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule
{
    [ModuleExport("DemoPlugInModule", typeof(DemoPlugInModuleInit))]
    public class DemoPlugInModuleInit : IModule
    {
        private readonly IServiceLocator serviceLocator;
        private IShellService shellService;
        private DemoPlugInMainModuleController demoPlugInModuleController;

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="serviceLocator">-</param>
        [ImportingConstructor]
        public DemoPlugInModuleInit(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        #region IModule Members

        /// <summary>
        /// Modulinitialisierung
        /// </summary>
        public void Initialize()
        {
            shellService = serviceLocator.GetInstance<IShellService>();
            demoPlugInModuleController = new DemoPlugInMainModuleController();
        }

        #endregion
    }
}
