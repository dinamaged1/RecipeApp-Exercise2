using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System;

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
    if (recipesList != null)
        return Results.Ok(recipesList);
    else 
        return Results.NoContent();
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

//Remove recipe
app.MapDelete("/recipe/{id}", (() =>
{

});

//Get all categories
app.MapGet("/categories", () =>
{
    if (categoryList != null)
        return Results.Ok(categoryList);
    else
        return Results.NoContent();
});

//Add category
app.MapPost("/category", async (string newCategory) =>
{
    if (!categoryList.Contains(newCategory) && newCategory != "")
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

//Edit category
app.MapPut("/category/{name}", async (string oldCategoryName, string newCategoryName) =>
{
    int indexOfCategory = categoryList.FindIndex(x => x == oldCategoryName);
    if (indexOfCategory != -1 && !categoryList.Contains(newCategoryName) && newCategoryName != "")
    {
        categoryList[indexOfCategory] = newCategoryName;
        for (int i = 0; i < recipesList.Count; i++)
        {
            for (int j = 0; j < recipesList[i].Categories.Count; j++)
            {
                if (recipesList[i].Categories[j] == oldCategoryName)
                {
                    recipesList[i].Categories[j] = newCategoryName;
                }
            }
        }
        await saveCategoryToJson();
        return Results.Ok(categoryList);
    }
    else
    {
        return Results.BadRequest();
    }
});

//Delete Category
app.MapDelete("category/{name}", (string categoryName) =>
{
    if (categoryList.Contains(categoryName))
    {
        categoryList.Remove(categoryName);
        return Results.Ok(categoryList);
    }
    else
    {
        return Results.NotFound();
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
