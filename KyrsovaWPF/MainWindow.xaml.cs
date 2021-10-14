using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace KyrsovaWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModel ViewModel = new ViewModel();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel;
            ViewModel.FromDB();
        }
        private void MainListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) //Dada, Doubleclick for NewWindow()
        {
            ViewModel.NewWindow();
        }
        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class Recipe : INotifyPropertyChanged
    {
        private string name { get; set; }
        private string type_of_meal { get; set; }
        private string type_of_cuisine { get; set; }
        private string ingredients { get; set; }
        private string how_to_cook { get; set; }

        public string Type_Of_Cuisine
        {
            get => type_of_cuisine;
            set
            {
                type_of_cuisine = value;
                OnPropertyChanged();
            }
        }
        public string Type_Of_Meal
        {
            get => type_of_meal;
            set
            {
                type_of_meal = value;
                OnPropertyChanged();
            }
        }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }
        public string Ingredients
        {
            get => ingredients;
            set
            {
                ingredients = value;
                OnPropertyChanged();
            }
        }
        public string How_To_Cook
        {
            get => how_to_cook;
            set
            {
                how_to_cook = value;
                OnPropertyChanged();
            }
        }
        public object Clone()
        {
            // shallow copy (поверхневе копіювання) - копіюються лише 
            // значення value типів та посилання reference типів
            Recipe clone = (Recipe)this.MemberwiseClone();

            // deep copy (глибоке копіювання) - кожний reference тип
            // копіюється власним методом клонування
            clone.Name = (string)this.Name.Clone();
            clone.Type_Of_Cuisine = (string)this.Type_Of_Cuisine.Clone();
            clone.Type_Of_Meal = (string)this.Type_Of_Meal.Clone();
            clone.Ingredients = (string)this.Ingredients.Clone();
            clone.How_To_Cook = (string)this.How_To_Cook.Clone();
            return clone;
        }
        //public string FullInfo => $"Name - {Name}, Cuisine - {Type_Of_Cuisine}, Meal - {Type_Of_Meal} \nIngredients => {Ingredients} \nHow to cook => {How_To_Cook}"; //For Binding To Label
        public override string ToString()
        {
            return $"Name - {Name}, Cuisine - {Type_Of_Cuisine}, Meal - {Type_Of_Meal} \nIngredients => {Ingredients} \nHow to cook => {How_To_Cook}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // Створення методу OnPropertyChanged для виклику події
        // В якості параметра буде використано ім'я властивості, що його викликала
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Recipe> recipes = new ObservableCollection<Recipe>(); //Private "List"
        private Recipe recipe; //Private Exemp of class Recipe
        private Command addrecipecmd; //Private Command for AddButton
        private Command refreshlistbox; //Private Command for refresh
        private Command removerecipecmd; //Private Command for RemoveButton
        private Recipe selectedrecipe; //Private for Delete and Edit or smth..
        private Command writetofile; //Private From File
        private Command searchtextbox; //Command for Search in TextBox 
        public string Name_Of_Recipe { get; set; } //Get Recipe name for search
        public IEnumerable<Recipe> Irecipes => recipes; //Public "List" ofr binding
        public Recipe PropRecipe
        {
            get => recipe;
            set
            {
                recipe = value;
                OnPropertyChanged();
            }
        } //Public Exemp for binding
        public Recipe SelectedRecipe
        {
            get => selectedrecipe;
            set 
            {
                selectedrecipe = value;
                OnPropertyChanged();
            }
        } //Public For Delete and Edit or smth..
        public ICommand AddRecipeCmd => addrecipecmd; //Public Command for AddButton =>(private Command addrecipecmd)
        public ICommand RemoveRecipeCmd => removerecipecmd; //Public Command for RemoveButton =>(private Command removerecipecmd
        public ICommand WriteToFile => writetofile; //Public Command for File =>( private Command writefromfile)
        public ICommand RefreshListBox => refreshlistbox; //Public Command for File =>( private Command writefromfile)
        public ICommand SearchTextBox => searchtextbox; //Public Command for Search =>(private Command searchtextbox)
        public ViewModel()
        {
            recipes = new ObservableCollection<Recipe>();
            recipe = new Recipe(); //Create PropRecipe
            selectedrecipe = new Recipe();
            addrecipecmd = new DelegateCommand(Add, () => PropRecipe != null);
            removerecipecmd = new DelegateCommand(Remove, () => SelectedRecipe != null);
            writetofile = new DelegateCommand(Save, () => recipes != null);
            refreshlistbox = new DelegateCommand(Refresh);
            searchtextbox = new DelegateCommand(Search, () => Name_Of_Recipe != null);
            PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == nameof(SelectedRecipe))
                {
                    addrecipecmd.RaiseCanExecuteChanged();
                    removerecipecmd.RaiseCanExecuteChanged();
                    writetofile.RaiseCanExecuteChanged();
                    /*openifdowindowcmd.RaiseCanExecuteChanged();*/
                }
            };
        }
        public void Refresh()
        {
            recipes.Clear(); /*Clear List*/
            FromDB(); /*Write Data to List*/
        }
        public void FromDB()
        {
            using (RecipeDBEntities recipeDB = new RecipeDBEntities()) /*Create new Exemp of Entity))*/
            {
                var tmp = (from a in recipeDB.RecipeTable select a).ToList(); /*From DB to New Recipe*/
                foreach (var item in tmp)
                {
                    Recipe re = new Recipe();
                    re.Name = item.Name;
                    re.Type_Of_Cuisine = item.TypeOfCuisine;
                    re.Type_Of_Meal = item.TypeOfMeal;
                    re.Ingredients = item.Ingredients;
                    re.How_To_Cook = item.HowToCook;
                    recipes.Add(re); /*Add to List*/
                }
            }
        }
        public void Add()
        {
            if (PropRecipe != null)
            {
                using (RecipeDBEntities db = new RecipeDBEntities())
                {
                    RecipeTable recipeTable = new RecipeTable(); //Create new Exmp of Table
                    recipeTable.Name = PropRecipe.Name; //RecipeTable(Add New Recipe)
                    recipeTable.TypeOfCuisine = PropRecipe.Type_Of_Cuisine;
                    recipeTable.TypeOfMeal = PropRecipe.Type_Of_Meal;
                    recipeTable.Ingredients = PropRecipe.Ingredients;
                    recipeTable.HowToCook = PropRecipe.How_To_Cook;
                    db.RecipeTable.Add(recipeTable);
                    db.SaveChangesAsync();
                    Refresh();
                }
            }
        }
        public void AddWithArg(Recipe recipe)
        {
            if (recipe != null)
            {
                using (RecipeDBEntities db = new RecipeDBEntities())
                {
                    RecipeTable recipeTable = new RecipeTable(); //Create new Exmp of Table
                    recipeTable.Name = recipe.Name; //RecipeTable(Add New Recipe)
                    recipeTable.TypeOfCuisine = recipe.Type_Of_Cuisine;
                    recipeTable.TypeOfMeal = recipe.Type_Of_Meal;
                    recipeTable.Ingredients = recipe.Ingredients;
                    recipeTable.HowToCook = recipe.How_To_Cook;
                    db.RecipeTable.Add(recipeTable);
                    db.SaveChangesAsync();
                    Refresh();
                }
            }
        }
        public void Remove()
        {
            if (SelectedRecipe != null)
            {
                using (RecipeDBEntities db = new RecipeDBEntities()) /*Exmp of Entity*/
                {
                    var Lrecipe = (from a in db.RecipeTable /*Write from DB to var*/
                                   where a.Name == SelectedRecipe.Name &&
                                   a.TypeOfCuisine == SelectedRecipe.Type_Of_Cuisine &&
                                   a.TypeOfMeal == SelectedRecipe.Type_Of_Meal &&
                                   a.Ingredients == SelectedRecipe.Ingredients &&
                                   a.HowToCook == SelectedRecipe.How_To_Cook
                                   select a).ToList();
                    var b = new RecipeTable(); /*Create new Exmp of Db Table*/
                    foreach (var item in Lrecipe)
                    {
                        b = item; /*From Selected DB to RecipeTable*/
                    }
                    db.RecipeTable.Remove(b); //Remove
                    db.SaveChangesAsync(); //Save
                    Refresh();
                }
            }
        }
        public void RemoveWithArg(Recipe recipe)
        {
            if (recipe != null)
            {
                using (RecipeDBEntities db = new RecipeDBEntities()) /*Exmp of Entity*/
                {
                    var Lrecipe = (from a in db.RecipeTable /*Write from DB to var*/
                                   where a.Name == recipe.Name &&
                                   a.TypeOfCuisine == recipe.Type_Of_Cuisine &&
                                   a.TypeOfMeal == recipe.Type_Of_Meal &&
                                   a.Ingredients == recipe.Ingredients &&
                                   a.HowToCook == recipe.How_To_Cook
                                   select a).ToList();
                    var b = new RecipeTable(); /*Create new Exmp of Db Table*/
                    foreach (var item in Lrecipe)
                    {
                        b = item; /*From Selected DB to RecipeTable*/
                    }
                    db.RecipeTable.Remove(b); //Remove
                    db.SaveChangesAsync(); //Save
                    Refresh();
                }
            }
        }
        /*
         * UNSAFE COD -------- DON`T TOUCH!
         * UNSAFE COD -------- DON`T TOUCH!
         * UNSAFE COD -------- DON`T TOUCH!
         */
        public void NewWindow()
        {
            Info_Edit window = new Info_Edit((Recipe)SelectedRecipe.Clone()); /*Clone for new exemple to NewWindow*/ /*Якщо помінять - зломається*/
            Recipe Edited_Recipe = new Recipe(); /*New Exemp from edit window to DB*/
            Recipe recipeAAA = new Recipe(); /*For if*/
            recipeAAA = (Recipe)SelectedRecipe.Clone(); /*For if*/
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3)}; /*send the data while the window is open*/
            window.Show();
            timer.Start();

            timer.Tick += (sender, args) =>  /*Every 3 sec - Tick*/
            {
                Edited_Recipe = (Recipe)window.BackToParent(); /*Send Edited Data to this Exemp*/
                if (Edited_Recipe.Name == SelectedRecipe.Name
                && Edited_Recipe.Type_Of_Cuisine == SelectedRecipe.Type_Of_Cuisine /*Just need*/
                && Edited_Recipe.Type_Of_Meal == SelectedRecipe.Type_Of_Meal
                && Edited_Recipe.Ingredients == SelectedRecipe.Ingredients
                && Edited_Recipe.How_To_Cook == SelectedRecipe.How_To_Cook)
                {
                }
                else
                {
                    if (SelectedRecipe != null)
                    {
                        RemoveWithArg(SelectedRecipe); /*Remove SelectedRecipe from DB*/
                        AddWithArg(Edited_Recipe); /*Add Edited Data to DB*/
                        timer.Stop();
                    }
                    timer.Stop();
                }
                if (window.IsActive == false)
                {
                    timer.Stop();
                }
            };
        }
        public void NewWindowWithArg(Recipe recipe)
        {
            Info_Edit window = new Info_Edit((Recipe)recipe.Clone()); /*Clone for new exemple to NewWindow*/ /*Якщо помінять - зломається*/
            Recipe Edited_Recipe = new Recipe(); /*New Exemp from edit window to DB*/
            Recipe recipeAAA = new Recipe(); /*For if*/
            recipeAAA = (Recipe)recipe.Clone(); /*For if*/
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) }; /*send the data while the window is open*/
            window.Show();
            timer.Start();

            timer.Tick += (sender, args) =>  /*Every 3 sec - Tick*/
            {
                Edited_Recipe = (Recipe)window.BackToParent(); /*Send Edited Data to this Exemp*/
                if (Edited_Recipe.Name == recipe.Name
                && Edited_Recipe.Type_Of_Cuisine == recipe.Type_Of_Cuisine /*Just need*/
                && Edited_Recipe.Type_Of_Meal == recipe.Type_Of_Meal
                && Edited_Recipe.Ingredients == recipe.Ingredients
                && Edited_Recipe.How_To_Cook == recipe.How_To_Cook)
                {
                }
                else
                {
                    if (recipe != null)
                    {
                        RemoveWithArg(recipe); /*Remove SelectedRecipe from DB*/
                        AddWithArg(Edited_Recipe); /*Add Edited Data to DB*/
                        timer.Stop();
                    }
                    timer.Stop();
                }
                if (window.IsActive == false)
                {
                    timer.Stop();
                }
            };
        }
        /*
        * UNSAFE COD -------- DON`T TOUCH!
        * UNSAFE COD -------- DON`T TOUCH!
        * UNSAFE COD -------- DON`T TOUCH!
        */
        public void Save()
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            if (fileDialog.ShowDialog() == false)
            {
                return;
            }
            string tmp = "";
            string filename = fileDialog.FileName;
            //foreach (var item in recipes)
            //{
            //    tmp = tmp + item.ToString() + "\n";
            //}
            tmp += "Name: " + SelectedRecipe.Name + "\n";
            tmp += "Coisine: " + SelectedRecipe.Type_Of_Cuisine + "\n";
            tmp += "Meal: " + SelectedRecipe.Type_Of_Meal + "\n";
            tmp += "Ingredients: " + SelectedRecipe.Ingredients + "\n";
            tmp += "How to Cook: " + SelectedRecipe.How_To_Cook + "\n";
            tmp += "---------------";
            tmp += "---------------";
            File.WriteAllText(filename, tmp);
        }
        public void Search()
        {
            bool IsTrue = false;
            Recipe searchedrecipe = new Recipe();
            if (Name_Of_Recipe != null)
            {
                foreach (var item in recipes)
                {
                    if (Name_Of_Recipe == item.Name)
                    {
                        IsTrue = true;
                        searchedrecipe = item;
                    }
                }
                if (IsTrue == true)
                {
                    NewWindowWithArg(searchedrecipe);
                }
            }
        }
        #region PropChange
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
    internal abstract class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        protected virtual bool CanExecute()
        {
            return true;
        }

        protected abstract void Execute();

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            Execute();
        }
    } //Class for Commands
    internal sealed class DelegateCommand : Command
    {
        private static readonly Func<bool> defaultCanExecuteMethod = () => true;

        private readonly Func<bool> canExecuteMethod;
        private readonly Action executeMethod;

        public DelegateCommand(Action executeMethod) :
            this(executeMethod, defaultCanExecuteMethod)
        {
        }

        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            this.canExecuteMethod = canExecuteMethod;
            this.executeMethod = executeMethod;
        }

        protected override bool CanExecute()
        {
            return canExecuteMethod();
        }

        protected override void Execute()
        {
            executeMethod();
        }
    } //Class for Commands
}
