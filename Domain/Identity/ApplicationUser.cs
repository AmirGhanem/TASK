using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Identity;

public class ApplicationUser : IdentityUser
{

    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    [DataType(DataType.DateTime)]
    public DateTime CreatedDate { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;


    [NotMapped]
    public List<string> Roles { get; set; } = new List<string>() {  };


   
    // hide from serialization
    [JsonIgnore]
    public override string SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }
    [JsonIgnore]
    public override string NormalizedEmail { get => base.NormalizedEmail; set => base.NormalizedEmail = value; }
    [JsonIgnore]
    public override string? NormalizedUserName { get => base.NormalizedUserName; set => base.NormalizedUserName = value; }
    [JsonIgnore]
    public override string ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }
    [JsonIgnore]
    public override string PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }
    [JsonIgnore]
    public override bool TwoFactorEnabled { get => base.TwoFactorEnabled; set => base.TwoFactorEnabled = value; }
    [JsonIgnore]
    public override int AccessFailedCount { get => base.AccessFailedCount; set => base.AccessFailedCount = value; }
    
    
}


