using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Exercise1;
using System.Text.Json;
using Spectre.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// Build a config object and get url
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();
var url = config.GetRequiredSection("url").Get<string>();

//Create HttpClient and add base address
HttpClient client = new HttpClient();
client.BaseAddress = new Uri(url);

//Create list of recipes and list of categories
List<Recipe> recipesList = new List<Recipe>();
List<string> categoryList = new List<string>();

//Adding data to recipe list and category list from the back-end
(recipesList, categoryList) = await GetDataRequest(client);

//Adding console GUI
AnsiConsole.Write(
new FigletText("Recipe App")
.Centered()
.Color(Color.Fuchsia));
string[] choices = new string[] { "Recipe", "Category", "Exit" };

while (true)
{
    choices = new string[] { "Recipe", "Category", "Exit" };
    string firstMenuChoice = ConsoleSelection(choices, "How can I serve you?");
    var secondMenuChoice = "";
    switch (firstMenuChoice)
    {
        case "Recipe":
            choices = new string[] { "Add recipe", "Edit recipe", "Delete recipe", "List recipes", "Exit" };
            secondMenuChoice = ConsoleSelection(choices, "How can I serve you?");
            break;

        case "Category":
            choices = new string[] { "Add category", "Edit category", "Delete category", "List categories", "Exit" };
            secondMenuChoice = ConsoleSelection(choices, "How can I serve you?");

            break;

        case "Exit":
            return;

        default:
            break;
    }

    switch (secondMenuChoice)
    {
        case "Add recipe":
            await AddRecipe(categoryList, recipesList, client);
            break;

        case "List recipes":
            ListRecipes(recipesList);
            break;

        case "Edit recipe":
            EditRecipe(categoryList, recipesList, client);
            break;

        case "Delete recipe":
            DeleteRecipe(client);
            break;

        case "Add category":
            AddCategory(categoryList, client);
            break;

        case "Edit category":
            EditCategory(categoryList, client);
            break;

        case "Delete category":
            DeleteCategory(categoryList, client);
            break;

        case "List categories":
            ListCategories(categoryList);
            break;

        case "Exit":
            return;

        default:
            break;
    }

    //Updating recipe list and category list
    (recipesList, categoryList) = await GetDataRequest(client);
}

async Task AddRecipe(List<string> categoryList, List<Recipe> recipesList, HttpClient client)
{
    //Get the data of the recipe from the user
    string title = AnsiConsole.Ask<string>("What's the recipe name?");
    string instructions = AnsiConsole.Ask<string>("What's the recipe ingredients? (ex: milk-sugar-cocoa powder)");
    string ingredients = AnsiConsole.Ask<string>("What's the recipe instructions? (ex: Pour in the Milk-Add cocoa powder-Add sugar)");
    var categories = ConsoleMultiSelection(categoryList, "What's the recipe categories?");

    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(instructions) || string.IsNullOrEmpty(ingredients))
    {
        AnsiConsole.MarkupLine("[red3_1]Input data is not complete. please enter valid data![/]");
        return;
    }

    //Split ingredients and instructions to be a list
    List<string> ingredientsList = instructions.Split('-').ToList();
    List<string> instructionsList = ingredients.Split('-').ToList();

    //Create the guid and add the recipe to the list
    Guid guid = Guid.NewGuid();
    Recipe newRecipe = new Recipe(guid, title, instructionsList, ingredientsList, categories);

    //Sending recipe to backend to be added to recipe list file
    try
    {
        var recipeItemJson = new StringContent(JsonSerializer.Serialize(newRecipe), Encoding.UTF8, "application/json");
        using var httpResponseMessage = await client.PostAsync("/recipe", recipeItemJson);
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteLine($"[red1]{ex.Message}[/]");
        Environment.Exit(0);
    }
}

void ListRecipes(List<Recipe> recipesList)
{
    if (recipesList.Count == 0)
    {
        AnsiConsole.WriteLine("No recipes added yet!");
        return;
    }

    //create table to view all recipes
    var recipeTable = new Table();
    recipeTable.AddColumn("[blue]Recipe Title[/]");
    recipeTable.AddColumn("[blue]Ingredients[/]");
    recipeTable.AddColumn("[blue]Instructions[/]");
    recipeTable.AddColumn("[blue]Category[/]");
    recipeTable.Border(TableBorder.Rounded);
    recipeTable.Centered();

    foreach (Recipe recipe in recipesList)
    {
        try
        {
            recipeTable.AddRow($"[yellow]{recipe.Title}[/]", " -" + string.Join("\n -", recipe.Ingredients), " -" + string.Join("\n -", recipe.Instructions), " -" + string.Join("\n -", recipe.Categories));
            recipeTable.AddRow("-------------", "------------", "-------------------", "----------------");
        }
        catch (Exception ex)
        {
            AnsiConsole.Markup($"[red]{ex.Message}[/]");
            Environment.Exit(0);
        }
    }

    AnsiConsole.Write(recipeTable);
}

