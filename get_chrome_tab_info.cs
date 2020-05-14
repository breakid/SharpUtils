// Sources:
//   https://stackoverflow.com/questions/40070703/how-to-get-a-list-of-open-tabs-from-chrome-c-sharp
//   https://stackoverflow.com/questions/18897070/getting-the-current-tabs-url-from-google-chrome-using-c-sharp/
//   https://social.msdn.microsoft.com/Forums/en-US/27236bd4-9c75-494b-b5e3-d3d52f660619/how-to-get-the-urls-of-all-tabs-in-a-chrome-browser?forum=csharpgeneral

// To Compile: 
//   C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\UIAutomationTypes\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationTypes.dll" /reference:"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\UIAutomationClient\v4.0_4.0.0.0__31bf3856ad364e35\UIAutomationClient.dll" /t:exe /out:get_chrome_tab_info.exe get_chrome_tab_info.cs

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Automation;

class EnumChromeTabs {
    private static void PrintUsage() {
        Console.WriteLine(@"Displays the URL of the current tab and the titles of all tabs in foremost Chrome window. Will NOT display information about other Chrome windows.
    
USAGE:
    get_chrome_tab_info.exe");
        Console.WriteLine("\nDONE");
    }
    
    public static void Main(string[] args) {
        // Parse arguments
        for (int i = 0; i < args.Length; i++) {
            string arg = args[i];
        
            switch (arg.ToUpper()) {
                case "/?": // Help
                    PrintUsage();
                    return;
                default:
                    break;
            }
        }
        
        // there are always multiple chrome processes, so we have to loop through all of them to find the
        // process with a Window Handle and an automation element of name "Address and search bar"
        Process[] procsChrome = Process.GetProcessesByName("chrome");
        
        if (procsChrome.Length <= 0) {
            Console.WriteLine("Chrome is not running");
        } else {
            foreach (Process proc in procsChrome) {
                // the chrome process must have a window
                if (proc.MainWindowHandle == IntPtr.Zero) {
                    continue;
                }
                
                try {
                    Console.WriteLine("Process ID: " + proc.Id);
                    Console.WriteLine("Session ID: " + proc.SessionId);
                    
                    // Get the main window
                    AutomationElement root = AutomationElement.FromHandle(proc.MainWindowHandle);
                    
                    // Find the address bar and display the URL
                    AutomationElement elmUrlBar = root.FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, "Address and search bar"));
                    
                    // if it can be found, get the value from the URL bar
                    if (elmUrlBar != null) {
                        AutomationPattern[] patterns = elmUrlBar.GetSupportedPatterns();
                        if (patterns.Length > 0) {
                            ValuePattern val = (ValuePattern)elmUrlBar.GetCurrentPattern(patterns[0]);
                            Console.WriteLine("Current URL: " + val.Current.Value);
                        }
                    }
                    
                    // to find the tabs we first need to locate something reliable - the 'New Tab' button 
                    AutomationElement elmNewTab = root.FindFirst(TreeScope.Descendants, 
                        new PropertyCondition(AutomationElement.NameProperty, "New Tab"));
                    
                    // get the tabstrip by getting the parent of the 'new tab' button 
                    TreeWalker treewalker = TreeWalker.ControlViewWalker;
                    AutomationElement elmTabStrip = treewalker.GetParent(elmNewTab);
                    
                    // loop through all the tabs and get the names which is the page title 
                    Condition condTabItem = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem);
                    
                    Console.WriteLine("Tabs:");

                    foreach (AutomationElement tabitem in elmTabStrip.FindAll(TreeScope.Children, condTabItem))
                    {
                        Console.WriteLine("  " + tabitem.Current.Name);
                    }
                } catch {
                    Console.WriteLine("Chrome is minimized");
                }
            }
        }
        
        Console.WriteLine("\nDONE");
    }
}