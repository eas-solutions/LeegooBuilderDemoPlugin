using System.ComponentModel.Composition;
using EAS.LeegooBuilder.Common.CommonTypes.Interfaces;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.ServiceLocation;

namespace DemoPlugIn
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
