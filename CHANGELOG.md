# Version 2.0.3 - .NET 8.0 LTS Upgrade & Comprehensive Security Audit

## 🎯 Framework & Dependencies
- **Major Upgrade**: Migrated from .NET 6.0 to .NET 8.0 LTS (latest long-term support version)
- **Updated all NuGet packages** to latest compatible versions:
  - Microsoft.Windows.Compatibility: 7.0.3 → 8.0.10
  - System.DirectoryServices: 7.0.1 → 8.0.1
  - System.Configuration.ConfigurationManager: 7.0.0 → 8.0.1
  - System.IO.FileSystem.AccessControl: 5.0.0 → 8.0.1
  - System.ComponentModel.Annotations: 5.0.0 → 8.0.0
  - Config.Net.Json: 4.15.0 → 5.2.0
  - ControlzEx: 5.0.2 → 6.0.1
  - LiteDB: 5.0.15 → 5.0.21
  - MahApps.Metro: 2.4.9 → 2.4.10
  - MahApps.Metro.IconPacks: 4.11.0 → 5.1.0
  - TaskScheduler: 2.10.1 → 2.11.1
  - Onova: 2.6.6 → 2.6.11
  - Newtonsoft.Json: 13.0.2 → 13.0.3
  - Microsoft.Bcl.AsyncInterfaces: 7.0.0 → 8.0.0
  - System.Text.Encodings.Web: 7.0.0 → 8.0.0

## 🔒 Critical Security Fixes
- **CRITICAL**: Replaced insecure DES encryption with Windows DPAPI (Data Protection API)
  - Removed all hardcoded encryption keys from source code
  - Fixed critical inconsistency: Encrypt() used DES while Decrypt() used AES (causing data loss!)
  - Now using ProtectedData API for secure, per-user encryption
  - Added legacy migration method for backwards compatibility with old encrypted passwords
  - Enhanced security with additional entropy layer

## 💪 Code Quality Improvements

### Exception Handling
- Replaced all empty catch blocks with comprehensive logging
- DirectoryHelper.cs: Added detailed error logging with specific exception types
- Helper.cs: Improved error handling in all user/domain enumeration methods
- UserListHelper.cs: Added comprehensive exception handling with domain controller checks
- MainWindow.xaml.cs: Enhanced error handling with proper cleanup

### Resource Management
- Fixed memory leaks by properly disposing IDisposable objects
- Helper.cs: Fixed PrincipalContext not being disposed in GetUsersSID()
- UserListHelper.cs: Added proper disposal of DirectoryEntry and Principal objects
- DirectoryHelper.cs: Ensured all resources are properly cleaned up

### Input Validation
- Added null-checks and validation to all public methods
- Added ArgumentNullException throws for invalid parameters
- Directory/File existence checks before all operations
- Enhanced validation in DirectoryHelper, FileHelper, and SecurityHelper

### Structured Logging
- Added informative Debug/Warning/Error messages throughout the application
- Better error context for troubleshooting
- Consistent logging patterns across all helper classes
- Added performance and diagnostic logging

## 🚀 Application Stability
- Replaced aggressive Process.Kill() with graceful Application.Shutdown()
- Added proper CancellationToken handling for background update checks
- Improved exception handling in UI event handlers
- Enhanced graceful shutdown process with proper cleanup

## 📝 Documentation
- Updated README.md with .NET 8.0 runtime requirements
- Updated download links for .NET 8.0 installers (x86, x64, ARM64)
- Enhanced code documentation with XML comments in SecurityHelper

## 🔧 Technical Improvements
- Removed duplicate user entries in GetAllUsers()
- Added null-safe property access throughout the codebase
- Improved Active Directory error handling (server down, not available, etc.)
- Enhanced domain controller connectivity checks

---

# Previous Releases

## Features
- Improvements to the stability of the speed
- Upgrade from .NET Framework 4.7 to .NET 6 (LTS)

## Bug Fixes
- Various stability improvements
