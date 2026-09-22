using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundButton : Button
{
    protected override void OnPaint(PaintEventArgs pevent)
    {
        Graphics g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle rectSurface = this.ClientRectangle;
        Rectangle rectBorder = Rectangle.Inflate(rectSurface, -1, -1);
        int borderRadius = 15; // Itt állítod, hogy mennyire legyen lekerekítve

        using (GraphicsPath pathSurface = GetFigurePath(rectSurface, borderRadius))
        using (SolidBrush brushSurface = new SolidBrush(this.BackColor))
        {
            this.Region = new Region(pathSurface);

            // Háttér kitöltése
            g.FillPath(brushSurface, pathSurface);
        }

        // Szöveg szépen középre igazítva
        TextRenderer.DrawText(g, this.Text, this.Font, rectSurface, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private GraphicsPath GetFigurePath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        float curveSize = radius * 2F;
        path.StartFigure();
        path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
        path.AddArc(rect.Right - curveSize, rect.Y, curveSize, curveSize, 270, 90);
        path.AddArc(rect.Right - curveSize, rect.Bottom - curveSize, curveSize, curveSize, 0, 90);
        path.AddArc(rect.X, rect.Bottom - curveSize, curveSize, curveSize, 90, 90);
        path.CloseFigure();
        return path;
    }
}