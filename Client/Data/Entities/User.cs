using System;
using System.ComponentModel.DataAnnotations;

namespace Programmin2_classroom.GoogleAuth.Data.Entities
{
    public class User
    {
        [Key]
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string refreshToken { get; set; }
        public DateTime? refershTokenExpiration { get; set; }
    }
}