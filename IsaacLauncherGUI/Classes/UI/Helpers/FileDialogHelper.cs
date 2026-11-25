using Microsoft.WindowsAPICodePack.Dialogs;

namespace IsaacLauncherGUI
{
    public static class FileDialogHelper
    {

        public static string SelectFolder(string title)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = title;

                return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
            }
        }

        public static string SelectFile(string title, string filterName, string filterPattern)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filters.Add(new CommonFileDialogFilter(filterName, filterPattern));

                return dialog.ShowDialog() == CommonFileDialogResult.Ok ? dialog.FileName : null;
            }
        }
    }
}
