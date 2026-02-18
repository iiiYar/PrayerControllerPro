using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Net;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Media;

namespace PrayerMediaApp
{
    // ==========================================
    // 🎨 THEME & CONSTANTS
    // ==========================================
    public static class UITheme
    {
        // Current Theme (يتم تحديثه عند تشغيل التطبيق)
        public static string CurrentTheme = "Dark";
        
        // DARK THEME - Midnight Blue Palette
        public static Color Dark_BgDark = Color.FromArgb(10, 15, 30);
        public static Color Dark_BgMedium = Color.FromArgb(20, 25, 45);
        public static Color Dark_BgLight = Color.FromArgb(35, 40, 65);
        public static Color Dark_TextPrimary = Color.FromArgb(240, 240, 255);
        public static Color Dark_TextSecondary = Color.FromArgb(160, 170, 190);
        public static Color Dark_TextMuted = Color.FromArgb(100, 110, 130);
        
        // LIGHT THEME - Clean White/Gray Palette
        public static Color Light_BgDark = Color.FromArgb(240, 240, 245);
        public static Color Light_BgMedium = Color.FromArgb(255, 255, 255);
        public static Color Light_BgLight = Color.FromArgb(230, 230, 235);
        public static Color Light_TextPrimary = Color.FromArgb(30, 30, 40);
        public static Color Light_TextSecondary = Color.FromArgb(80, 80, 90);
        public static Color Light_TextMuted = Color.FromArgb(120, 120, 130);
        
        // Dynamic Colors (تتغير حسب الـ Theme)
        public static Color BgDark
        {
            get { return CurrentTheme == "Dark" ? Dark_BgDark : Light_BgDark; }
        }
        public static Color BgMedium
        {
            get { return CurrentTheme == "Dark" ? Dark_BgMedium : Light_BgMedium; }
        }
        public static Color BgLight
        {
            get { return CurrentTheme == "Dark" ? Dark_BgLight : Light_BgLight; }
        }
        public static Color TextPrimary
        {
            get { return CurrentTheme == "Dark" ? Dark_TextPrimary : Light_TextPrimary; }
        }
        public static Color TextSecondary
        {
            get { return CurrentTheme == "Dark" ? Dark_TextSecondary : Light_TextSecondary; }
        }
        public static Color TextMuted
        {
            get { return CurrentTheme == "Dark" ? Dark_TextMuted : Light_TextMuted; }
        }
        // Neon Accents (ثابتة في البالتين)
        public static Color AccentGold = Color.FromArgb(255, 215, 0);    // Progress/Sunrise
        public static Color AccentBlue = Color.FromArgb(0, 190, 255);    // General/Clock
        public static Color AccentGreen = Color.FromArgb(0, 255, 150);   // Success/Active
        public static Color AccentRed = Color.FromArgb(255, 80, 80);     // Warning/Stop

        // المدن المدعومة
        public static Dictionary<string, string> SupportedCities = new Dictionary<string, string> {
            { "Riyadh", "Riyadh,Saudi Arabia" },
            { "Jeddah", "Jeddah,Saudi Arabia" },
            { "Makkah", "Makkah,Saudi Arabia" },
            { "Madinah", "Madinah,Saudi Arabia" },
            { "Dammam", "Dammam,Saudi Arabia" },
            { "Dubai", "Dubai,UAE" },
            { "Cairo", "Cairo,Egypt" }
        };

        // طرق الحساب
        public static Dictionary<int, string> CalculationMethods = new Dictionary<int, string> {
            { 4, "Umm Al-Qura (Saudi)" },
            { 5, "Egyptian General Authority" },
            { 3, "Muslim World League" },
            { 2, "Islamic Society of North America" },
            { 1, "University of Islamic Sciences, Karachi" }
        };

        public static Font FontLight(float size) { try { return new Font("Segoe UI Light", size); } catch { return new Font("Arial", size); } }
        public static Font FontRegular(float size) { try { return new Font("Segoe UI", size); } catch { return new Font("Arial", size); } }
        public static Font FontBold(float size) { try { return new Font("Segoe UI Semibold", size); } catch { return new Font("Arial", size, FontStyle.Bold); } }
    }

    // ==========================================
    // 📝 LOGGER SYSTEM
    // ==========================================
    public static class Logger
    {
        private static string logFile = Path.Combine(Application.StartupPath, "debug_log.txt");

        public static void Log(string message)
        {
            try
            {
                // استخدام التاريخ الميلادي بشكل واضح
                string logEntry = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, message);
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch { /* If logging fails, we can't really do much */ }
        }
    }

    // ==========================================
    // 🚀 MAIN APPLICATION FORM
    // ==========================================
    public class MainForm : Form
    {
        #region Windows API
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        const int KEYEVENTF_EXTENDEDKEY = 0x01;
        const int KEYEVENTF_KEYUP = 0x02;
        
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        #endregion

        // Controls
        private Timer mainTimer;
        private NotifyIcon trayIcon;
        
        // Custom Controls
        private CircularTimeWidget timeWidget;
        private FlowLayoutPanel prayersPanel;
        private Panel controlBar;
        
        // Data
        private DateTime nextPrayerTime;
        private string nextPrayerName = "";
        private TimeSpan timeRemaining;
        private Dictionary<string, PrayerSettings> prayerSettings = new Dictionary<string, PrayerSettings>();
        private List<CustomPrayer> customPrayers = new List<CustomPrayer>(); // User defined
        private List<ModernPrayerCard> cards = new List<ModernPrayerCard>();
        private string settingsFilePath = Path.Combine(Application.StartupPath, "prayer_settings.json");
        private string customPrayersPath = Path.Combine(Application.StartupPath, "custom_prayers.json");
        private string appSettingsPath = Path.Combine(Application.StartupPath, "app_settings.json");
        private bool isMiniMode = false;
        private dynamic lastApiData = null; // Store API data to refresh list
        
        // App Settings (City & Method)
        private AppSettings appSettings = new AppSettings();
        
        // Sound Alert tracking
        private string lastAlertedPrayer = "";

