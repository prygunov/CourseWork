using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CourseWork
{
    interface IObject
    {
        public int getMinY();
        public int getMaxY();
        public Borders getBorders(int y);

        public void move(float dx, float dy);

        public void rotate(float angle, PointF relatePoint);

        public void scale(float s, PointF relatePoint, int type);

        public PointF getCenter();

        private void drawNumber(Graphics g, string text)
        {
            PointF origin = getCenter();
            Font drawFont = new Font("Arial", 16);
            SolidBrush drawBrush = new SolidBrush(Color.Chocolate);
            g.DrawString(text, drawFont, drawBrush, origin);
        }

        public void reset();
        public void Fill(Graphics g, Pen pen);
        public void Fill(Graphics g, Pen pen, string v) {
            Fill(g, pen);
            drawNumber(g, v);
        }
        public bool isInside(int x, int y);
    }
}
