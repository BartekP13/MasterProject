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
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace MasterProject.Controllers
{
    [Authorize] 
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

        private async Task<List<int>> GetRecommendedRecipesAsync(string userId)
        {
            var apiUrl = $"https://flaskrecommenderapp.azurewebsites.net/recommend?user_id={userId}";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetStringAsync(apiUrl);
                    var jsonResponse = JObject.Parse(response);
                    var recommendedRecipeIds = jsonResponse["recommendations"].ToObject<List<int>>();

                    return recommendedRecipeIds;
                }
            }
            catch (Exception ex)
            {
                // Zaloguj błąd lub obsłuż wyjątek według potrzeb
                return new List<int>();
            }
        }

        // GET: Recipes
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var recipes = from r in _context.Recipe select r;

            if (!String.IsNullOrEmpty(searchString))
            {
                recipes = recipes.Where(r => r.Name.Contains(searchString));
            }

            // Sortowanie przepisów losowo
            recipes = recipes.OrderBy(r => Guid.NewGuid());

            int pageSize = 21;
            int pageNumber = (page ?? 1);

            var totalItems = await recipes.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            var paginatedRecipes = await recipes.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            // Pobieranie rekomendowanych przepisów
            var user = await _userManager.GetUserAsync(User);
            List<Recipe> recommendedRecipes = new List<Recipe>();

            if (user != null)
            {
                var recommendedRecipeIds = await GetRecommendedRecipesAsync(user.Id);
                if (recommendedRecipeIds != null && recommendedRecipeIds.Count > 0)
                {
                    recommendedRecipes = await _context.Recipe
                        .Where(r => recommendedRecipeIds.Contains(r.Id))
                        .ToListAsync();
                }
            }

            // Przekazanie rekomendowanych przepisów do widoku
            ViewBag.RecommendedRecipes = recommendedRecipes;

            return View(paginatedRecipes);
        }

        private async Task<List<int>> GetSimilarRecipesAsync(int recipeId)
        {
            var apiUrl = $"https://flaskrecommenderapp.azurewebsites.net/similar_recipes/{recipeId}";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetStringAsync(apiUrl);
                    var jsonResponse = JArray.Parse(response);
                    var similarRecipeIds = jsonResponse.ToObject<List<int>>();

                    return similarRecipeIds;
                }
            }
            catch (Exception ex)
            {
                // Zaloguj błąd lub obsłuż wyjątek według potrzeb
                return new List<int>();
            }
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
                    .ThenInclude(i => i.IngredientNames) 
                .Include(r => r.Recipe_Tag)
                    .ThenInclude(rt => rt.Tag)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Pobierz nazwy tagów
            var tagNames = recipe.Recipe_Tag
                .Where(rt => rt.Tag != null)
                .Select(rt => rt.Tag.Name)
                .ToList();

            ViewBag.TagNames = tagNames;

            // Pobierz podobne przepisy
            var similarRecipeIds = await GetSimilarRecipesAsync(recipe.Id);
            var similarRecipes = new List<Recipe>();

            if (similarRecipeIds.Count > 0)
            {
                similarRecipes = await _context.Recipe
                    .Where(r => similarRecipeIds.Contains(r.Id))
                    .ToListAsync();
            }

            ViewBag.SimilarRecipes = similarRecipes;

            return View(recipe);
        }
    }
}
