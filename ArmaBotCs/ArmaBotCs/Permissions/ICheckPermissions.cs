using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmaBotCs.Permissions;

internal interface ICheckPermissions
{
    Task<bool> CheckPermissionsAsync(ICommandContext context, DiscordPermission permission);
}
