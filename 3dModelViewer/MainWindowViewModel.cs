using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private RotationAxisValues rotationAxis;
        private double rotationAngle;
        private string scaleFactor;
        private string translateX;
        private string translateY;
        private string translateZ;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public string ScaleFactor
        {
            get => scaleFactor;
            set
            {
                scaleFactor = value;
                OnPropertyChanged("ScaleFactor");
            }
        }

        public string TranslateX
        {
            get => translateX;
            set
            {
                translateX = value;
                OnPropertyChanged("TranslateX");
            }
        }

        public string TranslateY
        {
            get => translateY;
            set
            {
                translateY = value;
                OnPropertyChanged("TranslateY");
            }
        }

        public string TranslateZ
        {
            get => translateZ;
            set
            {
                translateZ = value;
                OnPropertyChanged("TranslateZ");
            }
        }

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
