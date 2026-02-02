using System;

namespace AirlineManagement
{
    public static class Session
    {
        public static string Username { get; set; }
        public static string Role { get; set; } = "Passenger"; // default role
        public static string PassengerId { get; set; }
        public static bool PresentationMode { get; set; } = false;
    }
}
