using System;
using System.Drawing;
using System.Windows.Forms;

namespace D4Automator
{
    public partial class HordesConfigForm : Form
    {
        private Settings settings;
        private Action onSettingsChanged;

        public HordesConfigForm(Settings settings, Action onSettingsChanged)
        {
            InitializeComponent();
            this.settings = settings;
            this.onSettingsChanged = onSettingsChanged;
            LoadSettings();
            ApplyDarkMode();
        }

        private void LoadSettings()
        {
            trackBarHordesCircleSize.Value = settings.HordesCircleSize;
            lblHordesCircleSizeValue.Text = settings.HordesCircleSize.ToString();
            nudCircleSpeed.Value = (decimal)settings.HordesCircleSpeed;
            nudTimerInterval.Value = settings.HordesTimerInterval;
        }

        private void trackBarHordesCircleSize_Scroll(object sender, EventArgs e)
        {
            settings.HordesCircleSize = trackBarHordesCircleSize.Value;
            lblHordesCircleSizeValue.Text = trackBarHordesCircleSize.Value.ToString();
            onSettingsChanged?.Invoke();
        }

        private void nudCircleSpeed_ValueChanged(object sender, EventArgs e)
        {
            settings.HordesCircleSpeed = (double)nudCircleSpeed.Value;
            onSettingsChanged?.Invoke();
        }

        private void nudTimerInterval_ValueChanged(object sender, EventArgs e)
        {
            settings.HordesTimerInterval = (int)nudTimerInterval.Value;
            onSettingsChanged?.Invoke();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ApplyDarkMode()
        {
            // Set dark background and light text for the form
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            // Apply dark mode to all controls
            foreach (Control control in this.Controls)
            {
                if (control is Label)
                {
                    control.ForeColor = Color.White;
                }
                else if (control is NumericUpDown)
                {
                    control.BackColor = Color.FromArgb(60, 60, 60);
                    control.ForeColor = Color.White;
                }
                else if (control is Button)
                {
                    control.BackColor = Color.FromArgb(60, 60, 60);
                    control.ForeColor = Color.White;
                }
                else if (control is TrackBar)
                {
                    control.BackColor = Color.FromArgb(45, 45, 48);
                }
            }
        }
    }
}
