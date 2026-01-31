namespace D4Automator
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.resetToDefaultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblSkill1 = new System.Windows.Forms.Label();
            this.lblSkill2 = new System.Windows.Forms.Label();
            this.lblSkill3 = new System.Windows.Forms.Label();
            this.lblSkill4 = new System.Windows.Forms.Label();
            this.lblPrimaryAttack = new System.Windows.Forms.Label();
            this.lblSecondaryAttack = new System.Windows.Forms.Label();
            this.lblMove = new System.Windows.Forms.Label();
            this.lblPotion = new System.Windows.Forms.Label();
            this.lblDodge = new System.Windows.Forms.Label();
            this.nudSkill1 = new System.Windows.Forms.NumericUpDown();
            this.nudSkill2 = new System.Windows.Forms.NumericUpDown();
            this.nudSkill3 = new System.Windows.Forms.NumericUpDown();
            this.nudSkill4 = new System.Windows.Forms.NumericUpDown();
            this.nudPrimaryAttack = new System.Windows.Forms.NumericUpDown();
            this.nudSecondaryAttack = new System.Windows.Forms.NumericUpDown();
            this.nudMove = new System.Windows.Forms.NumericUpDown();
            this.nudPotion = new System.Windows.Forms.NumericUpDown();
            this.nudDodge = new System.Windows.Forms.NumericUpDown();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.btnKeyConfig = new System.Windows.Forms.Button();
            this.btnHordesConfig = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrimaryAttack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecondaryAttack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMove)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPotion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDodge)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(532, 24);
            this.menuStrip1.TabIndex = 20;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadConfigurationToolStripMenuItem,
            this.saveConfigurationToolStripMenuItem,
            this.toolStripSeparator1,
            this.resetToDefaultsToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadConfigurationToolStripMenuItem
            // 
            this.loadConfigurationToolStripMenuItem.Name = "loadConfigurationToolStripMenuItem";
            this.loadConfigurationToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.loadConfigurationToolStripMenuItem.Text = "&Load Configuration...";
            this.loadConfigurationToolStripMenuItem.Click += new System.EventHandler(this.loadConfigurationToolStripMenuItem_Click);
            // 
            // saveConfigurationToolStripMenuItem
            // 
            this.saveConfigurationToolStripMenuItem.Name = "saveConfigurationToolStripMenuItem";
            this.saveConfigurationToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.saveConfigurationToolStripMenuItem.Text = "&Save Configuration...";
            this.saveConfigurationToolStripMenuItem.Click += new System.EventHandler(this.saveConfigurationToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
            // 
            // resetToDefaultsToolStripMenuItem
            // 
            this.resetToDefaultsToolStripMenuItem.Name = "resetToDefaultsToolStripMenuItem";
            this.resetToDefaultsToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.resetToDefaultsToolStripMenuItem.Text = "&Reset to Defaults";
            this.resetToDefaultsToolStripMenuItem.Click += new System.EventHandler(this.resetToDefaultsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(183, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // lblSkill1
            // 
            this.lblSkill1.AutoSize = true;
            this.lblSkill1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSkill1.Location = new System.Drawing.Point(18, 47);
            this.lblSkill1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSkill1.Name = "lblSkill1";
            this.lblSkill1.Size = new System.Drawing.Size(156, 20);
            this.lblSkill1.TabIndex = 0;
            this.lblSkill1.Text = "Skill 1 (1) Delay (ms):";
            // 
            // lblSkill2
            // 
            this.lblSkill2.AutoSize = true;
            this.lblSkill2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSkill2.Location = new System.Drawing.Point(18, 87);
            this.lblSkill2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSkill2.Name = "lblSkill2";
            this.lblSkill2.Size = new System.Drawing.Size(156, 20);
            this.lblSkill2.TabIndex = 1;
            this.lblSkill2.Text = "Skill 2 (2) Delay (ms):";
            // 
            // lblSkill3
            // 
            this.lblSkill3.AutoSize = true;
            this.lblSkill3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSkill3.Location = new System.Drawing.Point(18, 127);
            this.lblSkill3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSkill3.Name = "lblSkill3";
            this.lblSkill3.Size = new System.Drawing.Size(156, 20);
            this.lblSkill3.TabIndex = 2;
            this.lblSkill3.Text = "Skill 3 (3) Delay (ms):";
            // 
            // lblSkill4
            // 
            this.lblSkill4.AutoSize = true;
            this.lblSkill4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSkill4.Location = new System.Drawing.Point(18, 167);
            this.lblSkill4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSkill4.Name = "lblSkill4";
            this.lblSkill4.Size = new System.Drawing.Size(156, 20);
            this.lblSkill4.TabIndex = 3;
            this.lblSkill4.Text = "Skill 4 (4) Delay (ms):";
            // 
            // lblPrimaryAttack
            // 
            this.lblPrimaryAttack.AutoSize = true;
            this.lblPrimaryAttack.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPrimaryAttack.Location = new System.Drawing.Point(18, 207);
            this.lblPrimaryAttack.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPrimaryAttack.Name = "lblPrimaryAttack";
            this.lblPrimaryAttack.Size = new System.Drawing.Size(194, 20);
            this.lblPrimaryAttack.TabIndex = 4;
            this.lblPrimaryAttack.Text = "Primary Attack Delay (ms):";
            // 
            // lblSecondaryAttack
            // 
            this.lblSecondaryAttack.AutoSize = true;
            this.lblSecondaryAttack.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSecondaryAttack.Location = new System.Drawing.Point(18, 247);
            this.lblSecondaryAttack.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSecondaryAttack.Name = "lblSecondaryAttack";
            this.lblSecondaryAttack.Size = new System.Drawing.Size(218, 20);
            this.lblSecondaryAttack.TabIndex = 5;
            this.lblSecondaryAttack.Text = "Secondary Attack Delay (ms):";
            // 
            // lblMove
            // 
            this.lblMove.AutoSize = true;
            this.lblMove.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMove.Location = new System.Drawing.Point(18, 287);
            this.lblMove.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMove.Name = "lblMove";
            this.lblMove.Size = new System.Drawing.Size(130, 20);
            this.lblMove.TabIndex = 6;
            this.lblMove.Text = "Move Delay (ms):";
            // 
            // lblPotion
            // 
            this.lblPotion.AutoSize = true;
            this.lblPotion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPotion.Location = new System.Drawing.Point(18, 327);
            this.lblPotion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPotion.Name = "lblPotion";
            this.lblPotion.Size = new System.Drawing.Size(163, 20);
            this.lblPotion.TabIndex = 7;
            this.lblPotion.Text = "Potion (Q) Delay (ms):";
            // 
            // lblDodge
            // 
            this.lblDodge.AutoSize = true;
            this.lblDodge.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDodge.Location = new System.Drawing.Point(18, 367);
            this.lblDodge.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDodge.Name = "lblDodge";
            this.lblDodge.Size = new System.Drawing.Size(200, 20);
            this.lblDodge.TabIndex = 8;
            this.lblDodge.Text = "Dodge (Space) Delay (ms):";
            // 
            // nudSkill1
            // 
            this.nudSkill1.Location = new System.Drawing.Point(390, 41);
            this.nudSkill1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudSkill1.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudSkill1.Name = "nudSkill1";
            this.nudSkill1.Size = new System.Drawing.Size(105, 26);
            this.nudSkill1.TabIndex = 8;
            // 
            // nudSkill2
            // 
            this.nudSkill2.Location = new System.Drawing.Point(390, 81);
            this.nudSkill2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudSkill2.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudSkill2.Name = "nudSkill2";
            this.nudSkill2.Size = new System.Drawing.Size(105, 26);
            this.nudSkill2.TabIndex = 9;
            // 
            // nudSkill3
            // 
            this.nudSkill3.Location = new System.Drawing.Point(390, 121);
            this.nudSkill3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudSkill3.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudSkill3.Name = "nudSkill3";
            this.nudSkill3.Size = new System.Drawing.Size(105, 26);
            this.nudSkill3.TabIndex = 10;
            // 
            // nudSkill4
            // 
            this.nudSkill4.Location = new System.Drawing.Point(390, 161);
            this.nudSkill4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudSkill4.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudSkill4.Name = "nudSkill4";
            this.nudSkill4.Size = new System.Drawing.Size(105, 26);
            this.nudSkill4.TabIndex = 11;
            // 
            // nudPrimaryAttack
            // 
            this.nudPrimaryAttack.Location = new System.Drawing.Point(390, 201);
            this.nudPrimaryAttack.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudPrimaryAttack.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudPrimaryAttack.Name = "nudPrimaryAttack";
            this.nudPrimaryAttack.Size = new System.Drawing.Size(105, 26);
            this.nudPrimaryAttack.TabIndex = 12;
            // 
            // nudSecondaryAttack
            // 
            this.nudSecondaryAttack.Location = new System.Drawing.Point(390, 241);
            this.nudSecondaryAttack.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudSecondaryAttack.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudSecondaryAttack.Name = "nudSecondaryAttack";
            this.nudSecondaryAttack.Size = new System.Drawing.Size(105, 26);
            this.nudSecondaryAttack.TabIndex = 13;
            // 
            // nudMove
            // 
            this.nudMove.Location = new System.Drawing.Point(390, 281);
            this.nudMove.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudMove.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudMove.Name = "nudMove";
            this.nudMove.Size = new System.Drawing.Size(105, 26);
            this.nudMove.TabIndex = 14;
            // 
            // nudPotion
            // 
            this.nudPotion.Location = new System.Drawing.Point(390, 321);
            this.nudPotion.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudPotion.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudPotion.Name = "nudPotion";
            this.nudPotion.Size = new System.Drawing.Size(105, 26);
            this.nudPotion.TabIndex = 15;
            // 
            // nudDodge
            // 
            this.nudDodge.Location = new System.Drawing.Point(390, 361);
            this.nudDodge.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.nudDodge.Maximum = new decimal(new int[] {
            30000,
            0,
            0,
            0});
            this.nudDodge.Name = "nudDodge";
            this.nudDodge.Size = new System.Drawing.Size(105, 26);
            this.nudDodge.TabIndex = 16;
            // 
            // lblInstructions
            // 
            this.lblInstructions.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstructions.Location = new System.Drawing.Point(18, 405);
            this.lblInstructions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(500, 103);
            this.lblInstructions.TabIndex = 17;
            this.lblInstructions.Text = "Press F5 to start/stop automation.\r\nPress F6 for automation + mouse move (Inferna" +
    "l Hordes).\r\n\r\nSet delay to 0 to disable an action.\r\nSet delay to 1 to keep butto" +
    "n/key pressed.";
            // 
            // btnKeyConfig
            //
            this.btnKeyConfig.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnKeyConfig.Location = new System.Drawing.Point(100, 525);
            this.btnKeyConfig.Name = "btnKeyConfig";
            this.btnKeyConfig.Size = new System.Drawing.Size(150, 29);
            this.btnKeyConfig.TabIndex = 18;
            this.btnKeyConfig.Text = "Configure Keys";
            this.btnKeyConfig.UseVisualStyleBackColor = true;
            this.btnKeyConfig.Click += new System.EventHandler(this.btnKeyConfig_Click);
            //
            // btnHordesConfig
            //
            this.btnHordesConfig.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnHordesConfig.Location = new System.Drawing.Point(260, 525);
            this.btnHordesConfig.Name = "btnHordesConfig";
            this.btnHordesConfig.Size = new System.Drawing.Size(150, 29);
            this.btnHordesConfig.TabIndex = 19;
            this.btnHordesConfig.Text = "Hordes Config";
            this.btnHordesConfig.UseVisualStyleBackColor = true;
            this.btnHordesConfig.Click += new System.EventHandler(this.btnHordesConfig_Click);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 579);
            this.Controls.Add(this.btnHordesConfig);
            this.Controls.Add(this.btnKeyConfig);
            this.Controls.Add(this.lblInstructions);
            this.Controls.Add(this.nudDodge);
            this.Controls.Add(this.nudPotion);
            this.Controls.Add(this.nudMove);
            this.Controls.Add(this.nudSecondaryAttack);
            this.Controls.Add(this.nudPrimaryAttack);
            this.Controls.Add(this.nudSkill4);
            this.Controls.Add(this.nudSkill3);
            this.Controls.Add(this.nudSkill2);
            this.Controls.Add(this.nudSkill1);
            this.Controls.Add(this.lblDodge);
            this.Controls.Add(this.lblPotion);
            this.Controls.Add(this.lblMove);
            this.Controls.Add(this.lblSecondaryAttack);
            this.Controls.Add(this.lblPrimaryAttack);
            this.Controls.Add(this.lblSkill4);
            this.Controls.Add(this.lblSkill3);
            this.Controls.Add(this.lblSkill2);
            this.Controls.Add(this.lblSkill1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "D4 Automator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkill4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrimaryAttack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSecondaryAttack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMove)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPotion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDodge)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSkill1;
        private System.Windows.Forms.Label lblSkill2;
        private System.Windows.Forms.Label lblSkill3;
        private System.Windows.Forms.Label lblSkill4;
        private System.Windows.Forms.Label lblPrimaryAttack;
        private System.Windows.Forms.Label lblSecondaryAttack;
        private System.Windows.Forms.Label lblMove;
        private System.Windows.Forms.Label lblPotion;
        private System.Windows.Forms.Label lblDodge;
        private System.Windows.Forms.NumericUpDown nudSkill1;
        private System.Windows.Forms.NumericUpDown nudSkill2;
        private System.Windows.Forms.NumericUpDown nudSkill3;
        private System.Windows.Forms.NumericUpDown nudSkill4;
        private System.Windows.Forms.NumericUpDown nudPrimaryAttack;
        private System.Windows.Forms.NumericUpDown nudSecondaryAttack;
        private System.Windows.Forms.NumericUpDown nudMove;
        private System.Windows.Forms.NumericUpDown nudPotion;
        private System.Windows.Forms.NumericUpDown nudDodge;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.Button btnKeyConfig;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem resetToDefaultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button btnHordesConfig;
    }
}