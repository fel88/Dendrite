namespace Dendrite
{
    partial class Processing
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.convertToIamgeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listView2 = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.listView3 = new System.Windows.Forms.ListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nCHWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.meanstdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.template1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveImageToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.binaryMaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button3 = new System.Windows.Forms.Button();
            this.saveArrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.binaryAsisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(8, 8);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Run";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Input";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(246, 76);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(293, 170);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(246, 47);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "load image";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 221);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Output";
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(53, 221);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(187, 136);
            this.listView1.TabIndex = 7;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Dims";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertToIamgeToolStripMenuItem,
            this.saveImageToFileToolStripMenuItem,
            this.saveArrayToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 92);
            // 
            // convertToIamgeToolStripMenuItem
            // 
            this.convertToIamgeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.binaryMaskToolStripMenuItem,
            this.rGBToolStripMenuItem});
            this.convertToIamgeToolStripMenuItem.Name = "convertToIamgeToolStripMenuItem";
            this.convertToIamgeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.convertToIamgeToolStripMenuItem.Text = "convert to image";
            this.convertToIamgeToolStripMenuItem.Click += new System.EventHandler(this.convertToIamgeToolStripMenuItem_Click);
            // 
            // listView2
            // 
            this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listView2.FullRowSelect = true;
            this.listView2.GridLines = true;
            this.listView2.HideSelection = false;
            this.listView2.Location = new System.Drawing.Point(53, 47);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(187, 136);
            this.listView2.TabIndex = 8;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.Details;
            this.listView2.SelectedIndexChanged += new System.EventHandler(this.listView2_SelectedIndexChanged);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Name";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Dims";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Location = new System.Drawing.Point(246, 252);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(421, 114);
            this.richTextBox1.TabIndex = 9;
            this.richTextBox1.Text = "";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(738, 53);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(91, 17);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "use mean/std";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(740, 76);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 12;
            this.textBox1.Text = "75";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(740, 102);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 13;
            this.textBox2.Text = "75";
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(740, 128);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 20);
            this.textBox3.TabIndex = 14;
            this.textBox3.Text = "75";
            this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(846, 128);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(100, 20);
            this.textBox4.TabIndex = 17;
            this.textBox4.Text = "45";
            this.textBox4.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(846, 102);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(100, 20);
            this.textBox5.TabIndex = 16;
            this.textBox5.Text = "45";
            this.textBox5.TextChanged += new System.EventHandler(this.textBox5_TextChanged);
            // 
            // textBox6
            // 
            this.textBox6.Location = new System.Drawing.Point(846, 76);
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(100, 20);
            this.textBox6.TabIndex = 15;
            this.textBox6.Text = "45";
            this.textBox6.TextChanged += new System.EventHandler(this.textBox6_TextChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(738, 166);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(73, 17);
            this.checkBox2.TabIndex = 18;
            this.checkBox2.Text = "normalize ";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // textBox7
            // 
            this.textBox7.Location = new System.Drawing.Point(735, 189);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(30, 20);
            this.textBox7.TabIndex = 19;
            this.textBox7.Text = "0";
            // 
            // textBox8
            // 
            this.textBox8.Location = new System.Drawing.Point(776, 189);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(30, 20);
            this.textBox8.TabIndex = 20;
            this.textBox8.Text = "1";
            // 
            // listView3
            // 
            this.listView3.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6});
            this.listView3.ContextMenuStrip = this.contextMenuStrip2;
            this.listView3.FullRowSelect = true;
            this.listView3.GridLines = true;
            this.listView3.Location = new System.Drawing.Point(545, 53);
            this.listView3.Name = "listView3";
            this.listView3.Size = new System.Drawing.Size(187, 136);
            this.listView3.TabIndex = 21;
            this.listView3.UseCompatibleStateImageBehavior = false;
            this.listView3.View = System.Windows.Forms.View.Details;
            this.listView3.SelectedIndexChanged += new System.EventHandler(this.listView3_SelectedIndexChanged);
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Name";
            this.columnHeader6.Width = 120;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(542, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Preprocessors";
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(107, 48);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resizeToolStripMenuItem,
            this.nCHWToolStripMenuItem,
            this.meanstdToolStripMenuItem,
            this.normalizeToolStripMenuItem,
            this.toolStripSeparator1,
            this.template1ToolStripMenuItem});
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addToolStripMenuItem.Text = "add";
            // 
            // resizeToolStripMenuItem
            // 
            this.resizeToolStripMenuItem.Name = "resizeToolStripMenuItem";
            this.resizeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.resizeToolStripMenuItem.Text = "resize";
            this.resizeToolStripMenuItem.Click += new System.EventHandler(this.resizeToolStripMenuItem_Click);
            // 
            // nCHWToolStripMenuItem
            // 
            this.nCHWToolStripMenuItem.Name = "nCHWToolStripMenuItem";
            this.nCHWToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.nCHWToolStripMenuItem.Text = "NCHW";
            this.nCHWToolStripMenuItem.Click += new System.EventHandler(this.nCHWToolStripMenuItem_Click);
            // 
            // meanstdToolStripMenuItem
            // 
            this.meanstdToolStripMenuItem.Name = "meanstdToolStripMenuItem";
            this.meanstdToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.meanstdToolStripMenuItem.Text = "mean/std";
            this.meanstdToolStripMenuItem.Click += new System.EventHandler(this.meanstdToolStripMenuItem_Click);
            // 
            // normalizeToolStripMenuItem
            // 
            this.normalizeToolStripMenuItem.Name = "normalizeToolStripMenuItem";
            this.normalizeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.normalizeToolStripMenuItem.Text = "normalize";
            this.normalizeToolStripMenuItem.Click += new System.EventHandler(this.normalizeToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.deleteToolStripMenuItem.Text = "delete";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // template1ToolStripMenuItem
            // 
            this.template1ToolStripMenuItem.Name = "template1ToolStripMenuItem";
            this.template1ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.template1ToolStripMenuItem.Text = "template1";
            this.template1ToolStripMenuItem.Click += new System.EventHandler(this.template1ToolStripMenuItem_Click);
            // 
            // saveImageToFileToolStripMenuItem
            // 
            this.saveImageToFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toFileToolStripMenuItem,
            this.toClipboardToolStripMenuItem});
            this.saveImageToFileToolStripMenuItem.Name = "saveImageToFileToolStripMenuItem";
            this.saveImageToFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveImageToFileToolStripMenuItem.Text = "save image";
            this.saveImageToFileToolStripMenuItem.Click += new System.EventHandler(this.saveImageToFileToolStripMenuItem_Click);
            // 
            // toFileToolStripMenuItem
            // 
            this.toFileToolStripMenuItem.Name = "toFileToolStripMenuItem";
            this.toFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.toFileToolStripMenuItem.Text = "to file";
            this.toFileToolStripMenuItem.Click += new System.EventHandler(this.toFileToolStripMenuItem_Click);
            // 
            // toClipboardToolStripMenuItem
            // 
            this.toClipboardToolStripMenuItem.Name = "toClipboardToolStripMenuItem";
            this.toClipboardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.toClipboardToolStripMenuItem.Text = "to clipboard";
            this.toClipboardToolStripMenuItem.Click += new System.EventHandler(this.toClipboardToolStripMenuItem_Click);
            // 
            // binaryMaskToolStripMenuItem
            // 
            this.binaryMaskToolStripMenuItem.Name = "binaryMaskToolStripMenuItem";
            this.binaryMaskToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.binaryMaskToolStripMenuItem.Text = "binary mask";
            this.binaryMaskToolStripMenuItem.Click += new System.EventHandler(this.binaryMaskToolStripMenuItem_Click);
            // 
            // rGBToolStripMenuItem
            // 
            this.rGBToolStripMenuItem.Name = "rGBToolStripMenuItem";
            this.rGBToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.rGBToolStripMenuItem.Text = "RGB";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(373, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(166, 23);
            this.button3.TabIndex = 24;
            this.button3.Text = "show postprocess data";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // saveArrayToolStripMenuItem
            // 
            this.saveArrayToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.binaryAsisToolStripMenuItem});
            this.saveArrayToolStripMenuItem.Name = "saveArrayToolStripMenuItem";
            this.saveArrayToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveArrayToolStripMenuItem.Text = "save array";
            // 
            // binaryAsisToolStripMenuItem
            // 
            this.binaryAsisToolStripMenuItem.Name = "binaryAsisToolStripMenuItem";
            this.binaryAsisToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.binaryAsisToolStripMenuItem.Text = "binary as-is";
            this.binaryAsisToolStripMenuItem.Click += new System.EventHandler(this.binaryAsisToolStripMenuItem_Click);
            // 
            // Processing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(965, 396);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listView3);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Processing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Processing";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem convertToIamgeToolStripMenuItem;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.ListView listView3;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nCHWToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem meanstdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem normalizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem template1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveImageToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem binaryMaskToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rGBToolStripMenuItem;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ToolStripMenuItem saveArrayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem binaryAsisToolStripMenuItem;
    }
}