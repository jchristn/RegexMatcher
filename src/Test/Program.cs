using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RegexMatcher;

namespace TestNetCore
{
    class Program
    {
        static Matcher _Matcher = new Matcher();
        static bool _RunForever = true;

        static void Main(string[] args)
        {
            string userInput;
            object val;

            // preload a few
            _Matcher.Add(new Regex("^/foo/\\d+$"), "foo with id");
            _Matcher.Add(new Regex("^/foo/?$"), "foo with optional slash");
            _Matcher.Add(new Regex("^/foo$"), "foo alone");
            _Matcher.Add(new Regex("^/bar/(.*?)/(.*?)/?$"), "bar with two children");
            _Matcher.Add(new Regex("^/bar/(.*?)/?$"), "bar with one child");
            _Matcher.Add(new Regex("^/bar/\\d+$"), "bar with id");
            _Matcher.Add(new Regex("^/bar/?$"), "bar with optional slash");
            _Matcher.Add(new Regex("^/bar$"), "bar alone");

            while (_RunForever)
            {
                Console.Write("Command [? for help] > ");
                userInput = Console.ReadLine();
                if (String.IsNullOrEmpty(userInput)) continue;

                switch (userInput)
                {
                    case "q":
                        _RunForever = false;
                        break;

                    case "?":
                        Menu();
                        break;

                    case "pref":
                        Console.Write("Preference [First|LongestFirst|ShortestFirst]: ");
                        _Matcher.MatchPreference = (MatchPreferenceType)(Enum.Parse(typeof(MatchPreferenceType), Console.ReadLine()));
                        break;

                    case "add":
                        _Matcher.Add(
                            new Regex(InputString("Regex:", null, false)),
                            InputString("Value:", null, true));
                        break;

                    case "del":
                        _Matcher.Remove(
                            new Regex(InputString("Regex:", null, false)));
                        break;

                    case "exists":
                        Console.WriteLine("Exists: " +
                            _Matcher.Exists(
                                new Regex(InputString("Regex:", null, false))));
                        break;

                    case "valexists":
                        Console.WriteLine("Exists: " +
                            _Matcher.ValueExists(
                                InputString("Value:", null, true)));
                        break;

                    case "match":
                        if (_Matcher.Match(
                            InputString("Input value:", null, false),
                            out val))
                        {
                            Console.Write("Match found: ");
                            if (val == null) Console.WriteLine("(null)");
                            else Console.WriteLine(val.ToString());
                        }
                        else
                        {
                            Console.WriteLine("No match found");
                        }
                        break;

                    case "list":
                        Dictionary<Regex, object> matches = _Matcher.Get();
                        if (matches != null && matches.Count > 0)
                        {
                            Console.WriteLine("Added:");
                            foreach (KeyValuePair<Regex, object> curr in matches)
                            {
                                Console.WriteLine("  " + curr.Key.ToString() + ": " + curr.Value);
                            }
                            Console.WriteLine();
                        }
                        break;
                }
            }
        }

        static void Menu()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  q          quit the application");
            Console.WriteLine("  ?          this menu");
            Console.WriteLine("  pref       set match preference (currently " + _Matcher.MatchPreference.ToString() + ")");
            Console.WriteLine("  add        add regex to evaluation dictionary");
            Console.WriteLine("  del        remove regex from evaluation dictionary");
            Console.WriteLine("  exists     check if regex exists in evaluation dictionary");
            Console.WriteLine("  valexists  check if val exists in evaluation dictionary");
            Console.WriteLine("  match      perform a match and retrieve val for matching regex");
            Console.WriteLine("  list       list the evaluation regular expressions and their values");
            Console.WriteLine("");
        }

        static string InputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }
    }
}
