# WorkShop Xamarin (@IPG)

### Primeira Parte (Login)
#### LoginPage.xaml

    <ContentPage.Content>
        <StackLayout Orientation="Vertical" Padding="30" Spacing="40">
            <Image HorizontalOptions="Center" WidthRequest="120" Source="IPG_M.jpg" />
            <StackLayout Orientation="Vertical" Spacing="10">
                <Entry x:Name="Email" Text="{Binding Email}" Placeholder="Email" Keyboard="Email"/>
                <Entry x:Name="Password" Text="{Binding Password}" Placeholder="Password" IsPassword="True"/>
            </StackLayout>
            <Button Command="{Binding LoginCommand}" Text="Login"/>
        </StackLayout>
    </ContentPage.Content>

### LoginPage.cs

    InitializeComponent ();
    Email.Completed += (object sender, EventArgs e) =>
       {
       Password.Focus();
       };
    Password.Completed += (object sender, EventArgs e) =>
       {
        // do the magic
       };


### LoginViewModel.cs

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

#### LoginViewModel.cs

        public ICommand LoginCommand
        {
            get
            {
                return new Command(async () =>
                {
                // faz montes de cenas
                });
            }
        }

#### LoginPage.cs
            // binding VIEW <-> VIEWMODEL
            var vm = new LoginViewModel();
            this.BindingContext = vm;
            // display errors in VIEW (Separação entre view e vewmodel)
            vm.DisplayInvalidLoginPrompt += () => DisplayAlert("Error", "Login Inválido, tentar novamente", "OK");
            vm.DisplayMainPage += () => App.Current.MainPage = new MainPage();

       
#### LoginViewModel.cs
        public ICommand LoginCommand
        {
            get
            {
                return new Command(async () =>
                {
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
            if (email != "Daniel@ept.pt" || password != "123456789") // deve utilizar settings em produção
            {
                DisplayInvalidLoginPrompt();
            }
            else
            {
                DisplayMainPage();
            }
        }

#### ApiServices.cs
        private static string AccessToken;
        public async Task<string> LoginAsync(string email, string password) 
        {
            var formContent = new FormUrlEncodedContent(new[] // o que vem do formulário
               {
                new KeyValuePair<string, string>("email", email), // aqui pede email e não username (API)   
                new KeyValuePair<string, string>("password", password)
                });

            using (var client = new HttpClient()) // client http para enviar request
            {
                client.Timeout = TimeSpan.FromMilliseconds(20000); // evitar o freeze no mobile, boa experiência de utilização
                var response = await client.PostAsync("https://omeuURLapi.com/api/Users/login", formContent); // resposta do post

                if (response.IsSuccessStatusCode)
                {
                    // great success
                    string content = await response.Content.ReadAsStringAsync();
                    var tokens = content.Split('"'); // método SIMPLES para não utilizar livraria JSON
                    AccessToken = tokens[3]; // local onde está o token depois do split
                    return AccessToken;
                }
            return null; // problema, login inválido
            }

        }


# Criar Helpers Folder
## Class Constantes
#### Constantes.cs
    public static class Constantes
    {
        public static string BaseApiAddress => "https://workshop-ipg.azurewebsites.net"; // api base para ser utilizado em vários locais
    }
    

## Class AccountDetailsStore
### AccountDetailsStore.cs
    public sealed class AccountDetailsStore
    {
        private AccountDetailsStore() { }

        public static AccountDetailsStore Instance { get; } = new AccountDetailsStore();

        public string Token { get; set; }
    }
    
#### LoginViewModel.cs
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

       //Alterar Método OnSubmit()  <<------------------------ ALTERAR SUBMIT 
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
 
 
 # Receber todos Items, Authorization Token (JWT)
 
 ### MockDataStore.cs
 
        List<Item> items;
        private readonly ApiServices _apiServices = new ApiServices();
        public MockDataStore()
        {
            items = new List<Item>(); <-- remover o dummy data criar uma collection vazia
        }
        
 ### ApiServices.cs
         public async Task<string> WorkshopAsync(String accessToken)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(20000);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(accessToken); // autorize
                var response = await client.GetAsync(Constantes.BaseApiAddress + "/api/workshops"); // EndPoint da API
                string content = await response.Content.ReadAsStringAsync();
                return content;
            }

        }

### MockupDataStore.cs
        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            var accessToken = AccountDetailsStore.Instance.Token;   
            var Workshops = await _apiServices.WorkshopAsync(accessToken); //<- fazer o get no endpoint
            // utilizar uma libraria para parse do JSON --- Download no nuGet
            var Items = JsonConvert.DeserializeObject<List<Item>>(Workshops); <-- Adicionar com NuGet
            foreach (var workshop in Items)
            {
                items.Add(workshop);
            }

            return await Task.FromResult(items);
        }
        
        
  ## Alterar Model
  ### Item.cs
      public class Item
    {
        public string Id { get; set; }
        public string Title { get; set; } // Rename !!! (refactoring)
        public string Speaker { get; set; }  // Rename !!! (refactoring)
        // Criar um novo MODEL ou alterar este (vou alterar este)
    }
    
 ## Alterar View ItemsPage.xaml (Title e Speaker, item model)
             <ListView.ItemTemplate>
                <DataTemplate>
                    <ViewCell>
                        <StackLayout Padding="10">
                            <Label Text="{Binding Title}" 
                                LineBreakMode="NoWrap" 
                                Style="{DynamicResource ListItemTextStyle}" 
                                FontSize="16" />
                            <Label Text="{Binding Speaker}" 
                                LineBreakMode="NoWrap"
                                Style="{DynamicResource ListItemDetailTextStyle}"
                                FontSize="13" />
                        </StackLayout>
                    </ViewCell>
                </DataTemplate>
            </ListView.ItemTemplate>

## Alterar View ItemDetailPage.xaml (Title e speaker, item model)
    <StackLayout Spacing="20" Padding="15">
        <Label Text="Title:" FontSize="Medium" />
        <Label Text="{Binding Item.Title}" FontSize="Small"/>
        <Label Text="Speaker:" FontSize="Medium" />
        <Label Text="{Binding Item.Speaker}" FontSize="Small"/>
    </StackLayout>


 # Enviar item, Authorization Token (JWT)
 
