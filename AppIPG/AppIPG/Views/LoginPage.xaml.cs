using AppIPG.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AppIPG.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
		public LoginPage ()
		{
            // binding VIEW <-> VIEWMODEL
            var vm = new LoginViewModel();
            this.BindingContext = vm;
            // display errors in VIEW (Separação entre view e vewmodel)
            vm.DisplayInvalidLoginPrompt += () => DisplayAlert("Error", "Login Inválido, tentar novamente", "OK");
            vm.DisplayMainPage += () => App.Current.MainPage = new MainPage();


            InitializeComponent ();

            Email.Completed += (object sender, EventArgs e) =>
            {
                Password.Focus();
            };

            Password.Completed += (object sender, EventArgs e) =>
            {
                vm.LoginCommand.Execute(null); // do the magic
            };

        }
	}
}