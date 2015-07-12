namespace BoringHeroes.GameLogic
{
    internal class ScreenStateMenu
    {
        public enum CurrentGameMode
        {
            Practice,
            Cooperative,
            QuickMatch
        }

        public bool IsSearching { get; set; }
        public bool IsInMenu { get; set; }
        public bool IsOnLoadingScreen { get; set; }
        public bool IsInGame { get; set; }
    }
}