using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wpf_simple_FFMPEG {
    /// <summary>
    /// Interaction logic for BlurSelector.xaml
    /// </summary>
    public partial class BlurSelector : Window {

        //private List<Rectangle> rectangles = new List<Rectangle>();
        private Rectangle lastSelected = null;

        private readonly SimpleConfig simpleConfig;

        public BlurSelector(ImageSource image, List<Rect> oldState) {
            InitializeComponent();

            // https://github.com/PetrVobornik/WpfShapeEditor/blob/master/ShapeEditor.Demo/ShapeEditor.Demo/ShapeEditor.cs

            canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            //testShape.MouseLeftButtonDown += TestShape_MouseLeftButtonDown;

            imageHolder.Source = image;
            imageHolder.InvalidateVisual();


            canvas.Height = imageHolder.Source.Height;
            canvas.Width = imageHolder.Source.Width;

            oldState.ForEach(r => {
                addSphere(r.Left, r.Top - canvas.Margin.Top, r.Width, r.Height);
            });

            this.Closing += (s, e) => { ShapeEditorControl.ReleaseElement(); };
        }

        private void addSphere(double x = 10, double y = 10, double width = 100, double height = 100) {
            Rectangle rectangle = new() {
                Width = width,
                Height = height,

                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Color.FromArgb(160, 244, 244, 245))
            };

            rectangle.MouseLeftButtonDown += TestShape_MouseLeftButtonDown;

            canvas.Children.Add(rectangle);
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            //rectangles.Add(rectangle);
        }


        private void TestShape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ShapeEditorControl.CaptureElement(sender as FrameworkElement, e);
            e.Handled = true;

            lastSelected = sender as Rectangle;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ShapeEditorControl.ReleaseElement();
        }

        private void addNewRectangleButton(object sender, RoutedEventArgs e) {
            addSphere(scrollViewer.HorizontalOffset + (scrollViewer.ActualWidth / 2D) - 50, scrollViewer.VerticalOffset + (scrollViewer.ActualHeight / 2D) - 50);
        }

        private void removeSelectedButton(object sender, RoutedEventArgs e) {
            ShapeEditorControl.ReleaseElement();
            canvas.Children.Remove(lastSelected);
        }

        public List<Rect> GetRects() {
            return canvas.Children.OfType<Rectangle>().Select(r => new Rect(Canvas.GetLeft(r), canvas.Margin.Top + Canvas.GetTop(r), r.Width, r.Height)).ToList();
        }

        private void RemoveSelectedSphereKeybind(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                removeSelectedButton(null, null);
            }
        }

        private void ButtonLoadPreset_Click(object sender, RoutedEventArgs e) {
                
            //IncList.ContextMenu = new ContextMenu();
            //IncList.ContextMenu.Items.Add(new MenuItem() { Header = "Test1" });
            //IncList.ContextMenu.Items.Add(new MenuItem() { Header = "Test2" });
        }

        private void ButtonSavePreset_Click(object sender, RoutedEventArgs e) {
            
        }
    }
}
