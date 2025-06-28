using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmaBot.Core.Models;

public sealed record SimplifiedMission
{
    public Guid Id { get; init; }

    public string Op { get; init; } = string.Empty;
}
