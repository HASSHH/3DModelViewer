using Assimp;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer.Graphics
{
    public static class AssimpToOpenTKConverter
    {
        public static Vector3 Vector3DToVector3(Vector3D source)
        {
            return new Vector3(source.X, source.Y, source.Z);
        }

        public static Vector2 Vector2DToVector2(Vector2D source)
        {
            return new Vector2(source.X, source.Y);
        }

        public static Vector4 Color4DToVector4(Color4D source)
        {
            return new Vector4(source.R, source.G, source.B, source.A);
        }

        public static Matrix4 Matrix4x4ToMatrix4(Matrix4x4 source)
        {
            Matrix4 result = new Matrix4();
            result.M11 = source.A1;
            result.M12 = source.A2;
            result.M13 = source.A3;
            result.M14 = source.A4;
            result.M21 = source.B1;
            result.M22 = source.B2;
            result.M23 = source.B3;
            result.M24 = source.B4;
            result.M31 = source.C1;
            result.M32 = source.C2;
            result.M33 = source.C3;
            result.M34 = source.C4;
            result.M41 = source.D1;
            result.M42 = source.D2;
            result.M43 = source.D3;
            result.M44 = source.D4;
            return result;
        }
    }
}
