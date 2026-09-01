using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BHEIIIKA
{
    public partial class Form2 : Form
    {
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        private static readonly Color BgColor = Color.FromArgb(20, 18, 28);
        private static readonly Color AccentPurple = Color.FromArgb(217, 70, 239);
        private static readonly Color AccentBlue = Color.FromArgb(99, 102, 241);
        private static readonly Color TextPrimary = Color.FromArgb(243, 244, 246);
        private static readonly Color TextMuted = Color.FromArgb(140, 142, 165);

        private ModernCardPanel cardPanel = null!;
        private ModernTextBox txtLogin = null!;
        private ModernTextBox txtPassword = null!;
        private ModernCheckBox chkRememberMe = null!;
        private RoundedButton btnSubmit = null!;
        private Button btnSwitchMode = null!;
        private Label lblTitle = null!;

        private bool _isRegisterMode = false;

        public event EventHandler<AuthEventArgs>? AuthRequested;

        public string Username => txtLogin.Text;
        public string Password => txtPassword.Text;
        public bool RememberMe => chkRememberMe.Checked;
        public bool IsRegisterMode => _isRegisterMode;

        public Form2()
        {
            InitializeComponent();
            ApplyDarkModeToTitleBar();
            BuildCustomUI();
        }

        private void ApplyDarkModeToTitleBar()
        {
            int useDarkMode = 1;
            DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));
            try
            {
                int captionBGR = ColorTranslator.ToWin32(BgColor);
                int textBGR = ColorTranslator.ToWin32(TextPrimary);
                DwmSetWindowAttribute(Handle, DWMWA_CAPTION_COLOR, ref captionBGR, sizeof(int));
                DwmSetWindowAttribute(Handle, DWMWA_TEXT_COLOR, ref textBGR, sizeof(int));
            }
            catch { }
        }

        private void BuildCustomUI()
        {
            this.Text = "Авторизация";
            this.Size = new Size(380, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = BgColor;
            this.ForeColor = TextPrimary;

            lblTitle = new Label
            {
                Text = "ВХОД В СИСТЕМУ",
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Введите данные для продолжения",
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMuted,
                Location = new Point(22, 48),
                AutoSize = true
            };

            cardPanel = new ModernCardPanel("АККАУНТ")
            {
                Location = new Point(20, 80),
                Size = new Size(325, 200)
            };

            Label lblLoginText = new Label
            {
                Text = "Логин / Email:",
                Location = new Point(15, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMuted
            };
            txtLogin = new ModernTextBox
            {
                Location = new Point(15, 52),
                Size = new Size(295, 30)
            };

            Label lblPassText = new Label
            {
                Text = "Пароль:",
                Location = new Point(15, 90),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = TextMuted
            };
            txtPassword = new ModernTextBox
            {
                Location = new Point(15, 112),
                Size = new Size(295, 30),
                IsPassword = true
            };

            chkRememberMe = new ModernCheckBox
            {
                Text = "Запомнить меня",
                Location = new Point(15, 156),
                Size = new Size(200, 22)
            };

            cardPanel.Controls.AddRange(new Control[] { lblLoginText, txtLogin, lblPassText, txtPassword, chkRememberMe });

            btnSubmit = new RoundedButton
            {
                Text = "ВОЙТИ",
                Location = new Point(20, 295),
                Size = new Size(325, 40),
                BorderRadius = 10,
                UseGradient = true,
                GradientStart = AccentPurple,
                GradientEnd = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };

            btnSubmit.Click += (s, e) =>
            {
                AuthRequested?.Invoke(this, new AuthEventArgs(Username, Password, RememberMe, _isRegisterMode));
            };

            btnSwitchMode = new Button
            {
                Text = "Нет аккаунта? Зарегистрироваться",
                Location = new Point(20, 345),
                Size = new Size(325, 30),
                BackColor = Color.Transparent,
                ForeColor = AccentPurple,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5F)
            };
            btnSwitchMode.FlatAppearance.BorderSize = 0;
            btnSwitchMode.FlatAppearance.MouseOverBackColor = Color.FromArgb(10, Color.White);

            btnSwitchMode.Click += (s, e) =>
            {
                _isRegisterMode = !_isRegisterMode;
                if (_isRegisterMode)
                {
                    lblTitle.Text = "РЕГИСТРАЦИЯ";
                    btnSubmit.Text = "ЗАРЕГИСТРИРОВАТЬСЯ";
                    btnSwitchMode.Text = "Уже есть аккаунт? Войти";
                    chkRememberMe.Visible = false;
                }
                else
                {
                    lblTitle.Text = "ВХОД В СИСТЕМУ";
                    btnSubmit.Text = "ВОЙТИ";
                    btnSwitchMode.Text = "Нет аккаунта? Зарегистрироваться";
                    chkRememberMe.Visible = true;
                }
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, cardPanel, btnSubmit, btnSwitchMode });
        }

        public static GraphicsPath GetRoundRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);
            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class AuthEventArgs : EventArgs
    {
        public string Username { get; }
        public string Password { get; }
        public bool RememberMe { get; }
        public bool IsRegister { get; }

        public AuthEventArgs(string username, string password, bool rememberMe, bool isRegister)
        {
            Username = username;
            Password = password;
            RememberMe = rememberMe;
            IsRegister = isRegister;
        }
    }
}