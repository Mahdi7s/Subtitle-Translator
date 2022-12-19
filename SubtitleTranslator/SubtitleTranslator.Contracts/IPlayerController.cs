using System;

namespace SubtitleTranslator.Contracts
{
    public interface IPlayerController
    {
        void PlayOrPause();
        bool IsPlaying();
        bool IsStopped();
        bool IsPaused();
        TimeSpan GetCurrentPosition();
        TimeSpan GetDuration();
        void PlayFromBeginning();
        void NextTrack();
        void PrevTrack();
        void Seek(TimeSpan time);
    }
}