namespace BoringHeroes.GameLogic.HeroRelated
{
    public class Talent
    {
        public delegate void TalentAction(Hero hero);

        public TalentAction HeroChanges;

        public Talent(string talentName, int level, int talentIndex, TalentAction heroChanges)
        {
            HeroChanges = heroChanges;
            TalentName = talentName;
            TalentLevel = level;
            IsLeveled = false;
            TalentIndex = talentIndex;
        }

        public string TalentName { get; set; }
        public int TalentIndex { get; set; }
        public int TalentLevel { get; set; }
        public bool IsLeveled { get; set; }
    }
}