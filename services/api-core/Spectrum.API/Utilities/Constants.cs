namespace Spectrum.API.Utilities
{
    public static class Constants
    {
        public static class Roles
        {
            public const string Admin = "ADMIN";
            public const string Reviewer = "REVIEWER";
            public const string Reader = "READER";
        }

        public static class ErrorMessages
        {
            public const string InvalidToken = "The provided token is invalid or expired.";
            public const string EmailAlreadyRegistered = "The email address is already registered.";
            public const string UserNotFound = "No user found with the provided credentials.";
            public const string AuthProviderError = "An error occurred with the authentication provider.";
            public const string AccountSuspended = "This account is currently suspended.";
            public const string UsernameAlreadyTaken = "The username is already taken.";
            public const string InvalidAdminKey = "The provided admin secret key is invalid.";
            public const string Unauthorized = "Unauthorized access.";
        }
    }
}
