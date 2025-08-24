using Microsoft.EntityFrameworkCore;
using Seu.Mail.Data.Context;
using Seu.Mail.Services.Extensions;
using Seu.Mail.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Entity Framework
builder.Services.AddDbContext<EmailDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add all email services using extension method
builder.Services.AddEmailServices(builder.Configuration);

// Add calendar module
builder.Services.AddCalendarModule();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Database will be created by EnsureCreated() below

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Add security middleware
app.UseSecurityMiddleware();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EmailDbContext>();
    context.Database.EnsureCreated();
}

app.Run();