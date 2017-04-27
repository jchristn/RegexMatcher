using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RegexMatcher;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Matcher matcher = new Matcher();
            bool runForever = true;
            string userInput;
            object val;

            // preload a few
            matcher.Add(new Regex("^/foo/\\d+$"), "foo with id");
            matcher.Add(new Regex("^/foo/?$"), "foo with optional slash");
            matcher.Add(new Regex("^/foo$"), "foo alone");
            matcher.Add(new Regex("^/bar/(.*?)/(.*?)/?$"), "bar with two children");
            matcher.Add(new Regex("^/bar/(.*?)/?$"), "bar with one child");
            matcher.Add(new Regex("^/bar/\\d+$"), "bar with id");
            matcher.Add(new Regex("^/bar/?$"), "bar with optional slash");
            matcher.Add(new Regex("^/bar$"), "bar alone");

            while (runForever)
            {
                Console.Write("Command [? for help] > ");
                userInput = Console.ReadLine();
                if (String.IsNullOrEmpty(userInput)) continue;

                switch (userInput)
                {
                    case "q":
                        runForever = false;
                        break;

                    case "?":
                        Menu();
                        break;

                    case "add":
                        matcher.Add(
                            new Regex(InputString("Regex", null, false)),
                            InputString("Value", null, true));
                        break;

                    case "del":
                        matcher.Remove(
                            new Regex(InputString("Regex", null, false)));
                        break;

                    case "exists":
                        Console.WriteLine("Exists: " +
                            matcher.Exists(
                                new Regex(InputString("Regex", null, false))));
                        break;

                    case "valexists":
                        Console.WriteLine("Exists: " +
                            matcher.ValueExists(
                                InputString("Value", null, true)));
                        break;

                    case "match":
                        if (matcher.Match(
                            InputString("Input value", null, false), 
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
                }
            }
        }

        static void Menu()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  q          quit the application");
            Console.WriteLine("  ?          this menu");
            Console.WriteLine("  add        add regex to evaluation dictionary");
            Console.WriteLine("  del        remove regex from evaluation dictionary");
            Console.WriteLine("  exists     check if regex exists in evaluation dictionary");
            Console.WriteLine("  valexists  check if val exists in evaluation dictionary");
            Console.WriteLine("  match      perform a match and retrieve val for matching regex");
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
