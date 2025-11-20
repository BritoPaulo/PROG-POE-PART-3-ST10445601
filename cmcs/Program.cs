using Microsoft.EntityFrameworkCore;
using cmcs.Data;
using cmcs.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ClaimValidationService>();

var app = builder.Build();

// DEVELOPMENT: Force database recreation with latest schema
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Delete and recreate the database
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();

        // Optional: Add sample data
        if (!dbContext.Claims.Any())
        {
            dbContext.Claims.AddRange(
                new cmcs.Models.Claim
                {
                    ClaimMonth = new DateTime(2025, 1, 1),
                    TotalHours = 40.0m,
                    RatePerHour = 300.0m,
                    Notes = "January teaching hours",
                    Status = "Submitted",
                    WorkflowStatus = "Submitted",
                    LecturerName = "Demo Lecturer",
                    LecturerEmail = "lecturer@iie.com",
                    SubmittedDate = DateTime.Now.AddDays(-10)
                },
                new cmcs.Models.Claim
                {
                    ClaimMonth = new DateTime(2025, 2, 1),
                    TotalHours = 35.5m,
                    RatePerHour = 320.0m,
                    Notes = "February lectures and marking",
                    Status = "Under Review",
                    WorkflowStatus = "Under Review",
                    LecturerName = "Demo Lecturer",
                    LecturerEmail = "lecturer@iie.com",
                    SubmittedDate = DateTime.Now.AddDays(-5)
                }
            );
            dbContext.SaveChanges();
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();