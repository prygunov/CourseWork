using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CourseWork
{
    class Bunch : IObject
    {
        IObject first;
        IObject second;

        int typeOperation;

        public Bunch(IObject first, IObject second, int type) {
            this.first = first;
            this.second = second;
            typeOperation = type;
        }

        public void draw(Graphics g, Pen pen)
        {
            int minY = first.getMinY();
            if (minY > second.getMinY())
                minY = second.getMinY();

            int maxY = second.getMaxY();
            if (maxY < first.getMaxY())
                maxY = first.getMaxY();

            for (int Y = minY + 1; Y < maxY; Y++)
            {
                Borders borders = getBorders(Y);

                List<float> Xrl = borders.getLeft();
                List<float> Xrr = borders.getRight();

                // отрисовка
                for (int i = 0; i < Xrr.Count; i++)
                {
                    if (Xrl[i] < Xrr[i])
                        g.DrawLine(pen, Xrl[i], Y, Xrr[i], Y);
                }
            }
        }

        public Borders getBorders(int Y)
        {
            List<float> Xal = first.getBorders(Y).getLeft();
            List<float> Xar = first.getBorders(Y).getRight();
            List<float> Xbl = second.getBorders(Y).getLeft();
            List<float> Xbr = second.getBorders(Y).getRight();


            List<float[]> M = new List<float[]>();
            for (int i = 0; i < Xal.Count; i++)
                M.Add(new float[] { Xal[i], 2 });

            for (int i = 0; i < Xar.Count; i++)
                M.Add(new float[] { Xar[i], -2 });

            for (int i = 0; i < Xbl.Count; i++)
                M.Add(new float[] { Xbl[i], 1 });

            for (int i = 0; i < Xbr.Count; i++)
                M.Add(new float[] { Xbr[i], -1 });
            // заполнение рабочего массива


            sortM(M);

            float Q = 0;

            int[] setQ;

            // весы для расчета Q
            switch (typeOperation)
            {
                case 1: setQ = new int[] { 1, 2 }; break; // пересечение           
                default:
                   setQ = new int[] { 3, 3 }; break; // симметрическая разность
            }

            List<float> Xrl = new List<float>();
            List<float> Xrr = new List<float>();

            for (int i = 0; i < M.Count; i++)
            {
                float nQ = Q + M[i][1]; // определение суммы и проверка на соответствие весам
                                        // добавление необходимых отрезков
                if ((Q < setQ[0] || Q > setQ[1]) && nQ >= setQ[0] && nQ <= setQ[1])
                {
                    Xrl.Add(M[i][0]);
                }
                if (Q >= setQ[0] && Q <= setQ[1] && (nQ < setQ[0] || nQ > setQ[1]))
                {
                    Xrr.Add(M[i][0]);
                }
                Q = nQ;
            }
            return new Borders(Xrl, Xrr);
        }

        private void sortM(List<float[]> M)
        {
            int len = M.Count;
            // пузырьковая
            for (int i = 0; i < len; i++)
            {
                for (int j = 0; j < len - 1; j++)
                {
                    if (M[j][0] > M[j + 1][0])
                    {
                        float[] temp = M[j];
                        M[j] = M[j + 1];
                        M[j + 1] = temp;
                    }
                }
            }

        }

        public int getMaxY()
        {
            int maxY = second.getMaxY();
            if (maxY < first.getMaxY())
                maxY = first.getMaxY();
            return maxY;
        }

        public int getMinY()
        {
            int minY = first.getMinY();
            if (minY > second.getMinY())
                minY = second.getMinY();
            return minY;
        }

        public bool isInside(int x, int y)
        {
            Borders borders = getBorders(y);

            for (int i = 0; i < borders.getLeft().Count; i++) {
                if (borders.getLeft()[i] <= x && borders.getRight()[i] >= x)
                    return true;
            }
            return false;
        }

        public void move(float dx, float dy)
        {
            first.move(dx, dy);
            second.move(dx, dy);
        }

        public void reset()
        {
            first.reset();
            second.reset();
        }

        public void rotate(float angle, PointF relatePoint)
        {
            first.rotate(angle, relatePoint);
            second.rotate(angle, relatePoint);
        }

        public void scale(float s, PointF relatePoint, int type)
        {
            first.scale(s, relatePoint, type);
            second.scale(s, relatePoint, type);
        }

        public PointF getCenter()
        {
            PointF firstCenter = first.getCenter();
            PointF secondCenter = second.getCenter();

            float x = (firstCenter.X + secondCenter.X)/2;
            float y = (firstCenter.Y + secondCenter.Y) / 2;

            return new PointF(x, y);
        }
    }
}
