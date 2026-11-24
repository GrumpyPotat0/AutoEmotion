# Maintenance audit and test plan

Audit date: 2026-11-23 (America/New_York). Maintainer: GrumpyPotat0.
Scope: maintenance branch in **GrumpyPotat0/AutoEmotion**, targeting that fork's
`main`. Original project: **FoxShadow10/AutoEmotion**. No upstream write is needed.

**Status: builds and isolated regression checks pass; no in-game testing has
been performed. Do not treat this branch as a validated release.**

## Baseline and compatibility findings

Baseline: `d33e18cdd977e4a2f8575f1f45490cac4c8e157f`.

| Component | Baseline | Maintenance target / finding |
| --- | --- | --- |
| Dalamud SDK | `Dalamud.NET.Sdk/15.0.0` | Retained; the source was already on API 15 |
| Framework | SDK-selected `net10.0-windows`, x64 | Retained; requires .NET 10 SDK, not just the runtime |
| Official release files tested | Not pinned by the old workflow; it downloaded staging | Release `15.0.4.1+d0e93484f99fdb7d58b3a2f22152cf108f3486ff` |
| ECommons | `8c79525937c02d205f321439036ae2edf63f1956`, 3.2.0.13 | Pin `b16d176ad689469afd5c3624d6179b243c70818f`, 3.2.1.23 |
| Build packages | DalamudPackager 15.0.0, DotNet.ReproducibleBuilds 1.2.39 | Retained and locked |
| ECommons analyzer | Microsoft.CodeAnalysis.BannedApiAnalyzers 3.12.0-beta1.25218.8 | Retained and locked; build-only dependency |
| Runtime references supplied by Dalamud | Lumina, Lumina.Excel, FFXIVClientStructs, ImGui bindings, Newtonsoft.Json, others | Build against release files; do not ship replacement framework DLLs |
| Plugin version | Project 2.1.4, historical repo entry 2.2.3 | Align generated assembly/manifest at 2.2.4.0 |

