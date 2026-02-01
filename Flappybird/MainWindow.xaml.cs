using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Flappybird
{
    public partial class MainWindow : Window
    {

        double velocityY = 0;
        double gravity = 0.6;
        const double jumpStrength = -10;


        DispatcherTimer timer;
        bool gameOver = false;

        DispatcherTimer rainTimer;
        bool isRaining = false;

        DispatcherTimer fogTimer;
        bool isFoggy = false;




        class Obstacle
        {
            public Rectangle TopPipe;
            public Rectangle BottomPipe;
            public bool Passed = false;
        }

        List<Obstacle> obstacles = new List<Obstacle>();
        Random rnd = new Random();

        const double pipeWidth = 60;
        const double gapHeight = 150;
        const double pipeSpeed = 3;
        const double pipeSpacing = 300;
        double rain_chance = 0;
        double fog_chance = 0;

        double lastPipeX = 0;

        int score = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) 
            };
            timer.Tick += GameLoop;
            

            this.Focus();
            
        }

        private void GameLoop(object sender, EventArgs e)
        {   
           
            if (gameOver) return;

            velocityY += gravity;
            Birdmove.Y += velocityY;

            double bottomLimit = backgrd.ActualHeight - Bird.ActualHeight;
            if (Birdmove.Y > bottomLimit)
            {
                Birdmove.Y = bottomLimit;
                velocityY = 0;
            }

            Birdmove.Y = Math.Max(Birdmove.Y, 0);

            foreach (var obs in obstacles)
            {
                Canvas.SetLeft(obs.TopPipe, Canvas.GetLeft(obs.TopPipe) - pipeSpeed);
                Canvas.SetLeft(obs.BottomPipe, Canvas.GetLeft(obs.BottomPipe) - pipeSpeed);
            }

            if (obstacles.Count == 0 || lastPipeX <= backgrd.ActualWidth - pipeSpacing)
            {
                SpawnPipes();
            }
            else
            {
                lastPipeX -= pipeSpeed;
            }

            double is_rain_happening = rnd.Next(1, 10000);

            if (is_rain_happening < rain_chance)
            {
                StartRain(5);
                gravity = 0.9;
                
            }

            double is_fog_happening = rnd.Next(0, 10000);

            if (is_fog_happening < fog_chance)
            {
                StartFog(4);
            }


            CheckCollision();
            CheckScore();
        }

        private void SpawnPipes()
        {
            double canvasHeight = backgrd.ActualHeight;
            if (canvasHeight == 0) return;

            double topHeight = rnd.Next(50, (int)(canvasHeight - gapHeight - 50));
            double bottomHeight = canvasHeight - topHeight - gapHeight;
            double startX = backgrd.ActualWidth;

            Rectangle topPipe = new Rectangle
            {
                Width = pipeWidth,
                Height = topHeight,
                Fill = Brushes.Green
            };
            Panel.SetZIndex(topPipe, 1);
            Canvas.SetLeft(topPipe, startX);
            Canvas.SetTop(topPipe, 0);

            Rectangle bottomPipe = new Rectangle
            {
                Width = pipeWidth,
                Height = bottomHeight,
                Fill = Brushes.Green
            };
            Panel.SetZIndex(bottomPipe, 1);
            Canvas.SetLeft(bottomPipe, startX);
            Canvas.SetTop(bottomPipe, topHeight + gapHeight);

            backgrd.Children.Add(topPipe);
            backgrd.Children.Add(bottomPipe);

            obstacles.Add(new Obstacle
            {
                TopPipe = topPipe,
                BottomPipe = bottomPipe
            });

            lastPipeX = startX;
        }

        private void CheckCollision()
        {
            Rect birdRect = new Rect(Canvas.GetLeft(Bird), Birdmove.Y, Bird.ActualWidth, Bird.ActualHeight);

            if (birdRect.Top <= -backgrd.ActualHeight || birdRect.Bottom >= backgrd.ActualHeight)
            {
                EndGame();
                return;
            }

            foreach (var obs in obstacles)
            {
                Rect topRect = new Rect(Canvas.GetLeft(obs.TopPipe), Canvas.GetTop(obs.TopPipe), obs.TopPipe.Width, obs.TopPipe.Height);
                Rect bottomRect = new Rect(Canvas.GetLeft(obs.BottomPipe), Canvas.GetTop(obs.BottomPipe), obs.BottomPipe.Width, obs.BottomPipe.Height);

                if (birdRect.IntersectsWith(topRect) || birdRect.IntersectsWith(bottomRect))
                {
                    EndGame();
                    return;
                }
            }
        }

        private void CheckScore()
        {
            foreach (var obs in obstacles)
            {
                if (obs.Passed) continue;

                double pipeX = Canvas.GetLeft(obs.TopPipe);

                if (pipeX + pipeWidth < Canvas.GetLeft(Bird))
                {
                    obs.Passed = true;
                    score++;
                    Title = $"Score: {score}";
                }
            }
        }

        private void EndGame()
        {
            gameOver = true;
            timer.Stop();
            MessageBox.Show($"Game Over\nPontszám: {score}");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameOver) return;

            if (e.Key == Key.Space)
            {
                velocityY = jumpStrength;
            }
        }

        private void StartRain(int durationSeconds)
        {
            if (isRaining) return;

            isRaining = true;
            SetRainBackground();

            rainTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationSeconds)
            };
            rainTimer.Tick += StopRain;
            rainTimer.Start();
        }

        private void StopRain(object sender, EventArgs e)
        {
            rainTimer.Stop();
            rainTimer.Tick -= StopRain;

            SetBackground();
            gravity = 0.6;
            isRaining = false;
        }

        private void StartFog(int durationSeconds)
        {
            if (isFoggy) return;

            isFoggy = true;

            FogOverlay.Opacity = 0.55;
            FogOverlay.Visibility = Visibility.Visible;

            fogTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationSeconds)
            };
            fogTimer.Tick += StopFog;
            fogTimer.Start();
        }

        private void StopFog(object sender, EventArgs e)
{
    fogTimer.Stop();
    fogTimer.Tick -= StopFog;

    FogOverlay.Opacity = 0;
    FogOverlay.Visibility = Visibility.Collapsed;

    isFoggy = false;
}


        private void SetRainBackground()
        {
            BackgroundBrush.ImageSource =
                new BitmapImage(new Uri("pack://application:,,,/Images/flappybackrain.png"));
        }
        private void SetBackground()
        {
            BackgroundBrush.ImageSource =
                new BitmapImage(new Uri("pack://application:,,,/Images/flappybckground.png"));
        }

        private void easy_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            easy.Visibility = Visibility.Collapsed;
            normal.Visibility = Visibility.Collapsed;
            hard.Visibility = Visibility.Collapsed;

            
        }

        private void normal_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            easy.Visibility = Visibility.Collapsed;
            normal.Visibility = Visibility.Collapsed;
            hard.Visibility = Visibility.Collapsed;
            rain_chance = 30;
            fog_chance = 10;
        }

        private void hard_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            easy.Visibility = Visibility.Collapsed;
            normal.Visibility = Visibility.Collapsed;
            hard.Visibility = Visibility.Collapsed;
            rain_chance = 200;
            fog_chance = 30;
        }
    }
}
