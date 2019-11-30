using NReco.VideoInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
using System.Windows.Threading;
using TestVideoPlay.Class;
using static System.Environment;
using static TestVideoPlay.LocalPlayer;

namespace TestVideoPlay
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class StreamPlayer : Page
    {
        public StreamPlayer(TorrentDownloader downloader)
        {
            InitializeComponent();
            this.downloader = downloader;
        }
        public TorrentDownloader downloader;
        string file;
        FFProbe media;
        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSidebar(); //Load tab sub
            MainWindow.HideContent();
            Helper.DisableScreenSaver();
            MainWindow.videoPlayback = true;
            StreamPage.Focus();
            media = new FFProbe();
            media.ToolPath = Environment.GetFolderPath(SpecialFolder.ApplicationData);
            VolumeSlider.Value = Player.Volume = Properties.Settings.Default.Volume;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            Focus();
            Player.MediaOpened += (s, ev) => MediaOpenedEvent();

            while (true)
            {
                Animate();
                await Task.Run(() =>
                {
                    Thread.Sleep(1080);
                });
                file = GetSource();
                if (file != null && downloader.Status.Progress > 0.01)
                {
                    try
                    {
                        var duration = media.GetMediaInfo(file).Duration.TotalSeconds;
                        var downloaded = duration * downloader.Status.Progress;
                        if (File.Exists(file) && duration != 0 && downloaded > 10)
                        {
                            Player.Source = new Uri(file);
                            Player.Stop();
                            break;
                        }
                    }
                    catch (Exception) { }
                }

            }
            ////TorrentDatabase.Save(downloader.TorrentSource);

            Animate();
            await Task.Delay(1080);
            Player.MediaFailed += (s, ev) => MediaFailedEvent();
            Player.MediaEnded += (s, ev) => MediaFinishedEvent();
            var sb = (Storyboard)FindResource("OpacityDown");
            var clone = sb.Clone();
            clone.Completed += (s, ev) =>
            {
                Middle.Visibility = Visibility.Collapsed;
            };
            sb.Begin(Middle);
        }

        DispatcherTimer sttUpdate = new DispatcherTimer();
        private void MediaFinishedEvent()
        {
            Return();
        }
        private void MediaFailedEvent()
        {
            Return();
        }
        private string GetSpeed(double speed)
        {
            string speedText = speed + " B/s";
            if (speed > 1000)
            {
                speedText = (speed / 1000).ToString("N0") + " kB/s";
            }
            if (speed > 1000000)
            {
                speedText = (speed / 1000000).ToString("N1") + " MB/s";
            }
            if (speed > 1000000000)
            {
                speedText = (speed / 1000000000).ToString("N1") + " GB/s";
            }
            return speedText;
        }

        DispatcherTimer timer;
        bool isRunning = false;
        private void Page_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isRunning)
            {
                isRunning = true;
                var sb = (Storyboard)FindResource("OpacityUp");
                var clone = sb.Clone();
                Mouse.OverrideCursor = null;
                clone.Completed += (s, ev) =>
                {
                    isRunning = false;

                };

                clone.Begin(TopBar);
                clone.Begin(BottomBar);
                //clone.Begin(RightPanel);
                if (timer != null)
                {
                    timer.Tick -= Timer_Tick;
                }
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 2, 500);
                timer.Tick += new EventHandler(Timer_Tick);
                timer.Start();
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            var sb = (Storyboard)FindResource("OpacityDown");
            Mouse.OverrideCursor = Cursors.None;
            sb.Begin(TopBar);
            sb.Begin(BottomBar);
            HideSideBar_MouseUp(null, null);
            //sb.Begin(RightPanel);
        }
        private void Return()
        {
            Helper.EnableScreenSaver();

            if (downloader.Handle.IsSeed)
            {
                downloader.StopAndMove();
            }
            else
            {
                Task.Run(() =>
                {
                    while (downloader.Handle != null && !downloader.Status.IsSeeding) { Thread.Sleep(2000); }
                    downloader.StopAndMove();
                    //NotificationSender sender = new NotificationSender("Stream finished", Helper.GenerateName(downloader.TorrentSource.Series, downloader.TorrentSource.Episode));
                    //sender.Show();
                });
            }
            MainWindow.ShowContent();
            MouseMove -= Page_MouseMove;
            if (timer != null)
            {
                timer.Stop();
            }
            MainWindow.videoPlayback = false;
            MainWindow.SwitchState(MainWindow.PlayerState.Normal);
            MainWindow.RemovePage();
            ////MainWindow.SetPage(new SeriesEpisodes(downloader.TorrentSource.Series));
            Mouse.OverrideCursor = null;
        }

        DispatcherTimer positionUpdate = new DispatcherTimer();
        private void MediaOpenedEvent()
        {
            Player.Play();
            if (positionUpdate.IsEnabled)
            {
                positionUpdate.Stop();
                positionUpdate = new DispatcherTimer();
            }
            positionUpdate.Interval = new TimeSpan(0, 0, 0, 1);
            positionUpdate.Tick += new EventHandler(UpdatePosition);
            positionUpdate.Start();
        }

        private async void UpdatePosition(object sender, EventArgs e)
        {
            var downloaded = await Task.Run(() =>
            {
                return media.GetMediaInfo(file).Duration.TotalSeconds * downloader.Status.Progress;
            });
            //var downloaded = media.GetMediaInfo(file).Duration.TotalSeconds * downloader.Status.Progress;
            var seconds = Player.MediaPosition / 10000000;
            TimeLine.Maximum = downloaded;
            TimeLine.Value = seconds;
            DownloadSpeed.Text = GetSpeed(downloader.Status.DownloadRate);
            UploadSpeed.Text = GetSpeed(downloader.Status.UploadRate);
            SetValue(downloader.Status.Progress);
            CurrentTime.Text = GetTime(seconds) + "/" + GetTime((float)downloaded);
            if (seconds >= downloaded - 5)
            {
                Player.Pause();
                Middle.Visibility = Visibility.Visible;
                Middle.BeginStoryboard((Storyboard)FindResource("OpacityUp"));
                while ((media.GetMediaInfo(file).Duration.TotalSeconds * downloader.Status.Progress) - 10 > seconds)
                {
                    Animate();
                    await Task.Delay(1080);
                }
                var sb = (Storyboard)FindResource("OpacityDown");
                var clone = sb.Clone();
                clone.Completed += (s, ev) =>
                {
                    Middle.Visibility = Visibility.Collapsed;
                };
                sb.Begin(Middle);
                Player.Play();
            }
        }
        private string GetTime(float value)
        {
            long minutes, seconds, hours;
            minutes = seconds = hours = 0;
            hours = Convert.ToInt32(Math.Floor((double)(value / 60 / 60)));
            minutes = Convert.ToInt32(Math.Floor((double)(value / 60 - 60 * hours)));
            seconds = Convert.ToInt32(Math.Floor((double)(value - ((60 * 60 * hours) + (60 * minutes)))));
            string hoursString = hours > 0 ? hours + ":" : "";
            string minutesString = minutes >= 10 ? minutes.ToString() + ":" : "0" + minutes + ":";
            string secondsString = seconds >= 10 ? seconds.ToString() : "0" + seconds;
            return hoursString + minutesString + secondsString;
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Properties.Settings.Default.Volume = Player.Volume = VolumeSlider.Value;
            Task.Run(() =>
            {
                Properties.Settings.Default.Save();
            });
            if (VolumeSlider.Value == VolumeSlider.Minimum)
            {
                Player.Volume = 0;
            }
        }
        private void Animate()
        {
            Storyboard sb = (Storyboard)FindResource("Rotate");
            Storyboard temp = sb.Clone();
            temp.Completed += (s, e) =>
            {
                MiddleIcon.RenderTransform = new RotateTransform(1);
            };
            temp.Begin(MiddleIcon);
        }
        private string GetSource()
        {
            string path = downloader.Status.SavePath + "\\" + downloader.Status.Name;
            if (File.Exists(path))
            {
                return path;
            }
            else if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                string[] fileExtension = new string[7] { ".mkv", ".m4v", ".avi", ".mp4", ".mov", ".wmv", ".flv" };
                foreach (var file in files)
                {
                    //if (Renamer.IsMatchToIdentifiers(file) && fileExtension.Contains(System.IO.Path.GetExtension(file).ToLower()))
                    //{
                    return file;
                    //}
                }
            }
            return null;
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Return();
        }

        DispatcherTimer contentUpdate = new DispatcherTimer();
        private void VideoSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            contentUpdate.Stop();
            contentUpdate = new DispatcherTimer();
            contentUpdate.Interval = new TimeSpan(0, 0, 0, 0, 500);
            contentUpdate.Tick += (s, ev) =>
            {
                Player.MediaPosition = Convert.ToInt64(TimeLine.Value) * 10000000;
                CurrentTime.Text = GetTime(Player.MediaPosition) + "/" + GetTime((float)media.GetMediaInfo(file).Duration.TotalSeconds * downloader.Status.Progress);
            };
            contentUpdate.Start();
            Player.Pause();
            positionUpdate.Stop();
        }

        private void VideoSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            contentUpdate.Stop();
            Player.MediaPosition = Convert.ToInt64(TimeLine.Value) * 10000000;
            if (isPlaying)
            {
                Player.Play();
            }
            positionUpdate.Start();
        }
        public void SetValue(float value)
        {
            DoubleAnimation animation = new DoubleAnimation(value, new TimeSpan(0, 0, 0, 0, 200));
            animation.AccelerationRatio = animation.DecelerationRatio = .5;
            DownloadProgress.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void BackIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            Player.MediaPosition -= 100000000;
        }
        private void GoForward()
        {
            Player.MediaPosition += 100000000;
        }
        double lastValue = 0.85;


        private void Mute()
        {
            if (Player.Volume == 0)
            {
                Player.Volume = lastValue;
            }
            else
            {
                lastValue = Player.Volume;
                Player.Volume = 0;
            }
        }

        private void PlayIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Pause();
        }
        //Yes I do know this can be simplified but that would result in slower pause and I do not want that
        bool isPlaying = true;
        private void Pause()
        {
            if (isPlaying)
            {
                Player.Pause();
                var sb = (Storyboard)FindResource("OpacityDown");
                var clone = sb.Clone();
                clone.Completed += (s, ev) =>
                {
                    ActualPlayIcon.Source = new BitmapImage(new Uri("/Icons/ico-play-light.png", UriKind.Relative));
                    var sboard = (Storyboard)FindResource("OpacityUp");
                    sboard.Begin(PlayIcon);
                };
                clone.Begin(PlayIcon);
            }
            else
            {
                var sb = (Storyboard)FindResource("OpacityDown");
                var clone = sb.Clone();
                clone.Completed += (s, ev) =>
                {
                    ActualPlayIcon.Source = new BitmapImage(new Uri("/Icons/ico-pause-light.png", UriKind.Relative));
                    var sboard = (Storyboard)FindResource("OpacityUp");
                    sboard.Begin(PlayIcon);
                };
                clone.Begin(PlayIcon);
                Player.Play();
            }
            isPlaying = !isPlaying;
        }

        private void ForwardIcon_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GoForward();
        }

        private void AlwaysTopIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchState(MainWindow.PlayerState.PiP);
        }

        private void FullscreenIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
        }

        private void SubtitlesIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RightPanel.Visibility = Hider.Visibility = Visibility.Visible;
            var sb = ((Storyboard)FindResource("OpacityUp"));
            sb.Begin(RightPanel);
            ThicknessAnimation thicc = new ThicknessAnimation(new Thickness(0, 0, 0, 75), new TimeSpan(0, 0, 0, 0, 300));
            RightPanel.BeginAnimation(MarginProperty, thicc);
        }

        List<STFoundSubtitle> allSubtitles = new List<STFoundSubtitle>();
        List<SubtitleItem> subtitles = new List<SubtitleItem>();
        public class STFoundSubtitle
        {
            public STFoundSubtitle(string file)
            {
                File = file;
            }
            public string File { get; set; }
            public string Version { get; set; }
        }

        private async Task LoadSidebar()
        {
            //List<SubtitleItem> subs = new List<SubtitleItem>();
            //CurrentStatus.Text = "Loading info";
            await Task.Run(() =>
            {
                //FFProbe probe = new FFProbe();
                //probe.ToolPath = Environment.GetFolderPath(SpecialFolder.ApplicationData);
                //var info = probe.GetMediaInfo(scannedFile.NewName);
                //var video = info.Streams.Where(x => x.CodecType == "video").FirstOrDefault();
                //var audio = info.Streams.Where(x => x.CodecType == "audio").FirstOrDefault();
                //Dispatcher.Invoke(() => {
                //    EpisodeName.Text = episode.episodeName;
                //    SeriesNumber.Text = Helper.GenerateName(episode);
                //    Framerate.Text = "Framerate: " + video.FrameRate.ToString("##.##");
                //    Resolution.Text = "Resolution: " + video.Width + "x" + video.Height;
                //    VideoCodec.Text = "Video codec: " + video.CodecLongName;
                //    PixelFormat.Text = "Pixel format: " + video.PixelFormat;
                //    AudioCodec.Text = "Audio codec: " + audio.CodecLongName;
                //});
                //var multiple = info.Streams.Where(x => x.CodecType == "subtitle").Where(x => x.CodecName == "srt");
                //var single = multiple.FirstOrDefault();
                //if (single != null)
                //{
                //    FFMpegConverter converter = new FFMpegConverter();
                //    converter.FFMpegToolPath = probe.ToolPath;
                //    Stream str = new MemoryStream();
                //    converter.ConvertMedia(scannedFile.NewName, str, "srt");
                //    FoundSubtitle fs = new FoundSubtitle(str);
                //    fs.Version = "Packed with video";
                //    allSubtitles.Add(fs);
                //}
                //else
                //{
                DirectoryInfo d = new DirectoryInfo(@"C:\Users\ThanhYen\Downloads\TorCACHE");//Assuming Test is your Folder
                FileInfo[] subtitles_Files = d.GetFiles("*.srt"); //Getting Text files
                                                                  //var subtitles = episode.files.Where(x => x.Type == ScannedFile.FileType.Subtitles);
                foreach (var sub in subtitles_Files)
                {
                    STFoundSubtitle fs = new STFoundSubtitle(sub.FullName);
                    //fs.Version = FileVersionInfo.GetVersionInfo(Environment.SystemDirectory).OriginalFilename;
                    allSubtitles.Add(fs);
                }
                //}
                LoadSideBarSubs();

            });
            //CurrentStatus.Text = "";
            RenderSubs();
        }


        private void LoadSideBarSubs()
        {
            foreach (var item in allSubtitles)
            {
                Dispatcher.Invoke(() =>
                {
                    STSubtitlePickerUC picker = new STSubtitlePickerUC(item);
                    picker.Version.Text = System.IO.Path.GetFileNameWithoutExtension(item.File);
                    picker.MouseLeftButtonUp += (s, ev) => SelectSubtitiles(picker);
                    SubtitleSelectionPanel.Children.Add(picker);
                });

            }
            Dispatcher.Invoke(() =>
            {
                STSubtitlePickerUC toLoad = null;
                foreach (STSubtitlePickerUC pick in SubtitleSelectionPanel.Children)
                {
                    //if (pick.Subtitle.Stream != null)
                    //{
                    //    toLoad = pick;
                    //}
                }
                if (toLoad == null && SubtitleSelectionPanel.Children.Count > 0)
                {
                    toLoad = (STSubtitlePickerUC)SubtitleSelectionPanel.Children[0];
                }
                if (toLoad != null)
                {
                    SelectSubtitiles(toLoad);
                }
            });
        }

        private void SelectSubtitiles(STSubtitlePickerUC picker)
        {
            if (picker.StreanSubtitle.File != null)
            {
                var lines = Subtitles.ParseSubtitleItems(picker.StreanSubtitle.File);
                if (lines.Count > 1)
                {
                    subtitles = lines;
                }
            }
            //else if (picker.Subtitle.Stream != null)
            //{
            //    picker.Subtitle.Stream.Position = 0;
            //    var lines = Subtitles.ParseSubtitleItems(new StreamReader(picker.Subtitle.Stream).ReadToEnd(), ".srt");
            //    if (lines.Count > 1)
            //    {
            //        subtitles = lines;
            //    }
            //}
            NoSubtitlesSt.ItemSelected.Opacity = 0;
            int index = SubtitleSelectionPanel.Children.IndexOf(picker);
            picker.ItemSelected.Opacity = 1;
            foreach (STSubtitlePickerUC pick in SubtitleSelectionPanel.Children)
            {
                if (SubtitleSelectionPanel.Children.IndexOf(pick) != index)
                {
                    pick.ItemSelected.Opacity = 0;
                }
            }
        }
        int SubtitleSize = 30;
        private void SubtitleSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            SubtitleSize = (int)SubtitleSlider.Value;
        }
        private void RenderSubs()
        {
            int index = 0;
            long position = 0;
            bool isLoaded = IsLoaded;
            Task.Run(async () =>
            {
                await Task.Delay(500);
                while (true)
                {
                    var subtitles = this.subtitles;
                    if (subtitles.Count > 1)
                    {
                        Dispatcher.Invoke(() => { position = GetMiliseconds(Player.MediaPosition); });
                        while (!(subtitles[index].StartTime < position && subtitles[index + 1].StartTime > position))
                        {
                            Dispatcher.Invoke(() => { position = GetMiliseconds(Player.MediaPosition); });
                            if (subtitles[index].StartTime < position)
                            {
                                index++;
                            }
                            else if (subtitles[index].StartTime > position)
                            {
                                // neu index = 0 index-- am nen bi loi sub
                                if (index > 0)
                                {
                                    index--;
                                }
                            }
                        }
                        if (subtitles[index].EndTime >= position)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                foreach (var line in subtitles[index].Lines)
                                {
                                    if (!SubtitlePanel.Children.Contains(line))
                                    {
                                        foreach (TextBlock childElement in line.Children)
                                        {
                                            childElement.FontSize = SubtitleSize > 20 ? SubtitleSize : 36;
                                        }
                                        SubtitlePanel.Children.Add(line);
                                    }
                                }
                            }, DispatcherPriority.Send);
                        }
                        while (subtitles[index].EndTime >= position)
                        {
                            Dispatcher.Invoke(() => { position = GetMiliseconds(Player.MediaPosition); });
                            await Task.Delay(5);
                            if (subtitles[index].StartTime > position)
                            {
                                break;
                            }
                        }
                        Dispatcher.Invoke(() =>
                        {
                            SubtitlePanel.Children.Clear();
                        }, DispatcherPriority.Send);
                        await Task.Delay(1);
                    }
                    else
                    {
                        index = 0;
                        await Task.Delay(500);
                    }
                    Dispatcher.Invoke(() => { isLoaded = false; });
                }
            });
        }
        private long GetMiliseconds(long value)
        {
            return value / 10000;
        }

        private void HideSideBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var sb = ((Storyboard)FindResource("OpacityDown")).Clone();
            sb.Completed += (s, ev) =>
            {
                RightPanel.Visibility = Hider.Visibility = Visibility.Hidden;
            };
            sb.Begin(RightPanel);
            ThicknessAnimation thicc = new ThicknessAnimation(new Thickness(0, 0, -250, 0), new TimeSpan(0, 0, 0, 0, 300));
            RightPanel.BeginAnimation(MarginProperty, thicc);

        }

        private void NoSubtitlesSt_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NoSubtitlesSt.ItemSelected.Opacity = 1;
            foreach (STSubtitlePickerUC pick in SubtitleSelectionPanel.Children)
            {
                pick.ItemSelected.Opacity = 0;
            }
            subtitles = new List<SubtitleItem>();
        }

        private void Page_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
            }
        }

        private void StreamPage_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    Pause();
                    break;
                case Key.Escape:
                    Return();
                    break;
                case Key.F:
                    MainWindow.SwitchState(MainWindow.PlayerState.Fullscreen);
                    break;
                case Key.M:
                    Mute();
                    break;
                case Key.Right:
                    GoForward();
                    break;
                case Key.Left:
                    GoBack();
                    break;
            }
            e.Handled = true;
        }
    }
}
