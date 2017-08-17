# qiwi-api-sharp
.NET Core compailable C# Wrapper for QIWI API

# supported platforms
Currently there are not so much supported platforms
* .NET Standard 1.1
* .NET Standard 1.3
* .NET Standard 1.6
* .NET Framework 4.6

More platforms will be supported soon.

# usage
1. Get token at https://qiwi.com/api
2. Call `QiwiApi.Initialize("*tokent*)` to initialize api.
3. Call `QiwiApi.UserProfile()` to get user profile data.
4. Call any other method.

# restrictions
There are 2 mehtods you can call without initializing api:
* QiwiApi.MobileProvider()
* QiwiApi.CardProvider()

Both of them used to identify phone or card number to use in payment methods.
Other methods must be used with proper specified _token_.

# issues
If you have some issues - feel free to create it in [https://github.com/blackboneworks/qiwi-api-sharp/issues](issued) page.

# contributing
1. Fork the _develop_ branch of https://github.com/blackboneworks/qiwi-api-sharp
2. Commit your changes in small, incremental steps with clear descriptions
3. When ready, issue a Pull Request (PR) against the _develop_ branch of https://github.com/blackboneworks/qiwi-api-sharp