using OpenGL_Viewer.Models;
using OpenGL_Viewer.OpenGL;

namespace OpenGL_Viewer.Forms
{
    public partial class MainForm : Form
    {
        private Model3D _model;
        private GLControlForm _viewer;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnLoadObj_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "OBJ Files (*.obj)|*.obj",
                Title = "Chọn file OBJ"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    _model = ObjLoader.LoadFromFile(filePath);

                    if (_viewer != null)
                    {
                        _viewer.Close();
                        _viewer.Dispose();
                        _viewer = null;
                    }

                    _viewer = new GLControlForm(_model);
                    _viewer.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tải file OBJ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSplitModel_Click(object sender, EventArgs e)
        {
            if (_model == null)
            {
                MessageBox.Show("Vui lòng tải file OBJ trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SplitModels");
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                ModelSplitter.SplitModel(_model, saveDirectory);
                MessageBox.Show("Tách mô hình thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tách mô hình: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
