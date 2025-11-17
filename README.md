![GitHub release (latest by date)](https://img.shields.io/github/v/release/HendrikKoelbel/RunAsAdmin)
![GitHub Releases](https://img.shields.io/github/downloads/HendrikKoelbel/RunAsAdmin/latest/total)

# Run as Admin

RunAsAdmin is a small program that allows administrators to start other programs with elevated privileges when logged in as a normal user.

## What's New in v2.1.2

This release includes important bug fixes and improvements for better reliability in non-Active Directory environments:

### Bug Fixes
- **Fixed Active Directory errors**: Properly handles `ActiveDirectoryOperationException` when the system is not joined to an AD domain
- **Fixed Single-File Publish issues**: Application now works correctly when published as a single executable file
- **Fixed Cryptographic errors**: Gracefully handles decryption failures when data was encrypted by a different user or machine
- **Fixed DriveInfo errors**: Correctly validates executable path before checking drive type

### Improvements
- **Enhanced User Discovery**:
  - Added cached AD user retrieval from local Windows profiles
  - Works even when AD connection is unavailable
  - Automatically falls back to cached users when AD is not accessible
- **Improved Domain Detection**:
  - Now includes `Environment.UserDomainName` for better domain discovery
  - Shows system domain even without AD forest connection
- **Better Error Handling**: All errors are now logged appropriately with graceful degradation instead of crashes

## Getting Started
You can easily download and run the program [here](https://github.com/HendrikKoelbel/RunAsAdmin/releases/latest)

### Prerequisites

- Supporting: .NET 8.0 LTS
- OS: Windows


| OS |  Installers
|----|:-----------:
| Windows: |  Arm64 | x64 | x86
| Links: |  [Arm64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0-windows-arm64-installer) | [x64](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0-windows-x64-installer) | [x86](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0-windows-x86-installer)


## Built With

* [UACHelper](https://github.com/falahati/UACHelper) - A helper class library to detect, manage and use UAC functionalities in your program
* [SimpleImpersonation](https://github.com/mj1856/SimpleImpersonation) - Simple Impersonation Library for .Net
* [Onova](https://github.com/Tyrrrz/Onova) - Unopinionated auto-update framework for desktop applications
* [Config.NET](https://github.com/aloneguid/config) - A comprehensive, easy to use and powerful .NET configuration library
* [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) - A toolkit for creating modern WPF applications
* [Resource-Embedder](https://github.com/MarcStan/resource-embedder) - Automatically embeds satellite assemblies into the main assembly
* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) - Popular high-performance JSON framework for .NET
* [Serilog](https://github.com/serilog/serilog) - Simple .NET logging with fully-structured events
* [LiteDB](https://github.com/mbdavid/litedb) - A .NET NoSQL Document Store in a single data file

## Versioning

For the versions available, see the [tags on this repository](https://github.com/HendrikKoelbel/RunAsAdmin/tags). 

## Authors

* **Hendrik Koelbel** - *Initial work* - [HendrikKoelbel](https://github.com/HendrikKoelbel)

See also the list of [contributors](https://github.com/HendrikKoelbel/RunAsAdmin/contributors) who participated in this project.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details