        public MainForm()
        {
            InitializeForm(); 
            InitializeLayout();
            
            // Logic
            LoadAppSettings(); // Load city & method
            LoadSettings();
            LoadCustomPrayers(); // Load user prayers
            LoadPrayerTimesAsync();
            
            // Timers
            mainTimer = new Timer { Interval = 1000 };
            mainTimer.Tick += OnTick;
            mainTimer.Start();

            // Tray
            InitializeTray();
        }

        private void InitializeForm()
        {
            this.Text = "Prayer & Media Controller Ultimate";
            this.Size = new Size(400, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = UITheme.BgDark;
            this.DoubleBuffered = true;
            this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));
            this.Icon = SystemIcons.Application;

            // Drag Interface
            this.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            // Keyboard Shortcuts
            this.KeyPreview = true;
            this.KeyDown += (s, e) => {
                switch (e.KeyCode) {
                    case Keys.M:
                        if (e.Control)
                            ToggleMiniMode();
                        break;
                    case Keys.S:
                        if (e.Control)
                            OpenAppSettings();
                        break;
                    case Keys.Add:
                    case Keys.Oemplus:
                        AddNewCustomPrayer();
                        break;
                    case Keys.Space:
                        if (e.Control) {
                            StopMedia();
                            Logger.Log("USER ACTION: Keyboard shortcut - Media Toggle");
                            ShowToast("Media", "Play/Pause via keyboard");
                        }
                        break;
                    case Keys.Escape:
                        if (isMiniMode) ToggleMiniMode();
                        else this.Hide();
                        break;
                    case Keys.F1:
                        ShowHelp();
                        break;
                    case Keys.F5:
                        LoadPrayerTimesAsync();
                        ShowToast("Refresh", "Reloading prayer times...");
                        break;
                    case Keys.R:
                        if (e.Control) {
                            // Reset all settings
                            if (MessageBox.Show("Reset all settings to defaults?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                                prayerSettings.Clear();
                                customPrayers.Clear();
                                appSettings = new AppSettings();
                                SaveSettings();
                                SaveCustomPrayers();
                                SaveAppSettings();
                                LoadPrayerTimesAsync();
                                ShowToast("Reset", "All settings reset to defaults");
                            }
                        }
                        break;
                }
            };
        }

        private void ShowHelp()
        {
            string help = @"⌨️ Keyboard Shortcuts

Ctrl + M → Mini Mode
Ctrl + S → Settings
Ctrl + Space → Media Toggle
Ctrl + R → Reset Settings
+ → Add Custom Prayer
F1 → Show Help
F5 → Refresh Prayer Times
Escape → Hide Window

🖱️ Double-click widget to toggle Mini Mode
Right-click for quick menu";
            MessageBox.Show(help, "Help - Prayer Controller Pro", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowQuickMenu(Point location)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("⏯ Media Toggle", null, (s, e) => {
                StopMedia();
                ShowToast("Media", "Play/Pause");
            });
            menu.Items.Add("-");
            menu.Items.Add("🏙 Change City", null, (s, e) => OpenAppSettings());
            menu.Items.Add("➕ Add Reminder", null, (s, e) => AddNewCustomPrayer());
            menu.Items.Add("-");
            menu.Items.Add("📐 Mini Mode", null, (s, e) => ToggleMiniMode());
            menu.Items.Add("🔄 Refresh", null, (s, e) => {
                LoadPrayerTimesAsync();
                ShowToast("Refresh", "Reloading...");
            });
            menu.Items.Add("-");
            menu.Items.Add("❓ Help (F1)", null, (s, e) => ShowHelp());
            
            menu.Show(location);
        }

        private void InitializeLayout()
        {
            // 1. Control Bar (Top)
            controlBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
            
            // Media Test Button (Left)
            var btnMedia = new Label { 
                Text = "⏯", ForeColor = UITheme.AccentBlue, Font = UITheme.FontBold(14), 
                Location = new Point(15, 10), AutoSize = true, Cursor = Cursors.Hand 
            };
            btnMedia.Click += (s, e) => {
                StopMedia();
                Logger.Log("USER ACTION: Media Toggle Button Clicked (Manual Test)");
                ShowToast("Media Control", "Play/Pause command sent.");
            };

            // Add Custom Prayer Button (Right of Media)
            var btnAdd = new Label { 
                Text = "+", ForeColor = UITheme.AccentGreen, Font = UITheme.FontBold(16), 
                Location = new Point(45, 8), AutoSize = true, Cursor = Cursors.Hand 
            };
            btnAdd.Click += (s, e) => AddNewCustomPrayer();

            // Settings Button (Gear icon)
            var btnSettings = new Label { 
                Text = "⚙", ForeColor = UITheme.AccentGold, Font = UITheme.FontBold(14), 
                Location = new Point(70, 10), AutoSize = true, Cursor = Cursors.Hand 
            };
            btnSettings.Click += (s, e) => OpenAppSettings();

            var btnClose = new Label { 
                Text = "●", ForeColor = UITheme.AccentRed, Font = UITheme.FontBold(14), 
                Location = new Point(Width - 30, 10), AutoSize = true, Cursor = Cursors.Hand 
            };
            btnClose.Click += (s, e) => this.Hide();
            
            var btnMini = new Label { 
                Text = "●", ForeColor = UITheme.AccentGold, Font = UITheme.FontBold(14), 
                Location = new Point(Width - 55, 10), AutoSize = true, Cursor = Cursors.Hand 
            };
            btnMini.Click += (s, e) => ToggleMiniMode();

            var title = new Label { 
                Text = "PRAYER CONTROLLER", ForeColor = UITheme.TextMuted, Font = UITheme.FontBold(9),
                Location = new Point(75, 15), AutoSize = true
            };

            controlBar.Controls.Add(btnMedia);
            controlBar.Controls.Add(btnAdd);
            controlBar.Controls.Add(btnSettings);
            controlBar.Controls.Add(btnClose);
            controlBar.Controls.Add(btnMini);
            controlBar.Controls.Add(title);
            
            // Drag Support for Top Bar
            controlBar.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
            
            this.Controls.Add(controlBar);

            // 2. Circular Time Widget (Hero Section)
            timeWidget = new CircularTimeWidget {
                Location = new Point(50, 60),
                Size = new Size(300, 300)
            };
            
            // Double-click to toggle Mini Mode
            timeWidget.DoubleClick += (s, e) => ToggleMiniMode();
            
            // Right-click for quick menu
            timeWidget.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Right) {
                    ShowQuickMenu(Cursor.Position);
                }
            };
            
