namespace Cake.Apigee
{
    public interface ICredentialSettings
    {
        bool Debug { get; set; }

        Credentials Credentials { get; set; }
   }
}
