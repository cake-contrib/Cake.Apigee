namespace Cake.Apigee
{
    public class BaseSettings : ICredentialSettings
    {
        public bool Debug { get; set; }

        public Credentials Credentials { get; set; }
    }
}
