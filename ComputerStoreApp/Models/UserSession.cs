namespace ComputerStoreApp.Models
{
    public static class UserSession
    {
        public static int? EmployeeId { get; set; }
        public static string FullName { get; set; }
        public static string Login { get; set; }
        public static string Role { get; set; }
        public static int? StoreId { get; set; }
        public static string StoreName { get; set; }

        public static bool IsAuthenticated => EmployeeId.HasValue;
        public static bool IsAdmin => Role == "Директор магазина" || Role == "Администратор";
        public static bool IsSeniorSeller => Role == "Старший продавец";

        public static void Clear()
        {
            EmployeeId = null;
            FullName = null;
            Login = null;
            Role = null;
            StoreId = null;
            StoreName = null;
        }
    }
}