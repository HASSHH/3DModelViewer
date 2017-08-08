using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public class Light : INotifyPropertyChanged
    {
        private Vector3 position;
        private Vector4 color;
        private float attenuation;

        public event PropertyChangedEventHandler PropertyChanged;

        public Vector3 Position { get => position; set { position = value; OnPropertyChanged("Position"); } }
        public Vector4 Color { get => color; set { color = value; OnPropertyChanged("Color"); } }
        public float Attenuation { get => attenuation; set { attenuation = value; OnPropertyChanged("Attenuation"); } }
        public bool Active { get; set; } = true;

        private void OnPropertyChanged(string propertyName)
        {
            if(Active)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
