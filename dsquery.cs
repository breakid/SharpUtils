// Sources:
//   - Query AD - https://stackoverflow.com/questions/456523/quick-way-to-retrieve-user-information-active-directory
//   - List specific attributes - https://docs.microsoft.com/en-us/dotnet/api/system.directoryservices.directorysearcher.attributescopequery?view=net-5.0#System_DirectoryServices_DirectorySearcher_AttributeScopeQuery
//   - Limit output - https://docs.microsoft.com/en-us/dotnet/api/system.directoryservices.directorysearcher.sizelimit?view=net-5.0#System_DirectoryServices_DirectorySearcher_SizeLimit
//   - Set start node - https://forums.asp.net/t/1592150.aspx?How+to+set+SearchRoot+Path+in+Active+Directory+in+this+scenario

// To Compile:
//   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /t:exe /out:bin\dsquery.exe dsquery.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Text;

class dsquery {
    // Defines a Table data structure; used to print data in a well-formatted manner
    // Source: https://genert.org/blog/csharp-programming/
    class Table {
        private List<object> _columns { get; set; }
        private List<object[]> _rows { get; set; }
        private bool _printDividers;
        
        public Table(string[] columns, bool printDividers = false) {
            if (columns == null || columns.Length == 0) {
                throw new System.ArgumentException("Parameter cannot be null nor empty", "columns");
            }
            
            _columns = new List<object>(columns);
            _rows = new List<object[]>();
            _printDividers = printDividers;
        }
        
        private List<int> GetColumnsMaximumStringLengths() {
            List<int> columnsLength = new List<int>();
            
            for (int i = 0; i < _columns.Count; i++) {
                List<object> columnRow = new List<object>();
                int max = 0;
                
                columnRow.Add(_columns[i]);
                
                for (int j = 0; j < _rows.Count; j++) {
                    columnRow.Add(_rows[j][i]);
                }
               
                for (int n = 0; n < columnRow.Count; n++) {
                   int len = columnRow[n].ToString().Length;
                   
                   if (len > max) {
                       max = len;
                   }
                }
               
                columnsLength.Add(max);
            }
            
            return columnsLength;
        }
        
        public void AddRow(object[] values) {
            if (values == null) {
                throw new System.ArgumentException("Parameter cannot be null", "values");
            }
            
            if (values.Length != _columns.Count) {
                throw new Exception("The number of values in row does not match columns count.");
            }
            
            _rows.Add(values);
        }
        
        public override string ToString() {
            StringBuilder tableString = new StringBuilder();
            List<int> columnsLength = GetColumnsMaximumStringLengths();
            
            var rowStringFormat = Enumerable
                .Range(0, _columns.Count)
                .Select(i => "{" + i + ",-" + columnsLength[i] + "}   ")
                .Aggregate((total, nextValue) => total + nextValue);
            
            if (_printDividers) {
                rowStringFormat = Enumerable
                    .Range(0, _columns.Count)
                    .Select(i => " | {" + i + ",-" + columnsLength[i] + "}")
                    .Aggregate((total, nextValue) => total + nextValue) + " |";
            }
            
            string columnHeaders = string.Format(rowStringFormat, _columns.ToArray());
            List<string> results = _rows.Select(row => string.Format(rowStringFormat, row)).ToList();
            
            int maximumRowLength = Math.Max(0, _rows.Any() ? _rows.Max(row => string.Format(rowStringFormat, row).Length) : 0);
            int maximumLineLength = Math.Max(maximumRowLength, columnHeaders.Length);
            
            string dividerLine = string.Join("", Enumerable.Repeat("-", maximumLineLength - 1));
            string divider = string.Format(" {0} ", dividerLine);
            
            if (_printDividers) {
                tableString.AppendLine(divider);
            }
            
            tableString.AppendLine(columnHeaders);
            
            foreach (var row in results) {
                if (_printDividers) {
                    tableString.AppendLine(divider);
                }
                
                tableString.AppendLine(row);
            }
            
            if (_printDividers) {
                tableString.AppendLine(divider);
            }
            
            return tableString.ToString();
        }
        
        public void Print() {
            Console.WriteLine(ToString());
        }
    }
    
