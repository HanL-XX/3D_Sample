using OpenTK.Graphics.OpenGL;
using OpenGL_Viewer.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;
using OpenTK.GLControl;

namespace OpenGL_Viewer.OpenGL
{
    public partial class GLControlForm : Form
    {
        private GLControl glControl;
        private Model3D model;
        private int _vao, _vbo, _ebo;
        private Shader? _shader;

        private bool isRotating = false;
        private bool isPanning = false;
        private Point lastMousePos;
        private float rotationX = 0f, rotationY = 0f;
        private float translateX = 0f, translateY = 0f;
        private float zoom = 1f;

        public GLControlForm(Model3D model)
        {
            InitializeComponent();
            this.model = model;

            glControl = new GLControl();
            glControl.Dock = DockStyle.Fill;
            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            glControl.MouseDown += GlControl_MouseDown;
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseUp += GlControl_MouseUp;
            glControl.MouseWheel += GlControl_MouseWheel;

            this.Controls.Add(glControl);
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            _shader = new Shader("Shaders/vertex_shader.glsl", "Shaders/fragment_shader.glsl");
            _shader.Use();

            GenerateBuffers();
            GlControl_Resize(null, null);
        }

        private void GenerateBuffers()
        {
            List<float> vertexList = new List<float>();
            List<int> indexList = new List<int>();

            Vector3 center = new Vector3(
                model.Vertices.Average(v => v.X),
                model.Vertices.Average(v => v.Y),
                model.Vertices.Average(v => v.Z)
            );
            foreach (var face in model.Faces)
            {
                foreach (var index in face.Vertices)
                {
                    Vector3 v = model.Vertices[index] - center;
                    vertexList.AddRange(new float[] { v.X, v.Y, v.Z });
                    indexList.Add(indexList.Count);
                }
            }

            float[] vertices = vertexList.ToArray();
            int[] indices = indexList.ToArray();

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            glControl.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _shader?.Use();

            Matrix4 modelMatrix = Matrix4.CreateScale(zoom) *
                                  Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotationY)) *
                                  Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotationX)) *
                                  Matrix4.CreateTranslation(translateX, translateY, 0);

            _shader?.SetMatrix4("model", modelMatrix);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, model.Faces.Count * 3, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            glControl.SwapBuffers();
        }

        private void GlControl_Resize(object sender, EventArgs? e)
        {
            if (_shader == null) return;

            glControl.MakeCurrent();

            int width = glControl.Width;
            int height = glControl.Height;

            GL.Viewport(0, 0, width, height);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f),
                width / (float)height,
                0.1f,
                100.0f
            );

            Vector3 cameraPos = new Vector3(0, 0, model.Vertices.Max(v => v.Z) + 3); // Lùi ra xa theo kích thước mô hình
            Matrix4 view = Matrix4.LookAt(
                cameraPos,
                Vector3.Zero,
                Vector3.UnitY
            );

            _shader.Use();
            _shader.SetMatrix4("projection", projection);
            _shader.SetMatrix4("view", view);
        }

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePos = e.Location;
            isRotating = e.Button == MouseButtons.Left;
            isPanning = e.Button == MouseButtons.Right;
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isRotating)
            {
                rotationX += (e.Y - lastMousePos.Y) * 0.5f;
                rotationY += (e.X - lastMousePos.X) * 0.5f;
            }
            else if (isPanning)
            {
                translateX += (e.X - lastMousePos.X) * 0.01f;
                translateY -= (e.Y - lastMousePos.Y) * 0.01f;
            }
            lastMousePos = e.Location;
            glControl.Invalidate();
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            isRotating = false;
            isPanning = false;
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            zoom = MathHelper.Clamp(zoom + (e.Delta > 0 ? 0.1f : -0.1f), 0.05f, 5f);
            glControl.Invalidate();
        }
    }
}
