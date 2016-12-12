using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System.Windows.Media.Media3D;

namespace KinectGraphics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        KinectSensor sensor = null;
        PerspectiveCamera camera = null;
        BodyFrameReader bodyReader = null;

        HighDefinitionFaceFrameSource faceSource = null;
        HighDefinitionFaceFrameReader faceFrameReader = null;
        IList<Body> bodies = null;

        Point3D initNose = new Point3D(0, 0, 0);

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                this.sensor = KinectSensor.GetDefault();
                if (this.sensor == null)
                    return;
                sensor.Open();

                bodies = new Body[sensor.BodyFrameSource.BodyCount];
                bodyReader = sensor.BodyFrameSource.OpenReader();
                bodyReader.FrameArrived += BodyReader_FrameArrived;

                /*this.faceSource = new FaceFrameSource(sensor, 10, FaceFrameFeatures.BoundingBoxInColorSpace | FaceFrameFeatures.RotationOrientation |
                    FaceFrameFeatures.FaceEngagement | FaceFrameFeatures.Glasses | FaceFrameFeatures.Happy | FaceFrameFeatures.LeftEyeClosed | FaceFrameFeatures.MouthOpen |
                    FaceFrameFeatures.PointsInColorSpace | FaceFrameFeatures.RightEyeClosed);
                    */

                this.faceSource = new HighDefinitionFaceFrameSource(sensor);

                this.faceFrameReader = faceSource.OpenReader();
                this.faceFrameReader.FrameArrived += FaceFrameReader_FrameArrived;

                // declare scene objects
                // this.viewport3d;
                var model3DGroup = new Model3DGroup();
                var modelVisual3D = new ModelVisual3D();

                // define camera
                camera = new PerspectiveCamera()
                {
                    Position = new Point3D(0, 0, 10),
                    LookDirection = new Vector3D(0, 0, -1),
                    FieldOfView = 60
                };
                this.viewport3d.Camera = camera;

                // scene needs light
                var directionalLight = new DirectionalLight()
                {
                    Color = Colors.White,
                    Direction = new Vector3D(-0.61, -0.5, -0.61)
                };

                model3DGroup.Children.Add(directionalLight);

                /*
                var cube = Cube();

                var rotateTransform = new RotateTransform3D();
                axisAngleRotation = new AxisAngleRotation3D()
                {
                    Axis = new Vector3D(0, 3, 0),
                    Angle = 30
                };
                rotateTransform.Rotation = axisAngleRotation;
                cube.Transform = rotateTransform;
                */

                var rand = new Random();
                int cubeCount = 1000;

                /*
                int width = 200;
                for(var i = 0; i < cubeCount; i++)
                {
                    var cube = Cube();
                    var slideTransform = new TranslateTransform3D();

                    slideTransform.OffsetX = ((i % width) - width/2) * 2;
                    slideTransform.OffsetY = ((i / width) - (cubeCount / (2 * width))) * 2;
                    slideTransform.OffsetZ = rand.Next(-20, 20);
                    cube.Transform = slideTransform;

                    model3DGroup.Children.Add(cube);
                }
                */

                for(var i = 0; i < cubeCount; i++)
                {
                    var cube = Cube();
                    var slideTransform = new TranslateTransform3D();

                    slideTransform.OffsetX = rand.Next(-50, 50);
                    slideTransform.OffsetY = rand.Next(-50, 50);
                    slideTransform.OffsetZ = rand.Next(-100, 20);

                    cube.Transform = slideTransform;
                    model3DGroup.Children.Add(cube);

                    /*
                    var line = Line(slideTransform.OffsetX, slideTransform.OffsetY, slideTransform.OffsetZ, 0, 0, -50, 10);
                    model3DGroup.Children.Add(line);
                    */
                }

                modelVisual3D.Content = model3DGroup;
                this.viewport3d.Children.Add(modelVisual3D);
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        GeometryModel3D Line(double x1, double y1, double z1, double x2, double y2, double z2, double thickness)
        {

            var geometryModel = new GeometryModel3D();
            var meshGeo3D = new MeshGeometry3D();

            var offset = thickness / 2;
            var positionCollection = new Point3DCollection()
            {
                new Point3D(x1 - offset/z1, y1 - offset/z1, z1 - offset/z1),
                new Point3D(x2 - offset/z2, y2 - offset/z2, z2 - offset/z2),
                new Point3D(x2 + offset/z2, y2 + offset/z2, z2 + offset/z2),
                new Point3D(x1 + offset/z1, y1 + offset/z1, z1 + offset/z1)
            };
            meshGeo3D.Positions = positionCollection;

            var triangleIndices = new Int32Collection()
            {
                1, 2, 3,
                3, 4, 1
            };
            meshGeo3D.TriangleIndices = triangleIndices;
            geometryModel.Geometry = meshGeo3D;

            // horizontal linear gradient
            var horizontalGradient = new LinearGradientBrush()
            {
                StartPoint = new Point(x1, y1),
                EndPoint = new Point(x2, y2),
            };
            horizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0));

            var material = new DiffuseMaterial(horizontalGradient);
            geometryModel.Material = material;

            return geometryModel;
        }

        GeometryModel3D Cube()
        {
            var geometryModel = new GeometryModel3D();
            var meshGeo3D = new MeshGeometry3D(); // by default, a flat sheet.

            var positionCollection = new Point3DCollection()
            {
                new Point3D(-0.5, -0.5, -0.5),
                new Point3D(0.5, -0.5, -0.5),
                new Point3D(-0.5, 0.5, -0.5),
                new Point3D(-0.5, -0.5, 0.5),
                new Point3D(0.5, -0.5, 0.5),
                new Point3D(0.5, 0.5, 0.5),
                new Point3D(-0.5, 0.5, 0.5),
                new Point3D(0.5, 0.5, -0.5),
            };
            meshGeo3D.Positions = positionCollection;

            // create collection of texture coordinates
            /*var textureCoordCollection = new PointCollection()
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(1, 1),
                new Point(1, 1),
                new Point(0, 1),
                new Point(0, 0)
            };
            meshGeo3D.TextureCoordinates = textureCoordCollection;
            */

            var triangleIndices = new Int32Collection()
            {
                    0, 1, 7,
                    7, 2, 0,
                    0, 2, 6,
                    6, 3, 0,
                    0, 3, 4,
                    4, 1, 0,
                    1, 4, 5,
                    5, 7, 1,
                    3, 4, 5,
                    5, 6, 3,
                    6, 2, 7,
                    7, 5, 6
            };
            meshGeo3D.TriangleIndices = triangleIndices;

            // horizontal linear gradient
            var horizontalGradient = new LinearGradientBrush()
            {
                StartPoint = new Point(-0.5, 0.5),
                EndPoint = new Point(1, 0.5),
            };
            horizontalGradient.GradientStops.Add(new GradientStop(Colors.Red, 0));

            var material = new DiffuseMaterial(horizontalGradient);

            geometryModel.Geometry = meshGeo3D;
            geometryModel.Material = material;

            return geometryModel;
        }

        FaceAlignment faceAlignment = new FaceAlignment();
        FaceModel faceModel = new FaceModel();
        bool set0 = true;
        private void FaceFrameReader_FrameArrived(object sender, HighDefinitionFaceFrameArrivedEventArgs e)
        {
            using(var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null)
                    return;

                frame.GetAndRefreshFaceAlignmentResult(faceAlignment);
                //FaceFrameResult res = frame.;
                if (faceAlignment == null)
                {
                    Console.WriteLine("frame result null");
                    return;
                }

                IReadOnlyList<CameraSpacePoint> vertices = null;
                try
                {
                   vertices = faceModel.CalculateVerticesForAlignment(faceAlignment);
                } catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }

                /*var nose = res.FacePointsInColorSpace[FacePointType.Nose];
                var rotation = res.FaceRotationQuaternion;
                var bound = res.FaceBoundingBoxInColorSpace;
                */
                var nose = vertices[18];

                if (double.IsNaN(nose.X) || double.IsNaN(nose.Y) || double.IsNaN(nose.Z))
                    return;
                //Console.WriteLine("Rotation {0}, {1}, {2}", rotation.X, rotation.Y, rotation.Z);
                Console.WriteLine("Nose {0}, {1}", nose.X, nose.Y);

                if(set0)
                {
                    initNose = new Point3D(nose.X, nose.Y, nose.Z);
                    set0 = false;
                }

                var diff = new Point3D(nose.X - initNose.X, initNose.Y - nose.Y, nose.Z - initNose.Z);
                //var angle = Math.Atan(diff.Y / diff.X);
                //this.camera.LookDirection = new Vector3D(rotation.X, rotation.Y, rotation.Z);
                const double scale = 10;
                this.camera.Position = new Point3D(diff.X * scale, -diff.Y * scale, 15 + diff.Z * scale);
                this.camera.LookDirection = new Vector3D(-scale * diff.X, scale * diff.Y, -10 + scale * diff.Z);
                Console.WriteLine("diff nose: {0}, {1}, {2}", diff.X, diff.Y, diff.Z);

            }
        }

        private void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using(var frame = e.FrameReference.AcquireFrame())
            {
                if (frame == null)
                    return;

                frame.GetAndRefreshBodyData(this.bodies);

                Body body = null;
                for (int i = 0; i < this.bodies.Count; i++)
                {
                    if (this.bodies[i] != null && this.bodies[i].IsTracked)
                        body = this.bodies[i];
                }

                if (body == null)
                    return;

                if (this.faceSource != null && !this.faceSource.IsTrackingIdValid && body != null)
                {
                    this.faceSource.TrackingId = body.TrackingId;
                    Console.WriteLine("set tracking id");
                }
                var handLoc = body.Joints[JointType.HandRight];
                if (handLoc.TrackingState != TrackingState.Tracked)
                    return;

                var hand = body.JointOrientations[JointType.HandRight];
                //this.axisAngleRotation.Angle = hand.Orientation.X * 360 / (2 * Math.PI);

                //Console.WriteLine("hand: {0}, {1}, {2}", hand.Orientation.X, hand.Orientation.Y, hand.Orientation.Z);


                /*if(initNose.X == 0 && initNose.Y == 0 && handLoc.TrackingState == TrackingState.Tracked)
                {
                    initNose = new Point(handLoc.Position.X, handLoc.Position.Y);
                }

                var diff = new Point(handLoc.Position.X - initNose.X, handLoc.Position.Y - initNose.Y);

                Console.WriteLine("diff: {0}, {1}", diff.X, diff.Y);
                this.camera.Position = new Point3D(diff.X, diff.Y, 5);
                */
            }
        }

        private void click_Click(object sender, RoutedEventArgs e)
        {
            this.set0 = true;
        }
    }
}
