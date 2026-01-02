using kz_webshop_be.Interfaces;
using kz_webshop_be.Mapping;
using kz_webshop_be.Repository;
using kz_webshop_be.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFunkoPopRepository, FunkoPopRepository>();
builder.Services.AddScoped<ILabubuRepository, LabubuRepository>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IEnumService, EnumService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserIdentityService, UserIdentityService>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
        .AllowCredentials(); 
    });
});


builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://securetoken.google.com/zeem-funko";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://securetoken.google.com/zeem-funko",
            ValidateAudience = true,
            ValidAudience = "zeem-funko",
            ValidateLifetime = true,
        };
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; 
    });

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("ProductActivationJob");

    q.AddJob<ProductActivationJob>(opts => opts.WithIdentity(jobKey));

    var cronExpr = builder.Configuration["Quartz:ProductActivationJobCron"];
    if (string.IsNullOrWhiteSpace(cronExpr))
    {
        throw new InvalidOperationException("The cron expression for ProductActivationJob is not configured.");
    }

    _ = q.AddTrigger(opts => opts
       .ForJob(jobKey)
       .WithIdentity("ProductActivationJob-trigger")
       .WithCronSchedule(cronExpr));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
