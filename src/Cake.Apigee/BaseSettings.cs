namespace Cake.Apigee
{
    public class BaseSettings : IBaseSettings
    {
        public bool Debug { get; set; }

        public Credentials Credentials { get; set; }
    }
}
