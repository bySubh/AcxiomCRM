using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;
[Authorize] public class HomeController(AppDbContext db,UserManager<AppUser> users):Controller {
public async Task<IActionResult> Index(string period="all",DateTime? from=null,DateTime? to=null){
var id=users.GetUserId(User);var admin=User.IsInRole("Admin");var manager=User.IsInRole("Manager");
var team=users.Users.Where(u=>u.ManagerId==id).Select(u=>u.Id);
var customers=db.Customers.Where(x=>admin||x.OwnerId==id||(manager&&team.Contains(x.OwnerId)));
var leads=db.Leads.Where(x=>admin||x.OwnerId==id||(manager&&team.Contains(x.OwnerId)));
var opportunities=db.Opportunities.Where(x=>admin||x.OwnerId==id||(manager&&team.Contains(x.OwnerId)));
var today=DateTime.UtcNow.Date;
if(period=="today"){from=today;to=today;}else if(period=="week"){from=today.AddDays(-((int)today.DayOfWeek+6)%7);to=today;}else if(period=="month"){from=new DateTime(today.Year,today.Month,1);to=today;}
if(from.HasValue){var d=from.Value.Date;customers=customers.Where(x=>x.CreatedAt>=d);leads=leads.Where(x=>x.CreatedAt>=d);opportunities=opportunities.Where(x=>x.CreatedAt>=d);}
if(to.HasValue){var d=to.Value.Date.AddDays(1);customers=customers.Where(x=>x.CreatedAt<d);leads=leads.Where(x=>x.CreatedAt<d);opportunities=opportunities.Where(x=>x.CreatedAt<d);}
ViewBag.Customers=await customers.CountAsync();ViewBag.Leads=await leads.CountAsync();
ViewBag.OpenLeads=await leads.CountAsync(x=>x.Status!="Lost"&&x.Status!="Converted"&&x.Status!="Unqualified");
ViewBag.Opportunities=await opportunities.CountAsync();
ViewBag.Open=await opportunities.CountAsync(x=>x.Stage!="Won"&&x.Stage!="Lost");
ViewBag.Won=await opportunities.CountAsync(x=>x.Stage=="Won");ViewBag.Lost=await opportunities.CountAsync(x=>x.Stage=="Lost");
// Project only the authorized, date-filtered open opportunity values before decimal aggregation.
var openAmounts=await opportunities.Where(x=>x.Stage!="Won"&&x.Stage!="Lost")
    .Select(x=>new {x.Amount,x.Probability}).ToListAsync();
ViewBag.Pipeline=openAmounts.Sum(x=>x.Amount);
ViewBag.Weighted=openAmounts.Sum(x=>x.Amount*x.Probability/100m);
ViewBag.Stages=await opportunities.GroupBy(x=>x.Stage).Select(g=>new {name=g.Key,count=g.Count()}).ToListAsync();
ViewBag.LeadStatuses=await leads.GroupBy(x=>x.Status).Select(g=>new{name=g.Key,count=g.Count()}).ToListAsync();
// SQLite cannot translate decimal SUM; materialize only the authorized won amounts and dates.
var wonAmounts=await opportunities.Where(x=>x.Stage=="Won")
    .Select(x=>new {x.CreatedAt,x.Amount}).ToListAsync();
ViewBag.Monthly=wonAmounts.GroupBy(x=>new{x.CreatedAt.Year,x.CreatedAt.Month})
    .Select(g=>new{year=g.Key.Year,month=g.Key.Month,total=g.Sum(x=>x.Amount)})
    .OrderBy(x=>x.year).ThenBy(x=>x.month).ToList();
ViewBag.Period=period;ViewBag.From=from?.ToString("yyyy-MM-dd");ViewBag.To=to?.ToString("yyyy-MM-dd");return View();}
[AllowAnonymous]public IActionResult Error()=>Content("An unexpected error occurred.");}
