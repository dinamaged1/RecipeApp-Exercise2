using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json.Serialization;


namespace Exercise1
{
    public class Recipe
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("instructions")]
        public List<string> Instructions { get; set; } = new();
        [JsonPropertyName("ingredients")]
        public List<string> Ingredients { get; set; } = new();
        [JsonPropertyName("categories")]
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
