using ModelContextProtocol.Server;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using WinterPasswordLib;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .AllowAnyOrigin()
    .WithMethods("GET", "POST", "OPTIONS")
    .WithHeaders("Content-Type", "Authorization", "Mcp-Session-Id", "Mcp-Protocol-Version")
    .WithExposedHeaders("Mcp-Session-Id", "Www-Authenticate")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Use the Issuer URL (base URL) as Authority for OIDC discovery
    options.Authority = builder.Configuration["Scalekit:Issuer"] ?? throw new InvalidOperationException("Scalekit Issuer is not configured");

    // For local development, you might need this if using HTTP
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Scalekit:Issuer"],
        ValidAudience = builder.Configuration["Scalekit:Audience"] ?? throw new InvalidOperationException("Scalekit Audience is not configured"),
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var name = context.Principal?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value ?? "unknown";
            logger.LogInformation("Token validated for: {Name}", name);
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(context.Exception, "Authentication failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            // context.Response.Headers.Append(
            //     "WWW-Authenticate",
            //     $"Bearer realm=\"OAuth\", resource_metadata=\"{builder.Configuration["Scalekit:OAuthProtectedResource"]!}\"");
            // context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            logger.LogInformation("Challenging client to authenticate");
            // context.HandleResponse();
            return Task.CompletedTask;
        }
    };
}).AddMcp(options =>
{
    options.ResourceMetadata = new()
    {
        AuthorizationServers = { new Uri(builder.Configuration["Scalekit:EnvironmentUrl"]!) },
        ScopesSupported = ["ponypwd:generate"],
    };
});;

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly()
    .WithResourcesFromAssembly();

var app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapMcp().RequireAuthorization();

app.Run();

[McpServerToolType]
public static class WinterPasswordTools
{
    private static readonly ActivitySource source = new("McpStreamableAuthServer");

    [McpServerTool, Description("Builds a password from winter words.")]
    public static string WinterPassword(
        [Description("Minimum length of the password")] int minLength = 16,
        [Description("Enable special character replacement")] bool special = false)
    {
        using var activity = source.StartActivity("WinterPassword");
        activity?.SetTag("minLength", minLength);
        activity?.SetTag("special", special);
        
        var opts = new PasswordGenerationOptions { MinLength = minLength, Special = special };
        var output = PasswordGenerator.BuildPassword(opts);
        return output;
    }

    [McpServerTool(Name = "winter_password_batch"), Description("Generates N passwords with the same options.")]
    public static string[] WinterPasswordBatch(
        [Description("Number of passwords to generate")] int count = 5,
        [Description("Minimum length of the password")] int minLength = 16,
        [Description("Enable special character replacement")] bool special = false)
    {
        using var activity = source.StartActivity("WinterPasswordBatch");
        activity?.SetTag("count", count);
        activity?.SetTag("minLength", minLength);
        activity?.SetTag("special", special);

        var opts = new PasswordGenerationOptions { MinLength = minLength, Special = special };
        return PasswordGenerator.BuildMany(count, opts);
    }

    [McpServerTool(Name = "winter_password_with_custom_words"), Description("Builds a password from winter words. The user can override the built-in words with their own custom words.")]
    public static async Task<string> WinterPasswordWithCustomWords(
        McpServer server,
        [Description("Minimum length of the password")] int minLength = 16,
        [Description("Enable special character replacement")] bool special = false)
    {
        string[]? customWords = null;

        using (var activity = source.StartActivity("Elicitation for custom words"))
        {
            // Check if the client supports elicitation
            if (server.ClientCapabilities?.Elicitation == null)
            {
                throw new McpException("Client does not support elicitation");
            }

            // Ask the user if they want to use custom words
            var useCustomSchema = new ElicitRequestParams.RequestSchema
            {
                Properties =
            {
                ["UseCustomWords"] = new ElicitRequestParams.BooleanSchema
                {
                    Title = "Use Custom Words",
                    Description = "Do you want to provide your own winter words instead of using the built-in ones?"
                }
            }
            };

            var useCustomResponse = await server.ElicitAsync(new ElicitRequestParams
            {
                Message = "Do you want to use custom winter words?",
                RequestedSchema = useCustomSchema
            }, CancellationToken.None);


            // If user wants to provide custom words
            if (useCustomResponse.Action == "accept" && useCustomResponse.Content?["UseCustomWords"].ValueKind == JsonValueKind.True)
            {
                var wordsSchema = new ElicitRequestParams.RequestSchema
                {
                    Properties =
                {
                    ["CustomWords"] = new ElicitRequestParams.StringSchema
                    {
                        Title = "Custom Words",
                        Description = "List your custom winter words, separated by commas (e.g., Snowflake, Icicle, Frost, Winter)",
                        MinLength = 1
                    }
                }
                };

                var wordsResponse = await server.ElicitAsync(new ElicitRequestParams
                {
                    Message = "Enter your custom winter words (comma-separated):",
                    RequestedSchema = wordsSchema
                }, CancellationToken.None);

                if (wordsResponse.Action == "accept")
                {
                    var wordsString = wordsResponse.Content?["CustomWords"].GetString();
                    if (!string.IsNullOrWhiteSpace(wordsString))
                    {
                        customWords = [.. wordsString.Split(',')
                            .Select(w => w.Trim())
                            .Where(w => !string.IsNullOrWhiteSpace(w))];
                    }
                }
            }
            else
            {
                // User chose not to provide custom words
                customWords = PasswordGenerator.DefaultWords;
            }
        }

        using var activity2 = source.StartActivity("Generating password with custom words");
        var opts = new PasswordGenerationOptions { MinLength = minLength, Special = special };
        var output = PasswordGenerator.BuildPassword(opts, customWords);
        return output;
    }
}

[McpServerPromptType]
public static class WinterPasswordPrompts
{
    [McpServerPrompt, Description("Prompt to generate a password from winter words")]
    public static ChatMessage MakeWinterPassword(
        [Description("Minimum length of the password")] string minLength = "16",
        [Description("Enable special character replacement")] string special = "false")
    {
        var specialBool = special.ToLower() == "true";
        return new ChatMessage(
            ChatRole.User,
            $"""
            Generate a secure password from winter words.
            - Minimum length: {minLength}
            - Special character replacement active: {specialBool}
            Replacement rules (if active): o/O→0, i/I→!, e/E→€, s/S→$.
            """
        );
    }
}

[McpServerResourceType]
public static class WinterWordResources
{
    [McpServerResource(Name = "winter-characters-text"), Description("Winter words (text) - One word per line from data/winter-words.txt")]
    public static string WinterCharactersText() => JsonSerializer.Serialize(PasswordGenerator.DefaultWords);
}

