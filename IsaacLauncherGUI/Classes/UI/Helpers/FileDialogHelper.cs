namespace IsaacLauncherGUI
{
    public static class FileDialogHelper
    {
        public static string SelectFolder(string title)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = title;
                dialog.UseDescriptionForTitle = true;

                return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }

        public static string SelectFile(string title, string filterName, string filterPattern)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = $"{filterName} ({filterPattern})|{filterPattern}";

                return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
            }
        }
    }
}