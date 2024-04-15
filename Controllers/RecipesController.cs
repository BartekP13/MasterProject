using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MasterProject.Data;
using MasterProject.Models;
using Microsoft.AspNetCore.Identity;

namespace MasterProject.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RecipesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddRating(int recipeId, int ratingValue)
        {
            // Pobierz zalogowanego użytkownika
            var user = await _userManager.GetUserAsync(User);

            // Sprawdź, czy użytkownik jest zalogowany
            if (user == null)
            {
                return Unauthorized(); // Użytkownik niezalogowany
            }

            // Pobierz przepis na podstawie jego identyfikatora
            var recipe = await _context.Recipe.FindAsync(recipeId);

            // Sprawdź, czy przepis o podanym identyfikatorze istnieje
            if (recipe == null)
            {
                return NotFound("Recipe not found.");
            }

            // Sprawdź, czy użytkownik ocenił już ten przepis
            var existingRating = await _context.Ratings.FirstOrDefaultAsync(r => r.UserId == user.Id && r.RecipeId == recipeId);

            if (existingRating != null)
            {
                // Użytkownik już ocenił ten przepis
                return Conflict("User already rated this recipe.");
            }

            // Utwórz nowy obiekt Rating
            var rating = new Ratings
            {
                UserId = user.Id,
                RecipeId = recipeId,
                Rating = ratingValue
            };

            // Dodaj ocenę do DbSet w kontekście
            _context.Ratings.Add(rating);

            // Zapisz zmiany w bazie danych
            await _context.SaveChangesAsync();

            return Ok(); // Pomyślnie dodano ocenę
        }



        // GET: Recipes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Recipe.ToListAsync());
        }
        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipe
                .Include(r => r.Ingredients)
                .Include(r => r.Recipe_Tag)
                    .ThenInclude(rt => rt.Tag) // Ładuje tagi dla Recipe_Tag
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Pobierz nazwy tagów
            var tagNames = recipe.Recipe_Tag
                .Where(rt => rt.Tag != null) // Upewnij się, że Tag nie jest null
                .Select(rt => rt.Tag.Name)
                .ToList();

            // Przekaż nazwy tagów do widoku
            ViewBag.TagNames = tagNames;

            return View(recipe);

        }
    }
}
