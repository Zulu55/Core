namespace Core4.Data.Entities
{
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;

    public class User : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "User")]
        public string FullName { get { return $"{this.FirstName} {this.LastName}"; } }

        [MaxLength(100)]
        public string Address { get; set; }

        public int CityId { get; set; }

        public City City { get; set; }
    }
}