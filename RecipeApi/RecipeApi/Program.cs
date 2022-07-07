using Microsoft.EntityFrameworkCore;
using RecipeApi.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Desrialize recipe file and category file
List<Recipe>? savedRecipes = new();
List<String>? savedCategories = new();
try
{
    string recipeJson = await ReadJsonFile("recipe");
    string categoryJson = await ReadJsonFile("category");
    savedRecipes = JsonSerializer.Deserialize<List<Recipe>>(recipeJson);
    savedCategories = JsonSerializer.Deserialize<List<string>>(categoryJson);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return;
}

//Create list of recipes and list of categories
List<Recipe> recipesList = new List<Recipe>(savedRecipes!);
List<string> categoryList = new List<string>(savedCategories!);

app.Run();


static async Task<string> ReadJsonFile(string fileName) =>
await File.ReadAllTextAsync($"{fileName}.json");

static async Task WriteJsonFile(string fileName, string fileData) =>
await File.WriteAllTextAsync($"{fileName}.json", fileData);