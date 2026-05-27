namespace Spectrum.API.Configuration
{
    public class DemoSeedOptions
    {
        public const string SectionName = "DemoSeed";

        public string MongoConnectionString { get; set; } = "mongodb://localhost:27017";
        public string SocialMongoConnectionString { get; set; } = "mongodb://localhost:27017/spectrum_social";
        public string DropsMongoConnectionString { get; set; } = "mongodb://localhost:27017/spectrum_drops";
        public string SocialDatabaseName { get; set; } = "spectrum_social";
        public string DropsDatabaseName { get; set; } = "spectrum_drops";
        public string DemoAdminPassword { get; set; } = "DemoAdmin123!";
        public string DemoPassword { get; set; } = "DemoPassword123!";
    }
}
