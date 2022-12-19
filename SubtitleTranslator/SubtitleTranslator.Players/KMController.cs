using System;
using System.Collections.Generic;
using System.Text;
using SubtitleTranslator.Contracts;

namespace SubtitleTranslator.Players
{
    public class KMController : IPlayerController
    {
        private readonly KMPlayer _km;

        public KMController()
        {
            _km = new KMPlayer();
        }

        public void PlayOrPause()
        {
            _km.Play();
        }

        public bool IsPlaying()
        {
            return _km.IsPlaying();
        }

        public bool IsStopped()
        {
            return _km.IsStopped();
        }

        public bool IsPaused()
        {
            return _km.IsPaused();
        }

        public TimeSpan GetCurrentPosition()
        {
            return TimeSpan.FromMilliseconds(_km.GetPos());
        }

        public TimeSpan GetDuration()
        {
            return TimeSpan.FromSeconds(_km.GetLength());
        }

        public void PlayFromBeginning()
        {
            _km.PlayFromBeginning();
        }

        public void NextTrack()
        {
            _km.NextTrack();
        }

        public void PrevTrack()
        {
            _km.PreviousTrack();
        }

        public void Seek(TimeSpan time)
        {
            _km.Seek((int) time.TotalSeconds);
        }
    }
}
