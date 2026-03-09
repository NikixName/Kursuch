using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System;

namespace Kurs_HTML.Models
{
    public class ServiceStatus
    {
        public string Title { get; set; } = null!;
        public string Date  { get; set; } = null!;
        public string State { get; set; } = null!;
    }


    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = null!;

        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        public string Car { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;

        public IFormFile? AvatarFile { get; set; }
        public string? AvatarPath { get; set; }

        public string Role { get; set; } = null!;

        public List<OrderViewModel> Orders { get; set; } = new();
    }
}

