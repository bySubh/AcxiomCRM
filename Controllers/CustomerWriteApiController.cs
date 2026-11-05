using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Controllers;
public record CustomerInput([Required,MaxLength(120)] string Name,[Required,EmailAddress] string Email,[Required,RegularExpression(@"^[0-9]{10}$")] string Phone,string? CompanyName,string? Address,string? City,string? State,string? Status);
public record LeadInput([Required] string Name,[EmailAddress] string? Email,[RegularExpression(@"^$|^[0-9]{10}$")] string? Phone,string? CompanyName,string? Source,[RegularExpression("New|Contacted|Qualified|Lost|Converted|Unqualified")] string? Status,[Range(0,1000000000)] decimal ExpectedValue);
public record OpportunityInput([Required] string Name,int CustomerId,int? LeadId,[Range(typeof(decimal),"0.01","100000000000")] decimal Amount,[Range(0,100)] int Probability,DateTime ExpectedCloseDate,[RegularExpression("Qualification|Proposal|Negotiation|Won|Lost")] string Stage);
[ApiController,Authorize,Route("api")]
public class CustomerWriteApiController(AppDbContext db,UserManager<AppUser> users,AuditService audit):ControllerBase {
private string Uid=>users.GetUserId(User)??"";
private bool Admin=>User.IsInRole("Admin");
private bool Manager=>User.IsInRole("Manager");
private IQueryable<string> Team=>users.Users.Where(u=>u.ManagerId==Uid).Select(u=>u.Id);
private IQueryable<Customer> Visible=>db.Customers.Where(x=>Admin||x.OwnerId==Uid||(Manager&&Team.Contains(x.OwnerId)));
[HttpPost("customers")]
public async Task<IActionResult> CreateCustomer(CustomerInput input){if(await db.Customers.AnyAsync(x=>x.Email==input.Email||x.Phone==input.Phone))return Conflict(new {error="Customer email or phone already exists"});var x=new Customer{Name=input.Name,Email=input.Email,Phone=input.Phone,CompanyName=input.CompanyName??"",Address=input.Address??"",City=input.City??"",State=input.State??"",Status=input.Status??"Active",OwnerId=Uid,CustomerCode="CUS-"+Guid.NewGuid().ToString("N")[..10]};db.Customers.Add(x);await db.SaveChangesAsync();await audit.Write(User,"Create","Customer",x.Id.ToString());return Created($"/api/customers/{x.Id}",new{x.Id,x.Name,x.Email,x.Phone,x.CompanyName,x.Status});}
[HttpPut("customers/{id:int}")]
public async Task<IActionResult> UpdateCustomer(int id,CustomerInput input){var x=await Visible.FirstOrDefaultAsync(x=>x.Id==id);if(x==null)return NotFound(new{error="Customer not found"});if(await db.Customers.AnyAsync(c=>c.Id!=id&&(c.Email==input.Email||c.Phone==input.Phone)))return Conflict(new{error="Customer email or phone already exists"});x.Name=input.Name;x.Email=input.Email;x.Phone=input.Phone;x.CompanyName=input.CompanyName??"";x.Address=input.Address??"";x.City=input.City??"";x.State=input.State??"";x.Status=input.Status??"Active";await db.SaveChangesAsync();await audit.Write(User,"Update","Customer",id.ToString());return Ok(new{x.Id,x.Name,x.Email,x.Phone,x.CompanyName,x.Status});}
[HttpDelete("customers/{id:int}")]
public async Task<IActionResult> DeleteCustomer(int id){var x=await Visible.FirstOrDefaultAsync(x=>x.Id==id);if(x==null)return NotFound(new{error="Customer not found"});db.Customers.Remove(x);try{await db.SaveChangesAsync();}catch(DbUpdateException){return Conflict(new{error="Customer has related records"});}await audit.Write(User,"Delete","Customer",id.ToString());return NoContent();}
[HttpPost("leads")]
public async Task<IActionResult> CreateLead(LeadInput input){var x=new Lead{Name=input.Name,Email=input.Email??"",Phone=input.Phone??"",CompanyName=input.CompanyName??"",Source=input.Source??"Website",Status=input.Status??"New",ExpectedValue=input.ExpectedValue,OwnerId=Uid,LeadCode="LEAD-"+Guid.NewGuid().ToString("N")[..10]};db.Leads.Add(x);await db.SaveChangesAsync();await audit.Write(User,"Create","Lead",x.Id.ToString());return Created($"/api/leads/{x.Id}",new{x.Id,x.Name,x.Status,x.ExpectedValue});}
[HttpPost("opportunities")]
public async Task<IActionResult> CreateOpportunity(OpportunityInput input){if(input.Stage!="Won"&&input.Stage!="Lost"&&input.ExpectedCloseDate.Date<DateTime.UtcNow.Date)return BadRequest(new{error="Active opportunity close date cannot be in the past"});if(!await Visible.AnyAsync(x=>x.Id==input.CustomerId))return BadRequest(new{error="Customer is not accessible"});if(input.LeadId.HasValue&&!await db.Leads.AnyAsync(x=>x.Id==input.LeadId&&(Admin||x.OwnerId==Uid||(Manager&&Team.Contains(x.OwnerId)))))return BadRequest(new{error="Lead is not accessible"});var x=new Opportunity{Name=input.Name,CustomerId=input.CustomerId,LeadId=input.LeadId,Amount=input.Amount,Probability=input.Probability,ExpectedCloseDate=input.ExpectedCloseDate,Stage=input.Stage,OwnerId=Uid,Status=input.Stage is "Won" or "Lost"?"Closed":"Open"};db.Opportunities.Add(x);await db.SaveChangesAsync();await audit.Write(User,"Create","Opportunity",x.Id.ToString());return Created($"/api/opportunities/{x.Id}",new{x.Id,x.Name,x.Amount,x.Probability,x.Stage});}
}
