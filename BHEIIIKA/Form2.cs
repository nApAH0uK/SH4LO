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
        private static readonly Color CardBg = Color.FromArgb(30, 27, 43);
        private static readonly Color AccentPurple = Color.FromArgb(217, 70, 239);
        private static readonly Color AccentBlue = Color.FromArgb(99, 102, 241);
        private static readonly Color TextPrimary = Color.FromArgb(243, 244, 246);
        private static readonly Color TextMuted = Color.FromArgb(140, 142, 165);

        // Используем элементы управления, уже объявленные в Form1.cs
        private ModernCardPanel cardPanel = null!;
        private ModernTextBox txtLogin = null!;
        private ModernTextBox txtPassword = null!;
        private ModernCheckBox chkRememberMe = null!;
        private RoundedButton btnSubmit = null!;
        private LanguageToggleSwitch btnLang = null!;
        private Button btnSwitchMode = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Label lblLoginText = null!;
        private Label lblPassText = null!;

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
            UpdateLanguage();
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
            this.Size = new Size(380, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = BgColor;
            this.ForeColor = TextPrimary;

            lblTitle = new Label
            {
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(20, 20),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextMuted,
                Location = new Point(22, 48),
                AutoSize = true
            };

            // Переключатель языка использует класс из Form1.cs
            btnLang = new LanguageToggleSwitch
            {
                Location = new Point(265, 23),
                Size = new Size(80, 30)
            };
            btnLang.LanguageChanged += (s, e) => UpdateLanguage();

            cardPanel = new ModernCardPanel("")
            {
                Location = new Point(20, 80),
                Size = new Size(325, 200)
            };

            lblLoginText = new Label
            {
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

            lblPassText = new Label
            {
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

            // Чекбокс объявлен ниже в этом файле
            chkRememberMe = new ModernCheckBox
            {
                Location = new Point(15, 156),
                Size = new Size(200, 22)
            };

            cardPanel.Controls.AddRange(new Control[] { lblLoginText, txtLogin, lblPassText, txtPassword, chkRememberMe });

            btnSubmit = new RoundedButton
            {
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
                chkRememberMe.Visible = !_isRegisterMode;
                UpdateLanguage();
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, btnLang, cardPanel, btnSubmit, btnSwitchMode });
        }

        private void UpdateLanguage()
        {
            bool en = GlobalSettings.IsEnglish;
            this.Text = en ? "Authorization" : "Авторизация";

            if (_isRegisterMode)
            {
                lblTitle.Text = en ? "REGISTER" : "РЕГИСТРАЦИЯ";
                btnSubmit.Text = en ? "SIGN UP" : "ЗАРЕГИСТРИРОВАТЬСЯ";
                btnSwitchMode.Text = en ? "Already have an account? Login" : "Уже есть аккаунт? Войти";
            }
            else
            {
                lblTitle.Text = en ? "LOGIN" : "ВХОД В СИСТЕМУ";
                btnSubmit.Text = en ? "LOGIN" : "ВОЙТИ";
                btnSwitchMode.Text = en ? "No account? Sign up" : "Нет аккаунта? Зарегистрироваться";
            }

            lblSubtitle.Text = en ? "Enter your details to continue" : "Введите данные для продолжения";
            cardPanel.Title = en ? "ACCOUNT" : "АККАУНТ";
            lblLoginText.Text = en ? "Login / Email:" : "Логин / Email:";
            lblPassText.Text = en ? "Password:" : "Пароль:";
            chkRememberMe.Text = en ? "Remember me" : "Запомнить меня";
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

    // Единственный элемент, уникальный для Form2 — это чекбокс
    public class ModernCheckBox : CheckBox
    {
        private readonly System.Windows.Forms.Timer _animTimer;
        private float _checkProgress = 0f;
        public ModernCheckBox()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent; Cursor = Cursors.Hand; Font = new Font("Segoe UI", 9F); ForeColor = Color.FromArgb(243, 244, 246);
            _animTimer = new System.Windows.Forms.Timer { Interval = 15 };
            _animTimer.Tick += (s, e) => { if (Checked) { _checkProgress += 0.15f; if (_checkProgress >= 1f) { _checkProgress = 1f; _animTimer.Stop(); } } else { _checkProgress -= 0.15f; if (_checkProgress <= 0f) { _checkProgress = 0f; _animTimer.Stop(); } } Invalidate(); };
        }
        protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e); _animTimer.Start(); }
        protected override void OnPaintBackground(PaintEventArgs pevent) { }
        private Color InterpolateColor(Color c1, Color c2, float factor) { return Color.FromArgb((int)(c1.R + (c2.R - c1.R) * factor), (int)(c1.G + (c2.G - c1.G) * factor), (int)(c1.B + (c2.B - c1.B) * factor)); }
        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Color bg = Parent != null ? Parent.BackColor : Color.FromArgb(30, 27, 43);
            using (SolidBrush bgBrush = new SolidBrush(bg)) g.FillRectangle(bgBrush, ClientRectangle);
            int boxSize = 16, boxX = 1, boxY = (Height - boxSize) / 2;
            Rectangle boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);
            Color currentBorder = InterpolateColor(Color.FromArgb(70, 65, 90), Color.FromArgb(217, 70, 239), _checkProgress);
            Color currentBg = InterpolateColor(Color.FromArgb(16, 14, 23), Color.FromArgb(217, 70, 239), _checkProgress);

            // Используем Form1.GetRoundRectPath для отрисовки чекбокса
            using (System.Drawing.Drawing2D.GraphicsPath path = Form1.GetRoundRectPath(boxRect, 4))
            {
                using (SolidBrush fillBrush = new SolidBrush(currentBg)) g.FillPath(fillBrush, path);
                using (Pen borderPen = new Pen(currentBorder, 1.5f)) g.DrawPath(borderPen, path);
            }

            if (_checkProgress > 0f)
            {
                using (Pen checkPen = new Pen(Color.White, 2f)) g.DrawLines(checkPen, new PointF[] { new PointF(boxX + 3.5f, boxY + 8f), new PointF(boxX + 6.5f, boxY + 11.5f), new PointF(boxX + 12.5f, boxY + 4.5f) });
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(boxSize + 10, 0, Width - boxSize - 10, Height), ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }
}