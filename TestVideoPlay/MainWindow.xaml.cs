using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestVideoPlay.Class;
using TestVideoPlay.ViewModels;

namespace TestVideoPlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int popularPage = 1;
        public MainWindow()
        {
            InitializeComponent();
            //HomePageViewModel.GetGenres("en", GenresDisplay);
        }
       
        public static bool videoPlayback = false;
        public static void AddPage(Page page)
        {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageCreator(page);
        }
        private void PageCreator(Page page)
        {
            Frame fr = new Frame();
            Grid.SetRowSpan(fr, 2);
            fr.Opacity = 0;
            fr.Content = page;
            Storyboard sb = this.FindResource("OpacityUp") as Storyboard;
            Storyboard blur = (Storyboard)FindResource("BlurBackground");
            blur.Begin();
            ContentOnTop.Children.Add(fr);
            sb.Begin(fr);
        }
        public static void ShowContent()
        {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).BaseGrid.Visibility = Visibility.Visible;
        }
        public static void HideContent()
        {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).BaseGrid.Visibility = Visibility.Collapsed;
        }
        public static void RemovePage()
        {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).PageRemover();
        }

        private void PageRemover()
        {
            var p = ContentOnTop.Children[ContentOnTop.Children.Count - 1] as FrameworkElement;
            Storyboard sb = this.FindResource("OpacityDown") as Storyboard;
            Storyboard sbLoad = sb.Clone();
            Storyboard blur = (Storyboard)FindResource("DeBlurBackground");
            blur.Begin();
            sbLoad.Completed += (s, e) => FinishedRemove(p);
            sbLoad.Begin(p);
        }
        private void FinishedRemove(UIElement ue)
        {
            ContentOnTop.Children.Remove(ue);
        }
        #region Page handling
        public enum PlayerState { PiP, Fullscreen, Normal };
        private class Dimensions
        {
            public double Width { get; set; }
            public double Height { get; set; }
            public WindowState LastState { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }
        }

        PlayerState currentState = PlayerState.Normal;
        Dimensions dimensions;
        public static void SwitchState(PlayerState state, bool reset = false)
        {
            Window main = Application.Current.MainWindow;
            ((MainWindow)main).ViewSwitcher(state, reset);
        }
        public void ViewSwitcher(PlayerState state, bool reset)
        {
            if (currentState == state)
            {
                state = PlayerState.Normal;
            }
            if (currentState == PlayerState.Normal && state == PlayerState.Normal)
            {
                return;
            }
            if ((currentState == PlayerState.Fullscreen && state == PlayerState.PiP) || ((currentState == PlayerState.PiP && state == PlayerState.Fullscreen)))
            {
                ViewSwitcher(PlayerState.Normal, reset);
            }
            if (currentState == PlayerState.PiP)
            {
                this.Top = dimensions.Top;
                this.Left = dimensions.Left;
            }
            switch (state)
            {
                case PlayerState.Fullscreen:
                    dimensions = new Dimensions()
                    {
                        Width = this.Width,
                        Height = this.Height,
                        LastState = this.WindowState
                    };
                    this.Visibility = Visibility.Collapsed;
                    this.Topmost = true;
                    this.WindowStyle = WindowStyle.None;
                    this.ResizeMode = ResizeMode.NoResize;
                    this.WindowState = WindowState.Maximized;
                    this.Visibility = Visibility.Visible;
                    break;
                case PlayerState.PiP:
                    dimensions = new Dimensions()
                    {
                        Width = this.Width,
                        Height = this.Height,
                        LastState = this.WindowState,
                        Left = this.Left,
                        Top = this.Top
                    };
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Normal;
                    this.MinWidth = 640;
                    this.Width = this.MinWidth;
                    this.MinHeight = 360;
                    this.Height = this.MinHeight;
                    this.Left = SystemParameters.PrimaryScreenWidth - 660;
                    this.Top = 20;
                    this.ResizeMode = ResizeMode.NoResize;
                    this.Topmost = true;
                    break;
                case PlayerState.Normal:
                    this.WindowState = dimensions.LastState;
                    this.Topmost = false;
                    this.MinHeight = 480;
                    this.MinWidth = 800;
                    this.Height = dimensions.Height;
                    this.Width = dimensions.Width;
                    this.ResizeMode = ResizeMode.CanResize;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    dimensions = null;
                    break;
            }
            currentState = state;



        }
        #endregion
        private void Play_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //string urlFile = @"C:\Users\ThanhYen\Downloads\New folder\New folder (2)\Season 01\A Confession - S01E01 - Episode 1.mkv";
            string urlFile = txtPATH.Text;
            MainWindow.AddPage(new LocalPlayer(urlFile));
        }

        private void BaseGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void HiderGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private async void Stream_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.AddPage(new PleaseWait());
            //Torrent tor = await Torrent.SearchSingle(Database.GetSeries((int)episode.seriesId), episode, Settings.StreamQuality);
            //txtMagnet.Text = "magnet:?xt=urn:btih:3D78150CE51D1632419802D40AB9A2B26A10D726&dn=Arrow.S08E04.HDTV.x264-SVA%5Bettv%5D&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.pirateparty.gr%3A6969%2Fannounce&tr=udp%3A%2F%2Fexodus.desync.com%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.internetwarriors.net%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.torrent.eu.org%3A451&tr=udp%3A%2F%2Ftracker.cyberia.is%3A6969%2Fannounce&tr=udp%3A%2F%2Fopen.demonii.si%3A1337%2Fannounce&tr=udp%3A%2F%2Fopen.stealth.si%3A80%2Fannounce&tr=udp%3A%2F%2Ftracker.tiny-vps.com%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.iamhansen.xyz%3A2000%2Fannounce&tr=udp%3A%2F%2Fexplodie.org%3A6969%2Fannounce&tr=udp%3A%2F%2Fdenis.stalker.upeer.me%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.zer0day.to%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&tr=udp%3A%2F%2Fcoppersurfer.tk%3A6969%2Fannounce";
            Torrent tor = new Torrent();
            tor.Magnet = txtMagnet.Text;
            MainWindow.RemovePage();
            TorrentDownloader td = new TorrentDownloader(tor);
            await td.Stream();
        }

        private void MoviesDisplay_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalOffset == e.ExtentHeight - e.ViewportHeight)
            {

                //if (isGenreMovieSelected)
                //{
                //    genrePage++;

                //    HomePageViewModel.GetMorePageMovieGenres(HomePageViewModel.GenreMovies, "en", SelectedGenreID, genrePage, MoviesDisplay);
                //}
                //else if (IsSearchedMovie)
                //{
                //    searchedMoviesPage++;
                //    HomePageViewModel.GetMorePageSearchedMovies(HomePageViewModel.SearchedMovies, "en",
                //        TxtSearchMovie.Text, searchedMoviesPage, MoviesDisplay);
                //}
                //else
                //{
                //    switch (ClickedButtonName)
                //    {
                //        case "BtnPopular":
                //            popularPage++;
                //            HomePageViewModel.GetMorePageForPopularMovies(HomePageViewModel.PopularMovies, "en",
                //                popularPage);
                //            break;
                //        case "BtnGreatest":

                //            greatestPage++;
                //            HomePageViewModel.GetMorePageForGreatestMovies(HomePageViewModel.GreatestMovies, "en",
                //                greatestPage);

                //            break;
                //        case "BtnRecent":
                //            recentPage++;
                //            HomePageViewModel.GetMorePageForRecentMovies(HomePageViewModel.RecentMovies, "en",
                //                recentPage);
                //            break;

                //        default:
                //            break;

                //    }
                }
            }

        private void MoviesDisplay_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (this.NavigationService != null)
            //{
            //    ListBox = MoviesDisplay;
            //    Movie mov = (Movie)MoviesDisplay.SelectedItem;
            //    MovieDetailsPage page = new MovieDetailsPage(mov);
            //    GetMovieDetailsAsync("en", mov, page);
            //    this.NavigationService.Navigate(page);
            //}
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
