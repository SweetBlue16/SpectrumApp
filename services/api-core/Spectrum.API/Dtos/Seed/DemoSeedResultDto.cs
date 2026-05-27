namespace Spectrum.API.Dtos.Seed
{
    public class DemoSeedResultDto
    {
        public int Admins { get; set; }
        public int Users { get; set; }
        public int Reviews { get; set; }
        public int Clips { get; set; }
        public int Comments { get; set; }
        public int Votes { get; set; }
        public int Reports { get; set; }
        public int DropEvents { get; set; }
        public int DropParticipants { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
