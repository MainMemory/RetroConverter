using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RetroConverter
{
	public class SplitButton : Button
    {
        [DefaultValue(20), Browsable(true),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int SplitWidth { get; set; }

        public event EventHandler ShowMenu = delegate { };

        public SplitButton()
        {
            SplitWidth = 20;
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            var splitRect = new Rectangle(Width - SplitWidth, 0, SplitWidth, Height);

            // Figure out if the button click was on the button itself or the menu split
            if (mevent.Button == MouseButtons.Left &&
                splitRect.Contains(mevent.Location))
            {
                ShowMenu(this, EventArgs.Empty);
            }
            else
            {
                base.OnMouseDown(mevent);
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            if (SplitWidth > 0)
            {
                // Draw the arrow glyph on the right side of the button
                int arrowX = ClientRectangle.Width - 14;
                int arrowY = ClientRectangle.Height / 2 - 1;

                var arrowBrush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ButtonShadow;
                var arrows = new[] { new Point(arrowX, arrowY), new Point(arrowX + 7, arrowY), new Point(arrowX + 3, arrowY + 4) };
                pevent.Graphics.FillPolygon(arrowBrush, arrows);

                // Draw a dashed separator on the left of the arrow
                int lineX = ClientRectangle.Width - SplitWidth;
                int lineYFrom = arrowY - 4;
                int lineYTo = arrowY + 8;
                using (var separatorPen = new Pen(Brushes.DarkGray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                {
                    pevent.Graphics.DrawLine(separatorPen, lineX, lineYFrom, lineX, lineYTo);
                }
            }
        }
    }
}