void EditRecipe(List<string> categoryList, List<Recipe> recipesList, HttpClient client)
{
    //Get the recipe that user want to edit
    Guid selectedRecipeGuid = RecipeSelection(recipesList);
    var selectedRecipe = recipesList.FirstOrDefault(x => x.Id == selectedRecipeGuid);
    if (selectedRecipe == null)
    {
        AnsiConsole.MarkupLine($"[red1]faild to edit[/]");
        return;
    }

    //ask user about the specific edit then edit the data needed
    string[] avaliableEdits = new string[] { "Edit title", "Edit instructions", "Edit ingredients", "Edit categories", "Exit" };
    string typeOfEdit = ConsoleSelection(avaliableEdits, "What do you want to edit");

    switch (typeOfEdit)
    {
        case "Edit title":
            string newRecipeTitle = AnsiConsole.Ask<string>("What's the new title?");
            if (newRecipeTitle != "")
            {
                var title = newRecipeTitle;
                Recipe newRecipe = new Recipe(
                    selectedRecipe.Id, newRecipeTitle, selectedRecipe.Instructions,
                    selectedRecipe.Ingredients, selectedRecipe.Categories);
                EditRecipeRequest(newRecipe, client);
            }
            else
            {
                AnsiConsole.WriteLine($"Edit {selectedRecipe.Title} title faild!");
            }
            break;

        case "Edit instructions":
            string newRecipeInstructions = AnsiConsole.Ask<string>("What's the new Instructions?(ex: Pour in the Milk-Add cocoa powder-Add sugar)");
            if (newRecipeInstructions != "")
            {
                var instructions = newRecipeInstructions.Split('-').ToList();
                Recipe newRecipe = new Recipe(
                    selectedRecipe.Id, selectedRecipe.Title, instructions,
                    selectedRecipe.Ingredients, selectedRecipe.Categories);
                EditRecipeRequest(newRecipe, client);
            }
            else
            {
                AnsiConsole.WriteLine($"Editing {selectedRecipe.Title} instructions faild!");
            }
            break;

        case "Edit ingredients":
            string newRecipeIngredients = AnsiConsole.Ask<string>("What's the new Ingrediants?(ex: milk-water-cocoa powder)");
            if (newRecipeIngredients != "")
            {
                var ingredients = newRecipeIngredients.Split('-').ToList();
                Recipe newRecipe = new Recipe(
                    selectedRecipe.Id, selectedRecipe.Title, selectedRecipe.Instructions,
                    ingredients, selectedRecipe.Categories);
                EditRecipeRequest(newRecipe, client);
            }
            else
            {
                AnsiConsole.WriteLine($"Editing {selectedRecipe.Title} ingredients faild!");
            }
            break;

        case "Edit categories":
            List<string> newRecipeCategories = ConsoleMultiSelection(categoryList, "What's recipe categories ? ");
            if (newRecipeCategories != null)
            {
                var categories = newRecipeCategories;
                Recipe newRecipe = new Recipe(
                    selectedRecipe.Id, selectedRecipe.Title, selectedRecipe.Instructions,
                    selectedRecipe.Ingredients, categories);
                EditRecipeRequest(newRecipe, client);
            }
            else
            {
                AnsiConsole.WriteLine($"Editing {selectedRecipe.Title} categories faild!");
            }
            break;

        case "Exit":
            Environment.Exit(0);
            break;

        default:
            break;
    }
}

async void DeleteRecipe(HttpClient client)
{
    //Get the recipe that user want to delete
    Guid selectedRecipeGuid = RecipeSelection(recipesList);
    var selectedRecipe = recipesList.FirstOrDefault(x => x.Id == selectedRecipeGuid);
    if (selectedRecipe == null)
    {
        AnsiConsole.MarkupLine($"[red1]faild to edit[/]");
        return;
    }
    else
    {
        using var httpResponseMessage =
            await client.DeleteAsync($"/recipe/{selectedRecipeGuid}");

        httpResponseMessage.EnsureSuccessStatusCode();

        AnsiConsole.MarkupLine($"Deleting [yellow]{selectedRecipe.Title}[/] recipe succeed.");
    }
}

async void AddCategory(List<string> categoryList, HttpClient client)
{
    string newCategory = AnsiConsole.Ask<string>("What's the name of category you want to add?");
    if (newCategory != "")
    {
        var categoryItemJson = new StringContent(
            JsonSerializer.Serialize(newCategory),
            Encoding.UTF8,
            "application/json");

        using var httpResponseMessage =
            await client.PostAsync("/category", categoryItemJson);

        httpResponseMessage.EnsureSuccessStatusCode();

        AnsiConsole.MarkupLine($"[green]{newCategory}[/] Added successfully");
    }
    else
    {
        AnsiConsole.MarkupLine($"[red]Adding new category failed[/]");
    }
}

