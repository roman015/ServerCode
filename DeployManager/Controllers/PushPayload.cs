namespace DeployManager.Controllers
{
    public class PushPayload
    {
        public string head{get;set;}
        public string before { get; set; }
        public int size { get; set; }
        public int distinct_size { get; set; }
        public CommitPayload[] commits { get; set; }        
    }

    public class CommitPayload
    {
        public string sha { get; set; }
        public string message { get; set; }
        public AuthorPayload[] author { get; set; }
        public string url { get; set; }
        public bool distinct { get; set; }
    }

    public class AuthorPayload
    {
        public string name { get; set; }
        public string email { get; set; }
    }
}