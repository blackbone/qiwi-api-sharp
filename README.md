# qiwi-api-sharp
.NET Core compatiable C# Wrapper for QIWI API

| **Build:** | **NuGet:** |
| ---------- | ---------- |
| [![Build status](https://ci.appveyor.com/api/projects/status/c7y65lkrf357s9aj/branch/master?svg=true)](https://ci.appveyor.com/project/blackboneworks/qiwi-api-sharp/branch/master) | [![NuGet](https://img.shields.io/nuget/dt/QiwiApiSharp.svg)](https://www.nuget.org/packages/QiwiApiSharp) [![NuGet](https://img.shields.io/nuget/dt/QiwiApiSharp.svg)](https://www.nuget.org/packages/QiwiApiSharp) |

# supported platforms
Currently there are not so much supported platforms
* .NET Standard 1.1
* .NET Standard 1.2
* .NET Standard 1.3
* .NET Standard 1.4
* .NET Standard 1.5
* .NET Standard 1.6
* .NET Standard 2.0
* .NET Framework 4.5
* .NET Framework 4.6
* .NET Framework 4.6.1
* .NET Framework 4.6.2
* .NET Framework 4.7

More platforms will be supported soon.

# usage
1. Get token at https://qiwi.com/api
2. Call `QiwiApi.Initialize("*tokent*)` to initialize api.
3. Call `QiwiApi.UserProfile()` to get user profile data.
4. Call any other method.

# restrictions
There are 2 mehtods you can call without initializing api:
* `QiwiApi.MobileProvider()`
* `QiwiApi.CardProvider()`

Both of them used to identify phone or card number to use in payment methods.
Other methods must be used with proper specified _token_.

# issues
If you have some issues - feel free to create it in [https://github.com/blackboneworks/qiwi-api-sharp/issues](issued) page.

# contributing
1. Fork the _develop_ branch of [https://github.com/blackboneworks/qiwi-api-sharp](repository)
2. Commit your changes in small, incremental steps with clear descriptions
3. When ready, issue a Pull Request (PR) against the _develop_ branch of [https://github.com/blackboneworks/qiwi-api-sharp](repository)
