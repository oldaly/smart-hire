namespace JobService
{
    public class Job
    {
#pragma warning disable IDE1006 // Naming Styles
        public string id { get; set; } = "";
#pragma warning restore IDE1006 // Naming Styles
        public string Title { get; set; } = "";
        public string Company { get; set; } = "";
        public string Location { get; set; } = "";
        public string Category { get; set; } = "";
    }
}