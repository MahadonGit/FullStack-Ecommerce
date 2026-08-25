using Ecommerce.Helpers;
using Ecommerce.Infrastructure.CustomMDW;
using Ecommerce.Mapping;
using Ecommerce.Models;
using Ecommerce.Services.Implementations;
using Ecommerce.Services.Interfaces;
using ECommerce.Data;
using ECommerce.Services.Implementations;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

//Adding the automapper service

builder.Services.AddAutoMapper(cfg =>
{
}, typeof(MappingProfile));


///Adding DbContext with SQL Server connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


//Adding the identity services for user authentication and authorization
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>  //here we assigned properites for password and user requirements
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


// Adding JWT authentication services and configuring the token validation parameters
builder.Services
 .AddAuthentication(options =>
 {
     options.DefaultAuthenticateScheme =
         JwtBearerDefaults.AuthenticationScheme;

     options.DefaultChallengeScheme =
         JwtBearerDefaults.AuthenticationScheme;
 })
 .AddJwtBearer(options =>
 {
     // Getting the JWT key from the configuration and throwing an exception if it's missing
     var jwtKey = builder.Configuration["Jwt:Key"]
         ?? throw new InvalidOperationException(
             "JWT Key is missing.");


     // Setting up the token validation parameters for JWT authentication that comes form the tokenservice
     options.TokenValidationParameters =
         new TokenValidationParameters
         {
             ValidateIssuer = true,

             ValidateAudience = true,

             ValidateLifetime = true,

             ValidateIssuerSigningKey = true,

             ValidIssuer =
                 builder.Configuration["Jwt:Issuer"],

             ValidAudience =
                 builder.Configuration["Jwt:Audience"],

             IssuerSigningKey =
                 new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(jwtKey)),

             ClockSkew = TimeSpan.Zero
         };
 });





//Registering the TokenService for dependency injection

builder.Services.AddScoped<ITokenService, TokenService>();

//Reh=gistering the Auth services 

builder.Services.AddScoped<IAuthService, AuthService>();

//Registerung th email service

builder.Services.AddScoped<IEmailService, EmailService>();

//register notification service

builder.Services.AddScoped<INotificationService, NotificationService>();

//Registering the payment service

builder.Services.AddScoped<IPaymentService, PaymentService>();

//Registering the swagger services
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ecommerce API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});


// Add OpenAPI


var app = builder.Build();

//Roleseeder....

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await RoleSeeder.SeedRolesAsync(services);
    await DbSeeder.SeedRolesAndAdminAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();               

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();