using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Flappybird
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        DispatcherTimer timer;
        double velocityY = 0;
        const double gravity = 0.6;
        const double jumpStrength = -10;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            timer.Tick += GameLoop;
            timer.Start();
        }
        private void GameLoop(object sender, EventArgs e)
        {
            velocityY += gravity;
            Birdmove.Y += velocityY;

            double bottomLimit = backgrd.Height-Bird.ActualHeight;

            if (Birdmove.Y >= bottomLimit)
            {
                Birdmove.Y = bottomLimit;
                velocityY = 0;
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) 
            {
                velocityY = jumpStrength;
            }
        }

    }
}