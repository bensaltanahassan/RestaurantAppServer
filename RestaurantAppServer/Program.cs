using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RestaurantAppServer.Data;
using RestaurantAppServer.Interfaces;
using RestaurantAppServer.Service.Models;
using RestaurantAppServer.Service.Services;
using RestaurantAppServer.Services;
using RestaurantAppServer.Utils;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(op
    => op.UseSqlServer(builder.Configuration.GetConnectionString("myCon"))
);

// adding authentication using jwt

builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfigurator>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();


// configure swagger for jwt token

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the bearer scheme"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
         {
                new OpenApiSecurityScheme()
                {

                            Reference = new OpenApiReference()
                            {
                                Id="Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                },new List<string>()
         }
    });
});

//Add token service config 
builder.Services.AddSingleton<JwtTokenService>();




//add email config
var emailConfig = builder.Configuration
    .GetSection("EmailConfiguration")
    .Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);

builder.Services.AddScoped<IEmailService, EmailService>();


// Cloudinary settings
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Enable CORS
app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
