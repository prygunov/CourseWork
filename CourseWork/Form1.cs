using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourseWork
{
    public partial class Form1 : Form
    {
        Bitmap myBitmap;
        Graphics g;
        Pen pen = new Pen(Color.Black, 1);
        List<Polygon> polygons = new List<Polygon>();
        int selectedIndex = -1;
        Polygon selectedPolygon = new Polygon();

        Point lastMousePos = new Point();

        private void update(object sender, MouseEventArgs e)
        {
            if (!checkBox2.Checked & selectedIndex != -1)
            {
                Polygon polygon = polygons[selectedIndex];
                if (e.Button == MouseButtons.Left)
                    polygon.move(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                if (e.Delta != 0)
                    if (Control.ModifierKeys == Keys.Control)
                        polygon.rotate(e.Delta / 120f * 0.0174533f); // радиан одного градуса
                    else
                        polygon.scale(1f + e.Delta / 1200f);

                render();

                pictureBox1.Refresh();
                lastMousePos = e.Location;
            }
        }

        public Form1()
        {
            InitializeComponent();
            myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(myBitmap);
            pictureBox1.MouseWheel += new MouseEventHandler(update);
            pictureBox1.MouseMove += new MouseEventHandler(update);
        }

        // заполнение списка вершин
        private void InputPgn(MouseEventArgs e)
        {
            Point NewP = new Point() { X = e.X, Y = e.Y };
            selectedPolygon.Add(NewP);
            int k = selectedPolygon.pointsCount();
            if (k > 1) 
                g.DrawLine(pen, selectedPolygon.getPoints()[k - 2], selectedPolygon.getPoints()[k - 1]);
            else 
                g.DrawRectangle(pen, e.X, e.Y, 1, 1);

            if (e.Button == MouseButtons.Right) // Конец ввода
            {
                polygons.Add(selectedPolygon);
                render();
                selectedPolygon = new Polygon();
            }
        }

        int getSelectedIndex(int x, int y) {
            int i = 0;
            foreach (Polygon polygon in polygons)
            {
                if (polygon.isInside(x, y)) return i;
                i++;
            }
            return -1;
        }

        // Обработчик события
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;
            int index = getSelectedIndex(e.X, e.Y);
            if (checkBox2.Checked)
            {
                InputPgn(e);
            }
            else if (index != -1)
            {
                selectedIndex = index;
                g.DrawEllipse(new Pen(Color.Blue), e.X - 2, e.Y - 2, 5, 5);
            }
            else selectedIndex = -1;

            pictureBox1.Image = myBitmap;
        }


        // Обработчик события выбора цвета в элементе ComboBox cbLineColor
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)  // выбор цвета  
            {
                case 0:
                    pen.Color = Color.Black;
                    break;
                case 1:
                    pen.Color = Color.Red;
                    break;
                case 2:
                    pen.Color = Color.Green;
                    break;
                case 3:
                    pen.Color = Color.Blue;
                    break;
            }
        }

        // очистка
        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = myBitmap;
            g.Clear(pictureBox1.BackColor);
            selectedIndex = -1;
            polygons.Clear();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Height > 0)
            {
                myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                g = Graphics.FromImage(myBitmap);
                selectedPolygon.Clear();
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                if (selectedPolygon.pointsCount() > 0)
                {
                    selectedPolygon.Fill(g, pen);
                    pictureBox1.Image = myBitmap;
                }
            }
        }

        void render() {
            g.Clear(pictureBox1.BackColor);

            foreach (Polygon polygon in polygons)
            {
                polygon.Fill(g, pen);
            }

            pictureBox1.Refresh();

        }

        // сброс преобразований
        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedIndex != -1) {
                Polygon polygon = polygons[selectedIndex];
                polygon.reset();
                render();
            }

            pictureBox1.Refresh();
        }
        // удаление
        private void button4_Click(object sender, EventArgs e)
        {
            if (selectedIndex != -1)
            {
                selectedIndex = -1;
                polygons.RemoveAt(selectedIndex);
                render();
            }
            pictureBox1.Refresh();
        }
    }
}
