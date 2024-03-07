using BudgetBuddy.Data;
using BudgetBuddy.Services.Authentication;
using BudgetBuddy.Services.Repositories.Account;
using BudgetBuddy.Services.Repositories.Achievement;
using BudgetBuddy.Services.Repositories.User;
using BudgetBuddy.Services.Repositories.Transaction;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

Env.TraversePath().Load("../.envs/server.env");
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ITransactionRepository, TransactionRepository>();
builder.Services.AddTransient<IAchievementRepository, AchievementRepository>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddTransient<IAccountRepository, AccountRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddDbContext<BudgetBuddyContext>(options =>
{
    Console.WriteLine("Trying to connect to database...");
    options.UseSqlServer(connectionString);
    Console.WriteLine("Connected to database!");
});

builder.Services.AddAuthentication(options => { 
    options.DefaultScheme = "Cookies"; 
}).AddCookie("Cookies", options => {
    options.Cookie.Name = "Cookie_Name";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Events = new CookieAuthenticationEvents
    {                          
        OnRedirectToLogin = redirectContext =>
        {
            redirectContext.HttpContext.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    };                
});

builder.Services.AddCors(options =>  
    options.AddPolicy("Development", builder =>  
    {  
        // Allow multiple HTTP methods  
        builder.WithMethods("GET", "POST", "PATCH", "DELETE", "OPTIONS")  
            .WithHeaders(  
                HeaderNames.Accept,  
                HeaderNames.ContentType,  
                HeaderNames.Authorization)  
            .AllowCredentials()  
            .SetIsOriginAllowed(origin =>  
            {  
                if (string.IsNullOrWhiteSpace(origin)) return false;  
                if (origin.ToLower().StartsWith("http://localhost")) return true;  
                return false;  
            });  
    })  
);  

var app = builder.Build();

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Migrating database...");
    logger.LogInformation("Database not ready yet; waiting...");
    Thread.Sleep(10000);
    serviceScope.ServiceProvider.GetRequiredService<BudgetBuddyContext>().Database.Migrate();
    logger.LogInformation("Database migrated successfully.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var connection ="http://localhost:5173/";
app.UseCors(b => {
    b.WithOrigins(connection!)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithExposedHeaders("content-type") // Allow the 'content-type' header to be exposed
        .SetIsOriginAllowed(_ => true); // Allow any origin for CORS
});

app.UseCookiePolicy(  
    new CookiePolicyOptions  
    {  
        Secure = CookieSecurePolicy.Always  
    });  

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();