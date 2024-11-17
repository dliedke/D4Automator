using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace D4Automator
{
    public partial class KeyConfigForm : Form
    {
        private Settings settings;
        private Dictionary<string, TextBox> actionControls;
        private bool isListening = false;
        private bool isFirstClick = true;

        private static readonly Keys[] RestrictedKeys = new Keys[]
        {
            Keys.Alt, Keys.LMenu, Keys.RMenu,
            Keys.Shift, Keys.LShiftKey, Keys.RShiftKey,
            Keys.LWin, Keys.RWin,
            Keys.Control, Keys.LControlKey, Keys.RControlKey,
            Keys.Tab, Keys.CapsLock, Keys.NumLock, Keys.Scroll,
            Keys.PrintScreen, Keys.Pause, Keys.Enter,
            Keys.ShiftKey, Keys.ControlKey, Keys.Menu
        };

        public KeyConfigForm(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            InitializeActionControls();
            LoadCurrentActionMappings();
            ApplyDarkMode();

            btnSave.Focus();
        }

        private void InitializeActionControls()
        {
            actionControls = new Dictionary<string, TextBox>
            {
                {"Skill1Action", txtSkill1},
                {"Skill2Action", txtSkill2},
                {"Skill3Action", txtSkill3},
                {"Skill4Action", txtSkill4},
                {"PrimaryAttackAction", txtPrimaryAttack},
                {"SecondaryAttackAction", txtSecondaryAttack},
                {"PotionAction", txtPotion},
                {"DodgeAction", txtDodge},
                {"ToggleAutomationAction", txtToggleAutomation}
            };

            foreach (var textBox in actionControls.Values)
            {
                textBox.ReadOnly = true;
                textBox.Enter += TextBox_Enter;
                textBox.Leave += TextBox_Leave;
                textBox.KeyDown += TextBox_KeyDown;
                textBox.MouseDown += TextBox_MouseDown;
            }
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            isFirstClick = true;
            isListening = false;
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            isListening = false;
            isFirstClick = true;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (isListening)
            {
                var textBox = (TextBox)sender;
                if (!IsValidKey(e.KeyCode, textBox))
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                }

                string displayValue = KeyToString(e.KeyCode);
                if (IsUnique(displayValue, textBox))
                {
                    textBox.Text = displayValue;
                    textBox.Tag = e.KeyCode; // Set the Tag for keyboard inputs
                    isListening = false;
                    isFirstClick = true;
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        private string GetDisplayTextForKey(Keys key)
        {
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                return key.ToString().Substring(1); // Remove the 'D' prefix for display
            }
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                return $"NumPad {key.ToString().Substring(6)}";
            }
            return key.ToString();
        }

        private void LoadCurrentActionMappings()
        {
            foreach (var kvp in actionControls)
            {
                var property = settings.GetType().GetProperty(kvp.Key);
                if (property != null)
                {
                    var value = property.GetValue(settings);
                    if (Enum.TryParse(value.ToString(), out Keys key))
                    {
                        kvp.Value.Text = GetDisplayTextForKey(key);
                        kvp.Value.Tag = key;
                    }
                    else
                    {
                        kvp.Value.Text = value.ToString();
                    }
                }
            }
        }

        private void TextBox_MouseDown(object sender, MouseEventArgs e)
        {
            var textBox = (TextBox)sender;

            if (isFirstClick)
            {
                textBox.Text = "Press key or mouse click...";
                isFirstClick = false;
                isListening = true;
            }
            else if (isListening && e.Button != MouseButtons.None)
            {
                string clickAction = GetMouseClickAction(e.Button);
                if (!string.IsNullOrEmpty(clickAction) && IsUnique(clickAction, textBox))
                {
                    textBox.Text = clickAction;
                    textBox.Tag = null; // Clear the Tag for mouse clicks
                    isListening = false;
                    isFirstClick = true;
                }
            }
        }

        private bool IsValidKey(Keys key, TextBox currentTextBox)
        {
            if (RestrictedKeys.Contains(key) || key == Keys.None)
            {
                MessageBox.Show("This key is not allowed.", "Invalid Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string keyString = KeyToString(key);
            return IsUnique(keyString, currentTextBox);
        }

        private bool IsUnique(string actionString, TextBox currentTextBox)
        {
            // Get all current values from other textboxes
            var existingValues = actionControls
                .Where(kvp => kvp.Value != currentTextBox)
                .Select(kvp => kvp.Value.Text)
                .ToList();

            if (existingValues.Contains(actionString))
            {
                MessageBox.Show("This action is already in use for another binding.",
                              "Duplicate Action",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private string GetMouseClickAction(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    return "LeftClick";
                case MouseButtons.Right:
                    return "RightClick";
                case MouseButtons.Middle:
                    return "MiddleClick";
                case MouseButtons.XButton1:
                    return "MouseBack";  // Usually "Back" button
                case MouseButtons.XButton2:
                    return "MouseForward";  // Usually "Forward" button
                default:
                    return string.Empty;
            }
        }

        private string KeyToString(Keys key)
        {
            if (key >= Keys.D0 && key <= Keys.D9)
            {
                return key.ToString().Substring(1); // Remove the 'D' prefix for display
            }
            if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
            {
                return $"NumPad {key.ToString().Substring(6)}";
            }
            return key.ToString();
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            const string placeholderText = "Press key or mouse click...";

            var invalidActions = actionControls.Where(kvp => string.IsNullOrWhiteSpace(kvp.Value.Text) || kvp.Value.Text == placeholderText)
                                               .Select(kvp => kvp.Key.Replace("Action", ""))
                                               .ToList();

            if (invalidActions.Any())
            {
                string actionList = string.Join(", ", invalidActions);
                MessageBox.Show($"The following actions do not have valid keys assigned: {actionList}. Please assign keys to all actions before saving.",
                                "Incomplete Configuration",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            foreach (var kvp in actionControls)
            {
                var property = settings.GetType().GetProperty(kvp.Key);
                if (property != null)
                {
                    string value;
                    if (kvp.Value.Tag != null)
                    {
                        // For keyboard inputs, use the Keys enum value
                        value = ((Keys)kvp.Value.Tag).ToString();
                    }
                    else
                    {
                        // For mouse clicks, use the text directly
                        value = kvp.Value.Text;
                    }
                    property.SetValue(settings, value);
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ApplyDarkMode()
        {
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            foreach (Control control in this.Controls)
            {
                ApplyDarkModeToControl(control);
            }

            btnSave.BackColor = Color.FromArgb(60, 60, 60);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderColor = Color.Gray;

            bntCancel.BackColor = Color.FromArgb(60, 60, 60);
            bntCancel.FlatStyle = FlatStyle.Flat;
            bntCancel.FlatAppearance.BorderColor = Color.Gray;
        }

        private void ApplyDarkModeToControl(Control control)
        {
            if (control is TextBox)
            {
                control.BackColor = Color.FromArgb(45, 45, 45);
                control.ForeColor = Color.White;
            }
            else if (control is Label)
            {
                control.ForeColor = Color.White;
            }
        }

        private void bntCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}