            // Drag Support for Widget (for Mini Mode)
            timeWidget.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left) {
                    ReleaseCapture();
                    SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
            
            this.Controls.Add(timeWidget);

            // 3. Prayer List Container
            prayersPanel = new FlowLayoutPanel {
                Location = new Point(20, 380),
                Size = new Size(360, 400),
                Padding = new Padding(20, 0, 0, 0), // Centering: 20(Loc)+20(Pad)=40px left margin for 320px cards
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            this.Controls.Add(prayersPanel);
        }

        private void InitializeTray()
        {
            trayIcon = new NotifyIcon {
                Icon = SystemIcons.Application,
                Text = "Prayer Controller - Loading...",
                Visible = true
            };
            
            var menu = new ContextMenuStrip();
            menu.Items.Add("Open", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            menu.Items.Add("Settings", null, (s, e) => OpenAppSettings());
            menu.Items.Add("-");
            menu.Items.Add("Exit", null, (s, e) => Application.Exit());
            trayIcon.ContextMenuStrip = menu;
            trayIcon.DoubleClick += (s, e) => this.Show();
        }

        private void UpdateTrayTooltip()
        {
            try {
                string tooltip = string.Format("Next: {0} at {1:h:mm tt}\n{2}", 
                    nextPrayerName, 
                    nextPrayerTime,
                    appSettings.City);
                // NotifyIcon.Text limited to 63 chars
                if (tooltip.Length > 63) tooltip = tooltip.Substring(0, 63);
                trayIcon.Text = tooltip;
            } catch {}
        }

        // ==========================================
        // ⚙️ LOGIC & EVENTS
        // ==========================================
        private void OnTick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            
            // Update Time Widget
            timeWidget.CurrentTime = now;
            
            // Calculation for Countdown
            if (nextPrayerTime > now)
            {
                timeRemaining = nextPrayerTime - now;
                timeWidget.RemainingTime = timeRemaining;
                timeWidget.PrayerName = nextPrayerName;
                
                // Progress Calculation
                double totalSeconds;
                if (timeWidget.IsResuming) {
                     // Progress based on window (Stop -> Resume)
                     // Estimate window: 20 mins default? We don't have start time here easily, 
                     // but we can assume a standard max window or just use linear remaining.
                     totalSeconds = 20 * 60; // Approximate
                } else {
                     totalSeconds = 120 * 60; // 2 hours for standard countdown
                }
                
                double currentSeconds = timeRemaining.TotalSeconds;
                float progress = 1.0f - (float)(currentSeconds / totalSeconds);
                if (progress < 0) progress = 0;
                timeWidget.Progress = progress;
            }
            else
            {
                // Time passed! Find next!
                timeWidget.PrayerName = "Updating...";
                RefreshNextPrayer();
            }
            
            timeWidget.Invalidate(); // Redraw widget

            // Check Auto-Stop Logic
            CheckAutoStop(now);
        }

        private async void LoadPrayerTimesAsync()
        {
            try {
                Logger.Log("INFO: Fetching prayer times from API...");
                // Enable TLS 1.2
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                
                // استخدام المدينة والطريقة من الإعدادات
                string cityParam = appSettings.City ?? "Riyadh";
                string methodParam = appSettings.Method.ToString();
                string url = string.Format("https://api.aladhan.com/v1/timingsByCity?city={0}&country=Saudi%20Arabia&method={1}", 
                    cityParam, methodParam);
                    
                using (WebClient client = new WebClient()) {
                    client.Encoding = System.Text.Encoding.UTF8;
                    string json = await client.DownloadStringTaskAsync(url);
                    Logger.Log("INFO: Prayer times downloaded successfully for " + cityParam);
                    var serializer = new JavaScriptSerializer();
                    lastApiData = serializer.Deserialize<dynamic>(json); // Store for refreshing
                    
                    if (lastApiData["code"] == 200) {
                        ProcessPrayerData(lastApiData["data"]);
                    }
                }
            } catch (Exception ex) { 
                Logger.Log("ERROR: Failed to load prayer times. " + ex.Message);
            }
        }

        private void ProcessPrayerData(dynamic data)
        {
            var timings = data["timings"];
            var date = data["date"]["gregorian"]["date"];
            timeWidget.DateText = date;

            // Clean & Build List
            prayersPanel.Controls.Clear();
            cards.Clear();
            Logger.Log("INFO: Rebuilding schedule with Custom Prayers...");

            var prayerList = new List<dynamic>();

            // 1. Add Standard Prayers
            var standardPrayers = new[] {
                new { Key = "Fajr", Name = "FAJR", Icon = "🌅", Offset = 25 },
                new { Key = "Dhuhr", Name = "DHUHR", Icon = "☀️", Offset = 20 },
                new { Key = "Asr", Name = "ASR", Icon = "🌤️", Offset = 20 },
                new { Key = "Maghrib", Name = "MAGHRIB", Icon = "🌇", Offset = 10 },
                new { Key = "Isha", Name = "ISHA", Icon = "🌙", Offset = 20 }
            };

            foreach(var p in standardPrayers) {
                string timeStr = timings[p.Key];
                DateTime t = DateTime.Parse(timeStr);
                prayerList.Add(new { Name = p.Name, Time = t, Iqama = t.AddMinutes(p.Offset), Key = p.Key, Icon = p.Icon, IsCustom = false });
            }

            // 2. Add Custom Prayers
            foreach(var cp in customPrayers) {
                DateTime t = DateTime.Parse(cp.TimeStr);
                // If time already passed for today, assume tomorrow? No, let standard logic handle "next".
                // Actually, Parse returns today's date with valid time.
                prayerList.Add(new { Name = cp.Name, Time = t, Iqama = t, Key = cp.Name, Icon = "📌", IsCustom = true });
            }

            // 3. Sort by Time
            prayerList.Sort((a, b) => a.Time.CompareTo(b.Time));

            // 4. Build Cards
            DateTime now = DateTime.Now;

            foreach (var p in prayerList)
            {
                var card = new ModernPrayerCard {
                    PrayerName = p.Name,
                    PrayerTime = p.Time,
                    IqamaTime = p.Iqama,
                    PrayerKey = p.Key,
                    IconCode = p.Icon,
                    Settings = prayerSettings.ContainsKey(p.Key) ? prayerSettings[p.Key] : new PrayerSettings()
                };
                
                card.Click += (s, e) => OpenSettings(card);
                if (p.IsCustom) {
                     if (!prayerSettings.ContainsKey(p.Key)) prayerSettings[p.Key] = card.Settings;
                }

                prayersPanel.Controls.Add(card);
                cards.Add(card);
            }
            
            RefreshNextPrayer();
        }

        private void CheckAutoStop(DateTime now)
        {
            // Check Sound Alert
            if (appSettings.SoundAlert && !string.IsNullOrEmpty(nextPrayerName)) {
                var alertTime = nextPrayerTime.AddMinutes(-appSettings.AlertMinutesBefore);
                if (now >= alertTime && now < nextPrayerTime && lastAlertedPrayer != nextPrayerName) {
                    SystemSounds.Exclamation.Play(); // Play system alert sound
                    ShowToast("Prayer Alert", string.Format("{0} in {1} minutes!", nextPrayerName, appSettings.AlertMinutesBefore));
                    Logger.Log(string.Format("ALERT: Sound alert for {0}", nextPrayerName));
                    lastAlertedPrayer = nextPrayerName;
                }
            }

            foreach (var card in cards)
            {
                if (!card.Settings.Enabled) continue;
                
                // Logic
                var stopTime = card.PrayerTime.AddMinutes(-card.Settings.MinutesBefore);
                var resumeTime = card.PrayerTime.AddMinutes(card.Settings.MinutesAfter);
                
                if (now >= stopTime && now < resumeTime)
                {
                    if (!card.Settings.HasStoppedToday) {
                        StopMedia();
                        card.Settings.HasStoppedToday = true;
                        card.Settings.WasStoppedByApp = true; // Mark as stopped by us
                        SaveSettings(); // Persist state!
                        Logger.Log(string.Format("ACTION: Media PAUSED for {0} (Before: {1}m)", card.PrayerName, card.Settings.MinutesBefore));
                        ShowToast("Media Paused", string.Format("Auto-stopped for {0}", card.PrayerName));
                        
                        // Force refresh to update UI state (Active)
                        RefreshNextPrayer();
                    }
                }
                else if (now >= resumeTime && card.Settings.WasStoppedByApp)
                {
                    // Auto-Resume Logic
                    StopMedia(); // Toggle Play/Pause again to Resume
                    card.Settings.WasStoppedByApp = false; // Reset flag
                    SaveSettings(); // Persist state!
                    Logger.Log(string.Format("ACTION: Media RESUMED after {0} (After: {1}m)", card.PrayerName, card.Settings.MinutesAfter));
                    ShowToast("Media Resumed", string.Format("Prayer time ended for {0}", card.PrayerName));
                    
                    // Force refresh to switch to next prayer immediately after resume
                    RefreshNextPrayer();
                }
            }
            
            // Reset at midnight
            if (now.Hour == 0 && now.Minute == 0 && now.Second == 0)
            {
                foreach(var c in cards) {
                    c.Settings.HasStoppedToday = false;
                    c.Settings.WasStoppedByApp = false;
                }
                lastAlertedPrayer = ""; // Reset sound alert
                // Refresh list for new day
                if (lastApiData != null) ProcessPrayerData(lastApiData["data"]);
            }
        }

        private void RefreshNextPrayer()
        {
            DateTime now = DateTime.Now;
            DateTime? foundNext = null;
            string foundName = "";
            ModernPrayerCard nextCard = null;
            bool isActiveWindow = false;

            // 1. Check if we are currently IN a prayer window (Stop -> Resume)
            foreach(var c in cards) {
                if (!c.Settings.Enabled) continue;
                
                var stopTime = c.PrayerTime.AddMinutes(-c.Settings.MinutesBefore);
                var resumeTime = c.PrayerTime.AddMinutes(c.Settings.MinutesAfter);
                
                if (now >= stopTime && now < resumeTime) {
                     // We are in an active window!
                     foundNext = resumeTime;
                     foundName = c.PrayerName;
                     nextCard = c;
                     isActiveWindow = true;
                     c.IsActive = true;
                     break;
                } else {
                    c.IsActive = false;
                }
            }

            // 2. If not active, find next scheduled prayer
            if (!isActiveWindow) {
                // Reset visual state
                foreach(var c in cards) { c.IsNext = false; c.IsActive = false; }

                foreach(var c in cards) {
                    if (c.PrayerTime > now) {
                        foundNext = c.PrayerTime;
                        foundName = c.PrayerName;
                        nextCard = c;
                        break; 
                    }
                }
                
                // If tomorrow
                if (foundNext == null && cards.Count > 0) {
                    var first = cards[0];
                    foundNext = first.PrayerTime.AddDays(1);
                    foundName = first.PrayerName;
                    nextCard = first;
                }
            }

            if (foundNext != null) {
                nextPrayerTime = foundNext.Value;
                nextPrayerName = foundName;
                if (nextCard != null) {
                     nextCard.IsNext = true;
                     // Mark if this is a "Resume" countdown (IsActive) or "Next Prayer" countdown
                     timeWidget.IsResuming = isActiveWindow; 
                }
                
                // Only log if changed to avoid spam? Or just log.
                // Logger.Log(...); 
            }
            
            // Update Tray Tooltip
            UpdateTrayTooltip();
            
            // Redraw
            prayersPanel.Invalidate(true); 
            timeWidget.Invalidate();
        }

        private void AddNewCustomPrayer()
        {
            var form = new AddPrayerForm();
            if (form.ShowDialog() == DialogResult.OK) {
                var newP = form.Result;
                customPrayers.Add(newP);
                SaveCustomPrayers();
                
                // إضافة إعدادات افتراضية للصلاة المخصصة الجديدة
                if (!prayerSettings.ContainsKey(newP.Name)) {
                    prayerSettings[newP.Name] = new PrayerSettings { 
                        Enabled = false, 
                        MinutesBefore = 5, 
                        MinutesAfter = 15 
                    };
                    SaveSettings();
                    Logger.Log("INFO: Added default settings for custom prayer: " + newP.Name);
                }
                
                Logger.Log("USER ACTION: Added custom prayer: " + newP.Name);
                
                // Refresh list if we have data
                if (lastApiData != null) ProcessPrayerData(lastApiData["data"]);
            }
        }

        private void SaveCustomPrayers()
        {
            try {
                var serializer = new JavaScriptSerializer();
                File.WriteAllText(customPrayersPath, serializer.Serialize(customPrayers));
            } catch {}
        }

        private void LoadCustomPrayers()
        {
            try {
                if (File.Exists(customPrayersPath)) {
                    var serializer = new JavaScriptSerializer();
                    customPrayers = serializer.Deserialize<List<CustomPrayer>>(File.ReadAllText(customPrayersPath));
                }
            } catch {}
        }
        
        private void StopMedia()
        {
            keybd_event((byte)VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event((byte)VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        private void OpenSettings(ModernPrayerCard card)
        {
            var form = new PrayerSettingsForm(card.PrayerName, card.Settings);
            if (form.ShowDialog() == DialogResult.OK)
            {
                card.Settings = form.ResultSettings;
                prayerSettings[card.PrayerKey] = card.Settings;
                SaveSettings();
                card.Invalidate(); // Redraw status dot
            }
        }

        private void ToggleMiniMode()
        {
            isMiniMode = !isMiniMode;
            if (isMiniMode)
            {
                this.Size = new Size(320, 320);
                prayersPanel.Visible = false;
                controlBar.Visible = false;
                timeWidget.Location = new Point(10, 10);
                this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 320, 320)); // Circular Form
            }
            else
            {
                this.Size = new Size(400, 800);
                prayersPanel.Visible = true;
                controlBar.Visible = true;
                timeWidget.Location = new Point(50, 60);
                this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 30, 30));
            }
        }

