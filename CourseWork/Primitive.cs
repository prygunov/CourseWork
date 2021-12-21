using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;


namespace CourseWork
{
    internal class Primitive : IObject
    {

        private List<PointF> points = new List<PointF>();
        private Color color;
        private float[,] modificationMatrix;
        private int mode = 0; // режимы 0 - многоугольник, 1 - прямая, 2 - сплайн

        public Primitive(List<PointF> points, Color color)
        {
            this.points = points.ToList();
            if (points.Count == 2)
                mode = 1;
            this.color = color;
            reset();

        }
        public void setMode(int mode) {
            this.mode = mode;
        }

        public int getMode() {
            return mode;
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

        public void draw(Graphics g, Pen pen)
        {
            Color cache = pen.Color;
            pen.Color = color;

            List<PointF> points = getModificatedList();
            if (points.Count > 1)
                if (mode == 0)
                {
                    int yMin = Math.Max(getMinY(), 0);
                    int yMax = Math.Min(getMaxY(), 1080); ;

                    for (int Y = yMin; Y < yMax; Y++)
                    {
                        Borders borders = getBorders(Y);
                        List<float> left = borders.getLeft();
                        List<float> right = borders.getRight();
                        for (int i = 0; i < left.Count; i ++)
                            g.DrawLine(pen, left[i], Y, right[i], Y);
                    }
                }
                else if (mode == 1)
                {
                    g.DrawLine(pen, points[0], points[1]);
                }
                else if (mode == 2) {
                    Pen arrowPen = new Pen(pen.Color, 1);
                    arrowPen.CustomEndCap = new AdjustableArrowCap(6, 6);
                    g.DrawLine(arrowPen, points[0], points[1]);
                    g.DrawLine(arrowPen, points[2], points[3]);

                    List<PointF> p = getSplinePoints(points);
                    
                    for(int i = 1; i<p.Count; i++)
                    {
                        g.DrawLine(pen, p[i-1], p[i]);
                    }
                }
            pen.Color = cache;
        }

        List<PointF> getSplinePoints(List<PointF> points) {
            const float dt = 0.001f;

            List<PointF> response = new List<PointF>();
            PointF[] L = new PointF[4]; // Матрица вещественных коэффициентов
            List<PointF> P = points;
            PointF Pv1 = P[0];
            PointF Pv2 = P[0];

            float t = 0;
            float xt, yt;
            response.Add(P[0]);
            // Касательные векторы
            Pv1.X = 4 * (P[1].X - P[0].X);
            Pv1.Y = 4 * (P[1].Y - P[0].Y);
            Pv2.X = 4 * (P[3].X - P[2].X);
            Pv2.Y = 4 * (P[3].Y - P[2].Y);
            // Коэффициенты полинома
            L[0].X = 2 * P[0].X - 2 * P[2].X + Pv1.X + Pv2.X; // Ax
            L[0].Y = 2 * P[0].Y - 2 * P[2].Y + Pv1.Y + Pv2.Y; // Ay
            L[1].X = -3 * P[0].X + 3 * P[2].X - 2 * Pv1.X - Pv2.X; // Bx
            L[1].Y = -3 * P[0].Y + 3 * P[2].Y - 2 * Pv1.Y - Pv2.Y; // By
            L[2].X = Pv1.X; // Cx
            L[2].Y = Pv1.Y; // Cy
            L[3].X = P[0].X; // Dx
            L[3].Y = P[0].Y; // Dy
            double until = 1 + dt / 2;

            while (t < until)
            {
                xt = ((L[0].X * t + L[1].X) * t + L[2].X) * t + L[3].X;
                yt = ((L[0].Y * t + L[1].Y) * t + L[2].Y) * t + L[3].Y;
                response.Add(new PointF(xt, yt));
                t = t + dt;
            }
            return response;
        }

        public int getMinY()
        {
            List<PointF> points = getModificatedList();
            int minY = (int)points[0].Y;
            foreach (PointF p in points)
            {
                if (p.Y < minY)
                    minY = (int)p.Y;
            }
            return minY;
        }

        public int getMaxY()
        {
            List<PointF> points = getModificatedList();
            int maxY = (int)points[0].Y;
            foreach (PointF p in points)
            {
                if (p.Y > maxY)
                    maxY = (int)p.Y;
            }
            return maxY;
        }

        public int getMaxYIndex()
        {
            List<PointF> points = getModificatedList();
            int maxY = 0;
            for (int i = 1; i<points.Count; i++)
            {
                if (points[i].Y > points[maxY].Y)
                    maxY = i;
            }
            return maxY;
        }

        // проверка, точка внутри примитива или нет (захват примитива)
        public bool isInside(int x, int y)
        {
            List<PointF> points = getModificatedList();
            if (points.Count == 2)
                if ((y > points[0].Y && y < points[1].Y) || (y < points[0].Y && y > points[1].Y))
                    return nearlyEqual(getX(y, points[0].X, points[0].Y, points[1].X, points[1].Y), x, 5);
                else return false;
            else if (mode == 2)
            {
                //проверка захвата сплайна
                if (((y > points[0].Y && y < points[1].Y) || (y < points[0].Y && y > points[1].Y)) ||
                    ((y > points[2].Y && y < points[3].Y) || (y < points[2].Y && y > points[3].Y)))
                    if (nearlyEqual(getX(y, points[0].X, points[0].Y, points[1].X, points[1].Y), x, 5) ||
                       nearlyEqual(getX(y, points[2].X, points[2].Y, points[3].X, points[3].Y), x, 5))
                        return true; // захват двух векторов
                
                List<PointF> p = getSplinePoints(points);

                for (int i = 1; i < p.Count; i++)
                {
                    if (((y > p[i-1].Y && y < p[i].Y) || (y < p[i-1].Y && y > p[i].Y)))
                       if (nearlyEqual(getX(y, p[i - 1].X, p[i - 1].Y, p[i].X, p[i].Y), x, 5))
                        return true; // захват кривой
                }
                return false; // захвата нет
            }

            Borders borders = getBorders(y);

            for (int i = 0; i < borders.getLeft().Count; i++) {
                if (borders.getLeft()[i] <= x && borders.getRight()[i] >= x)
                    return true;
            }
            return false; 
        }

        public static bool nearlyEqual(float a, float b, float epsilon)
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

        public void rotate(float rad, PointF origin)
        {
            move(-origin.X, -origin.Y);

            float sin = (float)Math.Sin(rad);
            float cos = (float)Math.Cos(rad);

            addOperation(new float[,] { { cos, sin, 0},
                                        { -sin, cos, 0},
                                        { 0, 0, 1} });

            move(origin.X, origin.Y);
        }
        public void scale(float s, PointF origin, int type)
        {
            move(-origin.X, -origin.Y);
            if (type == 0)
                addOperation(new float[,] { { s, 0, 0},
                                            { 0, 1, 0},
                                            { 0, 0, 1} });
            else if (type == 1)
                addOperation(new float[,] { { 1, 0, 0},
                                            { 0, s, 0},
                                            { 0, 0, 1} });
            else
                addOperation(new float[,] { { s, 0, 0},
                                            { 0, s, 0},
                                            { 0, 0, 1} });

            move(origin.X, origin.Y);
        }

        private int lastHashCode = 0;
        private List<PointF> modPoints = new List<PointF>();
        private List<PointF> getModificatedList()
        {
            List<PointF> points = new List<PointF>();

            //ускорение за счет кеширования модифицированных точек
            if (lastHashCode != modificationMatrix.GetHashCode()) // если поменялся хешкод матрицы - модификации
            {
                lastHashCode = modificationMatrix.GetHashCode();
                // вычисляем новые точки
                float[,] polygonMatrix = multiplyMatrix(getAsMatrix(), modificationMatrix);

                for (int i = 0; i < this.points.Count; i++)
                {
                    points.Add(new PointF(polygonMatrix[i, 0], polygonMatrix[i, 1]));
                }
                modPoints = points;
                return points;
            } else return modPoints; // иначе возвращаем просчитанные ранее точки
        }
        public PointF getCenter()
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

        public Borders getBorders(int Y)
        {
            List<float> Xl = new List<float>();
            List<float> Xr = new List<float>();

            List<PointF> points = getModificatedList();
            bool cw = orientation();
            for (int i = 0; i < points.Count; i++)
            {
                int k = 0;
                if (i < points.Count - 1)
                    k = i + 1;

                if (((points[i].Y < Y) && (points[k].Y >= Y)) ||
                    ((points[i].Y >= Y) && (points[k].Y < Y)))
                {
                    float x = Primitive.getX(Y, points[i].X, points[i].Y, points[k].X, points[k].Y);
                    // в зависимости от направления стороны могут меняться,
                    // необходимо учитывать направление перебора вершин
                    if (!cw)
                    {
                        if (points[k].Y < points[i].Y)
                            Xr.Add(x); //движимся вниз
                        else
                            Xl.Add(x);
                    }
                    else
                    {
                        if (points[k].Y < points[i].Y)
                            Xl.Add(x);
                        else
                            Xr.Add(x); //движимся вверх
                    }
                }
            }

            Xl.Sort();
            Xr.Sort();

            return new Borders(Xl, Xr);
        }
        public static float getX(float Y, float x1, float y1, float x2, float y2)
        {
            return ((Y - y1) * (x2 - x1) / (y2 - y1)) + x1;
        }
        private bool orientation()
        {
            int maxIndex = getMaxYIndex();
            int prevTriangle = maxIndex - 1;
            int nextTriangle = maxIndex + 1;

            if (prevTriangle == -1)
                prevTriangle = points.Count - 1;
            if (nextTriangle == points.Count)
                nextTriangle = 0;

            float s = getSquare(points[prevTriangle].X, points[prevTriangle].Y, points[maxIndex].X, points[maxIndex].Y, points[nextTriangle].X, points[nextTriangle].Y);
            // по часовой площадь отрицательная

            return s < 0; // cw значит "по часовой"
        }
        private float getSquare(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            return (-x1 * y2 - x2 * y3 - x3 * y1 + y1 * x2 + y2 * x3 + y3 * x1) / 2;
        }
    }
}