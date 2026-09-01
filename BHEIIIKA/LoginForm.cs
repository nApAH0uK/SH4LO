using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BHEIIIKA
{
    public partial class LoginForm : Form
    {
        // DWM API для темной темы заголовка Windows
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        // Палитра
        private static readonly Color BgColor = Color.FromArgb(20, 18, 28);
        private static readonly Color CardBg = Color.FromArgb(30, 27, 43);
        private static readonly Color InputBg = Color.FromArgb(16, 14, 23);
        private static readonly Color AccentPurple = Color.FromArgb(217, 70, 239);
        private static readonly Color AccentBlue = Color.FromArgb(99, 102, 241);
        private static readonly Color TextPrimary = Color.FromArgb(243, 244, 246);
        private static readonly Color TextMuted = Color.FromArgb(140, 142, 165);

        // Элементы
        private ModernCardPanel cardPanel = null!;
        private ModernTextBox txtLogin = null!;
        private ModernTextBox txtPassword = null!;
        private ModernCheckBox chkRememberMe = null!;
        private RoundedButton btnLogin = null!;

        // --- СОБЫТИЯ И СВОЙСТВА ДЛЯ БЭКЕНДЕРА ---
        public event EventHandler<LoginEventArgs>? LoginRequested;

        public string Username => txtLogin.Text;
        public string Password => txtPassword.Text;
        public bool RememberMe => chkRememberMe.Checked;

        public LoginForm()
        {
            InitializeComponent();
            ApplyDarkModeToTitleBar();
            BuildCustomUI();
        }

        private void ApplyDarkModeToTitleBar()
        {
            int useDarkMode = 1;
            if (DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useDarkMode, sizeof(int));
            }

            try
            {
                Color captionColor = BgColor;
                int captionBGR = ColorTranslator.ToWin32(captionColor);
                DwmSetWindowAttribute(Handle, DWMWA_CAPTION_COLOR, ref captionBGR, sizeof(int));

                Color textColor = TextPrimary;
                int textBGR = ColorTranslator.ToWin32(textColor);
                DwmSetWindowAttribute(Handle, DWMWA_TEXT_COLOR, ref textBGR, sizeof(int));
            }
            catch { }
        }

        private void BuildCustomUI()
        {
            this.Text = "Авторизация";
            this.Size = new Size(380, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = BgColor;
            this.ForeColor = TextPrimary;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "ВХОД В СИСТЕМУ",
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(20, 20),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Введите данные для авторизации",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = TextMuted,
                Location = new Point(22, 48),
                AutoSize = true
            };

            // Карточка с полями
            cardPanel = new ModernCardPanel("АВТОРИЗАЦИЯ")
            {
                Location = new Point(20, 80),
                Size = new Size(325, 200)
            };

            // Поле Логин
            Label lblLogin = new Label
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

            // Поле Пароль
            Label lblPassword = new Label
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

            // Чекбокс (теперь берется из Form2/Form1)
            chkRememberMe = new ModernCheckBox
            {
                Text = "Запомнить меня",
                Location = new Point(15, 156),
                Size = new Size(200, 22)
            };

            cardPanel.Controls.AddRange(new Control[] { lblLogin, txtLogin, lblPassword, txtPassword, chkRememberMe });

            // Кнопка Вход
            btnLogin = new RoundedButton
            {
                Text = "ВОЙТИ",
                Location = new Point(20, 300),
                Size = new Size(325, 42),
                BorderRadius = 10,
                UseGradient = true,
                GradientStart = AccentPurple,
                GradientEnd = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };

            btnLogin.Click += (s, e) =>
            {
                LoginRequested?.Invoke(this, new LoginEventArgs(Username, Password, RememberMe));
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, cardPanel, btnLogin });
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

    public class LoginEventArgs : EventArgs
    {
        public string Username { get; }
        public string Password { get; }
        public bool RememberMe { get; }

        public LoginEventArgs(string username, string password, bool rememberMe)
        {
            Username = username;
            Password = password;
            RememberMe = rememberMe;
        }
    }
}