using app.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace app.Controllers;

[ApiController]
[Route("[controller]")]
public class BlogController : ControllerBase
{

    private readonly ApplicationDbContext _db;

    public BlogController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult<List<Blog>> Index()
    {
        return _db.Blogs.ToList();
    }

    [HttpGet("{id}")]
    public ActionResult<Blog> Show(int id)
    {
        Blog blog = _db.Blogs.Find(id);
        if (blog == null)
            return NotFound();

        return blog;
    }

    [HttpPost]
    public IActionResult Create(Blog blog)
    {
        _db.Blogs.Add(blog);
        _db.SaveChanges();
        return CreatedAtAction(nameof(Show), new { id = blog.Id }, blog);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Blog blog)
    {
        if (id != blog.Id)
            return BadRequest();

        Blog existingBlog = _db.Blogs.Find(id);
        blog.CreatedDate = existingBlog.CreatedDate;
        _db.Entry(existingBlog).State = EntityState.Detached;

        if (existingBlog is null)
            return NotFound();

        _db.Blogs.Update(blog);
        try
        {
            _db.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        Blog blog = _db.Blogs.Find(id);
        if (blog == null)
            return NotFound();

        _db.Blogs.Remove(blog);
        _db.SaveChanges();
        return NoContent();
    }
}