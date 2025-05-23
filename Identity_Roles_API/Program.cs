using System;
using FluentValidation.AspNetCore;
using Identity_Roles_API.Data;
using Identity_Roles_API.Repos;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509.Qualified;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Identity_Roles_API.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TestRESTAPI.Extentions;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Identity_Roles_API.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers()
 .AddViewLocalization()
 .AddDataAnnotationsLocalization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//edit swagger to accept header
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<AddAcceptLanguageHeaderParameter>();
});

builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("mycon")));


builder.Services.AddSwaggerGenJwtAuth();
builder.Services.AddCustomJwtAuth(builder.Configuration);

builder.Services.AddScoped(typeof(IBase<>),typeof(Base<>)) ;

builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

//inject identity 
builder.Services.AddIdentityCore<AppUser>()
    .AddRoles<IdentityRole>()  
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//auto mapping
builder.Services.AddAutoMapper(typeof(MappingProfile));

//localization
///////////////////////////////////
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources"); // path=> folder  Resources

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
//////////////////////////////////
var app = builder.Build();

//seeding roles
//////////////////////////////////////////////
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = UserRoles.GetRoles();

    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}


////////////////////////////////////////////



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//request localization
//////////////////////////
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);
//////////////////////////

app.UseStaticFiles(); 

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
