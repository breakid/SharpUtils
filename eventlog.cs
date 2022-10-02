/*
 * USAGE: eventlog.exe <log_name> [-c <count>] [e <event IDs (comma-separated, no space)>]
 *
 * Examples:
 *     eventlog.exe Security -e 4624 -c 5
 *
 *     eventlog.exe Security -e 4648 -c 3
 */

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /out:eventlog.exe eventlog.cs


using System;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

public class ReadEventLog
{
    private static void PrintUsage()
    {
        Console.WriteLine(@"Reads events from the specified log file; optionally limit by EventID and number of events returned
    
USAGE:
    eventlog.exe <log name> [/C <count>] [/E <event IDs (comma-separated, no space)>] [/?]
    
EXAMPLES:
    eventlog.exe Security -e 4624 -c 5
        - Displays the most recent 5 'Logon' events from the Security log
    
    eventlog.exe Security -e 4648 -c 3
        - Displays the most recent 3 'Logon with explicit credentials' events from the Security log");
    }

    public static void Main(string[] args)
    {
        try
        {
            string eventLogName = "";
            int count = 0;
            List<long> eventIDs = new List<long>();
            bool test;

            // Parse arguments
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToUpper())
                {
                    case "/C": // Count
                    case "-C":
                        i++;

                        try
                        {
                            // Catch error while attempting to parse the count to prevent exception
                            test = int.TryParse(args[i], out count);
                            if (test == false || count < 1)
                            {
                                throw new ArgumentException("Invalid count");
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No count specified");
                        }
                        break;

                    case "-E": // EventID(s)
                    case "/E":
                        i++;

                        try
                        {
                            foreach (string eventIdStr in args[i].Split(','))
                            {
                                long eventId;

                                // Catch error while attempting to parse the count to prevent exception
                                test = long.TryParse(eventIdStr, out eventId);
                                if (test == false)
                                {
                                    throw new ArgumentException("Invalid EventID(s)");
                                }

                                eventIDs.Add(eventId);
                            }
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ArgumentException("No EventID specified");
                        }
                        break;
                    case "/?":
                        PrintUsage();
                        return;
                    default: // eventLogName
                        eventLogName = arg;
                        break;
                }
            }

            if (eventLogName != "")
            {
                EventLog eventLog = new EventLog();
                eventLog.Log = eventLogName;

                int numEntries = 0;

                // Loop through the events in reverse order to get the most recent first
                foreach (EventLogEntry log in eventLog.Entries.Cast<EventLogEntry>().Reverse())
                {
                    // Print entries whose eventID matches one of the ones specified; limit to the last "count" entries
                    if (eventIDs.Count == 0 || eventIDs.IndexOf(log.InstanceId) != -1)
                    {
                        Console.WriteLine("Timestamp: {0}", log.TimeGenerated);
                        Console.WriteLine("EventID: {0}", log.InstanceId);
                        Console.WriteLine("Computer: {0}", log.MachineName);
                        Console.WriteLine("Username: {0}", log.UserName);
                        Console.WriteLine("Message: {0}\n", log.Message);
                        //Console.WriteLine("--------------------");
                        numEntries++;
                    }

                    // Stop when "count" entries have been printed
                    if (count > 0 && numEntries == count)
                    {
                        break;
                    }
                }
            }
            else
            {
                PrintUsage();
                return;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("[-] ERROR: {0}", e.Message.Trim());
        } finally
        {
            Console.WriteLine("\nDONE");
        }
    }
}