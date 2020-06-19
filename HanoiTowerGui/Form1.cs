using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HanoiTowerGui
{
    public partial class Form1 : Form
    {
        private Graphics graphics;

        private const int columnWidth = 20;

        private Stack<Rectangle>[] column;

        private Rectangle baseRect;

        private int step;

        private CancellationTokenSource cts;
        public Form1()
        {
            InitializeComponent();

            graphics = pictureBox1.CreateGraphics();
            column = new Stack<Rectangle>[3];
            for (int i = 0; i < 3; i++)
                column[i] = new Stack<Rectangle>();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
           
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            int n;
            try
            {
                n = int.Parse(textBox1.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("输入错误");
                return;
            }
            button1.Enabled = false;
            button2.Enabled = true;
            cts = new CancellationTokenSource();
            InitTower(n);
            await Task.Run(() => System.Threading.Thread.Sleep(1000));
            try
            {
                await Hanoi(n, 0, 1, 2);
            }
            catch (OperationCanceledException)
            {
                
            }
            finally
            {
                button1.Enabled = true;
                button2.Enabled = false;
            }
        }

        private void InitTower(int n)
        {
            for (int i = 0; i < 3; i++)
                column[i] = new Stack<Rectangle>();
            graphics.Clear(pictureBox1.BackColor);
            // draw the rectangles that stand for the columns
            Rectangle[] columnRectangle = new Rectangle[3];
            columnRectangle[0] = new Rectangle(pictureBox1.Width / 4 - columnWidth / 2, 
                pictureBox1.Height / 4, columnWidth, pictureBox1.Height / 2);
            columnRectangle[1] = new Rectangle(pictureBox1.Width / 2 - columnWidth / 2,
                pictureBox1.Height / 4, columnWidth, pictureBox1.Height / 2);
            columnRectangle[2] = new Rectangle(pictureBox1.Width / 4 * 3 - columnWidth / 2,
                pictureBox1.Height / 4, columnWidth, pictureBox1.Height / 2);
            //graphics.FillRectangles(new SolidBrush(Color.Black), columnRectangle);
            // draw the rectangles that stand for the base
            Rectangle baseRectangle = new Rectangle(columnRectangle[0].X - 2 * columnWidth
                , pictureBox1.Height / 4 * 3, columnRectangle[2].X + 5 * columnWidth - columnRectangle[0].X,
                columnWidth);
            graphics.FillRectangle(new SolidBrush(Color.Black), baseRectangle);
            baseRect = baseRectangle;

            step = (n > 1) ? ((15 / 4 * columnWidth) / (n - 1)) : 0;

            // draw the rectangles that stand for the solids
            Rectangle[] solidRectangles = new Rectangle[n];
            for (int i = 0; i < n; i++)
            {
                int x = baseRectangle.X + step * i / 2;
                int y = baseRectangle.Y - columnWidth * (i + 1);
                int width = 5 * columnWidth - step * i;
                int height = columnWidth;
                solidRectangles[i] = new Rectangle(x, y, width, height);
                column[0].Push(solidRectangles[i]);
            }
            graphics.FillRectangles(new SolidBrush(Color.Blue), solidRectangles);
            graphics.DrawRectangles(new Pen(Color.Red, 1.0f), solidRectangles);
        }

        private async Task MoveSolid(int srcIndex, int dstIndex)
        {
            cts.Token.ThrowIfCancellationRequested();
            int deltaX = (dstIndex - srcIndex) * pictureBox1.Width / 4;
            int y = baseRect.Y - columnWidth * (column[dstIndex].Count + 1);
            Rectangle rectangle = column[srcIndex].Pop();
            int x = pictureBox1.Width / 4 * (dstIndex + 1) - rectangle.Width / 2;
            graphics.FillRectangle(new SolidBrush(Color.White), rectangle);
            graphics.DrawRectangle(new Pen(Color.White, 1.0f), rectangle);
            rectangle.X = x;
            rectangle.Y = y;
            graphics.FillRectangle(new SolidBrush(Color.Blue), rectangle);
            graphics.DrawRectangle(new Pen(Color.Red, 1.0f), rectangle);
            column[dstIndex].Push(rectangle);

            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(1000);
            });
            
        }

        private async Task Hanoi(int n, int a, int b, int c)
        {
            if (n == 1)
            {
                await MoveSolid(a, c);
            }
            else
            {
                await Hanoi(n - 1, a, c, b);
                await MoveSolid(a, c);
                await Hanoi(n - 1, b, a, c);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }
    }
}
