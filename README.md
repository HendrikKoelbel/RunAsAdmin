![GitHub release (latest by date)](https://img.shields.io/github/v/release/HendrikKoelbel/RunAsAdmin)
![GitHub Releases](https://img.shields.io/github/downloads/HendrikKoelbel/RunAsAdmin/latest/total)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows-blue)
![License](https://img.shields.io/github/license/HendrikKoelbel/RunAsAdmin)

# RunAsAdmin

A powerful Windows utility that enables administrators to start applications with elevated privileges while logged in as a standard user. Perfect for IT professionals managing workstations in enterprise environments.

## Features

- **🔐 Secure Credential Storage**: Passwords are encrypted using Windows DPAPI (Data Protection API)
- **👥 User Management**:
  - Supports both local and Active Directory users
  - Automatic discovery of cached AD users from local profiles
  - Works seamlessly in non-AD environments
- **🌐 Multi-Domain Support**: Automatically detects and lists available domains
- **🚀 UAC Integration**: Leverages Windows UAC for secure privilege elevation
- **📝 Comprehensive Logging**: Detailed logging with Serilog and LiteDB for troubleshooting
- **🎨 Modern UI**: Built with MahApps.Metro for a sleek, modern interface
- **🔄 Auto-Update**: Automatic update checks via GitHub releases
- **🖥️ Single-File Deployment**: Optimized for single-file publishing

## What's New in v2.1.3

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
  - Reads user profiles from Windows registry for comprehensive user list
- **Improved Domain Detection**:
  - Now includes `Environment.UserDomainName` for better domain discovery
  - Shows system domain even without AD forest connection
  - Prevents duplicate entries in domain list
- **Better Error Handling**: All errors are now logged appropriately with graceful degradation instead of crashes
- **Improved Logging**: More detailed debug information for troubleshooting

## Getting Started
You can easily download and run the program [here](https://github.com/HendrikKoelbel/RunAsAdmin/releases/latest)

### Prerequisites

**Required:**
- .NET 8.0 Desktop Runtime (LTS)
- Windows Operating System

**Download .NET 8.0 Runtime:**

| Architecture | Download Link |
|--------------|---------------|
| **x64** (most common) | [Download](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0-windows-x64-installer) |
| **x86** (32-bit) | [Download](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0-windows-x86-installer) |
| **ARM64** | [Download](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0-windows-arm64-installer) |

### Installation

1. Download the latest release from [Releases](https://github.com/HendrikKoelbel/RunAsAdmin/releases/latest)
2. Extract the ZIP file to your desired location
3. Run `RunAsAdmin.exe`

**Note:** The application must be run from a local drive (not from a network share).

## How It Works

1. **Launch the Application**: Start RunAsAdmin.exe with administrator privileges
2. **Select Credentials**: Choose or enter domain/username and password
3. **Select Program**: Click "Start Program with Admin Rights" and select the executable
4. **Automatic Elevation**: The selected program runs with elevated privileges using the specified credentials

## Built With

### Core Dependencies

* **[UACHelper](https://github.com/falahati/UACHelper)** - UAC (User Account Control) management and detection
* **[SimpleImpersonation](https://github.com/mj1856/SimpleImpersonation)** - User impersonation for .NET applications
* **[Onova](https://github.com/Tyrrrz/Onova)** - Auto-update framework with GitHub integration
* **[Config.Net.Json](https://github.com/aloneguid/config)** - Configuration management library
* **[LiteDB](https://github.com/mbdavid/litedb)** - NoSQL document database for logging storage
* **[Serilog.Sinks.LiteDB](https://github.com/serilog/serilog)** - Structured logging with LiteDB sink

### UI & Presentation

* **[MahApps.Metro](https://github.com/MahApps/MahApps.Metro)** - Modern WPF UI framework
* **[MahApps.Metro.IconPacks](https://github.com/MahApps/MahApps.Metro.IconPacks)** - Icon library for Metro-styled applications
* **[ControlzEx](https://github.com/ControlzEx/ControlzEx)** - Shared controls for MahApps.Metro

### Utilities

* **[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)** - High-performance JSON framework
* **[Resource.Embedder](https://github.com/MarcStan/resource-embedder)** - Embeds satellite assemblies into main executable
* **[TaskScheduler](https://github.com/dahall/TaskScheduler)** - Windows Task Scheduler wrapper
* **[Trinet.Core.IO.Ntfs](https://github.com/RichardD2/NTFS-Streams)** - NTFS Alternate Data Streams support
* **[Castle.Core](https://github.com/castleproject/Core)** - Core library for dynamic proxy generation

### Windows Integration

* **System.DirectoryServices** - Active Directory integration
* **System.DirectoryServices.AccountManagement** - User and group management
* **System.Configuration.ConfigurationManager** - Configuration file management
* **System.IO.FileSystem.AccessControl** - File system security management

## Versioning

For the versions available, see the [tags on this repository](https://github.com/HendrikKoelbel/RunAsAdmin/tags). 

## Authors

* **Hendrik Koelbel** - *Initial work* - [HendrikKoelbel](https://github.com/HendrikKoelbel)

See also the list of [contributors](https://github.com/HendrikKoelbel/RunAsAdmin/contributors) who participated in this project.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details
