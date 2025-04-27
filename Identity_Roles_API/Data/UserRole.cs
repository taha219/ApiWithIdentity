namespace Identity_Roles_API.Data
{
    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string User = "user";
        public const string Manager = "manager";

        public static List<string> GetRoles()
        {
            return new List<string> { Admin, Manager, User };
        }
    }
}
