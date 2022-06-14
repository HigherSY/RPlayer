using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace RPlayer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        VLC vlc;
        ObservableCollection<MyMedia> mediaList = new ObservableCollection<MyMedia>();
        public MainWindow()
        {
            InitializeComponent();
            vlc = new VLC("--verbose", "--no-video");

            vlc.Player.Playing += Player_UpdateState;
            vlc.Player.Paused += Player_UpdateState;
            vlc.Player.Stopped += Player_UpdateState;
            vlc.Player.EndReached += Player_EndReached;
            vlc.Player.PositionChanged += Player_PositionChanged;
            vlc.Player.TimeChanged += Player_TimeChanged;
            vlc.Player.LengthChanged += Player_LengthChanged;
            dgMedia.ItemsSource = mediaList;
        }

        private void Player_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                lTotPos.Content = TimeSpan.FromMilliseconds(vlc.Player.Media.Duration).ToString("c");
            });
        }

        private void Player_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                lCurPos.Content = TimeSpan.FromMilliseconds(vlc.Player.Time).ToString(@"hh\:mm\:ss");
            });
        }

        private void Player_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                sPos.Value = vlc.Player.Position;
            });
        }

        private void Player_EndReached(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(PlayNext);
        }

        private void PlayNext(object _)
        {
            for (int i = 0; i < mediaList.Count; i++)
            {
                var m = mediaList[i];
                if (m.VLCMedia.NativeReference == vlc.Player.Media.NativeReference)
                {
                    switch (m.EndAction)
                    {
                        case MediaEndAction.Stop:
                            break;
                        case MediaEndAction.Next:
                            if (i + 1 < mediaList.Count) vlc.Player.Play(mediaList[i + 1].VLCMedia);
                            else vlc.Player.Play(mediaList[0].VLCMedia);
                            break;
                        case MediaEndAction.Loop:
                            vlc.Player.Play(m.VLCMedia);
                            break;
                    }
                    break;
                }
            }
        }

        private void Player_UpdateState(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                switch (vlc.Player.State)
                {
                    case VLCState.Playing:
                        bPlay.IsEnabled = false;
                        bPause.IsEnabled = true;
                        break;
                    case VLCState.Paused:
                        bPause.IsEnabled = false;
                        bPlay.IsEnabled = true;
                        break;
                    default:
                        bPause.IsEnabled = true;
                        bPlay.IsEnabled = true;
                        break;
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void bAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog
            {
                Filter = "音乐文件|*.mp3;*.flac;*.wav;|RPlayer 播放列表|*.xml"
            };

            if (d.ShowDialog() == true)
            {
                switch (d.FilterIndex)
                {
                    case 1:
                        mediaList.Add(new MyMedia(vlc.LibVLC, d.FileName, d.SafeFileName));
                        break;
                    case 2:
                        loadMediaListXml(d.FileName);
                        break;
                }
            }
        }

        private void loadMediaListXml(string xmlPath)
        {
            XElement x = XElement.Load(xmlPath);
            foreach(var e in x.Elements("Media"))
            {
                var m = new MyMedia(vlc.LibVLC, e.Attribute("Path").Value, e.Attribute("Name").Value);
                m.EndAction = (MediaEndAction)Enum.Parse(typeof(MediaEndAction), e.Attribute("EndAction").Value);
                mediaList.Add(m);
            }
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog
            {
                Filter = "RPlayer 播放列表|*.xml"
            };

            if (d.ShowDialog() != true) return;
            using (XmlWriter w = new XmlTextWriter(d.FileName, Encoding.UTF8))
            {
                w.WriteStartDocument();
                w.WriteStartElement("MediaList");
                foreach (MyMedia m in mediaList)
                {
                    w.WriteStartElement("Media");
                    w.WriteAttributeString("Name", m.Name);
                    w.WriteAttributeString("Path", m.FilePath);
                    w.WriteAttributeString("EndAction", Enum.GetName(typeof(MediaEndAction), m.EndAction));
                    w.WriteEndElement();
                }
                w.WriteEndElement();
                w.WriteEndDocument();
                w.Close();
            }
        }

        private void lbMute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbMute.SelectedIndex != -1)
            {
                vlc.Player.Mute = true;
            }
            else
            {
                vlc.Player.Mute = false;
            }
        }

        private void bPlay_Click(object sender, RoutedEventArgs e)
        {
            vlc.Player.Play();
        }

        private void bPause_Click(object sender, RoutedEventArgs e)
        {
            vlc.Player.Pause();
        }

        private void bStop_Click(object sender, RoutedEventArgs e)
        {
            vlc.Player.Stop();
        }

        private void bReplay_Click(object sender, RoutedEventArgs e)
        {
            vlc.Player.SeekTo(TimeSpan.Zero);
        }

        private void bPlayItem_Click(object sender, RoutedEventArgs e)
        {
            MyMedia row = ((Button)sender).DataContext as MyMedia;
            vlc.Player.Play(row.VLCMedia);
        }

        private void cbEndAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MyMedia row = ((ComboBox)sender).DataContext as MyMedia;
            MediaEndAction uiState = (MediaEndAction)((ComboBox)sender).SelectedIndex;
            if (row.EndAction != uiState)
            {
                row.EndAction = uiState;
            }
        }

        private void cbEndAction_Loaded(object sender, RoutedEventArgs e)
        {
            MyMedia row = ((ComboBox)sender).DataContext as MyMedia;
            int state = (int)row.EndAction;
            if (((ComboBox)sender).SelectedIndex != state)
            {
                ((ComboBox)sender).SelectedIndex = state;
            }
        }

        private void bMoveDown_Click(object sender, RoutedEventArgs e)
        {
            MyMedia row = ((Button)sender).DataContext as MyMedia;
            int i = mediaList.IndexOf(row);
            if (i + 1 < mediaList.Count) mediaList.Move(i, i + 1);
        }

        private void bMoveUp_Click(object sender, RoutedEventArgs e)
        {
            MyMedia row = ((Button)sender).DataContext as MyMedia;
            int i = mediaList.IndexOf(row);
            if (i > 0) mediaList.Move(i, i - 1);
        }

        private void bItemDelete_Click(object sender, RoutedEventArgs e)
        {
            MyMedia row = ((Button)sender).DataContext as MyMedia;
            mediaList.Remove(row);
            row.Dispose();
        }

        private void dgMedia_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] d = e.Data.GetData(DataFormats.FileDrop) as string[];
                foreach(string p in d)
                {
                    if (Path.GetExtension(p) == ".xml") loadMediaListXml(p);
                    else mediaList.Add(new MyMedia(vlc.LibVLC, p, Path.GetFileName(p)));
                }
            }
        }
    }
}
