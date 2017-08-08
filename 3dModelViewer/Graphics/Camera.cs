using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public class Camera : INotifyPropertyChanged
    {
        private Vector3 position;
        private Vector3 lookAt;
        private Vector3 cameraUp;
        private float fieldOfViewY;
        private float aspectRatio;

        public Camera()
        {
            InitValues();
            ComputeMatrices();
        }
        public Camera(Vector3 position, Vector3 lookAt) : this()
        {
            InitValues();
            this.position = position;
            this.lookAt = lookAt;
            ComputeMatrices();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Matrix4 ProjectionMatrix { get; private set; }
        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ViewProjectionMatrix { get; private set; }
        public Vector3 Position { get => position; set { position = value; ComputeMatrices(); } }
        public Vector3 LookAt { get => lookAt; set { lookAt = value; ComputeMatrices(); } }
        public Vector3 CameraUp { get => cameraUp; set { cameraUp = value; ComputeMatrices(); } }
        /// <summary>
        /// Vertical FOV in degrees.
        /// </summary>
        public float FieldOfViewY { get => fieldOfViewY; set { fieldOfViewY = value; ComputeMatrices(); } }
        public float AspectRatio { get => aspectRatio; set { aspectRatio = value; ComputeMatrices(); } }
        public bool Active { get; set; }

        public void ComputeMatrices()
        {
            //projection matrix
            float fovRads = (float)(FieldOfViewY / 180 * Math.PI);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(fovRads, aspectRatio, 0.1f, 100f);
            //view matrix
            ViewMatrix = Matrix4.LookAt(Position, LookAt, CameraUp);
            //vp
            ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
            if(Active)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewProjectionMatrix"));
        }

        private void InitValues()
        {
            cameraUp = Vector3.UnitY;
            fieldOfViewY = 45;
            aspectRatio = 1;
            Active = true;
        }
    }
}
