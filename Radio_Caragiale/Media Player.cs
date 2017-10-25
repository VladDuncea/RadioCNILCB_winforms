/*---------------------------*/
/* ECN Media Player
 * Copyright Edward Nutting © 2010
 * Please do not delete this! 
 * 
 * This class is open to use and adaptation, however, if you could spare
 * the time to email me with the code change or the concept of what you
 * change I would greatly appreciate it.
 * 
 * Email : EdMan196@hotmail.co.uk
 */
/*---------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMPLib;

namespace Radio_Caragiale
{
    internal enum Players
    {
        NULL,
        Player1,
        Player2
    }
    /// <summary>
    /// The different states the player can be in.
    /// </summary>
    public enum PlayerStates
    {
        NULL,
        Stopped,
        Paused,
        Playing,
        Loading,
        Ready,
        Error
    }
    internal enum FadeStates
    {
        In,
        Out,
        Down,
        Up
    }

    public class MediaPlayer
    {
        private const int CrossfadeTimerTickTime = 500;
        private const int FadeTimerTickTime = 500;

        WindowsMediaPlayerClass Player1 = new WindowsMediaPlayerClass();
        WindowsMediaPlayerClass Player2 = new WindowsMediaPlayerClass();

        Players PlayingPlayer = Players.NULL;
        Players StoppingPlayer = Players.NULL;

        PlayerStates Player1State = PlayerStates.NULL;
        PlayerStates Player2State = PlayerStates.NULL;

        string Player1Song;
        string Player2Song;

        /// <summary>
        /// The filename of the current playing track.
        /// </summary>
        public string CurrentSong
        {
            get
            {
                try
                {
                    switch (PlayingPlayer)
                    {
                        case Players.Player1:
                            {
                                return Player1Song;
                            }
                            break;
                        case Players.Player2:
                            {
                                return Player2Song;
                            }
                            break;
                    }
                    switch (StoppingPlayer)
                    {
                        case Players.Player1:
                            {
                                return Player1Song;
                            }
                            break;
                        case Players.Player2:
                            {
                                return Player2Song;
                            }
                            break;
                    }
                }
                catch
                {
                }
                return null;
            }
            set
            {
                if (PlayerState == PlayerStates.Playing || PlayerState == PlayerStates.Paused)
                {
                    switch (PlayingPlayer)
                    {
                        case Players.Player1:
                            Player1Song = value;
                            break;
                        case Players.Player2:
                            Player2Song = value;
                            break;
                    }
                }
                else
                {
                    Play(value, Crossfade);
                }
            }
        }

        public PlayerStates PlayerState = PlayerStates.NULL;

        /// <summary>
        /// The position of the player in seconds. Can be set to change the position. 
        /// Can only be set when player is playing or paused.
        /// Cannot be set if player is performing fade or crossfade.
        /// </summary>
        public int Position
        {
            get
            {
                try
                {
                    switch (PlayingPlayer)
                    {
                        case Players.Player1:
                            {
                                return (int)Player1.currentPosition;
                            }
                            break;
                        case Players.Player2:
                            {
                                return (int)Player2.currentPosition;
                            }
                            break;
                    }
                    switch (StoppingPlayer)
                    {
                        case Players.Player1:
                            {
                                return (int)Player1.currentPosition;
                            }
                            break;
                        case Players.Player2:
                            {
                                return (int)Player2.currentPosition;
                            }
                            break;
                    }
                }
                catch
                {
                }
                return -1;
            }
            set
            {
                if (PlayerState == PlayerStates.Playing || PlayerState == PlayerStates.Paused)
                {
                    if (!InFade && !InCrossfade)
                    {
                        switch (PlayingPlayer)
                        {
                            case Players.Player1:
                                Player1.currentPosition = value;
                                break;
                            case Players.Player2:
                                Player2.currentPosition = value;
                                break;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// The duration of the current song in seconds.
        /// </summary>
        public int Duration
        {
            get
            {
                try
                {
                    switch (PlayingPlayer)
                    {
                        case Players.Player1:
                            {
                                return (int)Player1.currentMedia.duration;
                            }
                            break;
                        case Players.Player2:
                            {
                                return (int)Player2.currentMedia.duration;
                            }
                            break;
                    }
                    switch (StoppingPlayer)
                    {
                        case Players.Player1:
                            {
                                return (int)Player1.currentMedia.duration;
                            }
                            break;
                        case Players.Player2:
                            {
                                return (int)Player2.currentMedia.duration;
                            }
                            break;
                    }
                }
                catch
                {
                }
                return 0;
            }
        }

        private int volume = 0;
        /// <summary>
        /// The volume the player should play at.
        /// Cannot be set if player is performing fade or crossfade.
        /// </summary>
        public int Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if (value <= 100 && value >= 0)
                {
                    volume = value;
                }
                else if (volume >= 100)
                {
                    volume = 100;
                }
                else
                {
                    volume = 0;
                }
                if (!InCrossfade && !InFade)
                {
                    switch (PlayingPlayer)
                    {
                        case Players.Player1:
                            Player1.volume = volume;
                            break;
                        case Players.Player2:
                            Player2.volume = volume;
                            break;
                        case Players.NULL:
                            Player1.volume = volume;
                            Player2.volume = volume;
                            break;
                    }
                }
            }
        }

        private float CrossfadeVolumeAdjustment = 0;
        private float FadeVolumeAdjustment = 0;

        /// <summary>
        /// Whether the player should crossfade or not.
        /// </summary>
        public bool Crossfade = false;
        /// <summary>
        /// The length of time to crossfade over.
        /// </summary>
        public int CrossfadeTime = 5;
        /// <summary>
        /// The length of time to fade over.
        /// </summary>
        public int FadeTime = 5;

        private int CrossfadeTotalRunTime = 0;
        private int FadeTotalRunTime = 0;

        private bool InCrossfade = false;
        private bool InFade = false;

        /// <summary>
        /// Whether the player is fading/crossfading.
        /// </summary>
        public bool inFade
        {
            get
            {
                return InFade || InCrossfade;
            }
        }

        private int NewVolume = 0;

        private FadeStates FadeState = FadeStates.In;

        private System.Windows.Forms.Timer CrossfadeTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer FadeTimer = new System.Windows.Forms.Timer();

        /// <summary>
        /// Fired when player starts playing.
        /// </summary>
        public event PlayerStartEvent OnPlayerStart;
        /// <summary>
        /// Fired when player stops.
        /// </summary>
        public event PlayerStopEvent OnPlayerStop;
        /// <summary>
        /// Fires when player pauses.
        /// </summary>
        public event PlayerPausedEvent OnPlayerPaused;
        /// <summary>
        /// Fired when player media ends.
        /// </summary>
        public event PlayerMediaEndedEvent OnPlayerMediaEnd;

        private event PlayerReadyEvent OnPlayerReady;
        private event PlayerErrorEvent OnPlayerError;

        /// <summary>
        /// Fired when an error occurs.
        /// </summary>
        public event ErrorEvent OnError;

        public MediaPlayer()
        {
            Init();
        }

        public MediaPlayer(int InitVolume, bool UseCrossfade)
        {
            Init();

            Crossfade = UseCrossfade;
            Volume = InitVolume;
        }
        public MediaPlayer(int InitVolume, bool UseCrossfade, int TheCrossfadeTime)
        {
            Init();

            Crossfade = UseCrossfade;
            CrossfadeTime = TheCrossfadeTime;
            Volume = InitVolume;
        }

        private void Init()
        {
            Player1.PlayStateChange += new _WMPOCXEvents_PlayStateChangeEventHandler(Player1_PlayStateChange);
            Player2.PlayStateChange += new _WMPOCXEvents_PlayStateChangeEventHandler(Player2_PlayStateChange);

            OnPlayerStart += new PlayerStartEvent(MediaPlayer_OnPlayerStart);
            OnPlayerStop += new PlayerStopEvent(MediaPlayer_OnPlayerStop);
            OnPlayerPaused += new PlayerPausedEvent(MediaPlayer_OnPlayerPaused);

            OnPlayerReady += new PlayerReadyEvent(MediaPlayer_OnPlayerReady);
            OnPlayerMediaEnd += new PlayerMediaEndedEvent(MediaPlayer_OnPlayerMediaEnd);
            OnPlayerError += new PlayerErrorEvent(MediaPlayer_OnPlayerError);

            CrossfadeTimer.Interval = CrossfadeTimerTickTime;
            CrossfadeTimer.Enabled = false;
            CrossfadeTimer.Tick += new EventHandler(CrossfadeTimer_Tick);

            FadeTimer.Interval = FadeTimerTickTime;
            FadeTimer.Enabled = false;
            FadeTimer.Tick += new EventHandler(FadeTimer_Tick);

        }

        private void MediaPlayer_OnPlayerStart(PlayerStartEventArgs e)
        {
            try
            {
                switch (e.ThePlayer)
                {
                    case Players.Player1:
                        Player1State = PlayerStates.Playing;
                        break;
                    case Players.Player2:
                        Player2State = PlayerStates.Playing;
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("OnPlayerStart : " + ex.Message));
            }
        }
        private void MediaPlayer_OnPlayerStop(PlayerStopEventArgs e)
        {
            try
            {
                switch (e.ThePlayer)
                {
                    case Players.Player1:
                        Player1State = PlayerStates.Stopped;
                        break;
                    case Players.Player2:
                        Player2State = PlayerStates.Stopped;
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("OnPlayerStop : " + ex.Message));
            }
        }
        private void MediaPlayer_OnPlayerPaused(PlayerPausedEventArgs e)
        {
            try
            {
                switch (e.ThePlayer)
                {
                    case Players.Player1:
                        Player1State = PlayerStates.Paused;
                        break;
                    case Players.Player2:
                        Player2State = PlayerStates.Paused;
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("OnPlayerPaused : " + ex.Message));
            }
        }

        private void MediaPlayer_OnPlayerReady(PlayerReadyEventArgs e)
        {
            try
            {
                switch (e.ThePlayer)
                {
                    case Players.Player1:
                        Player1State = PlayerStates.Ready;
                        break;
                    case Players.Player2:
                        Player2State = PlayerStates.Ready;
                        break;
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("OnPlayerPaused : " + ex.Message));
            }
        }
        private void MediaPlayer_OnPlayerMediaEnd(PlayerMediaEndedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("OnPlayerMediaEnded : " + ex.Message));
            }
        }
        private void MediaPlayer_OnPlayerError(PlayerErrorEventArgs e)
        {
            try
            {
                switch (e.ThePlayer)
                {
                    case Players.Player1:
                        Player1State = PlayerStates.Error;
                        break;
                    case Players.Player2:
                        Player2State = PlayerStates.Error;
                        break;
                }
                OnError.Invoke(new ErrorEventArgs("Player " + e.ThePlayer.ToString() + " error - StateNum = " + e.StateNum.ToString()));
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("OnPlayerError : " + ex.Message));
            }
        }

        private void Player1_PlayStateChange(int NewState)
        {
            PlayerStateChange(NewState, Player1Song, Players.Player1);
        }
        private void Player2_PlayStateChange(int NewState)
        {
            PlayerStateChange(NewState, Player2Song, Players.Player2);
        }

        private void PlayerStateChange(int NewState, string CurrentSong, Players ThePlayer)
        {
            try
            {
                switch (NewState)
                {
                    case 0:    // Undefined
                        OnPlayerError.Invoke(new PlayerErrorEventArgs(ThePlayer, 0));
                        break;

                    case 1:    // Stopped
                        if (!Crossfade && !InFade)
                        {
                            OnPlayerStop.Invoke(new PlayerStopEventArgs(CurrentSong, ThePlayer));
                        }
                        break;

                    case 2:    // Paused
                        OnPlayerPaused.Invoke(new PlayerPausedEventArgs(CurrentSong, ThePlayer));
                        break;

                    case 3:    // Playing
                        OnPlayerStart.Invoke(new PlayerStartEventArgs(CurrentSong, ThePlayer));
                        break;

                    case 4:    // ScanForward
                        break;

                    case 5:    // ScanReverse
                        break;

                    case 6:    // Buffering
                        switch (ThePlayer)
                        {
                            case Players.Player1:
                                Player1State = PlayerStates.Loading;
                                break;
                            case Players.Player2:
                                Player2State = PlayerStates.Loading;
                                break;
                        }
                        break;

                    case 7:    // Waiting
                        break;

                    case 8:    // MediaEnded
                        OnPlayerMediaEnd.Invoke(new PlayerMediaEndedEventArgs(CurrentSong));
                        break;

                    case 9:    // Transitioning
                        break;

                    case 10:   // Ready
                        OnPlayerReady.Invoke(new PlayerReadyEventArgs(ThePlayer));
                        break;

                    case 11:   // Reconnecting
                        break;

                    case 12:   // Last
                        break;

                    default:
                        OnPlayerError.Invoke(new PlayerErrorEventArgs(ThePlayer, -1));
                        break;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Starts the player playing the specified file.
        /// </summary>
        /// <param name="TheSong">The filename of the song to play.</param>
        /// <param name="Fade">Whether to fade or not.</param>
        /// <returns>Returns true if player starts playing</returns>
        public bool Play(string TheSong, bool Fade)
        {
            try
            {
                if (PlayerState != PlayerStates.Paused)
                {
                    if (PlayingPlayer == Players.NULL)
                    {
                        if (!InCrossfade && !InFade)
                        {
                            if (Fade)
                            {
                                PlayingPlayer = Players.Player1;
                                Player1Song = TheSong;
                                Player1.URL = TheSong;

                                FadeState = FadeStates.In;
                                StartFade();
                            }
                            else
                            {
                                PlayingPlayer = Players.Player1;
                                Player1Song = TheSong;
                                Player1.URL = TheSong;
                                Player1.volume = Volume;
                                Player1.play();
                                PlayerState = PlayerStates.Playing;
                                return true;
                            }
                        }
                    }
                    else if (Crossfade)
                    {
                        if (!InCrossfade && !InFade)
                        {
                            if (PlayingPlayer == Players.Player1)
                            {
                                Player2Song = TheSong;
                                StoppingPlayer = Players.Player1;
                                StartCrossfade();
                                return true;
                            }
                            else if (PlayingPlayer == Players.Player2)
                            {
                                Player1Song = TheSong;
                                StoppingPlayer = Players.Player2;
                                StartCrossfade();
                                return true;
                            }
                            else
                            {
                                if (Fade)
                                {
                                    PlayingPlayer = Players.Player1;
                                    Player1Song = TheSong;
                                    Player1.URL = TheSong;

                                    FadeState = FadeStates.In;
                                    StartFade();
                                }
                                else
                                {
                                    PlayingPlayer = Players.Player1;
                                    Player1Song = TheSong;
                                    Player1.URL = TheSong;
                                    Player1.volume = Volume;
                                    Player1.play();
                                    PlayerState = PlayerStates.Playing;
                                    return true;
                                }
                            }
                        }
                    }
                    else if (!InFade)
                    {
                        if (Fade)
                        {
                            PlayingPlayer = Players.Player1;
                            Player1Song = TheSong;
                            Player1.URL = TheSong;

                            FadeState = FadeStates.In;
                            StartFade();
                        }
                        else
                        {
                            PlayingPlayer = Players.Player1;
                            Player1Song = TheSong;
                            Player1.URL = TheSong;
                            Player1.volume = Volume;
                            Player1.play();
                            PlayerState = PlayerStates.Playing;
                            return true;
                        }
                    }
                }
                else if (PlayerState == PlayerStates.Paused)
                {
                    Resume(Fade);
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("Play : " + ex.Message));
            }
            return false;
        }
        /// <summary>
        /// Starts the player playing the paused file.
        /// </summary>
        /// <param name="Fade">Whether to fade or not.</param>
        /// <returns>Returns true if player starts playing</returns>
        public bool Resume(bool Fade)
        {
            try
            {
                if (PlayerState == PlayerStates.Paused)
                {
                    if (!InFade && !InCrossfade)
                    {
                        if (Fade)
                        {
                            FadeState = FadeStates.In;
                            switch (StoppingPlayer)
                            {
                                case Players.Player1:
                                    PlayingPlayer = Players.Player1;
                                    break;
                                case Players.Player2:
                                    PlayingPlayer = Players.Player2;
                                    break;
                            }
                            StartFade();
                        }
                        else
                        {
                            switch (PlayingPlayer)
                            {
                                case Players.Player1:
                                    PlayingPlayer = Players.Player1;
                                    Player1.play();
                                    break;
                                case Players.Player2:
                                    PlayingPlayer = Players.Player2;
                                    Player2.play();
                                    break;
                            }
                        }
                        PlayerState = PlayerStates.Playing;
                    }
                }
            }
            catch
            {
            }
            return false;
        }
        /// <summary>
        /// Stops the player playing.
        /// </summary>
        /// <param name="Fade">Whether to fade or not.</param>
        /// <returns>Returns true if player stops playing</returns>
        public bool Stop(bool Fade)
        {
            try
            {
                if (PlayerState == PlayerStates.Playing)
                {
                    if (!InCrossfade && !InFade)
                    {
                        if (Fade)
                        {
                            switch (PlayingPlayer)
                            {
                                case Players.Player1:
                                    StoppingPlayer = Players.Player1;
                                    PlayingPlayer = Players.NULL;
                                    break;
                                case Players.Player2:
                                    StoppingPlayer = Players.Player2;
                                    PlayingPlayer = Players.NULL;
                                    break;
                            }
                            FadeState = FadeStates.Out;
                            StartFade();
                            return true;
                        }
                        else
                        {
                            switch (PlayingPlayer)
                            {
                                case Players.Player1:
                                    StoppingPlayer = Players.Player1;
                                    PlayingPlayer = Players.NULL;
                                    Player1.stop();
                                    break;
                                case Players.Player2:
                                    StoppingPlayer = Players.Player2;
                                    PlayingPlayer = Players.NULL;
                                    Player2.stop();
                                    break;
                            }
                            PlayerState = PlayerStates.Stopped;
                            return true;
                        }
                    }
                }
                else if (PlayerState == PlayerStates.Paused)
                {
                    switch (PlayingPlayer)
                    {
                        case Players.Player1:
                            StoppingPlayer = Players.Player1;
                            PlayingPlayer = Players.NULL;
                            Player1.stop();
                            break;
                        case Players.Player2:
                            StoppingPlayer = Players.Player2;
                            PlayingPlayer = Players.NULL;
                            Player2.stop();
                            break;
                    }
                    PlayerState = PlayerStates.Stopped;
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("Stop : " + ex.Message));
            }
            return false;
        }
        /// <summary>
        /// Pauses the player.
        /// </summary>
        /// <param name="Fade">Whether to fade or not.</param>
        /// <returns>Returns true if player pauses playing</returns>
        public bool Pause(bool Fade)
        {
            try
            {
                if (!InFade && !InCrossfade)
                {
                    if (PlayerState == PlayerStates.Playing)
                    {
                        if (Fade)
                        {
                            switch (PlayingPlayer)
                            {
                                case Players.Player1:
                                    StoppingPlayer = Players.Player1;
                                    PlayingPlayer = Players.NULL;
                                    break;
                                case Players.Player2:
                                    StoppingPlayer = Players.Player2;
                                    PlayingPlayer = Players.NULL;
                                    break;
                            }
                            FadeState = FadeStates.Out;
                            StartFade();
                        }
                        else
                        {
                            switch (PlayingPlayer)
                            {
                                case Players.Player1:
                                    Player1.pause();
                                    break;
                                case Players.Player2:
                                    Player2.pause();
                                    break;
                            }
                        }
                        PlayerState = PlayerStates.Paused;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("Pause : " + ex.Message));
            }
            return false;
        }

        private void StartCrossfade()
        {
            InCrossfade = true;
            CrossfadeVolumeAdjustment = ((float)Volume / (float)CrossfadeTime);
            CrossfadeTotalRunTime = 0;
            PlayerState = PlayerStates.Playing;
            CrossfadeTimer.Enabled = true;
            CrossfadeTimer.Start();
        }
        private void CrossfadeTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (CrossfadeTotalRunTime <= 0)
                {
                    switch (StoppingPlayer)
                    {
                        case Players.Player1:
                            PlayingPlayer = Players.Player2;
                            Player2.volume = 0;
                            Player2.URL = Player2Song;
                            Player2.play();
                            break;
                        case Players.Player2:
                            PlayingPlayer = Players.Player1;
                            Player1.volume = 0;
                            Player1.URL = Player1Song;
                            Player1.play();
                            break;
                    }
                }
                else if (CrossfadeTotalRunTime < (CrossfadeTime * 1000))
                {
                    switch (PlayingPlayer)
                    {
                        case Players.Player1:
                            {
                                Player1.volume = (int)(CrossfadeVolumeAdjustment * ((float)CrossfadeTotalRunTime / 1000));
                                Player2.volume = (int)(Volume - (CrossfadeVolumeAdjustment * (float)(CrossfadeTotalRunTime / 1000)));
                            }
                            break;
                        case Players.Player2:
                            {
                                Player1.volume = (int)(Volume - (CrossfadeVolumeAdjustment * ((float)CrossfadeTotalRunTime / 1000)));
                                Player2.volume = (int)(CrossfadeVolumeAdjustment * ((float)CrossfadeTotalRunTime / 1000));
                            }
                            break;
                    }
                }
                else if (CrossfadeTotalRunTime >= (CrossfadeTime * 1000))
                {
                    CrossfadeTimer.Enabled = false;
                    CrossfadeTimer.Stop();
                    InCrossfade = false;
                    switch (PlayingPlayer)
                    {
                        case Players.Player1:
                            Player1.volume = Volume;
                            Player2.volume = 0;
                            break;
                        case Players.Player2:
                            Player2.volume = Volume;
                            Player1.volume = 0;
                            break;
                    }
                }
                CrossfadeTotalRunTime += CrossfadeTimerTickTime;
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("Crossfade timer tick : " + ex.Message));
            }
        }

        private void StartFade()
        {
            InFade = true;
            FadeTotalRunTime = 0;
            switch (FadeState)
            {
                case FadeStates.In:
                    FadeVolumeAdjustment = ((float)Volume / (float)FadeTime);
                    PlayerState = PlayerStates.Playing;
                    break;
                case FadeStates.Out:
                    FadeVolumeAdjustment = ((float)Volume / (float)FadeTime);
                    PlayerState = PlayerStates.Stopped;
                    break;
                case FadeStates.Up:
                    FadeVolumeAdjustment = ((float)(NewVolume - Volume) / (float)FadeTime);
                    PlayerState = PlayerStates.Playing;
                    break;
                case FadeStates.Down:
                    FadeVolumeAdjustment = ((float)(Volume - NewVolume) / (float)FadeTime);
                    PlayerState = PlayerStates.Playing;
                    break;
            }
            FadeTimer.Enabled = true;
            FadeTimer.Start();
        }
        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (FadeState == FadeStates.In)
                {
                    if (FadeTotalRunTime <= 0)
                    {
                        switch (PlayingPlayer)
                        {
                            case Players.Player1:
                                Player1.volume = 0;
                                Player1.play();
                                break;
                            case Players.Player2:
                                Player2.volume = 0;
                                Player2.play();
                                break;
                        }
                    }
                    else if (FadeTotalRunTime < (FadeTime * 1000))
                    {
                        switch (PlayingPlayer)
                        {
                            case Players.Player1:
                                {
                                    Player1.volume = (int)(FadeVolumeAdjustment * ((float)FadeTotalRunTime / 1000));
                                }
                                break;
                            case Players.Player2:
                                {
                                    Player2.volume = (int)(FadeVolumeAdjustment * ((float)FadeTotalRunTime / 1000));
                                }
                                break;
                        }
                    }
                    else if (FadeTotalRunTime >= (FadeTime * 1000))
                    {
                        FadeTimer.Enabled = false;
                        FadeTimer.Stop();
                        InFade = false;
                        switch (PlayingPlayer)
                        {
                            case Players.Player1:
                                Player1.volume = Volume;
                                break;
                            case Players.Player2:
                                Player2.volume = Volume;
                                break;
                        }
                    }
                }
                else if (FadeState == FadeStates.Out)
                {
                    if (FadeTotalRunTime <= 0)
                    {
                    }
                    else if (FadeTotalRunTime < (FadeTime * 1000))
                    {
                        switch (StoppingPlayer)
                        {
                            case Players.Player1:
                                {
                                    Player1.volume = (int)(Volume - (FadeVolumeAdjustment * ((float)FadeTotalRunTime / 1000)));
                                }
                                break;
                            case Players.Player2:
                                {
                                    Player2.volume = (int)(Volume - (FadeVolumeAdjustment * ((float)FadeTotalRunTime / 1000)));
                                }
                                break;
                        }
                    }
                    else if (FadeTotalRunTime >= (FadeTime * 1000))
                    {
                        FadeTimer.Enabled = false;
                        FadeTimer.Stop();
                        InFade = false;
                        switch (StoppingPlayer)
                        {
                            case Players.Player1:
                                Player1.pause();
                                Player1.volume = 0;
                                break;
                            case Players.Player2:
                                Player2.pause();
                                Player2.volume = 0;
                                break;
                        }
                    }
                }
                else if (FadeState == FadeStates.Up)
                {
                    if (FadeTotalRunTime < (FadeTime * 1000))
                    {
                        switch (PlayingPlayer)
                        {
                            case Players.Player1:
                                {
                                    Player1.volume = (int)(FadeVolumeAdjustment * ((float)FadeTotalRunTime / 1000)) + Volume;
                                }
                                break;
                            case Players.Player2:
                                {
                                    Player2.volume = (int)(FadeVolumeAdjustment * ((float)FadeTotalRunTime / 1000)) + Volume;
                                }
                                break;
                        }
                    }
                    else if (FadeTotalRunTime >= (FadeTime * 1000))
                    {
                        Volume = NewVolume;
                        FadeTimer.Enabled = false;
                        FadeTimer.Stop();
                        InFade = false;
                        switch (PlayingPlayer)
                        {
                            case Players.Player1:
                                Player1.volume = Volume;
                                break;
                            case Players.Player2:
                                Player2.volume = Volume;
                                break;
                        }
                    }
                }
                else if (FadeState == FadeStates.Down)
                {
                    if (FadeTotalRunTime <= 0)
                    {
                    }
                    else if (FadeTotalRunTime < (FadeTime * 1000))
                    {
                        switch (PlayingPlayer)
                        {
                            case Players.Player1:
                                {
                                    Player1.volume = (int)(Volume - (FadeVolumeAdjustment * ((float)FadeTotalRunTime / 1000)));
                                }
                                break;
                            case Players.Player2:
                                {
                                    Player2.volume = (int)(Volume - (FadeVolumeAdjustment * ((float)FadeTotalRunTime / 1000)));
                                }
                                break;
                        }
                    }
                    else if (FadeTotalRunTime >= (FadeTime * 1000))
                    {
                        FadeTimer.Enabled = false;
                        FadeTimer.Stop();
                        InFade = false;
                        switch (PlayingPlayer)
                        {
                            case Players.Player1:
                                Player1.volume = NewVolume;
                                break;
                            case Players.Player2:
                                Player2.volume = NewVolume;
                                break;
                        }
                        volume = NewVolume;
                    }
                }
                else
                {
                    FadeTimer.Enabled = false;
                    FadeTimer.Stop();
                    InFade = false;
                }
                FadeTotalRunTime += FadeTimerTickTime;
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("Fade timer tick : " + ex.Message));
            }
        }

        /// <summary>
        /// Fades the player's volume up/down to the specified level.
        /// </summary>
        /// <param name="ANewVolume">The volume to fade to.</param>
        /// <returns>Returns true if fade starts.</returns>
        public bool Fade(int ANewVolume)
        {
            try
            {
                if (!InFade && !InCrossfade && PlayerState == PlayerStates.Playing)
                {
                    NewVolume = ANewVolume > 100 ? 100 : ANewVolume < 0 ? 0 : ANewVolume;
                    InFade = true;
                    FadeState = NewVolume < Volume ? FadeStates.Down : FadeStates.Up;
                    StartFade();
                }
            }
            catch (Exception ex)
            {
                OnError.Invoke(new ErrorEventArgs("Pause : " + ex.Message));
            }
            return false;
        }

        public delegate void PlayerStartEvent(PlayerStartEventArgs e);
        public delegate void PlayerStopEvent(PlayerStopEventArgs e);
        public delegate void PlayerPausedEvent(PlayerPausedEventArgs e);
        public delegate void PlayerMediaEndedEvent(PlayerMediaEndedEventArgs e);

        private delegate void PlayerReadyEvent(PlayerReadyEventArgs e);
        private delegate void PlayerErrorEvent(PlayerErrorEventArgs e);

        public delegate void ErrorEvent(ErrorEventArgs e);

    }

    public class ErrorEventArgs : EventArgs
    {
        public string Message;

        public ErrorEventArgs(string AMessage)
        {
            Message = AMessage;
        }
    }

    public class PlayerStartEventArgs : EventArgs
    {
        public string TheSong;
        internal Players ThePlayer;

        internal PlayerStartEventArgs(string ASong, Players APlayer)
        {
            TheSong = ASong;
            ThePlayer = APlayer;
        }
    }
    public class PlayerStopEventArgs : EventArgs
    {
        public string TheSong;
        internal Players ThePlayer;

        internal PlayerStopEventArgs(string ASong, Players APlayer)
        {
            TheSong = ASong;
            ThePlayer = APlayer;
        }
    }
    public class PlayerPausedEventArgs : EventArgs
    {
        public string TheSong;
        internal Players ThePlayer;

        internal PlayerPausedEventArgs(string ASong, Players APlayer)
        {
            TheSong = ASong;
            ThePlayer = APlayer;
        }
    }
    public class PlayerMediaEndedEventArgs : EventArgs
    {
        public string TheSong;

        public PlayerMediaEndedEventArgs(string ASong)
        {
            TheSong = ASong;
        }
    }

    internal class PlayerReadyEventArgs : EventArgs
    {
        public Players ThePlayer;

        public PlayerReadyEventArgs(Players APlayer)
        {
            ThePlayer = APlayer;
        }
    }
    internal class PlayerErrorEventArgs : EventArgs
    {
        public Players ThePlayer;
        public int StateNum;

        public PlayerErrorEventArgs(Players APlayer, int AStateNum)
        {
            ThePlayer = APlayer;
            StateNum = AStateNum;
        }
    }
}
