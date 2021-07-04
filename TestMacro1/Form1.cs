using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using System.Diagnostics;
using System.Drawing.Imaging;

// https://yc0345.tistory.com/222 를 보고 만들었습니다. 구글링 최고. 사랑해요.

namespace TestMacro1
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("User32", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName); // 앱플레이어 확인

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags); // 화면 출력

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr Parent, IntPtr Child, string lpszClass, string lpszWindows);

        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;

        String AppPlayerName = "BlueStacks";
        Bitmap Lobby_img = null;

        public void InClick(int x, int y)
        {
            //클릭이벤트를 발생시킬 플레이어를 찾습니다.
            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero)
            {
                //플레이어를 찾았을 경우 클릭이벤트를 발생시킬 핸들을 가져옵니다.
                IntPtr hwnd_child = FindWindowEx(findwindow, IntPtr.Zero, "RenderWindow", "TheRender");
                IntPtr lparam = new IntPtr(x | (y << 16));

                //플레이어 핸들에 클릭 이벤트를 전달합니다.
                SendMessage(hwnd_child, WM_LBUTTONDOWN, 1, lparam);
                SendMessage(hwnd_child, WM_LBUTTONUP, 0, lparam);
            }
        }

        //searchIMG에 스크린 이미지와 찾을 이미지를 넣어줄거에요
        public void searchIMG(Bitmap screen_img, Bitmap find_img)
        {
            using (Mat ScreenMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(screen_img)) //스크린 이미지 선언
            using (Mat FindMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(find_img)) //찾을 이미지 선언
            using (Mat res = ScreenMat.MatchTemplate(FindMat, TemplateMatchModes.CCoeffNormed)) //스크린 이미지에서 FindMat 이미지를 찾아라
            {
                double minval, maxval = 0; //찾은 이미지의 유사도를 담을 더블형 최대 최소 값을 선언합니다.
                OpenCvSharp.Point minloc, maxloc; //찾은 이미지의 위치를 담을 포인트형을 선업합니다.
                Cv2.MinMaxLoc(res, out minval, out maxval, out minloc, out maxloc); //찾은 이미지의 유사도 및 위치 값을 받습니다. 
                Debug.WriteLine("찾은 이미지의 유사도 : " + maxval);

                if (maxval >= 0.8) //이미지를 찾았을 경우 클릭이벤트를 발생!!
                {
                    InClick(maxloc.X, maxloc.Y);
                }
            }
        }


        public Form1()
        {
            InitializeComponent();
            Lobby_img = new Bitmap(@"img\Lobby.PNG");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IntPtr findwindow = FindWindow(null, AppPlayerName);
            if (findwindow != IntPtr.Zero) // 플레이어를 찾았을 경우
            {
                Debug.WriteLine("앱플레이어를 찾았습니다.");
                Graphics Graphicsdata = Graphics.FromHwnd(findwindow); // 찾은 플레이어를 바탕으로 Graphics 정보를 가져옵니다.
                Rectangle rect = Rectangle.Round(Graphicsdata.VisibleClipBounds); // 찾은 플레이어 창 크기 및 위치를 가져옵니다. 
                Bitmap bmp = new Bitmap(rect.Width, rect.Height); // 플레이어 창 크기 만큼의 비트맵을 선언해줍니다.
                // 매개 변수 값 오류
                using (Graphics g = Graphics.FromImage(bmp)) // 비트맵을 바탕으로 그래픽스 함수로 선언해줍니다.
                {
                    IntPtr hdc = g.GetHdc(); // 찾은 플레이어의 크기만큼 화면을 캡쳐합니다.
                    PrintWindow(findwindow, hdc, 0x2);
                    g.ReleaseHdc(hdc);
                }
                pictureBox1.Image = bmp; //  pictureBox1 이미지를 표시해줍니다.
                searchIMG(bmp, Lobby_img);
            }
            else
            {
                Debug.WriteLine("플레이어가 없어용");
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
