using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BoringHeroes.GameLogic.HeroRelated
{
    public class Skill
    {
        public delegate void SkillAction(Hero hero);

        public enum SkillProperty
        {
            SoloDamage,
            SoloHeal,
            AOEDamage,
            AOEHeal,
            Buff,
            LocalMobility,
            Debuff,
            Stun,
            SkillShot,
            Ultimate,
            Lifesteal
        }

        public Skill(string name, int cooldown, int range, int castTime, Keys key, bool shouldCastOnCreeps,
            List<SkillProperty> type, SkillAction action)
        {
            Name = name;
            Cooldown = cooldown;
            Range = range;
            Properties = type;
            Key = key;
            ShouldCastOnCreeps = shouldCastOnCreeps;
            SkillCastTime = castTime;
            AfterSkillUseAction = action;
        }

        public string Name { get; set; }
        public int Cooldown { get; set; }
        public int Range { get; set; }
        public int SkillCastTime { get; set; }
        public List<SkillProperty> Properties { get; set; }
        public Keys Key { get; set; }
        public bool ShouldCastOnCreeps { get; set; }
        public DateTime OnCooldownUntil { get; set; }
        public SkillAction AfterSkillUseAction { get; set; }

        public bool IsCastable
        {
            get { return SkillCastTime >= 0; }
        }
    }
}