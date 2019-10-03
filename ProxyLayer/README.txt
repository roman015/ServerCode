ProxyLayer 2.0
---------------

USAGE
-----
dotnet run -- -c <PATH TO Server.config.json FILE>

FEATURES
--------
* Reroutes requests to apps listening to other ports using config file
* Basic blacklisting/whitelisting
* GUI to view the state of the application and change settings

DEPENDENCIES
------------
* McMaster.Extensions.CommandLineUtils
* Terminal.Gui
* Microsoft.AspNetCore.Proxy
* log4net