using Event.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Event.API
{
    public class ShiftDTO
    {
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Range(1, 50)]
        public int RequiredStaff { get; set; } = 1;

        public List<StaffAssignmentDTO> StaffAssignments { get; set; } = new();

        [StringLength(300)]
        public string? Description { get; set; }

        public ShiftStatusDTO Status { get; set; } = ShiftStatusDTO.Open;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ShiftStatusDTO
    {
        Open,
        Full,
        InProgress,
        Completed,
        Cancelled
    }
}
