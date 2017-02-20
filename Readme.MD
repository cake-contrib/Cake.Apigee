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
}

```

# General Notes
**This is an initial version and not tested thoroughly**. Currently only deals with proxy 
deployments.

Tested only on Windows at this time. 

# Debugging

There is a "Debug" flag on each of the Settings objects that will cause the Apigee response to be dumped to the console.

[![Follow @cybercohesion](https://img.shields.io/badge/Twitter-Follow%20%40cybercohesion-blue.svg)](https://twitter.com/intent/follow?screen_name=cybercohesion)