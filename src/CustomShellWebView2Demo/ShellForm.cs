using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CustomShellWebView2Demo
{
    public sealed class ShellForm : Form
    {
        private const int CornerRadius = 8;
        private const int ResizeBorder = 8;
        private const int TitleBarHeight = 40;

        private readonly Panel _titleBar;
        private readonly Panel _contentHost;
        private readonly PictureBox _iconBox;
        private readonly Label _titleLabel;
        private readonly Button _minButton;
        private readonly Button _maxButton;
        private readonly Button _closeButton;
        private readonly WebView2 _webView;

        public ShellForm()
        {
            SuspendLayout();

            BackColor = Color.FromArgb(250, 250, 252);
            ForeColor = Color.FromArgb(35, 39, 46);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(900, 640);
            Size = new Size(1280, 820);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WebView2 Custom Shell";
            Padding = new Padding(1);

            _titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TitleBarHeight,
                BackColor = Color.FromArgb(28, 38, 58)
            };

            _iconBox = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 40,
                Image = SystemIcons.Application.ToBitmap(),
                SizeMode = PictureBoxSizeMode.CenterImage
            };

            _titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "WebView2 Custom Shell Demo",
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold, GraphicsUnit.Point),
                Padding = new Padding(4, 0, 0, 0)
            };

            _minButton = CreateTitleButton(" _ ");
            _maxButton = CreateTitleButton("[]");
            _closeButton = CreateTitleButton("X");
            _closeButton.BackColor = Color.FromArgb(190, 63, 69);

            _contentHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            _webView = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = Color.White
            };

            _minButton.Click += (_, __) => WindowState = FormWindowState.Minimized;
            _maxButton.Click += (_, __) => ToggleMaximizeRestore();
            _closeButton.Click += (_, __) => Close();
            _titleBar.MouseDown += TitleBar_MouseDown;
            _titleLabel.MouseDown += TitleBar_MouseDown;
            _iconBox.MouseDown += TitleBar_MouseDown;

            _titleBar.Controls.Add(_titleLabel);
            _titleBar.Controls.Add(_closeButton);
            _titleBar.Controls.Add(_maxButton);
            _titleBar.Controls.Add(_minButton);
            _titleBar.Controls.Add(_iconBox);

            _contentHost.Controls.Add(_webView);
            Controls.Add(_contentHost);
            Controls.Add(_titleBar);

            Resize += (_, __) => ApplyRoundedRegion();
            Shown += async (_, __) => await InitializeWebViewAsync();

            ResumeLayout();
            ApplyRoundedRegion();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= NativeMethods.CS_DROPSHADOW;
                return cp;
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_GETMINMAXINFO)
            {
                WmGetMinMaxInfo(m.HWnd, m.LParam);
            }

            if (m.Msg == NativeMethods.WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if ((int)m.Result == NativeMethods.HTCLIENT)
                {
                    Point clientPoint = PointToClient(new Point((int)m.LParam));
                    m.Result = (IntPtr)GetResizeHit(clientPoint);
                    if ((int)m.Result == NativeMethods.HTCLIENT && _titleBar.Bounds.Contains(clientPoint))
                    {
                        m.Result = (IntPtr)NativeMethods.HTCAPTION;
                    }
                }
                return;
            }

            base.WndProc(ref m);
        }

        private async System.Threading.Tasks.Task InitializeWebViewAsync()
        {
            await _webView.EnsureCoreWebView2Async();
            _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Navigate("https://www.example.com/");
        }

        private Button CreateTitleButton(string text)
        {
            Button button = new Button
            {
                Dock = DockStyle.Right,
                Width = 48,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point),
                BackColor = Color.FromArgb(28, 38, 58),
                TabStop = false
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(53, 67, 97);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(72, 89, 126);
            return button;
        }

        private void ToggleMaximizeRestore()
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;

            ApplyRoundedRegion();
        }

        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, (IntPtr)NativeMethods.HTCAPTION, IntPtr.Zero);
        }

        private void ApplyRoundedRegion()
        {
            if (!IsHandleCreated)
            {
                return;
            }

            if (WindowState == FormWindowState.Maximized)
            {
                NativeMethods.SetWindowRgn(Handle, IntPtr.Zero, true);
                Padding = new Padding(0);
                return;
            }

            Padding = new Padding(1);
            IntPtr regionHandle = NativeMethods.CreateRoundRectRgn(
                0,
                0,
                Width + 1,
                Height + 1,
                CornerRadius * 2,
                CornerRadius * 2);
            NativeMethods.SetWindowRgn(Handle, regionHandle, true);
        }

        private int GetResizeHit(Point point)
        {
            if (WindowState == FormWindowState.Maximized)
            {
                return NativeMethods.HTCLIENT;
            }

            bool left = point.X <= ResizeBorder;
            bool right = point.X >= Width - ResizeBorder;
            bool top = point.Y <= ResizeBorder;
            bool bottom = point.Y >= Height - ResizeBorder;

            if (left && top) return NativeMethods.HTTOPLEFT;
            if (right && top) return NativeMethods.HTTOPRIGHT;
            if (left && bottom) return NativeMethods.HTBOTTOMLEFT;
            if (right && bottom) return NativeMethods.HTBOTTOMRIGHT;
            if (left) return NativeMethods.HTLEFT;
            if (right) return NativeMethods.HTRIGHT;
            if (top) return NativeMethods.HTTOP;
            if (bottom) return NativeMethods.HTBOTTOM;

            return NativeMethods.HTCLIENT;
        }

        private void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            NativeMethods.MINMAXINFO mmi = (NativeMethods.MINMAXINFO)System.Runtime.InteropServices.Marshal.PtrToStructure(
                lParam,
                typeof(NativeMethods.MINMAXINFO));

            IntPtr monitor = NativeMethods.MonitorFromWindow(hwnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                NativeMethods.MONITORINFO monitorInfo = new NativeMethods.MONITORINFO
                {
                    cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NativeMethods.MONITORINFO))
                };

                if (NativeMethods.GetMonitorInfo(monitor, ref monitorInfo))
                {
                    NativeMethods.RECT workArea = monitorInfo.rcWork;
                    NativeMethods.RECT monitorArea = monitorInfo.rcMonitor;

                    mmi.ptMaxPosition.X = Math.Abs(workArea.Left - monitorArea.Left);
                    mmi.ptMaxPosition.Y = Math.Abs(workArea.Top - monitorArea.Top);
                    mmi.ptMaxSize.X = Math.Abs(workArea.Right - workArea.Left);
                    mmi.ptMaxSize.Y = Math.Abs(workArea.Bottom - workArea.Top);
                }
            }

            System.Runtime.InteropServices.Marshal.StructureToPtr(mmi, lParam, true);
        }
    }
}
