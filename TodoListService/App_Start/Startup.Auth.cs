using System.IdentityModel.Tokens;
using Microsoft.Owin.Security.ActiveDirectory;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace TodoListService
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Tenant = ConfigurationManager.AppSettings["ida:Tenant"],
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidAudience = ConfigurationManager.AppSettings["ida:Audience"]
                    }
                });
        }
    }
}