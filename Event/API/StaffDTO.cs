using Event.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Event.API
{
    public class StaffDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ReferenceId { get; set; } // Reference to Domain.Staff

        [Required]
        public StaffRoleDTO Role { get; set; }

        public bool IsActive { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StaffRoleDTO
    {
        FirstAider,
        TeamLeader,
        Paramedic,
        Doctor,
        Volunteer
    }
}
