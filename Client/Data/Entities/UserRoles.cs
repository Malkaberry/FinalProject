using System.ComponentModel.DataAnnotations;

namespace Programmin2_classroom.Client.Data.Entities
{
    public class UserRoles
    {
        [Key]
        public int roleID { get; set; }
        public int userID { get; set; }
        public string name { get; set; }
    }
}
