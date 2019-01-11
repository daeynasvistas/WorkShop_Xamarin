using AppIPG.Helpers;
using AppIPG.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AppIPG.Services
{
    class ApiServices
    {
        private static string AccessToken;
        private static string Response; 

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

        }

        public async Task<string> WorkshopAsync(String accessToken)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMilliseconds(20000);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(accessToken); // autorize

                var response = await client.GetAsync(Constantes.BaseApiAddress + "/api/workshops"); // EndPoint da API para receber workshops
                string content = await response.Content.ReadAsStringAsync();
                return content;
            }

        }
        public async Task<string> WorkshopPostAsync(String accessToken, Item item) 
        {
            var formContent = new FormUrlEncodedContent(new[] // o que vem do formulário
               {
                new KeyValuePair<string, string>("title", item.Title), // aqui pede email e não username (API)   
                new KeyValuePair<string, string>("speaker", item.Speaker),
                new KeyValuePair<string, string>("date", DateTime.Now.ToString())
                });

            using (var client = new HttpClient()) // client http para enviar request
            {
                client.Timeout = TimeSpan.FromMilliseconds(20000); // evitar o freeze no mobile, boa experiência de utilização
                var response = await client.PostAsync(Constantes.BaseApiAddress + "/api/workshops", formContent); // resposta do post

                if (response.IsSuccessStatusCode)
                {
                    // great success
                    string content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                return null; // problema, login inválido
            }
        }

    }
}
