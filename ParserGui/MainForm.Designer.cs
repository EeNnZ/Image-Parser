namespace ParserGui
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.progressLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.websitesListBox = new MaterialSkin.Controls.MaterialCheckedListBox();
            this.label5 = new MaterialSkin.Controls.MaterialLabel();
            this.progressBar2 = new MaterialSkin.Controls.MaterialProgressBar();
            this.label4 = new MaterialSkin.Controls.MaterialLabel();
            this.progressBar1 = new MaterialSkin.Controls.MaterialProgressBar();
            this.statusTextBox = new MaterialSkin.Controls.MaterialMultiLineTextBox();
            this.goButton = new MaterialSkin.Controls.MaterialButton();
            this.cancelButton = new MaterialSkin.Controls.MaterialButton();
            this.searchTextBox = new MaterialSkin.Controls.MaterialTextBox();
            this.rangeTextBox = new MaterialSkin.Controls.MaterialMaskedTextBox();
            this.label1 = new MaterialSkin.Controls.MaterialLabel();
            this.label2 = new MaterialSkin.Controls.MaterialLabel();
            this.label3 = new MaterialSkin.Controls.MaterialLabel();
            this.statusStrip1.SuspendLayout();
            this.websitesListBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar,
            this.progressLabel});
            this.statusStrip1.Location = new System.Drawing.Point(3, 611);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1108, 29);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.SystemColors.ControlDark;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(115, 21);
            // 
            // progressLabel
            // 
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(65, 23);
            this.progressLabel.Text = "Progress";
            // 
            // websitesListBox
            // 
            this.websitesListBox.AutoScroll = true;
            this.websitesListBox.BackColor = System.Drawing.SystemColors.Control;
            this.websitesListBox.Controls.Add(this.label5);
            this.websitesListBox.Controls.Add(this.progressBar2);
            this.websitesListBox.Controls.Add(this.label4);
            this.websitesListBox.Controls.Add(this.progressBar1);
            this.websitesListBox.Depth = 0;
            this.websitesListBox.Location = new System.Drawing.Point(25, 93);
            this.websitesListBox.MouseState = MaterialSkin.MouseState.HOVER;
            this.websitesListBox.Name = "websitesListBox";
            this.websitesListBox.Size = new System.Drawing.Size(461, 241);
            this.websitesListBox.Striped = false;
            this.websitesListBox.StripeDarkColor = System.Drawing.Color.Empty;
            this.websitesListBox.TabIndex = 29;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Depth = 0;
            this.label5.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label5.Location = new System.Drawing.Point(406, 52);
            this.label5.MouseState = MaterialSkin.MouseState.HOVER;
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(22, 19);
            this.label5.TabIndex = 40;
            this.label5.Text = "0%";
            // 
            // progressBar2
            // 
            this.progressBar2.Depth = 0;
            this.progressBar2.Location = new System.Drawing.Point(233, 59);
            this.progressBar2.MouseState = MaterialSkin.MouseState.HOVER;
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(165, 5);
            this.progressBar2.TabIndex = 37;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Depth = 0;
            this.label4.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label4.Location = new System.Drawing.Point(406, 11);
            this.label4.MouseState = MaterialSkin.MouseState.HOVER;
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(22, 19);
            this.label4.TabIndex = 39;
            this.label4.Text = "0%";
            // 
            // progressBar1
            // 
            this.progressBar1.Depth = 0;
            this.progressBar1.Location = new System.Drawing.Point(233, 18);
            this.progressBar1.MouseState = MaterialSkin.MouseState.HOVER;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(165, 5);
            this.progressBar1.TabIndex = 36;
            // 
            // statusTextBox
            // 
            this.statusTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.statusTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.statusTextBox.Depth = 0;
            this.statusTextBox.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.statusTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.statusTextBox.Location = new System.Drawing.Point(492, 93);
            this.statusTextBox.MouseState = MaterialSkin.MouseState.HOVER;
            this.statusTextBox.Name = "statusTextBox";
            this.statusTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.statusTextBox.Size = new System.Drawing.Size(544, 429);
            this.statusTextBox.TabIndex = 31;
            this.statusTextBox.Text = "";
            this.statusTextBox.TextChanged += new System.EventHandler(this.StatusTextBoxTextChanged);
            // 
            // goButton
            // 
            this.goButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.goButton.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.goButton.Depth = 0;
            this.goButton.HighEmphasis = true;
            this.goButton.Icon = null;
            this.goButton.Location = new System.Drawing.Point(337, 531);
            this.goButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.goButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.goButton.Name = "goButton";
            this.goButton.NoAccentTextColor = System.Drawing.Color.Empty;
            this.goButton.Size = new System.Drawing.Size(64, 36);
            this.goButton.TabIndex = 32;
            this.goButton.Text = "Go";
            this.goButton.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.goButton.UseAccentColor = false;
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.GoButtonCLick);
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cancelButton.Density = MaterialSkin.Controls.MaterialButton.MaterialButtonDensity.Default;
            this.cancelButton.Depth = 0;
            this.cancelButton.HighEmphasis = true;
            this.cancelButton.Icon = null;
            this.cancelButton.Location = new System.Drawing.Point(409, 531);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.cancelButton.MouseState = MaterialSkin.MouseState.HOVER;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.NoAccentTextColor = System.Drawing.Color.Empty;
            this.cancelButton.Size = new System.Drawing.Size(77, 36);
            this.cancelButton.TabIndex = 33;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.cancelButton.UseAccentColor = false;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButtonOnClick);
            // 
            // searchTextBox
            // 
            this.searchTextBox.AnimateReadOnly = false;
            this.searchTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.searchTextBox.Depth = 0;
            this.searchTextBox.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.searchTextBox.LeadingIcon = null;
            this.searchTextBox.Location = new System.Drawing.Point(25, 387);
            this.searchTextBox.MaxLength = 50;
            this.searchTextBox.MouseState = MaterialSkin.MouseState.OUT;
            this.searchTextBox.Multiline = false;
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(461, 50);
            this.searchTextBox.TabIndex = 34;
            this.searchTextBox.Text = "";
            this.searchTextBox.TrailingIcon = null;
            // 
            // rangeTextBox
            // 
            this.rangeTextBox.AllowPromptAsInput = true;
            this.rangeTextBox.AnimateReadOnly = false;
            this.rangeTextBox.AsciiOnly = false;
            this.rangeTextBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.rangeTextBox.BeepOnError = false;
            this.rangeTextBox.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludeLiterals;
            this.rangeTextBox.Depth = 0;
            this.rangeTextBox.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.rangeTextBox.HidePromptOnLeave = false;
            this.rangeTextBox.HideSelection = true;
            this.rangeTextBox.InsertKeyMode = System.Windows.Forms.InsertKeyMode.Default;
            this.rangeTextBox.LeadingIcon = null;
            this.rangeTextBox.Location = new System.Drawing.Point(25, 474);
            this.rangeTextBox.Mask = "0 - 0";
            this.rangeTextBox.MaxLength = 32767;
            this.rangeTextBox.MouseState = MaterialSkin.MouseState.OUT;
            this.rangeTextBox.Name = "rangeTextBox";
            this.rangeTextBox.PasswordChar = '\0';
            this.rangeTextBox.PrefixSuffixText = null;
            this.rangeTextBox.PromptChar = '_';
            this.rangeTextBox.ReadOnly = false;
            this.rangeTextBox.RejectInputOnFirstFailure = false;
            this.rangeTextBox.ResetOnPrompt = true;
            this.rangeTextBox.ResetOnSpace = true;
            this.rangeTextBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.rangeTextBox.SelectedText = "";
            this.rangeTextBox.SelectionLength = 0;
            this.rangeTextBox.SelectionStart = 0;
            this.rangeTextBox.ShortcutsEnabled = true;
            this.rangeTextBox.Size = new System.Drawing.Size(461, 48);
            this.rangeTextBox.SkipLiterals = true;
            this.rangeTextBox.TabIndex = 35;
            this.rangeTextBox.TabStop = false;
            this.rangeTextBox.Text = "  - ";
            this.rangeTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            this.rangeTextBox.TextMaskFormat = System.Windows.Forms.MaskFormat.IncludeLiterals;
            this.rangeTextBox.TrailingIcon = null;
            this.rangeTextBox.UseSystemPasswordChar = false;
            this.rangeTextBox.ValidatingType = null;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Depth = 0;
            this.label1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label1.Location = new System.Drawing.Point(25, 71);
            this.label1.MouseState = MaterialSkin.MouseState.HOVER;
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 19);
            this.label1.TabIndex = 36;
            this.label1.Text = "Websites";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Depth = 0;
            this.label2.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label2.Location = new System.Drawing.Point(25, 365);
            this.label2.MouseState = MaterialSkin.MouseState.HOVER;
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 19);
            this.label2.TabIndex = 37;
            this.label2.Text = "Search Term";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Depth = 0;
            this.label3.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.label3.Location = new System.Drawing.Point(25, 452);
            this.label3.MouseState = MaterialSkin.MouseState.HOVER;
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 19);
            this.label3.TabIndex = 38;
            this.label3.Text = "Start/End Point";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1114, 643);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rangeTextBox);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.statusTextBox);
            this.Controls.Add(this.websitesListBox);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Image Parser";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.websitesListBox.ResumeLayout(false);
            this.websitesListBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel progressLabel;
        private ToolStripProgressBar progressBar;
        private MaterialSkin.Controls.MaterialCheckedListBox websitesListBox;
        private MaterialSkin.Controls.MaterialMultiLineTextBox statusTextBox;
        private MaterialSkin.Controls.MaterialButton goButton;
        private MaterialSkin.Controls.MaterialButton cancelButton;
        private MaterialSkin.Controls.MaterialTextBox searchTextBox;
        private MaterialSkin.Controls.MaterialMaskedTextBox rangeTextBox;
        private MaterialSkin.Controls.MaterialProgressBar progressBar1;
        private MaterialSkin.Controls.MaterialProgressBar progressBar2;
        private MaterialSkin.Controls.MaterialLabel label5;
        private MaterialSkin.Controls.MaterialLabel label4;
        private MaterialSkin.Controls.MaterialLabel label1;
        private MaterialSkin.Controls.MaterialLabel label2;
        private MaterialSkin.Controls.MaterialLabel label3;
    }
}