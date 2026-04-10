public interface ICommand
{
    /// <summary>
    /// Executes the command against the live game state.
    /// Returns true if the state was mutated, false otherwise.
    /// </summary>
    bool Execute(GameState_Live liveState);
}
