using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Exercise1;
using System.Text.Json;
using Spectre.Console;

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
    AnsiConsole.WriteLine($"[red1]{ex.Message}[/]");
    return;
}

//Create list of recipes and list of categories
List<Recipe> recipesList = new List<Recipe>(savedRecipes!);
List<string> categoryList = new List<string>(savedCategories!);

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
            choices = new string[] { "Add recipe", "Edit recipe", "List recipes", "Exit" };
            secondMenuChoice = ConsoleSelection(choices, "How can I serve you?");
            break;

        case "Category":
            choices = new string[] { "Add Category", "Edit Category", "Exit" };
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
            await AddRecipe(categoryList, recipesList);
            break;

        case "List recipes":
            ListRecipes(recipesList);
            break;

        case "Edit recipe":
            EditRecipe(categoryList, recipesList);
            break;

        case "Add Category":
            AddCategory(categoryList);
            break;

        case "Edit Category":
            EditCategory(categoryList, recipesList);
            break;

        case "Exit":
            return;

        default:
            break;
    }
}

static async Task<string> ReadJsonFile(string fileName) =>
await File.ReadAllTextAsync($"{fileName}.json");

static async Task WriteJsonFile(string fileName, string fileData) =>
await File.WriteAllTextAsync($"{fileName}.json", fileData);

static async Task AddRecipe(List<string> categoryList, List<Recipe> recipesList)
{

    //Get the data of the recipe from the user
    string title = AnsiConsole.Ask<string>("What's the recipe name?");
    string instructions = AnsiConsole.Ask<string>("What's the recipe ingredients? (ex: milk-sugar-cocoa powder)");
    string ingredients = AnsiConsole.Ask<string>("What's the recipe instructions? (ex: Pour in the Milk-Add cocoa powder-Add sugar)");
    var categories = ConsoleMultiSelection(categoryList, "What's the recipe categories?");

    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(instructions) || string.IsNullOrEmpty(ingredients))
    {
        AnsiConsole.WriteLine("[red3_1]Input data is not complete. please enter valid data![/]");
        return;
    }
    //Split ingredients and instructions to be a list
    List<string> ingredientsList = instructions.Split('-').ToList();
    List<string> instructionsList = ingredients.Split('-').ToList();

    //Create the guid and add the recipe to the list
    Guid guid = Guid.NewGuid();
    Recipe newRecipe = new Recipe(guid, title, instructionsList, ingredientsList, categories);
    recipesList.Add(newRecipe);

    //Serialize recipe list and write it to recipe file
    try
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(recipesList, options);
        await WriteJsonFile("recipe", jsonString);
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteLine($"[red1]{ex.Message}[/]");
        Environment.Exit(0);
    }
}

static void ListRecipes(List<Recipe> recipesList)
{
    if (recipesList.Count == 0)
    {
        AnsiConsole.WriteLine("No recipes added yet!");
        return;
    }
    //create table to view all recipes
    var recipeTable = new Table();
    recipeTable.AddColumn("Recipe Title");
    recipeTable.AddColumn("Ingredients");
    recipeTable.AddColumn("Instructions");
    recipeTable.AddColumn("Category");
    recipeTable.Border(TableBorder.Rounded);
    recipeTable.Centered();

    foreach (Recipe recipe in recipesList)
    {
        try
        {
            recipeTable.AddRow($"[yellow]{recipe.Title}[/]", " -" + string.Join("\n -", recipe.Ingredients), " -" + string.Join("\n -", recipe.Instructions), " -" + string.Join("\n -", recipe.Categories));
            recipeTable.AddRow("-------------", "------------", "-------------------", "----------------");
        }
        catch(Exception ex)
        {
            AnsiConsole.Markup($"[red]{ex.Message}[/]");
            Environment.Exit(0);
        }
        }

    AnsiConsole.Write(recipeTable);
}

