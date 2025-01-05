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

using SharpDX.XInput;

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
        private const int MOUSEEVENTF_XDOWN = 0x0080;
        private const int MOUSEEVENTF_XUP = 0x0100;
        private const int XBUTTON1 = 0x0001;
        private const int XBUTTON2 = 0x0002;
        private const int HOTKEY_ID = 9000;

        private bool isRunning = false;
        private Settings settings;
        private string settingsPath;
        private CancellationTokenSource cancellationTokenSource;

        private Controller controller;
        private System.Windows.Forms.Timer controllerCheckTimer;
        private const float DEAD_ZONE = 0.1f; // Adjust as needed
        private bool wasAutomationRunning = false;

        private bool isMouseMoveEnabled = false;
        private bool isMoving = false;
        private System.Windows.Forms.Timer moveTimer;
        private const int PAUSE_DURATION = 5000; // 5 second pause at each corner
        private const int RECT_WIDTH = 700; // Width of rectangle
        private const int RECT_HEIGHT = 400; // Height of rectangle
        private const int MOVE_STEPS = 30; // Steps for smooth movement
        private Point initialPosition;
        private Point targetPosition;
        private Point startPosition;
        private int currentStep = 0;
        private bool isMovingToTarget = false;
        private int currentCorner = 0; // 0: top-right, 1: bottom-right, 2: bottom-left, 3: top-left

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
            InitializeControllerDetection();
        }

        private void InitializeControllerDetection()
        {
            // Assumes the first controller
            controller = new Controller(UserIndex.One);

            // Check if controller is connected
            if (controller.IsConnected)
            {
                controllerCheckTimer = new System.Windows.Forms.Timer();
                controllerCheckTimer.Interval = 50; // Check every 50ms
                controllerCheckTimer.Tick += ControllerCheckTimer_Tick;
                controllerCheckTimer.Start();
            }
        }

        private void ControllerCheckTimer_Tick(object sender, EventArgs e)
        {
            if (controller.IsConnected)
            {
                var state = controller.GetState();

                bool controllerMoved = IsControllerMoved(state);

                if (controllerMoved)
                {
                    if (isRunning)
                    {
                        StopAutomation();
                        wasAutomationRunning = true;
                    }
                }
                else if (wasAutomationRunning)
                {
                    StartAutomation();
                    wasAutomationRunning = false;
                }
            }
        }

        private bool IsControllerMoved(State state)
        {
            // Check analog sticks
            bool leftStickMoved = Math.Abs(state.Gamepad.LeftThumbX) > short.MaxValue * DEAD_ZONE ||
                                  Math.Abs(state.Gamepad.LeftThumbY) > short.MaxValue * DEAD_ZONE;
            bool rightStickMoved = Math.Abs(state.Gamepad.RightThumbX) > short.MaxValue * DEAD_ZONE ||
                                   Math.Abs(state.Gamepad.RightThumbY) > short.MaxValue * DEAD_ZONE;

            // Check digital pad
            bool dPadPressed = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp) ||
                               state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown) ||
                               state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft) ||
                               state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight);

            return leftStickMoved || rightStickMoved || dPadPressed;
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
                case "MouseBack":
                    return "Mouse Back";
                case "MouseForward":
                    return "Mouse Forward";
                default:
                    return keyString;
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
            if (m.Msg == 0x0312) // WM_HOTKEY
            {
                // Original automation hotkey
                if (m.WParam.ToInt32() == HOTKEY_ID) 
                {
                    ToggleAutomation();

                    if (isMouseMoveEnabled)
                    {
                        ToggleMovement();
                    }
                }
            }

            base.WndProc(ref m);
        }


        private void chkMouseMove_CheckedChanged(object sender, EventArgs e)
        {
            isMouseMoveEnabled = chkMouseMove.Checked;

            if (chkMouseMove.Checked)
            {
                InitializeMouseMovement();
            }
            else
            {
                moveTimer.Stop();
            }
        }

        private void InitializeMouseMovement()
        {
            moveTimer = new System.Windows.Forms.Timer();
            moveTimer.Interval = 50;
            moveTimer.Tick += MoveTimer_Tick;
        }

        private void ToggleMovement()
        {
            isMoving = !isMoving;
            if (isMoving)
            {
                initialPosition = System.Windows.Forms.Cursor.Position;
                currentCorner = 0;
                StartNextMove();
                moveTimer.Start();
            }
            else
            {
                moveTimer.Stop();
            }
        }

        private Point GetCornerPosition(int corner)
        {
            switch (corner)
            {
                case 0: // Top-right
                    return new Point(initialPosition.X + RECT_WIDTH / 2, initialPosition.Y - RECT_HEIGHT / 2);
                case 1: // Bottom-right
                    return new Point(initialPosition.X + RECT_WIDTH / 2, initialPosition.Y + RECT_HEIGHT / 2);
                case 2: // Bottom-left
                    return new Point(initialPosition.X - RECT_WIDTH / 2, initialPosition.Y + RECT_HEIGHT / 2);
                case 3: // Top-left
                    return new Point(initialPosition.X - RECT_WIDTH / 2, initialPosition.Y - RECT_HEIGHT / 2);
                default:
                    return initialPosition;
            }
        }

        private void StartNextMove()
        {
            startPosition = System.Windows.Forms.Cursor.Position;
            targetPosition = GetCornerPosition(currentCorner);
            currentStep = 0;
            isMovingToTarget = true;
        }

        private async Task PauseBetweenMoves()
        {
            isMovingToTarget = false;
            await Task.Delay(PAUSE_DURATION);
            if (isMoving)
            {
                // Move to next corner
                currentCorner = (currentCorner + 1) % 4;
                StartNextMove();
            }
        }

        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            if (!isMoving) return;

            if (!isMovingToTarget)
            {
                return; // We're in a pause
            }

            // Calculate progress (0.0 to 1.0)
            float progress = (float)currentStep / MOVE_STEPS;

            // Use easing function to make movement more natural
            progress = EaseInOutQuad(progress);

            // Calculate new position
            int newX = (int)(startPosition.X + (targetPosition.X - startPosition.X) * progress);
            int newY = (int)(startPosition.Y + (targetPosition.Y - startPosition.Y) * progress);

            // Move cursor
            System.Windows.Forms.Cursor.Position = new Point(newX, newY);

            // Increment step
            currentStep++;

            // If we've reached the target, start a pause
            if (currentStep >= MOVE_STEPS)
            {
                _ = PauseBetweenMoves();
            }
        }

        // Easing function to make movement more natural
        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : 1 - (float)Math.Pow(-2 * t + 2, 2) / 2;
        }

        private void ToggleAutomation()
        {
            if (!isRunning)
            {
                StartAutomation();
                wasAutomationRunning = false; // Reset this flag when manually starting
            }
            else
            {
                StopAutomation();
                wasAutomationRunning = false; // Ensure this is false when manually stopping
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
            int mouseData = 0;  // Required for X buttons

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
                case MouseButtons.XButton1:
                    downFlag = MOUSEEVENTF_XDOWN;
                    upFlag = MOUSEEVENTF_XUP;
                    mouseData = XBUTTON1;
                    break;
                case MouseButtons.XButton2:
                    downFlag = MOUSEEVENTF_XDOWN;
                    upFlag = MOUSEEVENTF_XUP;
                    mouseData = XBUTTON2;
                    break;
                default:
                    // Unsupported mouse button
                    return;
            }

            mouse_event(downFlag, 0, 0, mouseData, 0);
            mouse_event(upFlag, 0, 0, mouseData, 0);
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
            UnregisterHotKey(this.Handle, 9001); // Unregister F4 hotkey

            controllerCheckTimer?.Stop();
            controllerCheckTimer?.Dispose();
            moveTimer?.Dispose();
            controller = null;
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
                case "MouseBack":
                    SimulateMouseClick(MouseButtons.XButton1);
                    break;
                case "MouseForward":
                    SimulateMouseClick(MouseButtons.XButton2);
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