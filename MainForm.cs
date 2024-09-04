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
            RegisterGlobalHotkey();
            ApplyDarkMode();
            AttachEventHandlers();
            SetFormTitle();
        }

        private void AttachEventHandlers()
        {
            nudSkill1.ValueChanged += nudSkill1_ValueChanged;
            nudSkill2.ValueChanged += nudSkill2_ValueChanged;
            nudSkill3.ValueChanged += nudSkill3_ValueChanged;
            nudSkill4.ValueChanged += nudSkill4_ValueChanged;
            nudRightClick.ValueChanged += nudRightClick_ValueChanged;
            nudLeftClick.ValueChanged += nudLeftClick_ValueChanged;
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
                RightClickDelay = 400,
                LeftClickDelay = 400,
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
            nudRightClick.Value = settings.RightClickDelay;
            nudLeftClick.Value = settings.LeftClickDelay;
            nudPotion.Value = settings.PotionDelay;
            nudDodge.Value = settings.DodgeDelay;
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
                RunActionLoop(settings.Skill1Delay, () => SimulateKeyPress("1"), cancellationToken),
                RunActionLoop(settings.Skill2Delay, () => SimulateKeyPress("2"), cancellationToken),
                RunActionLoop(settings.Skill3Delay, () => SimulateKeyPress("3"), cancellationToken),
                RunActionLoop(settings.Skill4Delay, () => SimulateKeyPress("4"), cancellationToken),
                RunActionLoop(settings.RightClickDelay, SimulateRightClick, cancellationToken),
                RunActionLoop(settings.LeftClickDelay, SimulateLeftClick, cancellationToken),
                RunActionLoop(settings.PotionDelay, () => SimulateKeyPress("q"), cancellationToken),
                RunActionLoop(settings.DodgeDelay, () => SimulateKeyPress(" "), cancellationToken)
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
        private void SimulateKeyPress(string key)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SimulateKeyPress(key)));
                return;
            }

            byte vk = 0;
            if (key.Length == 1)
            {
                if (char.IsLetterOrDigit(key[0]))
                {
                    vk = (byte)key.ToUpper()[0];
                }
                else if (key == " ")
                {
                    vk = 0x20; // Space
                }
            }

            if (vk != 0)
            {
                keybd_event(vk, 0, 0, UIntPtr.Zero);
                keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }

        private void SimulateRightClick()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(SimulateRightClick));
                return;
            }

            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        private void SimulateLeftClick()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(SimulateLeftClick));
                return;
            }

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private void RegisterGlobalHotkey()
        {
            RegisterHotKey(this.Handle, HOTKEY_ID, 0, VK_F5);
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
                if (control is NumericUpDown)
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
            settings.RightClickDelay = (int)nudRightClick.Value;
            SaveSettings();
        }

        private void nudLeftClick_ValueChanged(object sender, EventArgs e)
        {
            settings.LeftClickDelay = (int)nudLeftClick.Value;
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
    }

    public class Settings
    {
        public int Skill1Delay { get; set; }
        public int Skill2Delay { get; set; }
        public int Skill3Delay { get; set; }
        public int Skill4Delay { get; set; }
        public int RightClickDelay { get; set; }
        public int LeftClickDelay { get; set; }
        public int PotionDelay { get; set; }
        public int DodgeDelay { get; set; }
    }
}