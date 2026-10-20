using System.ComponentModel.DataAnnotations;
using CinemaGo.Domain.Models.Entities.Enums;

namespace CinemaGo.Domain.Models.DTOs
{
    public sealed record UserDto(int Id, string UserName, string Email, UserRole UserRole);

    public sealed class UserWriteDto
    {
        [Required, StringLength(60, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [EnumDataType(typeof(UserRole))]
        public UserRole UserRole { get; set; }
    }
}
