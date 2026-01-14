using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Flappybird
{
    public partial class MainWindow : Window
    {

        double velocityY = 0;
        const double gravity = 0.6;
        const double jumpStrength = -10;


        DispatcherTimer timer;
        bool gameOver = false;


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
            timer.Start();

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
            Canvas.SetLeft(topPipe, startX);
            Canvas.SetTop(topPipe, 0);

            Rectangle bottomPipe = new Rectangle
            {
                Width = pipeWidth,
                Height = bottomHeight,
                Fill = Brushes.Green
            };
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

        
    }
}
