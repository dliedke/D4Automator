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
using System.Text;

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
        private const int MOUSEMOVE_HOTKEY_ID = 9001;

        private bool isRunning = false;
        private Settings settings;
        private string settingsPath;
        private CancellationTokenSource cancellationTokenSource;
        private HashSet<Keys> heldKeys = new HashSet<Keys>();
        private HashSet<MouseButtons> heldMouseButtons = new HashSet<MouseButtons>();

        private Controller controller;
        private System.Windows.Forms.Timer controllerCheckTimer;
        private const float DEAD_ZONE = 0.1f; // Adjust as needed
        private bool wasAutomationRunning = false;

        private bool isMoving = false;
        private System.Windows.Forms.Timer moveTimer;
        private const int PAUSE_DURATION = 4000; // Pause between moves
        private const int BORDER_PADDING = 50; // Distance from screen borders
        private const int MOVE_STEPS = 50; // Steps for smooth movement
        private Point targetPosition;
        private Point startPosition;
        private int currentStep = 0;
        private bool isMovingToTarget = false;
        private int currentDirection = 0; // 0=up, 1=down, 2=up-left, 3=down-right
        
        private bool hasUnsavedChanges = false;
        private string currentFileName = string.Empty;
        private string recentFilesPath;

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
            
            // Update recent files menu after form is loaded
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateRecentFilesMenu();
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
            nudMove.ValueChanged += nudMove_ValueChanged;
            nudPotion.ValueChanged += nudPotion_ValueChanged;
            nudDodge.ValueChanged += nudDodge_ValueChanged;

            // Add KeyDown event handlers to detect immediate changes
            nudSkill1.KeyDown += NumericUpDown_KeyDown;
            nudSkill2.KeyDown += NumericUpDown_KeyDown;
            nudSkill3.KeyDown += NumericUpDown_KeyDown;
            nudSkill4.KeyDown += NumericUpDown_KeyDown;
            nudPrimaryAttack.KeyDown += NumericUpDown_KeyDown;
            nudSecondaryAttack.KeyDown += NumericUpDown_KeyDown;
            nudMove.KeyDown += NumericUpDown_KeyDown;
            nudPotion.KeyDown += NumericUpDown_KeyDown;
            nudDodge.KeyDown += NumericUpDown_KeyDown;
        }

        private void NumericUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            // Mark as changed when user types in numeric controls
            // This will be more responsive than waiting for ValueChanged
            if (char.IsDigit((char)e.KeyCode) || e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                // Use a timer to mark as changed after a short delay
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 100; // Very short delay
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    timer.Dispose();
                    CheckForPendingChanges();
                };
                timer.Start();
            }
        }
        private void SetFormTitle()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string baseTitle = $"D4 Automator v{version.Major}.{version.Minor}.{version.Build}";
            
            if (!string.IsNullOrEmpty(currentFileName))
            {
                baseTitle += $" - {Path.GetFileName(currentFileName)}";
            }
            
            if (hasUnsavedChanges)
            {
                baseTitle += "*";
            }
            
            this.Text = baseTitle;
        }

        private void MarkAsChanged()
        {
            if (!hasUnsavedChanges)
            {
                hasUnsavedChanges = true;
                SetFormTitle();
            }
        }

        private void MarkAsSaved()
        {
            if (hasUnsavedChanges)
            {
                hasUnsavedChanges = false;
                SetFormTitle();
            }
        }

        private void InitializeSettings()
        {
            string executablePath = Assembly.GetExecutingAssembly().Location;
            string executableDirectory = Path.GetDirectoryName(executablePath);
            settingsPath = Path.Combine(executableDirectory, "D4AutomatorSettings.xml");
            recentFilesPath = Path.Combine(executableDirectory, "RecentFiles.txt");

            if (File.Exists(settingsPath))
            {
                LoadSettings();
            }
            else
            {
                CreateDefaultSettings();
            }

            ApplySettingsToControls();
            AutoLoadLastConfiguration();
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
                MoveDelay = 400,
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
            UpdateLabels();
        }

        private void SaveSettings()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (FileStream stream = new FileStream(settingsPath, FileMode.Create))
            {
                serializer.Serialize(stream, settings);
            }
        }

        private void AutoLoadLastConfiguration()
        {
            if (!string.IsNullOrEmpty(settings.LastLoadedConfigFile) && File.Exists(settings.LastLoadedConfigFile))
            {
                try
                {
                    string jsonContent = File.ReadAllText(settings.LastLoadedConfigFile);
                    var loadedSettings = DeserializeSettingsFromJson(jsonContent);
                    
                    // Preserve the LastLoadedConfigFile value
                    loadedSettings.LastLoadedConfigFile = settings.LastLoadedConfigFile;
                    
                    settings = loadedSettings;
                    currentFileName = settings.LastLoadedConfigFile;
                    ApplySettingsToControls();
                    UpdateKeyMappings();
                    UpdateLabels();
                    MarkAsSaved(); // Mark as saved since we just loaded
                }
                catch
                {
                    // If loading fails, just continue with current settings
                    // Don't show error message on startup
                }
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
            nudMove.Value = settings.MoveDelay;
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
            lblMove.Text = $"Move ({GetDisplayTextForKey(settings.MoveAction)}) Delay (ms):";
            lblPotion.Text = $"Potion ({GetDisplayTextForKey(settings.PotionAction)}) Delay (ms):";
            lblDodge.Text = $"Dodge ({GetDisplayTextForKey(settings.DodgeAction)}) Delay (ms):";

            lblInstructions.Text = $"Press {GetDisplayTextForKey(settings.ToggleAutomationAction)} to start/stop automation.\r\nPress {GetDisplayTextForKey(settings.ToggleMouseMoveAction)} for automation + mouse move (Infernal Hordes).\r\n\r\nSet delay to 0 to disable an action.\r\nSet delay to 1 to keep button/key pressed.";
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
                // Automation toggle hotkey
                if (m.WParam.ToInt32() == HOTKEY_ID)
                {
                    ToggleAutomation();
                }
                // Mouse move + automation toggle hotkey
                else if (m.WParam.ToInt32() == MOUSEMOVE_HOTKEY_ID)
                {
                    ToggleAutomationWithMouseMove();
                }
            }

            base.WndProc(ref m);
        }


        private void ToggleAutomationWithMouseMove()
        {
            if (!isRunning)
            {
                // Start automation
                StartAutomation();
                wasAutomationRunning = false;

                // Start mouse movement
                InitializeMouseMovement();
                isMoving = true;
                currentDirection = 0; // Start with up
                StartNextMove();
                moveTimer.Start();
            }
            else
            {
                // Stop automation
                StopAutomation();
                wasAutomationRunning = false;

                // Stop mouse movement
                isMoving = false;
                moveTimer?.Stop();
            }
        }

        private void InitializeMouseMovement()
        {
            if (moveTimer == null)
            {
                moveTimer = new System.Windows.Forms.Timer();
                moveTimer.Interval = 50;
                moveTimer.Tick += MoveTimer_Tick;
            }
        }

        private Point GetDirectionalPosition()
        {
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;
            Point newPos;
            
            switch (currentDirection)
            {
                case 0: // Up - center width, top with padding
                    newPos = new Point(
                        screenBounds.Width / 2,
                        screenBounds.Top + BORDER_PADDING
                    );
                    break;
                case 1: // Down - center width, bottom with padding
                    newPos = new Point(
                        screenBounds.Width / 2,
                        screenBounds.Bottom - BORDER_PADDING
                    );
                    break;
                case 2: // Up-Left corner with padding
                    newPos = new Point(
                        screenBounds.Left + BORDER_PADDING,
                        screenBounds.Top + BORDER_PADDING
                    );
                    break;
                case 3: // Down-Right corner with padding
                    newPos = new Point(
                        screenBounds.Right - BORDER_PADDING,
                        screenBounds.Bottom - BORDER_PADDING
                    );
                    break;
                default:
                    newPos = System.Windows.Forms.Cursor.Position;
                    break;
            }
            
            // Move to next direction for next cycle
            currentDirection = (currentDirection + 1) % 4;
            
            return newPos;
        }

        private void StartNextMove()
        {
            startPosition = System.Windows.Forms.Cursor.Position;
            targetPosition = GetDirectionalPosition();
            currentStep = 0;
            isMovingToTarget = true;
        }

        private async Task PauseBetweenMoves()
        {
            isMovingToTarget = false;
            await Task.Delay(PAUSE_DURATION);
            if (isMoving)
            {
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

            // Use smooth easing for directional movement
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
                ReleaseHeldInputs();
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
                RunActionLoop(settings.SecondaryAttackDelay, keyMappings["SecondaryAttack"], cancellationToken),
                RunActionLoop(settings.PotionDelay, keyMappings["Potion"], cancellationToken),
                RunActionLoop(settings.DodgeDelay, keyMappings["Dodge"], cancellationToken)
            };

            // Handle Primary Attack and Move together
            if (settings.PrimaryAttackAction == settings.MoveAction)
            {
                // If both are the same, only run Primary Attack
                tasks.Add(RunActionLoop(settings.PrimaryAttackDelay, keyMappings["PrimaryAttack"], cancellationToken));
            }
            else
            {
                // If different, run both
                tasks.Add(RunActionLoop(settings.PrimaryAttackDelay, keyMappings["PrimaryAttack"], cancellationToken));
                tasks.Add(RunActionLoop(settings.MoveDelay, keyMappings["Move"], cancellationToken));
            }

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
            bool buttonHeld = false;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                if (delay == 1 && !buttonHeld)
                {
                    // Hold button pressed when delay is 1
                    action();
                    buttonHeld = true;
                    await Task.Delay(50, cancellationToken);
                }
                else if (delay > 1)
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

        private void SimulateKeyPress(Keys key, bool holdKey = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SimulateKeyPress(key, holdKey)));
                return;
            }

            byte vk = (byte)key;
            keybd_event(vk, 0, 0, UIntPtr.Zero);
            if (!holdKey)
            {
                keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            else
            {
                heldKeys.Add(key);
            }
        }

        private void SimulateMouseClick(MouseButtons button, bool holdButton = false)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SimulateMouseClick(button, holdButton)));
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
            if (!holdButton)
            {
                mouse_event(upFlag, 0, 0, mouseData, 0);
            }
            else
            {
                heldMouseButtons.Add(button);
            }
        }

        private void ReleaseHeldInputs()
        {
            // Release all held keys
            foreach (var key in heldKeys)
            {
                byte vk = (byte)key;
                keybd_event(vk, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            heldKeys.Clear();

            // Release all held mouse buttons
            foreach (var button in heldMouseButtons)
            {
                int upFlag;
                int mouseData = 0;

                switch (button)
                {
                    case MouseButtons.Left:
                        upFlag = MOUSEEVENTF_LEFTUP;
                        break;
                    case MouseButtons.Right:
                        upFlag = MOUSEEVENTF_RIGHTUP;
                        break;
                    case MouseButtons.Middle:
                        upFlag = MOUSEEVENTF_MIDDLEUP;
                        break;
                    case MouseButtons.XButton1:
                        upFlag = MOUSEEVENTF_XUP;
                        mouseData = XBUTTON1;
                        break;
                    case MouseButtons.XButton2:
                        upFlag = MOUSEEVENTF_XUP;
                        mouseData = XBUTTON2;
                        break;
                    default:
                        continue;
                }

                mouse_event(upFlag, 0, 0, mouseData, 0);
            }
            heldMouseButtons.Clear();
        }

        private void RegisterGlobalHotkey()
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            UnregisterHotKey(this.Handle, MOUSEMOVE_HOTKEY_ID);

            if (Enum.TryParse(settings.ToggleAutomationAction, out Keys automationKey))
            {
                RegisterHotKey(this.Handle, HOTKEY_ID, 0, (uint)automationKey);
            }

            if (Enum.TryParse(settings.ToggleMouseMoveAction, out Keys mouseMoveKey))
            {
                RegisterHotKey(this.Handle, MOUSEMOVE_HOTKEY_ID, 0, (uint)mouseMoveKey);
            }
        }


        private void UnregisterGlobalHotkey()
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            UnregisterHotKey(this.Handle, MOUSEMOVE_HOTKEY_ID);
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
            MarkAsChanged();
            SaveSettings();
        }

        private void nudSkill2_ValueChanged(object sender, EventArgs e)
        {
            settings.Skill2Delay = (int)nudSkill2.Value;
            MarkAsChanged();
            SaveSettings();
        }

        private void nudSkill3_ValueChanged(object sender, EventArgs e)
        {
            settings.Skill3Delay = (int)nudSkill3.Value;
            MarkAsChanged();
            SaveSettings();
        }

        private void nudSkill4_ValueChanged(object sender, EventArgs e)
        {
            settings.Skill4Delay = (int)nudSkill4.Value;
            MarkAsChanged();
            SaveSettings();
        }

        private void nudRightClick_ValueChanged(object sender, EventArgs e)
        {
            settings.PrimaryAttackDelay = (int)nudPrimaryAttack.Value;
            MarkAsChanged();
            SaveSettings();
        }

        private void nudLeftClick_ValueChanged(object sender, EventArgs e)
        {
            settings.SecondaryAttackDelay = (int)nudSecondaryAttack.Value;
            MarkAsChanged();
            SaveSettings();
        }

        private void nudPotion_ValueChanged(object sender, EventArgs e)
        {
            settings.PotionDelay = (int)nudPotion.Value;
            MarkAsChanged();
            SaveSettings();
        }

        private void nudMove_ValueChanged(object sender, EventArgs e)
        {
            settings.MoveDelay = (int)nudMove.Value;
            MarkAsChanged();
            SaveSettings();
        }

        private void nudDodge_ValueChanged(object sender, EventArgs e)
        {
            settings.DodgeDelay = (int)nudDodge.Value;
            MarkAsChanged();
            SaveSettings();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PromptSaveChanges())
            {
                e.Cancel = true; // Cancel the closing if user cancels save operation
                return;
            }

            SaveSettings();
            UnregisterGlobalHotkey();
            ReleaseHeldInputs(); // Release any held inputs before closing

            controllerCheckTimer?.Stop();
            controllerCheckTimer?.Dispose();
            moveTimer?.Dispose();
            controller = null;
        }

        private void btnKeyConfig_Click(object sender, EventArgs e)
        {
            // Unregister hotkeys to prevent them from triggering during configuration
            UnregisterGlobalHotkey();

            using (var keyConfigForm = new KeyConfigForm(settings))
            {
                if (keyConfigForm.ShowDialog() == DialogResult.OK)
                {
                    MarkAsChanged();
                    SaveSettings();
                    UpdateKeyMappings();
                    UpdateLabels();
                }
            }

            // Re-register hotkeys after configuration dialog closes
            RegisterGlobalHotkey();
        }

        private void UpdateKeyMappings()
        {
            keyMappings = new Dictionary<string, Action>
            {
                {"Skill1", () => SimulateAction(settings.Skill1Action, settings.Skill1Delay == 1)},
                {"Skill2", () => SimulateAction(settings.Skill2Action, settings.Skill2Delay == 1)},
                {"Skill3", () => SimulateAction(settings.Skill3Action, settings.Skill3Delay == 1)},
                {"Skill4", () => SimulateAction(settings.Skill4Action, settings.Skill4Delay == 1)},
                {"PrimaryAttack", () => SimulateAction(settings.PrimaryAttackAction, settings.PrimaryAttackDelay == 1)},
                {"SecondaryAttack", () => SimulateAction(settings.SecondaryAttackAction, settings.SecondaryAttackDelay == 1)},
                {"Move", () => SimulateAction(settings.MoveAction, settings.MoveDelay == 1)},
                {"Potion", () => SimulateAction(settings.PotionAction, settings.PotionDelay == 1)},
                {"Dodge", () => SimulateAction(settings.DodgeAction, settings.DodgeDelay == 1)}
            };

            RegisterGlobalHotkey();
        }

        // Add this field to the MainForm class
        private Dictionary<string, Action> keyMappings;

        private void SimulateAction(string action, bool holdAction = false)
        {
            switch (action)
            {
                case "LeftClick":
                    SimulateMouseClick(MouseButtons.Left, holdAction);
                    break;
                case "RightClick":
                    SimulateMouseClick(MouseButtons.Right, holdAction);
                    break;
                case "MiddleClick":
                    SimulateMouseClick(MouseButtons.Middle, holdAction);
                    break;
                case "MouseBack":
                    SimulateMouseClick(MouseButtons.XButton1, holdAction);
                    break;
                case "MouseForward":
                    SimulateMouseClick(MouseButtons.XButton2, holdAction);
                    break;
                default:
                    if (Enum.TryParse(action, out Keys key))
                    {
                        SimulateKeyPress(key, holdAction);
                    }
                    break;
            }
        }

        private string SerializeSettingsToJson(Settings settings)
        {
            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"  \"Skill1Delay\": {settings.Skill1Delay},");
            json.AppendLine($"  \"Skill2Delay\": {settings.Skill2Delay},");
            json.AppendLine($"  \"Skill3Delay\": {settings.Skill3Delay},");
            json.AppendLine($"  \"Skill4Delay\": {settings.Skill4Delay},");
            json.AppendLine($"  \"PrimaryAttackDelay\": {settings.PrimaryAttackDelay},");
            json.AppendLine($"  \"SecondaryAttackDelay\": {settings.SecondaryAttackDelay},");
            json.AppendLine($"  \"MoveDelay\": {settings.MoveDelay},");
            json.AppendLine($"  \"PotionDelay\": {settings.PotionDelay},");
            json.AppendLine($"  \"DodgeDelay\": {settings.DodgeDelay},");
            json.AppendLine($"  \"Skill1Action\": \"{EscapeJsonString(settings.Skill1Action)}\",");
            json.AppendLine($"  \"Skill2Action\": \"{EscapeJsonString(settings.Skill2Action)}\",");
            json.AppendLine($"  \"Skill3Action\": \"{EscapeJsonString(settings.Skill3Action)}\",");
            json.AppendLine($"  \"Skill4Action\": \"{EscapeJsonString(settings.Skill4Action)}\",");
            json.AppendLine($"  \"PrimaryAttackAction\": \"{EscapeJsonString(settings.PrimaryAttackAction)}\",");
            json.AppendLine($"  \"SecondaryAttackAction\": \"{EscapeJsonString(settings.SecondaryAttackAction)}\",");
            json.AppendLine($"  \"MoveAction\": \"{EscapeJsonString(settings.MoveAction)}\",");
            json.AppendLine($"  \"PotionAction\": \"{EscapeJsonString(settings.PotionAction)}\",");
            json.AppendLine($"  \"DodgeAction\": \"{EscapeJsonString(settings.DodgeAction)}\",");
            json.AppendLine($"  \"ToggleAutomationAction\": \"{EscapeJsonString(settings.ToggleAutomationAction)}\",");
            json.AppendLine($"  \"ToggleMouseMoveAction\": \"{EscapeJsonString(settings.ToggleMouseMoveAction)}\",");
            json.AppendLine($"  \"LastLoadedConfigFile\": \"{EscapeJsonString(settings.LastLoadedConfigFile)}\"");
            json.AppendLine("}");
            return json.ToString();
        }

        private string EscapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private Settings DeserializeSettingsFromJson(string json)
        {
            var settings = new Settings();
            
            // Simple JSON parsing - split by lines and extract key-value pairs
            var lines = json.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim().TrimEnd(',');
                if (trimmed.Contains(":"))
                {
                    var parts = trimmed.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim().Trim('"');
                        var value = parts[1].Trim().Trim('"');
                        
                        switch (key)
                        {
                            case "Skill1Delay":
                                if (int.TryParse(value, out int skill1Delay)) settings.Skill1Delay = skill1Delay;
                                break;
                            case "Skill2Delay":
                                if (int.TryParse(value, out int skill2Delay)) settings.Skill2Delay = skill2Delay;
                                break;
                            case "Skill3Delay":
                                if (int.TryParse(value, out int skill3Delay)) settings.Skill3Delay = skill3Delay;
                                break;
                            case "Skill4Delay":
                                if (int.TryParse(value, out int skill4Delay)) settings.Skill4Delay = skill4Delay;
                                break;
                            case "PrimaryAttackDelay":
                                if (int.TryParse(value, out int primaryDelay)) settings.PrimaryAttackDelay = primaryDelay;
                                break;
                            case "SecondaryAttackDelay":
                                if (int.TryParse(value, out int secondaryDelay)) settings.SecondaryAttackDelay = secondaryDelay;
                                break;
                            case "MoveDelay":
                                if (int.TryParse(value, out int moveDelay)) settings.MoveDelay = moveDelay;
                                break;
                            case "PotionDelay":
                                if (int.TryParse(value, out int potionDelay)) settings.PotionDelay = potionDelay;
                                break;
                            case "DodgeDelay":
                                if (int.TryParse(value, out int dodgeDelay)) settings.DodgeDelay = dodgeDelay;
                                break;
                            case "Skill1Action":
                                settings.Skill1Action = value;
                                break;
                            case "Skill2Action":
                                settings.Skill2Action = value;
                                break;
                            case "Skill3Action":
                                settings.Skill3Action = value;
                                break;
                            case "Skill4Action":
                                settings.Skill4Action = value;
                                break;
                            case "PrimaryAttackAction":
                                settings.PrimaryAttackAction = value;
                                break;
                            case "SecondaryAttackAction":
                                settings.SecondaryAttackAction = value;
                                break;
                            case "MoveAction":
                                settings.MoveAction = value;
                                break;
                            case "PotionAction":
                                settings.PotionAction = value;
                                break;
                            case "DodgeAction":
                                settings.DodgeAction = value;
                                break;
                            case "ToggleAutomationAction":
                                settings.ToggleAutomationAction = value;
                                break;
                            case "ToggleMouseMoveAction":
                                settings.ToggleMouseMoveAction = value;
                                break;
                            case "LastLoadedConfigFile":
                                settings.LastLoadedConfigFile = value;
                                break;
                        }
                    }
                }
            }

            return settings;
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckForPendingChanges(); // Check for pending changes first
            if (!PromptSaveChanges())
                return; // User cancelled, don't continue with load

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.DefaultExt = "json";
                openFileDialog.Title = "Load Configuration";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(openFileDialog.FileName);
                        var loadedSettings = DeserializeSettingsFromJson(jsonContent);
                        
                        settings = loadedSettings;
                        settings.LastLoadedConfigFile = openFileDialog.FileName; // Store for autoload
                        currentFileName = openFileDialog.FileName;
                        ApplySettingsToControls();
                        UpdateKeyMappings();
                        UpdateLabels();
                        SaveSettings(); // Save to XML as well
                        MarkAsSaved(); // Mark as saved since we just loaded
                        
                        AddToRecentFiles(openFileDialog.FileName);
                        
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading configuration: {ex.Message}", "Load Configuration Error", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "json";
                saveFileDialog.Title = "Save Configuration";
                saveFileDialog.FileName = "D4AutomatorConfig.json";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string jsonContent = SerializeSettingsToJson(settings);
                        File.WriteAllText(saveFileDialog.FileName, jsonContent);
                        currentFileName = saveFileDialog.FileName;
                        MarkAsSaved();
                        
                        AddToRecentFiles(saveFileDialog.FileName);
                        
                        MessageBox.Show("Configuration saved successfully!", "Save Configuration", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving configuration: {ex.Message}", "Save Configuration Error", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void resetToDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckForPendingChanges(); // Check for pending changes first
            if (!PromptSaveChanges())
                return; // User cancelled, don't continue with reset

            if (MessageBox.Show("This will reset all delays to their default values. Continue?", 
                              "Reset to Defaults", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Set default values: 1000 for skills/attacks, 0 for potion and dodge
                settings.Skill1Delay = 1000;
                settings.Skill2Delay = 1000;
                settings.Skill3Delay = 1000;
                settings.Skill4Delay = 1000;
                settings.PrimaryAttackDelay = 1000;
                settings.SecondaryAttackDelay = 1000;
                settings.PotionDelay = 0;
                settings.DodgeDelay = 0;
                
                ApplySettingsToControls();
                MarkAsChanged();
                SaveSettings();
                
                MessageBox.Show("Settings have been reset to defaults.", "Reset Complete", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CheckForPendingChanges()
        {
            // Check if any numeric controls have different values than stored settings
            // This catches changes where user typed but didn't exit the field
            if ((int)nudSkill1.Value != settings.Skill1Delay ||
                (int)nudSkill2.Value != settings.Skill2Delay ||
                (int)nudSkill3.Value != settings.Skill3Delay ||
                (int)nudSkill4.Value != settings.Skill4Delay ||
                (int)nudPrimaryAttack.Value != settings.PrimaryAttackDelay ||
                (int)nudSecondaryAttack.Value != settings.SecondaryAttackDelay ||
                (int)nudMove.Value != settings.MoveDelay ||
                (int)nudPotion.Value != settings.PotionDelay ||
                (int)nudDodge.Value != settings.DodgeDelay)
            {
                // Force validation of all controls to trigger ValueChanged events
                this.ValidateChildren();
                MarkAsChanged();
            }
        }

        private bool PromptSaveChanges()
        {
            CheckForPendingChanges(); // Check for any pending changes first
            
            if (hasUnsavedChanges)
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to save them?", 
                                           "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                
                switch (result)
                {
                    case DialogResult.Yes:
                        if (!string.IsNullOrEmpty(currentFileName))
                        {
                            try
                            {
                                string jsonContent = SerializeSettingsToJson(settings);
                                File.WriteAllText(currentFileName, jsonContent);
                                MarkAsSaved();
                                return true;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error saving file: {ex.Message}", "Save Error", 
                                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                        else
                        {
                            // Show save dialog
                            using (var saveFileDialog = new SaveFileDialog())
                            {
                                saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                                saveFileDialog.DefaultExt = "json";
                                saveFileDialog.Title = "Save Configuration";
                                saveFileDialog.FileName = "D4AutomatorConfig.json";

                                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    try
                                    {
                                        string jsonContent = SerializeSettingsToJson(settings);
                                        File.WriteAllText(saveFileDialog.FileName, jsonContent);
                                        currentFileName = saveFileDialog.FileName;
                                        MarkAsSaved();
                                        return true;
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Error saving configuration: {ex.Message}", "Save Configuration Error", 
                                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false; // User cancelled save dialog
                                }
                            }
                        }
                    case DialogResult.No:
                        return true; // Don't save, but continue
                    case DialogResult.Cancel:
                        return false; // Cancel the operation
                }
            }
            return true; // No changes to save
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AddToRecentFiles(string filePath)
        {
            var recentFiles = LoadRecentFiles();

            // Remove if already exists
            recentFiles.Remove(filePath);
            
            // Add to beginning
            recentFiles.Insert(0, filePath);
            
            // Keep only 5 most recent
            if (recentFiles.Count > 5)
            {
                recentFiles.RemoveAt(5);
            }
            
            SaveRecentFiles(recentFiles);
            UpdateRecentFilesMenu();
        }

        private List<string> LoadRecentFiles()
        {
            var recentFiles = new List<string>();
            
            if (File.Exists(recentFilesPath))
            {
                try
                {
                    var lines = File.ReadAllLines(recentFilesPath);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && File.Exists(line))
                        {
                            recentFiles.Add(line.Trim());
                        }
                    }
                }
                catch
                {
                    // If file is corrupted, start fresh
                }
            }
            
            return recentFiles;
        }

        private void SaveRecentFiles(List<string> recentFiles)
        {
            try
            {
                File.WriteAllLines(recentFilesPath, recentFiles);
            }
            catch
            {
                // Ignore save errors for recent files
            }
        }

        private void UpdateRecentFilesMenu()
        {
            // Remove existing recent file menu items
            var itemsToRemove = new List<ToolStripItem>();
            foreach (ToolStripItem item in fileToolStripMenuItem.DropDownItems)
            {
                if (item.Tag?.ToString() == "RecentFile" || item.Tag?.ToString().StartsWith("RecentFile:") == true)
                {
                    itemsToRemove.Add(item);
                }
            }
            foreach (var item in itemsToRemove)
            {
                fileToolStripMenuItem.DropDownItems.Remove(item);
            }

            // Load recent files from text file
            var recentFiles = LoadRecentFiles();
            
            // Add recent files if any exist
            if (recentFiles.Count > 0)
            {
                // Find the position after "Save Configuration..."
                int insertIndex = fileToolStripMenuItem.DropDownItems.IndexOf(saveConfigurationToolStripMenuItem) + 1;
                
                // Add separator
                var separator = new ToolStripSeparator();
                separator.Tag = "RecentFile";
                fileToolStripMenuItem.DropDownItems.Insert(insertIndex, separator);
                insertIndex++;

                // Add recent files
                for (int i = 0; i < recentFiles.Count; i++)
                {
                    string filePath = recentFiles[i];
                    var menuItem = new ToolStripMenuItem();
                    menuItem.Text = $"&{i + 1} {Path.GetFileName(filePath)}";
                    menuItem.ToolTipText = filePath;
                    
                    // Store the path in the Tag along with a marker
                    menuItem.Tag = $"RecentFile:{filePath}";
                    menuItem.Click += RecentFileMenuItem_Click;
                    
                    fileToolStripMenuItem.DropDownItems.Insert(insertIndex, menuItem);
                    insertIndex++;
                }
            }
        }

        private void RecentFileMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem?.Tag?.ToString().StartsWith("RecentFile:") == true)
            {
                string filePath = menuItem.Tag.ToString().Substring("RecentFile:".Length);
                LoadRecentFile(filePath);
            }
        }

        private void LoadRecentFile(string filePath)
        {
            CheckForPendingChanges();
            if (!PromptSaveChanges())
                return;

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                var loadedSettings = DeserializeSettingsFromJson(jsonContent);
                
                settings = loadedSettings;
                settings.LastLoadedConfigFile = filePath;
                currentFileName = filePath;
                ApplySettingsToControls();
                UpdateKeyMappings();
                UpdateLabels();
                SaveSettings();
                MarkAsSaved();
                
                AddToRecentFiles(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Load Configuration Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Remove from recent files if it failed to load
                var recentFiles = LoadRecentFiles();
                recentFiles.Remove(filePath);
                SaveRecentFiles(recentFiles);
                UpdateRecentFilesMenu();
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
        public int MoveDelay { get; set; }
        public int PotionDelay { get; set; }
        public int DodgeDelay { get; set; }

        public string Skill1Action { get; set; }
        public string Skill2Action { get; set; }
        public string Skill3Action { get; set; }
        public string Skill4Action { get; set; }
        public string PrimaryAttackAction { get; set; }
        public string SecondaryAttackAction { get; set; }
        public string MoveAction { get; set; }
        public string PotionAction { get; set; }
        public string DodgeAction { get; set; }
        public string ToggleAutomationAction { get; set; }
        public string ToggleMouseMoveAction { get; set; }
        public string LastLoadedConfigFile { get; set; }

        public Settings()
        {
            // Set default values
            Skill1Action = "D1";
            Skill2Action = "D2";
            Skill3Action = "D3";
            Skill4Action = "D4";
            PrimaryAttackAction = "RightClick";
            SecondaryAttackAction = "LeftClick";
            MoveAction = "RightClick";
            PotionAction = "Q";
            DodgeAction = "Space";
            ToggleAutomationAction = "F5";
            ToggleMouseMoveAction = "F6";

            // Set default delays
            Skill1Delay = 1000;
            Skill2Delay = 1000;
            Skill3Delay = 1000;
            Skill4Delay = 1000;
            PrimaryAttackDelay = 100;
            SecondaryAttackDelay = 100;
            MoveDelay = 100;
            PotionDelay = 5000;
            DodgeDelay = 1000;
        }
    }
}