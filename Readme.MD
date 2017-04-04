# Cake.Apigee

![https://ci.appveyor.com/api/projects/status/3118flw53uycwffg?svg=true](https://ci.appveyor.com/api/projects/status/3118flw53uycwffg?svg=true)

A Cake AddIn that extends Cake with [Apigee](https://apigee.com) management API commands.

[![cakebuild.net](https://img.shields.io/badge/WWW-cakebuild.net-blue.svg)](http://cakebuild.net/)

## Including addin

```
#addin "Cake.Apigee"
```

## Usage

```csharp
#addin "Cake.Apigee"

...

Task("DeployProxy")
    .Does(() =>
{
    var orgName = "myorg";
    var apigeeCredentials = new Credentials
        {
            Username = "me@me.com",
            Password = "mypassword"
        };

    var result = ImportProxy(
                    orgName, 
                    "myapi", 
                    File("apiproxy.zip"), 
                    new ImportProxySettings { Credentials = apigeeCredentials });

    InstallNodePackagedModules(
                    orgName, 
                    result, 
                    new InstallNodePackagedModulesSettings { Credentials = apigeeCredentials });

    DeployProxy(
            orgName, 
            "dev", 
            result, 
            new DeployProxySettings { Credentials = apigeeCredentials });

    var proxy = GetApiProxy(
            orgName,             
            "my-proxy", 
            new GetApiProxySettings { Credentials = apigeeCredentials });

    DeleteApiProxyRevision(
            orgName,             
            "my-proxy", 
            "123",
            new DeleteApiProxyRevisionSettings { Credentials = apigeeCredentials });

    DeleteAllUndeployedApiProxyRevisions(
            orgName,             
            "my-proxy",             
            new DeleteAllUndeployedApiProxyRevisionsSettings { Credentials = apigeeCredentials });

    CreateKeyValueMap(
            orgName,
            new KeyValueMap
            {
                Name = "myMap",
                Encrypted = false,
                Entry = new[]
                {
                    new KeyValueMapEntry { Name = "myKey", Value = "myValue" }
                }
            },
            new CreateKeyValueMapSettings { 
              Credentials = apigeeCredentials, 
              Environment = "dev" });

    DeleteKeyValueMap(
            orgName,
            "myMap",
            new DeleteKeyValueMapSettings { 
              Credentials = apigeeCredentials, 
              Environment = "dev" });
            
    IEnumerable<string> mapNames = ListKeyValueMaps(
            orgName,
            new ListKeyValueMapsSettings { 
               Credentials = apigeeCredentials, 
               Environment = "dev" });
}

```
# Notes
For more information on the behaviour of these commands, refer to the Apigee management API documentation at <http://docs.apigee.com/api-services/content/api-reference-getting-started>

# Debugging

There is a "Debug" flag on each of the Settings objects that will cause the Apigee response to be dumped to the console.

[![Follow @cybercohesion](https://img.shields.io/badge/Twitter-Follow%20%40cybercohesion-blue.svg)](https://twitter.com/intent/follow?screen_name=cybercohesion)

# Permissions
## Key Value Maps
Apigee requires additional permissions to manage Key Value Maps that are not settable in their UI. These permissions can be added to a role
by using http://docs.apigee.com/management/apis/post/organizations/%7Borg_name%7D/userroles/%7Brole_name%7D/resourcepermissions

Post the following JSON to https://api.enterprise.apigee.com/v1/organizations/{org}/userroles/{yourrole}/resourcepermissions:

```json
{
 "resourcePermission" : [ 
   {
    "path" : "/keyvaluemaps",
    "permissions" : [ "get","put","delete" ]
   }, 
   {
    "path" : "/environments/*/keyvaluemaps",
    "permissions" : [ "get","put","delete" ]
   }
  ]
}
```

# Future
## Removal of node_modules
An option to remove the node_modules from the zip prior to uploading the proxy. This is to mitigate exceeding the 15Mb proxy limit. Currently this must be done using standard Cake Zip functions and the above *InstallNodePackagedModules* run once the proxy has imported.  