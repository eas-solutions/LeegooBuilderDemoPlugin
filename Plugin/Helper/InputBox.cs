
using EAS.DevExpressGenericDialogs.Dialogs;

namespace EAS.LeegooBuilder.Client.GUI.Modules.Plugin.Helper
{
    public static class InputBox
    {
        public static string Query(string labelText)
        {
            var dialog = new FormDialog("User input", string.Empty);
            dialog.Title = "User input";
        
            var text = dialog.AddTextbox(labelText);

            
            if (dialog.ShowDialog(System.Windows.MessageBoxButton.OKCancel) == System.Windows.MessageBoxResult.OK)
            {
                return text.Text;
            }

            return null;
        }
    }
}