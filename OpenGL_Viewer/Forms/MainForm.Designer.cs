using System.Drawing;
using System.Windows.Forms;

namespace OpenGL_Viewer.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnLoadObj;
        private Button btnSplitModel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnLoadObj = new System.Windows.Forms.Button();
            this.btnSplitModel = new System.Windows.Forms.Button();

            SuspendLayout();

            // 
            // btnLoadObj
            // 
            this.btnLoadObj.Location = new System.Drawing.Point(20, 20);
            this.btnLoadObj.Size = new System.Drawing.Size(120, 40);
            this.btnLoadObj.Text = "Tải File OBJ";
            this.btnLoadObj.UseVisualStyleBackColor = true;
            this.btnLoadObj.Click += new System.EventHandler(this.btnLoadObj_Click);

            // 
            // btnSplitModel
            // 
            this.btnSplitModel.Location = new System.Drawing.Point(160, 20);
            this.btnSplitModel.Size = new System.Drawing.Size(120, 40);
            this.btnSplitModel.Text = "Chia 4 Phần";
            this.btnSplitModel.UseVisualStyleBackColor = true;
            this.btnSplitModel.Click += new System.EventHandler(this.btnSplitModel_Click);

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.btnLoadObj);
            this.Controls.Add(this.btnSplitModel);
            this.Name = "MainForm";
            this.Text = "OpenGL Viewer";
            this.ResumeLayout(false);
        }
    }
}
