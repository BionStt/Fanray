using Fan.Accounts;
using Fan.Blogs.Api.Models;
using Fan.Blogs.Enums;
using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Fan.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blogs.Api
{
    [Route("api/blog/[controller]")]
    [EnableCors("CorsPolicy")]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = SystemRoles.Admininistrator)]
    public class PostsController : Controller
    {
        private readonly IBlogService _blogSvc;
        public PostsController(IBlogService blogService)
        {
            _blogSvc = blogService;
        }

        /// <summary>
        /// GET api/blog/posts
        /// </summary>
        /// <param name="status">What kind posts. Default <see cref="EPostStatus.Published"/>.</param>
        /// <param name="page">Which page. Default 1.</param>
        /// <param name="size">Page size. Default 20.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<BlogPostListVM> Get(EPostStatus status = EPostStatus.Published, int page = 1, int size = 20)
        {
            var blogPostList = await _blogSvc.GetPostsAsync(page);
            return new BlogPostListVM(blogPostList);
        }

        /// <summary>
        /// GET api/blog/posts/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<BlogPostVM> Get(int id)
        {
            var blogPost = await _blogSvc.GetPostAsync(id);
            return new BlogPostVM(blogPost);
        }

        /// <summary>
        /// POST api/blog/posts, returns 201 on success, 400 on error.
        /// </summary>
        /// <param name="post"></param>
        [HttpPost]
        [Produces(typeof(BlogPostVM))]
        public async Task<IActionResult> Post([FromBody]BlogPostIM post)
        {
            var blogPost = new BlogPost
            {
                UserId = post.UserId,
                Title = post.Title,
                Slug = post.Slug,
                Body = post.Body,
                Excerpt = post.Excerpt,
                CreatedOn = post.CreatedOn,
                CategoryTitle = post.CategoryTitle,
                TagTitles = new List<string>(post.TagTitles.Split(',')),
                Status = post.Status,
                CommentStatus = ECommentStatus.AllowComments, // hardcode todo
            };

            blogPost = await _blogSvc.CreatePostAsync(blogPost);
            return CreatedAtAction("Get", new { id = blogPost.Id }, new BlogPostVM(blogPost));
        }

        /// <summary>
        /// PUT api/blog/posts/5, returns 200 with updated <see cref="BlogPostVM"/>, or 400 on failure.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="post"></param>
        /// <remarks>
        /// https://twitter.com/FanrayMedia/status/944243856425467904
        /// </remarks>
        [HttpPut("{id:int}")]
        public async Task<BlogPostVM> Put(int id, [FromBody]BlogPostIM post)
        {
            var blogPost = new BlogPost
            {
                Id = id,
                UserId = post.UserId,
                Title = post.Title,
                Slug = post.Slug,
                Body = post.Body,
                Excerpt = post.Excerpt,
                CreatedOn = post.CreatedOn,
                CategoryTitle = post.CategoryTitle,
                TagTitles = new List<string>(post.TagTitles.Split(',')),
                Status = post.Status,
                CommentStatus = ECommentStatus.AllowComments, // todo
            };

            blogPost = await _blogSvc.UpdatePostAsync(blogPost);
            return new BlogPostVM(blogPost);
        }

        /// <summary>
        /// DELETE api/blog/posts/5
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id:int}")]
        public async Task Delete(int id)
        {
            await _blogSvc.DeletePostAsync(id);
        }
    }
}
