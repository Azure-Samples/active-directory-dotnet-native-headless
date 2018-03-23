---
services: active-directory
platforms: dotnet
author: jmprieur
level: 300
client: .NET Framework 4.6.1 Console 
service: .NET Framework 4.6.1 Web aApi
endpoint: AAD V1
---
# Authenticating to Azure AD non-interactively using a username & password or Windows Integrated Authentication

![Build badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/22/badge)

## About this sample

### Scenario

This sample demonstrates a .Net console application calling a web API that is secured using Azure AD:

![Topology](./ReadmeFiles/Topology.png)

1. The .Net application uses the Active Directory Authentication Library (ADAL) to obtain a JWT access token through the OAuth 2.0 protocol.
2. The access token is sent to the web API to authenticate the user.

This sample shows you how to use ADAL to authenticate users via **raw credentials** (username and password, or Windows integrated authentication) via a text-only interface. Information is available in the ADAL.NET conceptual documentation in [Acquiring tokens with username and password](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Acquiring-tokens-with-username-and-password) and [AcquireTokenSilentAsync using Integrated authentication on Windows (Kerberos)](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-Integrated-authentication-on-Windows-(Kerberos))

### More information about protocols

For more information about how the protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](https://azure.microsoft.com/documentation/articles/active-directory-authentication-scenarios/).

> Looking for previous versions of this code sample? Check out the tags on the [releases](../../releases) GitHub page.

## How To Run This Sample

>[!Note] If you want to run this sample on **Azure Government**, navigate to the "Azure Government Deviations" section at the bottom of this page.
>

To run this sample you will need:

- [Visual Studio 2017](https://aka.ms/vsdownload)
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, please see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account), so if you signed in to the Azure portal with a Microsoft account and have never created a user account in your directory before, you need to do that now. This sample will not work with a Microsoft account.

### Step 1:  Clone or download this repository

From your shell or command line:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-native-headless.git`

### Step 2:  Register the sample with your Azure Active Directory tenant and configure the code accordingly

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects you can:

- either follow the steps in the paragraphs below (Step 2 and Step 3)
- or you can use PowerShell scripts which automatically create for you the Azure AD applications and related objects (passwords, permissions, dependencies) and modify the projects' configuration files. If you still want to use this automation, read the instructions in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

#### Register the TodoListService web API

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **All Services** in the left hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **New application registration**.
5. Enter a friendly name for the application, for example 'TodoListService' and select 'Web app / API' as the Application type. For the Sign-on URL, enter the base URL for the sample, which is by default `https://localhost:44321`. Click on **Create** to create the application.
6. In the succeeding page, find the Application ID value and copy it to the clipboard.
7. Then click on **Settings** and choose **Properties**.
8. For the App ID URI, update the existing value https://\<your_tenant_name\>/TodoListService by replacing \<your_tenant_name\> with the name of your Azure AD tenant.

#### Register the TodoListClient app

1. Sign in to the [Azure portal](https://portal.azure.com).
2. On the top bar, click on your account and under the **Directory** list, choose the Active Directory tenant where you wish to register your application.
3. Click on **All Services** in the left hand nav, and choose **Azure Active Directory**.
4. Click on **App registrations** and choose **New application registration**.
5. Enter a friendly name for the application, for example 'TodoListClient-Headless-DotNet' and select 'Native' as the Application Type. For the redirect URI, enter `https://TodoListClient`. Please note that the Redirect URI will not be used in this sample, but it needs to be defined nonetheless. Click on **Create** to create the application.
6. In the succeeding page, find the Application ID value and copy it to the clipboard.
7. Then click on **Settings** and choose **Properties**.
8. Configure Permissions for your application - in the Settings menu, choose the 'Required permissions' section, click on **Add**, then **Select an API**, and type 'TodoListService' in the textbox and hit enter. Select TodoListService from the results and click the 'Select' button. Then, click on  **Select Permissions** and select 'Access TodoListService'. Click the 'Select' button again to close this screen. Click on "Done" to finish adding the permission.
9. Then select `ToDoListService` in the "Required permissions" blade and click "Grant Permissions". Since our client is a console application using raw credentials, it's incapable of displaying an UI for the user or tenant administrator to grant consent. So you need to provide the user consent for yourself (if you are not a tenant admin) or the admin consent for all users (if you are a tenant admin) in the Azure portal itself. Read more about administrator consent in [Overview of the consent framework](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-integrating-applications#overview-of-the-consent-framework).

### Step 3:  Configure the sample to use your Azure AD tenant

#### Configure the TodoListService project

1. Open the solution in Visual Studio 2017.
2. Open the `web.config` file.
3. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
4. Find the app key `ida:Audience` and replace the value with the App ID URI you registered earlier, for example `https://<your_tenant_name>/TodoListService`.

#### Configure the TodoListClient project

1. Open `app.config`.
2. Find the app key `ida:Tenant` and replace the value with your AAD tenant name.
3. Find the app key `ida:ClientId` and replace the value with the Client ID for the TodoListClient from the Azure portal.
4. Find the app key `todo:TodoListResourceId` and replace the value with the  App ID URI of the TodoListService, for example `https://<your_tenant_name>/TodoListService`
5. Find the app key `todo:TodoListBaseAddress` and replace the value with the base address of the TodoListService project.

### Step 4:  Run the sample

Clean the solution, rebuild the solution, and run it.  You might want to go into the solution properties and set both projects as startup projects, with the service project starting first.

Type the command `add`. You will be prompted for your username and password. Enter them, then enter the description a new todo. That done, type the command `list`. You will see that the command executes without prompting you again. You can clear the token cache by typing `clear`, and leave the client by typing `exit`.
Notice that if you stop the application without clearing the cache, the next time you run the application you won't be prompted to sign-in again - that is the sample implements a persistent cache for ADAL, and remembers the tokens from the previous run.

## How To Deploy This Sample to Azure

To deploy the TodoListService to Azure App Service, you will create an App Service, publish the TodoListService to the app, and update the TodoListClient to call the Azure App instead of IIS Express.

### Create and Publish the TodoListService to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
2. Click New in the top left hand corner, select Web + Mobile --> Web App, select the hosting plan and region, and give your web site a name, e.g. todolistservice-contoso.azurewebsites.net.  Click Create Web Site.
3. Once the web site is created, click on it to manage it.  For this set of steps, download the publish profile and save it.  Other deployment mechanisms, such as from source control, can also be used.
4. Switch to Visual Studio and go to the TodoListService project.  Right click on the project in the Solution Explorer and select Publish.  Click Import, and import the publish profile that you just downloaded.
5. On the Connection tab, update the Destination URL so that it is https, for example https://todolistservice-skwantoso.azurewebsites.net.  Click Next.
6. On the Settings tab, make sure Enable Organizational Authentication is NOT selected.  Click Publish.
7. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the TodoListClient to call the TodoListService Running in Azure Web Sites

1. In Visual Studio, go to the TodoListClient project.
2. Open `app.config`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published, e.g. https://todolistservice-skwantoso.azurewebsites.net.
3. Run the client!  If you are trying multiple different client types (e.g. .Net, Windows Store, Android, iOS) you can have them all call this one published web API.

NOTE:  Remember, the To Do list is stored in memory in this TodoListService sample.  Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied.  Also, if you increase the instance count of the web site, requests will be distributed among the instances and the To Do will not be the same on each instance.

## About The Code

This sample shows how to use the OpenID Connect ASP.Net OWIN middleware to secure calls to an Asp.net Web Api to users of a single Azure Active Directory tenant. The middleware is initialized in the `Startup.Auth.cs` file, by passing it the URL of the Azure AD tenant (token issuer) and the AppId URI , which is the identifier by which the Web API is known to Windows Azure AD.
The middleware then takes care of:

- Downloading the Azure AD metadata, finding the signing keys, and finding the issuer name for the tenant.
- Processing OpenID Connect sign-in responses by validating the signature and issuer in an incoming JWT, extracting the user's claims, and putting them on ClaimsPrincipal.Current.
- Any tokens carrying a different Audience are meant for another resource and will be rejected.

### Acquiring a token with username password

To add an element to the todo list, first the program will try to acquire a token silently from the cache [Program.cs, line 218](https://github.com/Azure-Samples/active-directory-dotnet-native-headless/blob/548946c420fdd777e87480aec968f004029db05e/TodoListClient/Program.cs#L218), if this fails, the program asks for a user name password and creates an instance of ``UserCredential``. This is done in [TextualPrompt()](https://github.com/Azure-Samples/active-directory-dotnet-native-headless/blob/548946c420fdd777e87480aec968f004029db05e/TodoListClient/Program.cs#L89). Then it calls the ``AcquireTokenAsync`` override with the ``UserCredential`` in [Program.cs, line 164](https://github.com/Azure-Samples/active-directory-dotnet-native-headless/blob/548946c420fdd777e87480aec968f004029db05e/TodoListClient/Program.cs#L164).

Since this sample works on .NET framework, it also features the custom serialization of the token cache which happens in [FileCache.cs](https://github.com/Azure-Samples/active-directory-dotnet-native-headless/blob/update/TodoListClient/FileCache.cs)

### Acquiring a token with Windows Integrated security

If your PC is domain joined or AAD joined, you can also use the Windows integrated security. For this, instead of calling TextualPrompt(), you need to uncomment the line creating an instance of UserCredential without parameters:
See [Program.cs](https://github.com/Azure-Samples/active-directory-dotnet-native-headless/blob/update/TodoListClient/Program.cs#L159-L161)

```CSharp
UserCredential uc = new UserCredential();
```

You can trigger the middleware to send an OpenID Connect sign-in request by decorating a class or method with the `[Authorize]` attribute

## Troubleshooting

If you get the following error: ``Inner Exception : AADSTS65001: The user or administrator has not consented to use the application with ID *your app ID* named 'TodoListClient'. Send an interactive authorization request for this user and resource``, then check that you have done bullet point 9 of [Register the TodoListClient app](README.md#register-the-todolistclient-app)

## How To Recreate This Sample

### Code for the service

1. In Visual Studio 2017, create a new `Visual C#` `ASP.NET Web Application (.NET Framework)`. Choose `Web Api` in the next screen. Leave the project's chosen authentication mode as the default, i.e. `No Authentication`.
2. Set SSL Enabled to be True.  Note the SSL URL.
3. Add the following ASP.Net OWIN middleware NuGets: `Microsoft.Owin.Security.ActiveDirectory` and `Microsoft.Owin.Host.SystemWeb`.
4. Add a class named `TodoItem` in the `Models` folder. Add the properties `Title` and `Owner` in this class.
5. Add a new `Web Api 2 Controller - Empty`  named `TodoListController` in the service.
6. The `TodoListController` will have an in-memory list of ToDo items and methods to read and write from that list. Refer to the provided `TodoListController` code for more details.
7. In the `App_Start` folder, create a class `Startup.Auth.cs`.You will need to remove `.App_Start` from the namespace name.  Replace the code for the `Startup` class with the code from the same file of the sample app.  Be sure to take the whole class definition!  The definition changes from `public class Startup` to `public partial class Startup`
8. In `Startup.Auth.cs` resolve missing references by adding `using` statements as suggested by Visual Studio intellisense.
9. Right-click on the project, select Add, select "Class", and in the search box enter "OWIN".  "OWIN Startup class" will appear as a selection; select it, and name the class `Startup.cs`.
10. In `Startup.cs`, replace the code for the `Startup` class with the code from the same file of the sample app.  Again, note the definition changes from `public class Startup` to `public partial class Startup`.
11. If you want the user to be required to sign-in before they can see any page of the api, then in the `HomeController`, decorate the `HomeController` class with the `[Authorize]` attribute.  If you leave this out, the user will be able to see the home page of the app without having to sign-in first, and can click the sign-in link on that page to get signed in.
12. In the `web.config` file, in `<appSettings>`, create keys for `ida:Tenant`, and `ida:Audience` and set the values accordingly.
13. Almost done!  Follow the steps in "Running This Sample" to register the application in your AAD tenant.

### Code for the console client

1. In Visual Studio 2017, create a new `Visual C#` `Console App (>NET Framework)`.
2. Add the nugets: `Microsoft.Owin.Security.ActiveDirectory`, `Newtonsoft.Json` and `System.Net.Http`.
3. Add a reference for the `System.Security' assembly in the project.
4. In `Program.cs`, replace the code for the `Program` class with the code from the same file of the sample app. Resolve missing references by adding `using` statements as suggested by Visual Studio intellisense.
5. Add a new class called `FileCache.cs` in the project. It is a simple persistent cache implementation for a desktop application. It uses DPAPI for storing tokens in a local file.Replace the code for the `FileCache` class with the code from the same file of the sample app.  Be sure to take the whole class definition!
6. In the `FileCache.cs` class, resolve missing references by adding `using` statements as suggested by Visual Studio intellisense.
7. In `app.config`, in `<appSettings>`, create keys for `ida:Tenant`, `ida:ClientId`, `ida:AADInstance`, `todo:TodoListResourceId` and `todo:TodoListBaseAddress` and set the values accordingly.  For the public Azure AD, the value of `ida:AADInstance` is `https://login.microsoftonline.com/{0}`.

## Azure Government Deviations

In order to run this sample on Azure Government you can follow through the steps above with a few variations:

- Step 2:
  - You must register this sample for your AAD Tenant in Azure Government by following Step 2 above in the [Azure Government portal](https://portal.azure.us).
- Step 3:
  - Before configuring the sample, you must make sure your [Visual Studio is connected to Azure Government](https://docs.microsoft.com/azure/azure-government/documentation-government-get-started-connect-with-vs).
  - Navigate to the Web.config file. Replace the `ida:AADInstance` property in the Azure AD section with `https://login.microsoftonline.us/`.

Once those changes have been accounted for, you should be able to run this sample on Azure Government.

## More information

For more information see ADAL.NET's conceptual documentation:

- [Recommanded pattern to acquire a token](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-a-cached-token#recommended-pattern-to-acquire-a-token)
- [Acquiring tokens with username and password](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Acquiring-tokens-with-username-and-password)
- [AcquireTokenSilentAsync using Integrated authentication on Windows (Kerberos)](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/AcquireTokenSilentAsync-using-Integrated-authentication-on-Windows-(Kerberos))
- [Customizing Token cache serialization](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/wiki/Token-cache-serialization)