    public static void PrintUsage() {
        Console.WriteLine(@"Query directory services
        
USAGE:
    dsquery * [startNode] -filter <filter_string> [-attr <attributes>] [-limit <number>] [-c | -l | -t] [[-s <server>] | [-d <domain>]] [-u <UserName>] [-p <password>] [-b <buffer_size_in_MB>] [-o <output_file>] [/?]
    
        [startNode]             Optional start node (e.g., specific OU; forestroot and domainroot 
                                are NOT supported at this time)
        -filter <filter>        Standard dsquery filter string
        -attr <attributes>      Space-delimited list of attributes; use '*' to return all attributes
                                If omitted, defaults to '-attr *'
        -limit <number>         Limits query to <number> records
        -c                      Count only; prints the number of records returned by a search and exits
        -l                      Print in list format
        -t                      Print in table format with ASCII borders around each cell
        -s <server>             Query the specified server
        -d <domain>             Query the specified domain
        -u <username>           Authenticate using the specified username
        -p <password>           Authenticate using the specified password
        -o <output_filepath>    Write output to the specified file; will not overwrite an existing file
        -b <buffer_size>        Write to output file in 'buffer_size' chunks (specified in MB)
        /?                      Prints help");
        Console.WriteLine("\nDONE");
    }
    
    public static void Main(string[] args) {
        if (args.Length == 0) {
            PrintUsage();
            return;
        }
        
        string startNode = "";
        string filter = "";
        List<string> attrs = new List<string>();
        int limit = 0;
        bool adspathIncluded = false;
        bool countOnly = false;
        bool listFormat = false;
        bool printDividers = false;
        string server = "";
        string domain = "";
        string username = "";
        string password = "";
        int buffer = 2 * 1024 * 1024;
        string outputFilepath = "";
        int argIndex = 1;
        SearchResultCollection results = null;
        
        // Parse arguments
        
        // Bail if anything but a complex query
        if (args[0] != "*") {
            Console.Error.WriteLine("ERROR: The specified operation is not supported");
            Console.WriteLine("\nDONE");
            return;
        }
        
        // Parse optional start node
        if (!args[argIndex].StartsWith("-")) {
            startNode = args[argIndex];
            argIndex++;
        }
        
        // Parse other arguments
        for (int i = argIndex; i < args.Length; i++) {
            string arg = args[i];
            
            switch (arg.ToUpper()) {
                case "-FILTER":
                case "/FILTER":
                    i++;
                    filter = args[i];
                    break;
                case "-ATTR":
                case "/ATTR":
                    i++;
                    
                    while (i < args.Length && !args[i].StartsWith("-") && !args[i].StartsWith("/")) {
                        if (args[i].ToLower() == "adspath") {
                            adspathIncluded = true;
                        }
                        attrs.Add(args[i]);
                        i++;
                    }
                    
                    // Back up one so any arguments after the attributes can be parsed
                    if (i < args.Length) {
                        i--;
                    }
                    
                    break;
                case "-LIMIT":
                case "/LIMIT":
                    i++;
                    
                    if (!int.TryParse(args[i], out limit)) {
                        Console.Error.WriteLine("ERROR: Invalid limit");
                        Console.WriteLine("\nDONE");
                        return;
                    }
                    
                    break;
                case "-C":
                case "/C":
                    countOnly = true;
                    break;
                case "-L":
                case "/L":
                    listFormat = true;
                    break;
                case "-T":
                case "/T":
                    printDividers = true;
                    break;
                case "-S":
                case "/S":
                    i++;
                    server = args[i];
                    break;
                case "-D":
                case "/D":
                    i++;
                    domain = args[i];
                    break;
                case "-U":
                case "/U":
                    i++;
                    username = args[i];
                    break;
                case "-P":
                case "/P":
                    i++;
                    password = args[i];
                    break;
                case "-B":
                case "/B":
                    i++;
                    
                    if (!int.TryParse(args[i], out buffer)) {
                        Console.Error.WriteLine("ERROR: Invalid buffer size");
                        Console.WriteLine("\nDONE");
                        return;
                    }
                    
                    buffer = buffer * 1024 * 1024;
                    break;
                case "-O":
                case "/O":
                    i++;
                    outputFilepath = args[i];
                    break;
                case "/?":
                    PrintUsage();
                    return;
            }
        }
        
        // Ensure a filter is specified
        if (string.IsNullOrEmpty(filter)) {
           Console.Error.WriteLine("ERROR: No filter specified");
           Console.WriteLine("\nDONE");
           return;
        }
        
        // If no 'attr' argument is specified, assume '-attr *'
        if (attrs.Count == 0) {
           attrs.Add("*");
        }
        
        // Default to list format if '-attr *' is specified
        if (attrs.Count > 0 && attrs[0] == "*") {
            listFormat = true;
        }
        
        // Prevent accidentally overwriting a file
        if (!string.IsNullOrEmpty(outputFilepath) && File.Exists(outputFilepath)) {
           Console.Error.WriteLine("ERROR: Output file ({0}) already exists", outputFilepath);
           Console.WriteLine("\nDONE");
           return;
        }
        
        try
        {
            DirectoryEntry rootEntry;
            
            // If a server is specified, use that; if a domain is specified, use that; otherwise, auto-detect the current domain
            if (!string.IsNullOrEmpty(server)) {
                rootEntry = new DirectoryEntry("LDAP://" + server);
            } else if (!string.IsNullOrEmpty(domain)) {
                rootEntry = new DirectoryEntry("LDAP://" + domain);
            } else {
                rootEntry = new DirectoryEntry("LDAP://rootDSE");
                rootEntry = new DirectoryEntry("LDAP://" + rootEntry.Properties["defaultNamingContext"].Value);
            }
            
            // Optionally specify username and password to use to collect
            // Source: https://stackoverflow.com/questions/10742661/c-sharp-accessing-active-directory-with-different-user-credentials
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) ) {
                rootEntry.Username = username;
                rootEntry.Password = password;
            }
            
            // Initialize DirectorySearcher based on appropriate target
            DirectorySearcher searcher = new DirectorySearcher(rootEntry);
            
            // Set the start node, if applicable
            if (!string.IsNullOrEmpty(startNode)) {
                searcher.SearchRoot = new DirectoryEntry("LDAP://" + startNode);
            }
            
            searcher.Filter = filter;
            
            // Set the limit, if applicable
            if (limit > 0) {
                searcher.SizeLimit = limit;
            }
            
            // Set attributes to load
            foreach (string attr in attrs) {
                searcher.PropertiesToLoad.Add(attr);
            }
            
            // Search
            results = searcher.FindAll();
            
            // Print the number of records returned
            // Differs from native functionality, but it's convenient
            Console.WriteLine("Records Found: {0}\n", results.Count);
            
            if (!countOnly) {
                if (string.IsNullOrEmpty(outputFilepath)) {
                    // Print results to terminal
                    PrintResults(results, adspathIncluded, listFormat, printDividers);
                } else {
                    // If outputFilepath is specified, redirect standard output from the console to the output file
                    
                    // Source: https://stackoverflow.com/questions/61074203/c-sharp-performance-comparison-async-vs-non-async-text-file-io-operation-via-r
                    using (FileStream stream = new FileStream(outputFilepath, FileMode.Create, FileAccess.Write, FileShare.Read, buffer, FileOptions.SequentialScan))
                    {
                        using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                        {
                            Console.SetOut(writer);
                            
                            PrintResults(results, adspathIncluded, listFormat, printDividers);
                        }
                    }
                    
                    // Source: https://docs.microsoft.com/en-us/dotnet/api/system.console.error?view=net-5.0
                    // Recover the standard output stream so that a completion message can be displayed.
                    StreamWriter stdout = new StreamWriter(Console.OpenStandardOutput());
                    stdout.AutoFlush = true;
                    Console.SetOut(stdout);
                }
            }
        } catch (Exception ex) {
            Console.Error.WriteLine("ERROR: {0}", ex.Message);
        } finally {
            // Dispose of objects used
            if (results != null) {
                results.Dispose();
            }
        }
        
