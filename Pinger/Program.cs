// pinger Utility
// use 'pinger.exe /?' for help
//
// Udpated for .NET Framework 4.5
using System;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;

namespace Pinger
{
    public enum MessageBeepType
    {
        Default = -1,
        Ok = 0x00000000,
        Error = 0x00000010,
        Question = 0x00000020,
        Warning = 0x00000030,
        Information = 0x00000040,
    }

    public class PingerTarget
    {
        private string hostNameOrAddress;
        private string dnsName;
        private string dnsipaddr;
        private string dnsReplyIPaddr;
        private long pingReplyRoundTripInMiliSec;
        private string dnsLookupStatus;
        private string logFile;
        private string errorMsg;
        private int errorCode;
        private int optionsTtl;
        private int hostUnreachableCount;
        private int hostReachableCount;
        private DateTime startDate;
        private DateTime endDate;

        private string currHostPingStatus;
        private int currHostPingCount;
        private DateTime currStatusPingDateCurrent;
        private DateTime currStatusPingDatePrevious;

        private string prevHostPingStatus;
        private DateTime prevStatusPingDate;
        private int prevStatusPingCount;

        public PingerTarget(string targetName)
        {
            this.hostNameOrAddress = targetName;
            this.dnsName = "-";
            this.dnsipaddr = "-";
            this.dnsReplyIPaddr = "-";
            this.pingReplyRoundTripInMiliSec = -1;
            this.dnsLookupStatus = "-";
            this.logFile = "-";
            this.currHostPingStatus = "-";
            this.prevHostPingStatus = "-";
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
        }

        public PingerTarget()
        {
            this.hostNameOrAddress = "-";
            this.dnsName = "-";
            this.dnsipaddr = "-";
            this.dnsReplyIPaddr = "-";
            this.pingReplyRoundTripInMiliSec = 0;
            this.dnsLookupStatus = "-";
            this.logFile = "-";
            this.currHostPingStatus = "-";
            this.prevHostPingStatus = "-";
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
        }

        public string Target
        {
            get { return hostNameOrAddress; }
            set { hostNameOrAddress = value; }
        }
        public string Hostname
        {
            get { return dnsName; }
            set { dnsName = value; }
        }
        public string IPAddress
        {
            get { return dnsipaddr; }
            set { dnsipaddr = value; }
        }
        public string DNSLookupStatus
        {
            get { return dnsLookupStatus; }
            set { dnsLookupStatus = value; }
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

        public string CurrHostPingStatus
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

        public string PrevHostPingStatus
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
            set
            {
                currStatusPingDateCurrent = value;
            }
        }
        public DateTime PrevStatusPingDate
        {
            get { return prevStatusPingDate; }
            set
            {
                prevStatusPingDate = value;
            }
        }

