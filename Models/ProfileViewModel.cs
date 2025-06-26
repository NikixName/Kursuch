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
        // Личные данные (имя, почта, телефон и т. д.)
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

        // Аватар
        public IFormFile? AvatarFile { get; set; }
        public string? AvatarPath { get; set; }

        // Роль текущего пользователя
        public string Role { get; set; } = null!;

        // КОМПОНЕНТ: Список заказов, выбранных по роли
        public List<OrderViewModel> Orders { get; set; } = new();
    }
}

