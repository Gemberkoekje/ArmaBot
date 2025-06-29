using Remora.Rest.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArmaBotCs.Posts;

/// <summary>
/// Defines a contract for managing mission-related posts within a Discord guild.
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Adds a new post entry to the repository.
    /// </summary>
    /// <param name="post">The <see cref="Post"/> object containing post details to add.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddPostAsync(Post post);

    /// <summary>
    /// Updates an existing post for a specific guild and mission.
    /// </summary>
    /// <param name="guild">The Discord guild identifier.</param>
    /// <param name="missionId">The unique identifier of the mission to update the post for.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdatePostAsync(Snowflake guild, Guid missionId);

    /// <summary>
    /// Retrieves the most recent mission post identifier for a given guild.
    /// </summary>
    /// <param name="guild">The Discord guild identifier.</param>
    /// <returns>
    /// The <see cref="Guid"/> of the most recent post, or <c>null</c> if no posts exist for the guild.
    /// </returns>
    Guid? GetMostRecentPost(Snowflake guild);

    /// <summary>
    /// Retrieves all mission posts for a given guild.
    /// </summary>
    /// <param name="guild">The Discord guild identifier.</param>
    /// <returns>
    /// A list of <see cref="Post"/> objects representing all posts for the specified guild.
    /// </returns>
    List<Post> GetAllPosts(Snowflake guild);
}
