using Remora.Rest.Core;
using System;

namespace ArmaBotCs.Posts;

public sealed record Post
{
    required public Guid Id { get; init; }

    required public string Op { get; init; }

    required public Snowflake? PostId { get; init; }

    required public DateTime MissionDate { get; init; }
}
