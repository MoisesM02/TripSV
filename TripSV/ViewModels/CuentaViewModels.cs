using System.ComponentModel.DataAnnotations;

namespace TripSV.ViewModels
{
    public class IniciarSesionViewModel
    {
        [Required(ErrorMessage = "Ingrese su usuario")]
        [StringLength(24, MinimumLength = 4, ErrorMessage = "El usuario debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su contraseña")]
        [StringLength(35, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }

        public string? UrlRetorno { get; set; }
    }

    public class RegistroViewModel
    {
        [Required(ErrorMessage = "Ingrese un usuario")]
        [StringLength(24, MinimumLength = 4, ErrorMessage = "El usuario debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese su correo")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "El correo debe tener entre {2} y {1} caracteres")]
        [Display(Name = "Correo")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese una contraseña")]
        [StringLength(35, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme su contraseña")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        public string Password2 { get; set; } = string.Empty;
    }

    public class RecuperarPasswordViewModel
    {
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        [Display(Name = "Correo")]
        public string? Correo { get; set; }

        [Display(Name = "Usuario")]
        public string? Usuario { get; set; }
    }

    public class RestablecerPasswordViewModel
    {
        [Required]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingrese la nueva contraseña")]
        [StringLength(35, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre {2} y {1} caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Repita la nueva contraseña")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Repetir contraseña")]
        public string Password2 { get; set; } = string.Empty;
    }
}
