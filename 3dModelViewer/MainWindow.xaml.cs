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
                    if(viewModel.SelectedModel != null)
                    {
                        float angle = (float)(Math.PI / 180 * vm.RotationAngle);
                        Matrix4 rotation;
                        if (vm.RotationAxis == RotationAxisValues.X)
                            rotation = Matrix4.CreateRotationX(angle);
                        else if(vm.RotationAxis == RotationAxisValues.Y)
                            rotation = Matrix4.CreateRotationY(angle);
                        else
                            rotation = Matrix4.CreateRotationZ(angle);
                        viewModel.SelectedModel.UserTransform.OnTheFlyRotation = rotation;
                    }
                    break;
                case "ScaleFactor":
                    if (viewModel.SelectedModel != null)
                    {
                        float scaleFactor;
                        if(float.TryParse(vm.ScaleFactor, out scaleFactor))
                        {
                            Matrix4 scale = Matrix4.CreateScale(scaleFactor);
                            viewModel.SelectedModel.UserTransform.ScaleMatrix = scale;
                        }
                    }
                    break;
                case "TranslateXAfter":
                case "TranslateYAfter":
                case "TranslateZAfter":
                    if (viewModel.SelectedModel != null)
                    {
                        float translateX, translateY, translateZ;
                        if (float.TryParse(vm.TranslateXAfter, out translateX) && float.TryParse(vm.TranslateYAfter, out translateY) && float.TryParse(vm.TranslateZAfter, out translateZ))
                        {
                            Matrix4 translate = Matrix4.CreateTranslation(translateX, translateY, translateZ);
                            viewModel.SelectedModel.UserTransform.TranslateAfterMatrix = translate;
                        }
                    }
                    break;
                case "TranslateXBefore":
                case "TranslateYBefore":
                case "TranslateZBefore":
                    if (viewModel.SelectedModel != null)
                    {
                        float translateX, translateY, translateZ;
                        if (float.TryParse(vm.TranslateXBefore, out translateX) && float.TryParse(vm.TranslateYBefore, out translateY) && float.TryParse(vm.TranslateZBefore, out translateZ))
                        {
                            Matrix4 translate = Matrix4.CreateTranslation(translateX, translateY, translateZ);
                            viewModel.SelectedModel.UserTransform.TranslateBeforeMatrix = translate;
                        }
                    }
                    break;
                case "LightColor":
                    if(scene != null)
                    {
                        Color color = viewModel.LightColor;
                        scene.Light.Color = new Vector4(color.ScR, color.ScG, color.ScB, color.ScA);
                    }
                    break;
                case "LightAttenuation":
                    if (scene != null)
                        scene.Light.Attenuation = (float)viewModel.LightAttenuation;
                    break;
                case "LightPosX":
                case "LightPosY":
                case "LightPosZ":
                    if (scene != null)
                    {
                        float posx, posy, posz;
                        if(float.TryParse(viewModel.LightPosX, out posx) && float.TryParse(viewModel.LightPosY, out posy) && float.TryParse(viewModel.LightPosZ, out posz))
                            scene.Light.Position = new Vector3(posx, posy, posz);
                    }
                    break;
                case "CameraPosX":
                case "CameraPosY":
                case "CameraPosZ":
                    if (scene != null)
                    {
                        float posx, posy, posz;
                        if (float.TryParse(viewModel.CameraPosX, out posx) && float.TryParse(viewModel.CameraPosY, out posy) && float.TryParse(viewModel.CameraPosZ, out posz))
                            scene.Camera.Position = new Vector3(posx, posy, posz);
                    }
                    break;
                case "CameraLookAtX":
                case "CameraLookAtY":
                case "CameraLookAtZ":
                    if (scene != null)
                    {
                        float posx, posy, posz;
                        if (float.TryParse(viewModel.CameraLookAtX, out posx) && float.TryParse(viewModel.CameraLookAtY, out posy) && float.TryParse(viewModel.CameraLookAtZ, out posz))
                            scene.Camera.LookAt = new Vector3(posx, posy, posz);
                    }
                    break;
                case "FloorEnabled":
                    if (scene != null)
                        scene.DrawFloorEnabled = viewModel.FloorEnabled;
                    break;
                case "FloorColor":
                    if (scene != null)
                        scene.Floor.SetColor(viewModel.FloorColor);
                    break;
                case "FloorElevation":
                    if (scene != null)
                    {
                        double elevation;
                        if (double.TryParse(viewModel.FloorElevation, out elevation))
                            scene.Floor.SetHeight(elevation);
                    }
                    break;
                default: break;
            }
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

        private void ApplyRotationButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedModel != null)
            {
                float angle = (float)(Math.PI / 180 * viewModel.RotationAngle);
                Matrix4 rotation;
                if (viewModel.RotationAxis == RotationAxisValues.X)
                    rotation = Matrix4.CreateRotationX(angle);
                else if (viewModel.RotationAxis == RotationAxisValues.Y)
                    rotation = Matrix4.CreateRotationY(angle);
                else
                    rotation = Matrix4.CreateRotationZ(angle);
                viewModel.SelectedModel.UserTransform.RotationMatrixList.Push(rotation);
                viewModel.SelectedModel.UserTransform.OnTheFlyRotation = Matrix4.Identity;
                viewModel.RotationAngle = 0;
            }
        }
        private void UndoRotationButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedModel != null)
            {
                LoadedModel.ModelTransform mt = viewModel.SelectedModel.UserTransform;
                if (mt.RotationMatrixList.Count > 0)
                    mt.RotationMatrixList.Pop();
                mt.OnTheFlyRotation = Matrix4.Identity;
                viewModel.RotationAngle = 0;
            }
        }
        private void AutoCenterButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedModel != null)
            {
                LoadedModel selectedModel = viewModel.SelectedModel;
                //scale adjusting
                float maxSize = 3;
                Vector3 delta = selectedModel.MaximumPosition - selectedModel.MinimumPosition;
                float maxDelta = delta.X;
                if (delta.Y > maxDelta)
                    maxDelta = delta.Y;
                if (delta.Z > maxDelta)
                    maxDelta = delta.Z;
                float scale = maxSize / maxDelta;
                Matrix4 scaleMatrix = Matrix4.CreateScale(scale);
                selectedModel.UserTransform.ScaleMatrix = scaleMatrix;
                viewModel.ScaleFactor = scale.ToString("N8");
                //centering
                Vector3 centerOffset = (selectedModel.MaximumPosition + selectedModel.MinimumPosition) / 2;
                centerOffset = new Vector3(scaleMatrix*(new Vector4(centerOffset,1f)));
                Matrix4 translateBeforeMatrix = Matrix4.CreateTranslation(-centerOffset.X, -centerOffset.Y, -centerOffset.Z);
                selectedModel.UserTransform.TranslateBeforeMatrix = translateBeforeMatrix;
                viewModel.TranslateXBefore = (-centerOffset.X).ToString("N4");
                viewModel.TranslateYBefore = (-centerOffset.Y).ToString("N4");
                viewModel.TranslateZBefore = (-centerOffset.Z).ToString("N4");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as System.Windows.Controls.TextBox).GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
        }

        private void LightColorRectangle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChooseColorWindow ccw = new ChooseColorWindow();
            if (ccw.ShowDialog().GetValueOrDefault())
            {
                Color color = (ccw.DataContext as ChooseColorViewModel).GetColor();
                viewModel.LightColor = color;
            }
        }

        private void FloorColorRectangle_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChooseColorWindow ccw = new ChooseColorWindow();
            if (ccw.ShowDialog().GetValueOrDefault())
            {
                Color color = (ccw.DataContext as ChooseColorViewModel).GetColor();
                viewModel.FloorColor = color;
            }
        }

        private void AddModelButton_Click(object sender, RoutedEventArgs e)
        {
            LoadedModel model = LoadModelOfd();
            if (model != null)
            {
                viewModel.LoadedModels.Add(model);
                viewModel.ModelTransforms.Add(model, new ModelTransform());
                viewModel.SelectedModel = model;
                scene.LoadedModels.Add(model);
            }
        }

        private LoadedModel LoadModelOfd()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ofd.CheckFileExists)
                {
                    string dirName = Path.GetDirectoryName(ofd.FileName);
                    try
                    {
                        using (Assimp.AssimpContext importer = new Assimp.AssimpContext())
                        {
                            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
                            Assimp.Scene model = importer.ImportFile(ofd.FileName, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality);
                            LoadedModel loadedModel = new LoadedModel(model, dirName);
                            loadedModel.Name = Path.GetFileName(ofd.FileName);
                            return loadedModel;
                        }
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("Unsupported file type.", "Error");
                    }
                }
            }
            return null;
        }

        private void RemoveModelButton_Click(object sender, RoutedEventArgs e)
        {
            if(viewModel.SelectedModel != null)
            {
                LoadedModel toDelete = viewModel.SelectedModel;
                viewModel.LoadedModels.Remove(toDelete);
                viewModel.SelectedModel = viewModel.LoadedModels.LastOrDefault();
                scene.LoadedModels.Remove(toDelete);
                toDelete.Dispose();
            }
        }
    }
}
