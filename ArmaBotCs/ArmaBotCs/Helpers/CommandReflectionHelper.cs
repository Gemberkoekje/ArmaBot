#nullable enable
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ArmaBotCs.Helpers;

/// <summary>
/// Provides reflection-based utilities for discovering command groups and their commands in the application.
/// </summary>
public static class CommandReflectionHelper
{
    /// <summary>
    /// Retrieves all commands defined in command groups, including their group name, command name, description, and whether they require ManageChannels.
    /// </summary>
    /// <returns>
    /// An enumerable of tuples containing the group name, command name, command description (if available), and a bool indicating if ManageChannels is required.
    /// </returns>
    public static IEnumerable<(string Group, string Command, string? Description, bool IsAdministratorCommand)> GetAllCommands()
    {
        var commandGroups = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(CommandGroup).IsAssignableFrom(t));

        foreach (var group in commandGroups)
        {
            var groupName = group.Name.Replace("Command", string.Empty);
            var methods = group.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var commandAttr = method.GetCustomAttribute<CommandAttribute>();
                if (commandAttr == null)
                    continue;

                var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                var permAttr = method.GetCustomAttributes()
                    .OfType<RequireDiscordPermissionAttribute>()
                    .FirstOrDefault(a => a.Permissions.Contains(DiscordPermission.ManageChannels));

                yield return (
                    Group: groupName,
                    Command: commandAttr.Name,
                    Description: descAttr?.Description,
                    IsAdministratorCommand: permAttr != null
                );
            }
        }
    }
}
