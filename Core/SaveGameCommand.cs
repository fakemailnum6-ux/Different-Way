public class SaveGameCommand : ICommand
{
    public bool Execute(GameState_Live liveState)
    {
        // Actually execute the database save on the simulation background thread
        ServiceLocator.SaveManager?.SaveGameState(liveState);
        return false; // Saving doesn't mutate the game state version itself
    }
}
