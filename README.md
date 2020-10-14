# RegexMatcher

[![NuGet Version](https://img.shields.io/nuget/v/RegexMatcher.svg?style=flat)](https://www.nuget.org/packages/RegexMatcher/) [![NuGet](https://img.shields.io/nuget/dt/RegexMatcher.svg)](https://www.nuget.org/packages/RegexMatcher) 

## Regex Matching Library in C#

RegexMatcher is a library that maintains an internal dictionary of type <Regex, object>.  Populate the dictionary with a series of Regex and the objects that should be returned when a match is found while evaluating some input.

For a sample app exercising the library please refer to the Test project. 

## Help or Feedback

Do you need help or have feedback?  Contact me at joel at maraudersoftware.com dot com or file an issue here!

## New in v1.0.5

- ```MatchPreference``` property to specify how to handle multiple match scenarios: first match, longest match, or shortest match

## Important Notes

Always add Regex and return object in order from most specific to least specific.  The first match found will be used and the associated object will be returned.

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
- ```$``` is an ending anchor, useful when indicating that the pattern mst be matched at the end of the input
- ```(.*?)``` will match any input string
- ```\\d+``` will match any number
- ```\\``` the escape character must be used when matching certain characters as a literal
- ```?``` marks the previous character or expression as optional

Helpful links:

- https://msdn.microsoft.com/en-us/library/gg578045(v=vs.110).aspx
- https://msdn.microsoft.com/en-us/library/h5181w5w(v=vs.110).aspx

## Version History

Refer to CHANGELOG.md for version history.
