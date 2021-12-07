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
        float[,] modificationMatrix;
        public Polygon()
        {
            points = new List<PointF>();
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

        // метод Добавление вершины
        public void Add(Point NewVertex)
        {
            points.Add(NewVertex);
        }

        public List<PointF> getPoints()
        {
            return points;
        }

        // метод Вывод закрашенного многоугольника с помощью g.FillPolygon
        // Вместо него здесь должен быть свой метод закрашивания из л.р. № 2 !
        public void Fill(Graphics g, Pen pen)
        {
            //Brush DrawBrush = new SolidBrush(DrawPen.Color);

            //g.FillPolygon(DrawBrush, VertexList.ToArray());
            if (points.Count > 2)
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
                        g.DrawLine(pen, Xb[i], Y, Xb[i + 1], Y);
                }
            }
        }

        private float getX(float Y, float x1, float y1, float x2, float y2)
        {
            return ((Y - y1) * (x2 - x1) / (y2 - y1)) + x1;
        }


        // выделение многоугольника
        public bool isInside(int mX, int mY)
        {
            List<PointF> points = getModificatedList();

            int n = points.Count() - 1, k = 0, m = 0;
            PointF Pi, Pk;
            bool check = false;
            for (int i = 0; i < points.Count(); i++)
            {
                if (i < n) k = i + 1; else k = 0;
                Pi = points[i]; Pk = points[k];
                if ((Pi.Y < mY) & (Pk.Y >= mY) | (Pi.Y >= mY) & (Pk.Y < mY))
                    if ((mY - Pi.Y) * (Pk.X - Pi.X) / (Pk.Y - Pi.Y) + Pi.X < mX) m++;
            }
            if (m % 2 == 1) check = true;
            return check;
        }

        // плоско-параллельное перемещение
        //Здесь должны быть реализованы все методы геометрических преобразований
        //в матричной форме !
        public void move(float dx, float dy)
        {
            addOperation(new float[,] { { 1, 0, 0},
                                         { 0, 1, 0},
                                         { dx, dy, 1} });
        }

        public void Clear()
        {
            points.Clear();
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

        public int pointsCount()
        {
            return points.Count();
        }


    }
}