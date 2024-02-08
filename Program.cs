// pinger Utility
// use 'pinger.exe /?' for help
//
// Udpated for .NET Framework 4.5
//using System;
//using System.Linq;
//using System.Threading;
using System.Net.NetworkInformation;
using System.Text;
using System.Net;
using System.Xml.Serialization;
//using System.IO;
//using System.Reflection;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using System.Runtime.InteropServices;
//using System.IO.Pipes;
using System.Net.Sockets;
//using System.Net.Mail;
//using System.Runtime.ConstrainedExecution;
//using Microsoft.VisualBasic;
//using System.ComponentModel;
//using System.Threading;

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
            this.currHostPingStatus = IPStatus.Unknown;
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
        /*
                public PingerTarget()
                {
                    this._LookupString = "-";
                    this._DnsResolvedHostname = "-";
                    this._DisplayName = "-";
                    //this.dnsipaddr = "-";
                    this.dnsReplyIPaddr = "-";
                    this.pingReplyRoundTripInMiliSec = 0;
                    this.skip = false;
                    this._DnsLookupStatus = "-";
                    this.logFile = "-";
                    this.currHostPingStatus = IPStatus.Unknown;
                    this.prevHostPingStatus = IPStatus.Unknown;
                    this.errorMsg = "-";
                    this.optionsTtl = -1;
                    this.errorCode = -1; // No Errors
                    this.hostUnreachableCount = 0;
                    this.hostReachableCount = 0;
                    this.currHostPingCount = 0;
                    this.prevStatusPingCount = 0;
                    this.currStatusPingDateCurrent = DateTime.Now;
                    this.currStatusPingDatePrevious = DateTime.Now;
                    this.prevStatusPingDate = DateTime.Now;
                    //this._DnsLookupType = false;
                    this.pingByIP = false;
                    this.parentTarget = "-";
                }
        */

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

        public IPStatus CurrHostPingStatus
        {
            get { return currHostPingStatus; }
            set
            {
                prevHostPingStatus = currHostPingStatus;
                currHostPingStatus = value;
            }
        }
        public int HostReachableCountUpdate
        {
            get { return hostReachableCount; }
            set { hostReachableCount = value; }
        }
        public int HostUnreachableCountUpdate
        {
            get { return hostUnreachableCount; }
            set { hostUnreachableCount = value; }
        }

        public IPStatus PrevHostPingStatus
        {
            get { return prevHostPingStatus; }
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
                //logThis("Arguments " + arg);
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
                            logThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.FormatException)
                        {
                            logThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.OverflowException)
                        {
                            logThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            logThis("Please specify a valid number.");
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
                            logThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.FormatException)
                        {
                            logThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.OverflowException)
                        {
                            logThis("Please specify a valid number.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            logThis("Please specify a valid number.");
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
                            logThis("Please specify a valid polling interval in seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.FormatException)
                        {
                            logThis("Please specify a valid polling interval in seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.OverflowException)
                        {
                            logThis("Please specify a valid polling interval in seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            logThis("Please specify a valid polling interval in seconds.");
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
                            logThis("Please specify a valid number.");
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
                            logThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.FormatException)
                        {
                            logThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.OverflowException)
                        {
                            logThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            Globals.RUNTIME_ERROR = ResultCodes.Error;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            logThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
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
                            //ENABLE_CONTINEOUS_PINGS = false;
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
                logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                logThisVerbose("[" + MYFUNCTION + "] STEP 1: DNS Lookup user host list  ");
                logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                string[] arrTargets;
                List<string> inputTargetListFiltered = new List<string>(); ; // need to work out why this is there.
                foreach (string targetHostname in inputTargetList.Split(','))
                {
                    inputTargetListFiltered.Add(targetHostname);
                }

                arrTargets = inputTargetListFiltered.Distinct().ToArray();

                if (Globals.PROGRAM_VERBOSE_LEVEL2)
                {
                    //logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++++++++++++++++ ");
                    logThisVerbose("[" + MYFUNCTION + "] User specified Targets:");
                    foreach (string str in arrTargets)
                    {
                        logThisVerbose("[" + MYFUNCTION + "]       : " + str);
                    }
                }
                if (Globals.IPV4_ONLY_IF)
                {
                    logThisVerbose("[" + MYFUNCTION + "] User Requested pinger on IPv4 Records only");
                }

                if (Globals.IPV6_ONLY_IF)
                {
                    logThisVerbose("[" + MYFUNCTION + "] User Requested pinger on IPv6 Records only");
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
                logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                logThisVerbose("[" + MYFUNCTION + "] STEP 2: Filtering DnsHostObjects    ");
                logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                string subFunction = "Foreach";
                // Iterate through the targets by 1 DNS lookup, and 2 add finalise list into listDnsHostsObjects
                int arrTargetsIndex = 0;
                int dnsRecordsIndex = 0;
                foreach (string hostname in arrTargets)
                {
                    DateTime startTime = DateTime.Now;
                    //logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++++++++++++++++ ");
                    logThisVerbose("[" + MYFUNCTION + "] Looking up [ " + hostname + " ] ");

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
                            logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Calling hostLookupResults.Printout()");
                            //logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  ++++++++++++++++++++++++++++++++++++");
                            hostLookupResults.Printout();
                            logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  ++++++++++++++++++++++++++++++++++++");
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
                            //logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   Returned IP [" + ipIndex + "] = " + ip.ToString() + "][" + ip.AddressFamily + "]");
                            int recordIndex = 0;
                            bool tryagain = true; // If you need the 1st IP of IPV4 or IPV6, but DNS returns the opposite, you want to continue until you find a match
                            foreach (System.Net.IPAddress addr in hostLookupResults.IPAddresses)
                            {
                                logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Exporting IP addresses for " + hostLookupResults.LookupString);
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
                                    //logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Excluding Record [" + hostLookupResults.LookupString + " if=" + recordIndex + " [" + addr.ToString() + "/" + addr.AddressFamily + "]");
                                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Excluding Record - Want IPv4 instead have if=" + recordIndex + " [" + addr.ToString() + "/" + addr.AddressFamily + "]");
                                    tmpDnsHostsObject.Skip = true;
                                    tryagain = true;
                                }
                                else if ((Globals.IPV6_ONLY_IF && (addr.AddressFamily != AddressFamily.InterNetworkV6)))
                                {
                                    //logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Including IPv6 Record [" + hostLookupResults.LookupString + " if=" + recordIndex + " [" + addr.ToString() + "/" + addr.AddressFamily + "]");
                                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Excluding Record - Want IPv6 instead have if=" + recordIndex + " [" + addr.ToString() + "/" + addr.AddressFamily + "]");
                                    tmpDnsHostsObject.Skip = true;
                                    tryagain = true;
                                }
                                else
                                {
                                    // We don't get which interface we are grabbing first. Which ever DNS resolve reports back first.
                                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Including Record [" + addr.ToString() + "]");
                                    hostLookupResults.Skip = false;
                                    tryagain = false;
                                }
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
                            logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "][Catch all] Record Will Skipped [" + hostLookupResults.LookupString + "]");
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
                logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                logThisVerbose("[" + MYFUNCTION + "] STEP 3: Generating PingerTarget arrays");
                logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                subFunction = "PingTargetList";
                string previousLookupString = "";
                int interfaceIndexDefaultStart = 0;
                int interfaceIndex = interfaceIndexDefaultStart;
                int recordsIndex = 0;
                // Expecting listDnsHostsObjects to be ordered by LookupString
                logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Generating Pinger Target list from " + listDnsHostsObjects.Count + " objects");
                foreach (DnsHostObject dnsRecord in listDnsHostsObjects)//.OrderBy(q => q.LookupString).ToList())
                {
                    subFunction = "foreach";
                    PingerTarget currentHostInterface = new PingerTarget(dnsRecord.LookupString, dnsRecord.Index);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Object " + recordsIndex + ": " + dnsRecord.LookupString);
                    // Find if there are duplicates that we need to be ware off

                    if (dnsRecord.DnsLookUpCode == ResultCodes.Ok)
                    {
                        if (dnsRecord.DnsResolvedHostname != null)
                        {
                            currentHostInterface.DisplayName = dnsRecord.DnsResolvedHostname;
                        }
                        if (dnsRecord.IPAddresses != null)
                        {
                            logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]           IP: " + dnsRecord.IPAddresses[0].ToString());
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
                        logThisVerbose(">>> There is an IP");
                        currentHostInterface.IPAddress = dnsRecord.IPAddresses[0];
                    }
                    else
                    {
                        logThisVerbose(">>> There is NO IP");
                    }
                    logThisVerbose(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
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
                        logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  START ________________________________________");
                        currentHostInterface.Printout();
                        logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  END ________________________________________");
                    }
                    //
                    if (currentHostInterface != null)
                    {
                        // if listOfUserRequestedTargets does not already contains an entry with the same Displayname, then add it
                        PingerTarget result = listOfUserRequestedTargets.Find(x => x.DisplayName == currentHostInterface.DisplayName);
                        if (result == null)
                        {
                            logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  Adding " + currentHostInterface.DisplayName + " to listOfUserRequestedTargets");
                            listOfUserRequestedTargets.Add(currentHostInterface);
                        }
                        else
                        {
                            logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]  DUPLICATE: " + currentHostInterface.DisplayName + "already exists in listOfUserRequestedTargets");
                        }
                    }
                    previousLookupString = dnsRecord.LookupString;
                    recordsIndex++;
                }
            }

            /* 
                STEP 4: Creating the final list of Targets");
            */
            logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
            logThisVerbose("[" + MYFUNCTION + "] STEP 4: Creating Final list of targets ");
            logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
            int pingerTargetsIindex = 1;
            logThisVerbose("[" + MYFUNCTION + "] Generating FINAL LIST TO PING from " + listOfUserRequestedTargets.Count + " objects");
            foreach (PingerTarget tmpPingerT in listOfUserRequestedTargets)
            {
                if (!tmpPingerT.Skip)
                {
                    if (!Globals.SKIP_DNS_LOOKUP && tmpPingerT.IPAddress != null)
                    {
                        logThisVerbose("[" + MYFUNCTION + "] Target " + pingerTargetsIindex + ": " + tmpPingerT.DisplayName + "(" + tmpPingerT.IPAddress.ToString() + ")(skip=" + tmpPingerT.Skip + ")");
                    }
                    else
                    {
                        logThisVerbose("[" + MYFUNCTION + "] Target " + pingerTargetsIindex + ": " + tmpPingerT.DisplayName + "(skip=" + tmpPingerT.Skip + ")");
                    }
                    pingableTargetList.Add(tmpPingerT);
                }
                pingerTargetsIindex++;
            }

            // Print out the headers
            logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
            logThisVerbose("[" + MYFUNCTION + "] STEP 5: Generating Headers          ");
            logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
            bool showHeaders = true;
            // Need to remove duplicates
            if (listOfUserRequestedTargets.Count > 0 && showHeaders)
            {
                if (Globals.PROGRAM_VERBOSE_LEVEL1)
                {
                    logThis(listOfUserRequestedTargets.Count + " hosts, " + Globals.SLEEP_IN_USER_REQUESTED_IN_SECONDS + "sec intervals, ttl=" + Globals.DEFAULT_PING_TIME_TO_LEAVE + ", RoundTripMaxTimeout " + timeoutsec + " sec");

                    int hostIndex = 0;
                    foreach (PingerTarget target in listOfUserRequestedTargets)
                    {
                        hostIndex = target.TargetIndex;
                        logThisVerbose("[" + MYFUNCTION + "] " + hostIndex + "/" + listOfUserRequestedTargets.Count + " tmpHostObject.Target [" + target.DisplayName + "]");
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
                            logThisVerbose("[" + MYFUNCTION + "] Skipping this object");
                            outputString += " <- Skipping";
                        }
                        else
                        {
                            if (target.DnsLookupStatus == ResultCodes.Ok)
                            {
                                outputString += " DnsOK";
                            }
                        }
                        /*else
                        {
                            logThisVerbose("[" + MYFUNCTION + "] Not skipping this object");
                            if (target.DnsLookupStatus == null)
                            {
                                logThis("Target " + hostIndex + ": " + target.DisplayName + " (" + target.Skip + " " + target.IPAddress.ToString() + ")");
                            }
                            else
                            {
                                if (target.DnsLookupStatus.ToUpper() == "SUCCESS")
                                {
                                    logThis("Target " + hostIndex + ": " + target.DisplayName + " (" + target.IPAddress.ToString() + "/" + target.IPAddress.AddressFamily + ")");
                                }
                                else
                                {
                                    logThis("Target " + hostIndex + ": " + target.DisplayName + " (->" + target.DnsLookupStatus + ")");
                                }
                            }
                        }*/

                        logThis(outputString);//outputString.Replace("InterNetworkV6", "IPv6").Replace("InterNetwork", "IPv4"));
                        logThisVerbose(outputString);
                        hostIndex++; //potentially useless as replace - need to test out
                    }
                }

                /*if (Globals.FORCE_SLEEP) // if true, default behavior 
                {
                    logThis("Pinging the following " + hostnames.Length + " hosts at " + Globals.SLEEP_IN_USER_REQUESTED_IN_SECONDS + "sec interval (Round Trip timeout set at " + timeoutsec + " seconds)");
                }
                else // If Globals.FORCE_SLEEP = false, user choice to ignore sleep time, increasing ping requests within a 1 second period
                {
                    logThis("Pinging the following " + hostnames.Length + " hosts with timeout window of " + timeoutsec + " seconds");
                }*/
                logThisVerbose("[" + MYFUNCTION + "] Globals.MAX_COUNT_USER_SPECIFIED = [" + Globals.MAX_COUNT_USER_SPECIFIED + "] Globals.DURATION_USER_SPECIFIED [" + Globals.DURATION_USER_SPECIFIED + "]");
                if (Globals.MAX_COUNT_USER_SPECIFIED && Globals.DURATION_USER_SPECIFIED)
                {
                    logThis(">> Both count (-c) and duration (-d) are set. Duration will be used");
                }
                if (Globals.MAX_COUNT_USER_SPECIFIED && Globals.PING_COUNT_VALUE_USER_SPECIFIED > 1)
                {
                    logThisVerbose("[" + MYFUNCTION + "] >> Globals.MAX_COUNT_USER_SPECIFIED is set to " + Globals.MAX_COUNT_USER_SPECIFIED + " and Globals.PING_COUNT_VALUE_USER_SPECIFIED is " + Globals.PING_COUNT_VALUE_USER_SPECIFIED);
                }

                if (Globals.DURATION_USER_SPECIFIED && Globals.DURATION_VALUE_IN_DECIMAL > 0)
                {
                    // = (DateTime.Now.AddHours(Globals.RUNTIME_IN_HOURS));
                    //string runtimeOutput = Globals.DURATION_TIMESPAN.ToString(@"dd\.hh\:mm\:ss");
                    logThisVerbose("[" + MYFUNCTION + "] >> Globals.MAX_COUNT_USER_SPECIFIED is set and Globals.PING_COUNT_VALUE_USER_SPECIFIED > 0");
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
                    //logThis(">> Runtime: " + runtimeOutput + ", Total ping expected=" + Globals.PING_COUNT_VALUE_USER_SPECIFIED + ", ETA "+ Globals.DURATION_END_DATE + " << ");
                    logThis(">> Runtime: " + runtimeOutput + ", time to completion is " + Globals.DURATION_END_DATE + " << ");
                }

                /* 
                    STEP 7: Ping the available targets");
                */
                // List is in listOfUserRequestedTargets
                logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                logThisVerbose("[" + MYFUNCTION + "] STEP 6: PINGER INTO ACTION - STARTING");
                logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++");
                bool startping = true;
                if (startping && pingableTargetList.Count > 0)
                {
                    logThisVerbose("[" + MYFUNCTION + "] " + pingableTargetList.Count + " Targets to pinger");
                    // The following loop will ping the list of hosts with '.Skip == true'
                    bool continueLoop = true;
                    int LOOP_PING_COUNT = 1;


                    /* Build the List of IPs
                    List<IPAddress> mylist = new List<IPAddress>();
                    foreach (PingerTarget currPingTarget in pingableTargetList)
                    {
                        mylist.Add(currPingTarget.IPAddress);
                    }*/


                    //List<IPAddress> mylist = new List<IPAddress>();
                    do
                    {
                        DateTime DO_WHILE_LOOP_START_DATETIME = DateTime.Now;
                        logThisVerbose("[" + MYFUNCTION + "] Loop Count " + LOOP_PING_COUNT + " on " + DO_WHILE_LOOP_START_DATETIME);

                        int finalistIndex = 1;
                        string subFunction = "PING";
                        logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] ");
                        bool enablenNewPingerRouting = true;
                        if (enablenNewPingerRouting)
                        {
                            //int foreachLoopIndex = 0;
                            foreach (PingerTarget currPingTarget in pingableTargetList)
                            {
                                logThis(currPingTarget.TargetIndex + " | " + currPingTarget.DisplayName);
                                // mylist.Add(currPingTarget.IPAddress);
                                //                           AutoResetEvent waiter = new AutoResetEvent(false);
                                //                           Ping pingSenderObject = new Ping();
                                //                            pingSenderObject.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
                                PingOptions options = new PingOptions(Globals.DEFAULT_PING_TIME_TO_LEAVE, true);
                                //Probe(currPingTarget.IPAddress.ToString(), timeoutms, buffer, options);
                                Ping ping = new Ping();
                                Object userToken = new object();
                                PingReply pr = await PingExtensions.SendTask(ping, currPingTarget.IPAddress.ToString(), timeoutms, buffer, options, userToken);

                                //https://stackoverflow.com/questions/45150837/running-a-ping-sendasync-with-status-message

                                if (pr != null)
                                {
                                    switch (pr.Status)
                                    {
                                        case IPStatus.Success:
                                            logThis(userToken.GetType() + "  prIP=" + pr.Address.ToString() + ", currTarget=" + currPingTarget.TargetIndex + " | " + currPingTarget.DisplayName + ", RT=" + pr.RoundtripTime + "ms, ttl=" + pr.Options.Ttl + ",Frag=" + pr.Options.DontFragment + ", replyBuffer=" + pr.Buffer.Length);
                                            break;
                                        case IPStatus.TimedOut:
                                            logThis(userToken.GetType() + "  prIP=" + pr.Address.ToString() + ", currTarget=" + currPingTarget.TargetIndex + " | " + currPingTarget.DisplayName + " Timouts");
                                            break;
                                        default:
                                            logThis(userToken.GetType() + "  prIP=" + pr.Address.ToString() + ", currTarget=" + currPingTarget.TargetIndex + " | " + currPingTarget.DisplayName + " UnknownStatus");
                                            break;
                                    }
                                }

                            }

                        }

                        bool enableOldPingerRouting = false;
                        if (enableOldPingerRouting)
                        {
                            foreach (PingerTarget currPingTarget in pingableTargetList)
                            {
                                int LOOP_DURATION_IN_MILLISECONDS = 0; // reset to Zero for the next loop
                                currPingTarget.CurrStatusPingDateCurrent = DateTime.Now;
                                currPingTarget.CurrHostPingCount++;
                                if (currPingTarget.IPAddress != null)
                                {
                                    logThisVerbose("[" + MYFUNCTION + "] Processing " + finalistIndex + "/" + pingableTargetList.Count + ": " + currPingTarget.DisplayName + "[" + currPingTarget.IPAddress.ToString() + "]");
                                    currPingTarget.PingByIP = true;
                                }
                                else
                                {
                                    logThisVerbose("[" + MYFUNCTION + "] Processing " + finalistIndex + "/" + pingableTargetList.Count + ": " + currPingTarget.DisplayName);
                                    currPingTarget.PingByIP = false;
                                }
                                //* 
                                //*STEP 4.b: IF THE OBJECT TO PING HAS BEEN MARKED AS INVALID IT IS SKIPPED, SO UNLESS the.SKIP is false, it is ignored
                                //*
                                //if (!currPingTarget.Skip)
                                //{
                                // THE SYSTEM WAS NOT SKIPPED BECAUSE IT APPEARS TO BE A VALID SYSTEM
                                try
                                {
                                    // Set options for transmission:
                                    // The data can go through 64 gateways or routers
                                    // before it is destroyed, and the data packet
                                    // cannot be fragmented.
                                    PingOptions options = new PingOptions(Globals.DEFAULT_PING_TIME_TO_LEAVE, true);
                                    //options.Ttl=64;
                                    //options.DontFragment = true;

                                    PingReply reply;
                                    if (currPingTarget.PingByIP && currPingTarget.IPAddress != null)
                                    {
                                        logThisVerbose("\tBy IP Address ]");
                                        reply = pingSender.Send(currPingTarget.IPAddress.ToString(), timeoutms, buffer, options);
                                    }
                                    else
                                    {
                                        logThisVerbose("\tBy LookupString  ]");
                                        reply = pingSender.Send(currPingTarget.LookupString, timeoutms, buffer, options);
                                    }

                                    //if (reply != null && reply.Options != null)
                                    if (reply.Status == IPStatus.Success)
                                    {
                                        logThisVerbose("\tReceived Reply OK");
                                        if (reply.Options != null)
                                        {
                                            currPingTarget.OptionsTtl = reply.Options.Ttl;
                                        }
                                        currPingTarget.ReplyIPAddress = reply.Address.ToString();
                                        currPingTarget.RoundTrip = reply.RoundtripTime;
                                        logThisVerbose("\t\t reply.status (new status) = " + reply.Status);
                                        currPingTarget.CurrHostPingStatus = reply.Status;
                                        currPingTarget.Errorcode = 0;
                                        logThisVerbose("\t\t currPingTarget.PrevHostPingStatus = " + currPingTarget.PrevHostPingStatus);
                                    }
                                    else
                                    {
                                        logThisVerbose("\t Received Reply Not OK]");
                                        currPingTarget.Errorcode = 1;
                                        if (reply == null)
                                        {
                                            logThisVerbose("\treply is null]");
                                            logThisVerbose("\t\t reply.status (new status) = null");

                                            currPingTarget.CurrHostPingStatus = IPStatus.TimedOut;
                                            logThisVerbose("\t\t reply.status (adjusted) = " + currPingTarget.CurrHostPingStatus);
                                            logThisVerbose("\t\t currPingTarget.PrevHostPingStatus = " + currPingTarget.PrevHostPingStatus);
                                        }
                                        if (reply.Options == null)
                                        {
                                            logThisVerbose("\treply.Options is null]");
                                            currPingTarget.CurrHostPingStatus = IPStatus.TimedOut;
                                        }
                                        // In Windows, when a server goes offline, it comes through here
                                        currPingTarget.OptionsTtl = -1;
                                    }

                                }
                                catch (System.Net.NetworkInformation.PingException pe)
                                {
                                    currPingTarget.Errorcode = 1;
                                    currPingTarget.ErrorMsg = pe.Message;
                                    logThisVerbose("\tPingException: ");
                                    logThisVerbose("\t\t" + pe.Message);
                                    // if (Globals.FORCE_SLEEP)
                                    //{
                                    //    Thread.Sleep(Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS);
                                    //}
                                }
                                catch (System.Net.Sockets.SocketException se)
                                {
                                    currPingTarget.Errorcode = 1;
                                    currPingTarget.ErrorMsg = se.Message;
                                    currPingTarget.Skip = true;
                                    currPingTarget.CurrHostPingStatus = IPStatus.BadDestination;
                                    logThisVerbose("\tSocketException: ");
                                    logThisVerbose("\t\t" + se.Message);
                                }
                                catch (System.NullReferenceException nre)
                                {
                                    currPingTarget.Errorcode = 1;
                                    currPingTarget.ErrorMsg = nre.Message;
                                    logThisVerbose("\tNullReferenceException: ");
                                    logThisVerbose("\t\t" + nre.Message);
                                }
                                finally
                                {
                                    if (currPingTarget.Errorcode == 0)
                                    {
                                        currPingTarget.HostReachableCountUpdate++;
                                    }
                                    else if (currPingTarget.Errorcode == 1)
                                    {
                                        currPingTarget.HostUnreachableCountUpdate++;
                                    }
                                    else
                                    {
                                        logThis("Unknown ping error code");
                                    }

                                    TimeSpan difference = currPingTarget.CurrStatusPingDateCurrent.Subtract(currPingTarget.PrevStatusPingDate);
                                    timelapsSinceStatusChange = Math.Ceiling(difference.TotalSeconds);

                                    LOOP_DURATION_IN_MILLISECONDS = int.Parse(currPingTarget.RoundTrip.ToString());

                                    // *******************************************
                                    //  CREATE OUTPUT STRING FOR SCREEN AND CSV EXPORT
                                    //********************************************
                                    string displayTargetName;
                                    if (currPingTarget.ParentTarget != "-")
                                    {
                                        //displayTargetName = currPingTarget.IPAddress.ToString();
                                        displayTargetName = currPingTarget.DisplayName;
                                    }
                                    else
                                    {
                                        //displayTargetName = currPingTarget.LookupString;
                                        displayTargetName = currPingTarget.DisplayName;
                                    }
                                    if (!verbose)
                                    {
                                        //logThis(currPingTarget.Target);
                                        //logThis(currPingTarget.CurrHostPingStatus.ToString());
                                        if (Globals.OUTPUT_SCREEN_TO_CSV || Globals.OUTPUT_ALL_TO_CSV)
                                        {
                                            // BUILD THE OUTPUT STRING WITH RESULTS TO EXPORT TO A CSV
                                            outstr = currPingTarget.CurrHostPingCount + "," + currPingTarget.OptionsTtl + "," + currPingTarget.CurrStatusPingDateCurrent + "," + displayTargetName + "," + currPingTarget.CurrHostPingStatus.ToString() + "," + timelapsSinceStatusChange + "," + currPingTarget.RoundTrip;
                                        }
                                        else
                                        {
                                            // BUILD THE OUTPUT STRING FOR ONSCREEN OUTPUT ---->
                                            outstr = currPingTarget.CurrHostPingCount + "," + currPingTarget.OptionsTtl + "," + currPingTarget.CurrStatusPingDateCurrent + "," + displayTargetName + "," + currPingTarget.CurrHostPingStatus.ToString() + "," + timelapsSinceStatusChange + "sec," + currPingTarget.RoundTrip + "ms"; // + proposedSleepTime;
                                        }
                                    }
                                    else
                                    {
                                        outstr = "Count=" + currPingTarget.CurrHostPingCount + ",ttl=" + currPingTarget.OptionsTtl + ",Date =" + currPingTarget.CurrStatusPingDateCurrent + ",TimeLapsSec=" + timelapsSinceStatusChange + "sec,trgt=" + displayTargetName + ",status=" + currPingTarget.CurrHostPingStatus.ToString() + ",rndtrip=" + currPingTarget.RoundTrip + "ms";
                                    }

                                    // *******************************************
                                    //    DISPLAY PING RESULT HERE FOR THIS OBJECT
                                    //********************************************

                                    // If Globals.ENABLE_CONTINEOUS_PINGS is enabled - Not a smart ping, then print the results
                                    if (Globals.ENABLE_CONTINEOUS_PINGS)
                                    {
                                        // Display the results
                                        logThis(outstr);

                                        // Check if also need to output to CSV
                                        if (Globals.OUTPUT_ALL_TO_CSV)
                                        {
                                            // Export results to CSV - TBA
                                            logToFile(outstr, outputCSVFilename);
                                        }
                                    }
                                    else
                                    {
                                        //* 
                                        //* The smart ping is enabled (ENABLE_CONTINEOUS_PINGS = false), proceed to showing the results and/or exporting results to CSV
                                        //*

                                        // Display if the current and current ping status differ only
                                        //if 
                                        //{
                                        //    logThisVerbose("[" + currPingTarget.Target + "] currPingTarget.PrevHostPingStatus = " + currPingTarget.PrevHostPingStatus);
                                        //    logThisVerbose("[" + currPingTarget.Target + "] currPingTarget.CurrHostPingStatus = " + currPingTarget.CurrHostPingStatus);
                                        //}
                                        if ((currPingTarget.PrevHostPingStatus != currPingTarget.CurrHostPingStatus) || currPingTarget.CurrHostPingCount == 1)
                                        {
                                            // Replace the latest Ping status and Date into the 'Previous' variables for the next iteration
                                            currPingTarget.PrevStatusPingDate = currPingTarget.CurrStatusPingDateCurrent;
                                            currPingTarget.PrevStatusPingCount = currPingTarget.CurrHostPingCount;

                                            // 1 print to screen the difference
                                            if (!return_code_only)
                                            {
                                                logThis(outstr);
                                            }
                                            // 2 - write to log file if requested to
                                            if (Globals.OUTPUT_SCREEN_TO_CSV || Globals.OUTPUT_ALL_TO_CSV)
                                            {
                                                logToFile(outstr, outputCSVFilename);
                                            }

                                            // Make beep sounds or not to alert the user on change
                                            if (!Globals.SILENCE_AUDIBLE_ALARM)
                                            {
                                                if (currPingTarget.CurrHostPingStatus != IPStatus.Success)
                                                {

                                                    for (int i = 0; i < Globals.BEEP_COUNTS_SUCCESSFULL; i++)
                                                    {
                                                        logThisVerbose("[" + MYFUNCTION + "] [BEEP] - Success");
                                                        //BackgroundBeep.Beep();//MessageBeepType.Error);
                                                    }
                                                }
                                                else
                                                {
                                                    for (int i = 0; i < Globals.BEEP_COUNTS_UNSUCCESSFULL; i++)
                                                    {
                                                        logThisVerbose("[" + MYFUNCTION + "] [BEEP] - No Success");
                                                        //BackgroundBeep.Beep();//MessageBeepType.Ok);
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                                currPingTarget.CurrStatusPingDatePrevious = DateTime.Now;
                                //} // if skip == false

                                //if (Globals.MAX_COUNT_USER_SPECIFIED)
                                //{
                                //    //Globals.PING_COUNT_VALUE_USER_SPECIFIED = currPingTarget.CurrHostPingCount + 1;
                                //    Globals.PING_COUNT_RUNTIME_VALUE = currPingTarget.CurrHostPingCount + 1;
                                //}
                                finalistIndex++;
                                if (verbose) { currPingTarget.Printout(); }

                            } // End Foreach
                        }

                        // Based on how long since the foreach command took to ping every hosts, recalculate how long to sleep this thred for.
                        // For large numbers, generally the script doesn't sleep, but for small numbers, it may sleep.
                        DateTime DO_WHILE_LOOP_END_DATETIME = DateTime.Now;
                        TimeSpan DO_WHILE_LOOP_DATETIME_DIFFERENCE = DO_WHILE_LOOP_START_DATETIME.Subtract(DO_WHILE_LOOP_END_DATETIME);
                        int timelapsInMilliseconds = ((int)Math.Ceiling(DO_WHILE_LOOP_DATETIME_DIFFERENCE.TotalMilliseconds));

                        // This is hard to make sense when are a lot of nodes to ping
                        if (Globals.FORCE_SLEEP)// && (pingTargets.Length == 1))
                        {
                            /*//                       
                            //if (currPingTarget.CurrHostPingCount < Globals.PING_COUNT_VALUE_USER_SPECIFIED) { Thread.Sleep(proposedSleepTime); }
                            //TimeSpan difference = pingTargets[0].CurrStatusPingDateCurrent.Subtract(pingTargets[0].PrevStatusPingDate);
                            //int timeinMilliseconds = ((int)Math.Ceiling(difference.TotalMilliseconds));
                            //if (Math.Ceiling(difference.TotalMilliseconds) <= (sleeptime / pingTargets.Length))
                            //{
                            //logThis(" 1 Sleeping for " + timeinMilliseconds + "ms");
                            // example 1000 - 2000 = -1000

                            // Calculate Proposed sleeptime
                            // value DO_WHILE_LOOP_DATETIME_DIFFERENCE is always Negative
                            // value of Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS is always positive
                            //*/

                            int sleepRequiredInMilliseconds = Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS + timelapsInMilliseconds;

                            if (sleepRequiredInMilliseconds > 0)// (Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS - timelapsInMilliseconds) > 0)
                            {
                                //if (pingTargets[0].CurrHostPingCount < Globals.PING_COUNT_VALUE_USER_SPECIFIED) { Thread.Sleep(sleeptime / pingTargets.Length); }
                                logThisVerbose("\t\tGlobals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS=" + Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS);
                                logThisVerbose("\t\tCalculated value of timelapsInMilliseconds = " + timelapsInMilliseconds);
                                logThisVerbose("\t\tSleeping for " + sleepRequiredInMilliseconds + " miliseconds");
                                // if (tmpHostObjectList[0].CurrHostPingCount < Globals.PING_COUNT_VALUE_USER_SPECIFIED) { Thread.Sleep(Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS - timelapsInMilliseconds); }

                                Thread.Sleep(sleepRequiredInMilliseconds);
                            }
                            else
                            {
                                logThisVerbose("\t\tGlobals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS=" + Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS);
                                logThisVerbose("\t\tCalculated value of timelapsInMilliseconds = " + timelapsInMilliseconds);
                                logThisVerbose("\t\ttimelapsInMilliseconds to large (" + sleepRequiredInMilliseconds + "ms)" + ", no need to sleep");
                                // Don't sleep at all if (pingTargets[0].CurrHostPingCount < Globals.PING_COUNT_VALUE_USER_SPECIFIED) { Thread.Sleep(Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS - timelapsInMilliseconds); }
                            }
                            //} else
                            //{
                            //  logThis(">> Sleeping for " + timeinMilliseconds + "ms");
                            // Don't wait
                            //}


                        }

                        // If User requested to ping by count
                        if (Globals.MAX_COUNT_USER_SPECIFIED)
                        {
                            Globals.PING_COUNT_RUNTIME_VALUE++;
                            logThisVerbose("[" + MYFUNCTION + "] " + Globals.PING_COUNT_RUNTIME_VALUE + " / " + Globals.PING_COUNT_VALUE_USER_SPECIFIED + " pings completed       ", Globals.PRINT_NEW_LINE);

                            // If ping count specified check that it doesn't exceed the user request number of ping count
                            if (Globals.PING_COUNT_RUNTIME_VALUE >= Globals.PING_COUNT_VALUE_USER_SPECIFIED)
                            {
                                // Ping count has been met or exceed, break out of the loop to stop pinging
                                continueLoop = false;
                                logThisVerbose("");
                                logThisVerbose("[END]           Max count reached - Exiting program.");
                            }
                            else
                            {
                                // keep looping because we are not done yet. 
                                continueLoop = true;
                            }
                        }
                        //Globals.DURATION_END_DATE;
                        //var result = (DateTime.Now - DO_WHILE_LOOP_START_DATETIME).TotalHours;
                        if (Globals.DURATION_USER_SPECIFIED)
                        {
                            DateTime thisDate = DateTime.Now;
                            double result = (Globals.DURATION_END_DATE - thisDate).TotalHours;// / 150;
                                                                                              //logThisVerbose("[END] result=" + result + ", Globals.DURATION_END_DATE=" + Globals.DURATION_END_DATE, Globals.PRINT_NEW_LINE);
                            logThisVerbose("[END] LOOP_PING_COUNT=" + LOOP_PING_COUNT + ", Globals.DURATION_END_DATE=" + Globals.DURATION_END_DATE + ", current Time=" + thisDate, Globals.PRINT_NEW_LINE);
                            if (result < 0)
                            {
                                //Do your business logic for expiring token
                                continueLoop = false;
                                logThisVerbose("");
                                logThisVerbose("[END] Timer down to zero - Exiting program.");
                            }
                        }
                        LOOP_PING_COUNT++;

                    } while (continueLoop); // End do Loop
                }
                else
                {
                    logThisVerbose("[" + MYFUNCTION + "]  There were no objects to ping. Exiting..");
                    //return 0;
                }

            }
            else
            {
                logThisVerbose("[" + MYFUNCTION + "]  There were no objects to ping. Exiting..");
                //return 0;
            }
            //return pt.Errorcode;
            //return 0;
        }

        /**
        * Automate Diagnostics Function and see the results
*/
        /* public static void RunDiag()
         {
             string MYFUNCTION = "RunDiag";
             // pinger 192.168.0.1 gives an exception
             // pinger 192.124.249.107 doesn't work (invalid hostname and does notjhing)
             // pinger ips,badnames,legitnames
             // pinger time.optusnet.com.au -i (all of the ips)
             // When using DNs.GetHostEntry, the following fails for 192 addresses. It works when Using DNS.Resolve(), but that function is depricated

             /* Target 2: -(Could not resolve host '192.124.249.107') - DNS lookup by IP fails on this guy 192.124.249.107
              * On MacOS
                         [VERBOSE] [MAIN] ++++++++++++++++++++++++++++++++++++++++++++++++++
                         [VERBOSE] [MAIN] User search string [ 192.124.249.107 ]
                         [VERBOSE] [MYFUNCTION] Searching by IP for 192.124.249.107
                         [VERBOSE] [MYFUNCTION] An Exception Caught
                         [VERBOSE] [MYFUNCTION]          se.HResult = -2147467259
                         [VERBOSE] [MYFUNCTION]          se.Message = Could not resolve host '192.124.249.107'
                         [VERBOSE] [MAIN]    DNS Lookup Completed - Not sure of the DNS lookup outcome

              * on Windows
                         [VERBOSE] [MAIN] ++++++++++++++++++++++++++++++++++++++++++++++++++
                         [VERBOSE] [MAIN] User search string [ 192.124.249.107 ]
                         [VERBOSE] [MYFUNCTION] Searching by IP for 192.124.249.107
                         [VERBOSE] [MYFUNCTION]     hostEntry.AddressList.Count = 0
                         [VERBOSE] [MYFUNCTION]     hostEntry.Aliases.Count = 0
                         [VERBOSE] [MYFUNCTION] An Exception Caught
                         [VERBOSE] [MYFUNCTION]          se.HResult = -2146233080
                         [VERBOSE] [MYFUNCTION]          se.Message = Index was outside the bounds of the array. <- Different error
              */
        // Issue pinging ipv6 on macOS

        /**
         * 
         * 
         * 
         * 
         * Teivas-MBP:~ teiva$ pinger nas,msd,firewall

Unhandled Exception:
System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection.
Parameter name: index
at System.Collections.Generic.List`1[T].get_Item (System.Int32 index) [0x00009] in <92218043474744ea9d64d27064c35dcb>:0
at Pinger.Program.Main (System.String[] args) [0x0151f] in <b8c39badb567473e95b6b476855540b9>:0
[ERROR] FATAL UNHANDLED EXCEPTION: System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and less than the size of the collection.
Parameter name: index
at System.Collections.Generic.List`1[T].get_Item (System.Int32 index) [0x00009] in <92218043474744ea9d64d27064c35dcb>:0
at Pinger.Program.Main (System.String[] args) [0x0151f] in <b8c39badb567473e95b6b476855540b9>:0


} */


        /* public static void logThisVerbose(string msg, bool newline = false)
          {
              if (Globals.PROGRAM_VERBOSE_LEVEL2 &)
              {
                  logThis("[" + MYFUNCTION + "]  " + msg);
              }
          }*/

        public static void logThisVerbose(string msg, bool newline = true)
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

        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        public static void logThis(string msg)
        {
            string MYFUNCTION = "logThis";
            if (msg.Contains("TimedOut"))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (msg.Contains("Skip") || msg.Contains("too long") || msg.Contains("BadDestination") || msg.Contains("Unknown") || msg.Contains("UnknownIP") || msg.Contains("Invalid") || msg.Contains("Could not resolve") || msg.Contains("not known") || msg.Contains("DestinationHostUnreachable") || msg.Contains("Unknown ping error code"))
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (msg.Contains("Success") || msg.Contains("OK"))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
            }
            if (msg.Contains("[" + MYFUNCTION + "] "))
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
            logThisVerbose("[" + MYFUNCTION + "] Globals.SKIP_DNS_LOOKUP = " + Globals.SKIP_DNS_LOOKUP);
            logThisVerbose("[" + MYFUNCTION + "] Globals.IPV4_ONLY_IF = " + Globals.IPV4_ONLY_IF);

            // Check if 'hostNameOrAddress' is an IP address
            logThisVerbose("[" + MYFUNCTION + "] Checking if '" + newObject.LookupString + "' is an IP address");
            if (System.Net.IPAddress.TryParse(newObject.LookupString, out System.Net.IPAddress? address))
            {
                string subFunction = "ReverseLookup";
                newObject.DnsLookupType = DnsLookupByCodes.ByIP;

                // Important, if DNS Reverse Lookup fails, still build the IPAddresses (IP and IP Familly) as it is how pinger needs it

                System.Net.IPAddress hostEntryIPAddress = address;
                string hostEntryIPAddressString = hostEntryIPAddress.ToString();
                string hostEntryIPAddressFamilly = hostEntryIPAddress.AddressFamily.ToString();
                logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Is an IP [" + hostEntryIPAddress.AddressFamily.ToString() + "]Address ");
                try
                {
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    Attempting to reverse lookup");
                    hostEntry = Dns.GetHostEntry(newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    Success");
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
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during reverse lookup of " + newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
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
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during reverse lookup of " + newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.IPAddresses = new System.Net.IPAddress[] { hostEntryIPAddress }; //New to create an array of IPs, since there is one
                                                                                               //newObject.DnsLookUpMessage = "The length of string is greater than 255 characters";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (System.Net.Sockets.SocketException se)
                {
                    // Reverse lookup failed
                    subFunction = "SocketException";
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during reverse lookup of " + newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.IPAddresses = new System.Net.IPAddress[] { hostEntryIPAddress }; //New to create an array of IPs, since there is one
                                                                                               //newObject.DnsLookUpMessage = "An error was encountered when resolving the hostNameOrAddress parameter.";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (ArgumentException se)
                {
                    // Reverse lookup failed
                    subFunction = "ArgumentException";
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during reverse lookup of " + newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.IPAddresses = new System.Net.IPAddress[] { hostEntryIPAddress }; //New to create an array of IPs, since there is one
                                                                                               //newObject.DnsLookUpMessage = "An error was encountered when resolving the hostNameOrAddress parameter.s";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }

                logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsResolvedHostname =" + newObject.DnsResolvedHostname);
                logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpMessage=" + newObject.DnsLookUpMessage);
                logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpCode = " + newObject.DnsLookUpCode);
                //logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   //newObject.DnsLookUpStatus = " + newObject.DnsLookUpStatus);
                int ipIndex = 1;
                foreach (System.Net.IPAddress ip in newObject.IPAddresses)
                {
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.IP[" + ipIndex + "] = " + ip.ToString() + "][" + ip.AddressFamily + "]");
                    ipIndex++;
                }
            }
            else // Forward Lookup by hostname
            {
                string subFunction = "ForwardLookup";
                newObject.DnsLookupType = DnsLookupByCodes.ByName;
                logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Performing a Lookup by name");
                logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] '" + newObject.LookupString + "' is not an IP address");

                try
                {
                    // attempt to forward lookup
                    //logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "] Attempting a Forward Lookup on " + newObject.LookupString);
                    hostEntry = Dns.GetHostEntry(newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "]    Success");
                    newObject.DnsLookUpCode = ResultCodes.Ok;
                    newObject.DnsLookUpMessage = "Success";
                    newObject.IPAddresses = hostEntry.AddressList;
                    newObject.DnsResolvedHostname = hostEntry.HostName;
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsResolvedHostname=" + newObject.DnsResolvedHostname);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpMessage=" + newObject.DnsLookUpMessage);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpCode=" + newObject.DnsLookUpCode);
                    //logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.DnsLookUpStatus= " + newObject.DnsLookUpStatus);
                    int ipIndex = 1;
                    foreach (System.Net.IPAddress ip in newObject.IPAddresses)
                    {
                        logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]   newObject.IP[" + ipIndex + "] = " + ip.ToString() + "][" + ip.AddressFamily + "]");
                        ipIndex++;
                    }
                    dnsResults = new object[] { hostEntry.HostName, hostEntry.AddressList, "Could not resolve by Name" };
                    newObject.DnsLookUpCode = ResultCodes.Ok;
                }
                /*catch (Exception se)
                {
                    // forward lookup failed for some reason
                    //logThis("[Exception] message = " + se.Message);
                    // You w// IF YOU ARE IN HERE THEN THE REQUESTED HOSTNAME CAN NOT BE FOUND
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "][Exception] An Exception Caught during forward lookup of hostNameOrAddress");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "][Exception]   se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "][Exception]   se.Message = " + se.Message);
                    //dnsResults = new string[] { hostNameOrAddress, "-", se. };
                    newObject.DnsLookUpMessage = "Could not resolve";
                    newObject.DnsLookUpCode = ResultCodes.Error;
                    newObject.IPAddresses = hostEntry.AddressList;
                }*/
                catch (ArgumentNullException se)
                {
                    // Forward lookup failed
                    subFunction = "ArgumentNullException";
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during Forward lookup of " + newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    newObject.DnsResolvedHostname = "-";
                    // newObject.DnsLookUpMessage = "The hostname parameter is null";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (ArgumentOutOfRangeException se)
                {
                    // Forward lookup failed
                    subFunction = "ArgumentOutOfRangeException";
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during Forward lookup of " + newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    //newObject.DnsLookUpMessage = "The length of string is greater than 255 characters";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (System.Net.Sockets.SocketException se)
                {
                    // Forward lookup failed
                    subFunction = "SocketException";
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during Forward lookup of " + newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    //newObject.DnsLookUpMessage = "An error was encountered when resolving the hostNameOrAddress parameter";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }
                catch (ArgumentException se)
                {
                    // Forward lookup failed
                    subFunction = "SocketException";
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    A problem occured");
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]    An Exception Caught during Forward lookup of " + newObject.LookupString);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.HResult = " + se.HResult);
                    logThisVerbose("[" + MYFUNCTION + "][" + subFunction + "]        se.Message = " + se.Message);
                    //newObject.DnsLookUpMessage = "An error was encountered when resolving the hostNameOrAddress parameter.s";
                    newObject.DnsLookUpMessage = se.Message;
                    newObject.DnsLookUpCode = ResultCodes.Error;
                }

            }
            return newObject;
        }

        public static void PrintOutGlobalVariables()
        {
            string MYFUNCTION = "PrintOutGlobalVariables";
            logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++++++++++");
            logThisVerbose("[" + MYFUNCTION + "] Global.PROGRAM_VERBOSE = " + Globals.PROGRAM_VERBOSE);
            logThisVerbose("[" + MYFUNCTION + "] Global.PROGRAM_VERBOSE_LEVEL2 = " + Globals.PROGRAM_VERBOSE_LEVEL2);
            logThisVerbose("[" + MYFUNCTION + "] Global.RUNTIME_ERROR = " + Globals.RUNTIME_ERROR);
            logThisVerbose("[" + MYFUNCTION + "] Global.PING_COUNT_VALUE_USER_SPECIFIED = " + Globals.PING_COUNT_VALUE_USER_SPECIFIED);
            logThisVerbose("[" + MYFUNCTION + "] Global.PING_COUNT_RUNTIME_VALUE = " + Globals.PING_COUNT_RUNTIME_VALUE);
            logThisVerbose("[" + MYFUNCTION + "] Global.MAX_COUNT_USER_SPECIFIED = " + Globals.MAX_COUNT_USER_SPECIFIED);
            logThisVerbose("[" + MYFUNCTION + "] Global.VERBOSE = " + Globals.VERBOSE);
            logThisVerbose("[" + MYFUNCTION + "] Global.PING_ALL_IP_ADDRESSES = " + Globals.PING_ALL_IP_ADDRESSES);
            logThisVerbose("[" + MYFUNCTION + "] Global.DURATION_USER_SPECIFIED = " + Globals.IPV4_ONLY_IF);
            logThisVerbose("[" + MYFUNCTION + "] Global.ENABLE_CONTINEOUS_PINGS = " + Globals.ENABLE_CONTINEOUS_PINGS);
            logThisVerbose("[" + MYFUNCTION + "] Global.SILENCE_AUDIBLE_ALARM = " + Globals.SILENCE_AUDIBLE_ALARM);
            logThisVerbose("[" + MYFUNCTION + "] Global.FORCE_SLEEP = " + Globals.FORCE_SLEEP);
            logThisVerbose("[" + MYFUNCTION + "] Global.RUNTIME_IN_HOURS = " + Globals.DURATION_VALUE_IN_DECIMAL);
            logThisVerbose("[" + MYFUNCTION + "] Global.DURATION_END_DATE = " + Globals.DURATION_END_DATE);
            logThisVerbose("[" + MYFUNCTION + "] Global.DURATION_TIMESPAN = " + Globals.DURATION_TIMESPAN);
            logThisVerbose("[" + MYFUNCTION + "] Global.DURATION_USER_SPECIFIED = " + Globals.DURATION_USER_SPECIFIED);
            logThisVerbose("[" + MYFUNCTION + "] Global.OUTPUT_SCREEN_TO_CSV = " + Globals.OUTPUT_SCREEN_TO_CSV);
            logThisVerbose("[" + MYFUNCTION + "] Global.OUTPUT_ALL_TO_CSV = " + Globals.OUTPUT_ALL_TO_CSV);
            logThisVerbose("[" + MYFUNCTION + "] Global.SKIP_DNS_LOOKUP = " + Globals.SKIP_DNS_LOOKUP);
            logThisVerbose("[" + MYFUNCTION + "] Global.DNS_SERVER = " + Globals.DNS_SERVER);
            logThisVerbose("[" + MYFUNCTION + "] Global.DEFAULT_POLLING_MILLISECONDS = " + Globals.DEFAULT_POLLING_MILLISECONDS);
            logThisVerbose("[" + MYFUNCTION + "] Global.DEFAULT_TIMEOUT_MILLISECONDS = " + Globals.DEFAULT_TIMEOUT_MILLISECONDS);
            logThisVerbose("[" + MYFUNCTION + "] Global.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS = " + Globals.SLEEP_IN_USER_REQUESTED_IN_MILLISECONDS);
            logThisVerbose("[" + MYFUNCTION + "] ++++++++++++++++++++++++++++++++++++++++++++");
        }

        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        static public void ShowHeader()
        {
            //string MYFUNCTION = "ShowHeader";
            //string version = Assembly.GetExecutingAssembly().GetName().Version.Major.ToString() + "." + Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();            
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            /*logThis("Product version: " + fvi.ProductVersion);
            logThis("File version: " + fvi.FileVersion);
            logThis("CompanyName: " + fvi.CompanyName);
            logThis("Product Name: " + fvi.ProductName);*/
            //string version = fvi.FileVersion;

            //logThis("Pinger is a custom ping utility - ver " + version);
            logThis(fvi.ProductName + " " + fvi.FileVersion + " by " + fvi.CompanyName);
        }

        /// <summary>
        /// Function: ShowSyntax
        /// Information: Display usage information for the application
        /// </summary>
        static public void ShowSyntax()
        {
            // Display Application Syntax
            //string MYFUNCTION = "ShowSyntax";
            logThis("Syntax  : Pinger <hosts> [OPTIONS]");
            // Display Return Codes Information
            //"\t-s:\tSmart switch. Pinger only shows pinger response \n\t\tif the current ping status is different to the last one \n"+                           
            logThis("[HOSTS]: \n" +
                    "\tsingle or multiple hostnames,fqdn,ipv4, and ipv6 IP addresses. Must comma separate (no spaces).\n");
            logThis("[Examples]: \n" +
                        "\tpinger google.com.au,fd8a:4d23:a340:4960:250:56ff:febb:a99d,192.168.0.1\n"
                    );
            logThis("[OPTIONS]: \n" +
                             "\t-n:\tPinger runs once then exists\n" +
                             "\t-d <n>: Set the amount of duration in Decimal pinger runs for before exiting - Specify a positive value such as 0.25 for 15 minutes or 1.5 for 1hr20mins.\n" +
                             "\t-c <n>: Specify how many times pinger will poll before exiting - Specify a positive value 'n' greater than 1.\n" +
                             "\t-s:\tRuns like a Standard ping which prints every ping results onscreen.\n" +
                             "\t-p <n>:\tSpecify how often (in seconds) Pinger will poll the target. Useful with '-s'. Specify a positive value 'n' greater than 1.\n" +
                             "\t-t <n>:\tSet a Round Trip timeout value of 'n' seconds - Default value is 1 seconds. For high latency links above 4000ms latency, \n\t\tincrease this value above 4. When this value is reached, pinger will assume the target is unreachable.\n" +
                             "\t-q: \tMute default audible alarms. By default, pinger will beep when the status changes in the following instance.\n\t\t> 2 beeps when Status transitions from Timeout to Pingable\n\t\t> 4 beeps when Status transitions from Pingble to TimeOut\n" +
                             "\t-f: \tFastping makes pinger starts a new poll as soon it receives the previous response. Fastping is automatically \n\t\tactivated when the Round Trip is above 1 seconds. Use in combination with the '-s' switch.\n" +
                             "\t-csv: \tSaves all onscreen responses to a CSV. Does not yet take any arguments. The resultant CSV is prefixed with \n\t\tthe target name in your current directory.\n" +
                             "\t-csvall:Saves all ping results to a CSV even regardless what's onscreen. Useful when wanting only the differences in\n\t\tresults onscreen but all of the ping results in a CSV. \n\t\tThe resultant CSV is prefixed with the target name in your current directory.\n" +
                             "\t-skipDnsLookup: \tSkip DNS lookup.\n" +
                             "\t-dnsonly: \tPing DNS resolvable targets only from the list.\n" +
                             "\t-i: \tPing all IP addresses enumerated from the NSLOOKUP query.\n" +
                             "\t-ipv4: \tPing all IPv4 addresses only.\n" +
                             //"\t-ipv6: \tPing all IPv6 addresses only.\n" +
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

            logThis("Examples: \n" +
                            "\tSmart ping server1, and only report when the status changes\n" +
                             "\t\tmono pinger.exe server1\n" +
                             "\tSmart ping multiple servers and only report when the status changes\n" +
                             "\t\tmono pinger.exe server1,server2,server3\n" +
                             "\tRun a standard ping on a single server 10 times\n" +
                             "\t\tmono pinger.exe server1 -s -c 10\n" +
                             "\tRun a standard ping on a single server 10 times but verbose the output and stop the audible noise on status changes \n" +
                             "\t\tmono pinger.exe server1 -s -c 10 -v -q\n");
            ShowHeader();
        }
    }
}
