using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RPlayer
{
    public class MyMedia : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName = null)
           => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public Media VLCMedia;

        private string name;
        public string Name
        {
            get => name;
            set
            {
                SetField(ref name, value);
            }
        }

        private string duration;
        public string Duration
        {
            get => duration;
        }

        private VLCState state;
        private bool disposedValue;

        public VLCState State
        {
            get => state;
        }

        public MediaEndAction EndAction { get; set; }

        internal MyMedia(LibVLC vlc, string filePath, string safeFileName)
        {
            Name = safeFileName;
            VLCMedia = new Media(vlc, filePath, FromType.FromPath, ":no-video");
            VLCMedia.DurationChanged += VLCMedia_DurationChanged;
            VLCMedia.StateChanged += VLCMedia_StateChanged;
            Task.Run(() =>
            {
                VLCMedia.Parse();
            });
        }

        private void VLCMedia_StateChanged(object sender, MediaStateChangedEventArgs e)
        {
            if (state == VLCState.Error) return;
            state = e.State;
            OnPropertyChanged("State");
        }

        private void VLCMedia_DurationChanged(object sender, MediaDurationChangedEventArgs e)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(e.Duration);
            duration = ts.ToString("c");
            OnPropertyChanged("Duration");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                VLCMedia.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~MyMedia()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
