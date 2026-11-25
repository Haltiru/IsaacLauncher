using System;
using System.Drawing;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;


    namespace IsaacLauncherGUI
    {
        public static class PopupManager
        {
            public static async Task<PopupUserControl.PopupChoice> ShowAsync(Form owner, string title, string message, string primaryText, string secondaryText = null, string tertiaryText = null, SystemSound sound = null)
            {
                TaskCompletionSource<PopupUserControl.PopupChoice> popupChoice = new TaskCompletionSource<PopupUserControl.PopupChoice>();
                PopupUserControl popup = new PopupUserControl();
                popup.Configure(title, message, primaryText, secondaryText, tertiaryText);
                popup.ChoiceSelected += choice => {popupChoice.TrySetResult(choice);};

                Panel modal = null;
                try
                {
                    modal = await CreateOverlayAsync(owner, popup);
                    if (sound != null) sound.Play();
                    return await popupChoice.Task;
                }
                finally {RemoveOverlay(modal);}
            }

            public static async Task<DialogResult> ShowUpdateDialogAsync(Form owner,string version)
            {
                UpdateUserControl updateUserControl = new UpdateUserControl(version);
                TaskCompletionSource<DialogResult> result = new TaskCompletionSource<DialogResult>();
                updateUserControl.ChoiceSelected += choice => result.TrySetResult(choice);

                Panel modal = await CreateOverlayAsync(owner, updateUserControl);

                try
                {
                    SystemSounds.Asterisk.Play();
                    return await result.Task;
                }
                finally { RemoveOverlay(modal); }
            
            }


            public static async Task ShowUpdateProgressDialogAsync(Form owner, Func<IProgress<int>, IProgress<string>, Task> operation)
            {
                bool isActive = true;
                UpdateProgressUserControl updateProgressUserControl = new UpdateProgressUserControl();
                Progress<int> progressReporter = new Progress<int>(percentage => { if (isActive) updateProgressUserControl.UpdateProgress(percentage); });
                Progress<string> statusReporter = new Progress<string>(status => { if (isActive) updateProgressUserControl.UpdateStatus(status); });

                Panel modal = await CreateOverlayAsync(owner, updateProgressUserControl);

                try { await operation(progressReporter, statusReporter); }
                finally
                {
                    isActive = false;
                    RemoveOverlay(modal);
                }
            }

            private static async Task<Panel> CreateOverlayAsync(Form owner, Control content)
            {

                Bitmap screenshot = null;
                Panel modal = null;
                try
                {
                    await Task.Delay(100);
                    screenshot = new Bitmap(owner.ClientSize.Width, owner.ClientSize.Height);
                    Point topLeft = owner.PointToScreen(Point.Empty);
                    using (Graphics g = Graphics.FromImage(screenshot))
                    {
                        g.CopyFromScreen(topLeft, Point.Empty, owner.ClientSize);
                        using (SolidBrush semiTransparentBrush = new SolidBrush(Color.FromArgb(170, 40, 30, 28))) g.FillRectangle(semiTransparentBrush, new Rectangle(Point.Empty, owner.ClientSize));
                    }

                    modal = new Panel();

                    modal.Dock = DockStyle.Fill;
                    modal.BackgroundImage = screenshot;
                    modal.TabStop = false;

                    owner.Controls.Add(modal);
                    modal.Controls.Add(content);

                    content.Location = new Point(owner.ClientSize.Width / 2 - content.Width / 2, owner.ClientSize.Height / 2 - content.Height / 2);

                    modal.BringToFront();
                    content.BringToFront();
                    return modal;
                }
                catch
                {
                    try
                    {
                        if (modal != null)
                        {
                            modal.BackgroundImage = null;
                            owner.Controls.Remove(modal);
                            modal.Dispose();
                        }
                    }
                    finally { if (screenshot != null) screenshot.Dispose(); }
                    throw;
                }
            }

            private static void RemoveOverlay(Panel modal)
            {
                if (modal == null) return;
                Image img = modal.BackgroundImage;
                try
                {
                    modal.Parent?.Controls.Remove(modal);
                    modal.BackgroundImage = null;
                    modal.Dispose();
                }
                finally { img?.Dispose(); }
            }

        }   
    }
