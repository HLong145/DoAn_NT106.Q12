using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;

namespace DoAn_NT106.Client
{
    /// <summary>Enums for sound effects</summary>
    public enum SoundEffect
    {
        ButtonClick,
        PunchKG,
        PunchBringer,
        PunchWarrior,
        PunchGM,
        KickKG,
        KickBringer,
        KickWarrior,
        KickGM,
        DashKG,
        DashBringer,
        DashWarrior,
        DashGM,
        ParryWarrior,
        Round1,
        Round2,
        Round3,
        SkillWarrior,
        SkillBringer
    }

    /// <summary>Enums for background music</summary>
    public enum BackgroundMusic
    {
        ThemeMusic,
        BattleMusic
    }

    /// <summary>Audio Manager for handling all sound effects and background music</summary>
    public class AudioManager : IDisposable
    {
        private IWavePlayer _musicPlayer;
        private IWavePlayer _soundEffectPlayer;
        private IWaveProvider _currentMusicProvider;
        
        private Dictionary<SoundEffect, string> _soundEffectPaths;
        private Dictionary<BackgroundMusic, string> _musicPaths;

        // Volume levels (0.0f to 1.0f)
        private float _musicVolume = 0.8f;      // ? T?ng t? 0.5f lên 0.8f
        private float _soundEffectVolume = 0.9f; // ? T?ng t? 0.7f lên 0.9f

