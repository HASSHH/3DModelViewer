using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public class Light
    {
        public Vector3 Position { get; set; }
        public Vector4 Color { get; set; }
        public float Attenuation { get; set; }
    }
}
