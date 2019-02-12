using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.IO;

namespace Pseudographic
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("Выберите режим псевдографики:");
            Console.WriteLine("1. Цветная");
            Console.WriteLine("2. Ч/Б (Автосохранение)");
            int rez = int.Parse(Console.ReadLine());
            Image img = Image.FromFile(args[0]);
            if (rez == 2)
            {
                Console.WriteLine("Введите имя файла");
                string path = Console.ReadLine();
                Bitmap b2 = new Bitmap(img, new Size(img.Size.Width / int.Parse(args[1]), img.Size.Height / int.Parse(args[1])));
                File.WriteAllText(path+".txt", printimage(b2));
                Console.Clear();
            }
            else
            {
                Console.Clear();
                ConsoleWriteImage((Bitmap)img);
            }
        }
        public static string printimage(Image path)
        {
            string tmp = "";
            Image Picture = path;
            Console.SetBufferSize((Picture.Width * 0x2), (Picture.Height * 0x2));
            FrameDimension Dimension = new FrameDimension(Picture.FrameDimensionsList[0x0]);
            int FrameCount = Picture.GetFrameCount(Dimension);
            int Left = Console.WindowLeft, Top = Console.WindowTop;
            char[] Chars = { '#', '#', '@', '%', '=', '+', '*', ':', '-', '.', ' ' };
            Picture.SelectActiveFrame(Dimension, 0x0);
            for (int i = 0x0; i < Picture.Height; i++)
            {
                for (int x = 0x0; x < Picture.Width; x++)
                {
                    Color Color = ((Bitmap)Picture).GetPixel(x, i);
                    int Gray = (Color.R + Color.G + Color.B) / 0x3;
                    int Index = (Gray * (Chars.Length - 0x1)) / 0xFF;
                    tmp += (Chars[Index]);
                    Console.Write(Chars[Index]);
                }
                tmp += ("\r\n");
                Console.Write('\n');
            }
            Console.SetCursorPosition(Left, Top);            
            return tmp;
        }

        static int[] cColors = { 0x000000, 0x000080, 0x008000, 0x008080, 0x800000, 0x800080, 0x808000, 0xC0C0C0, 0x808080, 0x0000FF, 0x00FF00, 0x00FFFF, 0xFF0000, 0xFF00FF, 0xFFFF00, 0xFFFFFF };

        public static void ConsoleWritePixel(Color cValue)
        {
            Color[] cTable = cColors.Select(x => Color.FromArgb(x)).ToArray();
            char[] rList = new char[] { (char)9617, (char)9618, (char)9619, (char)9608 }; // 1/4, 2/4, 3/4, 4/4
            //char[] rList = new char[] { '#', '@', '*' }; // 1/4, 2/4, 3/4, 4/4
            int[] bestHit = new int[] { 0, 0, 4, int.MaxValue }; //ForeColor, BackColor, Symbol, Score

            for (int rChar = rList.Length; rChar > 0; rChar--)
            {
                for (int cFore = 0; cFore < cTable.Length; cFore++)
                {
                    for (int cBack = 0; cBack < cTable.Length; cBack++)
                    {
                        int R = (cTable[cFore].R * rChar + cTable[cBack].R * (rList.Length - rChar)) / rList.Length;
                        int G = (cTable[cFore].G * rChar + cTable[cBack].G * (rList.Length - rChar)) / rList.Length;
                        int B = (cTable[cFore].B * rChar + cTable[cBack].B * (rList.Length - rChar)) / rList.Length;
                        int iScore = (cValue.R - R) * (cValue.R - R) + (cValue.G - G) * (cValue.G - G) + (cValue.B - B) * (cValue.B - B);
                        if (!(rChar > 1 && rChar < 4 && iScore > 50000)) // rule out too weird combinations
                        {
                            if (iScore < bestHit[3])
                            {
                                bestHit[3] = iScore; //Score
                                bestHit[0] = cFore;  //ForeColor
                                bestHit[1] = cBack;  //BackColor
                                bestHit[2] = rChar;  //Symbol
                            }
                        }
                    }
                }
            }
            Console.ForegroundColor = (ConsoleColor)bestHit[0];
            Console.BackgroundColor = (ConsoleColor)bestHit[1];
            Console.Write(rList[bestHit[2] - 1]);
        }


        public static void ConsoleWriteImage(Bitmap source)
        {
            int sMax = 39;
            decimal percent = Math.Min(decimal.Divide(sMax, source.Width), decimal.Divide(sMax, source.Height));
            Size dSize = new Size((int)(source.Width * percent), (int)(source.Height * percent));
            Bitmap bmpMax = new Bitmap(source, dSize.Width * 2, dSize.Height);
            for (int i = 0; i < dSize.Height; i++)
            {
                for (int j = 0; j < dSize.Width; j++)
                {
                    ConsoleWritePixel(bmpMax.GetPixel(j * 2, i));
                    ConsoleWritePixel(bmpMax.GetPixel(j * 2 + 1, i));
                }
                System.Console.WriteLine();
            }
            Console.ResetColor();
            Console.ReadLine();
        }
        void PrintLine()
        {
            double x1 = 0, y1 = 24;
            double x2 = 79, y2 = 0;

            double dx = x2 - x1;//79 - 0 = 79
            double dy = y2 - y1;//0 - 24 = -24

            double maxDelta = Math.Abs(Math.Max(dx, dy));
            dx = dx / maxDelta;
            dy = dy / maxDelta;

            double xScale = (Math.Max(x1, x2) > (Console.WindowWidth - 1))
                    ? Math.Max(x1, x2) / (Console.WindowWidth - 1)
                    : 1;
            double yScale = (Math.Max(y1, y2) > (Console.WindowHeight - 1))
                                ? Math.Max(y1, y2) / (Console.WindowHeight - 1)
                                : 1;
            double scale = Math.Max(xScale, yScale);

            //настоящие координаты точек линии
            double x = x1;
            double y = y1;
            //целые координаты пикселей для точек линии
            int graphX = (int)Math.Round(x / scale);
            int graphY = (int)Math.Round(y / scale);

            while (graphX >= 0 && graphX < Console.WindowWidth
                && graphY >= 0 && graphY < Console.WindowHeight)
            {
                Console.SetCursorPosition(graphX, graphY);
                Console.Write('*');
                x += dx;
                y += dy;
                graphX = (int)Math.Round(x / scale);
                graphY = (int)Math.Round(y / scale);
            }
            Console.ReadLine();
        }
    }
}
