// Syntax 
// use 'pinger.exe /?' for help
// 
using System;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;
using System.Net;
using System.IO;

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
        private string pingStatus;
        private string pingStatusPrevious;        
        private string errorMsg;
        private int hostPingCount;
        private int errorCode;
        private string optionsTtl;
        private int hostUnreachableCount;
        private int hostReachableCount;
        private DateTime startDate;
        private DateTime endDate;
        private DateTime dateLatestStatus;
        private DateTime dateLatestStatusPrevious;

        public PingerTarget(string targetName)
        {
            this.hostNameOrAddress = targetName;
            this.dnsName = "-";
            this.dnsipaddr = "-";
            this.dnsReplyIPaddr = "-";
            this.pingReplyRoundTripInMiliSec = -1;
            this.dnsLookupStatus = "-";
            this.pingStatus = "-";
            this.pingStatusPrevious = "-";
            this.errorMsg = "-";
            this.optionsTtl = "-";
            this.errorCode = -1; // No Errors
            this.hostUnreachableCount = 0;
            this.hostReachableCount = 0;
            this.hostPingCount = 0;
        }

        public PingerTarget()
        {
            this.hostNameOrAddress = "-";
            this.dnsName = "-";
            this.dnsipaddr = "-";
            this.dnsReplyIPaddr = "-";
            this.pingReplyRoundTripInMiliSec = 0;
            this.dnsLookupStatus = "-";
            this.pingStatus = "-";
            this.pingStatusPrevious = "-";
            this.errorMsg = "-";
            this.optionsTtl = "-";
            this.errorCode = -1; // No Errors
            this.hostUnreachableCount = 0;
            this.hostReachableCount = 0;
            this.hostPingCount = 0;
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
        public string ReplyIPAddress
        {
            get { return dnsReplyIPaddr; }
            set { dnsReplyIPaddr = value; }
        }
        
        public string PingStatus
        {
            get { return pingStatus; }
            set {
                pingStatusPrevious = pingStatus;
                pingStatus = value;              
            }
        }
        public int HostReachableCountUpdate
        {
            get { return hostReachableCount;  }
            set { hostReachableCount = value; }
        }
        public int HostUnreachableCountUpdate
        {
            get { return hostUnreachableCount; }
            set { hostUnreachableCount = value; }
        }

        public string PreviousPingStatus
        {
            get { return pingStatusPrevious; }
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

        public int HostPingCount
        {
            get { return hostPingCount; }
            set { hostPingCount = value; }
        }
        public string OptionsTtl
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

        public DateTime DateLatestStatus
        {
            get { return dateLatestStatus; }
            set {
                dateLatestStatusPrevious = dateLatestStatus;
                dateLatestStatus = DateTime.Now;
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
            Console.WriteLine("     hostPingCount = " + this.hostPingCount);
            Console.WriteLine("     hostNameOrAddress = " + hostNameOrAddress);
            Console.WriteLine("     dnsName = " + dnsName);
            Console.WriteLine("     dnsipaddr = " + dnsipaddr);
            Console.WriteLine("     dnsReplyIPaddr = " + dnsReplyIPaddr);
            Console.WriteLine("     dnsLookupStatus = " + dnsLookupStatus);
            Console.WriteLine("     pingStatus = " + pingStatus);
            Console.WriteLine("     pingStatusPrevious = " + pingStatusPrevious);
            Console.WriteLine("     pingReplyRoundTripInMiliSec = " + pingReplyRoundTripInMiliSec);
            Console.WriteLine("     errorMsg = " + errorMsg);
            Console.WriteLine("     errorCode = " + errorCode);            
            Console.WriteLine("     hostUnreachableCount = " + hostUnreachableCount);
            Console.WriteLine("     hostReachableCount = " + hostReachableCount);
            Console.WriteLine("     startDate = " + startDate);
            Console.WriteLine("     endDate = " + endDate);
            Console.WriteLine("     dateLatestStatus = " + dateLatestStatus);
            Console.WriteLine("     dateLatestStatusPrevious = " + dateLatestStatusPrevious);
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
            bool loop = true; // true = ping will loop until Ctrl + C is pressed
            int items = -1; // compensate for "pinger" counting as 1 command line argument
            int maxcount = 1;
            bool maxCountSpecified = false;
            bool smartping = true; // by default use the smart ping switch
            bool return_code_only = false;
            string target = ""; // target IP address or DNS name to ping
            int defaultPollingTimeInMilliseconds = 1000; //iteration defaultPollingTimeInMilliseconds in ms or can be seen as polling
            bool stopBeeps = true;
            bool outputScreenToCSV = false; // Output only what's on the screen to CSV. So if printing only changes then it will only output to file that
            bool outputAllToCSV = false; // Output every ping response to CSV, even if you are using the smart ping function which only prints the status changes
            string outputCSVFilename="";
            string outstr = "";
            int sleeptime = defaultPollingTimeInMilliseconds;               
            int runtimeError = 0;
            int runtimeInHours = 0;
            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000; // amount of time in ms to wait for the ping reply
                // Use the default Ttl value which is 128,
                // but change the fragmentation behavior.            
               
            // iterate through the arguments
            string[] arguments = Environment.GetCommandLineArgs();
            for (int argIndex=0; argIndex < arguments.Length; argIndex++)
            {
                //logThis("Arguments " + arg);
                switch (arguments[argIndex].ToUpper())
                {
                    case "/?":
                        ShowSyntax();
                        runtimeError = 1;
                        break;
                    case "-V":
                        verbose = true;
                        break;
                    case "-S": // smart switch
                        // Show OK and Down and OK, implies -C
                        smartping = false;
                        loop = true;
                        break;
                    case "-N":
                        loop = false;
                        maxcount = 1;
                        break;
                    case "-Q":
                        //verbose = false;
                        stopBeeps = true;
                        break;
                    case "-C":
                        //verbose = false;
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
                    case "-H":
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            //bool success = int.TryParse(arguments[argIndex], out sleeptime);
                            runtimeInHours = int.Parse(arguments[argIndex]);
                            maxCountSpecified = false;
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
                    case "-CSV":
                        //verbose = false;
                        outputScreenToCSV = true;
                        outputAllToCSV=false;
                        break;
                    case "-CSVALL":
                        //verbose = false;
                        outputScreenToCSV = false;
                        outputAllToCSV = true;
                        break;
                    case "-R":
                        //verbose = false;
                        smartping = false;
                        loop = false;
                        return_code_only = true;
                        break;
                    case "-P": // smart switch
                        // Show OK and Down and OK, implies -C
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            //bool success = int.TryParse(arguments[argIndex], out sleeptime);
                            sleeptime = int.Parse(arguments[argIndex]) * 1000;
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
                    case "-T": // smart switch
                        // Show OK and Down and OK, implies -C
                        try
                        {
                            argIndex++; // get the next value, hopefully a digit
                            timeout = int.Parse(arguments[argIndex]) * 1000;
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
                        if (items == 0) target = arguments[argIndex];
                        items++; 
                        break;
                }
            }

            if (runtimeError > 0)
                return runtimeError;

            if ( items > 1 || target.Length <= 0 )
            {
                ShowSyntax();
                runtimeError = 1;
            }
            else
            {
                
                pt.Target = target;
                pt.Startdate = DateTime.Now;
               
                string[] dnsresults = DNSLookup(pt.Target);

                if (dnsresults.Length > 0)
                {
                    pt.Hostname = dnsresults[0];
                    pt.IPAddress = dnsresults[1];
                    pt.DNSLookupStatus = dnsresults[2];
                } else
                {
                    pt.Hostname = pt.Target;
                }
                

                if (!return_code_only)
                {
                    logThis("Pinging " + pt.Target + " at " + sleeptime / 1000 + "sec interval & timeout of " + timeout / 1000 + " seconds");
                    logThis("Looking up DNS : ");
                    logThis("      Hostname : " + pt.Hostname);
                    logThis("      IPAddress: " + pt.IPAddress);
                    logThis("");
                    if (!maxCountSpecified && runtimeInHours > 0)
                    {
                        maxcount = (runtimeInHours * 60 * 60) / (sleeptime / 1000);
                        //logThis(">> sleeptime = " + sleeptime / 1000 + ",runtimeInHours=" + runtimeInHours + "hrs,maxcount=" + maxcount +"<<");
                        logThis(">> Runtime: " + runtimeInHours + "hrs, Total ping expected=" + maxcount + " <<");
                    }
                }                
                if (outputScreenToCSV || outputAllToCSV)
                {
                    outputCSVFilename = "pinger-" + pt.Target.Replace('.', '_').Trim() + "_" + ".txt";
                    logThis(">> Responses will be saved to " + outputCSVFilename);
                    logThis("");
                    logToFile("poltime,Target Device,Reply,Round Trip (ms),TTL,Ping Count\n", outputCSVFilename);
                }
                logThis("poltime,Target Device,Reply,Round Trip (ms),TTL,Ping Count\n");
                do
                {
                    pt.DateLatestStatus = DateTime.Now;                    
                    try
                    {
                        options.DontFragment = true;
                        PingReply reply;
                        
                        if (pt.IPAddress != null)
                        {
                            reply = pingSender.Send(pt.IPAddress, timeout, buffer, options);
                            pt.HostPingCount++;
                        } else
                        {
                            reply = pingSender.Send(pt.Hostname, timeout, buffer, options);
                            pt.HostPingCount++;
                        }

                        if (reply != null && reply.Options != null)
                        {
                            pt.OptionsTtl = reply.Options.Ttl.ToString();
                        } else
                        {
                            pt.OptionsTtl = "-";
                        }

                        pt.ReplyIPAddress = reply.Address.ToString(); // ? reply.Address.ToString() : "-";
                        pt.RoundTrip = reply.RoundtripTime; //? reply.RoundtripTime : -1);
                        pt.PingStatus = reply.Status.ToString(); //? reply.Status.ToString() : "-");
                                                             // logThis(reply.Status.ToString());
                        if (reply.Status.ToString() == "DestinationHostUnreachable")
                        {
                            pt.Errorcode = 1;
                            pt.PingStatus = "DestinationHostUnreachable";
                        } else {
                            pt.Errorcode = 0;
                        }
                                                
                    }
                    catch (System.Net.Sockets.SocketException se)
                    {
                        pt.Errorcode = 1; pt.ErrorMsg = se.Message; pt.PingStatus = se.Message; Thread.Sleep(sleeptime);
                    }
                    catch (System.Net.NetworkInformation.PingException pe)
                    {
                        pt.Errorcode = 1; pt.ErrorMsg = pe.Message; pt.PingStatus = pe.Message; Thread.Sleep(sleeptime);
                    }
                    catch (System.NullReferenceException nre)
                    {
                        pt.Errorcode = 1; pt.ErrorMsg = nre.Message; pt.PingStatus = "DestinationHostUnreachable"; Thread.Sleep(sleeptime);
                        //pt.Errorcode = 1; pt.ErrorMsg = nre.Message; pt.PingStatus = nre.Message; Thread.Sleep(sleeptime);
                    }
                    finally
                    {
                        if (pt.Errorcode == 0)
                        {
                            pt.HostReachableCountUpdate++;
                        }
                        else if (pt.Errorcode == 1)
                        {
                            pt.HostUnreachableCountUpdate++;
                        } else
                        {
                            logThis("Unknown ping error code");
                        }

                        // Create the output string
                        if (!verbose)
                        {
                            outstr = pt.DateLatestStatus + "," + pt.Hostname + "," + pt.PingStatus + "," + pt.RoundTrip + "ms," + pt.OptionsTtl + "," + pt.HostPingCount;
                        }
                        else
                        {
                            outstr = "poltime=" + pt.DateLatestStatus + ",trgt=" + pt.Hostname + ",status=" + pt.PingStatus + ",rndtrip=" + pt.RoundTrip + "ms,ttl=" + pt.OptionsTtl + ",pcount" + pt.HostPingCount;
                        }

                        if (String.Equals(pt.PreviousPingStatus, pt.PingStatus) && smartping)
                        {
                            // don't print out anything because the previous status is the same as the current.       
                            if(outputAllToCSV)
                            {
                                logToFile(outstr, outputCSVFilename);
                            }
                        }
                        else
                        {
                            // START BEEPING ON STATUS CHANGE
                            if (pt.Errorcode == 0 && pt.HostPingCount > 1 && smartping && !stopBeeps)
                            {                                
                                for (int i = 0; i < 2; i++)
                                {
                                    Console.Beep();
                                }
                            }
                            else if (pt.Errorcode == 1 && pt.HostPingCount > 1 && smartping && !stopBeeps)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Console.Beep();
                                }
                            }
                            // Print to screen
                            
                            if (outputScreenToCSV || outputAllToCSV)
                            {
                                logToFile(outstr, outputCSVFilename);
                            }
                            logThis(outstr);
                        } 
                    }
                    if (verbose) { pt.Printout(); }
                    if (loop) { Thread.Sleep(sleeptime); }
                } while (loop && pt.HostPingCount < maxcount);
            }
            return pt.Errorcode;
        }


        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        public static void logThis(string msg)
        {
            Console.WriteLine(msg);
        }

        public static void logToFile(string msg, string filename)
        {
            using (StreamWriter w = File.AppendText(filename))
            {
                //Log(msg, w);
                w.WriteLine(msg);
            }
        }
    
        static string[] DNSLookup(string hostNameOrAddress)
        {
            //Console.WriteLine("Lookup: {0}\n", hostNameOrAddress);
            string[] dnsResults;
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostNameOrAddress);
                //Console.WriteLine("  Host Name: {0}", hostEntry.HostName);
                IPAddress[] ips = hostEntry.AddressList;
                foreach (IPAddress ip in ips)
                {
                    //Console.WriteLine("  Address: {0}", ip);
                }
                dnsResults = new string[] { hostEntry.HostName, ips[0].ToString(), "Success"};
            } catch (System.Net.Sockets.SocketException se)
            {
                dnsResults = new string[] { "UnknownHostName", "UnknownIP", se.Message};
            } 
            return dnsResults;
        }
        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        static public void ShowHeader()
        {
            logThis("\nPinger is a custom ping utility written by Teiva Rodiere");
        }
        /// <summary>
        /// Function: ShowSyntax
        /// Information: Display usage information for the application
        /// </summary>
        static public void ShowSyntax()
        {
            // Display Application Syntax
            ShowHeader();
            logThis("Syntax  : Pinger.exe <host> [OPTIONS]");

            // Display Return Codes Information
            //"\t-s:\tSmart switch. Pinger only shows pinger response \n\t\tif the current ping status is different to the last one \n"+
            logThis("[OPTIONS]: \n"+
                             "\t-n:\tNo loop. Stops pinger after one attempt \n"+
                             "\t-c:\tQuit after 'n' number of poll\n" +
                             "\t-r:\tReturn Code only. Pinger does not output anything to screen.\n" +
                             "\t-s:\tOld switch. Pinger behaves the same way the traditional ping \n\t\tIt displays every ping output to screen\n"+
                             "\t-p <n>:\tPolling period. Every 'n' seconds\n" +
                             "\t-t <n>:\tTimeout value. The script waits for 'n' seconds before calling it a ping timeout.\n" +
                             "\t-v: \tVerbose output\n" +
                             "\t-h: \tSpecify to run the script for the next 'n' hours - Specify a positive value\n" +
                             "\t-q: \tTurn of the  Beeps \n" +
                             "\t-csv: \tOutput onscreen ping responses to CSV\n" +
                             "\t-csvall: \tOutput ALL ping responses to CSV even if you are using the smart ping. Onscreen will display changes only but on file it will log everything\n" +
                             "\nReturn Codes:" + "\n" +
                             "\t0\tSuccessfull Ping" + "\n" +
                             "\t1\tUnsuccessfull or other errors reported" + "\n" +
                             "\nActions on Failure (To be implemented):" + "\n" +
                             "\t-traceroute\tPerform a traceroute on failure" + "\n" +
                             "\t-webcheck <fullURL>:\tPerform a url check on failure" + "\n");
        }
    }
}
