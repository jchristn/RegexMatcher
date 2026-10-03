# RegexMatcher

[![NuGet Version](https://img.shields.io/nuget/v/RegexMatcher.svg?style=flat)](https://www.nuget.org/packages/RegexMatcher/) [![NuGet](https://img.shields.io/nuget/dt/RegexMatcher.svg)](https://www.nuget.org/packages/RegexMatcher) 

## Regex Matching Library in C#

RegexMatcher is a library that maintains an ordered list of `Regex` and `object` pairs.  Populate it with a series of `Regex` and the objects that should be returned when a match is found while evaluating some input.  All methods are thread-safe.

For examples exercising every API, refer to the test suites in `src/Test.Shared`.

## Help or Feedback

Do you need help or have feedback?  Contact me at joel at maraudersoftware.com dot com or file an issue here!

## New in v1.1.1

- Test dependency updates (Touchstone 0.2.0, NUnit 5.0.0, Microsoft.NET.Test.Sdk 18.10.1, and others); no changes to the library or its public API

## New in v1.1.0

- `ValueExists` compares values with `Object.Equals`, so equal strings and boxed value types are found
- `Get` returns a snapshot copy that is safe to enumerate while other threads modify the matcher
- Entries are always evaluated in the order they were added, including after removals
- Retargeted to .NET 8.0 and .NET 10.0 (plus .NET Standard 2.0/2.1 and .NET Framework 4.6.2/4.8)

Refer to CHANGELOG.md for upgrade notes.

## Important Notes

Always add Regex and return object in order from most specific to least specific.  Entries are evaluated in the order they were added; with the default `MatchPreference` of `First`, the first match found is used and the associated object is returned.

Regular expressions are identified by instance.  Two distinct `Regex` instances with the same pattern are separate entries, and `Exists` and `Remove` require the same instance that was passed to `Add`.  Adding the same instance twice throws `ArgumentException`.

`Match` and `AllMatches` throw `ArgumentNullException` for null or empty input.  A `RegexMatchTimeoutException` from a `Regex` constructed with a timeout is passed through to the caller.

## Simple Example
```csharp
using RegexMatcher;

static void Main(string[] args)
{
    Matcher matcher = new Matcher();

    // preload a few
    matcher.Add(new Regex("^/foo/\\d+$"), "foo with id");
    matcher.Add(new Regex("^/foo/?$"), "foo with optional slash");
    matcher.Add(new Regex("^/foo$"), "foo alone");
    matcher.Add(new Regex("^/bar/(.*?)/(.*?)/?$"), "bar with two children");
    matcher.Add(new Regex("^/bar/(.*?)/?$"), "bar with one child");
    matcher.Add(new Regex("^/bar/\\d+$"), "bar with id");
    matcher.Add(new Regex("^/bar/?$"), "bar with optional slash");
    matcher.Add(new Regex("^/bar$"), "bar alone");

    if (matcher.Match("/bar/child/foo", out object val1))
    { 
        // val is "bar with two children" 
    }

    if (matcher.Match("/foo/36", out object val2))
    { 
        // val is "foo with id" 
    }

    if (matcher.Match("/unknown", out object val3)) 
    { 
        // won't get here
    }
    else
    {
        Console.WriteLine("Not found");
    }
}
```

## Regular Expression Notes

RegexMatcher uses standard C#/.NET regular expressions.  I tested primarily against simple regular expressions with values that would be encountered as raw URLs/paths and it worked well.  

Some notes that I found helpful which may help you too:

- ```^``` is a starting anchor, useful when indicating that the pattern must be matched at the start of the input
- ```$``` is an ending anchor, useful when indicating that the pattern must be matched at the end of the input
- ```(.*?)``` will match any input string
- ```\\d+``` will match any number
- ```\\``` the escape character must be used when matching certain characters as a literal
- ```?``` marks the previous character or expression as optional

## Multiple Matches

The library behavior when multiple matches are found can be configured using the ```Matcher.MatchPreference``` property.

- ```First``` (default) returns the value for the first matching regex, in insertion order
- ```LongestFirst``` returns the value for the matching regex with the longest **pattern string**, which usually indicates the most specific pattern
- ```ShortestFirst``` returns the value for the matching regex with the shortest **pattern string**

`LongestFirst` and `ShortestFirst` compare the length of the regex pattern text, not the length of the matched input.  Ties go to the entry added first.  `AllMatches` ignores `MatchPreference` and returns every matching value in insertion order.

Helpful links:

- https://msdn.microsoft.com/en-us/library/gg578045(v=vs.110).aspx
- https://msdn.microsoft.com/en-us/library/h5181w5w(v=vs.110).aspx

## Running Tests

Tests are written once as [Touchstone](https://github.com/jchristn/touchstone) descriptors in `src/Test.Shared` and run through any of three runners:

```bash
# Console runner (exit code 0 on success, 1 on failure)
dotnet run --project src/Test.Automated -f net8.0

# Export results to JSON
dotnet run --project src/Test.Automated -f net8.0 -- --results results.json

# xUnit and NUnit runners
dotnet test src/Test.Xunit
dotnet test src/Test.Nunit
```

## Version History

Refer to CHANGELOG.md for version history.
