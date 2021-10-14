using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KyrsovaWPF
{
    /// <summary>
    /// Логика взаимодействия для Info_Edit.xaml
    /// </summary>
    public partial class Info_Edit : Window
    {
        ViewModelOfInfoWindow view;
        public Info_Edit(Recipe recipe)
        {
            InitializeComponent();
            this.DataContext = new ViewModelOfInfoWindow(recipe);
            view = new ViewModelOfInfoWindow(recipe);
        }
        public object BackToParent()
        {
            return view.Ret();
        }
        #region PropChange
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
    public class ViewModelOfInfoWindow : INotifyPropertyChanged
    {
        private Command givetoparents; //Private return Data to MainWindow
        public ICommand GiveToParents => givetoparents; //Public return Data to MainWindow(private Command givetoparents)
        public Recipe Get_Recipe { get; set; }
        public Recipe Edit_Recipe { get; set; }
        public ViewModelOfInfoWindow(Recipe recipe)
        {
            Get_Recipe = new Recipe();
            Get_Recipe = recipe;
            Edit_Recipe = new Recipe();
            givetoparents = new DelegateCommand(SaveChange, () => Get_Recipe != null);
        }
        public void SaveChange()
        {
            Get_Recipe.Name = Edit_Recipe.Name;
            Get_Recipe.Type_Of_Cuisine = Edit_Recipe.Type_Of_Cuisine;
            Get_Recipe.Type_Of_Meal = Edit_Recipe.Type_Of_Meal;
            Get_Recipe.Ingredients = Edit_Recipe.Ingredients;
            Get_Recipe.How_To_Cook = Edit_Recipe.How_To_Cook;
        }
        public object Ret()
        {
            return Get_Recipe.Clone();
        }

        #region PropChange
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
