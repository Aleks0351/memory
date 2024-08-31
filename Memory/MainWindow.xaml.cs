using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MemoryGame
{
    public partial class MainWindow : Window
    {
        private List<string> imageList;
        private List<Button> buttons;
        private Button firstSelectedButton;
        private Button secondSelectedButton;
        private int pairsFound;
        private int moves;
        private DispatcherTimer timer;
        private int remainingTime;
        private MediaPlayer backgroundMusic;
        private MediaPlayer clickSound;
        private MediaPlayer matchSound;
        private MediaPlayer victorySound;
        private MediaPlayer defeatSound;
        private MediaPlayer timeWarningSound;

        public MainWindow()
        {
            InitializeComponent();
            SetupGame();
        }

        private void SetupGame()
        {
            imageList = new List<string>
            {
                "image1.png", "image1.png",
                "image2.png", "image2.png",
                "image3.png", "image3.png",
                "image4.png", "image4.png",
                "image5.png", "image5.png",
                "image6.png", "image6.png"
            };

            buttons = new List<Button>();
            pairsFound = 0;
            moves = 0;
            remainingTime = 60; // 60 seconds

            SetupSounds();
            CreateButtons();
            ShuffleImages();
            SetupTimer();

            UpdateMovesDisplay();
            UpdateTimeDisplay();

            backgroundMusic.Play();
        }

        private void SetupSounds()
        {
            backgroundMusic = new MediaPlayer();
            backgroundMusic.Open(new Uri("background_music.wav", UriKind.Relative));
            backgroundMusic.MediaEnded += BackgroundMusic_MediaEnded;

            clickSound = new MediaPlayer();
            clickSound.Open(new Uri("click_sound.wav", UriKind.Relative));

            matchSound = new MediaPlayer();
            matchSound.Open(new Uri("match_sound.wav", UriKind.Relative));

            victorySound = new MediaPlayer();
            victorySound.Open(new Uri("victory_sound.wav", UriKind.Relative));

            defeatSound = new MediaPlayer();
            defeatSound.Open(new Uri("defeat_sound.wav", UriKind.Relative));

            timeWarningSound = new MediaPlayer();
            timeWarningSound.Open(new Uri("time_warning_sound.wav", UriKind.Relative));
        }

        private void BackgroundMusic_MediaEnded(object sender, EventArgs e)
        {
            backgroundMusic.Position = TimeSpan.Zero;
            backgroundMusic.Play();
        }

        private void CreateButtons()
        {
            for (int i = 0; i < 12; i++)
            {
                Button button = new Button();
                button.Click += Button_Click;
                button.Width = 100;
                button.Height = 100;
                button.Margin = new Thickness(5);
                buttons.Add(button);
                GameGrid.Children.Add(button);
            }
        }

        private void ShuffleImages()
        {
            Random rng = new Random();
            imageList = imageList.OrderBy(x => rng.Next()).ToList();

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Tag = imageList[i];
                buttons[i].Content = new Image
                {
                    Source = new BitmapImage(new Uri("back.png", UriKind.Relative)),
                    Stretch = Stretch.Fill
                };
            }
        }

        private void SetupTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;
            UpdateTimeDisplay();

            if (remainingTime <= 10)
            {
                timeWarningSound.Stop();
                timeWarningSound.Position = TimeSpan.Zero;
                timeWarningSound.Play();
            }

            if (remainingTime <= 0)
            {
                timer.Stop();
                defeatSound.Stop();
                defeatSound.Position = TimeSpan.Zero;
                defeatSound.Play();
                MessageBox.Show("Время вышло! Игра окончена.");
                Application.Current.Shutdown();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            clickSound.Stop();
            clickSound.Position = TimeSpan.Zero;
            clickSound.Play();

            if (clickedButton == firstSelectedButton) return;

            if (firstSelectedButton == null)
            {
                firstSelectedButton = clickedButton;
                ShowImage(firstSelectedButton);
            }
            else if (secondSelectedButton == null)
            {
                secondSelectedButton = clickedButton;
                ShowImage(secondSelectedButton);

                moves++;
                UpdateMovesDisplay();

                if (firstSelectedButton.Tag.ToString() == secondSelectedButton.Tag.ToString())
                {
                    matchSound.Stop();
                    matchSound.Position = TimeSpan.Zero;
                    matchSound.Play();
                    pairsFound++;
                    RemoveButtons();
                    CheckForWin();
                }
                else
                {
                    timer.Stop();
                    timer.Tick += HideButtons;
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Start();
                }
            }
        }

        private void ShowImage(Button button)
        {
            string imagePath = button.Tag.ToString();
            button.Content = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.Relative)),
                Stretch = Stretch.Fill
            };
        }

        private void RemoveButtons()
        {
            firstSelectedButton.Visibility = Visibility.Hidden;
            secondSelectedButton.Visibility = Visibility.Hidden;
            firstSelectedButton = null;
            secondSelectedButton = null;
        }

        private void HideButtons(object sender, EventArgs e)
        {
            firstSelectedButton.Content = new Image
            {
                Source = new BitmapImage(new Uri("back.png", UriKind.Relative)),
                Stretch = Stretch.Fill
            };
            secondSelectedButton.Content = new Image
            {
                Source = new BitmapImage(new Uri("back.png", UriKind.Relative)),
                Stretch = Stretch.Fill
            };
            firstSelectedButton = null;
            secondSelectedButton = null;

            timer.Tick -= HideButtons;
            timer.Interval = TimeSpan.FromSeconds(1);
        }

        private void CheckForWin()
        {
            if (pairsFound == 6)
            {
                timer.Stop();
                victorySound.Stop();
                victorySound.Position = TimeSpan.Zero;
                victorySound.Play();
                MessageBox.Show($"Победа! Вам потребовалось {moves} ходов.");
                Application.Current.Shutdown();
            }
        }

        private void UpdateMovesDisplay()
        {
            MovesTextBlock.Text = $"Кол-во ходов: {moves}";
        }

        private void UpdateTimeDisplay()
        {
            TimeTextBlock.Text = $"Время: {remainingTime}s";
        }
    }
}
