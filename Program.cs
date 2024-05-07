/**************************************************************************
 * Pinger - Advanced command line ping tool
 * Copyright (c) 2024 Teiva Rodiere
 * https://github.com/teivarodiere/pinger
 *************************************************************************/

// pinger Utility
// use 'pinger.exe /?' for help+
using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Net;
using System.Xml.Serialization;
using System.Net.Sockets;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;
using System.Security.Permissions;
using System.Globalization;
using System.Formats.Tar;

namespace Pinger
{
    public enum MessageBeepType
    {
        //Default = -1,
        Ok = 262,
        Error = 311,
        // Question = 0x00000020,
        // Warning = 0x00000030,
        //Information = 0x00000040,
    }
    public enum ResultCodes
    {
        //Default = -1,
        Ok = 0,
        Error = 1
    }
    public enum DnsLookupByCodes
    {
        ByIP = 0,
        ByName = 1
    }

    static class Globals
    {
        public static bool PROGRAM_VERBOSE = false; // false by default. Verbose on screen
        public static bool PROGRAM_VERBOSE_LEVEL1 = false; // false by default. Verbose more on screen 
        public static bool PROGRAM_VERBOSE_LEVEL2 = false; // false by default. Verbose more on screen 

        public static ResultCodes RUNTIME_ERROR = ResultCodes.Ok; // OK no errors by default. The latest error is stored in this variable
        // Ping for a number of counts
        public static bool MAX_COUNT_USER_SPECIFIED = false; // false by default. How many times the user requested to ping a specific target host
        public static double PING_COUNT_RUNTIME_VALUE = 0; // 0 by default. Keep track of the current ping count per host
        public static double PING_COUNT_VALUE_USER_SPECIFIED = 0; // 0 by default. The number of times the user requested to ping a host
        public static bool VERBOSE = false; // false by default. ot sure
        public static bool PING_ALL_IP_ADDRESSES = false;  // false by default. If a hostname is specified, and the DNS resolve to multiple IP addresses, set to true to ping all it's IPs, or false to ping the first returned IP
        public static bool PING_ONLY_DNS_RESOLVABLE_TARGETS = false; // false by default. If you wish to pinger to ping only DNS resolvable systems, set this to true
        public static bool IPV4_ONLY_IF = false;  // false by default. The user is only requesting to ping IPv4 addresses. NOTE: DNS lookup may resolve hosts to IPv6 or the user can pass on to pinger an IPv6 address. Setting this variable to true, skips IPv6 addresses
        public static bool IPV6_ONLY_IF = false;  // false by default. The user is only requesting to ping IPv4 addresses. NOTE: DNS lookup may resolve hosts to IPv6 or the user can pass on to pinger an IPv6 address. Setting this variable to true, skips IPv6 addresses
        public static bool ENABLE_CONTINEOUS_PINGS = false; // false by default. When true, pinger pings like standard pint, aka contenously.
        public static bool SILENCE_AUDIBLE_ALARM = false; // false by default. Do not issue 4 Beeps on failed ping, and 2 Beeps on successfull beeps. Beeps work on Windows and Macs, but on Mac only hear 1 beep regardless.
        public static int BEEP_COUNTS_SUCCESSFULL = 2; // 2 by default. The console will beep this many times to alert of a successfull ping reply 
        public static int BEEP_COUNTS_UNSUCCESSFULL = 4; // 4 by default. The console will beep this many times to alert of a unsuccessfull ping reply 
        public static bool PRINT_NEW_LINE = false; // false by default. Do not print a new line in some cases in the console output - not recommended to set to true. Don't know why it's an option still
        public static bool FORCE_SLEEP = true; // true by default. overrides pinger from sleeping 1 second between a ping response and the next ping. This will increase the ping rate to potentially. 
        public static int SLEEP_IN_USER_REQUESTED_IN_SECONDS = 1; // 1 by default. pinger will 'try' to ping host every set time in seconds specified in SLEEP_IN_USER_REQUESTED_IN_SECONDS.
        public static int DEFAULT_POLLING_MILLISECONDS = 1000; //  1000 by default. Tells the script to ping  in ms or can be seen as polling, not how long to wait for a response
        public static int DEFAULT_PING_TIME_TO_LEAVE = 64; //  128 by default. Tells the script to ping  in ms or can be seen as polling, not how long to wait for a response
        public static int DEFAULT_TIMEOUT_MILLISECONDS = 1000; // 120 milliseconds of amount of time in ms to wait for the ping reply - matches regular ping on windows
        public static int SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS = Globals.DEFAULT_POLLING_MILLISECONDS;
        public static bool DURATION_USER_SPECIFIED = false;
        public static double DURATION_VALUE_USER_SPECIFIED = 0;
        public static double DURATION_VALUE_IN_DECIMAL = 0;
        public static DateTime DURATION_END_DATE;
        public static TimeSpan DURATION_TIMESPAN;
        public static bool OUTPUT_SCREEN_TO_CSV = false;
        public static bool OUTPUT_ALL_TO_CSV = false;
        public static bool SKIP_DNS_LOOKUP = false;
        public static string DNS_SERVER = "";
        public static string SEPARATOR_CHAR = ","; // this string will be used to separate the output

        public static string SUCCESS_STATUS_STRING = "Success";

        public static string TIMEDOUT_STATUS_STRING = "NoReply";
        public static string OTHER_STATUS_STRING = "Other";
    }

    public class BackgroundBeep
    {
        static Thread _beepThread;
        static AutoResetEvent _signalBeep;
        public enum MessageBeepType
        {
            //Default = -1,
            Ok = 262,
            Error = 311,
            // Question = 0x00000020,
            // Warning = 0x00000030,
            //Information = 0x00000040,
        }

        static BackgroundBeep()
        {
            _signalBeep = new AutoResetEvent(false);
            _beepThread = new Thread(() =>
            {
                for (; ; )
                {
                    _signalBeep.WaitOne();
                    if (OperatingSystem.IsWindows())
                    {
                        Console.Beep(311, 1600);
                    }
                }
            }, 1);
            _beepThread.IsBackground = true;
            _beepThread.Start();
        }


        public static void Beep()
        {
            _signalBeep.Set();
        }
    }

    public class DnsHostObject
    {
        public required string LookupString { get; set; }
        public int Index { get; set; }
        public string? HostName { get; set; }
        public string? DnsResolvedHostname { get; set; }
        public bool Skip { get; set; }
        public System.Net.IPAddress[] IPAddresses { get; set; }
        public string[]? Aliases { get; set; }
        public string? DnsLookUpMessage { get; set; }