        private void SaveSettings()
        {
            try {
                var serializer = new JavaScriptSerializer();
                File.WriteAllText(settingsFilePath, serializer.Serialize(prayerSettings));
            } catch {}
        }

        private void LoadSettings()
        {
            try {
                if (File.Exists(settingsFilePath)) {
                    var serializer = new JavaScriptSerializer();
                    prayerSettings = serializer.Deserialize<Dictionary<string, PrayerSettings>>(File.ReadAllText(settingsFilePath));
                }
            } catch {}
        }

        private void LoadAppSettings()
        {
            try {
                if (File.Exists(appSettingsPath)) {
                    var serializer = new JavaScriptSerializer();
                    appSettings = serializer.Deserialize<AppSettings>(File.ReadAllText(appSettingsPath));
                    // تطبيق الـ Theme المحفوظ
                    UITheme.CurrentTheme = appSettings.Theme ?? "Dark";
                }
            } catch { 
                appSettings = new AppSettings(); // Use defaults
                UITheme.CurrentTheme = "Dark";
            }
        }

        private void SaveAppSettings()
        {
            try {
                var serializer = new JavaScriptSerializer();
                File.WriteAllText(appSettingsPath, serializer.Serialize(appSettings));
                
                // Update Auto-start
                SetAutoStart(appSettings.AutoStart);
            } catch {}
        }

