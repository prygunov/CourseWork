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
        List<PointF> inputPoints = new List<PointF>();

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
            inputPoints.Add(NewP);
            renderInput();
            if (e.Button == MouseButtons.Right) // Конец ввода
            {
                polygons.Add(new Polygon(inputPoints, getColor(comboBox5.SelectedIndex)));
                render();
                inputPoints.Clear();
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
            if (checkBox2.Checked){
                InputPgn(e);
            } else if (index != -1){
                selectedIndex = index;
                g.DrawEllipse(new Pen(Color.Blue), e.X - 2, e.Y - 2, 5, 5);
            }
            else selectedIndex = -1;

            pictureBox1.Image = myBitmap;
        }


        // Обработчик события выбора цвета в элементе ComboBox cbLineColor
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pen.Color = getColor(comboBox5.SelectedIndex);
        }

        Color getColor(int index) {
            switch (index)  // выбор цвета  
            {
                case 1:
                    return Color.Red;
                case 2:
                    return Color.Green;
                case 3:
                    return Color.Blue;
                default:
                    return Color.Black;
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
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                render();
            }
        }

        void render() {
            g.Clear(pictureBox1.BackColor);
            renderInput();

            int i = 0;
            if(!checkBox1.Checked)
                foreach (Polygon polygon in polygons)
                {
                    polygon.Fill(g, pen);
                }
            else
                foreach (Polygon polygon in polygons)
                {
                    polygon.Fill(g, pen, ""+i);
                    i++;
                }
            pictureBox1.Refresh();

        }
        void renderInput()
        {
            int i = 0;
            if (inputPoints.Count == 1)
                g.DrawRectangle(pen, inputPoints[0].X, inputPoints[0].Y, 1, 1);

            foreach (PointF point in inputPoints)
            {
                if (i > 0)
                    g.DrawLine(pen, inputPoints[i], inputPoints[i - 1]);
                i++;
            }
        }

        // сброс преобразований
        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedIndex != -1) {
                Polygon polygon = polygons[selectedIndex];
                polygon.reset();
                render();
            }
        }
        // удаление
        private void button4_Click(object sender, EventArgs e)
        {
            if (selectedIndex != -1)
            {
                polygons.RemoveAt(selectedIndex);
                selectedIndex = -1;
                render();
            }
            pictureBox1.Refresh();
        }
        //добавление примитива
        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
