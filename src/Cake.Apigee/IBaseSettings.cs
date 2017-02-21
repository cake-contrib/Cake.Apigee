namespace Cake.Apigee
{
    public interface IBaseSettings
    {
        bool Debug { get; set; }

        Credentials Credentials { get; set; }
   }
}
