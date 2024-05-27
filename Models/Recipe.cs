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
        public string ImageUrl => $"https://master-project-s3.s3.eu-north-1.amazonaws.com/images/{Id}.jpg";
    public ICollection<Ingredient> Ingredients { get; set; }
        public ICollection<Recipe_Tag> Recipe_Tag { get; set; }

        public Recipe()
        {

        }
    }
}

