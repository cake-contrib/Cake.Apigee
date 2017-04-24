# Cake.Apigee

![https://ci.appveyor.com/api/projects/status/3118flw53uycwffg?svg=true](https://ci.appveyor.com/api/projects/status/3118flw53uycwffg?svg=true)

A Cake AddIn that extends Cake with [Apigee](https://apigee.com) management API commands.

[![cakebuild.net](https://img.shields.io/badge/WWW-cakebuild.net-blue.svg)](http://cakebuild.net/)

## Including addin

```
#addin "Cake.Apigee"
```

## Usage
The following shows a simple example of importing a proxy, other aliases are covered
in further sections.

```csharp
#addin "Cake.Apigee"

...

Task("DeployProxy")
    .Does(() =>
{    
    var apigeeCredentials = new Credentials
        {
            Username = "me@me.com",
            Password = "mypassword"
        };

    var result = ImportProxy(
                    "myorg", 
                    "myapi", 
                    File("apiproxy.zip"), 
                    new ImportProxySettings { Credentials = apigeeCredentials });
}
```

### InstallNodePackagedModules

This command effectively runs ```npm install``` on a proxy that contains a NodeJS application (e.g. an a127 project).
It can be useful to do this when the proxy exceeds the default 15Mb limit (you'll need to manually delete the node_modules folder prior
to importing the proxy). Further reading:

* Proxy size limit https://community.apigee.com/questions/10850/api-proxy-size-limitation.html
* Management API function http://docs.apigee.com/management/apis/post/organizations/%7Borg_name%7D/apis/%7Bapi_name%7D/revisions/%7Brevision_num%7D/npm-0

```csharp
InstallNodePackagedModules(
                "myorg", 
                result, 
                new InstallNodePackagedModulesSettings { Credentials = apigeeCredentials });
```

### DeployProxy

This command deploys an already imported proxy to a specified environment.
By default, the DeployProxySettings will use override=true and an allowance of
15 seconds for existing client calls to complete to minimise any downtime during the switchover.

Further reading:
* Seamless deployment http://docs.apigee.com/api-services/content/deploy-api-proxies-using-management-api#seamless

```csharp
DeployProxy(
        "myorg", 
        "dev", 
        result, 
        new DeployProxySettings { Credentials = apigeeCredentials });
```

### GetApiProxy

```csharp
var proxy = GetApiProxy(
        "myorg",             
        "my-proxy", 
        new GetApiProxySettings { Credentials = apigeeCredentials });
```

### DeleteApiProxyRevision

```csharp
DeleteApiProxyRevision(
        "myorg",             
        "my-proxy", 
        "123",
        new DeleteApiProxyRevisionSettings { Credentials = apigeeCredentials });
```

### DeleteAllUndeployedApiProxyRevisions

The underlying code attempts to delete all proxies and Apigee prevents
the deleting of currently deployed proxies.

```csharp
DeleteAllUndeployedApiProxyRevisions(
        "myorg",             
        "my-proxy",             
        new DeleteAllUndeployedApiProxyRevisionsSettings { Credentials = apigeeCredentials });
```

### CreateKeyValueMap

Create a key value map either at the organisation level or environment level (when 
an environment is specified in CreateKeyValueMapSettings).

Important: 

* see Permissions section below on special permissions required to modify Key Value Maps.
* apigee-access > 1.4.0 is required for KVM use from NodeJS https://community.apigee.com/questions/19816/accessing-kvm-from-node-js.html


```csharp
CreateKeyValueMap(
    "myorg",
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
```

### DeleteKeyValueMap

Specify the environment in DeleteKeyValueMapSettings if this is an environment specific key value map.

```csharp
DeleteKeyValueMap(
        "myorg",
        "myMap",
        new DeleteKeyValueMapSettings { 
            Credentials = apigeeCredentials, 
            Environment = "dev" });
```
            
### ListKeyValueMaps

```csharp
IEnumerable<string> mapNames = ListKeyValueMaps(
        "myorg",
        new ListKeyValueMapsSettings { 
            Credentials = apigeeCredentials, 
            Environment = "dev" });
```

# Notes
For more information on the behaviour of these commands, refer to the Apigee management API documentation at <http://docs.apigee.com/api-services/content/api-reference-getting-started>

# Debugging

There is a "Debug" flag on each of the Settings objects that will cause the Apigee response to be dumped to the console.

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

[![Follow @cybercohesion](https://img.shields.io/badge/Twitter-Follow%20%40cybercohesion-blue.svg)](https://twitter.com/intent/follow?screen_name=cybercohesion)
