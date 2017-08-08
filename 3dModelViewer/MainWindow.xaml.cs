using _3dModelViewer.Graphics;
using Assimp.Configs;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace _3dModelViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GLControl glc;
        private bool initDone = false;
        private Graphics.Scene scene;
        private Timer loopTimer;
        private MainWindowViewModel viewModel;
        private LoadedModel loadedModel;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGlComponent();
            InitializeLoopTimer();

            viewModel = DataContext as MainWindowViewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MainWindowViewModel vm = sender as MainWindowViewModel;
            switch (e.PropertyName)
            {
                case "RotationAxis":
                case "RotationAngle":
                    if(loadedModel != null)
                    {
                        float angle = (float)(Math.PI / 180 * vm.RotationAngle);
                        Matrix4 rotation;
                        if (vm.RotationAxis == RotationAxisValues.X)
                            rotation = Matrix4.CreateRotationX(angle);
                        else if(vm.RotationAxis == RotationAxisValues.Y)
                            rotation = Matrix4.CreateRotationY(angle);
                        else
                            rotation = Matrix4.CreateRotationZ(angle);
                        loadedModel.UserTransform.OnTheFlyRotation = rotation;
                    }
                    break;
                case "ScaleFactor":
                    if (loadedModel != null)
                    {
                        float scaleFactor;
                        if(float.TryParse(vm.ScaleFactor, out scaleFactor))
                        {
                            Matrix4 scale = Matrix4.CreateScale(scaleFactor);
                            loadedModel.UserTransform.ScaleMatrix = scale;
                        }
                    }
                    break;
                case "TranslateXAfter":
                case "TranslateYAfter":
                case "TranslateZAfter":
                    if (loadedModel != null)
                    {
                        float translateX, translateY, translateZ;
                        if (float.TryParse(vm.TranslateXAfter, out translateX) && float.TryParse(vm.TranslateYAfter, out translateY) && float.TryParse(vm.TranslateZAfter, out translateZ))
                        {
                            Matrix4 translate = Matrix4.CreateTranslation(translateX, translateY, translateZ);
                            loadedModel.UserTransform.TranslateAfterMatrix = translate;
                        }
                    }
                    break;
                case "TranslateXBefore":
                case "TranslateYBefore":
                case "TranslateZBefore":
                    if (loadedModel != null)
                    {
                        float translateX, translateY, translateZ;
                        if (float.TryParse(vm.TranslateXBefore, out translateX) && float.TryParse(vm.TranslateYBefore, out translateY) && float.TryParse(vm.TranslateZBefore, out translateZ))
                        {
                            Matrix4 translate = Matrix4.CreateTranslation(translateX, translateY, translateZ);
                            loadedModel.UserTransform.TranslateBeforeMatrix = translate;
                        }
                    }
                    break;
                default: break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateObjects(scene);
        }

        private void InitializeGlComponent()
        {
            scene = new Graphics.Scene();
            glc = new GLControl();
            Toolkit.Init();
            glc.Paint += Draw;
            glc.Resize += Resize;
            WfHost.Child = glc;
            glc.CreateControl();
        }

        private void InitializeLoopTimer()
        {
            loopTimer = new Timer { Interval = 1000 / 60 };
            loopTimer.Tick += (a, b) => glc.Invalidate();
            loopTimer.Start();
        }

        private void Resize(object sender, EventArgs e)
        {
            scene.Resize(glc.Width, glc.Height);
        }

        private void Draw(object sender, PaintEventArgs e)
        {
            if (!initDone)
            {
                scene.Init(glc.Width, glc.Height);
                initDone = true;
            }
            scene.Draw();
            glc.SwapBuffers();
        }

        private void CreateObjects(Scene scene)
        {
            LoadedModel model = LoadModelOfd();
            if (model != null)
            {
                loadedModel = model;
                scene.LoadedModels.Add(model);
            }
        }

        private LoadedModel LoadModelOfd()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.CheckFileExists)
                {
                    string dirName = Path.GetDirectoryName(ofd.FileName);
                    using (Assimp.AssimpContext importer = new Assimp.AssimpContext())
                    {
                        importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
                        Assimp.Scene model = importer.ImportFile(ofd.FileName, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality);
                        return new LoadedModel(model, dirName);
                    }
                }
            }
            return null;
        }

        private void ApplyRotationButton_Click(object sender, RoutedEventArgs e)
        {
            if (loadedModel != null)
            {
                float angle = (float)(Math.PI / 180 * viewModel.RotationAngle);
                Matrix4 rotation;
                if (viewModel.RotationAxis == RotationAxisValues.X)
                    rotation = Matrix4.CreateRotationX(angle);
                else if (viewModel.RotationAxis == RotationAxisValues.Y)
                    rotation = Matrix4.CreateRotationY(angle);
                else
                    rotation = Matrix4.CreateRotationZ(angle);
                loadedModel.UserTransform.RotationMatrixList.Push(rotation);
                loadedModel.UserTransform.OnTheFlyRotation = Matrix4.Identity;
                viewModel.RotationAngle = 0;
            }
        }
        private void UndoRotationButton_Click(object sender, RoutedEventArgs e)
        {
            if (loadedModel != null)
            {
                LoadedModel.ModelTransform mt = loadedModel.UserTransform;
                if (mt.RotationMatrixList.Count > 0)
                    mt.RotationMatrixList.Pop();
                mt.OnTheFlyRotation = Matrix4.Identity;
                viewModel.RotationAngle = 0;
            }
        }
        private void AutoCenterButton_Click(object sender, RoutedEventArgs e)
        {
            if (loadedModel != null)
            {
                //scale adjusting
                float maxSize = 3;
                Vector3 delta = loadedModel.MaximumPosition - loadedModel.MinimumPosition;
                float maxDelta = delta.X;
                if (delta.Y > maxDelta)
                    maxDelta = delta.Y;
                if (delta.Z > maxDelta)
                    maxDelta = delta.Z;
                float scale = maxSize / maxDelta;
                Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
                loadedModel.UserTransform.ScaleMatrix = scaleMatrix;
                viewModel.ScaleFactor = scale.ToString("N8");
                //centering
                Vector3 centerOffset = (loadedModel.MaximumPosition + loadedModel.MinimumPosition) / 2;
                centerOffset = new Vector3(scaleMatrix*(new Vector4(centerOffset,1f)));
                Matrix4 translateBeforeMatrix = Matrix4.CreateTranslation(-centerOffset.X, -centerOffset.Y, -centerOffset.Z);
                loadedModel.UserTransform.TranslateBeforeMatrix = translateBeforeMatrix;
                viewModel.TranslateXBefore = (-centerOffset.X).ToString("N4");
                viewModel.TranslateYBefore = (-centerOffset.Y).ToString("N4");
                viewModel.TranslateZBefore = (-centerOffset.Z).ToString("N4");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as System.Windows.Controls.TextBox).GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        }

        private void Rectangle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChooseColorWindow ccw = new ChooseColorWindow();
            if (ccw.ShowDialog().GetValueOrDefault())
            {
                Color color = (ccw.DataContext as ChooseColorViewModel).GetColor();
                (sender as System.Windows.Shapes.Rectangle).Fill = new SolidColorBrush(color);
            }
        }
    }
}
