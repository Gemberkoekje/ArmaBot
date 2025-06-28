using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.API.Abstractions.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArmaBotCs.Posts;

public sealed class PostUpdater : IPostRepository
{
    private List<Post> Posts;
    private readonly IServiceProvider _serviceProvider;

    public PostUpdater(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        using var session = serviceProvider.GetService<IDocumentStore>().QuerySession();
        Posts = session.Query<Post>().ToList();
    }

    public async Task AddPost(Post post)
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

        await UpdatePost(post.Id);
    }

    public async Task UpdatePost(Guid missionId)
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
            };
            Posts.Add(post);
            using var session = _serviceProvider.GetService<IDocumentStore>().LightweightSession();
            session.Store(post);
            await session.SaveChangesAsync();
        }

        if (mission != null && post.PostId != null)
        {
            var result = await channelApi.EditMessageAsync(
                mission.GetMissionData().Channel, // This should be a Snowflake
                post.PostId.Value,
                $"<@&{mission.GetMissionData().RoleToPing}>",
                embeds: new[] { mission.GetMissionDataEmbed(), mission.GetMissionSidesEmbed(), mission.GetMissionResponsesEmbed() }
            );
            if (!result.IsSuccess)
            {
                Console.WriteLine("Failed to update post: " + result.Error.Message);
            }
        }
    }

    public Guid? GetMostRecentPost()
    {
        if (!Posts.Any(p => p.PostId != null))
            return null;
        return Posts.Where(p => p.PostId != null).OrderBy(o => o.MissionDate).Last().Id;
    }

    public List<Post> GetAllPosts()
    {
        return Posts.OrderBy(o => o.MissionDate).ToList();
    }
}