        private void SetAutoStart(bool enable)
        {
            try {
                string appName = "PrayerControllerPro";
                string exePath = Application.ExecutablePath;
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true)) {
                    if (enable) {
                        key.SetValue(appName, "\"" + exePath + "\"");
                        Logger.Log("INFO: Auto-start enabled");
                    } else {
                        key.DeleteValue(appName, false);
                        Logger.Log("INFO: Auto-start disabled");
                    }
                }
            } catch (Exception ex) {
                Logger.Log("WARN: Failed to set auto-start: " + ex.Message);
            }
        }

        private void OpenAppSettings()
        {
            var form = new AppSettingsForm(appSettings);
            if (form.ShowDialog() == DialogResult.OK)
            {
                appSettings = form.ResultSettings;
                SaveAppSettings();
                Logger.Log("USER ACTION: Settings changed - City: " + appSettings.City + ", Method: " + appSettings.Method);
                LoadPrayerTimesAsync(); // Reload with new settings
                ShowToast("Settings Saved", "City: " + appSettings.City);
            }
        }
        
        private void ShowToast(string title, string msg)
        {
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipText = msg;
            trayIcon.ShowBalloonTip(3000);
        }

        [STAThread]
        static void Main()
        {
            try {
                Logger.Log("=========================================");
                Logger.Log("INFO: Application Starting...");
                
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                    string error = e.ExceptionObject.ToString();
                    Logger.Log("CRITICAL: Unhandled Exception: " + error);
                    MessageBox.Show(error, "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                Application.ThreadException += (s, e) => {
                    string error = e.Exception.ToString();
                    Logger.Log("ERROR: UI Thread Exception: " + error);
                    MessageBox.Show(error, "UI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                
                Logger.Log("INFO: Application Closed normally.");
            } catch (Exception ex) {
                Logger.Log("FATAL: Startup Failed: " + ex.ToString());
                MessageBox.Show(ex.ToString(), "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ==========================================
    // 🎨 CUSTOM CONTROLS
    // ==========================================
    
    // 1. The GLOWING CIRCULAR WIDGET
    public class CircularTimeWidget : Control
    {
        public DateTime CurrentTime { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public string PrayerName { get; set; }
        public string DateText { get; set; }
        public float Progress { get; set; }
        public bool IsResuming { get; set; } // New property

        public CircularTimeWidget()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            CurrentTime = DateTime.Now;
            PrayerName = "--";
            DateText = "--";
            Progress = 0.5f;
            IsResuming = false;

            DoubleBuffered = true;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = Width;
            int h = Height;
            int margin = 20;
            Rectangle rect = new Rectangle(margin, margin, w - margin*2, h - margin*2);

            // 1. Background Circle (Dark Track)
            using (var pen = new Pen(Color.FromArgb(30, UITheme.AccentBlue), 8))
            {
                g.DrawEllipse(pen, rect);
            }

            // 2. Progress Arc (Neon Glow)
            // -90 is Top. Sweep 360 * Progress
            using (var pen = new Pen(UITheme.AccentBlue, 8))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawArc(pen, rect, -90, 360 * Progress);
            }

            // 3. Digital Clock (Center)
            string timeStr = CurrentTime.ToString("hh:mm");
            string ttStr = CurrentTime.ToString("tt", System.Globalization.CultureInfo.InvariantCulture);
            
            // Draw Main Time
            using (var font = UITheme.FontLight(48)) 
            using (var brush = new SolidBrush(UITheme.TextPrimary))
            {
                var size = g.MeasureString(timeStr, font);
                g.DrawString(timeStr, font, brush, (w - size.Width)/2, (h - size.Height)/2 - 20);
            }
            
            // Draw AM/PM
            using (var font = UITheme.FontBold(12)) 
            using (var brush = new SolidBrush(UITheme.AccentBlue))
            {
                var size = g.MeasureString(ttStr, font);
                g.DrawString(ttStr, font, brush, (w - size.Width)/2, (h/2) + 15);
            }

            // Draw Seconds (Small)
            // using (var font = UITheme.FontRegular(14))
            // using (var brush = new SolidBrush(UITheme.AccentGold))
            // {
            //    g.DrawString(secStr, font, brush, w/2 + 70, h/2 + 10);
            // }

            // 4. Info Text (Bottom)
            string prevText = IsResuming ? "RESUMING IN..." : string.Format("NEXT: {0}", PrayerName); 
            string remText = string.Format("-{0:D2}:{1:D2}:{2:D2}", RemainingTime.Hours, RemainingTime.Minutes, RemainingTime.Seconds);

            using (var font = UITheme.FontBold(10)) 
            using (var brush = new SolidBrush(IsResuming ? UITheme.AccentRed : UITheme.TextSecondary))
            {
                var size = g.MeasureString(prevText, font);
                g.DrawString(prevText, font, brush, (w - size.Width)/2, h/2 + 40);
            }
            
            using (var font = UITheme.FontRegular(12)) 
            using (var brush = new SolidBrush(UITheme.AccentGold))
            {
                var size = g.MeasureString(remText, font);
                g.DrawString(remText, font, brush, (w - size.Width)/2, h/2 + 60);
            }
        }
    }

    // 2. MODERN CARD
    public class ModernPrayerCard : Control
    {
        public string PrayerName { get; set; }
        public DateTime PrayerTime { get; set; }
        public DateTime IqamaTime { get; set; }
        public string PrayerKey { get; set; }
        public string IconCode { get; set; }
        public bool IsNext { get; set; }
        public bool IsActive { get; set; } // New: Currently in pause window
        public PrayerSettings Settings { get; set; }

        public ModernPrayerCard()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.Size = new Size(320, 70);
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;
            this.Margin = new Padding(0, 0, 0, 10);
            this.BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background
            var bgRect = new Rectangle(0, 0, Width - 1, Height - 1);
            var bgColor = IsNext ? Color.FromArgb(40, UITheme.AccentBlue) : UITheme.BgMedium;
            if (Settings.Enabled) {
                int newBlue = bgColor.B + 15;
                if (newBlue > 255) newBlue = 255;
                bgColor = Color.FromArgb(bgColor.R, bgColor.G, newBlue);
            }

            using (var brush = new SolidBrush(bgColor))
            {
                FillRoundedRectangle(g, brush, bgRect, 12);
            }

            // Border (if next)
            if (IsNext)
            {
                using (var pen = new Pen(UITheme.AccentBlue, 2))
                {
                    DrawRoundedRectangle(g, pen, bgRect, 12);
                }
            }

            // Icon
            using (var font = new Font("Segoe UI Emoji", 18))
            using (var brush = new SolidBrush(Color.White))
            {
                g.DrawString(IconCode, font, brush, 15, 15);
            }

            // Name
            using (var font = UITheme.FontBold(12)) 
            using (var brush = new SolidBrush(UITheme.TextPrimary))
            {
                g.DrawString(PrayerName, font, brush, 60, 10);
            }

            // Adhan Time (Top Right)
            string adhanLabel = "Time:    " + PrayerTime.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
            if (!PrayerKey.StartsWith("Custom") && PrayerKey != "Midnight" && PrayerKey != "Lastthird") 
                 adhanLabel = "Adhan:  " + PrayerTime.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            using (var font = UITheme.FontLight(10)) 
            using (var brush = new SolidBrush(IsNext ? UITheme.AccentGold : UITheme.TextSecondary))
            {
                var size = g.MeasureString(adhanLabel, font);
                g.DrawString(adhanLabel, font, brush, Width - size.Width - 15, 12);
            }

            // Iqama Time - Hide for custom/special
            if (PrayerKey != "Midnight" && PrayerKey != "Lastthird" && !Settings.Enabled) // Logic: For custom, we might hide Iqama or show same time
            {
               // Simplified: Only standard prayers show Iqama
               bool isStandard = (PrayerKey == "Fajr" || PrayerKey == "Dhuhr" || PrayerKey == "Asr" || PrayerKey == "Maghrib" || PrayerKey == "Isha");
               
                if (isStandard) {
                     string iqamaLabel = "Iqama:  " + IqamaTime.ToString("hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
                     using (var font = UITheme.FontBold(10)) 
                     using (var brush = new SolidBrush(UITheme.TextPrimary))
                     {
                         var size = g.MeasureString(iqamaLabel, font);
                         g.DrawString(iqamaLabel, font, brush, Width - size.Width - 15, 35);
                     }
                }
             }

             // Status Badge (if enabled or active)
             if (IsActive)
             {
                 using (var font = UITheme.FontBold(9)) 
                 using (var brush = new SolidBrush(UITheme.AccentRed))
                 {
                     g.DrawString("⛔ ACTIVE", font, brush, 60, 35);
                 }
             }
             else if (Settings.Enabled)
             {
                 using (var font = UITheme.FontRegular(8)) 
                 using (var brush = new SolidBrush(UITheme.AccentGreen))
                 {
                     g.DrawString("AUTO-STOP ON", font, brush, 60, 35);
                 }
             }
         }
        
        // Helper for Rounded Rects
        private void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
        {
            var path = GetRoundedRect(rect, radius);
            g.FillPath(brush, path);
        }
        private void DrawRoundedRectangle(Graphics g, Pen pen, Rectangle rect, int radius)
        {
            var path = GetRoundedRect(rect, radius);
            g.DrawPath(pen, path);
        }
        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0) { path.AddRectangle(bounds); return path; }

            // Top left
            path.AddArc(arc, 180, 90);
            // Top right
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            // Bottom right
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            // Bottom left
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }

    // 3. SETTINGS FORM
    public class PrayerSettingsForm : Form
    {
        public PrayerSettings ResultSettings { get; private set; }
        private CheckBox chkEnable;
        private NumericUpDown numBefore;
        private NumericUpDown numAfter;

        public PrayerSettingsForm(string title, PrayerSettings settings)
        {
            ResultSettings = new PrayerSettings { 
                Enabled = settings.Enabled, 
                MinutesBefore = settings.MinutesBefore, 
                MinutesAfter = settings.MinutesAfter 
            };

            // Start Form UI
            this.Size = new Size(300, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = UITheme.BgMedium;
            this.Region = Region.FromHrgn(MainForm.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            // Controls
            var lblTitle = new Label { 
                Text = title + " SETTINGS", 
                Location = new Point(0, 20), Size = new Size(300, 30), 
                TextAlign = ContentAlignment.MiddleCenter, 
                Font = UITheme.FontBold(12), ForeColor = UITheme.AccentBlue 
            };
            this.Controls.Add(lblTitle);

            // Enable
            chkEnable = new CheckBox {
                Text = "Enable Auto-Stop",
                Location = new Point(40, 70), AutoSize = true,
                Font = UITheme.FontRegular(10), ForeColor = UITheme.TextPrimary,
                Checked = ResultSettings.Enabled
            };
            this.Controls.Add(chkEnable);

            // Inputs
            this.Controls.Add(CreateLabel("Stop Before (mins):", 40, 110));
            numBefore = CreateNum(ResultSettings.MinutesBefore, 40, 135);
            this.Controls.Add(numBefore);

            this.Controls.Add(CreateLabel("Resume After (mins):", 40, 180));
            numAfter = CreateNum(ResultSettings.MinutesAfter, 40, 205);
            this.Controls.Add(numAfter);

            // Buttons
            var btnSave = new Button {
                Text = "SAVE", DialogResult = DialogResult.OK,
                Location = new Point(160, 280), Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat, BackColor = UITheme.AccentBlue, ForeColor = Color.White
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => {
                ResultSettings.Enabled = chkEnable.Checked;
                ResultSettings.MinutesBefore = (int)numBefore.Value;
                ResultSettings.MinutesAfter = (int)numAfter.Value;
            };
            this.Controls.Add(btnSave);

            var btnCancel = new Button {
                Text = "CANCEL", DialogResult = DialogResult.Cancel,
                Location = new Point(40, 280), Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat, BackColor = UITheme.BgLight, ForeColor = UITheme.TextSecondary
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);
        }

        private Label CreateLabel(string text, int x, int y) {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = UITheme.TextSecondary, Font = UITheme.FontRegular(9) };
        }
        private NumericUpDown CreateNum(int val, int x, int y) {
            return new NumericUpDown { Value = (decimal)val, Location = new Point(x, y), Width = 120, BackColor = UITheme.BgDark, ForeColor = Color.White };
        }
    }

    // DATA MODELS
    public class PrayerSettings
    {
        public bool Enabled { get; set; }
        public int MinutesBefore { get; set; }
        public int MinutesAfter { get; set; }
        public bool HasStoppedToday { get; set; }
        public bool WasStoppedByApp { get; set; }
        
        public PrayerSettings() { MinutesBefore = 5; MinutesAfter = 15; }
    }

    public class AppSettings
    {
        public string City { get; set; }
        public int Method { get; set; }
        public bool AutoStart { get; set; } // بدء مع Windows
        public bool SoundAlert { get; set; } // تنبيه صوتي
        public int AlertMinutesBefore { get; set; } // دقائق قبل الأذان
        public string Theme { get; set; } // "Dark" or "Light"
        
        public AppSettings() { 
            City = "Riyadh"; 
            Method = 4; // Umm Al-Qura
            AutoStart = false;
            SoundAlert = false;
            AlertMinutesBefore = 5;
            Theme = "Dark"; // الافتراضي
        }
    }

    public class CustomPrayer
    {
        public string Name { get; set; }
        public string TimeStr { get; set; } // Format HH:mm
    }

    public class AddPrayerForm : Form
    {
        public CustomPrayer Result { get; private set; }
        private TextBox txtName;
        private DateTimePicker timePicker;

        public AddPrayerForm()
        {
            this.Size = new Size(300, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = UITheme.BgMedium;
            this.Region = Region.FromHrgn(MainForm.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            // Title
            var lbl = new Label { Text = "ADD CUSTOM REMINDER", Location = new Point(0, 20), Size = new Size(300, 30), TextAlign = ContentAlignment.MiddleCenter, Font = UITheme.FontBold(12), ForeColor = UITheme.AccentBlue };
            this.Controls.Add(lbl);

            // Name
            this.Controls.Add(new Label { Text = "Name:", Location = new Point(40, 70), AutoSize = true, ForeColor = UITheme.TextSecondary });
            txtName = new TextBox { Location = new Point(40, 95), Width = 220, BackColor = UITheme.BgDark, ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            this.Controls.Add(txtName);

            // Time
            this.Controls.Add(new Label { Text = "Time:", Location = new Point(40, 130), AutoSize = true, ForeColor = UITheme.TextSecondary });
            timePicker = new DateTimePicker { Location = new Point(40, 155), Width = 220, Format = DateTimePickerFormat.Time, ShowUpDown = true };
            this.Controls.Add(timePicker);

            // Buttons
            var btnSave = new Button { Text = "ADD", Location = new Point(160, 200), Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = UITheme.AccentGreen, ForeColor = Color.White, DialogResult = DialogResult.OK };
            btnSave.Click += (s, e) => {
                // Validation: لا تقبل أسماء فارغة
                if (string.IsNullOrWhiteSpace(txtName.Text)) {
                    MessageBox.Show("الرجاء إدخال اسم للتذكير!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
                
                // Validation: فحص صحة الوقت
                string timeStr = timePicker.Value.ToString("HH:mm");
                DateTime dummyDate;
                if (!DateTime.TryParseExact(timeStr, "HH:mm", null, System.Globalization.DateTimeStyles.None, out dummyDate)) {
                    MessageBox.Show("الرجاء إدخال وقت صحيح (HH:mm)!", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }
                
                Result = new CustomPrayer { Name = txtName.Text.Trim(), TimeStr = timeStr };
            };
            this.Controls.Add(btnSave);

            var btnCancel = new Button { Text = "CANCEL", Location = new Point(40, 200), Size = new Size(100, 35), FlatStyle = FlatStyle.Flat, BackColor = UITheme.BgLight, ForeColor = UITheme.TextSecondary, DialogResult = DialogResult.Cancel };
            this.Controls.Add(btnCancel);
        }
    }

    // 4. APP SETTINGS FORM (City & Method)
    public class AppSettingsForm : Form
    {
        public AppSettings ResultSettings { get; private set; }
        private ComboBox cmbCity;
        private ComboBox cmbMethod;
        private CheckBox chkAutoStart;

        public AppSettingsForm(AppSettings current)
        {
            ResultSettings = new AppSettings { 
                City = current.City, 
                Method = current.Method,
                AutoStart = current.AutoStart
            };

            this.Size = new Size(320, 470);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = UITheme.BgMedium;
            this.Region = Region.FromHrgn(MainForm.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            // Title
            var lblTitle = new Label { 
                Text = "⚙ APP SETTINGS", 
                Location = new Point(0, 20), Size = new Size(320, 30), 
                TextAlign = ContentAlignment.MiddleCenter, 
                Font = UITheme.FontBold(12), ForeColor = UITheme.AccentBlue 
            };
            this.Controls.Add(lblTitle);

            // City Dropdown
            this.Controls.Add(new Label { 
                Text = "City:", Location = new Point(40, 70), AutoSize = true, 
                ForeColor = UITheme.TextSecondary, Font = UITheme.FontRegular(10) 
            });
            cmbCity = new ComboBox { 
                Location = new Point(40, 95), Width = 240, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UITheme.BgDark, ForeColor = Color.White,
                Font = UITheme.FontRegular(10)
            };
            foreach (var city in UITheme.SupportedCities.Keys) {
                cmbCity.Items.Add(city);
            }
            cmbCity.SelectedItem = current.City;
            this.Controls.Add(cmbCity);

            // Method Dropdown
            this.Controls.Add(new Label { 
                Text = "Calculation Method:", Location = new Point(40, 135), AutoSize = true, 
                ForeColor = UITheme.TextSecondary, Font = UITheme.FontRegular(10) 
            });
            cmbMethod = new ComboBox { 
                Location = new Point(40, 160), Width = 240, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UITheme.BgDark, ForeColor = Color.White,
                Font = UITheme.FontRegular(10)
            };
            foreach (var method in UITheme.CalculationMethods) {
                cmbMethod.Items.Add(method.Key + " - " + method.Value);
            }
            // Select current method
            for (int i = 0; i < cmbMethod.Items.Count; i++) {
                if (cmbMethod.Items[i].ToString().StartsWith(current.Method + " -")) {
                    cmbMethod.SelectedIndex = i;
                    break;
                }
            }
            this.Controls.Add(cmbMethod);

            // Auto-start Checkbox
            chkAutoStart = new CheckBox {
                Text = "Start with Windows",
                Location = new Point(40, 210), AutoSize = true,
                Font = UITheme.FontRegular(10), ForeColor = UITheme.TextPrimary,
                BackColor = Color.Transparent,
                Checked = current.AutoStart
            };
            this.Controls.Add(chkAutoStart);

            // Sound Alert Checkbox
            var chkSound = new CheckBox {
                Text = "Sound Alert before Adhan",
                Location = new Point(40, 245), AutoSize = true,
                Font = UITheme.FontRegular(10), ForeColor = UITheme.TextPrimary,
                BackColor = Color.Transparent,
                Checked = current.SoundAlert
            };
            this.Controls.Add(chkSound);

            // Alert Minutes
            this.Controls.Add(new Label { 
                Text = "Minutes before:", Location = new Point(40, 280), AutoSize = true, 
                ForeColor = UITheme.TextSecondary, Font = UITheme.FontRegular(9) 
            });
            var numAlert = new NumericUpDown { 
                Location = new Point(140, 277), Width = 60, 
                Minimum = 1, Maximum = 30, Value = current.AlertMinutesBefore,
                BackColor = UITheme.BgDark, ForeColor = Color.White
            };
            this.Controls.Add(numAlert);

            // Theme Selector
            this.Controls.Add(new Label { 
                Text = "Theme:", Location = new Point(40, 310), AutoSize = true, 
                ForeColor = UITheme.TextSecondary, Font = UITheme.FontRegular(9) 
            });
            var cmbTheme = new ComboBox { 
                Location = new Point(100, 307), Width = 120, 
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UITheme.BgDark, ForeColor = Color.White,
                Font = UITheme.FontRegular(10)
            };
            cmbTheme.Items.Add("Dark");
            cmbTheme.Items.Add("Light");
            cmbTheme.SelectedItem = current.Theme ?? "Dark";
            this.Controls.Add(cmbTheme);

            // Buttons
            var btnSave = new Button { 
                Text = "SAVE", Location = new Point(170, 380), Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat, BackColor = UITheme.AccentBlue, ForeColor = Color.White
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => {
                ResultSettings.City = cmbCity.SelectedItem != null ? cmbCity.SelectedItem.ToString() : "Riyadh";
                // Parse method number from "4 - Umm Al-Qura"
                string methodStr = cmbMethod.SelectedItem != null ? cmbMethod.SelectedItem.ToString() : "4";
                ResultSettings.Method = int.Parse(methodStr.Split('-')[0].Trim());
                ResultSettings.AutoStart = chkAutoStart.Checked;
                ResultSettings.SoundAlert = chkSound.Checked;
                ResultSettings.AlertMinutesBefore = (int)numAlert.Value;
                ResultSettings.Theme = cmbTheme.SelectedItem != null ? cmbTheme.SelectedItem.ToString() : "Dark";
                UITheme.CurrentTheme = ResultSettings.Theme; // تطبيق الـ Theme فوراً
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(btnSave);

            var btnCancel = new Button { 
                Text = "CANCEL", Location = new Point(50, 380), Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat, BackColor = UITheme.BgLight, ForeColor = UITheme.TextSecondary,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);
        }
    }
}
