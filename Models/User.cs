namespace UserManagementAPI.Models
{
    public class User
    {
        public int Id { get; set; }
      required  public string FirstName { get; set; }
      required  public string LastName { get; set; }
      required  public string Email { get; set; }
       required public string Password { get; set; }
    }
}
