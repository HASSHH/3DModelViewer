using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _3dModelViewer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private RotationAxisValues rotationAxis;
        private double rotationAngle;
        private string scaleFactor;
        private string translateXAfter;
        private string translateYAfter;
        private string translateZAfter;
        private string translateXBefore;
        private string translateYBefore;
        private string translateZBefore;
        private Color lightColor;
        private double lightAttenuation;
        private string lightPosX;
        private string lightPosY;
        private string lightPosZ;
        private string cameraPosX;
        private string cameraPosY;
        private string cameraPosZ;
        private string cameraLookAtX;
        private string cameraLookAtY;
        private string cameraLookAtZ;
        private Color floorColor;
        private string floorElevation;
        private bool floorEnabled;

        public event PropertyChangedEventHandler PropertyChanged;

        //rotation
        public RotationAxisValues RotationAxis
        {
            get => rotationAxis;
            set
            {
                rotationAxis = value;
                OnPropertyChanged("RotationAxis");
            }
        }
        public double RotationAngle
        {
            get => rotationAngle;
            set
            {
                rotationAngle = value;
                OnPropertyChanged("RotationAngle");
            }
        }

        //scale
        public string ScaleFactor
        {
            get => scaleFactor;
            set
            {
                scaleFactor = value;
                OnPropertyChanged("ScaleFactor");
            }
        }

        //translations
        public string TranslateXAfter
        {
            get => translateXAfter;
            set
            {
                translateXAfter = value;
                OnPropertyChanged("TranslateXAfter");
            }
        }
        public string TranslateYAfter
        {
            get => translateYAfter;
            set
            {
                translateYAfter = value;
                OnPropertyChanged("TranslateYAfter");
            }
        }
        public string TranslateZAfter
        {
            get => translateZAfter;
            set
            {
                translateZAfter = value;
                OnPropertyChanged("TranslateZAfter");
            }
        }
        public string TranslateXBefore
        {
            get => translateXBefore;
            set
            {
                translateXBefore = value;
                OnPropertyChanged("TranslateXBefore");
            }
        }
        public string TranslateYBefore
        {
            get => translateYBefore;
            set
            {
                translateYBefore = value;
                OnPropertyChanged("TranslateYBefore");
            }
        }
        public string TranslateZBefore
        {
            get => translateZBefore;
            set
            {
                translateZBefore = value;
                OnPropertyChanged("TranslateZBefore");
            }
        }

        //light
        public Color LightColor { get => lightColor; set { lightColor = value; OnPropertyChanged("LightColor"); } }
        public double LightAttenuation
        {
            get => lightAttenuation;
            set
            {
                if (value < 0)
                    lightAttenuation = 0;
                else if (value > 1)
                    lightAttenuation = 1;
                else
                    lightAttenuation = value;
                OnPropertyChanged("LightAttenuation");
            }
        }
        public string LightPosX { get => lightPosX; set { lightPosX = value; OnPropertyChanged("LightPosX"); } }
        public string LightPosY { get => lightPosY; set { lightPosY = value; OnPropertyChanged("LightPosY"); } }
        public string LightPosZ { get => lightPosZ; set { lightPosZ = value; OnPropertyChanged("LightPosZ"); } }

        //camera
        public string CameraPosX { get => cameraPosX; set { cameraPosX = value; OnPropertyChanged("CameraPosX"); } }
        public string CameraPosY { get => cameraPosY; set { cameraPosY = value; OnPropertyChanged("CameraPosY"); } }
        public string CameraPosZ { get => cameraPosZ; set { cameraPosZ = value; OnPropertyChanged("CameraPosZ"); } }
        public string CameraLookAtX { get => cameraLookAtX; set { cameraLookAtX = value; OnPropertyChanged("CameraLookAtX"); } }
        public string CameraLookAtY { get => cameraLookAtY; set { cameraLookAtY = value; OnPropertyChanged("CameraLookAtY"); } }
        public string CameraLookAtZ { get => cameraLookAtZ; set { cameraLookAtZ = value; OnPropertyChanged("CameraLookAtZ"); } }

        //floor
        public Color FloorColor { get => floorColor; set { floorColor = value; OnPropertyChanged("FloorColor"); } }
        public bool FloorEnabled { get => floorEnabled; set { floorEnabled = value; OnPropertyChanged("FloorEnabled"); } }
        public string FloorElevation { get => floorElevation; set { floorElevation = value; OnPropertyChanged("FloorElevation"); } }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum RotationAxisValues
    {
        X, Y, Z
    }
}
