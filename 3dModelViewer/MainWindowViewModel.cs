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
        private string translateXAfter;
        private string translateYAfter;
        private string translateZAfter;
        private string translateXBefore;
        private string translateYBefore;
        private string translateZBefore;

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
