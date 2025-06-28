using System;

namespace ArmaBot.Infrastructure.Extensions;

public static class GuidExtension
{
    public static Guid ToGuid(this Guid Guid)
    {
        return Guid.Parse(Guid.ToString());
    }
}
