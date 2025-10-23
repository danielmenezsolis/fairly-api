using FairlyApi.Data;
using FairlyApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace FairlyApi.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class UsersController : ControllerBase
    {
        private readonly FairlyDbContext _context;
        public UsersController(FairlyDbContext context)
        {
            _context = context; 
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUser(Guid id)
        {
            var user =  await _context.Users.FindAsync(id);
            if (user == null) {
                return NotFound(new { message = "Usuario no encontrado" });

            }
            return Ok(user);
        }

        [HttpGet("{id}/groups")]
        public async Task<ActionResult<IEnumerable<Group>>> GetUserGroups(Guid id)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (userExists == null)
            {
                return NotFound("Usuario No encontrado");
            }

            var groups = await _context.GroupMembers.Where(gm=> gm.UserId == id)
                .Include(gm=> gm.Group)
                .ThenInclude(gm=>gm.Creator).Select(gm=>gm.Group)
                .ToListAsync();

            return Ok(groups);
        }

        [HttpPost]
        public async Task<ActionResult<Users>> CreateUser(Users user)
        {
            if (await _context.Users.AnyAsync(us => us.Email == user.Email))
            {
                return BadRequest(new { message = "Email ya registrado " });
            }

            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            user.CreatedAt = DateTime.UtcNow;

            _context.Add(user);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);

        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, Users user)
        {
            if (id != user.Id)
            {
                return BadRequest(new { message = "El ID no coincide" });
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Actualizar solo los campos permitidos
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.ProfilePictureUrl = user.ProfilePictureUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
