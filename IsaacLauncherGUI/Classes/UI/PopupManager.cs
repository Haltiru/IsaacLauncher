using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;


    namespace IsaacLauncherGUI
    {
        public static class PopupManager
        {
            public static Task<PopupUserControl.PopupChoice> ShowAsync(Form owner,string title,string message,string primaryText,string secondaryText = null,string tertiaryText = null, SystemSound sound = null)
            {
            TaskCompletionSource<PopupUserControl.PopupChoice> popupChoice = new TaskCompletionSource<PopupUserControl.PopupChoice>();
            PopupUserControl popup = new PopupUserControl();

            Bitmap screenshot = new Bitmap(owner.ClientSize.Width, owner.ClientSize.Height);

            using (Graphics g = Graphics.FromImage(screenshot))
            {
                Point topLeft = owner.PointToScreen(Point.Empty);
                g.CopyFromScreen(topLeft, Point.Empty, owner.ClientSize);

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(170, 40, 30, 28))) g.FillRectangle(brush, new Rectangle(Point.Empty, owner.ClientSize));
            }

            Panel overlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackgroundImage = screenshot
            };

            owner.Controls.Add(overlay);
            overlay.Controls.Add(popup);
            popup.Configure(title, message, primaryText, secondaryText, tertiaryText);

            int x = (overlay.ClientSize.Width - popup.Width) / 2;
            int y = (overlay.ClientSize.Height - popup.Height) / 2;
            popup.Location = new Point(x, y);

            overlay.BringToFront();
            if (sound != null) sound.Play();

            popup.ChoiceSelected += choice =>
            {
                owner.Controls.Remove(overlay);

                overlay.BackgroundImage = null;
                overlay.Dispose();
                screenshot.Dispose();

                popupChoice.TrySetResult(choice);
            };
            return popupChoice.Task;

            }
        }
    }
