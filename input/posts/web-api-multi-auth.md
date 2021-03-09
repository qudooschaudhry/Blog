Title: ASP.NET Web API Multi-Auth
Published: 2021-03-05
Tags:
    - .Net
    - Authorization
    - JWT
---

We are building an application that is to be used by our customers, but also some internal folks at our company. Since we are on office 365, we thought it made sense to have our internal team login using office 365. The external clients were setup using asp.net Identity aka local accounts.

Seting up the multiple authorizations is quite easy as it all happens inside the startup pipeline through the use of middleware. The Microsoft Identity platform has a very handy extension method for setting that up. 

```c
services.AddAuthentication().AddMicrosoftIdentityWebApi(Configuration, "AzureAd", "AzureAd");`
```
The second parmeter is the key of the configuration item in the `appsettings.json` like so 


```json
"AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Tenant": "<your tenant name>",
    "Audience": "<the client id of the registered application within Azure>",
    "TenantId": "<your tenante id>",
    "ClientId": "<the client id of the registered application within Azure>",
    "CallbackPath": "/signin-oidc"
  }
```

The third parameter is a name you give to this authentication scheme, which can later be utilized on the controllers or actions like so 

```c
    [Route("[controller]"), ApiController, Authorize(AuthenticationSchemes = "AzureAd")]
    public class MyController : BaseController
```

for the local accounts, I was so inspired by the clean one line setup of the `AddMicrosoftIdentityWebApi extension` method that I added my own in the same way 


```c
    services.AddLocalAuthorization(Configuration);
```

and then I added my extension class like so 

```c

public static class AuthenticationExtensions
    {
        public static IServiceCollection AddLocalAuthorization(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            serviceCollection
                .AddAuthorization(options =>
                {
                    options.AddLocalAuthPolicy();
                })
                .AddAuthentication("LocalAccountsScheme")
                .AddJwtBearer("LocalAccountsScheme",
                    options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateIssuer = true,
                            ValidIssuer = configuration.GetSection("TokenConfiguration")["Issuer"],
                            ValidateAudience = true,
                            ValidAudience = configuration.GetSection("TokenConfiguration")["Audience"],
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("TokenConfiguration")["SecretKey"]))
                        };
                    });

            return serviceCollection;
        }

        private static AuthorizationOptions AddLocalAuthPolicy(this AuthorizationOptions options)
        {
            var policy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("LocalAccountsScheme")
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy("LocalAccountsPolicy", policy);
            return options;
        }
    }

```

and then the controllers are decorated the same way 

```c
    [Route("[controller]"), ApiController, Authorize(AuthenticationSchemes = "LocalAccountsScheme")]
    public class MyController : BaseController
```

Now, when the Bearer token is passed from the UI, the appropriate mechanism to validate the token is used based on the authentication scheme on the controller. 

One thing to remember when registering the application in Azure AD is to ensure it is setup as a SPA application and issues Id_Tokens. Without that, the UI application will not be able to get the JWT tokens from Azure AD. 

Happy coding. 

~Q


