using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public class LoadedNode : IDisposable
    {
        public readonly List<VaoDescription> Meshes = new List<VaoDescription>();
        public readonly List<LoadedNode> Children = new List<LoadedNode>();
        
        public Matrix4 Transform { get; set; }
        public LoadedNode Parent { get; set; }

        public void Dispose()
        {
            foreach (VaoDescription mesh in Meshes)
                GL.DeleteBuffer(mesh.VaoHandler);
            foreach (LoadedNode child in Children)
                child.Dispose();
        }
    }

    public class VaoDescription
    {
        public int VaoHandler { get; set; }
        public int CorrespondingMeshIndex { get; set; }
    }
}
