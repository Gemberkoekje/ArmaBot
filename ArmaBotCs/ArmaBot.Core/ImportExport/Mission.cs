using Remora.Rest.Core;
using System;
using System.Collections.Immutable;

namespace ArmaBot.Core.ImportExport;

public sealed class Mission
{
    public string Campaign { get; init; }

    public string Modset { get; init; }

    public string Op { get; init; }

    public DateTime Date { get; init; }

    public string Description { get; init; }

    public Snowflake Channel { get; init; }

    public Snowflake RoleToPing { get; init; }

    public ImmutableArray<Side> Sides { get; init; } = [];

    public sealed class Side
    {
        public Enums.Side SideName { get; init; }

        public ImmutableArray<Division> Divisions { get; init; } = [];

        public sealed class Division
        {
            public string Name { get; init; } = string.Empty;

            public sealed class Subdivision
            {
                public string Name { get; init; } = string.Empty;

                public ImmutableArray<Enums.Role> Roles { get; init; } = [];
            }

            public ImmutableArray<Subdivision> Subdivisions { get; init; } = [];
        }
    }
}
