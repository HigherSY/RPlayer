using LibVLCSharp.Shared;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RPlayer
{
    internal class VLC : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
           => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value,
            [CallerMemberName] string propertyName = null)
        {
            // if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        internal LibVLC LibVLC;
        internal MediaPlayer Player;

        internal bool Mute => Player.Mute;
        internal bool IsPlaying => Player.IsPlaying;
        internal VLCState State => Player.State;

        internal VLC(params string[] vlcOptions)
        {
            LibVLC = new LibVLC(vlcOptions);
            Player = new MediaPlayer(LibVLC);

            Player.Muted += Player_Muted;

            Player.NothingSpecial += Player_StateChanged;
            Player.Opening += Player_StateChanged;
            Player.Buffering += Player_StateChanged;
            Player.Playing += Player_StateChanged;
            Player.Paused += Player_StateChanged;
            Player.Stopped += Player_StateChanged;
            Player.EndReached += Player_StateChanged;
            Player.EncounteredError += Player_StateChanged;
        }

        private void Player_StateChanged(object sender, System.EventArgs e)
        {
            OnPropertyChanged("IsPlaying");
            OnPropertyChanged("State");
        }

        private void Player_Muted(object sender, System.EventArgs e)
        {
            OnPropertyChanged("Mute");
        }

        internal void PlayFile(string filePath)
        {
            Media m = new Media(LibVLC, filePath, FromType.FromPath);
            Player.Play(m);
        }
    }
}
