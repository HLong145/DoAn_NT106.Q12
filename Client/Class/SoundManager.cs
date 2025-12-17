using System;

namespace DoAn_NT106.Client.Class
{
    /// <summary>Static helper class for easy sound access throughout the game</summary>
    public static class SoundManager
    {
        private static AudioManager _instance;
        // Global flag to enable/disable background music (theme + battleground)
        public static bool MusicEnabled { get; set; } = true;

        /// <summary>Initialize the sound manager (call once at game startup)</summary>
        public static void Initialize()
        {
            if (_instance == null)
            {
                _instance = new AudioManager();
                Console.WriteLine("? SoundManager initialized");
            }
        }

        /// <summary>Play a sound effect</summary>
        public static void PlaySound(SoundEffect effect)
        {
            _instance?.PlaySoundEffect(effect);
        }

        /// <summary>Play a sound effect with delay (for sequential sounds)</summary>
        public static void PlaySoundWithDelay(SoundEffect effect, int delayMs)
        {
            _instance?.PlaySoundEffectWithDelay(effect, delayMs);
        }

        /// <summary>Play background music</summary>
        public static void PlayMusic(BackgroundMusic music, bool loop = true)
        {
            if (!MusicEnabled) return;
            _instance?.PlayBackgroundMusic(music, loop);
        }

        /// <summary>Stop background music</summary>
        public static void StopMusic()
        {
            _instance?.StopMusic();
        }

        /// <summary>Pause background music</summary>
        public static void PauseMusic()
        {
            _instance?.PauseMusic();
        }

        /// <summary>Resume background music</summary>
        public static void ResumeMusic()
        {
            _instance?.ResumeMusic();
        }

        /// <summary>Set music volume (0.0 - 1.0)</summary>
        public static void SetMusicVolume(float volume)
        {
            if (_instance != null)
            {
                _instance.MusicVolume = volume;
            }
        }

        /// <summary>Set sound effect volume (0.0 - 1.0)</summary>
        public static void SetSoundEffectVolume(float volume)
        {
            if (_instance != null)
            {
                _instance.SoundEffectVolume = volume;
            }
        }

        /// <summary>Stop all sounds</summary>
        public static void StopAll()
        {
            _instance?.StopAllSounds();
        }

        /// <summary>Stop only sound effects (keep music playing)</summary>
        public static void StopSoundEffectsOnly()
        {
            _instance?.StopSoundEffectsOnly();
        }

        /// <summary>Cleanup and dispose (call at game shutdown)</summary>
        public static void Cleanup()
        {
            _instance?.Dispose();
            _instance = null;
        }
    }
}
