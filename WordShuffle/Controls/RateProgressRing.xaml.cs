using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WordShuffle.Controls
{
    public sealed partial class RateProgressRing : UserControl
    {


        public double Rate
        {
            get { return (double)GetValue(RateProperty); }
            set {
                SetValue(RateProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Rate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(double), typeof(RateProgressRing), new PropertyMetadata(0, (s, e) =>
             {
                 var control = s as RateProgressRing;
                 control.ChangeRate((double)e.NewValue);
             }));



        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StrokeWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(RateProgressRing), new PropertyMetadata(5.0, (s, e) =>
             {
                 var control = s as RateProgressRing;
                 control.path.StrokeThickness = (double)e.NewValue;
             }));



        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(RateProgressRing), new PropertyMetadata(new SolidColorBrush(Colors.Blue), (s, e) =>
             {
                 var control = s as RateProgressRing;
                 control.path.Stroke = e.NewValue as Brush;
             }));



        public Brush RingBackground
        {
            get { return (Brush)GetValue(RingBackgroundProperty); }
            set { SetValue(RingBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RingBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RingBackgroundProperty =
            DependencyProperty.Register("RingBackground", typeof(Brush), typeof(RateProgressRing), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), (s, e) =>
            {
                var control = s as RateProgressRing;
                control.ellipse_back.Fill = e.NewValue as Brush;
            }));
        

        public RateProgressRing()
        {
            this.InitializeComponent();
            this.SizeChanged += (s, e) =>
            {
                ChangeRate(Rate);
            };
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ChangeRate(Rate);
        }

        public void ChangeRate(double rate)
        {
            double angle = 360 * rate;
            double width = ActualWidth;
            double halfWidth = width / 2;
            double height = ActualHeight;
            double halfHeight = height / 2;

            archSegment.SweepDirection = SweepDirection.Clockwise;

            archSegment.Size = new Size(halfWidth, halfHeight);
            if (angle >= 0 && angle < 90)
            {
                var convertAngle = angle * Math.PI / 180.0;
                archSegment.Point = new Point(halfWidth + halfWidth * Math.Sin(convertAngle), halfHeight - halfHeight * Math.Cos(convertAngle));
                archSegment.IsLargeArc = false;
            }
            else if (angle >= 90 && angle < 180)
            {
                var convertAngle = angle - 90;
                convertAngle = convertAngle * Math.PI / 180.0;
                archSegment.Point = new Point(halfWidth + halfWidth * Math.Cos(convertAngle), halfHeight + halfHeight * Math.Sin(convertAngle));
                archSegment.IsLargeArc = false;
            }
            else if (angle >= 180 && angle < 270)
            {
                var convertAngle = angle - 180;
                convertAngle = convertAngle * Math.PI / 180.0;
                archSegment.Point = new Point(halfWidth - halfWidth * Math.Sin(convertAngle), halfHeight + halfHeight * Math.Cos(convertAngle));
                archSegment.IsLargeArc = true;
            }
            else if (angle >= 270 && angle < 360)
            {
                var convertAngle = angle - 270;
                convertAngle = convertAngle * Math.PI / 180.0;
                archSegment.Point = new Point(halfWidth - halfWidth * Math.Cos(convertAngle), halfHeight - halfHeight * Math.Sin(convertAngle));
                archSegment.IsLargeArc = true;
            }else
            {
                var convertAngle = angle * Math.PI / 180.0;
                archSegment.Point = new Point(halfWidth - halfWidth * Math.Sin(convertAngle)-0.00001, halfHeight - halfHeight * Math.Cos(convertAngle));
                archSegment.IsLargeArc = true;
            }

            pathFigure.StartPoint = new Point(halfWidth, 0);
        }
    }
}
