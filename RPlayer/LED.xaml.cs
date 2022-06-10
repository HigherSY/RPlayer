using LibVLCSharp.Shared;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RPlayer
{
    /// <summary>
    /// LED.xaml 的交互逻辑
    /// </summary>
    public partial class MediaLED : UserControl
    {
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(VLCState), typeof(MediaLED), new PropertyMetadata(VLCState.NothingSpecial, StateChanged));

        private static BrushConverter brushConverter = new BrushConverter();
        private static Brush myRed = (Brush)brushConverter.ConvertFromString("#f44336");
        private static Brush myGreen = (Brush)brushConverter.ConvertFromString("#4caf50");
        private static Brush myYellow = (Brush)brushConverter.ConvertFromString("#ffeb3b");
        private static Brush myBlue = (Brush)brushConverter.ConvertFromString("#2196f3");

        private static Int32Animation fastBlinking;
        private static DoubleAnimation breathing;

        static MediaLED()
        {
            breathing = new DoubleAnimation
            {
                From = 1,
                To = 0,
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseInOut },
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = TimeSpan.FromSeconds(1)
            };

            fastBlinking = new Int32Animation
            {
                From = 1,
                To = 0,
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = TimeSpan.FromMilliseconds(500)
            };
        }

        private static void RefreshState(MediaLED led, VLCState newState)
        {
            switch (newState)
            {
                case VLCState.Opening:
                case VLCState.Buffering:
                    led.light.Fill = myGreen;
                    led.BeginAnimation(Ellipse.OpacityProperty, breathing);
                    break;
                case VLCState.Playing:
                    led.light.Fill = myRed;
                    led.BeginAnimation(Ellipse.OpacityProperty, breathing);
                    break;
                case VLCState.Paused:
                    led.light.Fill = myYellow;
                    led.BeginAnimation(Ellipse.OpacityProperty, breathing);
                    break;
                case VLCState.NothingSpecial:
                case VLCState.Stopped:
                case VLCState.Ended:
                    led.light.Fill = myGreen;
                    led.BeginAnimation(Ellipse.OpacityProperty, null);
                    break;
                case VLCState.Error:
                    led.light.Fill = myRed;
                    led.BeginAnimation(Ellipse.OpacityProperty, null);
                    break;
            }
        }

        private static void StateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MediaLED self = (MediaLED)d;
            RefreshState(self, (VLCState)e.NewValue);
        }
        public VLCState State
        {
            get { return (VLCState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public MediaLED()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshState((MediaLED)sender, State);
        }
    }
}
