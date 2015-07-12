using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TreeSharp;

namespace BoringHeroes.GameLogic.HeroRelated
{
    public class Heroes
    {
        public static Hero Raynor
        {
            get
            {
                return new Hero(null, null, null, "Raynor", 1328, 151, 350, 9,
                    new List<Skill>
                    {
                        new Skill("Penetrating Shot", 12, 450, 0, Keys.Q, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.SoloDamage}, delegate { }),
                        new Skill("Inpire", 15, -1, 0, Keys.W, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.Buff}, delegate { }),
                        new Skill("Adrenalin Rush", 45, -1, -1, Keys.E, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.SoloHeal}, delegate { })
                    }, new[]
                    {
                        new Talent("Give Me More", 1, 5, delegate { }),
                        new Talent("Vampiric Assault", 4, 2, delegate
                        {
                            /*TODO:ADD VAMPIRIC AND SHIT*/
                        }),
                        new Talent("Penetrating Ammo", 7, 4, delegate { }),
                        new Talent("Raynor's Raiders", 10, 1,
                            delegate(Hero hero)
                            {
                                hero.Skills.Add(new Skill("Raynor's Raiders", 100, 400, 0, Keys.R, false,
                                    new List<Skill.SkillProperty>
                                    {
                                        Skill.SkillProperty.SoloDamage,
                                        Skill.SkillProperty.Ultimate
                                    }, delegate { }));
                            }),
                        new Talent("Activated Rush", 13, 2,
                            delegate(Hero hero)
                            {
                                hero.Skills.Single(o => o.Name.Contains("Adrenalin Rush")).SkillCastTime = 0;
                            }),
                        new Talent("Bull's Eye", 16, 2, delegate { }),
                        new Talent("Dusk Volley", 20, 3, delegate { })
                    });
            }
        }

        public static Hero Muradin
        {
            get
            {
                return new Hero(null, null, null, "Muradin", 1328, 151, 35, 9,
                    new List<Skill>
                    {
                        new Skill("Storm Bolt", 10, 450, 0, Keys.Q, false,
                            new List<Skill.SkillProperty>
                            {
                                Skill.SkillProperty.SoloDamage,
                                Skill.SkillProperty.Stun,
                                Skill.SkillProperty.SkillShot
                            }, delegate { }),
                        new Skill("Thunderclap", 8, 200, 0, Keys.W, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { }),
                        new Skill("Dwarf Toss", 12, 400, 0, Keys.E, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.LocalMobility}, delegate { })
                    }, new[]
                    {
                        new Talent("Infused Hammer", 1, 3, delegate { }),
                        new Talent("Third Wind", 4, 4, delegate { }),
                        new Talent("Landing Momentum", 7, 2, delegate { }),
                        new Talent("Avatar", 10, 2,
                            delegate(Hero hero)
                            {
                                hero.Skills.Add(new Skill("Avatar", 100, 400, 0, Keys.R, false,
                                    new List<Skill.SkillProperty>
                                    {
                                        Skill.SkillProperty.Buff,
                                        Skill.SkillProperty.Ultimate
                                    }, delegate { }));
                            }),
                        new Talent("Healing Static", 13, 4,
                            delegate(Hero hero)
                            {
                                hero.Skills.Single(o => o.Name.Contains("Thunderclap"))
                                    .Properties.Add(Skill.SkillProperty.Lifesteal);
                            }),
                        new Talent("Heavy Impact", 16, 4,
                            delegate(Hero hero)
                            {
                                hero.Skills.Single(o => o.Name.Contains("Dwarf Toss"))
                                    .Properties.Add(Skill.SkillProperty.Stun);
                            }),
                        new Talent("Dusk Volley", 20, 3, delegate { })
                    });
            }
        }

        public static Hero Valla
        {
            get
            {
                return new Hero(null, null, null, "Valla", 1328, 151, 350, 9,
                    new List<Skill>
                    {
                        new Skill("Hungering Arrow", 14, 600, 0, Keys.Q, false,
                            new List<Skill.SkillProperty>
                            {
                                Skill.SkillProperty.SoloDamage,
                                Skill.SkillProperty.SkillShot
                            }, delegate { }),
                        new Skill("Multishot", 8, 600, 0, Keys.W, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { }),
                        new Skill("Vault", 10, 250, 0, Keys.E, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.LocalMobility}, delegate { })
                    }, new[]
                    {
                        new Talent("Siphoning Arrow", 1, 4, delegate
                        {
                            /*TODO: ADDS LIFESTEAL*/
                        }),
                        new Talent("Vampiric Assault", 4, 4, delegate
                        {
/*TODO:SAME as ABOVE*/
                        }),
                        new Talent("Repeating Arrow", 7, 2, delegate(Hero hero)
                        {
                            hero.Skills.First(o => o.Name.Contains("Vault")).AfterSkillUseAction =
                                delegate
                                {
                                    hero.Skills.First(o => o.Name.Contains("Hungering Arrow")).OnCooldownUntil =
                                        DateTime.Now;
                                };
                        }),
                        new Talent("Rain of Vengeance", 10, 1,
                            delegate(Hero hero)
                            {
                                hero.Skills.Add(new Skill("Rain of Vengeance", 90, 400, 0, Keys.R, false,
                                    new List<Skill.SkillProperty>
                                    {
                                        Skill.SkillProperty.AOEDamage,
                                        Skill.SkillProperty.Ultimate
                                    }, delegate { }));
                            }),
                        new Talent("Tempered by Discipline", 13, 2, delegate
                        {
                            //adds lifesteal to basic attacks...
                        }),
                        new Talent("Tumble", 16, 3,
                            delegate(Hero hero) { hero.Skills.Single(o => o.Name.Contains("Vault")).Cooldown -= 3; }),
                        new Talent("Storm of Vengeance", 20, 3, delegate { })
                    });
            }
        }

        public static Hero Jaina
        {
            get
            {
                return new Hero(null, null, null, "Jaina", 1328, 151, 350, 9,
                    new List<Skill>
                    {
                        new Skill("Frost Bolt", 4, 600, 0, Keys.Q, false,
                            new List<Skill.SkillProperty>
                            {
                                Skill.SkillProperty.SoloDamage,
                                Skill.SkillProperty.SkillShot
                            }, delegate { }),
                        new Skill("Blizzard", 15, 600, 0, Keys.W, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { }),
                        new Skill("Cone of Cold", 10, 350, 0, Keys.E, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { })
                    }, new[]
                    {
                        new Talent("Lingering Cold", 1, 3, delegate
                        {
                            /*TODO: ADDS LIFESTEAL*/
                        }),
                        new Talent("Snow Storm", 4, 3, delegate
                        {
/*TODO:SAME as ABOVE*/
                        }),
                        new Talent("Frost Bitten", 7, 3, delegate { }),
                        new Talent("Rain of Vengeance", 10, 1,
                            delegate(Hero hero)
                            {
                                hero.Skills.Add(new Skill("Ring of Frost", 100, 300, 0, Keys.R, false,
                                    new List<Skill.SkillProperty>
                                    {
                                        Skill.SkillProperty.AOEDamage,
                                        Skill.SkillProperty.Ultimate
                                    }, delegate { }));
                            }),
                        new Talent("Storm Front", 13, 4, delegate
                        {
                            //RANGE
                        }),
                        new Talent("Snow Crash", 16, 4, delegate { }),
                        new Talent("Second Snap", 20, 4, delegate { })
                    });
            }
        }

        public static Hero Furion
        {
            get
            {
                return new Hero(null, null, null, "Furion", 1328, 151, 350, 9,
                    new List<Skill>
                    {
                        new Skill("Regrowth", 7, 600, 0, Keys.Q, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.SoloHeal}, delegate { }),
                        new Skill("Moonfire", 12, 600, 0, Keys.W, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.SoloDamage}, delegate { }),
                        new Skill("Entangling Roots", 12, 450, 0, Keys.E, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { }),
                        new Skill("Innervate", 30, 450, 0, Keys.E, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.Buff}, delegate { })
                    }, new[]
                    {
                        new Talent("Lingering Cold", 1, 4, delegate
                        {
                            /*TODO: ADDS LIFESTEAL*/
                        }),
                        new Talent("Snow Storm", 4, 5, delegate
                        {
/*TODO:SAME as ABOVE*/
                        }),
                        new Talent("Frost Bitten", 7, 4, delegate { }),
                        new Talent("Rain of Vengeance", 10, 2,
                            delegate(Hero hero)
                            {
                                hero.Skills.Add(new Skill("Tranquility", 100, 300, 0, Keys.R, false,
                                    new List<Skill.SkillProperty>
                                    {
                                        Skill.SkillProperty.AOEHeal,
                                        Skill.SkillProperty.Ultimate
                                    }, delegate { }));
                            }),
                        new Talent("Storm Front", 13, 5, delegate
                        {
                            //RANGE
                        }),
                        new Talent("Snow Crash", 16, 4, delegate { }),
                        new Talent("Second Snap", 20, 4, delegate { })
                    });
            }
        }

        public static Hero Tassadar
        {
            get
            {
                return new Hero(null, null, null, "Furion", 1328, 151, 350, 9,
                    new List<Skill>
                    {
                        new Skill("Plasma Shield", 8, 600, 0, Keys.Q, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.Buff}, delegate { }),
                        new Skill("Psionic Storm", 9, 600, 0, Keys.W, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.SoloDamage}, delegate { }),
                        new Skill("Psionic Storm[TEMPFIX]", 8, 450, 0, Keys.W, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { }),
                        new Skill("Dimensional Shift", 20, 0, 0, Keys.E, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.LocalMobility}, delegate { })
                    }, new[]
                    {
                        new Talent("Lingering Cold", 1, 4, delegate
                        {
                            /*TODO: ADDS LIFESTEAL*/
                        }),
                        new Talent("Snow Storm", 4, 5, delegate
                        {
/*TODO:SAME as ABOVE*/
                        }),
                        new Talent("Frost Bitten", 7, 4, delegate { }),
                        new Talent("Rain of Vengeance", 10, 2,
                            delegate(Hero hero)
                            {
                                hero.Skills.Add(new Skill("Tranquility", 100, 300, 0, Keys.R, false,
                                    new List<Skill.SkillProperty>
                                    {
                                        Skill.SkillProperty.AOEHeal,
                                        Skill.SkillProperty.Ultimate
                                    }, delegate { }));
                            }),
                        new Talent("Storm Front", 13, 5, delegate
                        {
                            //RANGE
                        }),
                        new Talent("Snow Crash", 16, 4, delegate { }),
                        new Talent("Second Snap", 20, 4, delegate { })
                    });
            }
        }

        public static Hero Tyrael
        {
            get
            {
                return new Hero(null, null, null, "Furion", 1328, 151, 350, 9,
                    new List<Skill>
                    {
                        new Skill("Plasma Shield", 8, 600, 0, Keys.W, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.Buff}, delegate { }),
                        new Skill("Psionic Storm", 9, 600, 0, Keys.Q, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.SoloDamage}, delegate { }),
                        new Skill("Psionic Storm[TEMPFIX]", 8, 450, 0, Keys.E, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { })
                    }, new[]
                    {
                        new Talent("Lingering Cold", 1, 4, delegate
                        {
                            /*TODO: ADDS LIFESTEAL*/
                        }),
                        new Talent("Snow Storm", 4, 5, delegate
                        {
/*TODO:SAME as ABOVE*/
                        }),
                        new Talent("Frost Bitten", 7, 4, delegate { }),
                        new Talent("Rain of Vengeance", 10, 2,
                            delegate(Hero hero)
                            {
                                hero.Skills.Add(new Skill("Tranquility", 100, 300, 0, Keys.R, false,
                                    new List<Skill.SkillProperty>
                                    {
                                        Skill.SkillProperty.SoloDamage,
                                        Skill.SkillProperty.Ultimate
                                    }, delegate { }));
                            }),
                        new Talent("Storm Front", 13, 5, delegate
                        {
                            //RANGE
                        }),
                        new Talent("Snow Crash", 16, 4, delegate { }),
                        new Talent("Second Snap", 20, 4, delegate { })
                    });
            }
        }

        public static Hero Zagara
        {
            get
            {
                return new Hero(null, null, null, "Zagara", 1328, 151, 350, 9,
                    new List<Skill>
                    {
                        new Skill("Banelings", 10, 800, 0, Keys.Q, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { }),
                        new Skill("Hunter Killer", 14, 600, 0, Keys.W, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.SoloDamage}, delegate { }),
                        new Skill("Infested Drop", 12, 450, 0, Keys.E, true,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.AOEDamage}, delegate { }),
                        new Skill("Creep Tumor", 30, 450, 0, Keys.D, false,
                            new List<Skill.SkillProperty> {Skill.SkillProperty.Buff}, delegate { })
                        /*THIS SHIT REQUIRES A CUSTOM FUCKING CLASS*/
                    }, new[]
                    {
                        new Talent("PLACEHOLDER ZAGARA", 1, 4, delegate
                        {
                            /*TODO: ADDS LIFESTEAL*/
                        }),
                        new Talent("PLACEHOLDER ZAGARA", 4, 4, delegate
                        {
/*TODO:SAME as ABOVE*/
                        }),
                        new Talent("PLACEHOLDER ZAGARA", 7, 4, delegate { }),
                        new Talent("PLACEHOLDER ZAGARA", 10, 2,
                            delegate(Hero hero)
                            {
                                hero.Skills.Add(new Skill("Eat that fucker", 100, 400, 0, Keys.R, false,
                                    new List<Skill.SkillProperty>
                                    {
                                        Skill.SkillProperty.SoloDamage,
                                        Skill.SkillProperty.Ultimate
                                    }, delegate { }));
                            }),
                        new Talent("PLACEHOLDER ZAGARA", 13, 5, delegate
                        {
                            //RANGE
                        }),
                        new Talent("PLACEHOLDER ZAGARA", 16, 4, delegate { }),
                        new Talent("Second Snap", 20, 4, delegate { })
                    });
            }
        }
    }

    public class Hero
    {
        protected Composite combatOverride;
        protected Composite pushOverride;
        protected Composite restOverride;

        public Hero(Composite combatOverride, Composite restOverride, Composite pushOverride, string name, int chooseX,
            int chooseY, int attackRange, int siegeCampLevel, List<Skill> skills, Talent[] talents)
        {
            this.combatOverride = combatOverride;
            this.restOverride = restOverride;
            this.pushOverride = pushOverride;
            Name = name;
            ChooseX = chooseX;
            ChooseY = chooseY;
            AttackRange = attackRange;
            SiegeCampLevel = siegeCampLevel;
            Skills = skills;
            Talents = talents;
        }

        public string Name { get; set; }
        public int ChooseX { get; set; }
        public int ChooseY { get; set; }
        public int AttackRange { get; set; }
        public int SiegeCampLevel { get; set; }
        public List<Skill> Skills { get; set; }

        public Composite CombatComposite
        {
            get { return combatOverride; }
        }

        public Composite RestComposite
        {
            get { return restOverride; }
        }

        public Composite PushComposite
        {
            get { return pushOverride; }
        }

        //bot to top (1-5)
        public Talent[] Talents { get; set; }
    }
}