namespace MasterProject.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Carbohydrates { get; set; }
        public string Fat { get; set; }
        public string Fiber { get; set; }
        public string Kcal { get; set; }
        public string Name { get; set; }
        public string Protein { get; set; }
        public ICollection<Ingredient> Ingredients { get; set; }

        public Recipe()
        {

        }
    }
}

