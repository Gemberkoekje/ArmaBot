using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using ArmaBotCs.LocalId;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArmaBotCs.Posts;

/// <summary>
/// Provides functionality to manage and update mission posts within a Discord guild, including adding, updating, and retrieving posts.
/// </summary>
public sealed class PostUpdater : IPostRepository
{
    private readonly List<Post> Posts;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostUpdater"/> class and loads existing posts from the database.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies and database sessions.</param>
    public PostUpdater(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        using var session = serviceProvider.GetService<IDocumentStore>().QuerySession();
        Posts = session.Query<Post>().ToList();
    }

    /// <summary>
    /// Adds a new post to the repository and updates the corresponding mission post in Discord.
    /// </summary>
    /// <param name="post">The <see cref="Post"/> object containing post details to add.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    public async Task AddPostAsync(Post post)
    {
        using var scope = _serviceProvider.CreateScope();
        using var session = _serviceProvider.GetService<IDocumentStore>().LightweightSession();
        session.Store(post);
        await session.SaveChangesAsync();
        var savedPost = Posts.FirstOrDefault(r => r.Id == post.Id);
        if (savedPost != null)
        {
            Posts.Remove(savedPost);
            savedPost = savedPost with { PostId = post.PostId };
            Posts.Add(savedPost);
        }
        else
        {
            Posts.Add(post);
        }

        await UpdatePostAsync(post.Guild, post.Id);
    }

    /// <summary>
    /// Updates an existing mission post in Discord for a specific guild and mission.
    /// If the post does not exist, it is created and stored.
    /// </summary>
    /// <param name="guild">The Discord guild identifier.</param>
    /// <param name="missionId">The unique identifier of the mission to update the post for.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdatePostAsync(Snowflake guild, Guid missionId)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetService<IAggregateRepository<Guid, Mission>>();
        var channelApi = scope.ServiceProvider.GetService<IDiscordRestChannelAPI>();
        var mission = await repository.LoadAsync(missionId);
        var post = Posts.FirstOrDefault(r => r.Id == missionId);
        if (post == null)
        {
            post = new Post()
            {
                Id = missionId,
                PostId = null,
                Op = mission.GetMissionData().Op,
                MissionDate = mission.GetMissionData().Date,
                Guild = guild,
            };
            Posts.Add(post);
            using var session = _serviceProvider.GetService<IDocumentStore>().LightweightSession();
            session.Store(post);
            await session.SaveChangesAsync();
        }
        var localIdRepository = _serviceProvider.GetService<ILocalIdRepository>();
        if (mission != null && post.PostId != null)
        {
            var result = await channelApi.EditMessageAsync(
                mission.GetMissionData().Channel, // This should be a Snowflake
                post.PostId.Value,
                $"<@&{mission.GetMissionData().RoleToPing}>",
                embeds: new[] { mission.GetMissionDataEmbed(await localIdRepository.GetOrAddLocalIdAsync(mission.Id)), mission.GetMissionSidesEmbed(), mission.GetMissionResponsesEmbed() });
            if (!result.IsSuccess)
            {
                Console.WriteLine("Failed to update post: " + result.Error.Message);
            }
        }
    }

    /// <summary>
    /// Retrieves the most recent mission post identifier for a given guild.
    /// </summary>
    /// <param name="guild">The Discord guild identifier.</param>
    /// <returns>
    /// The <see cref="Guid"/> of the most recent post, or <c>null</c> if no posts exist for the guild.
    /// </returns>
    public Guid? GetMostRecentPost(Snowflake guild)
    {
        if (!Posts.Any(p => p.Guild == guild && p.PostId != null))
            return null;
        return Posts.Where(p => p.Guild == guild && p.PostId != null).OrderBy(o => o.MissionDate).Last().Id;
    }

    /// <summary>
    /// Retrieves all mission posts for a given guild, ordered by mission date.
    /// </summary>
    /// <param name="guild">The Discord guild identifier.</param>
    /// <returns>
    /// A list of <see cref="Post"/> objects representing all posts for the specified guild.
    /// </returns>
    public List<Post> GetAllPosts(Snowflake guild)
    {
        return Posts.Where(p => p.Guild == guild).OrderBy(o => o.MissionDate).ToList();
    }
}
