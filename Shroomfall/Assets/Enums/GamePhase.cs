namespace Assets.Enums
{
    public enum GamePhase
    {
        // --- Menu Scene ---
        MainMenu,
        SettingMenu,
        SignIn,
        SignUp,

        // --- Game Scene ---
        HostCombat,
        JoinCombat,
        CreateSession,
        ListSession,
        InGame,

        // --- Global ---
        Paused,
    }
}
