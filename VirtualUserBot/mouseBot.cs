using System;


using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using WindowsInput;

namespace gc.bot
{
    public class MouseBot
    {

        /**
         * Array of Form for each Screen
         */
        private static Form[] _botForm;

        private Form MyPrimaryForm
        { get
            {
                if (_botForm == null)
                {
                    _botForm = CreateScreenForm();
                }
                return _botForm[0]; }
        }
        private Form[] AllForms
        {
            get
            {
                if (_botForm == null)
                {
                    _botForm = CreateScreenForm();
                }
                return _botForm;
            }
        }
        private static InputSimulator _sim;
        private InputSimulator Simulator
        {
            get
            { if(_sim == null )
                {
                    _sim = new InputSimulator();
                }
                return _sim; }
        }



        public MouseBot()
        {
        }


        #region Windows32
 

        internal struct IconInfo
        {
            public bool fIcon; public int xHotspot; public int yHotspot; public IntPtr hbmMask; public IntPtr hbmColor;
        }

   

        public static Size GetPrimaryScreenSize()
        {
            return Screen.PrimaryScreen.Bounds.Size;
        }

        [DllImport("user32.dll")]
        internal static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        #endregion


        public void MoveCursorWPF(int dx, int dy)
        {
            // Set the Current cursor, move the cursor's Position,
            // and set its clipping rectangle to the form.             
            MyPrimaryForm.Cursor = new Cursor(Cursor.Current.Handle);            
            Point p = new Point(Cursor.Position.X + dx, Cursor.Position.Y + dy);
            if (p.X > GetPrimaryScreenSize().Width)
                p.X = p.X - GetPrimaryScreenSize().Width;
            if (p.Y > GetPrimaryScreenSize().Height)
                p.Y = p.Y - GetPrimaryScreenSize().Height;

            Debug.WriteLine("Position" + p.ToString()+ " / Size " + GetPrimaryScreenSize().Width +":"+ GetPrimaryScreenSize().Height );
            Cursor.Position = p;
            
            Cursor.Clip = new Rectangle(MyPrimaryForm.Location, MyPrimaryForm.Size );

        }
        public void MoveCursorWindowsForms(int dx, int dy)
        {
            Simulator.Mouse
               .MoveMouseTo(dx, dx);
        }


        public void drawFullPanels( Color PanelColor)
        {
            if (PanelColor == null)
                PanelColor = Color.Blue;
         
            foreach (Form MyForm in AllForms)
            {
                //Panel panel = new TransparentPanel();
                //PictureBox pb = new PictureBox() { Size = screenSize ,Parent = MyForm, BackColor = Color.Transparent };
                ColorPanel panel = new ColorPanel();
                panel.Parent = MyForm;
                MyForm.KeyPreview = true;
                panel.Size = MyForm.Size;
                panel.BackColor = PanelColor;                
                panel.InitKeyPressed();               
                MyForm.Controls.Add(panel);                
                MyForm.Show();
            }

        }

    public void drawMouseText(String text,  Brush mouseColor)
        {
            if (mouseColor == null)
                mouseColor = Brushes.Tomato;

            
            // Primary Screen
            Bitmap bitmap = new Bitmap(GetPrimaryScreenSize().Width, GetPrimaryScreenSize().Height);
            bitmap.MakeTransparent(Color.Transparent);
            Graphics g = Graphics.FromImage(bitmap);            
            using (Font f = new Font(FontFamily.GenericSansSerif, 10))
                g.DrawString(text, f, mouseColor, 0, 0);

            //create the cursor thanks to Win32
            System.Windows.Forms.Cursor.Current = CreateCursorWin32(bitmap, 3, 3);

            bitmap.Dispose();
        }

        private Form[] CreateScreenForm()
        {
            List<Form> f = new List<Form>(Screen.AllScreens.Length);
            foreach (Screen s in Screen.AllScreens)
            {
                Form screen_f = new Form();
                screen_f.TransparencyKey = screen_f.BackColor;
                screen_f.WindowState = FormWindowState.Normal;
                screen_f.FormBorderStyle = FormBorderStyle.None;
                screen_f.Bounds = s.Bounds;                
                screen_f.TopMost = true;

                screen_f.StartPosition = FormStartPosition.Manual;
                screen_f.Location = new Point(s.Bounds.X, s.Bounds.Y);

                Application.EnableVisualStyles();
                //Application.Run(f);

                f.Add(screen_f);
            }
            return f.ToArray();
        }

        private static Cursor CreateCursorWin32(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IconInfo tmp = new IconInfo();
            GetIconInfo(bmp.GetHicon(), ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            IntPtr ptr = CreateIconIndirect(ref tmp);             
            return new Cursor(ptr);
        }

        internal void removeFullPanels()
        {
            foreach (Form MyForm in AllForms)
            {               
                MyForm.Dispose();
            }
        }
    }// end of MouseBot


    //public partial class MouseCursorForm: Form
    //{
    //    public MouseCursorForm()
    //    {
    //        InitializeComponent();
    //        this.BackColor = Color.White;
    //        panel1.BackColor = Color.FromArgb(25, Color.Black);
    //    }
    //    protected override void OnPaint(PaintEventArgs e)
    //    {
    //        e.Graphics.DrawLine(Pens.Yellow, 0, 0, 100, 100);
    //    }
    //}

    public class TransparentPanel : Panel
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return cp;
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
       //     base.OnPaintBackground(e);
        }
        public void MyKeyPressEventHandler(Object sender, KeyPressEventArgs e)
        {
        }

    }

    public class ColorPanel : Panel
    {
        public void InitKeyPressed()
        {
            (this as Control).KeyPress += new KeyPressEventHandler(MyKeyPressEventHandler);
            (this as Control).PreviewKeyDown += MyKeyPressDownEventHandler;

        }
        public void MyKeyPressEventHandler(Object sender, KeyPressEventArgs e)
        {
            Debug.WriteLine("Press " + e.ToString());
        }

        public void MyKeyPressDownEventHandler(Object sender, PreviewKeyDownEventArgs e)
        {
            Debug.WriteLine("PressDown " + e.ToString());
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            MessageBox.Show("You press " + keyData.ToString());

            // dO operations here...

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}