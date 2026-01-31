namespace D4Automator
{
    partial class HordesConfigForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblHordesCircleSize = new System.Windows.Forms.Label();
            this.trackBarHordesCircleSize = new System.Windows.Forms.TrackBar();
            this.lblHordesCircleSizeValue = new System.Windows.Forms.Label();
            this.lblCircleSpeed = new System.Windows.Forms.Label();
            this.nudCircleSpeed = new System.Windows.Forms.NumericUpDown();
            this.lblTimerInterval = new System.Windows.Forms.Label();
            this.nudTimerInterval = new System.Windows.Forms.NumericUpDown();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarHordesCircleSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCircleSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimerInterval)).BeginInit();
            this.SuspendLayout();
            //
            // lblHordesCircleSize
            //
            this.lblHordesCircleSize.AutoSize = true;
            this.lblHordesCircleSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHordesCircleSize.Location = new System.Drawing.Point(18, 70);
            this.lblHordesCircleSize.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHordesCircleSize.Name = "lblHordesCircleSize";
            this.lblHordesCircleSize.Size = new System.Drawing.Size(92, 20);
            this.lblHordesCircleSize.TabIndex = 0;
            this.lblHordesCircleSize.Text = "Circle Size:";
            //
            // trackBarHordesCircleSize
            //
            this.trackBarHordesCircleSize.Location = new System.Drawing.Point(200, 70);
            this.trackBarHordesCircleSize.Maximum = 300;
            this.trackBarHordesCircleSize.Minimum = 50;
            this.trackBarHordesCircleSize.Name = "trackBarHordesCircleSize";
            this.trackBarHordesCircleSize.Size = new System.Drawing.Size(250, 45);
            this.trackBarHordesCircleSize.TabIndex = 1;
            this.trackBarHordesCircleSize.TickFrequency = 25;
            this.trackBarHordesCircleSize.Value = 100;
            this.trackBarHordesCircleSize.Scroll += new System.EventHandler(this.trackBarHordesCircleSize_Scroll);
            //
            // lblHordesCircleSizeValue
            //
            this.lblHordesCircleSizeValue.AutoSize = true;
            this.lblHordesCircleSizeValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHordesCircleSizeValue.Location = new System.Drawing.Point(460, 70);
            this.lblHordesCircleSizeValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblHordesCircleSizeValue.Name = "lblHordesCircleSizeValue";
            this.lblHordesCircleSizeValue.Size = new System.Drawing.Size(36, 20);
            this.lblHordesCircleSizeValue.TabIndex = 2;
            this.lblHordesCircleSizeValue.Text = "100";
            //
            // lblCircleSpeed
            //
            this.lblCircleSpeed.AutoSize = true;
            this.lblCircleSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCircleSpeed.Location = new System.Drawing.Point(18, 130);
            this.lblCircleSpeed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCircleSpeed.Name = "lblCircleSpeed";
            this.lblCircleSpeed.Size = new System.Drawing.Size(169, 20);
            this.lblCircleSpeed.TabIndex = 3;
            this.lblCircleSpeed.Text = "Circle Speed (deg/tick):";
            //
            // nudCircleSpeed
            //
            this.nudCircleSpeed.DecimalPlaces = 1;
            this.nudCircleSpeed.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nudCircleSpeed.Location = new System.Drawing.Point(200, 130);
            this.nudCircleSpeed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudCircleSpeed.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudCircleSpeed.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.nudCircleSpeed.Name = "nudCircleSpeed";
            this.nudCircleSpeed.Size = new System.Drawing.Size(105, 26);
            this.nudCircleSpeed.TabIndex = 4;
            this.nudCircleSpeed.Value = new decimal(new int[] {
            35,
            0,
            0,
            65536});
            this.nudCircleSpeed.ValueChanged += new System.EventHandler(this.nudCircleSpeed_ValueChanged);
            //
            // lblTimerInterval
            //
            this.lblTimerInterval.AutoSize = true;
            this.lblTimerInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTimerInterval.Location = new System.Drawing.Point(18, 180);
            this.lblTimerInterval.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTimerInterval.Name = "lblTimerInterval";
            this.lblTimerInterval.Size = new System.Drawing.Size(147, 20);
            this.lblTimerInterval.TabIndex = 5;
            this.lblTimerInterval.Text = "Timer Interval (ms):";
            //
            // nudTimerInterval
            //
            this.nudTimerInterval.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudTimerInterval.Location = new System.Drawing.Point(200, 180);
            this.nudTimerInterval.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudTimerInterval.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nudTimerInterval.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudTimerInterval.Name = "nudTimerInterval";
            this.nudTimerInterval.Size = new System.Drawing.Size(105, 26);
            this.nudTimerInterval.TabIndex = 6;
            this.nudTimerInterval.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudTimerInterval.ValueChanged += new System.EventHandler(this.nudTimerInterval_ValueChanged);
            //
            // btnClose
            //
            this.btnClose.Location = new System.Drawing.Point(200, 240);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(105, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // lblDescription
            //
            this.lblDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.Location = new System.Drawing.Point(18, 15);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(490, 45);
            this.lblDescription.TabIndex = 8;
            this.lblDescription.Text = "Configure mouse movement settings for Infernal Hordes automation.\r\nLower speed = slower movement. Higher interval = slower movement.";
            //
            // HordesConfigForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 290);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.nudTimerInterval);
            this.Controls.Add(this.lblTimerInterval);
            this.Controls.Add(this.nudCircleSpeed);
            this.Controls.Add(this.lblCircleSpeed);
            this.Controls.Add(this.lblHordesCircleSizeValue);
            this.Controls.Add(this.trackBarHordesCircleSize);
            this.Controls.Add(this.lblHordesCircleSize);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HordesConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hordes Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarHordesCircleSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCircleSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimerInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHordesCircleSize;
        private System.Windows.Forms.TrackBar trackBarHordesCircleSize;
        private System.Windows.Forms.Label lblHordesCircleSizeValue;
        private System.Windows.Forms.Label lblCircleSpeed;
        private System.Windows.Forms.NumericUpDown nudCircleSpeed;
        private System.Windows.Forms.Label lblTimerInterval;
        private System.Windows.Forms.NumericUpDown nudTimerInterval;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblDescription;
    }
}
