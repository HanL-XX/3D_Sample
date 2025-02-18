namespace TreeViewApp
{
    partial class Form1
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
            btnLoadXml = new Button();
            treeViewXml = new TreeView();
            panelTop = new Panel();
            groupBoxTreeView = new GroupBox();

            // 
            // panelTop
            // 
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 60;
            panelTop.BackColor = Color.LightGray;
            panelTop.Padding = new Padding(10);

            // 
            // btnLoadXml
            // 
            btnLoadXml.Text = "📂 Load XML";
            btnLoadXml.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLoadXml.Size = new Size(150, 40);
            btnLoadXml.Location = new Point(10, 10);
            btnLoadXml.FlatStyle = FlatStyle.Flat;
            btnLoadXml.FlatAppearance.BorderSize = 1;
            btnLoadXml.BackColor = Color.WhiteSmoke;
            btnLoadXml.Click += btnLoadXml_Click;

            // 
            // groupBoxTreeView (bọc TreeView)
            // 
            groupBoxTreeView.Text = "XML Structure";
            groupBoxTreeView.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            groupBoxTreeView.Dock = DockStyle.Fill;
            groupBoxTreeView.Padding = new Padding(10);

            // 
            // treeViewXml
            // 
            treeViewXml.Dock = DockStyle.Fill;
            treeViewXml.Font = new Font("Segoe UI", 10F);
            treeViewXml.BorderStyle = BorderStyle.FixedSingle;
            treeViewXml.AfterSelect += treeViewXml_AfterSelect;

            // 
            // Form1
            // 
            ClientSize = new Size(800, 500);
            Controls.Add(groupBoxTreeView);
            Controls.Add(panelTop);
            panelTop.Controls.Add(btnLoadXml);
            groupBoxTreeView.Controls.Add(treeViewXml);
            Text = "XML Viewer";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            ResumeLayout(false);
        }

        #endregion

        private Button btnLoadXml;
        private TreeView treeViewXml;
        private OpenFileDialog openFileDialog1 = new OpenFileDialog();
        private Panel panelTop;
        private GroupBox groupBoxTreeView;
    }
}
