using AcxiomCRM.Data;
using AcxiomCRM.Models;
using System.Security.Claims;
using System.Text.Json;
namespace AcxiomCRM.Services;
public class AuditService(AppDbContext db,IHttpContextAccessor context){public async Task Write(ClaimsPrincipal user,string action,string entity,string id,object? oldValue=null,object? newValue=null){var entry=new AuditLog{UserId=user.FindFirstValue(ClaimTypes.NameIdentifier)??"anonymous",Action=action,EntityName=entity,RecordId=id,OldValue=oldValue is null?"":JsonSerializer.Serialize(oldValue),NewValue=newValue is null?"":JsonSerializer.Serialize(newValue),IpAddress=context.HttpContext?.Connection.RemoteIpAddress?.ToString()??""};db.AuditLogs.Add(entry);await db.SaveChangesAsync();}}