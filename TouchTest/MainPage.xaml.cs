using System.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using System.ComponentModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace TouchTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        protected List<Crossline> points;
        protected List<TextBlock> textBlocks;
        protected List<Color> colors;
        protected Random random;
        public ObservableDict<uint, Polyline> PointerDictionary{ get;set;}
        protected bool isHiddenCommandBar = false;
        public ObservableDict<uint, Polyline> TempPointerDictionary { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            MainCanvas.Width = Window.Current.Bounds.Width;
            MainCanvas.Height = Window.Current.Bounds.Height;
            points = new List<Crossline>();
            colors = new List<Color>();
            textBlocks = new List<TextBlock>();
            random = new Random();
            PointerDictionary = new ObservableDict<uint, Polyline>();
            TempPointerDictionary = new ObservableDict<uint, Polyline>();
             var cls = typeof(Colors).GetTypeInfo().DeclaredProperties.ToList();
            foreach (PropertyInfo propertyInfo in cls)
            {
                colors.Add((Color)propertyInfo.GetValue(null));
            }
            this.DataContext = this;
        }




        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Config.isReserveTrace.Value)
            {
                AppBarButtonClear.Visibility = Visibility.Visible;
                BarButtonBack.Visibility = Visibility.Visible;
                BarButtonNext.Visibility = Visibility.Visible;
            }
            else
            {
                AppBarButtonClear.Visibility = Visibility.Collapsed;
                BarButtonBack.Visibility = Visibility.Collapsed;
                BarButtonNext.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (this.cVsCover.Visibility == Visibility.Visible)
                this.cVsCover.Visibility = Visibility.Collapsed;
            PointerPoint point = e.GetCurrentPoint(this);
            var line = new Crossline(Colors.White, point);
            line.Tag = point.PointerId;
            Color color = colors[random.Next(0, colors.Count - 1)];
            line.LineColor = color;
            CanvasLine.Children.Add(line.HorizontalPath);
            CanvasLine.Children.Add(line.VerticalPath);
            points.Add(line);
            if (points.Count > Config.supportMaxTouchs.Value)
            {
                Config.supportMaxTouchs.Value = points.Count;
            }
            if (Config.isNeedShowTrace.Value)
            {
                //var pointForTrace = new Point(point.Position.X, point.Position.Y - Config.TOUCH_Y_OFFSET);
                var pointForTrace = new Point(point.Position.X, point.Position.Y);
                var polyline = new Polyline();
                polyline.Stroke = new SolidColorBrush(color);
                polyline.StrokeThickness = Config.TraceThickness.Value;
                CanvasLine.Children.Add(polyline);
                PointerDictionary.AddValue(point.PointerId, polyline);
            }
            if (Config.isShowCoord.Value)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.Width = StackPanelCoords.Width;
                textBlock.Height = 40;
                textBlock.Tag = point.PointerId;
                textBlock.Foreground = new SolidColorBrush(color);
                textBlocks.Add(textBlock);
                textBlock.Text = string.Format("Point{0}:  x:{1:N0} y:{2:N0}", textBlocks.IndexOf(textBlock)+1, point.Position.X*Config.scaleFactor,
                    point.Position.Y*Config.scaleFactor);
                StackPanelCoords.Children.Add(textBlock);
               
            }
           
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            PointerPoint point = e.GetCurrentPoint(this);
            Crossline line = points.Find(s => s.Tag == point.PointerId);
            if (line == null) return;
            line.Point = point;
            if (PointerDictionary.ContainsKey(point.PointerId))
            {
                //PointerDictionary[point.PointerId].Points.Add(new Point(point.Position.X,point.Position.Y-Config.TOUCH_Y_OFFSET));
                PointerDictionary[point.PointerId].Points.Add(new Point(point.Position.X, point.Position.Y));
            }
            if (Config.isShowCoord.Value)
            {
                TextBlock textBlock = textBlocks.Find(s => Convert.ToUInt16(s.Tag) == point.PointerId);
                if (textBlock != null)
                {
                    textBlock.Text = string.Format("Point{0}:  x:{1:N0} y:{2:N0}", textBlocks.IndexOf(textBlock)+1, point.Position.X*Config.scaleFactor,
                    point.Position.Y*Config.scaleFactor);
                }
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            PointerPoint point = e.GetCurrentPoint(this);
            Crossline line = points.Find(s => s.Tag == point.PointerId);
            if (line == null) return;
            this.CanvasLine.Children.Remove(line.HorizontalPath);
            this.CanvasLine.Children.Remove(line.VerticalPath);
            points.Remove(line);
            if (!Config.isReserveTrace.Value)
            {
                if (PointerDictionary.ContainsKey(point.PointerId))
                {
                    CanvasLine.Children.Remove(PointerDictionary[point.PointerId]);
                    PointerDictionary.RemoveValue(point.PointerId);
                }
            }
            if (Config.isReserveTrace.Value)
            {
                this.AppBarButtonClear.Visibility = Visibility.Visible;
                if (PointerDictionary.Count > 0)
                {
                    BarButtonBack.Visibility = Visibility.Visible;
                }
            }
            if (Config.isShowCoord.Value)
            {
                TextBlock textBlock = textBlocks.Find(s => Convert.ToUInt16(s.Tag) == point.PointerId);
                if (textBlock != null)
                {
                    StackPanelCoords.Children.Remove(textBlock);
                    textBlocks.Remove(textBlock);
                }
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            PointerPoint point = e.GetCurrentPoint(this);
            if (!points.Exists(s=>s.Tag==point.PointerId))return;   
            Crossline line = points.Find(s => s.Tag == point.PointerId);
            if (line == null) return;
            this.CanvasLine.Children.Remove(line.HorizontalPath);
            this.CanvasLine.Children.Remove(line.VerticalPath);
            points.Remove(line);
            if (!Config.isReserveTrace.Value)
            {
                if (PointerDictionary.ContainsKey(point.PointerId))
                {
                    CanvasLine.Children.Remove(PointerDictionary[point.PointerId]);
                    PointerDictionary.RemoveValue(point.PointerId);
                }

            }
            if (Config.isShowCoord.Value)
            {
                TextBlock textBlock = textBlocks.Find(s => Convert.ToUInt16(s.Tag) == point.PointerId);
                if (textBlock != null)
                {
                    StackPanelCoords.Children.Remove(textBlock);
                    textBlocks.Remove(textBlock);
                }
            }
        }

        private void CanvasCoverTaped(object sender, TappedRoutedEventArgs e)
        {
            this.cVsCover.Visibility = Visibility.Collapsed;

        }

        private void appbar_settingClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof (SettingPage));
        }

        //这些判断非常不理想,应该用Bind比较合适
        private void appbar_deleteClick(object sender, RoutedEventArgs e)
        {
            this.CanvasLine.Children.Clear();
            PointerDictionary.ClearValue();
            TempPointerDictionary.ClearValue();
            points.Clear();
            if (StackPanelCoords.Children.Count > 0)
            {
                textBlocks.Clear();
                StackPanelCoords.Children.Clear();
            }
            
        }

        private void appbar_nextClick(object sender, RoutedEventArgs e)
        {
            if (TempPointerDictionary.Count == 0)
            {
                return;
            }
            List<uint> keys = TempPointerDictionary.Keys.ToList();
            uint key = keys.Min();
            Polyline polyline = TempPointerDictionary[key];
            CanvasLine.Children.Add(polyline);
            PointerDictionary.AddValue(key,polyline);
            TempPointerDictionary.RemoveValue(key);
        }

        private void appbar_backClick(object sender, RoutedEventArgs e)
        {
            List<uint> keys = PointerDictionary.Keys.ToList();
            uint key = keys.Max();
            Polyline polyline = PointerDictionary[key];
            CanvasLine.Children.Remove(polyline);
            TempPointerDictionary.AddValue(key, polyline);
            PointerDictionary.RemoveValue(key);
        }
    }
}
