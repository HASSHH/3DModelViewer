using _3dModelViewer.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly ObservableCollection<LoadedModel> loadedModels = new ObservableCollection<LoadedModel>();
        private LoadedModel selectedModel;
        private readonly Dictionary<LoadedModel, ModelTransform> modelTransforms = new Dictionary<LoadedModel, ModelTransform>();

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

        public ObservableCollection<LoadedModel> LoadedModels => loadedModels;
        public LoadedModel SelectedModel { get => selectedModel; set { selectedModel = value; UpdateTransformValues(value); OnPropertyChanged("SelectedModel"); } }
        public Dictionary<LoadedModel, ModelTransform> ModelTransforms => modelTransforms;

        private void OnPropertyChanged(string propertyName)
        {
            if(selectedModel != null)
            {
                ModelTransform transform = ModelTransforms[selectedModel];
                if (transform != null)
                {
                    switch (propertyName)
                    {
                        case "RotationAxis":
                            transform.RotationAxis = RotationAxis;
                            break;
                        case "RotationAngle":
                            transform.RotationAngle = RotationAngle;
                            break;
                        case "ScaleFactor":
                            transform.ScaleFactor = ScaleFactor;
                            break;
                        case "TranslateXAfter":
                            transform.TranslateXAfter = TranslateXAfter;
                            break;
                        case "TranslateYAfter":
                            transform.TranslateYAfter = TranslateYAfter;
                            break;
                        case "TranslateZAfter":
                            transform.TranslateZAfter = TranslateZAfter;
                            break;
                        case "TranslateXBefore":
                            transform.TranslateXBefore = TranslateXBefore;
                            break;
                        case "TranslateYBefore":
                            transform.TranslateYBefore = TranslateYBefore;
                            break;
                        case "TranslateZBefore":
                            transform.TranslateZBefore = TranslateZBefore;
                            break;
                        default: break;
                    }
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateTransformValues(LoadedModel model)
        {
            ModelTransform transform;
            if (model != null)
                transform = ModelTransforms[model];
            else
                transform = new ModelTransform();
            RotationAxis = transform.RotationAxis;
            RotationAngle = transform.RotationAngle;
            ScaleFactor = transform.ScaleFactor;
            TranslateXAfter = transform.TranslateXAfter;
            TranslateYAfter = transform.TranslateYAfter;
            TranslateZAfter = transform.TranslateZAfter;
            TranslateXBefore = transform.TranslateXBefore;
            TranslateYBefore = transform.TranslateYBefore;
            TranslateZBefore = transform.TranslateZBefore;
        }
    }

    public enum RotationAxisValues
    {
        X, Y, Z
    }
}
