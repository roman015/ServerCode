namespace DeployManager.Controllers
{
    public class PingPayload
    {
        public string zen { get; set; }
        public string hook_id { get; set; }
        public Hook hook { get; set; }
        public Repository repository { get; set; }
    }

    public class Hook
    {
        public string type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string active { get; set; }

    }
    public class Repository
    {
        public string id { get; set; }
        public string name { get; set; }
        public string full_name { get; set; }
        public string html_url{ get; set; }
        public string ssh_url { get; set; }
    }
}