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
        private const float fieldOfViewY = 45f;
        private const float FreeCameraPositionUnit = 0.1f;
        /// <summary>
        /// The unit angle is 1/100th part of the vertical FOV.
        /// </summary>
        private const float FreeCameraAngleUnit = (float)Math.PI * fieldOfViewY / 180 / 100;
        private const float FreeCameraXAngleBarrier = 0.0001f;

        private Vector3 position;
        private Vector3 lookAt;
        private Vector3 cameraUp;
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
        public float FieldOfViewY { get => fieldOfViewY; }
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
            if (Active)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ViewProjectionMatrix"));
        }

        public void FreeCameraMoveUpDown(bool up = true)
        {
            Vector3 change = new Vector3(0f, up ? FreeCameraPositionUnit : -FreeCameraPositionUnit, 0f);
            Position = Vector3.Add(Position, change);
            LookAt = Vector3.Add(LookAt, change);
        }

        public void FreeCameraMoveForwardBackward(bool forward = true)
        {
            float unit = forward ? FreeCameraPositionUnit : -FreeCameraPositionUnit;
            Vector3 directionVector = LookAt - Position;
            directionVector.Normalize();
            directionVector = Vector3.Multiply(directionVector, unit);
            Position = Vector3.Add(Position, directionVector);
            LookAt = Vector3.Add(LookAt, directionVector);
        }

        public void FreeCameraMoveLeftRight(bool left)
        {
            float unit = left ? FreeCameraPositionUnit : -FreeCameraPositionUnit;
            Vector3 directionVector = LookAt - Position;
            //remove the x rotation (up/down)
            GetFreeCameraXY(out float yAngle, out float xAngle);
            //undo y rotation
            Vector4 resultVector = (new Vector4(directionVector, 0f)) * Matrix4.CreateRotationY(-yAngle);
            //undo x rotation
            resultVector = resultVector * Matrix4.CreateRotationX(-xAngle);
            //redo y rotation + 90 degrees (left turn)
            resultVector = resultVector * Matrix4.CreateRotationY(yAngle + (float)Math.PI / 2f);

            resultVector.Normalize();
            resultVector = Vector4.Multiply(resultVector, unit);
            Vector3 change = new Vector3(resultVector);
            Position = Vector3.Add(Position, change);
            LookAt = Vector3.Add(LookAt, change);
        }

        public void FreeCameraRotate(float yUnits, float xUnits)
        {
            float floatPi = (float)Math.PI;
            GetFreeCameraXY(out float yAngle, out float xAngle);
            Vector3 directionVector = LookAt - Position;
            //undo y rotation
            Vector4 resultVector = (new Vector4(directionVector, 0f)) * Matrix4.CreateRotationY(-yAngle);
            //undo x rotation
            resultVector = resultVector * Matrix4.CreateRotationX(-xAngle);
            float tempXAngle = (xAngle + xUnits * FreeCameraAngleUnit) % (2f * floatPi);
            if (tempXAngle >= floatPi / 2f && tempXAngle <= floatPi * 3f / 2f)
                if (xAngle <= floatPi / 2f)
                    xAngle = floatPi / 2f - FreeCameraXAngleBarrier;
                else
                    xAngle = floatPi * 3f / 2f + FreeCameraXAngleBarrier;
            else
                xAngle = tempXAngle;
            //do the full x rotation
            resultVector = resultVector * Matrix4.CreateRotationX(xAngle);
            //do the full y rotation
            yAngle = (yAngle + yUnits * FreeCameraAngleUnit) % (2f * floatPi);
            resultVector = resultVector * Matrix4.CreateRotationY(yAngle);
            //change LookAt vector
            LookAt = Position + new Vector3(resultVector);
        }

        /// <summary>
        /// Gets the rotation angles in radians for x and y axis. Order: xy
        /// The y = 0 and x = 0 values represent a camera that is lookind down the "-z" axis with camera up on y axis.
        /// </summary>
        /// <param name="yAngle"></param>
        /// <param name="xAngle"></param>
        public void GetFreeCameraXY(out float yAngle, out float xAngle)
        {
            yAngle = GetFreeCameraYAngle();
            xAngle = GetFreeCameraXAngle(yAngle);
        }

        private float GetFreeCameraYAngle()
        {
            float floatPi = (float)Math.PI;
            Vector3 directionVector = LookAt - Position;
            if (directionVector.Z == 0f)
                if (directionVector.X > 0f)
                    return floatPi * 3f / 2f;
                else
                    return floatPi / 2f;
            if (directionVector.X == 0f)
                if (directionVector.Z > 0f)
                    return floatPi;
                else
                    return 0f;
            float tanY = directionVector.X / directionVector.Z;
            float angleY = (float)Math.Atan(tanY);
            if (directionVector.Z > 0)
                angleY += floatPi;
            return angleY;
        }

        private float GetFreeCameraXAngle(float yAngle)
        {
            float floatPi = (float)Math.PI;
            Vector3 directionVector = LookAt - Position;
            Matrix4 reverseYRotationMatrix = Matrix4.CreateRotationY(-yAngle);
            Vector3 zeroXVector = new Vector3(new Vector4(directionVector, 0f) * reverseYRotationMatrix);
            if (zeroXVector.Z == 0f)
                if (zeroXVector.Y > 0f)
                    return floatPi / 2f;
                else
                    return floatPi * 3f / 2f;
            if (zeroXVector.Y == 0f)
                if (zeroXVector.Z > 0f)
                    return floatPi;
                else
                    return 0;
            float tanX = -zeroXVector.Y / zeroXVector.Z;
            float angleX = (float)Math.Atan(tanX);
            if (zeroXVector.Z > 0)
                angleX += floatPi;
            return angleX;
        }

        private void InitValues()
        {
            cameraUp = Vector3.UnitY;
            aspectRatio = 1f;
            Active = true;
        }
    }
}
