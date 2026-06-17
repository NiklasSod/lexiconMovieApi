using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieApi.DTOs.Actor;
using MovieApi.Models;

[Route("api/[controller]")]
[ApiController]
public class ActorsController : ControllerBase
{
    private readonly MovieApiContext _context;
    private readonly IMapper _mapper;
    public ActorsController(MovieApiContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/Actors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Actor>>> GetActor()
    {
        var actors = await _context.Actors.ToListAsync();
        return Ok(actors);
    }

    // GET: api/Actors/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Actor>> GetActor(int id)
    {
        var actor = await _context.Actors.FindAsync(id);

        if (actor == null)
        {
            return NotFound();
        }

        return Ok(actor);
    }

    // PUT: api/Actors/5
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutActor(int? id, ActorUpdateDto actorDto)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null)
        {
            return NotFound();
        }

        _mapper.Map(actorDto, actor);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ActorExists(id)) { return NotFound(); }
            else { throw; }
        }

        return NoContent();
    }

    // POST: api/Actors
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Actor>> PostActor(ActorCreateDto actorDto)
    {
        var actor = _mapper.Map<Actor>(actorDto);
        _context.Actors.Add(actor);
        await _context.SaveChangesAsync();

        var returnDto = _mapper.Map<ActorDto>(actor);
        return CreatedAtAction("GetActor", new { id = returnDto.Id }, returnDto);
    }

    // DELETE: api/Actors/5
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActor(int? id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null)
        {
            return NotFound();
        }

        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ActorExists(int? id)
    {
        return _context.Actors.Any(e => e.Id == id);
    }
}
