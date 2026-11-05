using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;
[Authorize]public class LeadsController(AppDbContext db,UserManager<AppUser> users,AuditService audit):Controller{
private bool Full=>User.IsInRole("Admin");
private IQueryable<string> TeamIds=>users.Users.Where(u=>u.ManagerId==users.GetUserId(User)).Select(u=>u.Id);
private bool Team=>User.IsInRole("Manager");
public async Task<IActionResult> Index(string? search,int page=1){var uid=users.GetUserId(User);var q=db.Leads.AsNoTracking().Where(x=>Full||(Team && (x.OwnerId==uid || TeamIds.Contains(x.OwnerId)))||x.OwnerId==uid);if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.Name.Contains(search)||x.CompanyName.Contains(search)||x.Status.Contains(search)||x.OwnerId.Contains(search));ViewBag.Search=search;page=Math.Max(1,page);const int pageSize=25;ViewBag.Page=page;ViewBag.TotalPages=(int)Math.Ceiling(await q.CountAsync()/(double)pageSize);return View(await q.OrderByDescending(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync());}
public IActionResult Create()=>View("Form",new Lead());
[HttpPost][ValidateAntiForgeryToken]public async Task<IActionResult> Create(Lead item){var uid=users.GetUserId(User)??"";var all=Full;ModelState.Remove("OwnerId");if(!ModelState.IsValid)return View("Form",item);item.OwnerId=uid;db.Leads.Add(item);try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Duplicate or invalid database value.");return View("Form",item);}await audit.Write(User,"Create","Lead",item.Id.ToString());return RedirectToAction("Index");}
public async Task<IActionResult> Edit(int id){var uid=users.GetUserId(User);var item=await db.Leads.FirstOrDefaultAsync(x=>x.Id==id&&(Full||(Team && (x.OwnerId==uid || TeamIds.Contains(x.OwnerId)))||x.OwnerId==uid));return item==null?NotFound():View("Form",item);}
[HttpPost][ValidateAntiForgeryToken]public async Task<IActionResult> Edit(int id,Lead item){var uid=users.GetUserId(User);var all=Full;var current=await db.Leads.FirstOrDefaultAsync(x=>x.Id==id&&(all||(Team && (x.OwnerId==uid || TeamIds.Contains(x.OwnerId)))||x.OwnerId==uid));if(current==null)return NotFound();ModelState.Remove("OwnerId");if(!ModelState.IsValid)return View("Form",item);item.Id=id;item.OwnerId=current.OwnerId;item.CreatedAt=current.CreatedAt;db.Entry(current).CurrentValues.SetValues(item);current.OwnerId=uid==current.OwnerId?current.OwnerId:current.OwnerId;db.Entry(current).Property(x=>x.OwnerId).IsModified=false;db.Entry(current).Property(x=>x.CreatedAt).IsModified=false;try{await db.SaveChangesAsync();}catch(DbUpdateException){ModelState.AddModelError("","Duplicate or invalid database value.");return View("Form",item);}await audit.Write(User,"Update","Lead",id.ToString());return RedirectToAction("Index");}
[HttpPost][ValidateAntiForgeryToken]public async Task<IActionResult> Delete(int id){var uid=users.GetUserId(User);var item=await db.Leads.FirstOrDefaultAsync(x=>x.Id==id&&(Full||(Team && (x.OwnerId==uid || TeamIds.Contains(x.OwnerId)))||x.OwnerId==uid));if(item==null)return NotFound();db.Leads.Remove(item);await db.SaveChangesAsync();await audit.Write(User,"Delete","Lead",id.ToString());return RedirectToAction("Index");}
}