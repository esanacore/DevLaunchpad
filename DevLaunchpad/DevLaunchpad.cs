// Copyright (c) Eric Sanacore
// SPDX-License-Identifier: GPL-3.0-only
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;

namespace DevLaunchpad;

/// <summary>
/// Main extension class for the Dev Launchpad Visual Studio Command Palette extension.
/// 
/// This class serves as the entry point for the extension and implements the IExtension interface
/// to integrate with the Visual Studio Command Palette ecosystem. It manages the lifecycle of
/// the extension and provides access to command providers.
/// </summary>
/// <remarks>
/// The extension is registered with a specific GUID to ensure unique identification within Visual Studio.
/// Implements IDisposable to properly clean up resources and signal completion via ManualResetEvent.
/// </remarks>
[Guid("3ccae5b0-6a8e-4a44-98f7-d5a7b7bcbb43")]
public sealed partial class DevLaunchpad : IExtension, IDisposable
{
    /// <summary>
    /// Event used to signal that the extension has been disposed and cleanup is complete.
    /// </summary>
    private readonly ManualResetEvent _extensionDisposedEvent;

    /// <summary>
    /// Provider for managing commands exposed through the Command Palette interface.
    /// </summary>
    private readonly DevLaunchpadCommandsProvider _provider = new();

    /// <summary>
    /// Initializes a new instance of the DevLaunchpad extension.
    /// </summary>
    /// <param name="extensionDisposedEvent">
    /// ManualResetEvent that will be signaled when the extension is disposed.
    /// Used by the host to wait for extension cleanup completion.
    /// </param>
    public DevLaunchpad(ManualResetEvent extensionDisposedEvent)
    {
        this._extensionDisposedEvent = extensionDisposedEvent;
    }

    /// <summary>
    /// Retrieves the provider for a specific provider type.
    /// </summary>
    /// <param name="providerType">The type of provider being requested.</param>
    /// <returns>
    /// The Commands provider if ProviderType.Commands is requested; otherwise null.
    /// </returns>
    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    /// <summary>
    /// Disposes the extension and signals completion to the host.
    /// </summary>
    public void Dispose() => this._extensionDisposedEvent.Set();
}
