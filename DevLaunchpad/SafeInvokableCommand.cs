using System;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace DevLaunchpad;

/// <summary>
/// An <see cref="InvokableCommand"/> that runs a delegate returning a <see cref="CommandResult"/>
/// and guarantees the call never throws out of <c>Invoke</c>. Any exception is logged and surfaced
/// to the user as a toast, keeping the out-of-process COM server alive.
/// </summary>
internal sealed partial class SafeInvokableCommand : InvokableCommand
{
    private readonly Func<CommandResult> _action;

    public SafeInvokableCommand(string name, Func<CommandResult> action)
    {
        Name = name;
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public override CommandResult Invoke()
    {
        try
        {
            return _action();
        }
        catch (Exception ex)
        {
            DevLaunchpadConfig.WriteDebugLog($"Command '{Name}' failed: {ex}");
            return CommandResult.ShowToast($"Action failed: {ex.Message}");
        }
    }
}