The official [versions page](https://dalamud.dev/versions/) reports release API 15;
the [API 15 notes](https://dalamud.dev/versions/v15/) describe the .NET SDK workflow.
The downloaded release DLL independently reported 15.0.4.1. These checks matter
because the documentation's channel table can lag or differ between channels.
Do not infer compatibility with a future API from `ApplicableVersion: any`.

Official download used: https://goatcorp.github.io/dalamud-distrib/latest.zip

SHA-256 of the audited archive:
`3F0D8B64D2ED7C868B57541C81F5308692EB69BB856517FA3D14955FD056E815`.
This records the tested snapshot; `latest.zip` can change. CI deliberately checks
the current release channel and rejects a different API major. Record its version
and hash on each run; a later run is a new compatibility check, not bit-for-bit reproduction.

The untouched baseline failed with five ECommons compilation errors involving
ambiguous `ExcelPage` types and consequent `IExcelRow` mismatches. The selected
[ECommons fix](https://github.com/NightmareXIV/ECommons/commit/b16d176ad689469afd5c3624d6179b243c70818f)
removes the conflicting import. This is an existing dependency revision, not a
local edit hidden inside the submodule. Advancing to it also includes intervening
ECommons changes (72 files); this is not a claim that every dependency line has
been audited. Chat and initialization paths were inspected and compiled.

The tested runtime distribution includes Lumina 7.7.0, Lumina.Excel 7.5.1,
Newtonsoft.Json 13.0.4, Serilog 4.3.0 and FFXIVClientStructs (assembly package
identity 1.0.0, which alone does not identify the game struct revision).

## Changes in this fork

- Preserve the original author, icon credit, LICENSE and contribution history;
  add a dated modification notice and copy AutoEmotion/ECommons license notices
  into build output. Point project/support metadata to this fork.
- Catch emote-hook creation/enable errors, dispose a partially created hook,
  and show an unavailable-reactions message in settings. Chat triggers remain usable.
- Catch managed reaction callback errors and forward the original native call
  exactly once through `finally`. This does not validate the native signature or ABI.
- Unsubscribe `OpenMainUi` during unload. Clear queues and reset both timers
  on logout, toggles, saving settings, manual clear and disposal. Recheck character
  availability before executing delayed commands; discard work when unavailable.
- Honor the configured loop-resume delay; prevent debug builds from reacting
  to the local player's own emotes. Fresh configurations start disabled;
  existing saved enable settings are preserved.
- Replace automatic release publishing with read-only PR/branch build checks.
  The old workflow used staging files, inconsistent output paths and release
  actions on ordinary pushes. The new workflow never releases, uploads binaries,
  modifies `main` or rewrites the repository manifest.
- Leave `repo.json` as an empty array until release gates pass. This prevents
  the fork from advertising upstream's old binaries as its own release.

## License and attribution audit

AutoEmotion's original project metadata explicitly declares
**AGPL-3.0-or-later**, and LICENSE contains the full AGPL v3 text. The original
author is FoxShadow10; this is a maintenance fork, not a claim of original authorship.
The license permits modification subject to its conditions. Keep notices and
the license, identify changes and dates, and offer the corresponding source
and build/install scripts for distributed binaries. See LICENSE sections 4–6
and 13; section 13 also addresses qualifying remote network interaction.

Dependency findings:

- ECommons's top-level license is MIT, copyright (c) 2023 NightmareXIV.
  `Hooks/ActionEffectTypes/ActionEffectType.cs` and `AttackType.cs` additionally
  state lmcintyre authorship and GPL v3 licensing. Do not describe the whole
  dependency as MIT-only. AGPL section 13 provides for combinations with GPL v3;
  retain the GPL notices and include its full license when distributing that code.
- `ECommons/Interop/OpenFileName.cs` contains a Microsoft Corporation, all-rights-
  reserved header without an accompanying permission statement in that file.
  Clarify its source and applicable permission, replace it through an appropriate
  upstream fix, or use a verified dependency build that omits it before publishing binaries.
- The updated `Automation/Chat.cs` cites ChatTwo; older ECommons revisions had an
  Anna Clemens MIT notice. Review that provenance and required notices before
  redistribution rather than assuming a top-level license resolves every file.
- The icon is credited to AlcielCroix. No separate artwork license was found;
  retain the credit and clarify redistribution permission before a public release.
- Restored package metadata lists DalamudPackager as EUPL-1.2 and both
  DotNet.ReproducibleBuilds and BannedApiAnalyzers as MIT. These are build tools,
  not intended runtime payload. Framework libraries remain supplied by Dalamud.

These are concrete release gates. This PR does not declare all dependency or
artwork provenance cleared. It provides source changes and CI checks, without
publishing a compiled plugin or adding a custom repository release entry.

## Reproduce the checks

Use a separate development folder, .NET 10 SDK and the current official API 15
development files. Do not overwrite a live Dalamud installation to build this.

```powershell
git clone --recurse-submodules https://github.com/GrumpyPotat0/AutoEmotion.git
cd AutoEmotion
git switch maintenance/dalamud-api15
git submodule update --init --recursive
# Extract official release latest.zip into a separate local folder first.
$dalamud = 'C:/dev/dalamud-api15/'
dotnet restore AutoEmotion.sln --locked-mode -p:DalamudLibPath=$dalamud -p:GeneratePackageOnBuild=false
dotnet build AutoEmotion.sln -c Release --no-restore -p:DalamudLibPath=$dalamud -p:GeneratePackageOnBuild=false
dotnet build AutoEmotion.sln -c Debug --no-restore -p:DalamudLibPath=$dalamud -p:GeneratePackageOnBuild=false
dotnet run --project tests/AutoEmotion.Regression -c Release
./scripts/Test-Package.ps1
dotnet list AutoEmotion.sln package --vulnerable --include-transitive
```

Keep the trailing slash in `DalamudLibPath`; pass this property to both restore
and build so the plugin and ECommons use the same files. Build logs identify the
resolved location. The local audit used .NET SDK 10.0.404 on Windows x64.

Verified locally: locked restore, Release and Debug builds, eight isolated hook
scenarios, generated API/version/fork/author/license checks, and whitespace checks.
Six existing ECommons warnings remain on a clean compilation (CS0612, CS9107,
CS1587); no AutoEmotion compiler warnings remain. The NuGet query reported no
known vulnerable restored packages from its configured source at audit time.
It does not scan native code, game hooks, dependency provenance or all runtime DLLs.

The test harness links the real `EmoteReaderHook.cs` against fake service boundaries.
It checks signature failure, enable failure/disposal, normal notification,
unrelated targets, unknown actors, throwing subscribers, logout and hook disposal.
It never loads a game or native hook. Queue timing and all UI/game behavior below
still require manual validation.

## Private install, test and rollback plan

1. Exit or unload the existing AutoEmotion. Back up
   `%APPDATA%/XIVLauncher/pluginConfigs/AutoEmotion.json` and the existing plugin
   directory, if present. Do not run two copies with `InternalName: AutoEmotion`.
   Existing configurations may already be enabled; inspect/disable them before testing.
2. Build this branch locally as above. Keep the whole
   `AutoEmotion/bin/x64/Release` folder together (at least AutoEmotion.dll,
   AutoEmotion.json, AutoEmotion.deps.json, ECommons.dll and the license notices).
   Do not copy the downloaded Dalamud distribution into the plugin folder.
3. Use Dalamud Settings → Experimental → Dev Plugin Locations to add the built
   plugin DLL/folder as described by the official
   [development instructions](https://dalamud.dev/faq/getting-started/).
   Verify your running Dalamud is API 15 before loading; never bypass API checks.
4. Load it with automation disabled. Confirm no errors, `/aemote` and `/ae` open
   settings, emote/expression lists render, and save/reopen retains settings.
   Test a new config as well as a backed-up existing config. A hook-unavailable
   message means emote reactions must remain untested/disabled until corrected.
5. In an appropriate quiet test area, configure one simple trigger in one
   intended channel. Enable chat reactions explicitly and verify matching,
   case/whole-word/regex behavior, delay, nonmatching messages and other senders.
   Check hidden chat-log mode separately. Remove the trigger after the test.
6. With a consenting tester, validate targeted emote reactions, whitelist,
   blacklist, Everyone fallback, range and cooldown. Confirm unrelated/self
   emotes do not trigger. Begin with one reaction, not a broad Everyone rule.
7. Queue a delayed reaction and then test `/ae clear`, `/aet`, `/aet c`, `/aet r`,
   settings save, combat, zoning and logout. No queued command should execute
   afterward or upon relogging. Test detarget/rotation restore only after basic
   reactions work. Verify a measurable configured loop-resume delay.
8. Unload/reload repeatedly; check for duplicate callbacks or commands and inspect
   Dalamud logs. Record game patch, Dalamud version, branch commit, config type,
   steps, expected/actual results and sanitized error logs. Do not publish private
   chat or character details unnecessarily.
9. On errors or unwanted actions, disable/unload the development plugin immediately.
   Remove its Dev Plugin Location. With the game closed, restore the backed-up
   config and old plugin folder if desired. Restart and confirm the test build
   is absent; keep backups until validation is complete.

Do not publish a release until provenance findings above are resolved, this
checklist passes on the current game patch, and the exact source (including the
ECommons commit), notices and build instructions accompany the binary. A future
release must update the fork's own download URLs and API/version manifest fields;
it must not silently reuse upstream's artifacts.

## Remaining runtime risks

The raw emote signature and its five-argument native calling convention have
not been validated in a running game. A successful signature match cannot prove
the ABI is correct, and managed exception handling cannot make an invalid native
pointer safe. FFXIVClientStructs reads, targeting, rotation changes and command
submission also need patch-specific testing. Regex parsing still uses unbounded
`Regex.IsMatch`; adversarial patterns or invalid saved expressions merit a future
bounded-parser change. Those are not covered by the hook regression harness.
