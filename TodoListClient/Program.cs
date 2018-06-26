using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TodoListClient
{
    class Program
    {
        #region Init
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need its URL as well.
        //
        private static string todoListResourceId = ConfigurationManager.AppSettings["todo:TodoListResourceId"];
        private static string todoListBaseAddress = ConfigurationManager.AppSettings["todo:TodoListBaseAddress"];
        private static AuthenticationContext authContext = null;
        #endregion

        static void Main(string[] args)
        {
            string commandString = string.Empty;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("**************************************************");
            Console.WriteLine("*              ToDo List Text Client             *");
            Console.WriteLine("*                                                *");
            Console.WriteLine("*       Type commands to manage your ToDos       *");
            Console.WriteLine("*                                                *");
            Console.WriteLine("**************************************************");
            Console.WriteLine("");

            // Initialize the AuthenticationContext for the AAD tenant of choice.
            // Add a persistent file-based token cache.
            authContext = new AuthenticationContext(authority, new FileCache());

            // main command cycle
            while (!commandString.Equals("Exit", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Enter command (list | add | clear | exit | help) >");
                commandString = Console.ReadLine();

                switch (commandString.ToUpper())
                {
                    case "LIST":
                        ListTodo().Wait();
                        break;
                    case "ADD":
                        AddTodo().Wait();
                        break;
                    case "CLEAR":
                        ClearCache();
                        break;
                    case "HELP":
                        Help();
                        break;
                    case "EXIT":
                        Console.WriteLine("Bye!"); ;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }
        }

        #region Textual UX

        // Gather user credentials form the command line
        static UserCredential TextualPrompt()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("There is no token in the cache or you are not connected to your domain.");
            Console.WriteLine("Please enter username and password to sign in.");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("User>");
            string user = Console.ReadLine();
            Console.WriteLine("Password>");
            string password = ReadPasswordFromConsole();
            Console.WriteLine("");
            return new UserPasswordCredential(user, password);
        }

        // Display exceptions text on the console
        static void ShowError(Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("An unexpected error occurred.");
            string message = ex.Message;
            if (ex.InnerException != null)
            {
                message += Environment.NewLine + "Inner Exception : " + ex.InnerException.Message;
            }
            Console.WriteLine("Message: {0}", message);
        }

        // Obscure the password being entered
        static string ReadPasswordFromConsole()
        {
            string password = string.Empty;
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            while (key.Key != ConsoleKey.Enter);
            return password;
        }
        #endregion

        #region Commands
        // List all the ToDos for the current user.
        // If there is no valid token available, obtain a new one.
        static async Task ListTodo()
        {
            #region Obtain token

            AuthenticationResult result = await TryFetchTokenSilently();

            if (result == null)
            {
                // UserCredential uc = TextualPrompt();
                // if you want to use Windows integrated auth, comment the line above and uncomment the one below
                UserCredential uc = new UserCredential();
                try
                {
                    result = authContext.AcquireTokenAsync(todoListResourceId, clientId, uc).Result;
                }
                catch (Exception ee)
                {
                    ShowError(ee);
                    return;
                }
            }

            #endregion

            #region Call Web API
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = httpClient.GetAsync(todoListBaseAddress + "/api/todolist").Result;

            if (response.IsSuccessStatusCode)
            {
                string rezstring = response.Content.ReadAsStringAsync().Result;
                var todoArray = JArray.Parse(rezstring);
                Console.ForegroundColor = ConsoleColor.Green;
                foreach (var todo in todoArray)
                {
                    Console.WriteLine(todo["Title"]);
                }
            }
            #endregion

            #region Error handling
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // If the To Do list service returns access denied, clear the token cache and have the user sign-in again.                    
                    Console.WriteLine("Sorry, you don't have access to the To Do Service. You might need to sign up.");
                    authContext.TokenCache.Clear();
                }
                else
                {
                    Console.WriteLine("Sorry, an error occurred accessing your To Do list.  Please try again.");
                }
            }
            #endregion
        }
        // Add a new ToDo in the list of the current user.
        // If there is no valid token available, obtain a new one.
        static async Task AddTodo()
        {
            #region Obtain token

            AuthenticationResult result = await TryFetchTokenSilently();

            if (result == null)
            {
                UserCredential uc = TextualPrompt();
                // if you want to use Windows integrated auth, comment the line above and uncomment the one below
                // UserCredential uc = new UserCredential();
                try
                {
                    result = authContext.AcquireTokenAsync(todoListResourceId, clientId, uc).Result;
                }
                catch (Exception ee)
                {
                    ShowError(ee);
                    return;
                }
            }

            #endregion

            #region Call Web API
            Console.WriteLine("Enter new todo description >");
            string descr = Console.ReadLine();

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpContent content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("Title", descr) });

            var response = httpClient.PostAsync(todoListBaseAddress + "/api/todolist", content).Result;

            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("New ToDo '{0}' successfully added.", descr);
            }
            #endregion

            #region Error handling
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // If the To Do list service returns access denied, clear the token cache and have the user sign-in again.                    
                    Console.WriteLine("Sorry, you don't have access to the To Do Service. You might need to sign up.");
                    authContext.TokenCache.Clear();
                }
                else
                {
                    Console.WriteLine("Sorry, an error occurred accessing your To Do list.  Please try again.");
                }
            }
            #endregion

          
        }

        private static async Task<AuthenticationResult> TryFetchTokenSilently()
        {
            AuthenticationResult result = null;

            // first, try to get a token silently
            try
            {
                return result = await authContext.AcquireTokenSilentAsync(todoListResourceId, clientId);
            }
            catch (AdalException adalException)
            {
                // There is no token in the cache; prompt the user to sign-in.
                if (adalException.ErrorCode == AdalError.FailedToAcquireTokenSilently
                    || adalException.ErrorCode == AdalError.InteractionRequired)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("No token in the cache");
                    return result;
                }

                // An unexpected error occurred.
                ShowError(adalException);
            }

            return result;
        }

        // Empties the token cache
        static void ClearCache()
        {
            authContext.TokenCache.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Token cache cleared.");
        }
        static void Help()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("LIST  - lists all of your ToDo. Requires sign in");
            Console.WriteLine("ADD   - adds a new ToDo to your list. Requires sign in");
            Console.WriteLine("CLEAR - empties the token cache, allowing you to sign in as a different user");
            Console.WriteLine("HELP  - displays this page");
            Console.WriteLine("EXIT  - closes this program");
            Console.WriteLine("");
        }
        #endregion
    }
}
