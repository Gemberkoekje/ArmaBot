using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArmaBotCs.Posts;

public interface IPostRepository
{
    public Task AddPost(Post post);

    public Task UpdatePost(Guid missionId);

    public Guid? GetMostRecentPost();

    public List<Post> GetAllPosts();
}
