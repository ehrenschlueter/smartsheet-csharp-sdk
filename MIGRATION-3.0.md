# Migration Guide: Upgrading from v2.126.0 to v3.0.0

This document covers all breaking changes introduced in the Smartsheet C# SDK v3.0.0 release and provides guidance on how to update your code.

## Table of Contents

- [Target Framework Changes](#target-framework-changes)
- [RestSharp Upgrade (106.x → 114.0.0)](#restsharp-upgrade-106x--11400)
- [TLS Configuration Removed](#tls-configuration-removed)
- [URL Encoding Change](#url-encoding-change)
- [Cryptography API Change](#cryptography-api-change)
- [OS Detection Change](#os-detection-change)
- [Dependency Version Summary](#dependency-version-summary)
- [Test Framework Changes](#test-framework-changes)
- [Removed Projects and Files](#removed-projects-and-files)

---

## Target Framework Changes

### What changed

| Property | v2.126.0 | v3.0.0 |
|---|---|---|
| Target Frameworks | `net452`, `netstandard2.0` | **`net10.0`**, `netstandard2.0` |
| Minimum .NET SDK | .NET Core 2.0 SDK | **.NET 10 SDK** |

### Impact

- **If your application targets .NET Framework 4.5.2 – 4.8:** You must retarget to .NET 10 (or another `netstandard2.0`-compatible runtime such as .NET 6+). The `net452` build of the SDK is no longer produced.
- **If your application targets .NET Standard 2.0 or .NET 6+:** No action required. The `netstandard2.0` build remains available and is fully compatible.

### Action required

Update your project's `<TargetFramework>` to `net10.0` or any framework compatible with `netstandard2.0`:

```xml
<!-- Before -->
<TargetFramework>net452</TargetFramework>

<!-- After (pick one) -->
<TargetFramework>net10.0</TargetFramework>
<!-- or -->
<TargetFramework>net8.0</TargetFramework>
```

---

## RestSharp Upgrade (106.x → 114.0.0)

This is the most significant breaking change. RestSharp underwent a complete API overhaul between versions 106 and 114.

### If you use `SmartsheetBuilder` without custom HTTP clients

**No code changes are required.** The SDK handles RestSharp internally. Your existing `SmartsheetBuilder` usage continues to work:

```csharp
// This still works exactly the same
SmartsheetClient client = new SmartsheetBuilder()
    .SetAccessToken("YOUR_TOKEN")
    .Build();
```

### If you extend `DefaultHttpClient` (e.g., for a proxy)

The `DefaultHttpClient` constructor and `RestClient` creation have changed.

**Before (v2.126.0):**

```csharp
using Smartsheet.Api.Internal.Http;
using RestSharp;
using System.Net;

class ProxyHttpClient : DefaultHttpClient
{
    public ProxyHttpClient(string host, int port)
        : base(new RestClient("https://api.smartsheet.com")
        {
            FollowRedirects = true,
            Proxy = new WebProxy(host, port)
        }, new Smartsheet.Api.Internal.Json.JsonNetSerializer())
    {
    }
}
```

**After (v3.0.0):**

```csharp
using Smartsheet.Api.Internal.Http;
using RestSharp;
using System.Net;

class ProxyHttpClient : DefaultHttpClient
{
    public ProxyHttpClient(string host, int port)
        : base(new RestClient(new RestClientOptions
        {
            RedirectOptions = new RedirectOptions { FollowRedirects = true },
            Proxy = new WebProxy(host, port)
        }), new Smartsheet.Api.Internal.Json.JsonNetSerializer())
    {
    }
}
```

**Key differences:**

| v2.126.0 (RestSharp 106.x) | v3.0.0 (RestSharp 114.x) |
|---|---|
| `new RestClient(baseUrl)` | `new RestClient(new RestClientOptions(...))` |
| `client.FollowRedirects = true` | `RestClientOptions.RedirectOptions = new RedirectOptions { FollowRedirects = true }` |
| `client.Proxy = proxy` | `RestClientOptions.Proxy = proxy` |
| `client.Execute(request)` | `client.ExecuteAsync(request).GetAwaiter().GetResult()` |
| `IRestResponse` | `RestResponse` |
| `Method.GET` | `Method.Get` |
| `Parameter.Type == ParameterType.RequestBody` | `parameter is BodyParameter` |

### If you override `CreateRestRequest`

The method signature is unchanged but the `RestRequest` API has minor differences:

```csharp
// Still works — signature unchanged
public override RestRequest CreateRestRequest(HttpRequest smartsheetRequest)
{
    RestRequest request = base.CreateRestRequest(smartsheetRequest);
    request.AddHeader("Custom-Header", "value");
    return request;
}
```

---

## TLS Configuration Removed

### What changed

The explicit `ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12` calls in `SmartsheetImpl` and `OAuthFlowImpl` have been removed.

### Why

.NET 6+ and .NET 10 enforce TLS 1.2 (and higher) by default at the runtime level. The `ServicePointManager` API is a legacy .NET Framework concept and is not needed in modern .NET.

### Action required

- **None** — if your application targets .NET 6+, TLS 1.2+ is the default.
- If you have your own `ServicePointManager.SecurityProtocol` calls in consuming code targeting .NET Framework via `netstandard2.0`, those calls remain valid but are no longer set by the SDK itself.

---

## URL Encoding Change

### What changed

`System.Web.HttpUtility.UrlEncode` was replaced with `System.Net.WebUtility.UrlEncode` in `QueryUtil`.

### Impact

This is an internal change with no public API impact. The encoding behavior is functionally equivalent for standard query parameters. The only subtle difference is in space encoding:

| Method | Space encoded as |
|---|---|
| `HttpUtility.UrlEncode` | `+` |
| `WebUtility.UrlEncode` | `%20` |

Both are valid per RFC 3986. The Smartsheet API accepts both forms.

### Action required

None — this is an internal implementation detail.

---

## Cryptography API Change

### What changed

`SHA256Managed` (obsolete in .NET 6+) was replaced with `SHA256.Create()` in `OAuthFlowImpl.getHash()`.

### Impact

No public API change. OAuth hash computation remains functionally identical.

### Action required

None — this is an internal implementation detail.

---

## OS Detection Change

### What changed

The `Util.GetOSFriendlyName()` method previously used Windows-only `System.Management` (WMI queries) on .NET Framework and `RuntimeInformation.OSDescription` on .NET Core. It now uses `RuntimeInformation.OSDescription` uniformly across all targets.

### Impact

- The User-Agent string sent to the Smartsheet API may report OS information in a slightly different format.
- The `System.Management` assembly reference has been removed from the project.

### Action required

None — this is an internal implementation detail. If you were relying on the exact User-Agent format, note that the OS description string may differ.

---

## Dependency Version Summary

| Package | v2.126.0 | v3.0.0 |
|---|---|---|
| RestSharp | 106.6.9 | **114.0.0** |
| Newtonsoft.Json | 12.0.2 | **13.0.4** |
| NLog | 4.6.3 | **6.1.1** |

### NLog 6.x notes

If you configure NLog in your application via `NLog.config` or programmatically, review the [NLog 5.x](https://nlog-project.org/2022/05/16/nlog-5-0-finally-here.html) and [NLog 6.x](https://nlog-project.org/2025/04/29/nlog-6-0-major-changes.html) migration guides for any breaking changes in your logging configuration.

### Newtonsoft.Json 13.x notes

Newtonsoft.Json 13.0.4 is backward-compatible with 12.x for standard usage. See the [Newtonsoft.Json release notes](https://github.com/JamesNK/Newtonsoft.Json/releases) for details.

---

## Test Framework Changes

These changes only affect developers building and testing the SDK itself, not consumers of the NuGet package.

| Package | v2.126.0 | v3.0.0 |
|---|---|---|
| Microsoft.NET.Test.Sdk | 15.x / 17.11.1 | **18.3.0** |
| MSTest.TestAdapter | 1.4.0 / 3.6.3 | **4.1.0** |
| MSTest.TestFramework | 1.4.0 / 3.6.3 | **4.1.0** |
| coverlet.msbuild | 1.0.0 / 6.0.2 | **8.0.0** |

### MSTest 4.x breaking changes applied

- `[ExpectedException]` attribute was removed in MSTest 4.x — replaced with `Assert.ThrowsExactly<T>()`
- `Assert.Fail(string, params object[])` overload was removed — replaced with `Assert.Fail(string.Format(...))`

---

## Removed Projects and Files

The following items were removed from the solution:

| Item | Reason |
|---|---|
| `Smartsheet-csharp-sdk.csproj` | Legacy .NET Framework project file (replaced by SDK-style `smartsheet-csharp-sdk-v2.csproj`) |
| `IntegrationTestSDK/net452/` | .NET Framework 4.5.2 test project (use `netcoreapp2.0` project retargeted to `net10.0`) |
| `TestSDKMockAPI/net452/` | .NET Framework 4.5.2 test project (use `netcoreapp2.0` project retargeted to `net10.0`) |
| `Sample/net452/` | .NET Framework 4.5.2 sample project (use `netcoreapp2.0` project retargeted to `net10.0`) |
| `documentation/smartsheet-csharp-sdk-docs-v2.shfbproj` | Sandcastle documentation project (requires external tooling not compatible with .NET 10) |

---

## Quick-Start Checklist

1. **Update your .NET SDK** to 10.0 or later (`dotnet --version` should show `10.0.x`)
2. **Retarget your project** to `net10.0` (or any `netstandard2.0`-compatible TFM)
3. **Update your NuGet reference** to `smartsheet-csharp-sdk` version `3.0.0`
4. **If you extend `DefaultHttpClient`:** update constructor calls to use `RestClientOptions` with `RedirectOptions` (see [RestSharp section](#restsharp-upgrade-106x--11400))
5. **Build and test** — no other code changes should be needed for typical SDK consumers
