using AppIPG.Helpers;
using AppIPG.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace AppIPG.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // ---   Private and getter setter EMAIL
        private string email;
        public string Email
        {
            get { return email; }
            set
            {
                email = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Email"));
            }
        }
        // ---   Private and getter setter PASSWORD
        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Password"));
            }
        }

        // --------- add Command
        private readonly ApiServices _apiServices = new ApiServices(); // instanciar apiservice
        public ICommand LoginCommand
        {
            get
            {
                return new Command(async () =>
                {
                    // faz montes de cenas
                    var accesstoken = await _apiServices.LoginAsync(email, password);
                    // utilizar apiservice
                    // GUARDAR O TOKEN para ser utilizado nas requestes seguintes
                    // método simples (deve guardar em settings, em produção usar https://www.nuget.org/packages/Xam.Plugins.Settings/)
                    AccountDetailsStore.Instance.Token = accesstoken;

                    OnSubmit();
                });
            }
        }

        // --- criar ações publicas para enviar informações ao utilizador ou mudar de page...
        // -- separar VIEWMODEL - VIEW
        public Action DisplayInvalidLoginPrompt; //
        public Action DisplayMainPage;

        public void OnSubmit()
        {
            // OK.. aqui flow para verificar se exite token (então houve login ou não, não houve login válido=null)
            if (AccountDetailsStore.Instance.Token == null) // deve utilizar settings em produção
            {
                DisplayInvalidLoginPrompt();
            }
            else
            {
                DisplayMainPage();
            }
        }

    }
}
