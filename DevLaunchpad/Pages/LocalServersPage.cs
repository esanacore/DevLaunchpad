using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;

namespace DevLaunchpad.Pages;

public sealed partial class LocalServersPage : ListPage
{
    public LocalServersPage()
    {
        Title = "Local Servers";
        Name = "local-servers";
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();
        var config = DevLaunchpadConfig.Load();

        if (config.LocalUrls == null || config.LocalUrls.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No local URLs configured",
                Subtitle = "Add entries to config.json"
            });

            return items.ToArray();
        }

        foreach (var entry in config.LocalUrls)
        {
            if (string.IsNullOrWhiteSpace(entry.Title) || string.IsNullOrWhiteSpace(entry.Url))
            {
                continue;
            }

            items.Add(new ListItem(new OpenUrlCommand(entry.Url))
            {
                Title = $"Open {entry.Title}",
                Subtitle = entry.Url
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No valid local URLs configured",
                Subtitle = "Check config.json"
            });
        }

        return items.ToArray();
    }

    private sealed partial class NoOpCommand : InvokableCommand
    {
        public override CommandResult Invoke() => CommandResult.KeepOpen();
    }
}