namespace Cake.Apigee
{
    public interface IEnvironmentSettings : ICredentialSettings
    {
        string Environment { get; set; }
    }
}