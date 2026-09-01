using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BHEIIIKA
{
    public partial class Form1 : Form
    {
        // --- DWM API для переключения темы и цвета заголовка Windows ---
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35; // Windows 11+
        private const int DWMWA_TEXT_COLOR = 36;    // Windows 11+

        private static readonly Color BgColor = Color.FromArgb(20, 18, 28);
        private static readonly Color CardBg = Color.FromArgb(30, 27, 43);
        private static readonly Color InputBg = Color.FromArgb(16, 14, 23);
        private static readonly Color AccentPurple = Color.FromArgb(217, 70, 239);
        private static readonly Color AccentBlue = Color.FromArgb(99, 102, 241);
        private static readonly Color TextPrimary = Color.FromArgb(243, 244, 246);
        private static readonly Color TextMuted = Color.FromArgb(140, 142, 165);

        private Button btnStart = null!;
        private Button btnOpen = null!;
        private Label lblStatus = null!;

        private ModernCardPanel gbColor = null!;
        private ModernRadioButton rbRed = null!;
        private ModernRadioButton rbYellow = null!;
        private ModernTextBox txtCustomHex = null!;
        private DarkNumericUpDown numTolerance = null!;

        private ModernCardPanel gbActivation = null!;
        private ModernRadioButton rbLmb = null!;
        private ModernRadioButton rbRmb = null!;

        private ModernCardPanel gbMainZone = null!;
        private DarkNumericUpDown numMainWidth = null!;
        private DarkNumericUpDown numMainHeight = null!;

        private ModernCardPanel gbNearZone = null!;
        private DarkNumericUpDown numNearWidth = null!;
        private DarkNumericUpDown numNearHeight = null!;

        private ModernCardPanel gbMotion = null!;
        private DarkNumericUpDown numMultiplier = null!;
        private DarkNumericUpDown numInterval = null!;
        private DarkNumericUpDown numMaxOffset = null!;

        private ModernCardPanel gbSearchRegion = null!;
        private ModernComboBox cbSearchMode = null!;

        private RichTextBox txtConsoleLog = null!;

        public Form1()
        {
            InitializeComponent();
            ApplyDarkModeToTitleBar(); // Тёмный системный заголовок
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
                Color captionColor = Color.FromArgb(20, 18, 28);
                int captionBGR = ColorTranslator.ToWin32(captionColor);
                DwmSetWindowAttribute(Handle, DWMWA_CAPTION_COLOR, ref captionBGR, sizeof(int));

                Color textColor = Color.FromArgb(243, 244, 246);
                int textBGR = ColorTranslator.ToWin32(textColor);
                DwmSetWindowAttribute(Handle, DWMWA_TEXT_COLOR, ref textBGR, sizeof(int));
            }
            catch { }
        }

        private void BuildCustomUI()
        {
            this.Text = "Панель управления";
            this.Size = new Size(540, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = BgColor;
            this.ForeColor = TextPrimary;

            btnStart = CreateGradientButton("▶ СТАРТ", new Point(20, 15), new Size(130, 42));
            btnOpen = CreateSoftButton("📁 Открыть", new Point(160, 15), new Size(120, 42));

            lblStatus = new Label
            {
                Text = "● Остановлено",
                Location = new Point(300, 26),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = AccentPurple
            };

            // 1. Цель и допуск
            gbColor = new ModernCardPanel("ЦЕЛЬ И ДОПУСК") { Location = new Point(20, 75), Size = new Size(240, 150) };
            rbRed = new ModernRadioButton { Text = "Красный (#FF0000)", Location = new Point(15, 35), Checked = true, Size = new Size(210, 22) };
            rbYellow = new ModernRadioButton { Text = "Жёлтый (#FFAB00)", Location = new Point(15, 60), Size = new Size(210, 22) };

            Label lblHex = new Label { Text = "HEX:", Location = new Point(15, 88), Size = new Size(100, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            txtCustomHex = new ModernTextBox { Text = "FF0000", Location = new Point(120, 88), Size = new Size(105, 25) };

            Label lblTol = new Label { Text = "Допуск (0-255):", Location = new Point(15, 116), Size = new Size(100, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numTolerance = new DarkNumericUpDown { Value = 35, Maximum = 255, Location = new Point(120, 116), Size = new Size(105, 25) };

            gbColor.Controls.AddRange(new Control[] { rbRed, rbYellow, lblHex, txtCustomHex, lblTol, numTolerance });

            // 2. Активация
            gbActivation = new ModernCardPanel("КНОПКА АКТИВАЦИИ") { Location = new Point(275, 75), Size = new Size(225, 150) };
            rbLmb = new ModernRadioButton { Text = "ЛКМ (Левая)", Location = new Point(20, 45), Size = new Size(180, 22) };
            rbRmb = new ModernRadioButton { Text = "ПКМ (Правая)", Location = new Point(20, 80), Checked = true, Size = new Size(180, 22) };
            gbActivation.Controls.AddRange(new Control[] { rbLmb, rbRmb });

            // 3. Основная зона
            gbMainZone = new ModernCardPanel("ОСНОВНАЯ ЗОНА") { Location = new Point(20, 240), Size = new Size(240, 100) };
            Label lblMW = new Label { Text = "Ширина (X):", Location = new Point(15, 35), Size = new Size(100, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numMainWidth = new DarkNumericUpDown { Value = 80, Location = new Point(120, 35), Size = new Size(105, 25) };
            Label lblMH = new Label { Text = "Высота (Y):", Location = new Point(15, 65), Size = new Size(100, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numMainHeight = new DarkNumericUpDown { Value = 45, Location = new Point(120, 65), Size = new Size(105, 25) };
            gbMainZone.Controls.AddRange(new Control[] { lblMW, numMainWidth, lblMH, numMainHeight });

            // 4. Ближняя зона
            gbNearZone = new ModernCardPanel("БЛИЖНЯЯ ЗОНА") { Location = new Point(275, 240), Size = new Size(225, 100) };
            Label lblNW = new Label { Text = "Ширина (X):", Location = new Point(15, 35), Size = new Size(90, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numNearWidth = new DarkNumericUpDown { Value = 45, Location = new Point(110, 35), Size = new Size(100, 25) };
            Label lblNH = new Label { Text = "Высота (Y):", Location = new Point(15, 65), Size = new Size(90, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numNearHeight = new DarkNumericUpDown { Value = 25, Location = new Point(110, 65), Size = new Size(100, 25) };
            gbNearZone.Controls.AddRange(new Control[] { lblNW, numNearWidth, lblNH, numNearHeight });

            // 5. Движение
            gbMotion = new ModernCardPanel("ПАРАМЕТРЫ ДВИЖЕНИЯ") { Location = new Point(20, 355), Size = new Size(480, 100) };
            Label lblMult = new Label { Text = "Множитель:", Location = new Point(15, 35), Size = new Size(90, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numMultiplier = new DarkNumericUpDown { Value = 3, Increment = 0.1m, DecimalPlaces = 1, Location = new Point(110, 35), Size = new Size(90, 25) };

            Label lblInt = new Label { Text = "Интервал (мс):", Location = new Point(220, 35), Size = new Size(110, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numInterval = new DarkNumericUpDown { Value = 10, Location = new Point(340, 35), Size = new Size(120, 25) };

            Label lblMaxOff = new Label { Text = "Макс. смещ.:", Location = new Point(15, 65), Size = new Size(90, 25), ForeColor = TextMuted, TextAlign = ContentAlignment.MiddleLeft };
            numMaxOffset = new DarkNumericUpDown { Value = 20, Location = new Point(110, 65), Size = new Size(90, 25) };

            gbMotion.Controls.AddRange(new Control[] { lblMult, numMultiplier, lblInt, numInterval, lblMaxOff, numMaxOffset });

            // 6. Область поиска
            gbSearchRegion = new ModernCardPanel("ОБЛАСТЬ ПОИСКА") { Location = new Point(20, 470), Size = new Size(480, 75) };
            cbSearchMode = new ModernComboBox { Location = new Point(15, 35), Size = new Size(450, 28) };
            cbSearchMode.Items.AddRange(new object[] { "Только ниже центра", "По всему экрану", "Только выше центра" });
            cbSearchMode.SelectedIndex = 0;
            gbSearchRegion.Controls.Add(cbSearchMode);

            // Консоль
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
            txtConsoleLog.AppendText($"[{DateTime.Now:HH:mm:ss}] Интерфейс обновлен.\n");

            this.Controls.AddRange(new Control[] {
                btnStart, btnOpen, lblStatus,
                gbColor, gbActivation, gbMainZone,
                gbNearZone, gbMotion, gbSearchRegion,
                txtConsoleLog
            });
        }

        private Button CreateGradientButton(string text, Point location, Size size)
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

        private Button CreateSoftButton(string text, Point location, Size size)
        {
            return new RoundedButton
            {
                Text = text,
                Location = location,
                Size = size,
                BorderRadius = 10,
                UseGradient = false,
                BackColor = CardBg,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold)
            };
        }

        public static GraphicsPath GetRoundRectPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            int diameter = radius * 2;
            if (diameter > rect.Width) diameter = rect.Width;
            if (diameter > rect.Height) diameter = rect.Height;

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

    // === СКРУГЛЕННАЯ АНИМИРОВАННАЯ КНОПКА ===
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
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;

            _animTimer = new System.Windows.Forms.Timer { Interval = 15 };
            _animTimer.Tick += AnimTimer_Tick;
        }

        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            if (_isHovered)
            {
                _hoverProgress += 0.08f;
                if (_hoverProgress >= 1f) { _hoverProgress = 1f; _animTimer.Stop(); }
            }
            else
            {
                _hoverProgress -= 0.08f;
                if (_hoverProgress <= 0f) { _hoverProgress = 0f; _animTimer.Stop(); }
            }
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            _animTimer.Start();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            _animTimer.Start();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _isPressed = false;
            Invalidate();
        }

        private Color InterpolateColor(Color c1, Color c2, float factor)
        {
            int r = (int)(c1.R + (c2.R - c1.R) * factor);
            int g = (int)(c1.G + (c2.G - c1.G) * factor);
            int b = (int)(c1.B + (c2.B - c1.B) * factor);
            return Color.FromArgb(r, g, b);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            using (GraphicsPath path = Form1.GetRoundRectPath(rect, BorderRadius))
            {
                this.Region = new Region(path);

                if (UseGradient)
                {
                    Color cStart = InterpolateColor(GradientStart, Color.FromArgb(240, 100, 255), _hoverProgress);
                    Color cEnd = InterpolateColor(GradientEnd, Color.FromArgb(130, 130, 255), _hoverProgress);

                    using (var brush = new LinearGradientBrush(ClientRectangle, cStart, cEnd, LinearGradientMode.Horizontal))
                    {
                        g.FillPath(brush, path);
                    }
                }
                else
                {
                    Color hoverBg = Color.FromArgb(Math.Min(255, BackColor.R + 20), Math.Min(255, BackColor.G + 20), Math.Min(255, BackColor.B + 25));
                    Color currentBg = InterpolateColor(BackColor, hoverBg, _hoverProgress);

                    using (var brush = new SolidBrush(currentBg))
                    {
                        g.FillPath(brush, path);
                    }
                }

                Rectangle textRect = ClientRectangle;
                if (_isPressed)
                {
                    textRect.Offset(1, 1);
                }

                TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }
    }

    // === СКРУГЛЕННЫЕ И АНИМИРОВАННЫЕ РАДИОКНОПКИ ===
    public class ModernRadioButton : RadioButton
    {
        private readonly System.Windows.Forms.Timer _animTimer;
        private float _checkProgress = 0f;

        public ModernRadioButton()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9F);
            ForeColor = Color.FromArgb(243, 244, 246);

            _animTimer = new System.Windows.Forms.Timer { Interval = 15 };
            _animTimer.Tick += (s, e) =>
            {
                if (Checked)
                {
                    _checkProgress += 0.12f;
                    if (_checkProgress >= 1f) { _checkProgress = 1f; _animTimer.Stop(); }
                }
                else
                {
                    _checkProgress -= 0.12f;
                    if (_checkProgress <= 0f) { _checkProgress = 0f; _animTimer.Stop(); }
                }
                Invalidate();
            };
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            _animTimer.Start();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Не рисуем стандартный прямоугольный фон
        }

        private Color InterpolateColor(Color c1, Color c2, float factor)
        {
            int r = (int)(c1.R + (c2.R - c1.R) * factor);
            int g = (int)(c1.G + (c2.G - c1.G) * factor);
            int b = (int)(c1.B + (c2.B - c1.B) * factor);
            return Color.FromArgb(r, g, b);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color bg = Parent != null ? Parent.BackColor : Color.FromArgb(30, 27, 43);
            using (SolidBrush bgBrush = new SolidBrush(bg))
            {
                g.FillRectangle(bgBrush, ClientRectangle);
            }

            int circleSize = 16;
            int circleX = 1;
            int circleY = (Height - circleSize) / 2;
            RectangleF circleRect = new RectangleF(circleX, circleY, circleSize, circleSize);

            Color inactiveBorder = Color.FromArgb(70, 65, 90);
            Color activeBorder = Color.FromArgb(217, 70, 239);
            Color currentBorder = InterpolateColor(inactiveBorder, activeBorder, _checkProgress);

            Color inactiveBg = Color.FromArgb(16, 14, 23);
            Color currentBg = InterpolateColor(inactiveBg, Color.FromArgb(35, 25, 48), _checkProgress);

            using (SolidBrush innerBg = new SolidBrush(currentBg))
            {
                g.FillEllipse(innerBg, circleRect);
            }

            using (Pen borderPen = new Pen(currentBorder, 1.8f))
            {
                g.DrawEllipse(borderPen, circleRect);
            }

            if (_checkProgress > 0f)
            {
                float maxDotSize = 8f;
                float currentDotSize = maxDotSize * _checkProgress;
                float dotX = circleX + (circleSize - currentDotSize) / 2f;
                float dotY = circleY + (circleSize - currentDotSize) / 2f;
                RectangleF dotRect = new RectangleF(dotX, dotY, currentDotSize, currentDotSize);

                using (LinearGradientBrush dotBrush = new LinearGradientBrush(
                    dotRect,
                    Color.FromArgb(217, 70, 239),
                    Color.FromArgb(99, 102, 241),
                    45f))
                {
                    g.FillEllipse(dotBrush, dotRect);
                }
            }

            Rectangle textRect = new Rectangle(circleSize + 10, 0, Width - circleSize - 10, Height);
            TextRenderer.DrawText(g, Text, Font, textRect, ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }

    // === СКРУГЛЕННЫЙ NUMERIC UP DOWN ===
    public class DarkNumericUpDown : Panel
    {
        private readonly TextBox _textBox;
        private readonly Button _btnUp;
        private readonly Button _btnDown;

        private decimal _value = 0;
        private decimal _minimum = 0;
        private decimal _maximum = 100;
        private decimal _increment = 1;
        private int _decimalPlaces = 0;
        private readonly int _borderRadius = 5;

        private static readonly Color BorderColor = Color.FromArgb(99, 102, 241);

        public decimal Value
        {
            get => _value;
            set { _value = Math.Max(_minimum, Math.Min(_maximum, value)); UpdateText(); }
        }

        public decimal Minimum
        {
            get => _minimum;
            set { _minimum = value; if (_value < _minimum) Value = _minimum; }
        }

        public decimal Maximum
        {
            get => _maximum;
            set { _maximum = value; if (_value > _maximum) Value = _maximum; }
        }

        public decimal Increment
        {
            get => _increment;
            set => _increment = value;
        }

        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set { _decimalPlaces = value; UpdateText(); }
        }

        public DarkNumericUpDown()
        {
            this.Size = new Size(100, 25);
            this.BackColor = Color.FromArgb(16, 14, 23);
            this.Padding = new Padding(5, 3, 2, 3);

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(16, 14, 23),
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9.5F),
                Dock = DockStyle.Fill
            };
            _textBox.TextChanged += TextBox_TextChanged;

            Panel btnPanel = new Panel
            {
                Width = 18,
                Dock = DockStyle.Right,
                BackColor = Color.FromArgb(16, 14, 23)
            };

            _btnUp = new Button { Dock = DockStyle.Top, Height = 11, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            _btnUp.FlatAppearance.BorderSize = 0;
            _btnUp.Paint += (s, e) => DrawArrow(e.Graphics, _btnUp.ClientRectangle, true);
            _btnUp.Click += (s, e) => Value += _increment;

            _btnDown = new Button { Dock = DockStyle.Bottom, Height = 11, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            _btnDown.FlatAppearance.BorderSize = 0;
            _btnDown.Paint += (s, e) => DrawArrow(e.Graphics, _btnDown.ClientRectangle, false);
            _btnDown.Click += (s, e) => Value -= _increment;

            btnPanel.Controls.Add(_btnUp);
            btnPanel.Controls.Add(_btnDown);

            this.Controls.Add(_textBox);
            this.Controls.Add(btnPanel);

            UpdateText();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width, Height), _borderRadius))
            {
                this.Region = new Region(path);
            }
        }

        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            if (decimal.TryParse(_textBox.Text, out decimal parsed))
            {
                _value = Math.Max(_minimum, Math.Min(_maximum, parsed));
            }
        }

        private void UpdateText()
        {
            string format = _decimalPlaces > 0 ? "0." + new string('0', _decimalPlaces) : "0";
            if (_textBox != null)
            {
                _textBox.Text = _value.ToString(format);
            }
        }

        private void DrawArrow(Graphics g, Rectangle rect, bool up)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(16, 14, 23)))
            {
                g.FillRectangle(bg, rect);
            }

            int cx = rect.Width / 2;
            int cy = rect.Height / 2;

            Point[] arrow = up
                ? new Point[] { new Point(cx, cy - 2), new Point(cx - 3, cy + 2), new Point(cx + 3, cy + 2) }
                : new Point[] { new Point(cx, cy + 2), new Point(cx - 3, cy - 2), new Point(cx + 3, cy - 2) };

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(217, 70, 239)))
            {
                g.FillPolygon(brush, arrow);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Form1.GetRoundRectPath(rect, _borderRadius))
            using (Pen borderPen = new Pen(BorderColor, 1.5f))
            {
                e.Graphics.DrawPath(borderPen, path);
            }
        }
    }

    // === СКРУГЛЕННЫЙ TEXTBOX (С ИСПРАВЛЕНИЯМИ) ===
    public class ModernTextBox : Panel
    {
        private readonly TextBox _innerTextBox;
        private static readonly Color BorderColor = Color.FromArgb(99, 102, 241);
        private readonly int _borderRadius = 5;

        [System.Diagnostics.CodeAnalysis.AllowNull]
        public override string Text
        {
            get => _innerTextBox.Text;
            set => _innerTextBox.Text = value ?? string.Empty;
        }

        public bool IsPassword
        {
            get => _innerTextBox.UseSystemPasswordChar;
            set => _innerTextBox.UseSystemPasswordChar = value;
        }

        public ModernTextBox()
        {
            this.Size = new Size(100, 25);
            this.BackColor = Color.FromArgb(16, 14, 23);
            this.Padding = new Padding(6, 3, 6, 3);

            _innerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(16, 14, 23),
                ForeColor = Color.FromArgb(243, 244, 246),
                Font = new Font("Segoe UI", 9.5F),
                Dock = DockStyle.Fill
            };

            this.Controls.Add(_innerTextBox);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width, Height), _borderRadius))
            {
                this.Region = new Region(path);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Form1.GetRoundRectPath(rect, _borderRadius))
            using (Pen borderPen = new Pen(BorderColor, 1.5f))
            {
                e.Graphics.DrawPath(borderPen, path);
            }
        }
    }

    // === СКРУГЛЕННЫЙ COMBOBOX ===
    public class ModernComboBox : ComboBox
    {
        private static readonly Color BorderColor = Color.FromArgb(99, 102, 241);
        private static readonly Color BgColor = Color.FromArgb(16, 14, 23);
        private static readonly Color ArrowColor = Color.FromArgb(217, 70, 239);
        private readonly int _borderRadius = 5;

        public ModernComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            FlatStyle = FlatStyle.Flat;
            BackColor = BgColor;
            ForeColor = Color.FromArgb(243, 244, 246);
            Font = new Font("Segoe UI", 9.5F);
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 22;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            using (GraphicsPath path = Form1.GetRoundRectPath(new Rectangle(0, 0, Width, Height), _borderRadius))
            {
                this.Region = new Region(path);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bg = isSelected ? Color.FromArgb(30, 27, 43) : BgColor;
            Color text = isSelected ? ArrowColor : ForeColor;

            using (SolidBrush b = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(b, e.Bounds);
            }

            if (Items[e.Index] is string itemText)
            {
                TextRenderer.DrawText(e.Graphics, itemText, Font, e.Bounds, text, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
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

                    using (SolidBrush bg = new SolidBrush(BgColor))
                    {
                        g.FillRectangle(bg, ClientRectangle);
                    }

                    if (SelectedItem != null)
                    {
                        Rectangle textRect = new Rectangle(10, 0, Width - 35, Height);
                        TextRenderer.DrawText(g, SelectedItem.ToString(), Font, textRect, ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                    }

                    Point[] arrow = new Point[]
                    {
                        new Point(Width - 18, Height / 2 - 2),
                        new Point(Width - 8, Height / 2 - 2),
                        new Point(Width - 13, Height / 2 + 4)
                    };
                    using (SolidBrush arrowBrush = new SolidBrush(ArrowColor))
                    {
                        g.FillPolygon(arrowBrush, arrow);
                    }

                    Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
                    using (GraphicsPath path = Form1.GetRoundRectPath(rect, _borderRadius))
                    using (Pen borderPen = new Pen(BorderColor, 1.5f))
                    {
                        g.DrawPath(borderPen, path);
                    }
                }
            }
        }
    }

    // === КАРТОЧКА-КОНТЕЙНЕР ===
    public class ModernCardPanel : Panel
    {
        public string Title { get; set; }

        public ModernCardPanel(string title)
        {
            Title = title;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(30, 27, 43);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = 10;

            using (GraphicsPath path = Form1.GetRoundRectPath(rect, radius))
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(30, 27, 43)))
                {
                    g.FillPath(brush, path);
                }
                using (Pen pen = new Pen(Color.FromArgb(55, 50, 75), 1))
                {
                    g.DrawPath(pen, path);
                }
            }

            using (Font titleFont = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(217, 70, 239)))
            {
                g.DrawString(Title, titleFont, titleBrush, new PointF(14, 10));
            }
        }
    }
}