        public ResultCodes DnsLookUpCode { get; set; }
        public DnsLookupByCodes DnsLookupType { get; set; }

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute]
        public DnsHostObject(string newLookString)
        {
            LookupString = newLookString;
        }
        public void Printout()
        {
            string subsection = "DnsHostObject->Printout()";
            //System.Reflection.MethodBase.GetCurrentMethod().MethodHandle.ToString();
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
            Console.WriteLine("[VERBOSE][" + subsection + "] _LookupString = " + LookupString);
            Console.WriteLine("[VERBOSE][" + subsection + "] Index = " + Index);
            Console.WriteLine("[VERBOSE][" + subsection + "] HostName=" + DnsResolvedHostname);
            Console.WriteLine("[VERBOSE][" + subsection + "] DnsLookUpMessage=" + DnsLookUpMessage);
            Console.WriteLine("[VERBOSE][" + subsection + "] DnsLookUpCode=" + DnsLookUpCode);
            Console.WriteLine("[VERBOSE][" + subsection + "] DnsLookupType=" + DnsLookupType);
            Console.WriteLine("[VERBOSE][" + subsection + "] Skip=" + Skip);
            if (DnsLookUpCode == ResultCodes.Ok)
            {
                int ipIndex = 1;
                foreach (System.Net.IPAddress ip in IPAddresses)
                {
                    Console.WriteLine("[VERBOSE][DnsHostObject] IP" + ipIndex + "=" + ip.ToString() + "[" + ip.AddressFamily + "]");
                    ipIndex++;
                }
            }
            else
            {
                Console.WriteLine("[VERBOSE][DnsHostObject] No IPAddresses");
            }
        }
    }

    public class DateRange {
        public int Index {get; set;}
        public DateTime Start {get; set;}
        public DateTime End {get; set;}
    }

    public class PingerTarget
    {
        public required int TargetIndex { get; set; }
        public required string LookupString { get; set; }
        private string _DnsResolvedHostname;
        private string _DisplayName;
        public System.Net.IPAddress? IPAddress { get; set; }
        private string dnsReplyIPaddr;
        private bool skip;
        private long pingReplyRoundTripInMiliSec;
        private ResultCodes _DnsLookupStatus;
        public string? DnsLookupMessage { get; set; }
        private string logFile;
        private string errorMsg;
        private int errorCode;
        private int optionsTtl;
        private int hostUnreachableCount;
        public List<DateRange> UnreachableDates { get; private set; }
        private TimeSpan hostUnreachableTimespan;
        private TimeSpan hostReachableTimespan;
        private int hostReachableCount;
        private DateTime startDate;
        private DateTime endDate;

        private IPStatus currHostPingStatus;
        private int currHostPingCount;
        private DateTime currStatusPingDateCurrent;
        private DateTime currStatusPingDatePrevious;

        private IPStatus prevHostPingStatus;
        private DateTime prevStatusPingDate;
        private int prevStatusPingCount;
        private DnsLookupByCodes _DnsLookupType;
        private bool pingByIP;
        private string parentTarget;

        [System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute]
        public PingerTarget(string targetName, int targetIndex)
        {
            LookupString = targetName;
            TargetIndex = targetIndex;
            UnreachableDates = new List<DateRange>();
            this.Startdate = DateTime.Now;
            this.currStatusPingDateCurrent = DateTime.Now;
            this.currStatusPingDatePrevious = DateTime.Now;
            this.prevStatusPingDate = DateTime.Now;
            this._DnsResolvedHostname = "-";            
            this._DisplayName = "-";
            //this._Dnsipaddr = "-";
            this.dnsReplyIPaddr = "-";
            this.pingReplyRoundTripInMiliSec = -1;
            this.skip = false;
            //this._DnsLookupStatus = ResultCodes.OK;
            this.logFile = "-";
            this.currHostPingStatus = IPStatus.Success;
            this.prevHostPingStatus = IPStatus.Unknown;
            this.errorMsg = "-";
            this.optionsTtl = -1;
            this.errorCode = -1; // No Errors
            this.hostUnreachableCount = 0;
            this.hostReachableCount = 0;
            this.currHostPingCount = 0;
            this.prevStatusPingCount = 0;
            //this._DnsLookupType = ;
            this.pingByIP = false;
            this.parentTarget = "-";

        }
      
        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; }
        }
        public string ParentTarget
        {
            get { return parentTarget; }
            set { parentTarget = value; }
        }
        public string DnsResolvedHostname
        {
            get { return _DnsResolvedHostname; }
            set { _DnsResolvedHostname = value; }
        }
        public bool Skip
        {
            get { return skip; }
            set { skip = value; }
        }
        public ResultCodes DnsLookupStatus
        {
            get { return _DnsLookupStatus; }
            set { _DnsLookupStatus = value; }
        }
        public DnsLookupByCodes DnsLookupType
        {
            get { return _DnsLookupType; }
            set { _DnsLookupType = value; }
        }
        public string LogFile
        {
            get { return logFile; }
            set { logFile = value; }
        }
        public string ReplyIPAddress
        {
            get { return dnsReplyIPaddr; }
            set { dnsReplyIPaddr = value; }
        }
        public IPStatus PrevHostPingStatus
        {
            get { return prevHostPingStatus; }
            set { prevHostPingStatus = value; }
        }
        public IPStatus CurrHostPingStatus
        {
            get { return currHostPingStatus; }
            set
            {
                //prevHostPingStatus = currHostPingStatus;
                currHostPingStatus = value;
            }
        }
        public int HostReachableCount
        {
            get { return hostReachableCount; }
            set { hostReachableCount = value; }
        }
        public int HostUnreachableCount
        {
            get { return hostUnreachableCount; }
            set { hostUnreachableCount = value; }
        }
        // public List<DateRange> UnreachableDates {get ;private set;}
        // {
        //     get { 
        //         return unreachableDates; 
        //     }
        //     set { 
        //         DateRange dr = new DateRange();
        //         dr.Index = value.Index;
        //         dr.Start = value.Start; 
        //         dr.End = value.End; 
        //         unreachableDates.Add(dr);
        //     }
        // }
        public TimeSpan HostUnreachableTimespan
        {
            get { return hostUnreachableTimespan; }
            set { hostUnreachableTimespan = value; }
        }
       
        public TimeSpan HostReachableTimespan
        {
            get { return hostReachableTimespan; }
            set { hostReachableTimespan = value; }
        }
       
        public int Errorcode
        {
            get { return errorCode; }
            set { errorCode = value; }
        }
        public string ErrorMsg
        {
            get { return errorMsg; }
            set { errorMsg = value; }
        }

        public int CurrHostPingCount
        {
            get { return currHostPingCount; }
            set { currHostPingCount = value; }
        }
        public int PrevStatusPingCount
        {
            get { return prevStatusPingCount; }
            set { prevStatusPingCount = value; }
        }
        public int OptionsTtl
        {
            get { return optionsTtl; }
            set { optionsTtl = value; }
        }
        public DateTime Startdate
        {
            get { return startDate; }
            set { startDate = DateTime.Now; }
        }
        public DateTime Enddate
        {
            get { return endDate; }
            set { endDate = DateTime.Now; }
        }

        public DateTime CurrStatusPingDatePrevious
        {
            get { return currStatusPingDatePrevious; }
            set
            {
                currStatusPingDatePrevious = value;
            }
        }
        public DateTime CurrStatusPingDateCurrent
        {
            get { return currStatusPingDateCurrent; }
            set { currStatusPingDateCurrent = value; }
        }
        public DateTime PrevStatusPingDate
        {
            get { return prevStatusPingDate; }
            set { prevStatusPingDate = value; }
        }
        public bool PingByIP
        {
            get { return pingByIP; }
            set { pingByIP = value; }
        }
        public long RoundTrip
        {
            get { return pingReplyRoundTripInMiliSec; }
            set { pingReplyRoundTripInMiliSec = value; }
        }
        public void Printout()
        {
            string MYFUNCTION = "Printout";
            Console.WriteLine("[" + MYFUNCTION + "]    ++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("[" + MYFUNCTION + "]    LookupString(User Searched String) = " + LookupString);
            Console.WriteLine("[" + MYFUNCTION + "]    DnsResolvedHostname = " + _DnsResolvedHostname);
            Console.WriteLine("[" + MYFUNCTION + "]    DisplayName = " + _DisplayName);
            if (IPAddress != null)
            {
                Console.WriteLine("[" + MYFUNCTION + "]    dnsipaddr = " + IPAddress.ToString());
                Console.WriteLine("[" + MYFUNCTION + "]    dnsipaddr Type = " + IPAddress.AddressFamily);
            }
            else
            {
                Console.WriteLine("[" + MYFUNCTION + "]    dnsipaddr = ");
                Console.WriteLine("[" + MYFUNCTION + "]    dnsipaddr Type = ");
            }

            Console.WriteLine("[" + MYFUNCTION + "]    dnsReplyIPaddr = " + dnsReplyIPaddr);
            Console.WriteLine("[" + MYFUNCTION + "]    DnsLookupStatus = " + DnsLookupStatus);
            Console.WriteLine("[" + MYFUNCTION + "]    parentTarget = " + parentTarget);
            Console.WriteLine("[" + MYFUNCTION + "]    pingReplyRoundTripInMiliSec = " + pingReplyRoundTripInMiliSec);
            Console.WriteLine("[" + MYFUNCTION + "]    optionsTtl = " + optionsTtl);
            Console.WriteLine("[" + MYFUNCTION + "]    errorMsg = " + errorMsg);
            Console.WriteLine("[" + MYFUNCTION + "]    errorCode = " + errorCode);
            Console.WriteLine("[" + MYFUNCTION + "]    hostUnreachableCount = " + hostUnreachableCount);
            Console.WriteLine("[" + MYFUNCTION + "]    hostReachableCount = " + hostReachableCount);
            Console.WriteLine("[" + MYFUNCTION + "]    startDate = " + startDate);
            Console.WriteLine("[" + MYFUNCTION + "]    endDate = " + endDate);
            Console.WriteLine("[" + MYFUNCTION + "]    LogFile = " + logFile);
            Console.WriteLine("[" + MYFUNCTION + "]    DnsLookupByCodes = " + _DnsLookupType);
            Console.WriteLine("[" + MYFUNCTION + "]    PingByIP = " + pingByIP);
            Console.WriteLine("[" + MYFUNCTION + "]    Skip = " + skip);
            Console.WriteLine("[" + MYFUNCTION + "]    currHostPingCount = " + currHostPingCount);
            Console.WriteLine("[" + MYFUNCTION + "]    currHostPingStatus = " + currHostPingStatus);
            Console.WriteLine("[" + MYFUNCTION + "]    currStatusPingDateCurrent = " + currStatusPingDateCurrent);
            Console.WriteLine("[" + MYFUNCTION + "]    currStatusPingDatePrevious = " + currStatusPingDatePrevious);
            Console.WriteLine("[" + MYFUNCTION + "]    prevHostPingStatus = " + prevHostPingStatus);
            Console.WriteLine("[" + MYFUNCTION + "]    PrevStatusPingCount = " + prevStatusPingCount);
            Console.WriteLine("[" + MYFUNCTION + "]    prevStatusPingDate = " + prevStatusPingDate);
            Console.WriteLine("[" + MYFUNCTION + "]    ++++++++++++++++++++++++++++++++++++++++++++");
        }
        public void Printshortnames()
        {
            string MYFUNCTION = "Printout-Shortnames";
            Console.WriteLine("[" + MYFUNCTION + "]    ++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("[" + MYFUNCTION + "]    LookupString(User Searched String) = " + LookupString);
            Console.WriteLine("[" + MYFUNCTION + "]    DnsResolvedHostname = " + _DnsResolvedHostname);
            Console.WriteLine("[" + MYFUNCTION + "]    DisplayName = " + _DisplayName);
            if (IPAddress != null)
            {
                Console.WriteLine("[" + MYFUNCTION + "]    dnsipaddr = " + IPAddress.ToString());
                Console.WriteLine("[" + MYFUNCTION + "]    dnsipaddr Type = " + IPAddress.AddressFamily);
            }
            else
            {
                Console.WriteLine("[" + MYFUNCTION + "]    dnsipaddr = ");
                Console.WriteLine("[" + MYFUNCTION + "]    dnsipaddr Type = ");
            }
            Console.WriteLine("[" + MYFUNCTION + "]    dnsReplyIPaddr = " + dnsReplyIPaddr);
        }

    }

    class Program
    {
        static async Task Main(string[] args)
        {
          
            string MYFUNCTION = "MAIN";
            // List<Task<PingReply>> pingTasks = new List<Task<PingReply>>();
            Ping pingSender = new Ping();

            //List<PingerTarget> tmpHostObjectList = new List<PingerTarget>();
            List<PingerTarget> listOfUserRequestedTargets = new List<PingerTarget>();
            List<PingerTarget> pingableTargetList = new List<PingerTarget>();
            List<DnsHostObject> listDnsHostsObjects = new List<DnsHostObject>();
             Console.CancelKeyPress += delegate {
                ShowSummary(pingableTargetList);
            };
            //List<DnsHostObject> listDnsHostsObjects;
            // Create  the ping target object, aka pt
            //PingerTarget pt;= new PingerTarget(); // <- Review this guy
            bool verbose = false; // true = print additional versbose stuff for the program
            int items = -1; // compensate for "pinger" counting as 1 command line argument

            //bool ENABLE_CONTINEOUS_PINGS = false; // by default use the smart ping switch
            bool return_code_only = false;
            string inputTargetList = ""; // target IP address or DNS name to ping
            string outputCSVFilename = "";
            string outstr = "";

            //int sleeptimesec = Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS / 1000;
            double timelapsSinceStatusChange = 0; // time since the last status change
                                                  //int proposedSleepTime = Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS;

            // Create a buffer of 32 bytes of data to be transmitted.
            //string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            // Create a buffer of 64 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeoutms = Globals.DEFAULT_TIMEOUT_MILLISECONDS;
            int timeoutsec = timeoutms / 1000;
            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.            

            // iterate through the arguments
            string[] arguments = Environment.GetCommandLineArgs();
            for (int argIndex = 0; argIndex < arguments.Length; argIndex++)
            {
                //LogThis("Arguments " + arg);
                switch (arguments[argIndex].ToUpper())
                {
                    case "/?":
                        ShowSyntax();
                        Globals.RUNTIME_ERROR = ResultCodes.Error;
                        break;
                    case "-R": // Returns code only, doesn't expects a value after this switch
                        return_code_only = true;
                        Globals.PING_COUNT_VALUE_USER_SPECIFIED = 1;
                        Globals.MAX_COUNT_USER_SPECIFIED = true;
                        break;
                    case "-V":
                        Globals.PROGRAM_VERBOSE_LEVEL1 = true;
                        break;
                    case "-VV":
                        Globals.PROGRAM_VERBOSE_LEVEL2 = true;
                        break;
                    case "-I":
                        Globals.PING_ALL_IP_ADDRESSES = true;
                        break;
                    case "-S": // Make pinger like ping and output every responses to screen
                        Globals.ENABLE_CONTINEOUS_PINGS = true;
                        break;
                    case "-N": // No loop, same as using '-c 1'
                        Globals.PING_COUNT_VALUE_USER_SPECIFIED = 1;
                        Globals.MAX_COUNT_USER_SPECIFIED = true;
                        //loop = false;
                        break;
                    case "-Q": // quietens audible sounds (beeps)
                        Globals.SILENCE_AUDIBLE_ALARM = true;
                        break;
                    case "-F": // overrides pinger from sleeping 1 second betyween a ping response and the next ping. This will increase the ping  withing a second. 
                        Globals.FORCE_SLEEP = false;
                        break;
                    case "-C": // Specify how many times pinger will loop ping a host, expects a positive value after the switch equal or greater than 1                        
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                                        //bool success = int.TryParse(arguments[argIndex], out sleeptime);
                            Globals.MAX_COUNT_USER_SPECIFIED = true;
                            Globals.PING_COUNT_VALUE_USER_SPECIFIED = int.Parse(arguments[argIndex]);
                        }
                        catch (System.ArgumentNullException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.FormatException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.OverflowException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        break;
                    case "-D": // Run pinger for a number of hours, expects a positive value after the switch
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                                        //bool success = int.TryParse(arguments[argIndex], out sleeptime);
                            Globals.DURATION_VALUE_IN_DECIMAL = double.Parse(arguments[argIndex]);
                            Globals.DURATION_USER_SPECIFIED = true;
                            // Convert numnber to timespan
                            Globals.DURATION_TIMESPAN = TimeSpan.FromHours(Globals.DURATION_VALUE_IN_DECIMAL);
                            Globals.DURATION_END_DATE = DateTime.Now.Add(Globals.DURATION_TIMESPAN);
                            Globals.MAX_COUNT_USER_SPECIFIED = false;

                            //loop = true;
                        }
                        catch (System.ArgumentNullException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.FormatException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.OverflowException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        break;
                    case "-CSV": // Output each ping responses to a CSV file but matches onscreen output.
                                 //verbose = false;
                        Globals.OUTPUT_SCREEN_TO_CSV = true;
                        Globals.OUTPUT_ALL_TO_CSV = !Globals.OUTPUT_SCREEN_TO_CSV;
                        break;
                    case "-CSVALL": // Output each ping responses to a CSV file even if you are using the ENABLE_CONTINEOUS_PINGS function
                                    //verbose = false;
                        Globals.OUTPUT_SCREEN_TO_CSV = false;
                        Globals.OUTPUT_ALL_TO_CSV = !Globals.OUTPUT_SCREEN_TO_CSV;
                        break;

                    case "-P": // Poll every 'n' seconds, expects a value after this switch
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                                        //bool success = int.TryParse(arguments[argIndex], out sleeptime);
                            Globals.SLEEP_IN_USER_REQUESTED_IN_SECONDS = int.Parse(arguments[argIndex]);
                            Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS = Globals.SLEEP_IN_USER_REQUESTED_IN_SECONDS * 1000;
                        }
                        catch (System.ArgumentNullException)
                        {
                            LogThis("Please specify a valid polling interval in seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.FormatException)
                        {
                            LogThis("Please specify a valid polling interval in seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.OverflowException)
                        {
                            LogThis("Please specify a valid polling interval in seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            LogThis("Please specify a valid polling interval in seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        break;
                    case "-SKIPDNSLOOKUP": // skip DNS lookups
                        Globals.SKIP_DNS_LOOKUP = true;
                        break;
                    case "-DNSONLY": // skip DNS lookups
                        Globals.PING_ONLY_DNS_RESOLVABLE_TARGETS = true;
                        break;
                    case "-DNSSERVER": // skip DNS lookups
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            Globals.DNS_SERVER = arguments[argIndex];
                        }
                        catch (System.ArgumentNullException)
                        {
                            LogThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        break;
                    case "-T": // smart switch
                               // Show OK and Down and OK, implies -C
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            timeoutsec = int.Parse(arguments[argIndex]);
                            if (timeoutsec == 0)
                            {
                                timeoutsec = 1;
                            }
                            timeoutms = timeoutsec * 1000; // convert to millisecomnds
                        }
                        catch (System.ArgumentNullException)
                        {
                            LogThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.FormatException)
                        {
                            LogThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.OverflowException)
                        {
                            LogThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            LogThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        break;
                    case "-IPV4": // ipv4 IP Addresses only
                        Globals.IPV4_ONLY_IF = true;
                        break;
                    case "-IPV6": // ipv4 IP Addresses only
                        Globals.IPV6_ONLY_IF = true;
                        break;
                    default:
                        if (items == 0)
                        {
                            inputTargetList = arguments[argIndex];
                            Globals.ENABLE_CONTINEOUS_PINGS = false;
                        }
                        items++;
                        break;
                }
            }

            //if (Globals.RUNTIME_ERROR != ResultCodes.Ok)
            //return 1;//Globals.RUNTIME_ERROR;

            if (items > 1 || inputTargetList.Length <= 0)
            {
                ShowSyntax();
                Globals.RUNTIME_ERROR = ResultCodes.Error;
            }
            else
            {
                /*
                 * Show GLobal Variables
                 */
                //PrintOutGlobalVariables();

                /* 
                 * STEP 1: BUILD UP THE LIST OF SYSTEMS SPECIFIED BY USER
                 */
                // Determine the list of hosts to ping 
                LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                LogThisVerbose("[" + MYFUNCTION + "] STEP 1: DNS Lookup user host list  ");
                LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                string[] arrTargets;
                List<string> inputTargetListFiltered = new List<string>(); ; // need to work out why this is there.
                foreach (string targetHostname in inputTargetList.Split(','))
                {
                    inputTargetListFiltered.Add(targetHostname);
                }

                arrTargets = inputTargetListFiltered.Distinct().ToArray();

                if (Globals.PROGRAM_VERBOSE_LEVEL2)
                {
                    //LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++++++++++++++++ ");
                    LogThisVerbose("[" + MYFUNCTION + "] User specified Targets:");
                    foreach (string str in arrTargets)
                    {
                        LogThisVerbose("[" + MYFUNCTION + "]       : " + str);
                    }
                }
                if (Globals.IPV4_ONLY_IF)
                {
                    LogThisVerbose("[" + MYFUNCTION + "] User Requested pinger on IPv4 Records only");
                }

                if (Globals.IPV6_ONLY_IF)
                {
                    LogThisVerbose("[" + MYFUNCTION + "] User Requested pinger on IPv6 Records only");
                }

                /* 
                 * STEP 2: ITERATE and ATTEMPT TO DNS Lookup all. Process the results. 
                 * Ignore hostnames that do not have IP addresses resolvable
                 * If the user specifies for IPV4 only, then filter out non-IPv4 addresses
                 * bad hostname lookups and ipv6
                 * Output an array of DnsHostObjects for processing into a pinger target list
                 * Note the the 
                */
                // The list of final dns records for processing
                LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                LogThisVerbose("[" + MYFUNCTION + "] STEP 2: Filtering DnsHostObjects    ");
                LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                
                string subFunction = "Foreach";
                // Iterate through the targets by 1 DNS lookup, and 2 add finalise list into listDnsHostsObjects
                int arrTargetsIndex = 0;
                int dnsRecordsIndex = 0;
                foreach (string hostname in arrTargets)
                {
                    DateTime startTime = DateTime.Now;
                    //LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++++++++++++++++ ");
                    LogThisVerbose("[" + MYFUNCTION + "] Looking up [ " + hostname + " ] ");

                    // What if we 1) Attempt to resolve, 2) if you can't just ping by IP
                    // For each target, check if we need to do a DNS lookup first - -Globals.SKIP_DNS_LOOKUP is enabled or node 
                    if (Globals.SKIP_DNS_LOOKUP == false)
                    {
                        // User wants to perform a DNS lookup using the system DNS server
                        //string[] dnsresults = DnsLookup(hostname);
                        subFunction = "DnsLookup";
                        DnsHostObject hostLookupResults = DnsLookupNew(hostname);
                        hostLookupResults.LookupString = hostname;
                        if (Globals.PROGRAM_VERBOSE_LEVEL2)
                        {
                            LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Calling hostLookupResults.Printout()");
                            //LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  ++++++++++++++++++++++++++++++++++++");
                            hostLookupResults.Printout();
                            LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  ++++++++++++++++++++++++++++++++++++");
                        }
                        /*
                            ITERATE THROUGH THE LIST OF DnsHostObjects to use as input for pinger's list
                        */
                        
                        subFunction = "Filtering Records";
                        if (
                            (hostLookupResults.DnsLookUpCode == ResultCodes.Ok) ||
                            (
                                (hostLookupResults.DnsLookupType == DnsLookupByCodes.ByIP) &&
                                (hostLookupResults.DnsLookUpCode == ResultCodes.Error) &&
                                !Globals.PING_ONLY_DNS_RESOLVABLE_TARGETS
                            )
                        )
                        {
                            // This means that the Resolution (Forward or reverse were successfull)
                            // Iterate through the <ibvj>.IPAddresses
                            // Filter IPV4 if requested, and finalise the list
                            //LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   Returned IP [" + ipIndex + "] = " + ip.ToString() + "][" + ip.AddressFamily + "]");
                            int recordIndex = 0;
                            bool tryagain = true; // If you need the 1st IP of IPV4 or IPV6, but DNS returns the opposite, you want to continue until you find a match
                            foreach (System.Net.IPAddress addr in hostLookupResults.IPAddresses)
                            {
                                LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Exporting IP addresses for " + hostLookupResults.LookupString);;
                                DnsHostObject tmpDnsHostsObject = new DnsHostObject(hostLookupResults.LookupString);
                                tmpDnsHostsObject.DnsResolvedHostname = hostLookupResults.DnsResolvedHostname;
                                tmpDnsHostsObject.DnsLookUpMessage = hostLookupResults.DnsLookUpMessage;
                                tmpDnsHostsObject.DnsLookUpCode = hostLookupResults.DnsLookUpCode;
                                tmpDnsHostsObject.DnsLookupType = hostLookupResults.DnsLookupType;
                                tmpDnsHostsObject.Index = dnsRecordsIndex;
                                tmpDnsHostsObject.IPAddresses = new System.Net.IPAddress[] { addr };
                                // By default include it in the list unless there is a reason not to
                                tmpDnsHostsObject.Skip = false;

                                //if (Globals.IPV4_ONLY_IF || Globals.IPV6_ONLY_IF)
                                //{
                                if ((Globals.IPV4_ONLY_IF && (addr.AddressFamily != AddressFamily.InterNetwork)))
                                {
                                    //LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Excluding Record [" + hostLookupResults.LookupString + " if=" + recordIndex + " [" + addr.ToString() + "/" + addr.AddressFamily + "]");
                                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Excluding Record - Want IPv4 instead have if=" + recordIndex + " [" + addr.ToString() + "/" + addr.AddressFamily + "]");
                                    tmpDnsHostsObject.Skip = true;
                                    tryagain = true;
                                }
                                else if ((Globals.IPV6_ONLY_IF && (addr.AddressFamily != AddressFamily.InterNetworkV6)))
                                {
                                    //LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Including IPv6 Record [" + hostLookupResults.LookupString + " if=" + recordIndex + " [" + addr.ToString() + "/" + addr.AddressFamily + "]");
                                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Excluding Record - Want IPv6 instead have if=" + recordIndex + " [" + addr.ToString() + "/" + addr.AddressFamily + "]");
                                    tmpDnsHostsObject.Skip = true;
                                    tryagain = true;
                                }
                                else
                                {
                                    // We don't get which interface we are grabbing first. Which ever DNS resolve reports back first.
                                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Including Record [" + addr.ToString() + "]");
                                    hostLookupResults.Skip = false;
                                    tryagain = false;
                                }
                                // If the ip address is 0.0.0.0 or ::0, ignore it
                               
                                     
                                if ( (addr.ToString() == "0.0.0.0") || (addr.ToString() == "::0"))
                                {
                                     tmpDnsHostsObject.Skip = true;
                                      LogThisVerbose ("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                                    LogThisVerbose ("IP address = " + addr.ToString());
                                    LogThisVerbose ("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                                   
                                }
                                // 
                                if (tryagain == false)
                                {
                                    listDnsHostsObjects.Add(tmpDnsHostsObject);
                                }

                                if (!Globals.PING_ALL_IP_ADDRESSES && tryagain == false)
                                {
                                    // When user choses Globals.PING_ALL_IP_ADDRESSES=true, we find the 1st interface only. otherwise we grab all DNS IP addresses 
                                    break;
                                }
                                dnsRecordsIndex++;
                                recordIndex++;
                            }
                        }
                        else // if ((hostLookupResults.DnsLookUpCode == ResultCodes.Error) && (hostLookupResults.DnsLookupType == DnsLookupByCodes.ByName))
                        {
                            hostLookupResults.Skip = true;
                            hostLookupResults.Index = dnsRecordsIndex;
                            listDnsHostsObjects.Add(hostLookupResults);
                            // This means that the user requested the lookup of an Name AND DNS resolution failed, we skip it
                            LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "][Catch all] Record Will Skipped [" + hostLookupResults.LookupString + "]");
                            dnsRecordsIndex++;
                        }
                    }
                    else
                    {
                        // User requested to skip DNS lookups
                        DnsHostObject tmpDnsHostsObject = new DnsHostObject(hostname);
                        tmpDnsHostsObject.LookupString = hostname;
                        tmpDnsHostsObject.DnsLookUpCode = ResultCodes.Error;
                        tmpDnsHostsObject.DnsLookUpMessage = "DNS resolution skipped";
                        tmpDnsHostsObject.Index = dnsRecordsIndex;
                        tmpDnsHostsObject.Skip = false;
                        listDnsHostsObjects.Add(tmpDnsHostsObject);
                        dnsRecordsIndex++;
                    }
                    arrTargetsIndex++;
                } /// End foreach hostnames

                /* 
                * STEP 3: Process the list of listDnsHostsObjects and convert into a anrray of ping targets that pinger can process
                */
                // all the records can be processed without filtering
                LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                LogThisVerbose("[" + MYFUNCTION + "] STEP 3: Generating PingerTarget arrays");
                LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                subFunction = "PingTargetList";
                string previousLookupString = "";
                int interfaceIndexDefaultStart = 0;
                int interfaceIndex = interfaceIndexDefaultStart;
                int recordsIndex = 0;
                // Expecting listDnsHostsObjects to be ordered by LookupString
                LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Generating Pinger Target list from " + listDnsHostsObjects.Count + " objects");
                foreach (DnsHostObject dnsRecord in listDnsHostsObjects)//.OrderBy(q => q.LookupString).ToList())
                {
                    subFunction = "foreach";
                    PingerTarget currentHostInterface = new PingerTarget(dnsRecord.LookupString, dnsRecord.Index);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Object " + recordsIndex + ": " + dnsRecord.LookupString);
                    // Find if there are duplicates that we need to be ware off

                    if (dnsRecord.DnsLookUpCode == ResultCodes.Ok)
                    {
                        if (dnsRecord.DnsResolvedHostname != null)
                        {
                            currentHostInterface.DisplayName = dnsRecord.DnsResolvedHostname;
                        }
                        if (dnsRecord.IPAddresses != null)
                        {
                            LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]           IP: " + dnsRecord.IPAddresses[0].ToString());
                        }
                    }
                    else
                    {
                        currentHostInterface.DisplayName = dnsRecord.LookupString;
                    }
                    if (previousLookupString == dnsRecord.LookupString)
                    {
                        // The record.LookupString has multiple IPs to ping, therefore up the interface Index number for visual display
                        interfaceIndex++;
                        currentHostInterface.DisplayName += "-IP-" + interfaceIndex;
                    }

                    currentHostInterface.Skip = dnsRecord.Skip;
                    currentHostInterface.DnsLookupStatus = dnsRecord.DnsLookUpCode;
                    currentHostInterface.DnsLookupMessage = dnsRecord.DnsLookUpMessage;
                    if (dnsRecord.DnsResolvedHostname != null)
                    {
                        currentHostInterface.DnsResolvedHostname = dnsRecord.DnsResolvedHostname;
                    }

                    //dnsRecord.Printout();
                    if (dnsRecord.IPAddresses != null)
                    {
                        LogThisVerbose(">>> There is an IP");
                        currentHostInterface.IPAddress = dnsRecord.IPAddresses[0];
                    }
                    else
                    {
                        LogThisVerbose(">>> There is NO IP");
                    }
                    LogThisVerbose(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
                    /*if (record.DnsResolvedHostname != null)
                    {
                        currentHostInterface.DnsResolvedHostname = record.DnsResolvedHostname;
                        currentHostInterface.DisplayName = displayName;
                    }
                    currentHostInterface.IPAddress = record.IPAddresses[0];
                    currentHostInterface.Skip = record.Skip;
                    */
                    if (Globals.PROGRAM_VERBOSE_LEVEL2)
                    {
                        LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  START ________________________________________");
                        currentHostInterface.Printout();
                        LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  END ________________________________________");
                    }
                    //
                    if (currentHostInterface != null)
                    {
                        // if listOfUserRequestedTargets does not already contains an entry with the same Displayname, then add it
                        PingerTarget result = listOfUserRequestedTargets.Find(x => x.DisplayName == currentHostInterface.DisplayName);
                        if (result == null)
                        {
                            LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Adding " + currentHostInterface.DisplayName + " to listOfUserRequestedTargets");
                            listOfUserRequestedTargets.Add(currentHostInterface);
                        }
                        else
                        {
                            LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  DUPLICATE: " + currentHostInterface.DisplayName + "already exists in listOfUserRequestedTargets");
                        }
                    }
                    previousLookupString = dnsRecord.LookupString;
                    recordsIndex++;
                }
            }
            /* 
                STEP 4: Creating the final list of Targets");
            */
            LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
            LogThisVerbose("[" + MYFUNCTION + "] STEP 4: Creating Final list of targets ");
            LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
            int pingerTargetsIindex = 1;
            LogThisVerbose("[" + MYFUNCTION + "] Generating FINAL LIST TO PING from " + listOfUserRequestedTargets.Count + " objects");
            foreach (PingerTarget tmpPingerT in listOfUserRequestedTargets)
            {
                if (!tmpPingerT.Skip)
                {
                    if (!Globals.SKIP_DNS_LOOKUP && tmpPingerT.IPAddress != null && (tmpPingerT.IPAddress.ToString() != "0.0.0.0") && (tmpPingerT.IPAddress.ToString() != "::0"))
                    {
                        LogThisVerbose("[" + MYFUNCTION + "] Target " + pingerTargetsIindex + ": " + tmpPingerT.DisplayName + "(" + tmpPingerT.IPAddress.ToString() + ")(skip=" + tmpPingerT.Skip + ")");
                    }
                    else
                    {
                        LogThisVerbose("[" + MYFUNCTION + "] Target " + pingerTargetsIindex + ": " + tmpPingerT.DisplayName + "(skip=" + tmpPingerT.Skip + ")");
                    }
                    tmpPingerT.TargetIndex = pingerTargetsIindex;
                    pingableTargetList.Add(tmpPingerT);
                }
                pingerTargetsIindex++;
            }

            // Print out the headers
            LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
            LogThisVerbose("[" + MYFUNCTION + "] STEP 5: Generating Headers          ");
            LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
            bool showHeaders = true;
            // Need to remove duplicates
            if (listOfUserRequestedTargets.Count > 0 && showHeaders)
            {
                if (Globals.PROGRAM_VERBOSE_LEVEL1)
                {
                    LogThis(listOfUserRequestedTargets.Count + " hosts, " + Globals.SLEEP_IN_USER_REQUESTED_IN_SECONDS + "sec intervals, ttl=" + Globals.DEFAULT_PING_TIME_TO_LEAVE + ", RoundTripMaxTimeout " + timeoutsec + " sec");

                    int hostIndex = 0;
                    foreach (PingerTarget target in listOfUserRequestedTargets)
                    {
                        hostIndex = target.TargetIndex;
                        LogThisVerbose("[" + MYFUNCTION + "] " + hostIndex + "/" + listOfUserRequestedTargets.Count + " tmpHostObject.Target [" + target.DisplayName + "]");
                        /*
                         * If the server is marked for Skip, it means that there is a host lookup issue and should be ignored.
                         * In this case, we print out the hostname name of the server as input by the user
                         */
                        string outputString = "Target " + hostIndex + ": " + target.DisplayName;

                        // if you have a IP addresses
                        if ((target.DnsLookupStatus == ResultCodes.Ok) && (target.IPAddress != null))
                        {
                            //outputString += " (" + target.IPAddress.ToString() + "/" + target.IPAddress.AddressFamily + ")";
                            outputString += " (" + target.IPAddress.ToString() + ")";
                        }
                        else if (target.DnsLookupStatus == ResultCodes.Error)
                        {
                            outputString += " (" + target.DnsLookupMessage + ")";

                        }
                        if (target.Skip == true)
                        {
                            LogThisVerbose("[" + MYFUNCTION + "] Skipping this object");
                            outputString += " <- Skipping";
                        }
                        else
                        {
                            if (target.DnsLookupStatus == ResultCodes.Ok)
                            {
                                outputString += " DnsOK";
                            }
                        }
                     
                        LogThis(outputString);//outputString.Replace("InterNetworkV6", "IPv6").Replace("InterNetwork", "IPv4"));
                        LogThisVerbose(outputString);
                        hostIndex++; //potentially useless as replace - need to test out
                    }
                }


                LogThisVerbose("[" + MYFUNCTION + "] Globals.MAX_COUNT_USER_SPECIFIED = [" + Globals.MAX_COUNT_USER_SPECIFIED + "] Globals.DURATION_USER_SPECIFIED [" + Globals.DURATION_USER_SPECIFIED + "]");
                if (Globals.MAX_COUNT_USER_SPECIFIED && Globals.DURATION_USER_SPECIFIED)
                {
                    LogThis(">> Both count (-c) and duration (-d) are set. Duration will be used");
                }
                if (Globals.MAX_COUNT_USER_SPECIFIED && Globals.PING_COUNT_VALUE_USER_SPECIFIED > 1)
                {
                    LogThisVerbose("[" + MYFUNCTION + "] >> Globals.MAX_COUNT_USER_SPECIFIED is set to " + Globals.MAX_COUNT_USER_SPECIFIED + " and Globals.PING_COUNT_VALUE_USER_SPECIFIED is " + Globals.PING_COUNT_VALUE_USER_SPECIFIED);
                }

                if (Globals.DURATION_USER_SPECIFIED && Globals.DURATION_VALUE_IN_DECIMAL > 0)
                {
                    // = (DateTime.Now.AddHours(Globals.RUNTIME_IN_HOURS));
                    //string runtimeOutput = Globals.DURATION_TIMESPAN.ToString(@"dd\.hh\:mm\:ss");
                    LogThisVerbose("[" + MYFUNCTION + "] >> Globals.MAX_COUNT_USER_SPECIFIED is set and Globals.PING_COUNT_VALUE_USER_SPECIFIED > 0");
                    string runtimeOutput = "";
                    if (Globals.DURATION_TIMESPAN.Days > 0)
                    {
                        runtimeOutput += Globals.DURATION_TIMESPAN.Days + " days ";
                    }
                    if (Globals.DURATION_TIMESPAN.Hours > 0)
                    {
                        runtimeOutput += Globals.DURATION_TIMESPAN.Hours + " hrs ";
                    }
                    if (Globals.DURATION_TIMESPAN.Minutes > 0)
                    {
                        runtimeOutput += Globals.DURATION_TIMESPAN.Minutes + " mins ";
                    }
                    if (Globals.DURATION_TIMESPAN.Seconds > 0)
                    {
                        runtimeOutput += Globals.DURATION_TIMESPAN.Seconds + " sec ";
                    }
                    //LogThis(">> Runtime: " + runtimeOutput + ", Total ping expected=" + Globals.PING_COUNT_VALUE_USER_SPECIFIED + ", ETA "+ Globals.DURATION_END_DATE + " << ");
                    LogThis(">> Runtime: " + runtimeOutput + ", time to completion is " + Globals.DURATION_END_DATE + " << ");
                }

                /* 
                    STEP 7: Ping the available targets");
                */
                // List is in listOfUserRequestedTargets
                LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                LogThisVerbose("[" + MYFUNCTION + "] STEP 6: PINGER INTO ACTION - STARTING");
                LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                bool startping = true;
                if (startping && pingableTargetList.Count > 0)
                {
                    LogThisVerbose("[" + MYFUNCTION + "] " + pingableTargetList.Count + " Targets to pinger");
                    // The following loop will ping the list of hosts with '.Skip == true'
                    bool continueLoop = true;
                    int LOOP_PING_COUNT = 1;

                    do
                    {
                        //string screen;
                        DateTime DO_WHILE_LOOP_START_DATETIME = DateTime.Now;
                        LogThisVerbose("[" + MYFUNCTION + "] Loop Count " + LOOP_PING_COUNT + " on " + DO_WHILE_LOOP_START_DATETIME);

                        string subFunction = "PING";
                        LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] ");

                        //int foreachLoopIndex = 0;
                        //Console.Clear() ;
                        foreach (PingerTarget currPingTarget in pingableTargetList)
                        {
                            DateTime timer = DateTime.Now;
                            currPingTarget.CurrStatusPingDateCurrent = timer;
                            // mylist.Add(currPingTarget.IPAddress);
                            //                           AutoResetEvent waiter = new AutoResetEvent(false);
                            //                           Ping pingSenderObject = new Ping();
                            //                            pingSenderObject.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
                            PingOptions options = new PingOptions(Globals.DEFAULT_PING_TIME_TO_LEAVE, true);
                            //Probe(currPingTarget.IPAddress.ToString(), timeoutms, buffer, options);
                            Ping ping = new Ping();
                            Object userToken = new object();
                            PingReply pr;
                            string hostnameToDisplay = "";
                            if (currPingTarget.IPAddress != null && (currPingTarget.IPAddress.ToString() != "0.0.0.0"))
                           {
                                // LogThis("currPingTarget.IPAddress.ToString(): " + currPingTarget.IPAddress.ToString());
                                // LogThis(" timeoutms: " + timeoutms);
                                // LogThis(" buffer: " + buffer);
                                // LogThis(" options: " + options);
                                // LogThis(" userToken: " + userToken);
                                pr = await PingExtensions.SendTask(ping, currPingTarget.IPAddress.ToString(), timeoutms, buffer, options, userToken);
                                hostnameToDisplay = currPingTarget.IPAddress.ToString();

                            } else {
                                // LogThis("currPingTarget.LookupString: " + currPingTarget.LookupString);
                                // LogThis(" timeoutms: " + timeoutms);
                                // LogThis(" buffer: " + buffer);
                                // LogThis(" options: " + options);
                                // LogThis(" userToken: " + userToken);
                                pr = await PingExtensions.SendTask(ping, currPingTarget.LookupString, timeoutms, buffer, options, userToken);
                                hostnameToDisplay = currPingTarget.LookupString;
                            }                            
                            currPingTarget.CurrHostPingCount++;
                            //https://stackoverflow.com/questions/45150837/running-a-ping-sendasync-with-status-message

                            if (pr != null)
                            {
                                currPingTarget.CurrHostPingStatus = pr.Status;
                                
                                // Must update currHost despite the fact that we may not be printing anything on screent
                                string message = "";
                                string messageVerbose = "";
                                switch (pr.Status)
                                {
                                    case IPStatus.Success:
                                        messageVerbose = currPingTarget.DisplayName + Globals.SEPARATOR_CHAR + hostnameToDisplay + Globals.SEPARATOR_CHAR + Globals.SUCCESS_STATUS_STRING + Globals.SEPARATOR_CHAR +"RT=" + pr.RoundtripTime + "ms" + Globals.SEPARATOR_CHAR +"ttl=" + pr.Options.Ttl +  Globals.SEPARATOR_CHAR +"Frag=" + pr.Options.DontFragment +  Globals.SEPARATOR_CHAR + "replyBuffer=" + pr.Buffer.Length + Globals.SEPARATOR_CHAR +"count="+currPingTarget.CurrHostPingCount;
                                        message = Globals.SUCCESS_STATUS_STRING +"\t| " + currPingTarget.DisplayName + " ("+hostnameToDisplay+")" ;
                                        currPingTarget.HostReachableCount++;
                                        // If we enter into the =
                                        break;
                                    case IPStatus.TimedOut:
                                        messageVerbose = currPingTarget.DisplayName + Globals.SEPARATOR_CHAR + hostnameToDisplay + Globals.SEPARATOR_CHAR + Globals.TIMEDOUT_STATUS_STRING + Globals.SEPARATOR_CHAR + "RT=-"+ Globals.SEPARATOR_CHAR+ "ttl=-" +Globals.SEPARATOR_CHAR + "Frag=-" + Globals.SEPARATOR_CHAR+ "replyBuffer=-" + Globals.SEPARATOR_CHAR + "count="+currPingTarget.CurrHostPingCount;
                                        message = Globals.TIMEDOUT_STATUS_STRING + "\t| " + currPingTarget.DisplayName+ " ("+hostnameToDisplay+") ";
                                        currPingTarget.HostUnreachableCount++;
                                        
                                        break;
                                    default:
                                        messageVerbose = currPingTarget.DisplayName + Globals.SEPARATOR_CHAR + hostnameToDisplay + Globals.SEPARATOR_CHAR + Globals.OTHER_STATUS_STRING + Globals.SEPARATOR_CHAR + "RT=-"+ Globals.SEPARATOR_CHAR+ "ttl=-" +Globals.SEPARATOR_CHAR + "Frag=-" + Globals.SEPARATOR_CHAR+ "replyBuffer=-" + Globals.SEPARATOR_CHAR + "count="+currPingTarget.CurrHostPingCount;
                                        message = Globals.OTHER_STATUS_STRING + "\t| " + currPingTarget.DisplayName + " ("+ hostnameToDisplay +") ";
                                        currPingTarget.HostUnreachableCount++;
                                        break;
                                }
                            
                                // Output status updates to screen only when required
                                if (Globals.ENABLE_CONTINEOUS_PINGS || (currPingTarget.CurrHostPingStatus != currPingTarget.PrevHostPingStatus))
                                {
                                    if ( (currPingTarget.CurrHostPingStatus == IPStatus.Success) && (currPingTarget.CurrHostPingCount > 1 ))
                                    {
                                        TimeSpan duration = (currPingTarget.CurrStatusPingDateCurrent).Subtract(currPingTarget.PrevStatusPingDate);
                                        currPingTarget.HostUnreachableTimespan += duration;
                                        DateRange newDr = new DateRange();
                                        newDr.Index = currPingTarget.CurrHostPingCount;
                                        newDr.Start = currPingTarget.PrevStatusPingDate;
                                        newDr.End = currPingTarget.CurrStatusPingDateCurrent;
                                        currPingTarget.UnreachableDates.Add(newDr);
                                    } 
                                    string finalMessage;
                                    if (Globals.PROGRAM_VERBOSE_LEVEL1)
                                    {
                                        finalMessage = messageVerbose;
                                    } else {
                                        finalMessage = message+" ";
                                    }
                                    if(currPingTarget.CurrHostPingCount != 1)
                                    {
                                        if (currPingTarget.CurrHostPingCount > 1 && (currPingTarget.CurrHostPingStatus != currPingTarget.PrevHostPingStatus))
                                        {
                                         //   TimeSpan duration = (currPingTarget.CurrStatusPingDateCurrent).Subtract(currPingTarget.PrevStatusPingDate);
                                           
                                        }
                                        if (!Globals.ENABLE_CONTINEOUS_PINGS)
                                        {
                                            TimeSpan duration = (currPingTarget.CurrStatusPingDateCurrent).Subtract(currPingTarget.PrevStatusPingDate);
                                            string message2 = "(In previous state ["+currPingTarget.PrevHostPingStatus + "] for " + ToReadableString(duration) +")";
                                            finalMessage += message2;
                                        }
                                    } 

                                    LogThis(finalMessage);
                                    currPingTarget.PrevStatusPingDate = currPingTarget.CurrStatusPingDateCurrent;
                                }
                                currPingTarget.PrevHostPingStatus = currPingTarget.CurrHostPingStatus;
                            } else {
                                LogThis(">>> Or here");
                            }
                           // screen += finalMessage;
                        }
                       

                        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        // Based on how long since the foreach command took to ping every hosts, recalculate how long to sleep this thred for.
                        // For large numbers, generally the script doesn't sleep, but for small numbers, it may sleep.
                        DateTime DO_WHILE_LOOP_END_DATETIME = DateTime.Now;
                        TimeSpan DO_WHILE_LOOP_DATETIME_DIFFERENCE = DO_WHILE_LOOP_START_DATETIME.Subtract(DO_WHILE_LOOP_END_DATETIME);
                        int timelapsInMilliseconds = ((int)Math.Ceiling(DO_WHILE_LOOP_DATETIME_DIFFERENCE.TotalMilliseconds));

                        // This is hard to make sense when are a lot of nodes to ping
                        if (Globals.FORCE_SLEEP)// && (pingTargets.Length == 1))
                        {
                            int sleepRequiredInMilliseconds = Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS + timelapsInMilliseconds;

                            if (sleepRequiredInMilliseconds > 0)// (Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS - timelapsInMilliseconds) > 0)
                            {
                                LogThisVerbose("\t\tGlobals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS=" + Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS);
                                LogThisVerbose("\t\tCalculated value of timelapsInMilliseconds = " + timelapsInMilliseconds);
                                LogThisVerbose("\t\tSleeping for " + sleepRequiredInMilliseconds + " miliseconds");
                                Thread.Sleep(sleepRequiredInMilliseconds);
                            }
                            else
                            {
                                LogThisVerbose("\t\tGlobals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS=" + Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS);
                                LogThisVerbose("\t\tCalculated value of timelapsInMilliseconds = " + timelapsInMilliseconds);
                                LogThisVerbose("\t\ttimelapsInMilliseconds to large (" + sleepRequiredInMilliseconds + "ms)" + ", no need to sleep");
                            }
                        }

                        // If User requested to ping by count
                        if (Globals.MAX_COUNT_USER_SPECIFIED)
                        {
                            Globals.PING_COUNT_RUNTIME_VALUE++;
                            LogThisVerbose("[" + MYFUNCTION + "] " + Globals.PING_COUNT_RUNTIME_VALUE + " / " + Globals.PING_COUNT_VALUE_USER_SPECIFIED + " pings completed       ", Globals.PRINT_NEW_LINE);

                            // If ping count specified check that it doesn't exceed the user request number of ping count
                            if (Globals.PING_COUNT_RUNTIME_VALUE >= Globals.PING_COUNT_VALUE_USER_SPECIFIED)
                            {
                                // Ping count has been met or exceed, break out of the loop to stop pinging
                                continueLoop = false;
                                LogThisVerbose("");
                                LogThisVerbose("[END]           Max count reached - Exiting program.");
                            }
                            else
                            {
                                // keep looping because we are not done yet. 
                                continueLoop = true;
                            }
                        }
                        if (Globals.DURATION_USER_SPECIFIED)
                        {
                            DateTime thisDate = DateTime.Now;
                            double result = (Globals.DURATION_END_DATE - thisDate).TotalHours;// / 150;
                            //LogThisVerbose("[END] result=" + result + ", Globals.DURATION_END_DATE=" + Globals.DURATION_END_DATE, Globals.PRINT_NEW_LINE);
                            LogThisVerbose("[END] LOOP_PING_COUNT=" + LOOP_PING_COUNT + ", Globals.DURATION_END_DATE=" + Globals.DURATION_END_DATE + ", current Time=" + thisDate, Globals.PRINT_NEW_LINE);
                            if (result < 0)
                            {
                                //Do your business logic for expiring token
                                continueLoop = false;
                                LogThisVerbose("");
                                LogThisVerbose("[END] Timer down to zero - Exiting program.");
                            }
                        }
                        LOOP_PING_COUNT++;

                    } while (continueLoop); // End do Loop
                }
                else
                {
                    LogThisVerbose("[" + MYFUNCTION + "]  There were no objects to ping. Exiting..");
                }
            }
            else
            {
                LogThisVerbose("[" + MYFUNCTION + "]  There were no objects to ping. Exiting..");
            }
        }

        public static void ShowSummary(List<PingerTarget> targets)
        {
            LogThis ("\n--- pinger statistics ---");
            foreach (PingerTarget currPingTarget in targets)
            {
                // Packet sent
                string pingCount = currPingTarget.CurrHostPingCount + " packets transmitted, ";
                // Packets Lost
                string pingCountLost;
                string unreachableFor="";
                string percReach;
                double percReachValue = Math.Round(Convert.ToDouble(currPingTarget.HostUnreachableCount) / Convert.ToDouble(currPingTarget.CurrHostPingCount) * 100,2);
                if (currPingTarget.HostUnreachableCount > 0)
                {
                    if (currPingTarget.HostUnreachableCount != currPingTarget.CurrHostPingCount)
                    {
                        unreachableFor = "(Unreachable for a Total of "+ToReadableString(currPingTarget.HostUnreachableTimespan)+")";
                    }

                    pingCountLost = currPingTarget.HostUnreachableCount + " lost" +  unreachableFor; 
                    percReach = ", "+percReachValue + "% packet loss";
                    if (currPingTarget.IPAddress != null)
                    {
                        LogThis (currPingTarget.DisplayName+" ("+ currPingTarget.IPAddress.ToString() + "): " + pingCount + pingCountLost + percReach);
                    } else {
                         LogThis (currPingTarget.DisplayName+" ("+ currPingTarget.LookupString + "): " + pingCount + pingCountLost + percReach);
                    }
                    if (currPingTarget.HostUnreachableCount != currPingTarget.CurrHostPingCount)
                    {
                        LogThis ("\t: ----- Disconnection Report ------");
                    }
                    foreach (DateRange dr in currPingTarget.UnreachableDates)
                    {
                        TimeSpan duration = (dr.End).Subtract(dr.Start);
                        LogThis ("\t: ("+ ToReadableString(duration) +") Between " + dr.Start + " - " + dr.End);
                    }
                } else {
                    //pingCountLost = currPingTarget.HostUnreachableCount + " lost, ";
                    percReach = "0.0% loss";
                     if (currPingTarget.IPAddress != null)
                    {
                        LogThis (currPingTarget.DisplayName+" ("+ currPingTarget.IPAddress.ToString() + "): " + pingCount + percReach);
                    } else {
                        LogThis (currPingTarget.DisplayName+" ("+ currPingTarget.LookupString + "): " + pingCount + percReach);
                    }
                }
            }
        }
        public static void LogThisVerbose(string msg, bool newline = true)
        {
            string MYFUNCTION = "VERBOSE";
            if (Globals.PROGRAM_VERBOSE_LEVEL2 && newline == true)
            {
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("[" + MYFUNCTION + "]" + msg);
            }
            else if (Globals.PROGRAM_VERBOSE_LEVEL2 && newline == false)
            {
                //   Console.SetCursorPosition(1, 0);
                Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("\r{0}           ", "[" + MYFUNCTION + "]" + msg);
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }

        /// Function: LogThis
        /// Information: DDisplays on screen user messages
        /// </summary>
        public static void LogThis(string msg)
        {
            string MYFUNCTION = "LogThis";
            if (
                    msg.Contains(Globals.TIMEDOUT_STATUS_STRING) &&  !msg.Contains("["+Globals.TIMEDOUT_STATUS_STRING+"]")
                )
            {
                // Console.BackgroundColor = ConsoleColor.Black;
                // Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Red;
            }
             else if (
                    (msg.Contains(Globals.SUCCESS_STATUS_STRING) &&  !msg.Contains("["+Globals.SUCCESS_STATUS_STRING+"]")) ||
                    msg.Contains(Globals.SEPARATOR_CHAR + "OK") || 
                    msg.Contains("DnsOK") 
                )
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
            }
             else if (
                    (msg.Contains(Globals.OTHER_STATUS_STRING) &&  !msg.Contains("["+Globals.OTHER_STATUS_STRING+"]"))
                    )
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (msg.Contains("Skip") || msg.Contains("too long") || msg.Contains("BadDestination") || msg.Contains("Unknown") || msg.Contains("UnknownIP") || msg.Contains("Invalid") || msg.Contains("Could not resolve") || msg.Contains("not known") || msg.Contains("DestinationHostUnreachable") || msg.Contains("Unknown ping error code"))
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            if (
                    msg.Contains("[" + MYFUNCTION + "] ")
                )
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Red;
            }

            // Output
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
            Console.Write(msg);

            // Reset color back to normal
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop);
            Console.WriteLine("");
        }

        public static void logToFile(string msg, string filename)
        {
            string MYFUNCTION = "logToFile";
            using (StreamWriter w = File.AppendText(filename))
            {
                //Log(msg, w);
                w.WriteLine(msg);
            }
        }

        /** Function: DNSLookupNew
            Input:
                Hostname or IPv4 IPv6 IP Addresses
            Output:
                 DnsHostObject newObject
                    ---------------------------------
                    |   LookupString                |   User input, Hostname or IP
                    ---------------------------------
            --------------------------------------------------------------------------------------------------------------------------------
                    ---------------------------------
                    |   DnsLookupType                |  ByName or ByIP
                    ---------------------------------
                    ---------------------------------
                    |   DnsResolvedHostname         |   DNS Resolved Hostname using Reverse Lookup
                    ---------------------------------
                    ---------------------------------                   -----------------
                    |   IPAddresses                 | __________________|  IPAddress    |  DNS Resolved IP using Forward Lookup
                    ---------------------------------          |        -----------------
                                                               |        ------------------
                                                               |________|  AddressFamily | DNS Resolved IP using Forward Lookup
                                                                        ------------------
                    ---------------------------------
                    |   DnsLookUpMessage            | A message back regarding, Success or various failures
                    ---------------------------------
                    ---------------------------------
                    |   DnsLookUpCode               | DNS lookup succeeded. OK for Resolved or Error for failures 
                    ---------------------------------

*/
        // Hostname, IP Address, DNS Lookup Status"
        static DnsHostObject DnsLookupNew(string hostNameOrAddress)
        {
            // Define the object we can return
            DnsHostObject newObject = new DnsHostObject(hostNameOrAddress);

            string MYFUNCTION = "DnsLookupNew";
            object[] dnsResults;// = new string[] { "-", "-", "-" };
            IPHostEntry hostEntry = new();// IPHostEntry();
                                          // System.Net.IPAddress ipAddr;
            LogThisVerbose("[" + MYFUNCTION + "] Globals.SKIP_DNS_LOOKUP = " + Globals.SKIP_DNS_LOOKUP);
            LogThisVerbose("[" + MYFUNCTION + "] Globals.IPV4_ONLY_IF = " + Globals.IPV4_ONLY_IF);

            // Check if 'hostNameOrAddress' is an IP address
            LogThisVerbose("[" + MYFUNCTION + "] Checking if '" + newObject.LookupString + "' is an IP address");
            if (System.Net.IPAddress.TryParse(newObject.LookupString, out System.Net.IPAddress? address))
            {
                string subFunction = "ReverseLookup";
                newObject.DnsLookupType = DnsLookupByCodes.ByIP;

                // Important, if DNS Reverse Lookup fails, still build the IPAddresses (IP and IP Familly) as it is how pinger needs it

                System.Net.IPAddress hostEntryIPAddress = address;
                string hostEntryIPAddressString = hostEntryIPAddress.ToString();
                string hostEntryIPAddressFamilly = hostEntryIPAddress.AddressFamily.ToString();
                LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Is an IP [" + hostEntryIPAddress.AddressFamily.ToString() + "]Address ");
                try
                {
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    Attempting to reverse lookup");
                    hostEntry = Dns.GetHostEntry(newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    Success");
                    newObject.DnsResolvedHostname = hostEntry.HostName;
                    newObject.IPAddresses = hostEntry.AddressList;
                    newObject.Aliases = hostEntry.Aliases;
                    newObject.DnsLookUpMessage = "Reverse Lookup successfull";
                    newObject.DnsLookUpCode = ResultCodes.Ok;
                }
                catch (ArgumentNullException se)
                {
                    // Reverse lookup failed
                    subFunction = "ArgumentNullException";
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during reverse lookup of " + newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.IPAddresses = new System.Net.IPAddress[] { hostEntryIPAddress }; //New to create an array of IPs, since there is one
                                                                                               //newObject.DnsLookUpMessage = "The hostname parameter is null";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                    //newObject.ExceptionCategory = "ArgumentNullException";
                }
                catch (ArgumentOutOfRangeException se)
                {
                    // Reverse lookup failed
                    subFunction = "ArgumentOutOfRangeException";
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during reverse lookup of " + newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.IPAddresses = new System.Net.IPAddress[] { hostEntryIPAddress }; //New to create an array of IPs, since there is one
                                                                                               //newObject.DnsLookUpMessage = "The length of string is greater than 255 characters";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (System.Net.Sockets.SocketException se)
                {
                    // Reverse lookup failed
                    subFunction = "SocketException";
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during reverse lookup of " + newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.IPAddresses = new System.Net.IPAddress[] { hostEntryIPAddress }; //New to create an array of IPs, since there is one
                                                                                               //newObject.DnsLookUpMessage = "An error was encountered when resolving the hostNameOrAddress parameter.";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (ArgumentException se)
                {
                    // Reverse lookup failed
                    subFunction = "ArgumentException";
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during reverse lookup of " + newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.IPAddresses = new System.Net.IPAddress[] { hostEntryIPAddress }; //New to create an array of IPs, since there is one
                                                                                               //newObject.DnsLookUpMessage = "An error was encountered when resolving the hostNameOrAddress parameter.s";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }

                LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsResolvedHostname =" + newObject.DnsResolvedHostname);
                LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpMessage=" + newObject.DnsLookUpMessage);
                LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpCode = " + newObject.DnsLookUpCode);
                //LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   //newObject.DnsLookUpStatus = " + newObject.DnsLookUpStatus);
                int ipIndex = 1;
                foreach (System.Net.IPAddress ip in newObject.IPAddresses)
                {
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.IP[" + ipIndex + "] = " + ip.ToString() + "][" + ip.AddressFamily + "]");
                    ipIndex++;
                }
            }
            else // Forward Lookup by hostname
            {
                string subFunction = "ForwardLookup";
                newObject.DnsLookupType = DnsLookupByCodes.ByName;
                LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Performing a Lookup by name");
                LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] '" + newObject.LookupString + "' is not an IP address");

                try
                {
                    // attempt to forward lookup
                    //LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Attempting a Forward Lookup on " + newObject.LookupString);
                    hostEntry = Dns.GetHostEntry(newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "]    Success");
                    newObject.DnsLookUpCode = ResultCodes.Ok;
                    newObject.DnsLookUpMessage = "Success";
                    newObject.IPAddresses = hostEntry.AddressList;
                    newObject.DnsResolvedHostname = hostEntry.HostName;
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsResolvedHostname=" + newObject.DnsResolvedHostname);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpMessage=" + newObject.DnsLookUpMessage);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpCode=" + newObject.DnsLookUpCode);
                    //LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpStatus= " + newObject.DnsLookUpStatus);
                    int ipIndex = 1;
                    foreach (System.Net.IPAddress ip in newObject.IPAddresses)
                    {
                        LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.IP[" + ipIndex + "] = " + ip.ToString() + "][" + ip.AddressFamily + "]");
                        ipIndex++;
                    }
                    dnsResults = new object[] { hostEntry.HostName, hostEntry.AddressList, "Could not resolve by Name" };
                    newObject.DnsLookUpCode = ResultCodes.Ok;
                }
                /*catch (Exception se)
                {
                    // forward lookup failed for some reason
                    //LogThis("[Exception] message = " + se.Message);
                    // You w// IF YOU ARE IN HERE THEN THE REQUESTED HOSTNAME CAN NOT BE FOUND
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "][Exception] An Exception Caught during forward lookup of hostNameOrAddress");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "][Exception]   se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "][Exception]   se.Message = " + se.Message);
                    //dnsResults = new string[] { hostNameOrAddress, "-", se. };
                    newObject.DnsLookUpMessage = "Could not resolve";
                    newObject.DnsLookUpCode = ResultCodes.Error;
                    newObject.IPAddresses = hostEntry.AddressList;
                }*/
                catch (ArgumentNullException se)
                {
                    // Forward lookup failed
                    subFunction = "ArgumentNullException";
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during Forward lookup of " + newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.DnsResolvedHostname = "-";
                    // newObject.DnsLookUpMessage = "The hostname parameter is null";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (ArgumentOutOfRangeException se)
                {
                    // Forward lookup failed
                    subFunction = "ArgumentOutOfRangeException";
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during Forward lookup of " + newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    //newObject.DnsLookUpMessage = "The length of string is greater than 255 characters";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (System.Net.Sockets.SocketException se)
                {
                    // Forward lookup failed
                    subFunction = "SocketException";
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during Forward lookup of " + newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    //newObject.DnsLookUpMessage = "An error was encountered when resolving the hostNameOrAddress parameter";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (ArgumentException se)
                {
                    // Forward lookup failed
                    subFunction = "SocketException";
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during Forward lookup of " + newObject.LookupString);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    LogThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    //newObject.DnsLookUpMessage = "An error was encountered when resolving the hostNameOrAddress parameter.s";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }

            }
            return newObject;
        }
        public static string ToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        public static void PrintOutGlobalVariables()
        {
            string MYFUNCTION = "PrintOutGlobalVariables";
            LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++++++++++");
            LogThisVerbose("[" + MYFUNCTION + "] Global.PROGRAM_VERBOSE = " + Globals.PROGRAM_VERBOSE);
            LogThisVerbose("[" + MYFUNCTION + "] Global.PROGRAM_VERBOSE_LEVEL2 = " + Globals.PROGRAM_VERBOSE_LEVEL2);
            LogThisVerbose("[" + MYFUNCTION + "] Global.RUNTIME_ERROR = " + Globals.RUNTIME_ERROR);
            LogThisVerbose("[" + MYFUNCTION + "] Global.PING_COUNT_VALUE_USER_SPECIFIED = " + Globals.PING_COUNT_VALUE_USER_SPECIFIED);
            LogThisVerbose("[" + MYFUNCTION + "] Global.PING_COUNT_RUNTIME_VALUE = " + Globals.PING_COUNT_RUNTIME_VALUE);
            LogThisVerbose("[" + MYFUNCTION + "] Global.MAX_COUNT_USER_SPECIFIED = " + Globals.MAX_COUNT_USER_SPECIFIED);
            LogThisVerbose("[" + MYFUNCTION + "] Global.VERBOSE = " + Globals.VERBOSE);
            LogThisVerbose("[" + MYFUNCTION + "] Global.PING_ALL_IP_ADDRESSES = " + Globals.PING_ALL_IP_ADDRESSES);
            LogThisVerbose("[" + MYFUNCTION + "] Global.DURATION_USER_SPECIFIED = " + Globals.IPV4_ONLY_IF);
            LogThisVerbose("[" + MYFUNCTION + "] Global.ENABLE_CONTINEOUS_PINGS = " + Globals.ENABLE_CONTINEOUS_PINGS);
            LogThisVerbose("[" + MYFUNCTION + "] Global.SILENCE_AUDIBLE_ALARM = " + Globals.SILENCE_AUDIBLE_ALARM);
            LogThisVerbose("[" + MYFUNCTION + "] Global.FORCE_SLEEP = " + Globals.FORCE_SLEEP);
            LogThisVerbose("[" + MYFUNCTION + "] Global.RUNTIME_IN_HOURS = " + Globals.DURATION_VALUE_IN_DECIMAL);
            LogThisVerbose("[" + MYFUNCTION + "] Global.DURATION_END_DATE = " + Globals.DURATION_END_DATE);
            LogThisVerbose("[" + MYFUNCTION + "] Global.DURATION_TIMESPAN = " + Globals.DURATION_TIMESPAN);
            LogThisVerbose("[" + MYFUNCTION + "] Global.DURATION_USER_SPECIFIED = " + Globals.DURATION_USER_SPECIFIED);
            LogThisVerbose("[" + MYFUNCTION + "] Global.OUTPUT_SCREEN_TO_CSV = " + Globals.OUTPUT_SCREEN_TO_CSV);
            LogThisVerbose("[" + MYFUNCTION + "] Global.OUTPUT_ALL_TO_CSV = " + Globals.OUTPUT_ALL_TO_CSV);
            LogThisVerbose("[" + MYFUNCTION + "] Global.SKIP_DNS_LOOKUP = " + Globals.SKIP_DNS_LOOKUP);
            LogThisVerbose("[" + MYFUNCTION + "] Global.DNS_SERVER = " + Globals.DNS_SERVER);
            LogThisVerbose("[" + MYFUNCTION + "] Global.DEFAULT_POLLING_MILLISECONDS = " + Globals.DEFAULT_POLLING_MILLISECONDS);
            LogThisVerbose("[" + MYFUNCTION + "] Global.DEFAULT_TIMEOUT_MILLISECONDS = " + Globals.DEFAULT_TIMEOUT_MILLISECONDS);
            LogThisVerbose("[" + MYFUNCTION + "] Global.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS = " + Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS);
            LogThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++++++++++");
        }

        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        static public void ShowHeader()
        {
            // Assembly assembly = Assembly.GetExecutingAssembly();
            // FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            // string text = fvi.FileVersion;
            // // Type t = typeof(PingerTarget);
            // // MethodInfo[] x = t.GetMethods();
            // // foreach (MethodInfo xtemp in x) 
            // // {
            // //     Console.WriteLine(xtemp.ToString());
            // // }
            LogThis("ver. 2.3.0-20240209");
        }

        /// <summary>
        /// Function: ShowSyntax
        /// Information: Display usage information for the application
        /// </summary>
        static public void ShowSyntax()
        {
            // Display Application Syntax
            //string MYFUNCTION = "ShowSyntax";
            LogThis("Syntax  : Pinger <hosts> [OPTIONS]");
            // Display Return Codes Information
            //"\t-s:\tSmart switch. Pinger only shows pinger response \n\t\tif the current ping status is different to the last one \n"+                           
            LogThis("[HOSTS]: \n" +
                    "\tsingle or multiple hostnames,fqdn,ipv4, and ipv6 IP addresses. Must comma separate (no spaces).\n");
            LogThis("[Examples]: \n" +
                        "\tpinger google.com.au,fd8a:4d23:a340:4960:250:56ff:febb:a99d,192.168.0.1\n"
                    );
            LogThis("[OPTIONS]: \n" +
                             "\t-n:\tPinger runs once then exists\n" +
                             "\t-d <n>: Set the amount of duration in Decimal pinger runs for before exiting - Specify a positive value such as 0.25 for 15 minutes or 1.5 for 1hr20mins.\n" +
                             "\t-c <n>: Specify how many times pinger will poll before exiting - Specify a positive value 'n' greater than 1.\n" +
                             "\t-s:\tRuns like a Standard ping which prints every ping results onscreen.\n" +
                             "\t-p <n>:\tSpecify how often (in seconds) Pinger will poll the target. Useful with '-s'. Specify a positive value 'n' greater than 1.\n" +
                             "\t-t <n>:\tSet a Round Trip timeout value of 'n' seconds - Default value is 1 seconds. For high latency links above 4000ms latency, \n\t\tincrease this value above 4. When this value is reached, pinger will assume the target is unreachable.\n" +
                             "\t-q: \tMute default audible alarms. By default, pinger will beep when the status changes in the following instance.\n\t\t> 2 beeps when Status transitions from Timeout to Pingable\n\t\t> 4 beeps when Status transitions from Pingble to TimeOut\n" +
                             "\t-f: \tFastping makes pinger starts a new poll as soon it receives the previous response. Fastping is automatically \n\t\tactivated when the Round Trip is above 1 seconds. Use in combination with the '-s' switch.\n" +
                            //  "\t-csv: \tSaves all onscreen responses to a CSV. Does not yet take any arguments. The resultant CSV is prefixed with \n\t\tthe target name in your current directory.\n" +
                            //  "\t-csvall:Saves all ping results to a CSV even regardless what's onscreen. Useful when wanting only the differences in\n\t\tresults onscreen but all of the ping results in a CSV. \n\t\tThe resultant CSV is prefixed with the target name in your current directory.\n" +
                             "\t-skipDnsLookup: \tSkip DNS lookup.\n" +
                             "\t-dnsonly: \tPing DNS resolvable targets only from the list.\n" +
                             "\t-i: \tPing all IP addresses enumerated from the NSLOOKUP query.\n" +
                             "\t-ipv4: \tPing all IPv4 addresses only.\n" +
                             "\t-ipv6: \tPing all IPv6 addresses only.\n" +
                             "\t-v: \tVerbose Variables.\n" +
                             "\t-vv: \tVerbose Runtime.\n" +
                             "\t-r:\tReturn Code only. Pinger does verbose to screen (0=Pingable,1=failure).\n");
            // +
            //"\nReturn Codes:\n" +
            //"\t0\tSuccessfull Ping.\n" +
            //"\t1\tUnsuccessfull or other errors reported.\n");

            //"\nFuture feature:" + "\n" +
            //"\t-traceroute\tPerform a traceroute on failure" + "\n" +
            //"\t-webcheck <fullURL>:\tPerform a url check on failure" + "\n");
            // "\t-range 10.130.16 -from .110 -t .140 :

            LogThis("Examples: \n" +
                            "\tSmart ping server1, and only report when the status changes\n" +
                            "\t\t(Windows) pinger.exe server1\n" +
                            "\tPing these servers with minimum output info\n" +
                            "\t\t(MacOS) pinger server1,8.8.8.8,google.com.au\n" +
                            "\tPing these servers but show a bit more info\n" +
                            "\t\t(MacOS) pinger server1,8.8.8.8,google.com.au -v\n" +
                            "\tPing these servers, but skip initial DNS Lookup, and  show a bit more info\n" +
                            "\t\t(MacOS) pinger server1,8.8.8.8,google.com.au -skipDnsLookup -v\n" +
                            "\tPing only server1's Ipv6 interfaces\n" +
                            "\t\t(MacOS) pinger server1 -i -ipv6\n" +
                            "\tRun a standard ping on a single server 10 times\n" +
                            "\t\tpinger server1 -s -c 10\n" +
                            "\tRun a standard ping on a single server 10 times but verbose the output and stop the audible noise on status changes \n" +
                            "\t\tpinger.exe server1 -s -c 10 -v -q\n");
            ShowHeader();
        }
    }
}
