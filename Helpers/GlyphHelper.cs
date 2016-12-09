using System.Windows.Media;

namespace EAS.LeegooBuilder.Client.GUI.Modules.DemoPluginModule.Helpers
{
    public class GlyphHelper
    {
        public static ImageSource GetGlyph(string itemPath, object obj)
        {
            string packUri = "pack://application:,,,/" + obj.GetType().Assembly.GetName() + ";component" + itemPath;
            return new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

    }
}
