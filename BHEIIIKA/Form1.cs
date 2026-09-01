using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BHEIIIKA
{
    // Общий класс для хранения текущего языка
    public static class GlobalSettings
    {
        public static bool IsEnglish { get; set; } = false;
    }

    public partial class Form1 : Form
    {
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // API для применения темной темы к стандартным элементам Windows (скроллбару)
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        private static readonly Color BgColor = Color.FromArgb(20, 18, 28);
        private static readonly Color CardBg = Color.FromArgb(30, 27, 43);
        private static readonly Color InputBg = Color.FromArgb(16, 14, 23);
        private static readonly Color AccentPurple = Color.FromArgb(217, 70, 239);
        private static readonly Color AccentBlue = Color.FromArgb(99, 102, 241);
        private static readonly Color TextPrimary = Color.FromArgb(243, 244, 246);
        private static readonly Color TextMuted = Color.FromArgb(140, 142, 165);

        private bool _isRunning = false;

        private RoundedButton btnStart = null!;
        private LanguageToggleSwitch btnLang = null!;
        private Label lblStatus = null!;

        private ModernCardPanel gbColor = null!;
        private ModernRadioButton rbRed = null!;
        private ModernRadioButton rbYellow = null!;
        private Label lblHex = null!, lblTol = null!;
        private ModernTextBox txtCustomHex = null!;
        private DarkNumericUpDown numTolerance = null!;

        private ModernCardPanel gbActivation = null!;
        private ModernRadioButton rbLmb = null!;
        private ModernRadioButton rbRmb = null!;

        private ModernCardPanel gbMainZone = null!;
        private Label lblMW = null!, lblMH = null!;
        private DarkNumericUpDown numMainWidth = null!;
        private DarkNumericUpDown numMainHeight = null!;

        private ModernCardPanel gbNearZone = null!;
        private Label lblNW = null!, lblNH = null!;
        private DarkNumericUpDown numNearWidth = null!;
        private DarkNumericUpDown numNearHeight = null!;

        private ModernCardPanel gbMotion = null!;
        private Label lblMult = null!, lblInt = null!, lblMaxOff = null!;
        private DarkNumericUpDown numMultiplier = null!;
        private DarkNumericUpDown numInterval = null!;
        private DarkNumericUpDown numMaxOffset = null!;

        private ModernCardPanel gbSearchRegion = null!;
        private ModernComboBox cbSearchMode = null!;

        private RichTextBox txtConsoleLog = null!;

        public Form1()
        {
            InitializeComponent();
            ApplyDarkModeToTitleBar();
            BuildCustomUI();
            UpdateLanguage(); // Применяем язык при загрузке
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
                int captionBGR = ColorTranslator.ToWin32(Color.FromArgb(20, 18, 28));
                DwmSetWindowAttribute(Handle, DWMWA_CAPTION_COLOR, ref captionBGR, sizeof(int));
                int textBGR = ColorTranslator.ToWin32(Color.FromArgb(243, 244, 246));
                DwmSetWindowAttribute(Handle, DWMWA_TEXT_COLOR, ref textBGR, sizeof(int));
            }
            catch { }
        }

        private void BuildCustomUI()
        {
            this.Size = new Size(540, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = BgColor;
            this.ForeColor = TextPrimary;

            btnStart = CreateGradientButton("▶ СТАРТ", new Point(20, 15), new Size(130, 42));
            btnStart.Click += BtnStart_Click;

            btnLang = new LanguageToggleSwitch
            {
                Location = new Point(160, 15),
                Size = new Size(110, 42)
            };
            btnLang.LanguageChanged += (s, e) => UpdateLanguage();

            lblStatus = new Label
            {
                Text = "● Остановлено",
                Location = new Point(280, 26),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = AccentPurple
            };

            gbColor = new ModernCardPanel("ЦЕЛЬ И ДОПУСК") { Location = new Point(20, 75), Size = new Size(240, 150) };
            rbRed = new ModernRadioButton { Text = "Красный (#FF0000)", Location = new Point(15, 35), Checked = true, Size = new Size(210, 22) };
            rbYellow = new ModernRadioButton { Text = "Жёлтый (#FFAB00)", Location = new Point(15, 60), Size = new Size(210, 22) };
            lblHex = new Label { Location = new Point(15, 88), Size = new Size(100, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            txtCustomHex = new ModernTextBox { Text = "FF0000", Location = new Point(120, 88), Size = new Size(105, 25) };
            lblTol = new Label { Location = new Point(15, 116), Size = new Size(100, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numTolerance = new DarkNumericUpDown { Value = 35, Maximum = 255, Location = new Point(120, 116), Size = new Size(105, 25) };
            gbColor.Controls.AddRange(new Control[] { rbRed, rbYellow, lblHex, txtCustomHex, lblTol, numTolerance });

            gbActivation = new ModernCardPanel("КНОПКА АКТИВАЦИИ") { Location = new Point(275, 75), Size = new Size(225, 150) };
            rbLmb = new ModernRadioButton { Location = new Point(20, 45), Size = new Size(180, 22) };
            rbRmb = new ModernRadioButton { Location = new Point(20, 80), Checked = true, Size = new Size(180, 22) };
            gbActivation.Controls.AddRange(new Control[] { rbLmb, rbRmb });

            gbMainZone = new ModernCardPanel("ОСНОВНАЯ ЗОНА") { Location = new Point(20, 240), Size = new Size(240, 100) };
            lblMW = new Label { Location = new Point(15, 35), Size = new Size(100, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numMainWidth = new DarkNumericUpDown { Value = 80, Location = new Point(120, 35), Size = new Size(105, 25) };
            lblMH = new Label { Location = new Point(15, 65), Size = new Size(100, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numMainHeight = new DarkNumericUpDown { Value = 45, Location = new Point(120, 65), Size = new Size(105, 25) };
            gbMainZone.Controls.AddRange(new Control[] { lblMW, numMainWidth, lblMH, numMainHeight });

            gbNearZone = new ModernCardPanel("БЛИЖНЯЯ ЗОНА") { Location = new Point(275, 240), Size = new Size(225, 100) };
            lblNW = new Label { Location = new Point(15, 35), Size = new Size(90, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numNearWidth = new DarkNumericUpDown { Value = 45, Location = new Point(110, 35), Size = new Size(100, 25) };
            lblNH = new Label { Location = new Point(15, 65), Size = new Size(90, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numNearHeight = new DarkNumericUpDown { Value = 25, Location = new Point(110, 65), Size = new Size(100, 25) };
            gbNearZone.Controls.AddRange(new Control[] { lblNW, numNearWidth, lblNH, numNearHeight });

            gbMotion = new ModernCardPanel("ПАРАМЕТРЫ ДВИЖЕНИЯ") { Location = new Point(20, 355), Size = new Size(480, 100) };
            lblMult = new Label { Location = new Point(15, 35), Size = new Size(90, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numMultiplier = new DarkNumericUpDown { Value = 3, Increment = 0.1m, DecimalPlaces = 1, Location = new Point(110, 35), Size = new Size(90, 25) };
            lblInt = new Label { Location = new Point(220, 35), Size = new Size(110, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numInterval = new DarkNumericUpDown { Value = 10, Location = new Point(340, 35), Size = new Size(120, 25) };
            lblMaxOff = new Label { Location = new Point(15, 65), Size = new Size(90, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numMaxOffset = new DarkNumericUpDown { Value = 20, Location = new Point(110, 65), Size = new Size(90, 25) };
            gbMotion.Controls.AddRange(new Control[] { lblMult, numMultiplier, lblInt, numInterval, lblMaxOff, numMaxOffset });

            gbSearchRegion = new ModernCardPanel("ОБЛАСТЬ ПОИСКА") { Location = new Point(20, 470), Size = new Size(480, 75) };
            cbSearchMode = new ModernComboBox { Location = new Point(15, 35), Size = new Size(450, 28) };
            gbSearchRegion.Controls.Add(cbSearchMode);

            txtConsoleLog = new RichTextBox
            {
                Location = new Point(20, 560),
                Size = new Size(480, 100),
                BackColor = InputBg,
                ForeColor = Color.FromArgb(192, 132, 252),
                Font = new Font("Consolas", 9.5F, FontStyle.Regular),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            // Применяем темную тему к полосе прокрутки
            txtConsoleLog.HandleCreated += (s, e) => SetWindowTheme(txtConsoleLog.Handle, "DarkMode_Explorer", null);

            this.Controls.AddRange(new Control[] {
                btnStart, btnLang, lblStatus,
                gbColor, gbActivation, gbMainZone,
                gbNearZone, gbMotion, gbSearchRegion,
                txtConsoleLog
            });
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            _isRunning = !_isRunning;
            UpdateLanguage();

            if (_isRunning)
            {
                lblStatus.ForeColor = Color.FromArgb(74, 222, 128);
                txtConsoleLog.AppendText(GlobalSettings.IsEnglish ? $"[{DateTime.Now:HH:mm:ss}] Process started.\n" : $"[{DateTime.Now:HH:mm:ss}] Процесс запущен.\n");
            }
            else
            {
                lblStatus.ForeColor = AccentPurple;
                txtConsoleLog.AppendText(GlobalSettings.IsEnglish ? $"[{DateTime.Now:HH:mm:ss}] Process stopped.\n" : $"[{DateTime.Now:HH:mm:ss}] Процесс остановлен.\n");
            }
            txtConsoleLog.ScrollToCaret();
        }

        private void UpdateLanguage()
        {
            bool en = GlobalSettings.IsEnglish;

            this.Text = en ? "Control Panel" : "Панель управления";

            if (_isRunning)
            {
                btnStart.Text = en ? "⏹ STOP" : "⏹ СТОП";
                lblStatus.Text = en ? "● Running" : "● Работает";
            }
            else
            {
                btnStart.Text = en ? "▶ START" : "▶ СТАРТ";
                lblStatus.Text = en ? "● Stopped" : "● Остановлено";
            }

            gbColor.Title = en ? "TARGET & TOLERANCE" : "ЦЕЛЬ И ДОПУСК";
            rbRed.Text = en ? "Red (#FF0000)" : "Красный (#FF0000)";
            rbYellow.Text = en ? "Yellow (#FFAB00)" : "Жёлтый (#FFAB00)";
            lblHex.Text = "HEX:";
            lblTol.Text = en ? "Tolerance (0-255):" : "Допуск (0-255):";

            gbActivation.Title = en ? "ACTIVATION KEY" : "КНОПКА АКТИВАЦИИ";
            rbLmb.Text = en ? "LMB (Left)" : "ЛКМ (Левая)";
            rbRmb.Text = en ? "RMB (Right)" : "ПКМ (Правая)";

            gbMainZone.Title = en ? "MAIN ZONE" : "ОСНОВНАЯ ЗОНА";
            lblMW.Text = en ? "Width (X):" : "Ширина (X):";
            lblMH.Text = en ? "Height (Y):" : "Высота (Y):";

            gbNearZone.Title = en ? "NEAR ZONE" : "БЛИЖНЯЯ ЗОНА";
            lblNW.Text = en ? "Width (X):" : "Ширина (X):";
            lblNH.Text = en ? "Height (Y):" : "Высота (Y):";

            gbMotion.Title = en ? "MOTION PARAMS" : "ПАРАМЕТРЫ ДВИЖЕНИЯ";
            lblMult.Text = en ? "Multiplier:" : "Множитель:";
            lblInt.Text = en ? "Interval (ms):" : "Интервал (мс):";
            lblMaxOff.Text = en ? "Max Offset:" : "Макс. смещ.:";

            gbSearchRegion.Title = en ? "SEARCH REGION" : "ОБЛАСТЬ ПОИСКА";

            int selectedIdx = Math.Max(0, cbSearchMode.SelectedIndex);
            cbSearchMode.Items.Clear();
            if (en) cbSearchMode.Items.AddRange(new object[] { "Below center only", "Full screen", "Above center only" });
            else cbSearchMode.Items.AddRange(new object[] { "Только ниже центра", "По всему экрану", "Только выше центра" });
            cbSearchMode.SelectedIndex = selectedIdx;
        }

        private RoundedButton CreateGradientButton(string text, Point location, Size size)
        {
            return new RoundedButton
            {
                Text = text,
                Location = location,
                Size = size,
                BorderRadius = 10,
                UseGradient = true,
                GradientStart = AccentPurple,
                GradientEnd = AccentBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            };
        }

        public static GraphicsPath GetRoundRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0) { path.AddRectangle(rect); path.CloseFigure(); return path; }
            int diameter = radius * 2;
            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;
            Rectangle arc = new Rectangle(rect.X, rect.Y, diameter, diameter);
            path.AddArc(arc, 180, 90); arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90); arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90); arc.X = rect.Left;
            path.AddArc(arc, 90, 90); path.CloseFigure();
            return path;
        }
    }

    public class LanguageToggleSwitch : Control
    {
        private float _progress = 0f;
        private readonly System.Windows.Forms.Timer _animTimer;

        public event EventHandler? LanguageChanged;

        public LanguageToggleSwitch()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI Black", 11.5F, FontStyle.Bold);
            _progress = GlobalSettings.IsEnglish ? 1f : 0f;

            _animTimer = new System.Windows.Forms.Timer { Interval = 15 };
            _animTimer.Tick += (s, e) =>
            {
                float target = GlobalSettings.IsEnglish ? 1f : 0f;
                float step = 0.1f;

                if (Math.Abs(_progress - target) <= step)
                {
                    _progress = target;
                    _animTimer.Stop();
                }
                else
                {
                    _progress += GlobalSettings.IsEnglish ? step : -step;
                }
                Invalidate();
            };
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            GlobalSettings.IsEnglish = !GlobalSettings.IsEnglish;
            _animTimer.Start();
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        private Color InterpolateColor(Color c1, Color c2, float factor)
        {
            return Color.FromArgb(
                (int)(c1.R + (c2.R - c1.R) * factor),
                (int)(c1.G + (c2.G - c1.G) * factor),
                (int)(c1.B + (c2.B - c1.B) * factor)
            );
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Form1.GetRoundRectPath(rect, 8))
            {
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(30, 27, 43)))
                {
                    e.Graphics.FillPath(bgBrush, path);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(70, 65, 90), 1.2f))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
            }

            Color active = Color.FromArgb(217, 70, 239);
            Color inactive = Color.FromArgb(100, 100, 120);

            Color enColor = InterpolateColor(inactive, active, _progress);
            Color ruColor = InterpolateColor(inactive, active, 1f - _progress);

            string textEN = "EN";
            string textSep = " \\ ";
            string textRU = "RU";

            SizeF sizeEN = e.Graphics.MeasureString(textEN, Font);
            SizeF sizeSep = e.Graphics.MeasureString(textSep, Font);
            SizeF sizeRU = e.Graphics.MeasureString(textRU, Font);

            float totalWidth = sizeEN.Width + sizeSep.Width + sizeRU.Width;
            float startX = (Width - totalWidth) / 2;
            float y = (Height - sizeEN.Height) / 2 + 1;

            using (SolidBrush enBrush = new SolidBrush(enColor))
                e.Graphics.DrawString(textEN, Font, enBrush, startX, y);

            using (SolidBrush sepBrush = new SolidBrush(inactive))
                e.Graphics.DrawString(textSep, Font, sepBrush, startX + sizeEN.Width, y);

            using (SolidBrush ruBrush = new SolidBrush(ruColor))
                e.Graphics.DrawString(textRU, Font, ruBrush, startX + sizeEN.Width + sizeSep.Width, y);
        }
    }

    public class RoundedButton : Button
    {
        public int BorderRadius { get; set; } = 8;
        public bool UseGradient { get; set; } = false;
        public Color GradientStart { get; set; } = Color.Purple;
        public Color GradientEnd { get; set; } = Color.Blue;
        private readonly System.Windows.Forms.Timer _animTimer;
        private float _hoverProgress = 0f;
        private bool _isHovered = false;
        private bool _isPressed = false;
        public RoundedButton()
        {
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand; DoubleBuffered = true;
            _animTimer = new System.Windows.Forms.Timer { Interval = 15 };
            _animTimer.Tick += AnimTimer_Tick;
        }
        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            if (_isHovered) { _hoverProgress += 0.08f; if (_hoverProgress >= 1f) { _hoverProgress = 1f; _animTimer.Stop(); } }
            else { _hoverProgress -= 0.08f; if (_hoverProgress <= 0f) { _hoverProgress = 0f; _animTimer.Stop(); } }
            Invalidate();
        }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _isHovered = true; _animTimer.Start(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _isHovered = false; _isPressed = false; _animTimer.Start(); }
        protected override void OnMouseDown(MouseEventArgs mevent) { base.OnMouseDown(mevent); _isPressed = true; Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs mevent) { base.OnMouseUp(mevent); _isPressed = false; Invalidate(); }
        private Color InterpolateColor(Color c1, Color c2, float factor)
        {
            return Color.FromArgb((int)(c1.R + (c2.R - c1.R) * factor), (int)(c1.G + (c2.G - c1.G) * factor), (int)(c1.B + (c2.B - c1.B) * factor));
        }
        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Form1.GetRoundRectPath(rect, BorderRadius))
            {
                this.Region = new Region(path);
                if (UseGradient)
                {
                    Color cStart = InterpolateColor(GradientStart, Color.FromArgb(240, 100, 255), _hoverProgress);
                    Color cEnd = InterpolateColor(GradientEnd, Color.FromArgb(130, 130, 255), _hoverProgress);
                    using (var brush = new LinearGradientBrush(ClientRectangle, cStart, cEnd, LinearGradientMode.Horizontal)) g.FillPath(brush, path);
                }
                else
                {
                    Color hoverBg = Color.FromArgb(Math.Min(255, BackColor.R + 20), Math.Min(255, BackColor.G + 20), Math.Min(255, BackColor.B + 25));
                    using (var brush = new SolidBrush(InterpolateColor(BackColor, hoverBg, _hoverProgress))) g.FillPath(brush, path);
                }
                Rectangle textRect = ClientRectangle; if (_isPressed) textRect.Offset(1, 1);
                TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }

    public class ModernRadioButton : RadioButton
    {
        private readonly System.Windows.Forms.Timer _animTimer;
        private float _checkProgress = 0f;
        public ModernRadioButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent; Cursor = Cursors.Hand; Font = new Font("Segoe UI", 9F); ForeColor = Color.FromArgb(243, 244, 246);
            _animTimer = new System.Windows.Forms.Timer { Interval = 15 };
            _animTimer.Tick += (s, e) => { if (Checked) { _checkProgress += 0.12f; if (_checkProgress >= 1f) { _checkProgress = 1f; _animTimer.Stop(); } } else { _checkProgress -= 0.12f; if (_checkProgress <= 0f) { _checkProgress = 0f; _animTimer.Stop(); } } Invalidate(); };
        }
        protected override void OnCheckedChanged(EventArgs e) { base.OnCheckedChanged(e); _animTimer.Start(); }
        protected override void OnPaintBackground(PaintEventArgs pevent) { }
        private Color InterpolateColor(Color c1, Color c2, float factor) { return Color.FromArgb((int)(c1.R + (c2.R - c1.R) * factor), (int)(c1.G + (c2.G - c1.G) * factor), (int)(c1.B + (c2.B - c1.B) * factor)); }
        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Color bg = Parent != null ? Parent.BackColor : Color.FromArgb(30, 27, 43);
            using (SolidBrush bgBrush = new SolidBrush(bg)) g.FillRectangle(bgBrush, ClientRectangle);
            int circleSize = 16, circleX = 1, circleY = (Height - circleSize) / 2;
            RectangleF circleRect = new RectangleF(circleX, circleY, circleSize, circleSize);
            Color currentBorder = InterpolateColor(Color.FromArgb(70, 65, 90), Color.FromArgb(217, 70, 239), _checkProgress);
            Color currentBg = InterpolateColor(Color.FromArgb(16, 14, 23), Color.FromArgb(35, 25, 48), _checkProgress);
            using (SolidBrush innerBg = new SolidBrush(currentBg)) g.FillEllipse(innerBg, circleRect);
            using (Pen borderPen = new Pen(currentBorder, 1.8f)) g.DrawEllipse(borderPen, circleRect);
            if (_checkProgress > 0f)
            {
                float curDot = 8f * _checkProgress;
                RectangleF dotRect = new RectangleF(circleX + (circleSize - curDot) / 2f, circleY + (circleSize - curDot) / 2f, curDot, curDot);
                using (LinearGradientBrush dotBrush = new LinearGradientBrush(dotRect, Color.FromArgb(217, 70, 239), Color.FromArgb(99, 102, 241), 45f)) g.FillEllipse(dotBrush, dotRect);
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(circleSize + 10, 0, Width - circleSize - 10, Height), ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }

    public class DarkNumericUpDown : Panel
    {
        private readonly TextBox _textBox;
        private readonly Button _btnUp, _btnDown;
        private decimal _value = 0, _minimum = 0, _maximum = 100, _increment = 1;
        private int _decimalPlaces = 0;
        private readonly int _borderRadius = 5;
        public decimal Value { get => _value; set { _value = Math.Max(_minimum, Math.Min(_maximum, value)); UpdateText(); } }
        public decimal Maximum { get => _maximum; set { _maximum = value; if (_value > _maximum) Value = _maximum; } }
        public decimal Increment { get => _increment; set => _increment = value; }
        public int DecimalPlaces { get => _decimalPlaces; set { _decimalPlaces = value; UpdateText(); } }
        public DarkNumericUpDown()
        {
            this.Size = new Size(100, 25); this.BackColor = Color.FromArgb(16, 14, 23); this.Padding = new Padding(5, 3, 2, 3);
            _textBox = new TextBox { BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(16, 14, 23), ForeColor = Color.FromArgb(243, 244, 246), Font = new Font("Segoe UI", 9.5F), Dock = DockStyle.Fill };
            _textBox.TextChanged += (s, e) => { if (decimal.TryParse(_textBox.Text, out decimal parsed)) _value = Math.Max(_minimum, Math.Min(_maximum, parsed)); };
            Panel btnPanel = new Panel { Width = 18, Dock = DockStyle.Right, BackColor = Color.FromArgb(16, 14, 23) };
            _btnUp = new Button { Dock = DockStyle.Top, Height = 11, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; _btnUp.FlatAppearance.BorderSize = 0;
            _btnUp.Paint += (s, e) => DrawArrow(e.Graphics, _btnUp.ClientRectangle, true); _btnUp.Click += (s, e) => Value += _increment;
            _btnDown = new Button { Dock = DockStyle.Bottom, Height = 11, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand }; _btnDown.FlatAppearance.BorderSize = 0;
            _btnDown.Paint += (s, e) => DrawArrow(e.Graphics, _btnDown.ClientRectangle, false); _btnDown.Click += (s, e) => Value -= _increment;
            btnPanel.Controls.Add(_btnUp); btnPanel.Controls.Add(_btnDown);
            this.Controls.Add(_textBox); this.Controls.Add(btnPanel);
            UpdateText();
        }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width, Height), _borderRadius)) this.Region = new Region(path); }
        private void UpdateText() { if (_textBox != null) _textBox.Text = _value.ToString(_decimalPlaces > 0 ? "0." + new string('0', _decimalPlaces) : "0"); }
        private void DrawArrow(Graphics g, Rectangle rect, bool up)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(16, 14, 23))) g.FillRectangle(bg, rect);
            int cx = rect.Width / 2, cy = rect.Height / 2;
            Point[] arrow = up ? new Point[] { new Point(cx, cy - 2), new Point(cx - 3, cy + 2), new Point(cx + 3, cy + 2) } : new Point[] { new Point(cx, cy + 2), new Point(cx - 3, cy - 2), new Point(cx + 3, cy - 2) };
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(217, 70, 239))) g.FillPolygon(brush, arrow);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width - 1, Height - 1), _borderRadius))
            using (Pen borderPen = new Pen(Color.FromArgb(99, 102, 241), 1.5f)) e.Graphics.DrawPath(borderPen, path);
        }
    }

    public class ModernTextBox : Panel
    {
        private readonly TextBox _innerTextBox;
        private readonly int _borderRadius = 5;
        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string Text { get => _innerTextBox.Text; set => _innerTextBox.Text = value ?? string.Empty; }
        public bool IsPassword { get => _innerTextBox.UseSystemPasswordChar; set => _innerTextBox.UseSystemPasswordChar = value; }
        public ModernTextBox()
        {
            this.Size = new Size(100, 25); this.BackColor = Color.FromArgb(16, 14, 23); this.Padding = new Padding(6, 3, 6, 3);
            _innerTextBox = new TextBox { BorderStyle = BorderStyle.None, BackColor = Color.FromArgb(16, 14, 23), ForeColor = Color.FromArgb(243, 244, 246), Font = new Font("Segoe UI", 9.5F), Dock = DockStyle.Fill };
            this.Controls.Add(_innerTextBox);
        }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width, Height), _borderRadius)) this.Region = new Region(path); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width - 1, Height - 1), _borderRadius))
            using (Pen borderPen = new Pen(Color.FromArgb(99, 102, 241), 1.5f)) e.Graphics.DrawPath(borderPen, path);
        }
    }

    public class ModernComboBox : ComboBox
    {
        private static readonly Color BgColor = Color.FromArgb(16, 14, 23);
        private readonly int _borderRadius = 5;
        public ModernComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList; FlatStyle = FlatStyle.Flat; BackColor = BgColor;
            ForeColor = Color.FromArgb(243, 244, 246); Font = new Font("Segoe UI", 9.5F); DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 22;
        }
        protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width, Height), _borderRadius)) this.Region = new Region(path); }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground(); if (e.Index < 0) return;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            using (SolidBrush b = new SolidBrush(isSelected ? Color.FromArgb(30, 27, 43) : BgColor)) e.Graphics.FillRectangle(b, e.Bounds);
            if (Items[e.Index] is string itemText) TextRenderer.DrawText(e.Graphics, itemText, Font, e.Bounds, isSelected ? Color.FromArgb(217, 70, 239) : ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            e.DrawFocusRectangle();
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0F) // WM_PAINT
            {
                using (Graphics g = Graphics.FromHwnd(Handle))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (SolidBrush bg = new SolidBrush(BgColor)) g.FillRectangle(bg, ClientRectangle);
                    if (SelectedItem != null) TextRenderer.DrawText(g, SelectedItem.ToString(), Font, new Rectangle(10, 0, Width - 35, Height), ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                    Point[] arrow = new Point[] { new Point(Width - 18, Height / 2 - 2), new Point(Width - 8, Height / 2 - 2), new Point(Width - 13, Height / 2 + 4) };
                    using (SolidBrush arrowBrush = new SolidBrush(Color.FromArgb(217, 70, 239))) g.FillPolygon(arrowBrush, arrow);
                    using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width - 1, Height - 1), _borderRadius))
                    using (Pen borderPen = new Pen(Color.FromArgb(99, 102, 241), 1.5f)) g.DrawPath(borderPen, path);
                }
            }
        }
    }

    public class ModernCardPanel : Panel
    {
        private string _title = "";
        public string Title
        {
            get => _title; set { _title = value; Invalidate(); }
        }
        public ModernCardPanel(string title) { Title = title; DoubleBuffered = true; BackColor = Color.FromArgb(30, 27, 43); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width - 1, Height - 1), 10))
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 27, 43))) g.FillPath(brush, path);
                using (Pen pen = new Pen(Color.FromArgb(55, 50, 75), 1)) g.DrawPath(pen, path);
            }
            using (Font titleFont = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(217, 70, 239))) g.DrawString(Title, titleFont, titleBrush, new PointF(14, 10));
        }
    }
}