        public long RoundTrip
        {
            get { return pingReplyRoundTripInMiliSec; }
            set { pingReplyRoundTripInMiliSec = value; }
        }
        public void Printout()
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("VERBOSE:");
            Console.WriteLine("     currHostPingCount = " + currHostPingCount);
            Console.WriteLine("     currHostPingStatus = " + currHostPingStatus);
            Console.WriteLine("     currStatusPingDateCurrent = " + currStatusPingDateCurrent);
            Console.WriteLine("     currStatusPingDatePrevious = " + currStatusPingDatePrevious);
            Console.WriteLine("     prevHostPingStatus = " + prevHostPingStatus);
            Console.WriteLine("     PrevStatusPingCount = " + prevStatusPingCount);
            Console.WriteLine("     prevStatusPingDate = " + prevStatusPingDate);
            Console.WriteLine("     hostNameOrAddress = " + hostNameOrAddress);
            Console.WriteLine("     dnsName = " + dnsName);
            Console.WriteLine("     dnsipaddr = " + dnsipaddr);
            Console.WriteLine("     dnsReplyIPaddr = " + dnsReplyIPaddr);
            Console.WriteLine("     dnsLookupStatus = " + dnsLookupStatus);
            Console.WriteLine("     pingReplyRoundTripInMiliSec = " + pingReplyRoundTripInMiliSec);
            Console.WriteLine("     optionsTtl = " + optionsTtl);
            Console.WriteLine("     errorMsg = " + errorMsg);
            Console.WriteLine("     errorCode = " + errorCode);
            Console.WriteLine("     hostUnreachableCount = " + hostUnreachableCount);
            Console.WriteLine("     hostReachableCount = " + hostReachableCount);
            Console.WriteLine("     startDate = " + startDate);
            Console.WriteLine("     endDate = " + endDate);
            Console.WriteLine("     LogFile = " + logFile);
        }
    }

    class Program
    {
        static int Main(string[] args)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            // Create  the ping target object, aka pt
            PingerTarget pt = new PingerTarget();
            bool verbose = false; // true = print additional versbose stuff for the program
            bool forceSleep = true; //true to allow pinger to ping normall with 1 second between the last ping response and the next ping, false to for pinger to ping straight after the last ping response
            //bool loop = true; // true = ping will loop until Ctrl + C is pressed
            int items = -1; // compensate for "pinger" counting as 1 command line argument
            int maxcount = 0;
            bool maxCountSpecified = false;
            bool smartping = true; // by default use the smart ping switch
            bool return_code_only = false;
            string target = ""; // target IP address or DNS name to ping
            int defaultPollingTimeInMilliseconds = 1000; //iteration defaultPollingTimeInMilliseconds in ms or can be seen as polling, not how long to wait for a response
            int defaultPingTimeoutInMilliseconds = 1000; // 4000 milliseconds of amount of time in ms to wait for the ping reply - matches regular ping on windows
            bool stopBeeps = false;
            bool outputScreenToCSV = false; // Output only what's on the screen to CSV. So if printing only changes then it will only output to file that
            bool skipDNSLookup = false; // Don't DNS lookup first
            string dnsServer = ""; //Use this DNS server for DNS Lookups
            bool outputAllToCSV = false; // Output every ping response to CSV, even if you are using the smart ping function which only prints the status changes
            string outputCSVFilename = "";
            string outstr = "";
            int sleeptimeInMilliseconds = defaultPollingTimeInMilliseconds;
            int sleeptimesec = sleeptimeInMilliseconds / 1000;
            double timelaps = 0;
            int runtimeError = 0;
            int runtimeInHours = 0;
            int proposedSleepTime = sleeptimeInMilliseconds;
            int actualroundtrip = 0;

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = defaultPingTimeoutInMilliseconds;
            int timeoutsec = timeout / 1000;
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
                        runtimeError = 1;
                        break;
                    case "-R": // Returns code only, doesn't expects a value after this switch
                        return_code_only = true;
                        maxcount = 1;
                        maxCountSpecified = true;
                        break;
                    case "-V":
                        verbose = true;
                        break;
                    case "-S": // Make pinger like ping and output every responses to screen
                        smartping = false;
                        break;
                    case "-N": // No loop, same as using '-c 1'
                        maxcount = 1;
                        maxCountSpecified = true;
                        //loop = false;
                        break;
                    case "-Q": // quietens audible sounds (beeps)
                        stopBeeps = true;
                        break;
                    case "-F": // overrides pinger from sleeping 1 second betyween a ping response and the next ping. This will increase the ping  withing a second. 
                        forceSleep = false;
                        break;
                    case "-C": // Specify how many times pinger will loop ping a host, expects a positive value after the switch equal or greater than 1                        
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            //bool success = int.TryParse(arguments[argIndex], out sleeptime);
                            maxCountSpecified = true;
                            maxcount = int.Parse(arguments[argIndex]);
                        }
                        catch (System.ArgumentNullException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
                        }
                        catch (System.FormatException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
                        }
                        catch (System.OverflowException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
                        }
                        break;
                    case "-H": // Run pinger for a number of hours, expects a positive value after the switch
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            //bool success = int.TryParse(arguments[argIndex], out sleeptime);
                            runtimeInHours = int.Parse(arguments[argIndex]);
                            maxCountSpecified = false;
                            //loop = true;
                        }
                        catch (System.ArgumentNullException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
                        }
                        catch (System.FormatException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
                        }
                        catch (System.OverflowException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
                        }
                        break;
                    case "-CSV": // Output each ping responses to a CSV file but matches onscreen output.
                        //verbose = false;
                        outputScreenToCSV = true;
                        outputAllToCSV = !outputScreenToCSV;
                        break;
                    case "-CSVALL": // Output each ping responses to a CSV file even if you are using the smartping function
                        //verbose = false;
                        outputScreenToCSV = false;
                        outputAllToCSV = !outputScreenToCSV;
                        break;

                    case "-P": // Poll every 'n' seconds, expects a value after this switch
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            //bool success = int.TryParse(arguments[argIndex], out sleeptime);
                            sleeptimeInMilliseconds = int.Parse(arguments[argIndex]) * 1000;
                        }
                        catch (System.ArgumentNullException)
                        {
                            logThis("Please specify a valid polling interval in seconds.");
                            runtimeError = 1;
                        }
                        catch (System.FormatException)
                        {
                            logThis("Please specify a valid polling interval in seconds.");
                            runtimeError = 1;
                        }
                        catch (System.OverflowException)
                        {
                            logThis("Please specify a valid polling interval in seconds.");
                            runtimeError = 1;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            logThis("Please specify a valid polling interval in seconds.");
                            runtimeError = 1;
                        }
                        break;
                    case "-SKIPDNSLOOKUP": // skip DNS lookups
                        skipDNSLookup = true;
                        break;
                    case "-DNSSERVER": // skip DNS lookups
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            dnsServer = arguments[argIndex];
                        }
                        catch (System.ArgumentNullException)
                        {
                            logThis("Please specify a valid number.");
                            runtimeError = 1;
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
                            timeout = timeoutsec * 1000;
                        }
                        catch (System.ArgumentNullException)
                        {
                            logThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            runtimeError = 1;
                        }
                        catch (System.FormatException)
                        {
                            logThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            runtimeError = 1;
                        }
                        catch (System.OverflowException)
                        {
                            logThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            runtimeError = 1;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            logThis("Please specify a valid timeout value in seconds larger than 1 seconds.");
                            runtimeError = 1;
                        }
                        break;
                    default:
                        if (items == 0)
                        {
                            target = arguments[argIndex];
                            //smartping = true;
                        }
                        items++;
                        break;
                }
            }

            if (runtimeError > 0)
                return runtimeError;

            if (items > 1 || target.Length <= 0)
            {
                ShowSyntax();
                runtimeError = 1;
            }
            else
            {
                // Determine the list of target hosts to ping                
                string[] hostnames;
                if (target.Contains(","))
                {
                    //logThis("Contains multiple hostnames with comma separated");
                    hostnames = target.Split(',');
                }
                else
                {
                    //logThis("Contains a single hostname");
                    hostnames = new string[1];
                    hostnames[0] = target;
                }
                PingerTarget[] pingTargets = new PingerTarget[hostnames.Length];

                // Create the  target objects  from a single or multiple hosts comma separated list
                int currentTargetIndex = 0;
                for (int i = 0; i < hostnames.Length; i++)
                {

                    // Create the objects
                    pingTargets[i] = new PingerTarget();
                    pingTargets[i].Target = hostnames[i];
                    pingTargets[i].Startdate = DateTime.Now;


                    // Now we have an array assign to the orginal PT variable
                    // pt = newtargets;

                    // Now we have to iterate
                    if (skipDNSLookup == false)
                    {
                        string[] dnsresults = DNSLookup(pingTargets[i].Target);
                        // There is an issue here
                        if (dnsresults.Length > 0)
                        {
                            pingTargets[i].Hostname = dnsresults[0];
                            pingTargets[i].IPAddress = dnsresults[1];
                            pingTargets[i].DNSLookupStatus = dnsresults[2];
                        }
                        else
                        {
                            pingTargets[i].Hostname = pingTargets[i].Target;
                        }
                    }
                    else
                    {
                        pingTargets[i].Hostname = pingTargets[i].Target;
                        pingTargets[i].IPAddress = null;
                        pingTargets[i].DNSLookupStatus = null;
                    }


                    // Object built, now we want to ping together

                    if (!return_code_only)
                    {
                        if (pingTargets.Length > 1)
                        {
                            // This means multiple hosts in the list
                            if (currentTargetIndex == 0)
                            {
                                if (forceSleep) // if true, default behavior
                                {
                                    logThis("Pinging the following hosts at " + sleeptimesec + "sec interval (Round Trip timeout set at " + timeoutsec + " seconds)");
                                }
                                else // If forceSleep = false, user choice to ignore sleep time, increasing ping requests within a 1 second period
                                {
                                    logThis("Pinging the following hosts with timeout window of " + timeoutsec + " seconds");
                                }
                            }

                        }
                        else
                        {
                            // Handle this for a single host
                            if (forceSleep) // if true, default behavior
                            {
                                logThis("Pinging " + pingTargets[i].Hostname + " at " + sleeptimesec + "sec interval (Round Trip timeout set at " + timeoutsec + " seconds)");
                            }
                            else // If forceSleep = false, user choice to ignore sleep time, increasing ping requests within a 1 second period
                            {
                                logThis("Pinging " + pingTargets[i].Hostname + " with timeout window of " + timeoutsec + " seconds");
                            }
                        }
                        // Here we want to show the DNS and Host mapping 

                        if (skipDNSLookup == false)
                        {
                            if (pingTargets[i].DNSLookupStatus == "Success")
                            {
                                logThis("Hostname " + currentTargetIndex + ": " + pingTargets[i].Hostname + "  (" + pingTargets[i].IPAddress + ")");
                            }
                            else
                            {
                                logThis("Hostname " + currentTargetIndex + ": " + pingTargets[i].Target + "  (" + pingTargets[i].IPAddress + ")");
                            }
                        }
                        else
                        {
                            logThis("Hostname " + currentTargetIndex + ": " + pingTargets[i].Target);
                        }
                        /*
                            if (currentTargetIndex = pingTargets.Length)
                        {
                            // logThis("Looking up DNS : ");
                        else
                        {
                            // logThis("      Hostname : " + pt.Hostname);                    
                            //logThis("      IP Addr  : " + pt.IPAddress);
                        }

                        //logThis("      Mac Addr : " + pt.IPAddress);
                        //logThis("      IPAddress: " + pt.Startdate); 
                        logThis("");
                        if (!maxCountSpecified && runtimeInHours > 0)
                        {
                            maxcount = (runtimeInHours * 60 * 60) / (sleeptimesec);
                            //logThis(">> sleeptime = " + sleeptimesec + ",runtimeInHours=" + runtimeInHours + "hrs,maxcount=" + maxcount +"<<");                       
                            logThis(">> Runtime: " + runtimeInHours + "hrs, Total ping expected=" + maxcount + " <<");
                        }
                        */


                        //logThis("      Mac Addr : " + pt.IPAddress);
                        //logThis("      IPAddress: " + pt.Startdate); 
                        if (!maxCountSpecified && runtimeInHours > 0)
                        {
                            maxcount = (runtimeInHours * 60 * 60) / (sleeptimesec);
                            //logThis(">> sleeptime = " + sleeptimesec + ",runtimeInHours=" + runtimeInHours + "hrs,maxcount=" + maxcount +"<<");                       
                            logThis(">> Runtime: " + runtimeInHours + "hrs, Total ping expected=" + maxcount + " <<");
                        }

                    }

                    pingTargets[i].LogFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/pinger-" + pingTargets[i].Hostname.Replace('.', '_').Trim() + "-" + pingTargets[i].Startdate.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', '-') + ".csv";

                    if (outputScreenToCSV || outputAllToCSV)
                    {

                        //outputCSVFilename = "pinger-" + pingTargets[i].Hostname.Replace('.', '_').Trim() + "-" + pingTargets[i].Startdate.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', '-') + ".csv";
                        logThis(">> Responses will be saved to " + pingTargets[i].LogFile);
                        logThis("");
                        logToFile("Count,\tTTL,Date,\tTarget,`tReply,\tTime Since Same Reply State (sec),\tRound Trip Latency (ms)", pingTargets[i].LogFile);
                    }

                    if (!return_code_only)
                    {
                        if (currentTargetIndex == (pingTargets.Length - 1))
                        {
                            logThis("Count,TTL,Date,Target,Reply,Time Since Same Reply State (sec),Round Trip Latency (ms)");
                        }
                    }

                    currentTargetIndex++;
                } // END FOOR LOOP - Object Array creation, PingTargets now in an array with all the required information, begin



                // BEGIN pinging
                do
                {
                    DateTime loopStartTime = DateTime.Now;
                    for (int currentPingTargetIndex = 0; currentPingTargetIndex < pingTargets.Length; currentPingTargetIndex++)
                    {
                        pingTargets[currentPingTargetIndex].CurrStatusPingDateCurrent = DateTime.Now;


                        pingTargets[currentPingTargetIndex].CurrHostPingCount++;
                        try
                        {

                            options.DontFragment = true;
                            PingReply reply;

                            if (pingTargets[currentPingTargetIndex].IPAddress != null)
                            {
                                //logThis("pingTargets[currentPingTargetIndex].IpAddress: " + pingTargets[currentPingTargetIndex].IPAddress);
                                reply = pingSender.Send(pingTargets[currentPingTargetIndex].IPAddress, timeout, buffer, options);
                                //pingTargets[currentPingTargetIndex].CurrHostPingCount++; // redundant with the one below ? should I take it outside this if else..may..no time to test it thought.
                            }
                            else
                            {
                                //logThis("Pinging " + pingTargets[currentPingTargetIndex].Hostname + " without DNS");
                                //logThis("pingTargets[currentPingTargetIndex].IpAddress: " + pingTargets[currentPingTargetIndex].IPAddress);
                                reply = pingSender.Send(pingTargets[currentPingTargetIndex].Hostname, timeout, buffer, options);
                                //pingTargets[currentPingTargetIndex].CurrHostPingCount++; // redundant with the one one ? should I take it outside this if else..may..no time to test it thought.
                            }

                            if (reply != null && reply.Options != null)
                            {
                                pingTargets[currentPingTargetIndex].OptionsTtl = reply.Options.Ttl;
                            }
                            else
                            {
                                pingTargets[currentPingTargetIndex].OptionsTtl = -1;
                            }

                            pingTargets[currentPingTargetIndex].ReplyIPAddress = reply.Address.ToString(); // ? reply.Address.ToString() : "-";
                            pingTargets[currentPingTargetIndex].RoundTrip = reply.RoundtripTime; //? reply.RoundtripTime : -1);
                            pingTargets[currentPingTargetIndex].CurrHostPingStatus = reply.Status.ToString(); //? reply.Status.ToString() : "-");
                                                                                                              // logThis(reply.Status.ToString());
                            if (reply.Status.ToString() == "DestinationHostUnreachable")
                            {
                                pingTargets[currentPingTargetIndex].Errorcode = 1;
                                pingTargets[currentPingTargetIndex].CurrHostPingStatus = "DestinationHostUnreachable";
                            }
                            else
                            {
                                pingTargets[currentPingTargetIndex].Errorcode = 0;
                            }

                        }
                        catch (System.Net.Sockets.SocketException se)
                        {
                            pingTargets[currentPingTargetIndex].Errorcode = 1; pingTargets[currentPingTargetIndex].ErrorMsg = se.Message; pingTargets[currentPingTargetIndex].CurrHostPingStatus = se.Message;

                            if (forceSleep)
                            {
                                Thread.Sleep(sleeptimeInMilliseconds);
                            }
                        }
                        catch (System.Net.NetworkInformation.PingException pe)
                        {
                            pingTargets[currentPingTargetIndex].Errorcode = 1; pingTargets[currentPingTargetIndex].ErrorMsg = pe.Message; pingTargets[currentPingTargetIndex].CurrHostPingStatus = pe.Message;
                            if (forceSleep)
                            {
                                Thread.Sleep(sleeptimeInMilliseconds);
                            }
                        }
                        catch (System.NullReferenceException nre)
                        {
                            pingTargets[currentPingTargetIndex].Errorcode = 1; pingTargets[currentPingTargetIndex].ErrorMsg = nre.Message; pingTargets[currentPingTargetIndex].CurrHostPingStatus = "DestinationHostUnreachable";
                            if (forceSleep)
                            {
                                Thread.Sleep(sleeptimeInMilliseconds);
                            }
                            //pingTargets[currentPingTargetIndex].Errorcode = 1; pingTargets[currentPingTargetIndex].ErrorMsg = nre.Message; pingTargets[currentPingTargetIndex].CurrHostPingStatus = nre.Message; Thread.Sleep(sleeptime);
                        }
                        finally
                        {
                            if (pingTargets[currentPingTargetIndex].Errorcode == 0)
                            {
                                pingTargets[currentPingTargetIndex].HostReachableCountUpdate++;
                                // timelaps = (sleeptimesec) * (pingTargets[currentPingTargetIndex].CurrHostPingCount - pingTargets[currentPingTargetIndex].PrevStatusPingCount);
                            }
                            else if (pingTargets[currentPingTargetIndex].Errorcode == 1)
                            {
                                pingTargets[currentPingTargetIndex].HostUnreachableCountUpdate++;
                                // timelaps = (sleeptimesec) * (pingTargets[currentPingTargetIndex].CurrHostPingCount - pingTargets[currentPingTargetIndex].PrevStatusPingCount);timelaps = (sleeptimesec + timeoutsec) * (pingTargets[currentPingTargetIndex].CurrHostPingCount - pingTargets[currentPingTargetIndex].PrevStatusPingCount);
                            }
                            else
                            {
                                logThis("Unknown ping error code");
                            }

                            TimeSpan difference = pingTargets[currentPingTargetIndex].CurrStatusPingDateCurrent.Subtract(pingTargets[currentPingTargetIndex].PrevStatusPingDate);
                            timelaps = Math.Ceiling(difference.TotalSeconds);
                            //if (loop) { Thread.Sleep(sleeptimeInMilliseconds); }

                            proposedSleepTime = sleeptimeInMilliseconds;
                            actualroundtrip = int.Parse(pingTargets[currentPingTargetIndex].RoundTrip.ToString());
                            if (sleeptimeInMilliseconds > actualroundtrip)
                            {
                                proposedSleepTime = sleeptimeInMilliseconds - actualroundtrip;
                            }
                            else if (sleeptimeInMilliseconds < actualroundtrip)
                            {
                                proposedSleepTime = 0;// Don't sleep just go ahead
                            }


                            if (pingTargets[currentPingTargetIndex].Hostname == "UnknownHostName")
                            {
                                pingTargets[currentPingTargetIndex].Hostname = pingTargets[currentPingTargetIndex].Target;
                            }

                            // Create the output string
                            if (!verbose)
                            {
                                if (outputScreenToCSV || outputAllToCSV)
                                {
                                    outstr = pingTargets[currentPingTargetIndex].CurrHostPingCount + "," + pingTargets[currentPingTargetIndex].OptionsTtl + "," + pingTargets[currentPingTargetIndex].CurrStatusPingDateCurrent + "," + pingTargets[currentPingTargetIndex].Hostname + "," + pingTargets[currentPingTargetIndex].CurrHostPingStatus + "," + timelaps + "," + pingTargets[currentPingTargetIndex].RoundTrip;
                                }
                                else
                                {
                                    outstr = pingTargets[currentPingTargetIndex].CurrHostPingCount + "," + pingTargets[currentPingTargetIndex].OptionsTtl + "," + pingTargets[currentPingTargetIndex].CurrStatusPingDateCurrent + "," + pingTargets[currentPingTargetIndex].Hostname + "," + pingTargets[currentPingTargetIndex].CurrHostPingStatus + "," + timelaps + "sec," + pingTargets[currentPingTargetIndex].RoundTrip + "ms"; // + proposedSleepTime;
                                }
                            }
                            else
                            {
                                outstr = "Count=" + pingTargets[currentPingTargetIndex].CurrHostPingCount + ",ttl=" + pingTargets[currentPingTargetIndex].OptionsTtl + ",Date =" + pingTargets[currentPingTargetIndex].CurrStatusPingDateCurrent + ",TimeLapsSec=" + timelaps + "sec,trgt=" + pingTargets[currentPingTargetIndex].Hostname + ",status=" + pingTargets[currentPingTargetIndex].CurrHostPingStatus + ",rndtrip=" + pingTargets[currentPingTargetIndex].RoundTrip + "ms";
                            }

                            if (pingTargets[currentPingTargetIndex].PrevHostPingStatus != pingTargets[currentPingTargetIndex].CurrHostPingStatus)
                            {
                                pingTargets[currentPingTargetIndex].PrevStatusPingDate = pingTargets[currentPingTargetIndex].CurrStatusPingDateCurrent;
                                // 1 print to screent the difference
                                if (!return_code_only)
                                {
                                    logThis(outstr);
                                }
                                // 2 - write to log file if requested to
                                if (outputScreenToCSV || outputAllToCSV)
                                {
                                    logToFile(outstr, outputCSVFilename);
                                }

                                if (pingTargets[currentPingTargetIndex].Errorcode == 0 && pingTargets[currentPingTargetIndex].CurrHostPingCount > 1 && !stopBeeps)
                                {
                                    for (int i = 0; i < 2; i++)
                                    {
                                        Console.Beep();
                                    }
                                }
                                else if (pingTargets[currentPingTargetIndex].Errorcode == 1 && pingTargets[currentPingTargetIndex].CurrHostPingCount > 1 && !stopBeeps) //&& smartping
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        Console.Beep();
                                    }
                                }
                                pingTargets[currentPingTargetIndex].PrevStatusPingCount = pingTargets[currentPingTargetIndex].CurrHostPingCount;

                            }
                            else // At this point the current and previous ping status differ so we need to process
                            {
                                // Only output to screen if the smart ping is not enabled and pinger behaves like ping
                                if (!smartping)
                                {
                                    if (!return_code_only)
                                    {
                                        logThis(outstr);
                                    }
                                }
                                // Only output to screen if the 
                                if (outputAllToCSV)
                                {
                                    logToFile(outstr, outputCSVFilename);
                                }
                            }
                        }
                        pingTargets[currentPingTargetIndex].CurrStatusPingDatePrevious = DateTime.Now;
                        if (!maxCountSpecified) { maxcount = pingTargets[currentPingTargetIndex].CurrHostPingCount + 1; }
                        if (verbose) { pingTargets[currentPingTargetIndex].Printout(); }

                    } // End For Loop


                    DateTime loopFinishTime = DateTime.Now;

                    TimeSpan loopDifference = loopStartTime.Subtract(loopFinishTime);
                    int timelapsInMilliseconds = ((int)Math.Ceiling(loopDifference.TotalMilliseconds));

                    // This is hard to make sense when are a lot of nodes to ping
                    if (forceSleep)// && (pingTargets.Length == 1))
                    {
                        //                       
                        //if (pingTargets[currentPingTargetIndex].CurrHostPingCount < maxcount) { Thread.Sleep(proposedSleepTime); }
                        //TimeSpan difference = pingTargets[0].CurrStatusPingDateCurrent.Subtract(pingTargets[0].PrevStatusPingDate);
                        //int timeinMilliseconds = ((int)Math.Ceiling(difference.TotalMilliseconds));
                        //if (Math.Ceiling(difference.TotalMilliseconds) <= (sleeptime / pingTargets.Length))
                        //{
                        //logThis(" 1 Sleeping for " + timeinMilliseconds + "ms");
                        // example 1000 - 2000 = -1000
                        if ((sleeptimeInMilliseconds - timelapsInMilliseconds) > 0)
                        {
                            //if (pingTargets[0].CurrHostPingCount < maxcount) { Thread.Sleep(sleeptime / pingTargets.Length); }
                            if (pingTargets[0].CurrHostPingCount < maxcount) { Thread.Sleep(sleeptimeInMilliseconds - timelapsInMilliseconds); }
                        }
                        else
                        {
                            // Don't sleep at all if (pingTargets[0].CurrHostPingCount < maxcount) { Thread.Sleep(sleeptimeInMilliseconds - timelapsInMilliseconds); }
                        }
                        //} else
                        //{
                        //  logThis(">> Sleeping for " + timeinMilliseconds + "ms");
                        // Don't wait
                        //}


                    } /*else
                        {
                            // need to calculate how long since thelogThis("else..:" + pingTargets.Length);
                            if (pingTargets[currentPingTargetIndex].CurrHostPingCount < maxcount) { Thread.Sleep(sleeptime / pingTargets.Length); }
                            
                        }*/

                } while (pingTargets[0].CurrHostPingCount < maxcount);
                //while (loop && pingTargets[currentPingTargetIndex].CurrHostPingCount < maxcount) ;
            }
            //logThis(pt.Errorcode.ToString());
            return pt.Errorcode;
        }


        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        public static void logThis(string msg)
        {
            if (msg.Contains("TimedOut"))
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(msg);
            }
            else if (msg.Contains("UnknownIP") || msg.Contains("Could not resolve host") || msg.Contains("DestinationHostUnreachable") || msg.Contains("Unknown ping error code"))
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(msg);
            }
            else if (msg.Contains("Success"))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(msg);
            }
            else if (msg.Contains("Success"))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(msg);
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(msg);
            }
            // Reset back to normal colours
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");

        }

        public static void logToFile(string msg, string filename)
        {
            using (StreamWriter w = File.AppendText(filename))
            {
                //Log(msg, w);
                w.WriteLine(msg);
            }
        }

        //static string[] DNSLookup(string hostNameOrAddress, string dnsServer)
        static string[] DNSLookup(string hostNameOrAddress)
        {
            //Console.WriteLine("Lookup: {0}\n", hostNameOrAddress);
            string[] dnsResults;
            try
            {
                /* try
                {
                    IPAddress dnsServerAddress = IPAddress.Parse("dnsServer");
                }
                catch
                {
                    Console.WriteLine("Invalid DNS server IP");
                }*/

                IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);
                //Console.WriteLine("  Host Name: {0}", hostEntry.HostName);
                string fistIP = "";
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    //Console.WriteLine("Find IP 1 Address: {0}", ip);
                    fistIP = ip.ToString();
                    // Break after the first one
                    break;
                }
                dnsResults = new string[] { hostEntry.HostName, fistIP, "Success" };

            }
            catch (System.Net.Sockets.SocketException se)
            {
                dnsResults = new string[] { "UnknownHostName", "UnknownIP", se.Message };
            }
            return dnsResults;
        }
        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        static public void ShowHeader()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            logThis("\nPinger is a custom ping utility written by Teiva Rodiere (Build " + version + ")");
        }
        /// <summary>
        /// Function: ShowSyntax
        /// Information: Display usage information for the application
        /// </summary>
        static public void ShowSyntax()
        {
            // Display Application Syntax
            ShowHeader();
            logThis("Syntax  : Pinger.exe <hosts> [OPTIONS]");
            // Display Return Codes Information
            //"\t-s:\tSmart switch. Pinger only shows pinger response \n\t\tif the current ping status is different to the last one \n"+                           
            logThis("[HOSTS]: \n" +
                    "\tsingle host or multiple hosts comma separated (no spaces).\n");
            logThis("[OPTIONS]: \n" +
                             "\t-n:\tPinger runs once then exists\n" +
                             "\t-h <n>: Set the amount of time (in hours) pinger runs for before exiting - Specify a positive value 'n' greater than 1.\n" +
                             "\t-c <n>: Specify how many times pinger will poll before exiting - Specify a positive value 'n' greater than 1.\n" +
                             "\t-s:\tRuns like a Standard ping which prints every ping results onscreen.\n" +
                             "\t-p <n>:\tSpecify how often (in seconds) Pinger will poll the target. Useful with '-s'. Specify a positive value 'n' greater than 1.\n" +
                             "\t-t <n>:\tSet a Round Trip timeout value of 'n' seconds - Default value is 1 seconds. For high latency links above 4000ms latency, \n\t\tincrease this value above 4. When this value is reached, pinger will assume the target is unreachable.\n" +
                             "\t-q: \tMute default audible alarms. By default, pinger will beep when the status changes in the following instance.\n\t\t> 2 beeps when Status transitions from Timeout to Pingable\n\t\t> 4 beeps when Status transitions from Pingble to TimeOut\n" +
                             "\t-f: \tFastping makes pinger starts a new poll as soon it receives the previous response. Fastping is automatically \n\t\tactivated when the Round Trip is above 1 seconds. Use in combination with the '-s' switch.\n" +
                             "\t-csv: \tSaves all onscreen responses to a CSV. Does not yet take any arguments. The resultant CSV is prefixed with \n\t\tthe target name in your current directory.\n" +
                             "\t-csvall:Saves all ping results to a CSV even regardless what's onscreen. Useful when wanting only the differences in\n\t\tresults onscreen but all of the ping results in a CSV. \n\t\tThe resultant CSV is prefixed with the target name in your current directory.\n" +
                             "\t-skipDNSLookup: \tSkip DNS lookup.\n" +
                             "\t-v: \tVerbose output.\n" +
                             "\t-r:\tReturn Code only. Pinger does verbose to screen (0=Pingable,1=failure).\n");
            // +
            //"\nReturn Codes:\n" +
            //"\t0\tSuccessfull Ping.\n" +
            //"\t1\tUnsuccessfull or other errors reported.\n");

            //"\nFuture feature:" + "\n" +
            //"\t-traceroute\tPerform a traceroute on failure" + "\n" +
            //"\t-webcheck <fullURL>:\tPerform a url check on failure" + "\n");
            logThis("Examples: \n" +
                            "\tSmart ping server1, and only report when the status changes\n" +
                             "\t\tmono pinger.exe server1\n" +
                             "\tSmart ping multiple servers and only report when the status changes\n" +
                             "\t\tmono pinger.exe server1,server2,server3\n" +
                             "\tRun a standard ping on a single server 10 times\n" +
                             "\t\tmono pinger.exe server1 -s -c 10\n" +
                             "\tRun a standard ping on a single server 10 times but verbose the output and stop the audible noise on status changes \n" +
                             "\t\tmono pinger.exe server1 -s -c 10 -v -q\n");
        }
    }
}
