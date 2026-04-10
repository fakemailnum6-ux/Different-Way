using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[AttributeUsage(AttributeTargets.Method)]
public class ConsoleCommandAttribute : Attribute
{
    public string CommandName { get; }
    public string Description { get; }

    public ConsoleCommandAttribute(string name, string description = "")
    {
        CommandName = name;
        Description = description;
    }
}

public partial class ConsoleManager : RefCounted
{
    private readonly Dictionary<string, MethodInfo> _commands = new Dictionary<string, MethodInfo>();

    public ConsoleManager()
    {
        RegisterCommands();
    }

    private void RegisterCommands()
    {
        var methods = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ConsoleCommandAttribute>();
            if (attribute != null)
            {
                _commands[attribute.CommandName.ToLower()] = method;
            }
        }
    }

    public string ExecuteCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string cmdName = parts[0].ToLower();

        if (!_commands.TryGetValue(cmdName, out MethodInfo method))
        {
            return $"Unknown command: {cmdName}";
        }

        var parameters = method.GetParameters();
        var args = new object[parameters.Length];

        try
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i + 1 < parts.Length)
                {
                    args[i] = Convert.ChangeType(parts[i + 1], parameters[i].ParameterType);
                }
                else if (parameters[i].HasDefaultValue)
                {
                    args[i] = parameters[i].DefaultValue;
                }
                else
                {
                    return $"Missing argument: {parameters[i].Name}";
                }
            }

            var result = method.Invoke(this, args);
            return result?.ToString() ?? "Command executed successfully.";
        }
        catch (Exception ex)
        {
            return $"Command error: {ex.InnerException?.Message ?? ex.Message}";
        }
    }

    [ConsoleCommand("help", "Lists all available commands")]
    private string CmdHelp()
    {
        var sb = new System.Text.StringBuilder("Available commands:\n");
        foreach (var cmd in _commands.OrderBy(c => c.Key))
        {
            var attr = cmd.Value.GetCustomAttribute<ConsoleCommandAttribute>();
            sb.AppendLine($"- {attr.CommandName}: {attr.Description}");
        }
        return sb.ToString();
    }

    [ConsoleCommand("give_gold", "Adds gold directly to the Live State")]
    private string CmdGiveGold(int amount)
    {
        // Enqueue a command to modify the Simulation Thread safely (Zero-Reference pattern)
        var cmd = new DevGiveGoldCommand(amount);
        ServiceLocator.Simulation.QueueCommand(cmd);
        return $"Queued command to give {amount} gold.";
    }

    [ConsoleCommand("heal", "Fully restores player HP")]
    private string CmdHeal()
    {
        ServiceLocator.Simulation.QueueCommand(new DevHealCommand());
        return "Queued command to heal player.";
    }
}

// Support Commands for Simulation Queue
public class DevGiveGoldCommand : ICommand
{
    private readonly int _amount;
    public DevGiveGoldCommand(int amount) => _amount = amount;

    public bool Execute(GameState_Live liveState)
    {
        liveState.PlayerGold += _amount;
        ServiceLocator.Logger.LogInfo($"DevConsole: Added {_amount} gold.");
        return true; // Mutated state
    }
}

public class DevHealCommand : ICommand
{
    public bool Execute(GameState_Live liveState)
    {
        liveState.PlayerHP = 999; // Arbitrary max for demo
        ServiceLocator.Logger.LogInfo("DevConsole: Player healed.");
        return true;
    }
}