        Console.WriteLine("\nDONE");
    }
    
    
    private static void PrintResults(SearchResultCollection results, bool adspathIncluded, bool listFormat, bool printDividers) {
        if (results.Count > 0) {
            // Get column names from PropertiesToLoad because it is in the user-specified order whereas PropertyNames is not
            string[] properties = results.PropertiesLoaded.ToArray();
            
            // ADsPath is automatically included; remove it from the list of properties unless it was explicitly specified
            if (!adspathIncluded) {
                properties = results.PropertiesLoaded.Where(e => e != "ADsPath").ToArray();
            }
            
            // If '-attr *' was specified, we have to get the property list from PropertyNames; might as well sort them for convenience
            if (properties[0] == "*") {
                List<string> props = new List<string>();
                
                foreach (string prop in results[0].Properties.PropertyNames) {
                    props.Add(prop);
                }
                
                props.Sort();
                properties = props.ToArray();
            }
            
            // Print results in either list or table format
            if (listFormat) {
                PrintResultsListFormat(properties, results);
            } else {
                PrintResultsTableFormat(properties, results, printDividers);
            }
        } else {
            Console.WriteLine("No records matched the specified criteria");
        }
    }
    
    
    private static void PrintResultsTableFormat(string[] columns, SearchResultCollection results, bool printDividers) {
        // Populate a Table data structure and print it, optionally with delimiters (enhancement)
        Table tbl = new Table(columns, printDividers);
        
        List<string> rowData;
        string entry;
        
        foreach (SearchResult searchResult in results) {
            rowData = new List<string>();
            
            foreach (string key in columns) {
                entry = "";
                
                // Join multiple entries with a ';'
                for (int i = 0; i < searchResult.Properties[key].Count; i++) {
                    entry += NormalizeData(key, searchResult.Properties[key][i]).ToString() + ";";
                }
                
                // Remove trailiing ';', if applicable
                if (entry.Length > 0) {
                    entry = entry.Remove(entry.Length - 1);
                }
                
                rowData.Add(entry);
            }
            
            tbl.AddRow(rowData.ToArray());
        }
        
        tbl.Print();
    }
    
    
    private static void PrintResultsListFormat(string[] properties, SearchResultCollection results) {
        // Loop through results and print all retrieved attributes
        foreach (SearchResult searchResult in results) {
            foreach (string key in properties) {
                for (int i = 0; i < searchResult.Properties[key].Count; i++) {
                    Console.WriteLine("{0}: {1}", key, NormalizeData(key, searchResult.Properties[key][i]));
                }
            }
            
            // New-line delimit entries
            Console.WriteLine("");
        }
    }
    
    
    private static string NormalizeData(string key, object data) {
        // Some data is returned as a byte-array; convert known fields to their appropriate string values
        if (key.ToLower() == "objectsid") {
            data = ConvertByteToStringSid((byte[])data);
        } else if (key.ToLower() == "objectguid") {
            data = "{" + (new Guid((byte[])data)).ToString().ToUpper() + "}";
        }
        
        return data.ToString();
    }
    
    
    // Convert a byte-array representing a Windows SID to an appropriate string representation
    // Source: https://stackoverflow.com/questions/47209459/adding-all-users-sids-from-active-directory-in-c-sharp
    public static string ConvertByteToStringSid(Byte[] sidBytes) {
        StringBuilder strSid = new StringBuilder();
        strSid.Append("S-");
        
        try {
            // Add SID revision.
            strSid.Append(sidBytes[0].ToString());
            
            // Next six bytes are SID authority value.
            if (sidBytes[6] != 0 || sidBytes[5] != 0) {
                string strAuth = String.Format
                    ("0x{0:2x}{1:2x}{2:2x}{3:2x}{4:2x}{5:2x}",
                    (Int16)sidBytes[1],
                    (Int16)sidBytes[2],
                    (Int16)sidBytes[3],
                    (Int16)sidBytes[4],
                    (Int16)sidBytes[5],
                    (Int16)sidBytes[6]);
                strSid.Append("-");
                strSid.Append(strAuth);
            } else {
                Int64 iVal = (Int32)(sidBytes[1]) +
                    (Int32)(sidBytes[2] << 8) +
                    (Int32)(sidBytes[3] << 16) +
                    (Int32)(sidBytes[4] << 24);
                strSid.Append("-");
                strSid.Append(iVal.ToString());
            }

            // Get sub authority count...
            int iSubCount = Convert.ToInt32(sidBytes[7]);
            int idxAuth = 0;
            
            for (int i = 0; i < iSubCount; i++) {
                idxAuth = 8 + i * 4;
                UInt32 iSubAuth = BitConverter.ToUInt32(sidBytes, idxAuth);
                strSid.Append("-");
                strSid.Append(iSubAuth.ToString());
            }
        } catch (Exception ex) {
            Console.Error.WriteLine("ERROR: {0}", ex.Message);
        }
        
        return strSid.ToString();
    }
}