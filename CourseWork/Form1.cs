using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        Pen arrowPen = new Pen(Color.Black, 1);

        List<IObject> objects = new List<IObject>();
        int selectedIndex = -1;
        List<PointF> inputPoints = new List<PointF>();

        Point lastMousePos = new Point();

        private void update(object sender, MouseEventArgs e)
        {
            if (!checkBox2.Checked & selectedIndex != -1)
            {
                IObject polygon = objects[selectedIndex];
                PointF relatePoint = new PointF(e.X, e.Y);
                if (e.Button == MouseButtons.Left)
                    polygon.move(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                if (e.Delta != 0)
                {
                    if (Control.ModifierKeys == Keys.Control)
                    {
                        int grad = Convert.ToInt32(numericUpDown2.Value);
                        float rad = grad * 0.0174533f; // радиан поворта, 0.0174.. радиан одного градуса
                        polygon.rotate(e.Delta / 120f * rad, relatePoint); 
                    }else{
                        int mode = comboBox6.SelectedIndex;

                        if(mode == 1)
                           polygon.scale(1f + e.Delta / 1200f, relatePoint, mode);
                        else
                           polygon.scale(1f + e.Delta / 1200f, polygon.getCenter(), mode);
                    }
                }
                    
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
            pictureBox1.Image = myBitmap;

            arrowPen.CustomEndCap = new AdjustableArrowCap(6, 6); // стрела
        }

        // заполнение списка вершин
        private void InputPgn(MouseEventArgs e)
        {
            Point NewP = new Point() { X = e.X, Y = e.Y };
            inputPoints.Add(NewP);
            renderInput();
            if (!splineCheckBox.Checked)
            {
                if (e.Button == MouseButtons.Right && inputPoints.Count > 1) // Конец ввода
                {
                    objects.Add(new Primitive(inputPoints, getColor(comboBox5.SelectedIndex)));
                    render();
                    updateBoxes();
                    inputPoints.Clear();
                }
            }
            else if(inputPoints.Count > 3){
                Primitive p = new Primitive(inputPoints, getColor(comboBox5.SelectedIndex));
                p.setMode(2);
                objects.Add(p);
                render();
                updateBoxes();
                inputPoints.Clear();
            }
            
        }

        int getSelectedIndex(int x, int y) {
            int i = 0;
            foreach (IObject polygon in objects)
            {
                if (polygon.isInside(x, y))
                {
                    label2.Text = "Выбранный объект: " + i;
                    return i;
                }
                i++;
            }
            label2.Text = "Выбранный объектом: отсутствует";
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
                if (objects[selectedIndex] is Primitive)
                    button1.Enabled = true;
                else
                    button1.Enabled = false;
                g.DrawEllipse(new Pen(Color.Blue), e.X - 2, e.Y - 2, 5, 5);
            }
            else selectedIndex = -1;

            pictureBox1.Image = myBitmap;
        }


        // Обработчик события выбора цвета в элементе ComboBox cbLineColor
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Color specificColor = getColor(comboBox5.SelectedIndex);
            pen.Color = specificColor;
            arrowPen.Color = specificColor;
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
            objects.Clear();
            inputPoints.Clear();
            render();
        }

        // при изменении размера необходимо обновить размер поля рисования и перерисовать
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Height > 0)
            {
                myBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                g = Graphics.FromImage(myBitmap);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                render();
            }
        }

        //перерисовка всех примитивов
        void render() {
            g.Clear(pictureBox1.BackColor);
            renderInput();
            
            int i = 0;
            if(!checkBox1.Checked)
                foreach (IObject obj in objects)
                {
                    obj.draw(g, pen);
                }
            else
                foreach (IObject obj in objects)
                {
                    obj.draw(g, pen, ""+i);
                    i++;
                }
            pictureBox1.Refresh();
        }
        // перерисовка режима рисования
        void renderInput()
        {
            if (inputPoints.Count > 0)
            {
                if (inputPoints.Count == 1)
                    g.DrawRectangle(pen, inputPoints[0].X, inputPoints[0].Y, 1, 1);
                if (!splineCheckBox.Checked)
                    for (int i = 1; i < inputPoints.Count; i++)
                    {
                        g.DrawLine(pen, inputPoints[i - 1], inputPoints[i]);
                    }
                else {
                    if(inputPoints.Count>1)
                        g.DrawLine(arrowPen, inputPoints[0], inputPoints[1]);
                    if (inputPoints.Count == 3)
                        g.DrawRectangle(pen, inputPoints[2].X, inputPoints[2].Y, 1, 1);
                }
            }
            

        }

        // сброс преобразований
        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedIndex != -1) {
                IObject polygon = objects[selectedIndex];
                polygon.reset();
                render();
            }
        }
        // удаление
        private void button4_Click(object sender, EventArgs e)
        {
            if (selectedIndex != -1)
            {
                objects.RemoveAt(selectedIndex);
                selectedIndex = -1;
                render();
            }
        }
        //добавление примитива
        private void button3_Click(object sender, EventArgs e)
        {
            List<PointF> points = new List<PointF>();
            int mode = 0;
            switch (comboBox1.SelectedIndex) {
                default:
                    points.Add(new PointF(300, 300));
                    points.Add(new PointF(400, 400));
                    mode = 1;
                    break;
                case 1:
                    points.Add(new PointF(300, 300));
                    points.Add(new PointF(400, 400));
                    points.Add(new PointF(500, 450));
                    points.Add(new PointF(600, 550));
                    mode = 2;
                    break;
                case 2:
                    points.Add(new PointF(300, 300));
                    points.Add(new PointF(400, 300));
                    points.Add(new PointF(400, 250));
                    points.Add(new PointF(375, 250));
                    points.Add(new PointF(350, 200));
                    points.Add(new PointF(325, 250));
                    points.Add(new PointF(300, 250));
                    break;
                case 3:
                    decimal n = numericUpDown1.Value;
                    PointF center = new PointF(300, 300);
                    int radius = 20;
                    float step = 360f / (float)(n * 2);
                    float radianStep = step * 0.0174533f;

                    for (int i = 0; i < n; i++) {
                        float radian = 2 * i * radianStep;
                        float x = radius * (float) Math.Cos(radian);
                        float y = radius * (float) Math.Sin(radian);
                        points.Add(center + new SizeF(x, y));
                        radian = (2 *i+1) * radianStep;
                        x = 0.5f * radius * (float)Math.Cos(radian);
                        y = 0.5f * radius * (float)Math.Sin(radian);
                        points.Add(center + new SizeF(x, y));
                    }
                    break;
            }
            Primitive p = new Primitive(points, getColor(comboBox5.SelectedIndex));
            p.setMode(mode);
            objects.Add(p);
            render();
            updateBoxes();
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 3)
                numericUpDown1.Enabled = true;
            else numericUpDown1.Enabled = false;
        }

        //новая связка для ТМО
        private void button2_Click_1(object sender, EventArgs e)
        {
            int indexF = (int) comboBox2.SelectedIndex;
            int indexS = (int) comboBox3.SelectedIndex;

            int type = comboBox4.SelectedIndex;

            IObject first;
            IObject second;
            if (indexS != -1 && indexF != -1)
            {
                first = objects[indexF];
                second = objects[indexS];
            }
            else {
                MessageBox.Show("Не выбраны объекты для операции", "Ошибка ТМО", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(!isCorrectForTMO(first) || !isCorrectForTMO(second))
            {
                MessageBox.Show("Прямая или сплайн не могут быть выбраны для операции", "Ошибка ТМО", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (indexF != indexS)
            {
                selectedIndex = -1;

                objects.Add(new Bunch(first, second, type));
                objects.Remove(first);
                objects.Remove(second);

                render();
                updateBoxes();
            }
            else {
                MessageBox.Show("Операция над одним объектом не возможна", "Ошибка ТМО", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        bool isCorrectForTMO(IObject obj) {
            if(obj is Primitive)
            {
                int mode = ((Primitive)obj).getMode();
                if (mode == 1 || mode == 2) return false;
            }
                
            return true;
        }

        private void updateBoxes()
        {
            comboBox2.Items.Clear();
            comboBox2.Text = "Не выбран";
            for (int i = 0; i < objects.Count; i++)
            {
                comboBox2.Items.Add(i);
            }
            
            comboBox3.Items.Clear();
            comboBox3.Text = "Не выбран";
            for (int i = 0; i < objects.Count; i++)
            {
                comboBox3.Items.Add(i);
            }
        }
    }
}
