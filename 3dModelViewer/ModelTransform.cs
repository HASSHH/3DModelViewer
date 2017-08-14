using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dModelViewer
{
    public class ModelTransform
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

        public ModelTransform()
        {
            rotationAxis = RotationAxisValues.X;
            rotationAngle = 0;
            scaleFactor = "1";
            translateXAfter = "0";
            translateYAfter = "0";
            translateZAfter = "0";
            translateXBefore = "0";
            translateYBefore = "0";
            translateZBefore = "0";
        }

        public RotationAxisValues RotationAxis { get => rotationAxis; set => rotationAxis = value; }
        public double RotationAngle { get => rotationAngle; set => rotationAngle = value; }
        public string ScaleFactor { get => scaleFactor; set => scaleFactor = value; }
        public string TranslateXAfter { get => translateXAfter; set => translateXAfter = value; }
        public string TranslateYAfter { get => translateYAfter; set => translateYAfter = value; }
        public string TranslateZAfter { get => translateZAfter; set => translateZAfter = value; }
        public string TranslateXBefore { get => translateXBefore; set => translateXBefore = value; }
        public string TranslateYBefore { get => translateYBefore; set => translateYBefore = value; }
        public string TranslateZBefore { get => translateZBefore; set => translateZBefore = value; }
    }
}
