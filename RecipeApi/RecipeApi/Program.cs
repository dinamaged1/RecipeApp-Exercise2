using Microsoft.AspNetCore.Mvc;
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

//Get all recipes
app.MapGet("/recipes", () =>
{
    return Results.Ok(recipesList);
}).WithName("GetRecipes");

//Add new recipe
app.MapPost("/recipe", async ([FromBody] Recipe newRecipe) =>
{
    recipesList.Add(newRecipe);
    await SaveRecipeToJson();
    return Results.Ok(recipesList);
});

//Edit recipe
app.MapPut("/recipe/{id}", async (Guid id, [FromBody] Recipe newRecipeData) =>
{
    var selectedRecipeIndex = recipesList.FindIndex(x => x.Id == id);
    if (selectedRecipeIndex != -1)
    {
        recipesList[selectedRecipeIndex] = newRecipeData;
        await SaveRecipeToJson();
        return Results.Ok(recipesList);
    }
    else
    {
        return Results.NotFound();
    }
});

//Add category
app.MapPost("/category", async (string newCategory) =>
{
    if (!categoryList.Contains(newCategory))
    {
        categoryList.Add(newCategory);
        await saveCategoryToJson();
        return Results.Ok(categoryList);
    }
    else
    {
        return Results.BadRequest();
    }
    
});

app.Run();

async Task<string> ReadJsonFile(string fileName) =>
await File.ReadAllTextAsync($"{fileName}.json");

async Task WriteJsonFile(string fileName, string fileData) =>
await File.WriteAllTextAsync($"{fileName}.json", fileData);

async Task SaveRecipeToJson()
{
    string jsonString = JsonSerializer.Serialize(recipesList);
    await WriteJsonFile("recipe", jsonString);
}

async Task saveCategoryToJson()
{
    string jsonString = JsonSerializer.Serialize(categoryList);
    await WriteJsonFile("category", jsonString);
}
