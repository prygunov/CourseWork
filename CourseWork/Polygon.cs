using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CourseWork
{
    internal class Polygon
    {

        List<PointF> points = new List<PointF>();
        Color color;
        float[,] modificationMatrix;
        public Polygon(Color color)
        {
            points = new List<PointF>();
            reset();
        }

        public Polygon(List<PointF> points, Color color)
        {
            this.points = points.ToList();
            this.color = color;
            reset();
        }

        void addOperation(float[,] matrix)
        {
            modificationMatrix = multiplyMatrix(modificationMatrix, matrix);
        }

        // представляет координаты вершин в виде матрицы
        public float[,] getAsMatrix()
        {
            float[,] polygonMatrix = new float[points.Count, 3]; // 3 столбца
            for (int i = 0; i < points.Count; i++)
            {
                polygonMatrix[i, 0] = points[i].X;
                polygonMatrix[i, 1] = points[i].Y;
                polygonMatrix[i, 2] = 1;
            }
            return polygonMatrix;
        }


        public void reset()
        {
            modificationMatrix = new float[,] { { 1,0,0},
                                                { 0,1,0},
                                                { 0,0,1} };
        }

        float[,] multiplyMatrix(float[,] first, float[,] second)
        {
            int rows = first.GetLength(0);
            int cols = first.GetLength(1);
            float[,] result = new float[rows, cols];
            for (int firstRow = 0; firstRow < first.Length / 3; firstRow++)
            {
                for (int secondColumn = 0; secondColumn < 3; secondColumn++)
                {
                    int secondRow = 0;
                    float num = 0f;
                    for (int firstColumn = 0; firstColumn < 3; firstColumn++)
                    {
                        num += first[firstRow, firstColumn] * second[secondRow, secondColumn];
                        secondRow++;
                    }
                    result[firstRow, secondColumn] = num;
                }

            }

            return result;
        }

        public List<PointF> getPoints()
        {
            return points;
        }

        // метод Вывод закрашенного многоугольника с помощью g.FillPolygon
        // Вместо него здесь должен быть свой метод закрашивания из л.р. № 2 !
        public void Fill(Graphics g, Pen pen)
        {
            Color cache = pen.Color;
            pen.Color = color;

            if (points.Count > 1)
            {
                
                List<PointF> points = getModificatedList();

                int yMin = (int)points[0].Y;
                int yMax = (int)points[0].Y;

                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].Y < yMin)
                    {
                        yMin = (int)points[i].Y;
                    }
                    if (points[i].Y > yMax)
                    {
                        yMax = (int)points[i].Y;
                    }
                }
                yMin = Math.Max(yMin, 0);
                yMax = Math.Min(yMax, 1000);

                for (int Y = yMin; Y < yMax; Y++)
                {
                    List<float> Xb = new List<float>();
                    for (int i = 0; i < points.Count; i++)
                    {
                        int k = 0;
                        if (i < points.Count - 1)
                            k = i + 1;

                        if (((points[i].Y < Y) && (points[k].Y >= Y)) ||
                            ((points[i].Y >= Y) && (points[k].Y < Y)))
                        {
                            Xb.Add(getX(Y, points[i].X, points[i].Y, points[k].X, points[k].Y));
                        }
                    }
                    Xb.Sort();
                    for (int i = 0; i < Xb.Count; i += 2)
                        if(Xb[i] == Xb[i + 1])
                            g.DrawRectangle(pen, Xb[i], Y, 1, 1);
                        else
                        {
                            g.DrawLine(pen, Xb[i], Y, Xb[i + 1], Y);
                        }
                    
                }
            }
            pen.Color = cache;
        }

        public void Fill(Graphics g, Pen pen, string text)
        {
            Fill(g, pen);
            drawNumber(g, text);
        }

        private float getX(float Y, float x1, float y1, float x2, float y2)
        {
            return ((Y - y1) * (x2 - x1) / (y2 - y1)) + x1;
        }

        void drawNumber(Graphics g, string text) {
            PointF origin = centralPoint();
            Font drawFont = new Font("Arial", 16);
            SolidBrush drawBrush = new SolidBrush(Color.Chocolate);
            g.DrawString(text, drawFont, drawBrush, origin);
        }

        // выделение многоугольника
        public bool isInside(int x, int y)
        {
            List<PointF> points = getModificatedList();
            if (points.Count == 2)
                return nearlyEqual(getX(y, points[0].X, points[0].Y, points[1].X, points[1].Y), x, 2);
            int n = points.Count() - 1, k, m = 0;
            PointF Pi, Pk;
            for (int i = 0; i < points.Count(); i++)
            {
                if (i < n) k = i + 1; else k = 0;
                Pi = points[i]; Pk = points[k];
                if ((Pi.Y < y) & (Pk.Y >= y) | (Pi.Y >= y) & (Pk.Y < y)) {
                    float mx = getX(y, Pi.X, Pi.Y, Pk.X, Pk.Y);
                    if (mx < x) m++;
                }
                
            }
            return m % 2 == 1;   
        }

        public bool nearlyEqual(float a, float b, float epsilon)
        {
            return Math.Abs(a - b) < epsilon;     
        }

        // плоско-параллельное перемещение
        public void move(float dx, float dy)
        {
            addOperation(new float[,] { { 1, 0, 0},
                                         { 0, 1, 0},
                                         { dx, dy, 1} });
        }

        public void rotate(float angle)
        {
            PointF origin = centralPoint();
            move(-origin.X, -origin.Y);

            float sin = (float)Math.Sin(angle);
            float cos = (float)Math.Cos(angle);

            addOperation(new float[,] { { cos, sin, 0},
                                        { -sin, cos, 0},
                                        { 0, 0, 1} });

            move(origin.X, origin.Y);
        }


        List<PointF> getModificatedList()
        {
            List<PointF> points = new List<PointF>();

            float[,] polygonMatrix = getAsMatrix();
            polygonMatrix = multiplyMatrix(polygonMatrix, modificationMatrix);

            for (int i = 0; i < this.points.Count; i++)
            {
                points.Add(new PointF(polygonMatrix[i, 0], polygonMatrix[i, 1]));
            }
            return points;
        }
        PointF centralPoint()
        {
            List<PointF> points = getModificatedList();

            float x = 0;
            float y = 0;
            foreach (PointF point in points)
            {
                x += point.X;
                y += point.Y;
            }
            return new PointF(x / points.Count, y / points.Count);
        }

        public void scale(float s)
        {
            PointF origin = centralPoint();
            move(-origin.X, -origin.Y);

            addOperation(new float[,] { { s, 0, 0},
                                        { 0, s, 0},
                                        { 0, 0, 1} });

            move(origin.X, origin.Y);
        }

    }
}