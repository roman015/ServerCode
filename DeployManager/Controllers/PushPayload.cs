namespace DeployManager.Controllers
{
    public class PushPayload
    {
        public string before { get; set; }
        public string after { get; set; }
        public Repository repository { get; set; }
    }
}