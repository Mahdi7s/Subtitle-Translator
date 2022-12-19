using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SubtitleTranslator.Contracts;

namespace SubtitleTranslator.Players
{
    public class WMPController : IPlayerController
    {
        public void PlayOrPause()
        {
            throw new NotImplementedException();
        }

        public bool IsPlaying()
        {
            throw new NotImplementedException();
        }

        public bool IsStopped()
        {
            throw new NotImplementedException();
        }

        public bool IsPaused()
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetCurrentPosition()
        {
            throw new NotImplementedException();
        }

        public TimeSpan GetDuration()
        {
            throw new NotImplementedException();
        }

        public void PlayFromBeginning()
        {
            throw new NotImplementedException();
        }

        public void NextTrack()
        {
            throw new NotImplementedException();
        }

        public void PrevTrack()
        {
            throw new NotImplementedException();
        }

        public void Seek(TimeSpan time)
        {
            throw new NotImplementedException();
        }
    }
}
