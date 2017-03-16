## New in 0.6.0 (Released 16/3/2017)
- fixed bug where deploy result failed to deserialize if no other revisions existed.

## New in 0.5.0 (Released 01/03/2017)
- added CreateKeyValueMap, ListKeyValueMaps and DeleteKeyValueMap which support
  organisation and environment scope.

## New in 0.4.0 (Released 19/02/2017)
- added DeleteAllUndeployedApiProxyRevisions, GetApiProxy and DeleteApiProxyRevision
- improved error messages to include the URL and method of any failed calls

## New in 0.3.0 (Released 19/02/2017)
- improved error reporting
- return and list the packages restored by npm in Apigee

## New in 0.2.0 (Released 17/02/2017)
- Fix for nuget package folder to lib\net45

## New in 0.1.0 (Released 16/02/2017)
- Added Cake Aliases Apigee Methods for ImportProxy, DeployProxy and InstallNodePackagedModules
- Setup build via AppVeyor for automatic deployment to NuGet
- Limited Unit and Integration Tests added for code coverage