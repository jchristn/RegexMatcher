# RegexMatcher

[![][nuget-img]][nuget]

[nuget]:     https://www.nuget.org/packages/RegexMatcher.dll
[nuget-img]: https://badge.fury.io/nu/Object.svg

## Regex Matching Library in C#

RegexMatcher is a library that maintains an internal dictionary of type <Regex, object>.  Populate the dictionary with a series of Regex and the objects that should be returned when a match is found while evaluating some input.

For a sample app exercising the library please refer to the Test project. 

## Help or Feedback

Do you need help or have feedback?  Contact me at joel at maraudersoftware.com dot com or file an issue here!

## New in v1.0.0

- initial release

## Important Notes
Always add Regex and return object in order from most specific to least specific.  The first match found will be used and the associated object will be returned.

## Simple Example
```
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

    object val;
    if (matcher.Match("/bar/child/foo", out val))
    { // val is "bar with two children" }

    if (matcher.Match("/foo/36", out val))
    { // val is 36 }

    if (matcher.Match("/unknown", out val)) { }
    else Console.WriteLine("Not found");
}
```

## Version History

Notes from previous versions (starting with v1.0.0) will be moved here.
