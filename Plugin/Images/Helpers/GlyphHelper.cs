using System.Windows.Media;

namespace Plugin.Images.Helpers
{
    /// <summary>   Hilfsklasse für Bilder und Icons. </summary>
    /// <remarks>   M Fries, 04.05.2021. </remarks>
    public class GlyphHelper
    {
        /// <summary>   Gets a glyph. </summary>
        /// <remarks>   M Fries, 04.05.2021. </remarks>
        /// <param name="itemPath"> Full pathname of the item file. </param>
        /// <param name="obj">      The object. </param>
        /// <returns>   The glyph. </returns>
        public static ImageSource GetGlyph(string itemPath, object obj)
        {
            string packUri = "pack://application:,,,/" + obj.GetType().Assembly.GetName() + ";component" + itemPath;
            return new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
        }

    }
}
