using AcxiomCRM.Models;
using AcxiomCRM.Data;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace AcxiomCRM.Controllers;
[Authorize(Roles="Admin")]public class UsersController(UserManager<AppUser> users,AuditService audit,AppDbContext db):Controller{
public IActionResult Index()=>View(users.Users.ToList());
[HttpPost][ValidateAntiForgeryToken]public async Task<IActionResult> SetRole(string id,string role){if(!new[]{"Admin","Manager","SalesExecutive"}.Contains(role))return BadRequest();var u=await users.FindByIdAsync(id);if(u==null)return NotFound();var old=await users.GetRolesAsync(u);if(old.Count==1 && old[0]==role)return RedirectToAction("Index");
await using var tx=await db.Database.BeginTransactionAsync();
try{
var removed=await users.RemoveFromRolesAsync(u,old);if(!removed.Succeeded)throw new InvalidOperationException("Unable to remove existing role.");
var added=await users.AddToRoleAsync(u,role);if(!added.Succeeded)throw new InvalidOperationException("Unable to assign requested role.");
var stamp=await users.UpdateSecurityStampAsync(u);if(!stamp.Succeeded)throw new InvalidOperationException("Unable to refresh session security.");
await audit.Write(User,"RoleChange","User",id,new { Roles=old },new { Role=role });await tx.CommitAsync();
}catch(Exception){await tx.RollbackAsync();return StatusCode(500,"Role change failed; no changes committed.");}
return RedirectToAction("Index");}
[HttpPost][ValidateAntiForgeryToken]public async Task<IActionResult> Toggle(string id){var u=await users.FindByIdAsync(id);if(u==null)return NotFound();if(users.GetUserId(User)==id)return BadRequest("Cannot deactivate self");u.LockoutEnabled=true;u.LockoutEnd=u.LockoutEnd>DateTimeOffset.UtcNow?null:DateTimeOffset.MaxValue;await users.UpdateAsync(u);await audit.Write(User,"AccountStatus","User",id);return RedirectToAction("Index");}}