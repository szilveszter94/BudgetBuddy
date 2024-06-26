using System.Text;
using BudgetBuddy.Data;
using BudgetBuddy.Model;
using BudgetBuddy.Services.AchievementService;
using BudgetBuddy.Services.Authentication;
using BudgetBuddy.Services.FinancialNewsService;
using BudgetBuddy.Services.GoalServices;
using BudgetBuddy.Services.ReportServices;
using BudgetBuddy.Services.Repositories.Account;
using BudgetBuddy.Services.Repositories.Achievement;
using BudgetBuddy.Services.Repositories.Goal;
using BudgetBuddy.Services.Repositories.Report;
using BudgetBuddy.Services.Repositories.Transaction;
using BudgetBuddy.Services.Repositories.User;
using BudgetBuddy.Services.TransactionServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

var builder = WebApplication.CreateBuilder(args);

var conf = builder.Configuration;

AddServices();
ConfigureSwagger();
AddDbContext();
AddAuthentication();
AddIdentity();

var app = builder.Build();


using var scope = app.Services.CreateScope();
var authenticationSeeder = scope.ServiceProvider.GetRequiredService<IAuthenticationSeeder>();
authenticationSeeder.AddRoles();
authenticationSeeder.AddAdmin();

var achievementSeeder = scope.ServiceProvider.GetRequiredService<AchievementSeeder>();
await achievementSeeder.SeedAchievementsAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var connection ="http://localhost:8080/";
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

void AddServices(){
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddTransient<IReportService, ReportService>();
    builder.Services.AddTransient<ITransactionRepository, TransactionRepository>();
    builder.Services.AddTransient<IAchievementRepository, AchievementRepository>();
    builder.Services.AddScoped<IAchievementService, AchievementService>();
    builder.Services.AddScoped<AchievementSeeder>();
    builder.Services.AddTransient<IReportRepository, ReportRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddTransient<IAccountRepository, AccountRepository>();
    builder.Services.AddTransient<IUserRepository, UserRepository>();
    builder.Services.AddTransient<IGoalRepository, GoalRepository>();
    builder.Services.AddTransient<IGoalService, GoalService>();
    builder.Services.AddTransient<ITransactionService, TransactionService>();
    builder.Services.AddScoped<IFinancialNewsProvider, FinancialNewsProvider>();
    builder.Services.AddScoped<IAuthenticationSeeder, AuthenticationSeeder>(provider =>
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            
        // Fetch adminInfo from configuration or any other source
        var adminInfo = new Dictionary<string, string>
        {
            {"adminEmail", conf["AdminInfo_AdminEmail"]},
            {"adminPassword", conf["AdminInfo_AdminPassword"]}
        };

        return new AuthenticationSeeder(roleManager, userManager, adminInfo);
    });
    // builder.Services.AddTransient<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ITokenService>(provider =>
        new TokenService(conf["JwtSettings_ValidIssuer"], conf["JwtSettings_ValidAudience"], conf["JwtSettings_IssuerSigningKey"]));
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
}

void ConfigureSwagger()
{
    builder.Services.AddSwaggerGen(option =>
    {
        option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
        option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });
        option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                new string[]{}
            }
        });
    });
}

void AddDbContext()
{
    builder.Services.AddDbContext<BudgetBuddyContext>(options =>
    {
        Console.WriteLine("Trying to connect to database...");
        options.UseSqlServer(conf["DB_CONNECTION_STRING"]);
        Console.WriteLine("Connected to database!");
    });
}

void AddIdentity()
{
    builder.Services
        .AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddRoles<IdentityRole>() 
        .AddEntityFrameworkStores<BudgetBuddyContext>();
}

void AddAuthentication()
{
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
    
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = conf["JwtSettings_ValidIssuer"],
                ValidAudience = conf["JwtSettings_ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(conf["JwtSettings_IssuerSigningKey"])
                ),
            };
        });
}

public partial class Program 
{}