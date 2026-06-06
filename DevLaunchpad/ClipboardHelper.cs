// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only

using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.ApplicationModel.DataTransfer;

namespace DevLaunchpad;

/// <summary>
/// Minimal clipboard helper. The Command Palette toolkit has no generic copy command, so
/// extensions provide their own; this mirrors the pattern used by built-in extensions.
/// </summary>
internal static class ClipboardHelper
{
    public static CommandResult CopyText(string text, string successToast)
    {
        if (string.IsNullOrEmpty(text))
        {
            return CommandResult.ShowToast("Nothing to copy.");
        }

        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);

        return CommandResult.ShowToast(successToast);
    }
}