static void EditRecipe(List<string> categoryList, List<Recipe> recipesList)
{
    //Get the recipe that user want to edit
    Guid recipeSelectedGuid = RecipeSelection(recipesList);
    var selectedRecipe = recipesList.FirstOrDefault(x => x.Id == recipeSelectedGuid);

    //ask user about the edits want then edit the data needed
    string[] avaliableEdits = new string[] { "Edit title", "Edit instructions", "Edit ingredients", "Edit categories", "Exit" };
    string typeOfEdit = ConsoleSelection(avaliableEdits, "What do you want to edit");
    if (selectedRecipe == null)
    {
        AnsiConsole.WriteLine("Edit faild!");
        return;
    }
    switch (typeOfEdit)
    {
        case "Edit title":
            string newRecipeTitle = AnsiConsole.Ask<string>("What's the new title?");
            if (newRecipeTitle != "")
            {
                selectedRecipe.Title = newRecipeTitle;
            }
            else
            {
                AnsiConsole.WriteLine("Edit faild!");
            }
            break;

        case "Edit instructions":
            string newRecipeInstructions = AnsiConsole.Ask<string>("What's the new Instructions?(ex: Pour in the Milk-Add cocoa powder-Add sugar)");
            if (newRecipeInstructions != "")
            {
                selectedRecipe.Instructions = newRecipeInstructions.Split('-').ToList();
            }
            else
            {
                AnsiConsole.WriteLine("Edit faild!");
            }
            break;

        case "Edit ingredients":
            string newRecipeIngrediants = AnsiConsole.Ask<string>("What's the new Ingrediants?(ex: milk-water-cocoa powder)");
            if (newRecipeIngrediants != "")
            {
                selectedRecipe.Ingredients = newRecipeIngrediants.Split('-').ToList();
            }
            else
            {
                AnsiConsole.WriteLine("Edit faild!");
            }
            break;

        case "Edit categories":
            List<string> newRecipeCategories = ConsoleMultiSelection(categoryList, "What's recipe categories ? ");
            if (newRecipeCategories != null)
            {
                selectedRecipe.Categories = newRecipeCategories;
            }
            else
            {
                AnsiConsole.WriteLine("Edit faild!");
            }
            break;

        case "Exit":
            Environment.Exit(0);
            break;

        default:
            break;
    }
}

static async void AddCategory(List<string> categoryList)
{
    string newCategory = AnsiConsole.Ask<string>("What's the name of category you want to add?");
    if (newCategory != "")
    {
        if (!categoryList.Contains(newCategory))
        {
            categoryList.Add(newCategory);
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(categoryList, options);
            await WriteJsonFile("category", jsonString);
            AnsiConsole.WriteLine($"{newCategory} Added successfully");
        }
        else
        {
            AnsiConsole.WriteLine($"{newCategory} is already in category list");
        }
    }
    else
    {
        AnsiConsole.WriteLine($"Adding new category failed");
    }
}

static async void EditCategory(List<string> categoryList, List<Recipe> recipesList)
{
    string oldCategoryName = ConsoleSelection(categoryList.ToArray(), "Which category you want to edit?");
    string newCategoryName = AnsiConsole.Ask<string>("What's the new name of the category?");
    if (newCategoryName != null)
    {
        int indexOfEdited = categoryList.FindIndex(x => x == oldCategoryName);
        categoryList[indexOfEdited] = newCategoryName;
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(categoryList, options);
        await WriteJsonFile("category", jsonString);
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
        AnsiConsole.WriteLine($"Category edited successfuly");
    }
    else
    {
        AnsiConsole.WriteLine($"Adding new category failed");
    }
}

static string ConsoleSelection(string[] list, string question)
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

static Guid RecipeSelection(List<Recipe> recipesList)
{
    var selectedRecipe = AnsiConsole.Prompt(
       new SelectionPrompt<string>()
           .Title("Which recipe you want to edit?")
           .PageSize(10)
           .MoreChoicesText("[grey](Move up and down to reveal more)[/]")
           .AddChoices(recipesList.Select((recipe, Index) => $"{Index + 1}-{recipe.Title}"))
);
    string indexOfSelectedRecipe = selectedRecipe.Split('-')[0];
    return recipesList[Convert.ToInt32(indexOfSelectedRecipe) - 1].Id;
}

static List<string> ConsoleMultiSelection(List<string> categoryList, string question)
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