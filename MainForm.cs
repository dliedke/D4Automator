using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace D4Automator
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int HOTKEY_ID = 9000;
        private const uint VK_F5 = 0x74;

        private bool isRunning = false;
        private Random random = new Random();
        private Settings settings;
        private string settingsPath;
        private CancellationTokenSource cancellationTokenSource;

        public MainForm()
        {
            InitializeComponent();
            InitializeSettings();
            InitializeKeyMappings();
            RegisterGlobalHotkey();
            ApplyDarkMode();
            AttachEventHandlers();
            SetFormTitle();
            UpdateLabels();
        }

        private void InitializeKeyMappings()
        {
            keyMappings = new Dictionary<string, Action>();
            UpdateKeyMappings();
        }

        private void AttachEventHandlers()
        {
            nudSkill1.ValueChanged += nudSkill1_ValueChanged;
            nudSkill2.ValueChanged += nudSkill2_ValueChanged;
            nudSkill3.ValueChanged += nudSkill3_ValueChanged;
            nudSkill4.ValueChanged += nudSkill4_ValueChanged;
            nudPrimaryAttack.ValueChanged += nudRightClick_ValueChanged;
            nudSecondaryAttack.ValueChanged += nudLeftClick_ValueChanged;
            nudPotion.ValueChanged += nudPotion_ValueChanged;
            nudDodge.ValueChanged += nudDodge_ValueChanged;
        }
        private void SetFormTitle()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = $"D4 Automator v{version.Major}.{version.Minor}.{version.Build}";
        }

        private void InitializeSettings()
        {
            string executablePath = Assembly.GetExecutingAssembly().Location;
            string executableDirectory = Path.GetDirectoryName(executablePath);
            settingsPath = Path.Combine(executableDirectory, "D4AutomatorSettings.xml");

            if (File.Exists(settingsPath))
            {
                LoadSettings();
            }
            else
            {
                CreateDefaultSettings();
            }

            ApplySettingsToControls();
        }

        private void CreateDefaultSettings()
        {
            settings = new Settings
            {
                Skill1Delay = 3000,
                Skill2Delay = 3000,
                Skill3Delay = 3000,
                Skill4Delay = 3000,
                PrimaryAttackDelay = 400,
                SecondaryAttackDelay = 400,
                PotionDelay = 2000,
                DodgeDelay = 0
            };

            SaveSettings();
        }

        private void LoadSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (FileStream stream = new FileStream(settingsPath, FileMode.Open))
            {
                settings = (Settings)serializer.Deserialize(stream);
            }
            UpdateKeyMappings();
            UpdateLabels(); // Add this line
        }

        private void SaveSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (FileStream stream = new FileStream(settingsPath, FileMode.Create))
            {
                serializer.Serialize(stream, settings);
            }
        }

        private void ApplySettingsToControls()
        {
            nudSkill1.Value = settings.Skill1Delay;
            nudSkill2.Value = settings.Skill2Delay;
            nudSkill3.Value = settings.Skill3Delay;
            nudSkill4.Value = settings.Skill4Delay;
            nudPrimaryAttack.Value = settings.PrimaryAttackDelay;
            nudSecondaryAttack.Value = settings.SecondaryAttackDelay;
            nudPotion.Value = settings.PotionDelay;
            nudDodge.Value = settings.DodgeDelay;
        }

        private void UpdateLabels()
        {
            lblSkill1.Text = $"Skill 1 ({GetDisplayTextForKey(settings.Skill1Action)}) Delay (ms):";
            lblSkill2.Text = $"Skill 2 ({GetDisplayTextForKey(settings.Skill2Action)}) Delay (ms):";
            lblSkill3.Text = $"Skill 3 ({GetDisplayTextForKey(settings.Skill3Action)}) Delay (ms):";
            lblSkill4.Text = $"Skill 4 ({GetDisplayTextForKey(settings.Skill4Action)}) Delay (ms):";
            lblPrimaryAttack.Text = $"Primary Attack ({GetDisplayTextForKey(settings.PrimaryAttackAction)}) Delay (ms):";
            lblSecondaryAttack.Text = $"Secondary Attack ({GetDisplayTextForKey(settings.SecondaryAttackAction)}) Delay (ms):";
            lblPotion.Text = $"Potion ({GetDisplayTextForKey(settings.PotionAction)}) Delay (ms):";
            lblDodge.Text = $"Dodge ({GetDisplayTextForKey(settings.DodgeAction)}) Delay (ms):";

            lblInstructions.Text = $"Press {GetDisplayTextForKey(settings.ToggleAutomationAction)} to start/stop automation.\r\nSet delay to 0 to disable an action.";
        }

        private string GetDisplayTextForKey(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                return string.Empty;
            }

            switch (keyString)
            {
                case "LeftClick":
                    return "Left Click";
                case "RightClick":
                    return "Right Click";
                case "MiddleClick":
                    return "Middle Click";
            }

            if (keyString.StartsWith("D") && keyString.Length == 2 && char.IsDigit(keyString[1]))
            {
                return keyString.Substring(1); // Remove the 'D' prefix for number keys
            }

            if (keyString.StartsWith("NumPad") && keyString.Length > 6)
            {
                return $"NumPad {keyString.Substring(6)}";
            }

            return keyString;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HOTKEY_ID)
            {
                ToggleAutomation();
            }
            base.WndProc(ref m);
        }

        private void ToggleAutomation()
        {
            if (!isRunning)
            {
                StartAutomation();
            }
            else
            {
                StopAutomation();
            }
        }

        private async void StartAutomation()
        {
            isRunning = true;
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                await RunAutomation(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Automation was cancelled, do nothing
            }
            finally
            {
                isRunning = false;
            }
        }

        private void StopAutomation()
        {
            if (isRunning)
            {
                cancellationTokenSource?.Cancel();
                isRunning = false;
            }
        }

        private async Task RunAutomation(CancellationToken cancellationToken)
        {
            var tasks = new List<Task>
            {
                RunActionLoop(settings.Skill1Delay, keyMappings["Skill1"], cancellationToken),
                RunActionLoop(settings.Skill2Delay, keyMappings["Skill2"], cancellationToken),
                RunActionLoop(settings.Skill3Delay, keyMappings["Skill3"], cancellationToken),
                RunActionLoop(settings.Skill4Delay, keyMappings["Skill4"], cancellationToken),
                RunActionLoop(settings.PrimaryAttackDelay, keyMappings["PrimaryAttack"], cancellationToken),
                RunActionLoop(settings.SecondaryAttackDelay, keyMappings["SecondaryAttack"], cancellationToken),
                RunActionLoop(settings.PotionDelay, keyMappings["Potion"], cancellationToken),
                RunActionLoop(settings.DodgeDelay, keyMappings["Dodge"], cancellationToken)
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                if (GetForegroundWindow() == this.Handle)
                {
                    StopAutomation();
                    break;
                }
                await Task.Delay(50, cancellationToken); // Small delay to prevent excessive CPU usage
            }

            await Task.WhenAll(tasks);
        }

        private async Task RunActionLoop(int delay, Action action, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (delay > 0)
                {
                    action();
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    await Task.Delay(50, cancellationToken); // Small delay for actions with 0 delay
                }
            }
        }

        private void SimulateKeyPress(Keys key)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SimulateKeyPress(key)));
                return;
            }

            byte vk = (byte)key;
            keybd_event(vk, 0, 0, UIntPtr.Zero);
            keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private void SimulateMouseClick(MouseButtons button)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SimulateMouseClick(button)));
                return;
            }

            int downFlag;
            int upFlag;

            switch (button)
            {
                case MouseButtons.Left:
                    downFlag = MOUSEEVENTF_LEFTDOWN;
                    upFlag = MOUSEEVENTF_LEFTUP;
                    break;
                case MouseButtons.Right:
                    downFlag = MOUSEEVENTF_RIGHTDOWN;
                    upFlag = MOUSEEVENTF_RIGHTUP;
                    break;
                case MouseButtons.Middle:
                    downFlag = MOUSEEVENTF_MIDDLEDOWN;
                    upFlag = MOUSEEVENTF_MIDDLEUP;
                    break;
                default:
                    // Unsupported mouse button
                    return;
            }

            mouse_event(downFlag, 0, 0, 0, 0);
            mouse_event(upFlag, 0, 0, 0, 0);
        }

        private void RegisterGlobalHotkey()
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            if (Enum.TryParse(settings.ToggleAutomationAction, out Keys key))
            {
                RegisterHotKey(this.Handle, HOTKEY_ID, 0, (uint)key);
            }
        }


        private void UnregisterGlobalHotkey()
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
        }

        private void ApplyDarkMode()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            foreach (Control control in this.Controls)
            {
                if (control is NumericUpDown || control is Button)
                {
                    control.BackColor = Color.FromArgb(45, 45, 45);
                    control.ForeColor = Color.White;
                }
            }
        }

        private void nudSkill1_ValueChanged(object sender, EventArgs e)
        {
            settings.Skill1Delay = (int)nudSkill1.Value;
            SaveSettings();
        }

        private void nudSkill2_ValueChanged(object sender, EventArgs e)
        {
            settings.Skill2Delay = (int)nudSkill2.Value;
            SaveSettings();
        }

        private void nudSkill3_ValueChanged(object sender, EventArgs e)
        {
            settings.Skill3Delay = (int)nudSkill3.Value;
            SaveSettings();
        }

        private void nudSkill4_ValueChanged(object sender, EventArgs e)
        {
            settings.Skill4Delay = (int)nudSkill4.Value;
            SaveSettings();
        }

        private void nudRightClick_ValueChanged(object sender, EventArgs e)
        {
            settings.PrimaryAttackDelay = (int)nudPrimaryAttack.Value;
            SaveSettings();
        }

        private void nudLeftClick_ValueChanged(object sender, EventArgs e)
        {
            settings.SecondaryAttackDelay = (int)nudSecondaryAttack.Value;
            SaveSettings();
        }

        private void nudPotion_ValueChanged(object sender, EventArgs e)
        {
            settings.PotionDelay = (int)nudPotion.Value;
            SaveSettings();
        }

        private void nudDodge_ValueChanged(object sender, EventArgs e)
        {
            settings.DodgeDelay = (int)nudDodge.Value;
            SaveSettings();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
            UnregisterGlobalHotkey();
        }

        private void btnKeyConfig_Click(object sender, EventArgs e)
        {
            using (var keyConfigForm = new KeyConfigForm(settings))
            {
                if (keyConfigForm.ShowDialog() == DialogResult.OK)
                {
                    SaveSettings();
                    UpdateKeyMappings();
                    UpdateLabels();
                }
            }
        }

        private void UpdateKeyMappings()
        {
            keyMappings = new Dictionary<string, Action>
            {
                {"Skill1", () => SimulateAction(settings.Skill1Action)},
                {"Skill2", () => SimulateAction(settings.Skill2Action)},
                {"Skill3", () => SimulateAction(settings.Skill3Action)},
                {"Skill4", () => SimulateAction(settings.Skill4Action)},
                {"PrimaryAttack", () => SimulateAction(settings.PrimaryAttackAction)},
                {"SecondaryAttack", () => SimulateAction(settings.SecondaryAttackAction)},
                {"Potion", () => SimulateAction(settings.PotionAction)},
                {"Dodge", () => SimulateAction(settings.DodgeAction)}
            };

            RegisterGlobalHotkey();
        }

        // Add this field to the MainForm class
        private Dictionary<string, Action> keyMappings;

        private void SimulateAction(string action)
        {
            switch (action)
            {
                case "LeftClick":
                    SimulateMouseClick(MouseButtons.Left);
                    break;
                case "RightClick":
                    SimulateMouseClick(MouseButtons.Right);
                    break;
                case "MiddleClick":
                    SimulateMouseClick(MouseButtons.Middle);
                    break;
                default:
                    if (Enum.TryParse(action, out Keys key))
                    {
                        SimulateKeyPress(key);
                    }
                    break;
            }
        }
    }

    public class Settings
    {
        public int Skill1Delay { get; set; }
        public int Skill2Delay { get; set; }
        public int Skill3Delay { get; set; }
        public int Skill4Delay { get; set; }
        public int PrimaryAttackDelay { get; set; }
        public int SecondaryAttackDelay { get; set; }
        public int PotionDelay { get; set; }
        public int DodgeDelay { get; set; }

        public string Skill1Action { get; set; }
        public string Skill2Action { get; set; }
        public string Skill3Action { get; set; }
        public string Skill4Action { get; set; }
        public string PrimaryAttackAction { get; set; }
        public string SecondaryAttackAction { get; set; }
        public string PotionAction { get; set; }
        public string DodgeAction { get; set; }
        public string ToggleAutomationAction { get; set; }

        public Settings()
        {
            // Set default values
            Skill1Action = "D1";
            Skill2Action = "D2";
            Skill3Action = "D3";
            Skill4Action = "D4";
            PrimaryAttackAction = "RightClick";
            SecondaryAttackAction = "LeftClick";
            PotionAction = "Q";
            DodgeAction = "Space";
            ToggleAutomationAction = "F5";

            // Set default delays
            Skill1Delay = 1000;
            Skill2Delay = 1000;
            Skill3Delay = 1000;
            Skill4Delay = 1000;
            PrimaryAttackDelay = 100;
            SecondaryAttackDelay = 100;
            PotionDelay = 5000;
            DodgeDelay = 1000;
        }
    }
}