# Change Log

## Current Version

v1.1.1

- Test dependency updates; no changes to the library code or public API, and the package has no new runtime dependencies
  - Touchstone.Core, Touchstone.Cli, Touchstone.XunitAdapter, Touchstone.NunitAdapter 0.1.12 -> 0.2.0
  - NUnit 4.3.2 -> 5.0.0, NUnit.Analyzers 4.7.0 -> 4.15.0, NUnit3TestAdapter 5.0.0 -> 6.3.0
  - Microsoft.NET.Test.Sdk 17.14.1 -> 18.10.1, coverlet.collector 6.0.4 -> 10.1.0, xunit.runner.visualstudio 3.1.4 -> 4.0.0

## Previous Versions

v1.1.0

- Behavior change: `ValueExists` now compares values with `Object.Equals` instead of reference equality, so equal strings and boxed value types (for example `ValueExists(42)`) are found
- Behavior change: `Get` now returns a snapshot copy of the entries.  Changes to the returned dictionary no longer affect the matcher, and the snapshot is safe to enumerate while other threads modify the matcher.  Code that changed the matcher through `Get()` must use `Add` and `Remove` instead
- Fix: entries are always evaluated in insertion order.  Previously, an entry added after a `Remove` could take the removed entry's place and be evaluated earlier than intended with `MatchPreference.First` and in `AllMatches`
- Duplicate `Add` of the same `Regex` instance still throws `ArgumentException`, now with a message identifying the pattern
- Documentation: `LongestFirst` and `ShortestFirst` compare regex pattern length, not matched text length; ties go to the entry added first
- XML documentation for exceptions and thread safety
- Retarget to net8.0 and net10.0, dropping net6.0 and net7.0
- Test infrastructure migrated to Touchstone (Test.Shared, Test.Automated, Test.Xunit, Test.Nunit) with expanded positive and negative coverage

v1.0.x

- ```MatchPreference``` property to specify how to handle multiple match scenarios: first match, longest match, or shortest match
- ```AllMatches``` API, thank you @sapurtcomputer30
- XML documentation
- Retarget to support both .NET Core 2.0 and .NET Framework 4.6.2
- Retarget to remove .NET 5.0, add .NET 7.0 and .NET Framework 4.8
- Initial release
