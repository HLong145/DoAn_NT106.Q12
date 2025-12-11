using System;
using DoAn_NT106.Client;

namespace DoAn_NT106
{
    /// <summary>Extension methods for CombatSystem to play sounds</summary>
    public static class CombatSoundExtensions
    {
        /// <summary>Play sound based on character type and attack type</summary>
        public static void PlayAttackSound(string characterType, string attackType)
        {
            SoundEffect sound = GetAttackSound(characterType, attackType);
            SoundManager.PlaySound(sound);
        }

        /// <summary>Map character and attack type to appropriate sound effect</summary>
        private static SoundEffect GetAttackSound(string characterType, string attackType)
        {
            if (characterType == null) return SoundEffect.ButtonClick;

            return (characterType.ToLower(), attackType.ToLower()) switch
            {
                // ? Punch attacks: KG/Bringer punch removed (played on hit instead)
                ("girlknight", "punch") => SoundEffect.ButtonClick, // Don't play early sound
                ("bringerofdeath", "punch") => SoundEffect.ButtonClick, // Delay 1s instead
                ("warrior", "punch") => SoundEffect.ButtonClick, // ? Warrior punch sound played directly in ExecutePunchAttack (always 2x)
                ("goatman", "punch") => SoundEffect.PunchGM, // ? Goatman punch plays at startup (1x regardless of hit)

                // ? Kick attacks: Warrior kick plays KickWarrior sound at startup (not ButtonClick)
                ("girlknight", "kick") => SoundEffect.ButtonClick, // Don't play early sound
                ("bringerofdeath", "kick") => SoundEffect.KickBringer,
                ("warrior", "kick") => SoundEffect.KickWarrior, // ? Warrior kick: KickWarrior at startup (not ButtonClick)
                // Goatman kick should only play on hit (handled inside ExecuteGoatmanKick)
                ("goatman", "kick") => SoundEffect.ButtonClick,

                // Dash/Special
                ("girlknight", "slide" or "dash") => SoundEffect.DashKG,
                ("bringerofdeath", "fireball" or "dash") => SoundEffect.DashBringer,
                ("warrior", "slide" or "dash") => SoundEffect.DashWarrior,
                ("goatman", "fireball" or "dash") => SoundEffect.DashGM,

                _ => SoundEffect.ButtonClick
            };
        }

        /// <summary>Play sound effect for button clicks</summary>
        public static void PlayButtonSound()
        {
            SoundManager.PlaySound(SoundEffect.ButtonClick);
        }
    }
}
