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
            public const string InvalidToken = "invalidToken";
            public const string EmailAlreadyRegistered = "emailAlreadyRegistered";
            public const string UserNotFound = "userNotFound";
            public const string InvalidCredentials = "invalidCredentials";
            public const string AuthProviderError = "authProviderError";
            public const string AccountSuspended = "accountSuspended";
            public const string UsernameAlreadyTaken = "usernameAlreadyTaken";
            public const string InvalidAdminKey = "invalidAdminKey";
            public const string Unauthorized = "unauthorized";
            public const string MissingRequiredParameter = "missingRequiredParameter";
            public const string InvalidParameterFormat = "invalidParameterFormat";
            public const string EmptyContent = "emptyContent";
            public const string ContentTooLong = "contentTooLong";
            public const string InvalidImageFormat = "invalidImageFormat";
            public const string ImageTooLarge = "imageTooLarge";
            public const string TokenExpired = "tokenExpired";
            public const string InsufficientPermissions = "insufficientPermissions";
            public const string SelfVoteNotAllowed = "selfVoteNotAllowed";
            public const string AdminSanctionForbidden = "adminSanctionForbidden";
            public const string ResourceNotFound = "resourceNotFound";
            public const string DuplicateParticipation = "duplicateParticipation";
            public const string EventKeysExhausted = "eventKeysExhausted";
            public const string InternalServerError = "internalServerError";
            public const string ExternalCatalogUnavailable = "externalCatalogUnavailable";
            public const string RpcServiceUnavailable = "rpcServiceUnavailable";
            public const string DatabaseTimeout = "databaseTimeout";
        }
    }
}
