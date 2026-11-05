using Microsoft.AspNetCore.RateLimiting;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(o=>o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddRateLimiter(o=>{o.RejectionStatusCode=429;o.AddFixedWindowLimiter("auth",x=>{x.PermitLimit=10;x.Window=TimeSpan.FromMinutes(1);x.QueueLimit=0;});});
builder.Services.AddDbContext<AppDbContext>(o=>o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentity<AppUser,IdentityRole>(o=>{o.Password.RequiredLength=8;o.Password.RequireDigit=true;o.Password.RequireUppercase=true;o.Password.RequireNonAlphanumeric=true;o.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(15);o.Lockout.MaxFailedAccessAttempts=5;o.User.RequireUniqueEmail=true;}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o=>{o.LoginPath="/Account/Login";o.AccessDeniedPath="/Account/Denied";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;o.Cookie.SecurePolicy=CookieSecurePolicy.Always;});
builder.Services.AddHttpContextAccessor();builder.Services.AddScoped<AuditService>();
var app=builder.Build();
// Explicit one-time CLI command; ordinary startup never invokes provisioning.
if(args.Length==1 && args[0]=="--bootstrap-initial-admin")
{
    Environment.ExitCode=await InitialAdminBootstrap.RunAsync(app.Services,app.Configuration);
    return;
}

using(var scope=app.Services.CreateScope()){var db=scope.ServiceProvider.GetRequiredService<AppDbContext>();// Existing EnsureCreated databases require a reviewed migration baseline before switching to Migrate.
await db.Database.EnsureCreatedAsync();var roles=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();foreach(var role in new[]{"Admin","Manager","SalesExecutive"})if(!await roles.RoleExistsAsync(role))await roles.CreateAsync(new IdentityRole(role));}
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}
app.UseHttpsRedirection();app.UseStaticFiles();app.UseRouting();app.UseRateLimiter();app.UseAuthentication();app.UseAuthorization();
app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");app.MapControllers();app.Run();