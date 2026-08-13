namespace NexusIT.Data
{
    public static class AuthorizationRoles
    {
        public const string Administrator = "Administrator";
        public const string ITManager = "IT Manager";
        public const string ITTechnician = "IT Technician";
        public const string Employee = "Employee";

        public const string Management = Administrator + "," + ITManager;
        public const string Staff = Administrator + "," + ITManager + "," + ITTechnician;
    }
}
