using Staff.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Staff.API
{
    public class StaffDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        public StaffRole Role { get; set; }

        [StringLength(50)]
        public string? CertificationLevel { get; set; }

        public DateTime? CertificationExpiry { get; set; }

        public bool IsActive { get; set; } = true;

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StaffRole
    {
        FirstAider,
        TeamLeader,
        Paramedic,
        Doctor,
        Volunteer
    }

}
