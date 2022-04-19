using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace LAB2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetSize();
            editButton.Enabled = false;
        }

        Bitmap map = new Bitmap(100, 100); //наша карта рисования
        Graphics graphics; //контроллер графики

        //Создаем кисти для рисования
        Pen dash_pen = new Pen(Brushes.Red, 3f);
        Brush quad_pen = new SolidBrush(Color.Black);
        Brush basie_pen = new SolidBrush(Color.Green);

        //Массивы точек для заданной прямой и кривой безье
        private ArrayPoints arrayPoints = new ArrayPoints(2);
        private ArrayPoints arrayPoints2 = new ArrayPoints(2);

        //Вторая форма, предназначенная для редактирвоания
        Form form2 = new Form2();

        /// <summary>
        /// Устанавливаем размер для контроллера графики
        /// </summary>
        private void SetSize()
        {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            map = new Bitmap(rect.Width, rect.Height);
            graphics = Graphics.FromImage(map);
        }

        /// <summary>
        /// Класс для хранения точек в списке
        /// </summary>
        public class ArrayPoints
        {
            private int index = 0;
            private List<Point> points;

            public ArrayPoints(int size)
            {
                if (size <= 0) { size = 2; }
                points = new List<Point>(2);
            }

            public void SetPoint(int x, int y)
            {

                points.Add(new Point(x, y));
                index++;
            }

            public void ResetPoints()
            {
                index = 0;
                points.Clear();
            }

            public int GetCountPoints()
            {
                return index;
            }

            public List<Point> GetPoints()
            {
                return points;
            }

            public void DeletePoint(int index)
            {
                points.RemoveAt(index);
                this.index--;
            }
        }

        /// <summary>
        /// Срабатывает при нажатии кнопки мыши в области рисования
        /// </summary>
        /// <param name="sender">объект, по которому произешл клик</param>
        /// <param name="e">событие, которое произошло</param>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            arrayPoints.SetPoint(e.X, e.Y);

            //Генерируем разные цвета для кисти кривой безье
            Random r = new Random();
            int red = r.Next(0, byte.MaxValue + 1);
            int green = r.Next(0, byte.MaxValue + 1);
            int blue = r.Next(0, byte.MaxValue + 1);
            basie_pen = new SolidBrush(Color.FromArgb(red, green, blue));

            if (arrayPoints.GetCountPoints() != 0) editButton.Enabled = true; //включаем кнопку редактирования
        }

        /// <summary>
        /// Срабатывает при отпускании кнопки мыши в области рисования
        /// </summary>
        /// <param name="sender">объект, по которому произешл клик</param>
        /// <param name="e">событие, которое произошло</param>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            Draw();
        }

        /// <summary>
        /// Высчитывает факториал числа
        /// </summary>
        /// <param name="n">Число, факториал которого будет считать</param>
        /// <returns>возвращает факториал</returns>
        static float fact(float n)
        {
            float factorial = 1;
            for (int i = 1; i <= n; i++)
                factorial *= i;

            return factorial;
        }

        /// <summary>
        /// Высчитывает число по формуле перестановок
        /// </summary>
        /// <param name="n">C из N</param>
        /// <param name="k">С по K</param>
        /// <returns>Возвращает полученное число перестановок</returns>
        static double bk(int n, int k)
        {
            double result = fact(n) / (fact(k) * fact(n - k));
            return result;
        }

        /// <summary>
        /// Кнопка очищения всего нарисованного
        /// </summary>
        /// <param name="sender">Кнопка сбрасывания</param>
        /// <param name="e">Клик</param>
        private void resetButton_Click(object sender, EventArgs e)
        {
            arrayPoints.ResetPoints();
            arrayPoints2.ResetPoints();
            editButton.Enabled = false;
            graphics.Clear(pictureBox1.BackColor);
            pictureBox1.Image = map;
        }

        /// <summary>
        /// Кнопка редактирования координат точек прямой
        /// </summary>
        /// <param name="sender">Кнопка редактирования</param>
        /// <param name="e">Клик по кноке</param>
        private void editButton_Click(object sender, EventArgs e)
        {
            DrawForm2();
            form2.ShowDialog();
        }

        /// <summary>
        /// Кнопка удаления координаты в панеле редактирования координат
        /// </summary>
        /// <param name="sender">Кнопка</param>
        /// <param name="e">Клик</param>
        private void deletePointButton_Click(object sender, EventArgs e)
        {
            Button clicked = sender as Button; //получаем кнопку
            string name = clicked.Name; //получаем ее имя
            int value;
            int.TryParse(string.Join("", name.Where(c => char.IsDigit(c))), out value); //находим число в названии (Он будет индексом)
            
            //удаляем
            arrayPoints.DeletePoint(value);
            form2.Controls.Remove(clicked);
            form2.Controls.Clear();

            //перерисовываем окна
            DrawForm2();
            graphics.Clear(pictureBox1.BackColor);
            Draw();

        }

        /// <summary>
        /// Закрытие окна редактирования
        /// </summary>
        /// <param name="sender">Кнопка</param>
        /// <param name="e">Клик</param>
        private void CloseBottun_Click(object sender, EventArgs e)
        {
            form2.Controls.Clear(); //очищаем форму редактирования, чтобы потом при открытии не было дубликатов
        }

        /// <summary>
        /// Кнопка сохранения в панеле редактирования координат
        /// </summary>
        /// <param name="sender">Кнопка</param>
        /// <param name="e">Клик</param>
        private void SaveButton_Click(object sender, EventArgs e)
        {
            //Введенные координаты в текстовое поле
            int X = 0;
            int Y = 0;

            //Очищаем наши массивы точек
            arrayPoints.ResetPoints();
            arrayPoints2.ResetPoints();

            var allTextBoxesOnMyFormAsList = form2.Controls.OfType<TextBox>().ToList(); // находим все текстбоксы
            foreach (var textbox in allTextBoxesOnMyFormAsList)
            {
                if (X == 0) X = int.Parse(textbox.Text); //Получаем координату X
                else Y = int.Parse(textbox.Text); //Получаем координату Y
                if (Y != 0)
                {
                    arrayPoints.SetPoint(X, Y); //Вставляем координаты в массив
                    X = Y = 0;
                }
            }

            graphics.Clear(pictureBox1.BackColor); //удаляем все с экрана рисования
            Draw(); //перериовываем
            form2.Close(); //закрываем форму
            form2.Controls.Clear(); //очищаем форму
        }

        /// <summary>
        /// Метод, который рисует на главном экране 
        /// </summary>
        private void Draw()
        {
            DrawMainLine(); //рисуем основную пунктирную линию

            //Если нарисованных точек больше 3, то начинаем строить кривую безье
            if (arrayPoints.GetCountPoints() >= 3)
            {
                //Если кривая безье уже была построенна, то удалим ее и очистим экран, перерисовав основную прямую
                arrayPoints2.ResetPoints(); 
                graphics.Clear(pictureBox1.BackColor);
                DrawMainLine();

                //Начиная формировать кривую безье по общей формуле
                for (double t = 0; t <= 1; t += 0.0005)
                {
                    int currentX = 0;
                    int currentY = 0;
                    for (int i = 0; i < arrayPoints.GetCountPoints(); i++)
                    {
                        currentX += (int)(bk(arrayPoints.GetCountPoints() - 1, i) * Math.Pow(t, i) * Math.Pow((1 - t), arrayPoints.GetCountPoints() - i - 1) * arrayPoints.GetPoints().ToArray()[i].X); //Считаем координату X прямой безье
                        currentY += (int)(bk(arrayPoints.GetCountPoints() - 1, i) * Math.Pow(t, i) * Math.Pow((1 - t), arrayPoints.GetCountPoints() - i - 1) * arrayPoints.GetPoints().ToArray()[i].Y);  //Считаем координату Y прямой безье
                    }
                    arrayPoints2.SetPoint(currentX, currentY); //Вставляем точки безье в массив безье
                    graphics.FillRectangle(basie_pen, arrayPoints2.GetPoints().ToArray()[arrayPoints2.GetCountPoints() - 1].X, arrayPoints2.GetPoints().ToArray()[arrayPoints2.GetCountPoints() - 1].Y, 5, 5); //Рисуем миксель
                }
            }
            pictureBox1.Image = map; //отображаем все нашу отрисованную карту
        }

        /// <summary>
        /// Метод для отрисовки второй формы
        /// </summary>
        private void DrawForm2()
        {
            if (arrayPoints.GetCountPoints() != 0) //Если есть хотя бы одна точка, то форма отобразится, иначе - закроется
            {
                for (int i = 0; i < arrayPoints.GetCountPoints(); i++) //Отрисовываем столько элементов, сколько у нас точек
                {
                    //Лейбл для координаты X
                    Label labelX = new Label();
                    labelX.Text = "X" + i.ToString();
                    labelX.Name = "labelX" + i.ToString();
                    labelX.Font = new Font(labelX.Font.Name, 14,
                    labelX.Font.Style, labelX.Font.Unit);
                    labelX.Width = 40;
                    labelX.Location = new Point(10, 10 + 40 * i);

                    //Текстбокс для координаты X
                    TextBox textBoxX = new TextBox();
                    textBoxX.Name = "textBoxX" + i.ToString();
                    textBoxX.Font = new Font(textBoxX.Font.Name, 14,
                   textBoxX.Font.Style, textBoxX.Font.Unit);
                    textBoxX.Location = new Point(50, 10 + 40 * i);
                    textBoxX.Text = arrayPoints.GetPoints().ToArray()[i].X.ToString();

                    //Кнопка удаления координаты
                    Button deleteButton = new Button();
                    deleteButton.Name = "deleteButton" + i.ToString();
                    deleteButton.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
                    deleteButton.BackgroundImageLayout = ImageLayout.Center;
                    deleteButton.TabIndex = 1;
                    deleteButton.TabStop = true;
                    deleteButton.BackgroundImage = Properties.Resources.delete;
                    deleteButton.Size = deleteButton.BackgroundImage.Size + new Size(8, 8);
                    deleteButton.Location = new Point(225, 5 + 40 * i);
                    deleteButton.Click += deletePointButton_Click;

                    //Лейбл для координаты Y
                    Label labelY = new Label();
                    labelY.Text = "Y" + i.ToString();
                    labelY.Name = "labelY" + i.ToString();
                    labelY.Font = new Font(labelY.Font.Name, 14,
                    labelY.Font.Style, labelY.Font.Unit);
                    labelY.Width = 40;
                    labelY.Location = new Point(300, 10 + 40 * i);

                    //Текстбокс для координаты Y
                    TextBox textBoxY = new TextBox();
                    textBoxY.Name = "textBoxY" + i.ToString();
                    textBoxY.Font = new Font(textBoxY.Font.Name, 14,
                   textBoxY.Font.Style, textBoxY.Font.Unit);
                    textBoxY.Location = new Point(340, 10 + 40 * i);
                    textBoxY.Text = arrayPoints.GetPoints().ToArray()[i].Y.ToString();

                    //Добавляем все эти элементы в форму
                    form2.Controls.Add(labelX);
                    form2.Controls.Add(textBoxX);
                    form2.Controls.Add(labelY);
                    form2.Controls.Add(textBoxY);
                    form2.Controls.Add(deleteButton);

                }
                //Отрисовываем кнопку "Сохранить"
                Button saveButton = new Button();
                saveButton.Name = "saveButton";
                saveButton.Text = "Сохранить";
                saveButton.Font = new Font(saveButton.Font.Name, 14,
                   saveButton.Font.Style, saveButton.Font.Unit);
                saveButton.Width = 130;
                saveButton.Height = 35;
                saveButton.Location = new Point(10, 10 + arrayPoints.GetCountPoints() * 40);
                saveButton.Click += SaveButton_Click;
                //Добавление кнопки к форме и действия по закрытию формы
                form2.FormClosed += CloseBottun_Click;
                form2.Controls.Add(saveButton);
            }
            else
            {
                form2.Close();
                editButton.Enabled = false;
            }
        }

        /// <summary>
        /// Метод для отрисовки пунктирной главной линии
        /// </summary>
        private void DrawMainLine()
        {
            dash_pen.DashStyle = DashStyle.Dash;
            for (int i = 0; i < arrayPoints.GetCountPoints(); i++)
            {
                graphics.FillRectangle(quad_pen, arrayPoints.GetPoints().ToArray()[i].X - 5, arrayPoints.GetPoints().ToArray()[i].Y - 5, 15, 15);
            }
            if (arrayPoints.GetCountPoints() >= 2)
            {
                graphics.DrawLines(dash_pen, arrayPoints.GetPoints().ToArray());
            }
        }
    }
}
