# Smartsheet C# SDK v3.0.0

**Release Date:** March 14, 2026

This is a major release that modernizes the Smartsheet C# SDK for .NET 10, upgrades all dependencies to their latest versions, and includes significant security hardening. This release contains breaking changes — please review the migration guide before upgrading.

📄 **[Full Migration Guide →](MIGRATION-3.0.md)**

---

## Highlights

- 🎯 **Target .NET 10** — SDK now targets `net10.0` and `netstandard2.0`
- 🔒 **Security hardened** — OAuth credentials no longer leak in URLs or logs
- 📦 **All dependencies at latest** — zero known vulnerabilities
- ♻️ **`IDisposable` support** — deterministic resource cleanup with `using` blocks
- 🧹 **Legacy cleanup** — removed .NET Framework 4.5.2 targets and legacy project files

---

## ⚠️ Breaking Changes

| Change | Impact | Action Required |
|--------|--------|-----------------|
| Dropped `net452` target | Apps targeting .NET Framework 4.x | Retarget to `net10.0` or any `netstandard2.0`-compatible TFM |
| RestSharp 106.x → 114.0.0 | Apps extending `DefaultHttpClient` | Update constructor to use `RestClientOptions` with `RedirectOptions` |
| `FollowRedirects` → `RedirectOptions` | Proxy/redirect customization code | Replace `FollowRedirects = true` with `RedirectOptions = new RedirectOptions { FollowRedirects = true }` |
| `ServicePointManager.SecurityProtocol` removed | Apps relying on SDK to set TLS 1.2 | No action — .NET 10 enforces TLS 1.2+ by default |

> **Most consumers** using `SmartsheetBuilder` without custom HTTP clients need **zero code changes** — just retarget and update the NuGet package.

---

## Security Fixes

- **OAuth token request parameters** (`client_id`, `code`, `hash`, `refresh_token`) moved from URL query strings to POST body — prevents credential leakage in server/proxy logs
- **Response body logging** now redacts `access_token`, `refresh_token`, and `token` fields at DEBUG level
- **Default NLog configuration** raised from DEBUG/INFO to WARN — prevents sensitive data in logs out-of-the-box
- **Content-Disposition header injection** mitigated by sanitizing filenames across all attachment upload endpoints
- **Exception swallowing** narrowed from `catch (Exception)` to specific types in OAuth flows

---

## What's Changed

### Dependencies

| Package | Previous | v3.0.0 |
|---------|----------|--------|
| RestSharp | 106.6.9 | **114.0.0** |
| Newtonsoft.Json | 12.0.2 | **13.0.4** |
| NLog | 4.6.3 | **6.1.1** |

### API & Internal Improvements

- `SmartsheetImpl` now implements `IDisposable` for deterministic cleanup
- `ShouldRetry` no longer crashes with `NullReferenceException` on empty server responses
- Backoff jitter uses a static `Random` instance for better distribution
- `SHA256Managed` (obsolete) replaced with `SHA256.Create()`
- OS detection unified to `RuntimeInformation.OSDescription` (cross-platform)
- `System.Web.HttpUtility.UrlEncode` replaced with `System.Net.WebUtility.UrlEncode`
- `QueryUtil` Regex compilation optimized with static compiled instance

### Test & Build Infrastructure

| Package | Previous | v3.0.0 |
|---------|----------|--------|
| Microsoft.NET.Test.Sdk | 15.x | **18.3.0** |
| MSTest.TestAdapter | 1.4.0 | **4.1.0** |
| MSTest.TestFramework | 1.4.0 | **4.1.0** |
| coverlet.msbuild | 1.0.0 | **8.0.0** |

- All test/sample projects retargeted from `netcoreapp2.0` to `net10.0`
- MSTest 4.x migration: `[ExpectedException]` → `Assert.ThrowsExactly<T>()`, `Assert.Fail` format overloads → `string.Format`

### Removed

- .NET Framework 4.5.2 (`net452`) target and all associated projects
- Legacy `Smartsheet-csharp-sdk.csproj` (old-style project file)
- Sandcastle documentation project (`smartsheet-csharp-sdk-docs-v2`)
- `System.Management` assembly reference

---

## Quick-Start Upgrade

```bash
# 1. Ensure .NET 10 SDK is installed
dotnet --version  # should show 10.0.x

# 2. Update your project target
# <TargetFramework>net10.0</TargetFramework>

# 3. Update NuGet package
dotnet add package smartsheet-csharp-sdk --version 3.0.0

# 4. Build and test
dotnet build
```

For proxy or custom HTTP client users, see the [RestSharp migration section](MIGRATION-3.0.md#restsharp-upgrade-106x--11400) in the full migration guide.

---

## Full Changelog

See **[CHANGELOG.md](CHANGELOG.md)** for the complete list of changes, and **[MIGRATION-3.0.md](MIGRATION-3.0.md)** for a detailed migration guide with before/after code examples.