        // Cache for loaded audio files
        private Dictionary<string, byte[]> _audioCache = new Dictionary<string, byte[]>();

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Math.Max(0f, Math.Min(1f, value));
                if (_musicPlayer is VolumeWaveProvider16 vwp)
                {
                    vwp.Volume = _musicVolume;
                }
            }
        }

        public float SoundEffectVolume
        {
            get => _soundEffectVolume;
            set => _soundEffectVolume = Math.Max(0f, Math.Min(1f, value));
        }

        public AudioManager()
        {
            InitializePlayers();
            InitializeSoundPaths();
        }

        /// <summary>Initialize audio players</summary>
        private void InitializePlayers()
        {
            try
            {
                _musicPlayer = new WaveOutEvent();
                _soundEffectPlayer = new WaveOutEvent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Failed to initialize audio players: {ex.Message}");
            }
        }

        /// <summary>Initialize sound file paths</summary>
        private void InitializeSoundPaths()
        {
            _soundEffectPaths = new Dictionary<SoundEffect, string>
            {
                { SoundEffect.ButtonClick, "button.mp3" },
                { SoundEffect.PunchKG, "punch_KG_warrior_Bringer.mp3" },
                { SoundEffect.PunchBringer, "punch_KG_warrior_Bringer.mp3" },
                { SoundEffect.PunchWarrior, "punch_KG_warrior_Bringer.mp3" },
                { SoundEffect.PunchGM, "goatman_punch.mp3" },
                { SoundEffect.KickKG, "kick_kg_warrior.mp3" },
                { SoundEffect.KickBringer, "kick_Bringer.mp3" },
                { SoundEffect.KickWarrior, "kick_kg_warrior.mp3" },
                { SoundEffect.KickGM, "goatman_kick.mp3" },
                { SoundEffect.DashKG, "dash_KG.mp3" },
                { SoundEffect.DashBringer, "dash_Bringer.mp3" },
                { SoundEffect.DashWarrior, "dash_warrior.mp3" },
                { SoundEffect.DashGM, "dash_gm.mp3" },
                { SoundEffect.ParryWarrior, "parry_warrior.mp3" },
                { SoundEffect.Round1, "round_1.mp3" },
                { SoundEffect.Round2, "round_2.mp3" },
                { SoundEffect.Round3, "round_3.mp3" },
                { SoundEffect.SkillWarrior, "skill_warrior.mp3" },
                { SoundEffect.SkillBringer, "skill_bringer.mp3" }
            };

            _musicPaths = new Dictionary<BackgroundMusic, string>
            {
                { BackgroundMusic.ThemeMusic, "music_theme.mp3" },
                { BackgroundMusic.BattleMusic, "music_battle.mp3" }
            };
        }

        /// <summary>Play a sound effect</summary>
        public void PlaySoundEffect(SoundEffect effect)
        {
            try
            {
                if (!_soundEffectPaths.TryGetValue(effect, out var filename))
                {
                    Console.WriteLine($"?? Sound effect '{effect}' not found in paths");
                    return;
                }

                Console.WriteLine($"?? Attempting to play: {effect} ({filename})");
                byte[] audioData = LoadAudioFile(filename);
                if (audioData == null || audioData.Length == 0)
                {
                    Console.WriteLine($"? Could not load audio file: {filename}");
                    return;
                }

                var ms = new MemoryStream(audioData);
                var reader = new Mp3FileReader(ms);
                var volumeProvider = new VolumeWaveProvider16(reader) { Volume = _soundEffectVolume };

                _soundEffectPlayer?.Stop(); // Stop previous sound
                _soundEffectPlayer?.Init(volumeProvider);
                _soundEffectPlayer?.Play();

                Console.WriteLine($"? Playing sound: {effect} (Volume: {_soundEffectVolume})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error playing sound effect {effect}: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }

        /// <summary>Play a sound effect with delay (for sequential sounds like warrior double punch)</summary>
        public void PlaySoundEffectWithDelay(SoundEffect effect, int delayMs)
        {
            try
            {
                var delayTimer = new System.Windows.Forms.Timer { Interval = delayMs };
                delayTimer.Tick += (s, e) =>
                {
                    delayTimer.Stop();
                    delayTimer.Dispose();
                    PlaySoundEffect(effect);
                };
                delayTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error scheduling delayed sound {effect}: {ex.Message}");
            }
        }

        /// <summary>Play background music (looping)</summary>
        public void PlayBackgroundMusic(BackgroundMusic music, bool loop = true)
        {
            try
            {
                StopMusic();

                if (!_musicPaths.TryGetValue(music, out var filename))
                {
                    Console.WriteLine($"?? Music '{music}' not found in paths");
                    return;
                }

                Console.WriteLine($"?? Attempting to play music: {music} ({filename})");
                byte[] audioData = LoadAudioFile(filename);
                if (audioData == null || audioData.Length == 0)
                {
                    Console.WriteLine($"? Could not load music file: {filename}");
                    return;
                }

                var ms = new MemoryStream(audioData);
                var reader = new Mp3FileReader(ms);

                if (loop)
                {
                    _currentMusicProvider = new LoopingWaveProvider(reader);
                    Console.WriteLine($"?? Music will loop");
                }
                else
                {
                    _currentMusicProvider = reader;
                }

                var volumeProvider = new VolumeWaveProvider16(_currentMusicProvider) { Volume = _musicVolume };
                _musicPlayer?.Init(volumeProvider);
                _musicPlayer?.Play();

                Console.WriteLine($"? Playing music: {music} (Volume: {_musicVolume})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error playing music {music}: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }

        /// <summary>Stop background music</summary>
        public void StopMusic()
        {
            try
            {
                _musicPlayer?.Stop();
                (_currentMusicProvider as IDisposable)?.Dispose();
                _currentMusicProvider = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error stopping music: {ex.Message}");
            }
        }

        /// <summary>Load audio file from Resources or file system</summary>
        private byte[] LoadAudioFile(string filename)
        {
            try
            {
                // Check cache first
                if (_audioCache.TryGetValue(filename, out var cachedData))
                {
                    Console.WriteLine($"? Loading from cache: {filename}");
                    return cachedData;
                }

                // Try loading from embedded resources first
                byte[] data = LoadFromResources(filename);
                if (data != null && data.Length > 0)
                {
                    Console.WriteLine($"? Loaded from Resources: {filename} ({data.Length} bytes)");
                    _audioCache[filename] = data;
                    return data;
                }

                // Try loading from local Sound folder
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", filename);
                Console.WriteLine($"?? Checking folder: {soundPath}");
                if (File.Exists(soundPath))
                {
                    data = File.ReadAllBytes(soundPath);
                    Console.WriteLine($"? Loaded from Sounds folder: {filename} ({data.Length} bytes)");
                    _audioCache[filename] = data;
                    return data;
                }

                // Try current directory
                if (File.Exists(filename))
                {
                    data = File.ReadAllBytes(filename);
                    Console.WriteLine($"? Loaded from current directory: {filename} ({data.Length} bytes)");
                    _audioCache[filename] = data;
                    return data;
                }

                Console.WriteLine($"? Audio file not found: {filename}");
                Console.WriteLine($"   Searched in Resources, {soundPath}, and current directory");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error loading audio file {filename}: {ex.Message}");
                return null;
            }
        }

        /// <summary>Load audio from embedded resources</summary>
        private byte[] LoadFromResources(string filename)
        {
            try
            {
                // Normalize key from filename
                string key = Path.GetFileNameWithoutExtension(filename)
                    .Replace('-', '_')
                    .Replace(' ', '_');

                // 1) Try direct property access (strongly typed Resources.Designer)
                var resourceType = typeof(Properties.Resources);
                var prop = resourceType.GetProperty(key,
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    var val = prop.GetValue(null);
                    // Common cases: byte[], UnmanagedMemoryStream, Stream
                    if (val is byte[] bytes) return bytes;
                    if (val is System.IO.UnmanagedMemoryStream ums)
                    {
                        using var ms = new MemoryStream();
                        ums.CopyTo(ms);
                        return ms.ToArray();
                    }
                    if (val is Stream s)
                    {
                        using var ms = new MemoryStream();
                        s.CopyTo(ms);
                        return ms.ToArray();
                    }
                }

                // 2) Try ResourceManager (handles ResXFileRef entries)
                var obj = Properties.Resources.ResourceManager.GetObject(key);
                if (obj is byte[] rmBytes) return rmBytes;
                if (obj is System.IO.UnmanagedMemoryStream rmUms)
                {
                    using var ms = new MemoryStream();
                    rmUms.CopyTo(ms);
                    return ms.ToArray();
                }
                if (obj is Stream rmStream)
                {
                    using var ms = new MemoryStream();
                    rmStream.CopyTo(ms);
                    return ms.ToArray();
                }

                // 3) Try loose match: iterate all resource keys (fallback)
                try
                {
                    var set = Properties.Resources.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);
                    if (set != null)
                    {
                        foreach (System.Collections.DictionaryEntry entry in set)
                        {
                            var name = entry.Key as string;
                            if (string.IsNullOrEmpty(name)) continue;
                            // Match by suffix or sanitized
                            var sanitized = name.Replace('-', '_').Replace(' ', '_');
                            if (sanitized.Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                var v = entry.Value;
                                if (v is byte[] eBytes) return eBytes;
                                if (v is System.IO.UnmanagedMemoryStream eUms)
                                {
                                    using var ms = new MemoryStream();
                                    eUms.CopyTo(ms);
                                    return ms.ToArray();
                                }
                                if (v is Stream eStream)
                                {
                                    using var ms = new MemoryStream();
                                    eStream.CopyTo(ms);
                                    return ms.ToArray();
                                }
                            }
                        }
                    }
                }
                catch { }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Pause background music</summary>
        public void PauseMusic()
        {
            _musicPlayer?.Pause();
        }

        /// <summary>Resume background music</summary>
        public void ResumeMusic()
        {
            if (_musicPlayer?.PlaybackState == PlaybackState.Paused)
            {
                _musicPlayer.Play();
            }
        }

        /// <summary>Stop all sounds</summary>
        public void StopAllSounds()
        {
            try
            {
                _musicPlayer?.Stop();
                _soundEffectPlayer?.Stop();
            }
            catch { }
        }

        /// <summary>Stop only sound effects (keep music playing)</summary>
        public void StopSoundEffectsOnly()
        {
            try
            {
                _soundEffectPlayer?.Stop();
            }
            catch { }
        }

        /// <summary>Test audio system - list all audio files</summary>
        public void DiagnosticInfo()
        {
            Console.WriteLine("\n=== ?? AUDIO SYSTEM DIAGNOSTIC ===");
            Console.WriteLine($"Music Volume: {_musicVolume}");
            Console.WriteLine($"Sound Effect Volume: {_soundEffectVolume}");
            Console.WriteLine($"Cached Files: {_audioCache.Count}");
            Console.WriteLine("\nSound Effects Mapping:");
            foreach (var kvp in _soundEffectPaths)
            {
                Console.WriteLine($"  {kvp.Key} ? {kvp.Value}");
            }
            Console.WriteLine("\nMusic Mapping:");
            foreach (var kvp in _musicPaths)
            {
                Console.WriteLine($"  {kvp.Key} ? {kvp.Value}");
            }
            Console.WriteLine("=== END DIAGNOSTIC ===\n");
        }

        /// <summary>Dispose resources</summary>
        public void Dispose()
        {
            try
            {
                StopAllSounds();
                _musicPlayer?.Dispose();
                _soundEffectPlayer?.Dispose();
                (_currentMusicProvider as IDisposable)?.Dispose();

                foreach (var stream in _audioCache.Values)
                {
                    // Streams are managed by NAudio
                }
                _audioCache.Clear();
            }
            catch { }
        }
    }

    /// <summary>Wave provider that loops audio</summary>
    public class LoopingWaveProvider : IWaveProvider
    {
        private IWaveProvider _source;
        private byte[] _buffer;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public LoopingWaveProvider(IWaveProvider source)
        {
            _source = source;
            _buffer = new byte[source.WaveFormat.AverageBytesPerSecond];
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = _source.Read(buffer, offset + totalRead, count - totalRead);
                if (read == 0)
                {
                    // Loop back to beginning
                    if (_source is Mp3FileReader mp3Reader)
                    {
                        mp3Reader.CurrentTime = TimeSpan.Zero;
                    }
                    continue;
                }
                totalRead += read;
            }
            return totalRead;
        }
    }
}
