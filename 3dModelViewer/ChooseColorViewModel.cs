using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace _3dModelViewer
{
    public class ChooseColorViewModel : INotifyPropertyChanged
    {
        private double hue;
        private double saturation;
        private double value;
        private Brush brush;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Hue { get => hue; set { hue = value; OnPropertyChanged("Hue"); SetNewBrush(); } }

        public double Saturation { get => saturation; set { saturation = value; OnPropertyChanged("Saturation"); SetNewBrush(); } }
        public double Value { get => value; set { this.value = value; OnPropertyChanged("Value"); SetNewBrush(); } }

        public Brush Brush { get => brush; set { brush = value; OnPropertyChanged("Brush"); } }

        public Color GetColor()
        {
            return HSVtoRGB((float)Hue, (float)Saturation, (float)Value);
        }

        /// <summary>
        /// Returns RGB color from HSV values
        /// </summary>
        /// <param name="H">hue - 0 to 360</param>
        /// <param name="S">saturation - 0 to 1</param>
        /// <param name="V">value - 0 to 1</param>
        /// <returns></returns>
        public static Color HSVtoRGB(float H, float S, float V)
        {
            float C = V * S;
            float Hp = H / 60;
            float X = C * (1 - Math.Abs((Hp % 2) - 1));
            float Rp, Gp, Bp;
            if (Hp >= 5)
            {
                Rp = C;
                Gp = 0;
                Bp = X;
            }
            else if (Hp >= 4)
            {
                Rp = X;
                Gp = 0;
                Bp = C;
            }
            else if (Hp >= 3)
            {
                Rp = 0;
                Gp = X;
                Bp = C;
            }
            else if (Hp >= 2)
            {
                Rp = 0;
                Gp = C;
                Bp = X;
            }
            else if (Hp >= 1)
            {
                Rp = X;
                Gp = C;
                Bp = 0;
            }
            else
            {
                Rp = C;
                Gp = X;
                Bp = 0;
            }
            float m = V - C;
            float R, G, B;
            R = Rp + m;
            G = Gp + m;
            B = Bp + m;
            return new Color { ScR = R, ScG = G, ScB = B, ScA = 1f };
        }
        
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetNewBrush()
        {
            Brush = new SolidColorBrush(GetColor());
        }
    }
}
