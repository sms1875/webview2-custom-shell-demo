using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomShellWebView2Demo
{
    public sealed class ShellForm : Form
    {
        private const int CornerRadius = 8;
        private const int ResizeBorder = 8;
        private const int TitleBarHeight = 40;
        private const int VisibleBorderThickness = 2;
        private const int InnerCornerRadius = CornerRadius - VisibleBorderThickness;
        private static readonly Color VisibleBorderColor = Color.FromArgb(96, 118, 146);
        private static readonly Color ContentBackColor = Color.FromArgb(255, 255, 255);
        private static readonly Color[] DebugBorderColors =
        {
            Color.FromArgb(231, 76, 60),   // top-left corner
            Color.FromArgb(241, 196, 15),  // top
            Color.FromArgb(46, 204, 113),  // top-right corner
            Color.FromArgb(52, 152, 219),  // right
            Color.FromArgb(155, 89, 182),  // bottom-right corner
            Color.FromArgb(230, 126, 34),  // bottom
            Color.FromArgb(26, 188, 156),  // bottom-left corner
            Color.FromArgb(236, 240, 241)  // left
        };

        private readonly Panel _chromeHost;
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

            BackColor = VisibleBorderColor;
            ForeColor = Color.FromArgb(35, 39, 46);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.None;
            MinimumSize = new Size(900, 640);
            Size = new Size(1280, 820);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WebView2 Custom Shell";
            DoubleBuffered = true;
            Padding = new Padding(VisibleBorderThickness);

            _chromeHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ContentBackColor,
                Padding = new Padding(0)
            };

            _titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TitleBarHeight,
                BackColor = Color.FromArgb(20, 34, 56)
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
                BackColor = ContentBackColor,
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

            _chromeHost.Controls.Add(_contentHost);
            _chromeHost.Controls.Add(_titleBar);
            _contentHost.Controls.Add(_webView);
            Controls.Add(_chromeHost);

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (WindowState == FormWindowState.Maximized)
            {
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int inset = Math.Max(1, VisibleBorderThickness);
            int guideThickness = 2;
            int diameter = CornerRadius * 2;
            Rectangle guideBounds = new Rectangle(
                inset,
                inset,
                Width - (inset * 2) - 1,
                Height - (inset * 2) - 1);

            int left = guideBounds.Left;
            int top = guideBounds.Top;
            int right = guideBounds.Right;
            int bottom = guideBounds.Bottom;
            int arcRight = right - diameter;
            int arcBottom = bottom - diameter;
            int lineStart = left + CornerRadius;
            int lineEndX = right - CornerRadius;
            int lineEndY = bottom - CornerRadius;

            using (Pen topLeftPen = CreateGuidePen(DebugBorderColors[0], guideThickness))
            using (Pen topPen = CreateGuidePen(DebugBorderColors[1], guideThickness))
            using (Pen topRightPen = CreateGuidePen(DebugBorderColors[2], guideThickness))
            using (Pen rightPen = CreateGuidePen(DebugBorderColors[3], guideThickness))
            using (Pen bottomRightPen = CreateGuidePen(DebugBorderColors[4], guideThickness))
            using (Pen bottomPen = CreateGuidePen(DebugBorderColors[5], guideThickness))
            using (Pen bottomLeftPen = CreateGuidePen(DebugBorderColors[6], guideThickness))
            using (Pen leftPen = CreateGuidePen(DebugBorderColors[7], guideThickness))
            {
                e.Graphics.DrawArc(topLeftPen, left, top, diameter, diameter, 180, 90);
                e.Graphics.DrawLine(topPen, lineStart, top, lineEndX, top);
                e.Graphics.DrawArc(topRightPen, arcRight, top, diameter, diameter, 270, 90);
                e.Graphics.DrawLine(rightPen, right, top + CornerRadius, right, lineEndY);
                e.Graphics.DrawArc(bottomRightPen, arcRight, arcBottom, diameter, diameter, 0, 90);
                e.Graphics.DrawLine(bottomPen, lineEndX, bottom, lineStart, bottom);
                e.Graphics.DrawArc(bottomLeftPen, left, arcBottom, diameter, diameter, 90, 90);
                e.Graphics.DrawLine(leftPen, left, lineEndY, left, top + CornerRadius);
            }
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
                BackColor = ContentBackColor;
                _chromeHost.Region = null;
                return;
            }

            BackColor = VisibleBorderColor;
            Padding = new Padding(VisibleBorderThickness);
            IntPtr regionHandle = NativeMethods.CreateRoundRectRgn(
                0,
                0,
                Width + 1,
                Height + 1,
                CornerRadius * 2,
                CornerRadius * 2);
            NativeMethods.SetWindowRgn(Handle, regionHandle, true);
            ApplyChromeRegion();
        }

        private void ApplyChromeRegion()
        {
            if (_chromeHost.Width <= 0 || _chromeHost.Height <= 0)
            {
                return;
            }

            Rectangle chromeRect = new Rectangle(0, 0, _chromeHost.Width, _chromeHost.Height);
            using (GraphicsPath chromePath = CreateRoundedPath(chromeRect, InnerCornerRadius))
            {
                _chromeHost.Region = new Region(chromePath);
            }
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

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
        {
            int safeRadius = Math.Max(1, radius);
            int diameter = safeRadius * 2;
            GraphicsPath path = new GraphicsPath();
            int right = Math.Max(bounds.X, bounds.Right - diameter);
            int bottom = Math.Max(bounds.Y, bounds.Bottom - diameter);

            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(right, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(right, bottom, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bottom, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        private static Pen CreateGuidePen(Color color, int thickness)
        {
            Pen pen = new Pen(color, thickness)
            {
                Alignment = PenAlignment.Center,
                LineJoin = LineJoin.Round
            };
            return pen;
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
