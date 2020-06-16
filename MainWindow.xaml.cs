using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using WelcomeToVichMat.Labs.semestr2;

// ReSharper disable once CheckNamespace
namespace VichMat.Solution
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainWindow _window;
        private static Solution _solution;

        public MainWindow()
        {
            Initialize();

            var laba = new LastLaba();
        }

        private void Initialize()
        {
            _window = this;

            InitializeComponent();
            helloButton.Opacity = 0;

            DoubleAnimation anim1 = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(1) };
            DoubleAnimation anim2 = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(3) };

            SequenceAnimation sequence1 = new SequenceAnimation();
            sequence1.AddAnimation(helloButton, anim1, Button.OpacityProperty);
            sequence1.AddAnimation(welcome, anim2, TextBox.OpacityProperty);

            SequenceAnimation sequence2 = new SequenceAnimation();
            sequence2.AddAnimation(student, anim2, TextBox.OpacityProperty);
            sequence2.AddAnimation(prepod, anim2, TextBox.OpacityProperty);


            sequence1.Play();
            sequence2.Play(true);
        }

        public static MainWindow GetWindow()
        {
            return _window;
        }

        public static void SetSolution(Solution solution)
        {
            _solution = solution;
        }

        void RunSolution()
        {
            _solution.Start();
        }

        private void helloButton_Click(object sender, RoutedEventArgs e)
        {
            RunSolution();
            //helloButton.IsEnabled = false;
            helloButton.Content = "Ещё раз";
        }

        public void ShowError(string error, bool close = false)
        {
            var result = MessageBox.Show(error);
            if (result == MessageBoxResult.OK && close)
            {
                Close();
            }
        }

        public void ShowData(string data)
        {
            this.data.Text = data;
        }

        public void CreateTable<TSource>(IEnumerable<TSource> source)
        {
            table.Visibility = Visibility.Visible;

            table.ItemsSource = source;
        }
    }
}
