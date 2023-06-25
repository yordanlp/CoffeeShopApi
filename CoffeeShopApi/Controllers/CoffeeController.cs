using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShopApi;
using CoffeeShopApi.Models;
using System.Text;
using CoffeeShopApi.ActionFilters;

namespace CoffeeShopApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoffeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CoffeeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Coffee>>> GetCoffee()
        {
            return await _context.Coffees.ToListAsync();
        }

        
        [HttpGet("favourite")]
        public async Task<ActionResult<Object>> Favourite()
        {
            var user = await GetUserFromAuthHeader();

            if (user == null)
            {
                return Unauthorized();
            }

            var favouriteCoffee = await _context.Coffees.FirstOrDefaultAsync(c => c.Id == user.CoffeeId);

            if (favouriteCoffee == null)
            {
                return NotFound("No favourite coffee set for this user.");
            }

            return new { data = new { favouriteCoffee = favouriteCoffee.Name } };
        }

        [HttpGet("favourite/leaderboard")]
        [RateLimit(3, 1, RateLimitType.USER)]
        public async Task<ActionResult<Object>> LeaderBoard()
        {
            var leaderboard = await GetLeaderBoard();

            return new { data = new { top3 = leaderboard.Select(coffee => coffee.Name).ToArray() } };
        }

        [RateLimit(10, 1, RateLimitType.IP)]
        [HttpPost("favourite")]
        public async Task<ActionResult<Object>> SetFavourite( [FromBody]int coffeeId )
        {
            var user = await GetUserFromAuthHeader();

            if (user == null)
            {
                return Unauthorized();
            }

            var favouriteCoffee = await _context.Coffees.FirstOrDefaultAsync(c => c.Id == coffeeId);

            if (favouriteCoffee == null)
            {
                return NotFound("The coffee provided was not found");
            }

            user.CoffeeId = coffeeId;
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            var leaderboard = await GetLeaderBoard();

            return new { data = new { top3 = leaderboard.Select(coffee => coffee.Name).ToArray() } };
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Coffee>> GetCoffee(int id)
        {
            var coffee = await _context.Coffees.FindAsync(id);

            if (coffee == null)
            {
                return NotFound();
            }

            return coffee;
        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCoffee(int id, Coffee coffee)
        {
            if (id != coffee.Id)
            {
                return BadRequest();
            }

            _context.Entry(coffee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoffeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Coffee>> PostCoffee(Coffee coffee)
        {
            _context.Coffees.Add(coffee);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCoffee", new { id = coffee.Id }, coffee);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoffee(int id)
        {
            var coffee = await _context.Coffees.FindAsync(id);
            if (coffee == null)
            {
                return NotFound();
            }

            _context.Coffees.Remove(coffee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CoffeeExists(int id)
        {
            return _context.Coffees.Any(e => e.Id == id);
        }

        private async Task<List<Coffee>> GetLeaderBoard()
        {
            return await _context.Coffees.OrderByDescending(coffe => coffe.UsersWhoLikeThis.Count).Take(3).ToListAsync();
        }

        private async Task<User> GetUserFromAuthHeader()
        {
            string authHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic"))
            {
                return null;
            }

            string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
            var encoding = Encoding.GetEncoding("iso-8859-1");
            string usernamePassword;
            try
            {
                usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
            }
            catch
            {
                return null;
            }

            var seperatorIndex = usernamePassword.IndexOf(':');
            var username = usernamePassword.Substring(0, seperatorIndex);
            var password = usernamePassword.Substring(seperatorIndex + 1);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username && u.Password == password);
            return user;
        }
    }
}
