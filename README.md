# WorkShop Xamarin (@IPG)
![Image of IPG](https://github.com/daeynasvistas/WorkShop_Xamarin/blob/Vers.01/AppIPG/AppIPG.Android/Resources/drawable/IPG_M.jpg?raw=true)

# Primeira Parte (Login)
Criar um novo ContentPage com o nome: LoginPage, na pasta Views. (Direito na pasta -> Add -> New item -> Escolher ContentPage)
nome:LoginView.xaml

Alterar Aspecto da View, colocando Dentro do StackLayout, duas Entry (Escolher da toolbox, arastar)
Arastar da toolbox um button para fora da stackLayout

![1](https://user-images.githubusercontent.com/32745683/51057394-63eddf00-15dd-11e9-9972-e83942e7cd02.PNG)
Arastar controlo Image e alterar as propriedades. Enviar a imagem para o projecto Android (em Resource -> Drawable)

#### LoginPage.xaml (1)
```xaml
    <ContentPage.Content>
        <StackLayout Orientation="Vertical" Padding="30" Spacing="40">
            <Image HorizontalOptions="Center" WidthRequest="120" Source="IPG_M.jpg" />
            <StackLayout Orientation="Vertical" Spacing="10">
                <Entry x:Name="Email" Text="{Binding Email}" Placeholder="Email" Keyboard="Email"/>
                <Entry x:Name="Password" Text="{Binding Password}" Placeholder="Password" IsPassword="True"/>
            </StackLayout>
            <Button Command="{Binding LoginCommand}" Text="Login"/>, 
        </StackLayout>
    </ContentPage.Content>
```
  - x:Name="Email" <- para poder ser chamado no code-behind
  - Text="{Binding Email}" <- Binding entre xaml e cs
  - Placeholder="Email" <- Identificador
  - Keyboard="Email" <- teclado dispositivo 
  
![xamarin--2019-jan-10-015](https://user-images.githubusercontent.com/2634610/51051516-e2418580-15cb-11e9-8365-9eac358b85d5.jpg)

### LoginPage.cs  (2)
Alterar LoginPage.cs para poder controlar o focus
```c#
    InitializeComponent ();
    Email.Completed += (object sender, EventArgs e) =>
       {
       Password.Focus();
       };
    Password.Completed += (object sender, EventArgs e) =>
       {
        // do the magic
       };
```

### LoginViewModel.cs  (3)
Criar um ViewModel para controlar a View acabada de criar. Criar em ViewModels -> LoginViewModel.cs


```c#
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
```
Editar Criando email e password e os get e set

#### LoginViewModel.cs  (4)
```c#
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
```
Criar um Command para ser acedido a partir da view -> LoginCommand


#### LoginPage.cs  (5)
```c# 
   // binding VIEW <-> VIEWMODEL
  var vm = new LoginViewModel();
  this.BindingContext = vm;
  // display errors in VIEW (Separação entre view e vewmodel)
  vm.DisplayInvalidLoginPrompt += () => DisplayAlert("Error", "Login Inválido, tentar novamente", "OK");
  vm.DisplayMainPage += () => App.Current.MainPage = new MainPage();
```
Na LoginPage.cs fazer o binding entre a view e o viewmodel
Adicionar validações e erros "DisplayInvalidLoginPrompt"


#### LoginViewModel.cs  (6)
```c#
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
 ```
Criar novo método onSubmit() para workflow do login .. Se email ou password inválido, msg de erro
![2](https://user-images.githubusercontent.com/32745683/51057395-63eddf00-15dd-11e9-81ca-dc4a9ca3425e.PNG)


Para Aceder à API, criar um serviço. Este servico irá ocupar-se exclusivamente disso mesmo.
FormContent passa a ser um keyValuePair do email e password intrduzido pelo utilizador.

Instanciamos um novo HttpClient() para fazer o request (client)
Obtemos resposta do request na variável response
Se resposta OK. O login é válido
Explodimos a string content e retiramos unicamente o token.
Este é um método simples mas pouco eficaz de tratar JSON. Mais à frente instalaremos uma libraria para isso.

#### ApiServices.cs  (7)
```c#
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

```

## Criar Helpers Folder  (8)
Criar uma nova pasta Helpers na raiz do projecto (Models, Services, ViewModels,View e Helpers)
### Class Constantes  (9)
Uma class contantes.cs irá permitir colocar as configurações da App. (BaseApiAddress neste momento)
#### Constantes.cs  (10)
```c#
    public static class Constantes
    {
        public static string BaseApiAddress => "https://workshop-ipg.azurewebsites.net"; // api base para ser utilizado em vários locais
    }
    
```
## Class AccountDetailsStore (11)
é necessário guardar o token para as subsequentes chamadas à API (Cada Requeste deve ser autorizada, enviando o token no header da request)
### AccountDetailsStore.cs (12)
Numa App de produção, devemos utilizar outro método, guarda nas settings por exemplo (https://www.nuget.org/packages/Xam.Plugins.Settings/)
```c#
    public sealed class AccountDetailsStore
    {
        private AccountDetailsStore() { }

        public static AccountDetailsStore Instance { get; } = new AccountDetailsStore();

        public string Token { get; set; }
    }
 ```   
 
O ViewModel passa então a guardar o token que vem do submit na store AccountDetailStore (simples e eficaz)
#### LoginViewModel.cs (13)
```c#
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
 
``` 
Alterar método OnSubmit() para mostrar MainPage quando login for válido e error quando login errado.

A view é responsável pelas informações ao utilizador:
vm.DisplayInvalidLoginPrompt <-- DisplayInvalidLoginPrompt();
vm.DisplayMainPage  <--  DisplayMainPage();

## O Meu novo servico de Login (ApiSercices -> )
```C#
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
                var response = await client.PostAsync(Constantes.BaseApiAddress + "/api/Users/login", formContent); // resposta do post

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
 ```   
 Login Inválido (token=null)
![3](https://user-images.githubusercontent.com/32745683/51057396-63eddf00-15dd-11e9-8f36-982c8742b20c.PNG)
Login OK (Token!=null)
![4](https://user-images.githubusercontent.com/32745683/51057695-3ce3dd00-15de-11e9-8c00-3b1f067eaa9d.PNG)


 # Parte 2 -  Receber todos Items, Authorization Token (JWT) (14)
 
 Para receber todos os Items da API no endpoint: /api/workshops
 Este EndPoint tem ACL, Anonymous deny
 Necessário enviar token para identificar utilizador válido.
 
 Remover os dummy data. e ficar só com a collection items 
  ### MockDataStore.cs (15)
 ```c#
        List<Item> items;
        private readonly ApiServices _apiServices = new ApiServices();
        public MockDataStore()
        {
            items = new List<Item>(); <-- remover o dummy data criar uma collection vazia
        }
 ```
 
 
 Necessário criar um novo método para o GET do endpoint /api/workshops
 É um serviço -> ApiServices responsável.
 
 Novo HttpClient -> client
 Client esse passa incluir o Token no request
 Return response 
 ### ApiServices.cs (16)
 ```c#
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
```

A gestão do dados está na dataStore MockupDataStore.cs
Neste método utilizaremos uma libraria para fazer o serialize do JSON

Devemos para isso adicionar a mesma ao projecto, utilizando o nuGet. 
Botão direito do rato na solução -> Manage MuGet Packages for solution

![alt text](https://user-images.githubusercontent.com/2634610/51049730-a821b500-15c6-11e9-8df1-27cae6d4bebe.PNG)

### MockupDataStore.cs (17)
```c#
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
        
 ```   
 Alterar o model Item para adaptar à nossa API. Neste caso necessitamos Title e Speaker
  ## Alterar Model (18)
 O DeserializeObject poderá ser ugualado ao objecto do nosso modelo, directamente. 
     var Items = JsonConvert.DeserializeObject<List<Item>>(Workshops);
     items.Add(workshop);

Refactor das proriedades do model.
  ### Item.cs (19)
  ```c#
      public class Item
    {
        public string Id { get; set; }
        public string Title { get; set; } // Rename !!! (refactoring)
        public string Speaker { get; set; }  // Rename !!! (refactoring)
        // Criar um novo MODEL ou alterar este (vou alterar este)
    }
 ```
 
 O rename/refactor não altera as Views Xaml .. alterar onde é necessário
 <Label Text="{Binding Title}" 
 #### Alterar View ItemsPage.xaml (Title e Speaker, item model) (20)
```xaml
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
```
.. alterar onde é necessário
#### Alterar View ItemDetailPage.xaml (Title e speaker, item model) (21)
```xaml
    <StackLayout Spacing="20" Padding="15">
        <Label Text="Title:" FontSize="Medium" />
        <Label Text="{Binding Item.Title}" FontSize="Small"/>
        <Label Text="Speaker:" FontSize="Medium" />
        <Label Text="{Binding Item.Speaker}" FontSize="Small"/>
    </StackLayout>

```
 # Parte 3 -  Enviar item, Authorization Token (JWT) (22)
 Para enviar um novo item para o endpoint da API (/api/workshops) criar um novo método no serviço.
 
 Novo formContent para receber do utilizador.
 O nosso EndPoint pede o seguinte:
  ![alt text](https://user-images.githubusercontent.com/2634610/51051209-09e41e00-15cb-11e9-9cba-cc4cad030be9.PNG)
 
 
 Novo client com token no header
 ### ApiServices (adiconar método) (23)
 ```c#
       public async Task<string> WorkshopPostAsync(String accessToken, Item item) 
        {
            var formContent = new FormUrlEncodedContent(new[] // o que vem do formulário
               {
                new KeyValuePair<string, string>("title", item.Title),  
                new KeyValuePair<string, string>("speaker", item.Speaker),
                new KeyValuePair<string, string>("date", DateTime.Now.ToString())
                });

            using (var client = new HttpClient()) // client http para enviar request
            {
                client.Timeout = TimeSpan.FromMilliseconds(20000); // evitar o freeze no mobile, boa experiência de utilização
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(accessToken); // autorize
                var response = await client.PostAsync(Constantes.BaseApiAddress + "/api/workshops", formContent); // resposta do post

                if (response.IsSuccessStatusCode)
                {
                    // great success
                    string content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                return null; // problema, token inválido
            }
        }
```
Na fataStore Alterar a função AddItemAsync() para adicionar um novo item.
### MockupDataStore.cs (alterar método AddItemAsync) (24)
```c#
        public async Task<bool> AddItemAsync(Item item)
        {
            var accessToken = AccountDetailsStore.Instance.Token;
            var Workshops = await _apiServices.WorkshopPostAsync(accessToken, item); //<- fazer o get no endpoint

            items.Add(item);

            return await Task.FromResult(true);
        }
```

![screenshot_20190110-214214](https://user-images.githubusercontent.com/2634610/51051864-f043d600-15cc-11e9-9bb0-fecba4cf0807.png)
![screenshot_20190110-214235](https://user-images.githubusercontent.com/2634610/51051866-f043d600-15cc-11e9-875e-4345999f78c0.png)
![screenshot_20190110-214309](https://user-images.githubusercontent.com/2634610/51051867-f043d600-15cc-11e9-85ae-d2b1f7d361d8.png)

        
