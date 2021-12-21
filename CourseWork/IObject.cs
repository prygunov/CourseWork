using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CourseWork
{
    interface IObject
    {
        //строки для обработки
        public int getMinY(); // нижняя строка
        public int getMaxY(); // верхняя строка
        
        public Borders getBorders(int y); //пары левых и правых границ для строки

        // реализация геометрических преобразований
        public void move(float dx, float dy); 
        public void rotate(float angle, PointF relatePoint);
        public void scale(float s, PointF relatePoint, int type);
        public void reset();

        public PointF getCenter(); // центр объекта

        private void drawString(Graphics g, string text) // подпись объекта
        {
            PointF origin = getCenter();
            Font drawFont = new Font("Arial", 16);
            SolidBrush drawBrush = new SolidBrush(Color.Chocolate);
            g.DrawString(text, drawFont, drawBrush, origin);
        }

        // обычная отрисовка
        public void draw(Graphics g, Pen pen); 
        // отрисовка с подписью
        public void draw(Graphics g, Pen pen, string v) { 
            draw(g, pen);
            drawString(g, v);
        }
        public bool isInside(int x, int y); // находится ли точка внутри объекта, нужно для захвата
    }
}
