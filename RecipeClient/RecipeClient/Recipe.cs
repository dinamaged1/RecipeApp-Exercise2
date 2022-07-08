using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Exercise1
{
    public class Recipe
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Instructions { get; set; } = new();
        public List<string> Ingredients { get; set; } = new();
        public List<string> Categories { get; set; } = new();

        public Recipe(Guid id, string title, List<string> instructions, List<string> ingredients, List<string> categories)
        {
            Id = id;
            Title = title;
            Instructions = instructions;
            Ingredients = ingredients;
            Categories = categories;
        }
    }
}
