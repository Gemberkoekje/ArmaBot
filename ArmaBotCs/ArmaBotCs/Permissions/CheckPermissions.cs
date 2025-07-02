using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace ArmaBotCs.Permissions;

public sealed class CheckPermissions(IDiscordRestGuildAPI guildApi, IDiscordRestChannelAPI channelApi) : ICheckPermissions
{
    public async Task<bool> CheckPermissionsAsync(ICommandContext context, DiscordPermission permission)
    {
        if (!context.TryGetGuildID(out var guildId) || !context.TryGetUserID(out var userId))
            return false;

        var memberResult = await guildApi.GetGuildMemberAsync(guildId, userId);
        if (!memberResult.IsSuccess || memberResult.Entity is not { } member)
            return false;

        var guildResult = await guildApi.GetGuildAsync(guildId);
        if (!guildResult.IsSuccess || guildResult.Entity is not { } guild)
            return false;

        if(guild.OwnerID == userId)
        {
            // The user is the owner of the guild, they have all permissions
            return true;
        }

        var guildRoleResult = await guildApi.GetGuildRolesAsync(guildId);
        if (!guildRoleResult.IsSuccess || guildRoleResult.Entity is not { } guildRoles)
            return false;

        foreach (var role in member.Roles)
        {
            var permissions = guildRoles.FirstOrDefault(r => r.ID == role)?.Permissions;
            if (permissions.HasPermission(permission))
            {
                return true;
            }
        }
        if (member.Permissions.HasValue)
        {
            return member.Permissions.Value.HasPermission(permission);
        }

        return false;
    }
}
