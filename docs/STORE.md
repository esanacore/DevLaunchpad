# Microsoft Store submission guide

Dev Launchpad ships as a single-project MSIX and is distributed as a **Command
Palette extension** for Microsoft PowerToys. This document is the checklist for
getting a build from this repo into the Microsoft Store.

## 1. Reserve the app in Partner Center

1. Sign in to [Partner Center](https://partner.microsoft.com/dashboard) with a
   registered developer account.
2. **Apps and games → New product → MSIX or PWA app**.
3. Reserve the name (e.g. `Dev Launchpad`).
4. Open **Product identity** and note the three identity values Partner Center
   assigns:
   - **Package/Identity/Name** (e.g. `12345EricSanacore.DevLaunchpad`)
   - **Package/Identity/Publisher** (e.g. `CN=ABCDEF12-3456-...`)
   - **Package/Properties/PublisherDisplayName** (e.g. `Eric Sanacore`)

## 2. Stamp the real identity into the manifest

`DevLaunchpad/Package.appxmanifest` ships with **placeholder** identity values so
the package can be built and sideloaded from this repo. Before submitting, replace
them with the Partner Center values from step 1.

The easiest path is Visual Studio: right-click the project →
**Publish → Associate App with the Store**, sign in, pick the reserved name, and
VS rewrites `Identity/Name`, `Identity/Publisher`, and `PublisherDisplayName`
automatically. Do **not** commit those real values if the repo is public — they
tie the package to your Store account.

| Manifest field | Placeholder in repo | Replace with |
|---|---|---|
| `Identity/@Name` | `DevLaunchpad` | Partner Center Identity Name |
| `Identity/@Publisher` | `CN=Eric Sanacore` | Partner Center Publisher (`CN=...`) |
| `Properties/PublisherDisplayName` | `Eric Sanacore` | Partner Center PublisherDisplayName |

## 3. Produce the packages

CI builds unsigned `.msix` packages for **x64** and **arm64** on every push/PR via
[`.github/workflows/msix.yml`](../.github/workflows/msix.yml); download them from
the run's **Artifacts**. To build locally instead:

```powershell
msbuild DevLaunchpad/DevLaunchpad.csproj /t:Restore /p:Configuration=Release /p:Platform=x64
msbuild DevLaunchpad/DevLaunchpad.csproj `
  /p:Configuration=Release /p:Platform=x64 `
  /p:GenerateAppxPackageOnBuild=true `
  /p:UapAppxPackageBuildMode=SideloadOnly `
  /p:AppxBundle=Never `
  /p:AppxPackageSigningEnabled=false `
  /p:AppxPackageDir=AppPackages\
```

Repeat with `Platform=arm64` for the Arm64 package. The Store re-signs uploads with
your publisher certificate, so these packages do **not** need to be signed for
submission. (For local sideload testing you must sign with your own certificate
and trust it.)

## 4. Submit

1. In Partner Center, create a submission and upload the **x64** and **arm64**
   `.msix` files (the Store accepts multiple architectures in one submission).
2. Fill in **Store listing**: description, category (*Developer tools*), a
   **support contact** (email or the GitHub issues URL), and the **privacy policy
   URL** (host [`PRIVACY.md`](PRIVACY.md), e.g. via GitHub Pages, and link it).
3. Add **screenshots** captured per [`images/SCREENSHOTS.md`](images/SCREENSHOTS.md)
   (at least one; PNG, 1366×768–3840×2160).
4. **Properties → Capabilities**: only `runFullTrust` is declared; expect the
   restricted-capability review note for `runFullTrust` and explain it is required
   for the out-of-process COM server that PowerToys activates.
5. Submit for certification.

## 5. Validate before you submit (recommended)

The unit test suite includes `StoreReadinessCheckerTests`, which verifies the repo
still contains the package manifest, Store guide, privacy policy, x64/arm64
publish profiles, required manifest identity fields, and the `runFullTrust`
capability needed by the Command Palette COM server:

```powershell
dotnet test DevLaunchpad.Tests/DevLaunchpad.Tests.csproj --filter "FullyQualifiedName~StoreReadinessCheckerTests" --nologo -v q
```

Run the **Windows App Certification Kit** (WACK) locally against a built package:

```powershell
& "${env:ProgramFiles(x86)}\Windows Kits\10\App Certification Kit\appcert.exe" `
  test -appxpackagepath <path-to>.msix -reportoutputpath wack-report.xml
```

Fix any failures before uploading; the Store runs the same checks during
certification.
