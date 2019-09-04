using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Organization
{
    public class AttachDetachRightRequest
    {
        [Required]
        public Guid OrganizationId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Right { get; set; }
    }
}