async void EditCategory(List<string> categoryList, HttpClient client)
{
    string oldCategoryName = ConsoleSelection(categoryList.ToArray(), "Which category you want to edit?");
    string newCategoryName = AnsiConsole.Ask<string>("What's the new name of the category?");
    if (newCategoryName != null)
    {
        int indexOfEdited = categoryList.FindIndex(x => x == oldCategoryName);
        var oldCategoryJson = new StringContent(
            JsonSerializer.Serialize(oldCategoryName),
            Encoding.UTF8,
            "application/json");
        var newCategoryJson = new StringContent(
            JsonSerializer.Serialize(newCategoryName),
            Encoding.UTF8,
            "application/json");

        using var httpResponseMessage =
            await client.PutAsync($"/category/{oldCategoryName}", newCategoryJson);

        httpResponseMessage.EnsureSuccessStatusCode();

        AnsiConsole.MarkupLine($"Editing [green]{oldCategoryName}[/] to [green]{newCategoryName}[/] succeed.");
    }
    else
    {
        AnsiConsole.WriteLine($"Editing category failed.");
    }
}

async void DeleteCategory(List<string> categoryList, HttpClient client)
{
    string toBeDeletedCategory = ConsoleSelection(categoryList.ToArray(), "Which category you want to delete?");
    if (toBeDeletedCategory != null)
    {
        using var httpResponseMessage =
            await client.DeleteAsync($"/category/{toBeDeletedCategory}");

        httpResponseMessage.EnsureSuccessStatusCode();

        AnsiConsole.MarkupLine($"Deleting [yellow]{toBeDeletedCategory}[/] succeed.");
    }
    else
    {
        AnsiConsole.MarkupLine($"Deleting [red]{toBeDeletedCategory}[/] category failed.");
    }
}

void ListCategories(List<string> categoryList)
{
    var categories = new Tree("Categories")
        .Style(Style.Parse("red"))
        .Guide(TreeGuide.Line);

    foreach (var category in categoryList)
    {
        categories.AddNode($"[yellow]{category}[/]");
    }
    AnsiConsole.Write(categories);
}

//Requests related functions
async void EditRecipeRequest(Recipe newRecipe, HttpClient client)
{
    var recipeItemJson = new StringContent(
        JsonSerializer.Serialize(newRecipe),
        Encoding.UTF8,
        "application/json");

    using var httpResponseMessage =
        await client.PutAsync($"/recipe/{newRecipe.Id}", recipeItemJson);

    httpResponseMessage.EnsureSuccessStatusCode();
}

async Task<(List<Recipe>, List<string>)> GetDataRequest(HttpClient client)
{
    //Get recipe list from back-end as json format
    var httpResponseMessage =
        await client.GetAsync($"/recipes");
    httpResponseMessage.EnsureSuccessStatusCode();
    var recipeData = httpResponseMessage.Content.ReadAsStringAsync().Result;

    //Get category list from back-end as json format
    httpResponseMessage =
        await client.GetAsync($"/categories");
    httpResponseMessage.EnsureSuccessStatusCode();
    var categoryData = httpResponseMessage.Content.ReadAsStringAsync().Result;

    //Desrialize recipeData and categoryData which are string in json format
    List<Recipe>? savedRecipes = new();
    List<String>? savedCategories = new();
    try
    {
        savedRecipes = JsonSerializer.Deserialize<List<Recipe>>(recipeData);
        savedCategories = JsonSerializer.Deserialize<List<string>>(categoryData);
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red1]{ex.Message}[/]");
    }

    //Updating recipe list and category list data
    List<Recipe> recipesList = new List<Recipe>(savedRecipes!);
    List<string> categoryList = new List<string>(savedCategories!);

    return (recipesList, categoryList);
}

//Console related functions
string ConsoleSelection(string[] list, string question)
{
    var action = "";

    //Show user avaliable actions
    action = AnsiConsole.Prompt(
       new SelectionPrompt<string>()
           .Title(question)
           .PageSize(10)
           .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
           .AddChoices(list)
           );

    return action;
}

Guid RecipeSelection(List<Recipe> recipesList)
{
    var selectedRecipe = AnsiConsole.Prompt(
       new SelectionPrompt<string>()
           .Title("Which recipe you want to edit?")
           .PageSize(10)
           .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
           .AddChoices(recipesList.Select((recipe, Index) => $"{Index + 1}-{recipe.Title}")));

    string indexOfSelectedRecipe = selectedRecipe.Split('-')[0];
    return recipesList[Convert.ToInt32(indexOfSelectedRecipe) - 1].Id;
}

List<string> ConsoleMultiSelection(List<string> categoryList, string question)
{
    var selectedCategories = AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
           .Title(question)
           .NotRequired()
           .PageSize(10)
           .MoreChoicesText("[grey](Move up and down to reveal more categories)[/]")
           .InstructionsText(
               "[grey](Press [blue]<space>[/] to toggle a category, " +
               "[green]<enter>[/] to accept)[/]")
           .AddChoices(categoryList));

    return selectedCategories;
}