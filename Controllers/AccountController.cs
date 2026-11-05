using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace AcxiomCRM.Controllers;
public class AccountController(SignInManager<AppUser> signIn,UserManager<AppUser> users,AuditService audit):Controller{
public record LoginInput([Required] string Email,[Required] string Password);
public record RegisterInput([Required] string Name,[Required,EmailAddress] string Email,[Required] string Password);
[AllowAnonymous][HttpGet]public IActionResult Login()=>View();
[AllowAnonymous][Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")][HttpPost][ValidateAntiForgeryToken]public async Task<IActionResult> Login(LoginInput input){if(!ModelState.IsValid)return View();var user=await users.FindByEmailAsync(input.Email);var result=user is null?null:await signIn.PasswordSignInAsync(user,input.Password,false,true);if(result?.Succeeded==true){await audit.Write(User,"LoginSuccess","User",user!.Id,new { UserId=user.Id });return RedirectToAction("Index","Home");}await audit.Write(User,result?.IsLockedOut==true?"Lockout":"LoginFailure","User",user?.Id??"unknown");ModelState.AddModelError("","Invalid credentials or account locked.");return View();}
[AllowAnonymous][HttpGet]public IActionResult Register()=>View();
[AllowAnonymous][Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")][HttpPost][ValidateAntiForgeryToken]public async Task<IActionResult> Register(RegisterInput input){if(!ModelState.IsValid)return View();var user=new AppUser{UserName=input.Email,Email=input.Email,DisplayName=input.Name};var result=await users.CreateAsync(user,input.Password);if(!result.Succeeded){foreach(var error in result.Errors)ModelState.AddModelError("",error.Description);return View();}var roleResult=await users.AddToRoleAsync(user,"SalesExecutive");if(!roleResult.Succeeded){foreach(var error in roleResult.Errors)ModelState.AddModelError("",error.Description);return View();}await audit.Write(User,"Register","User",user.Id);await signIn.SignInAsync(user,false);return RedirectToAction("Index","Home");}
[Authorize][HttpPost][ValidateAntiForgeryToken]public async Task<IActionResult> Logout(){await audit.Write(User,"Logout","User",users.GetUserId(User)??"");await signIn.SignOutAsync();return RedirectToAction("Login");}
public IActionResult Denied()=>View();}