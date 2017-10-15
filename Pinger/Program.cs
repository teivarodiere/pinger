// Syntax 
// use 'pinger.exe /?' for help
// 
using System;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;
using System.Net;

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
        private string replyDnsipaddr;
        private long replyRoundTrip;
        private string dnsLookupStatus;
        private string status;
        private string statusPrevious;        
        private string errorMsg;
        private int pingCount;
        private int errorCode;
        private string optionsTtl;
        private int offlineCount;
        private int onlineCount;
        private DateTime startDate;
        private DateTime endDate;
        private DateTime statusDate;
        private DateTime statusDatePrevious;

        public PingerTarget(string targetName)
        {
            this.hostNameOrAddress = targetName;
            this.dnsName = "-";
            this.dnsipaddr = "-";
            this.replyDnsipaddr = "-";
            this.replyRoundTrip = 0;
            this.dnsLookupStatus = "-";
            this.status = "-";
            this.statusPrevious = "-";
            this.errorMsg = "-";
            this.optionsTtl = "-";
            this.errorCode = 0; // No Errors
            this.offlineCount = 0;
            this.onlineCount = 0;
            this.pingCount = 0;
        }

        public PingerTarget()
        {
            this.hostNameOrAddress = "-";
            this.dnsName = "-";
            this.dnsipaddr = "-";
            this.replyDnsipaddr = "-";
            this.replyRoundTrip = 0;
            this.dnsLookupStatus = "-";
            this.status = "-";
            this.statusPrevious = "-";
            this.errorMsg = "-";
            this.optionsTtl = "-";
            this.errorCode = 0; // No Errors
            this.offlineCount = 0;
            this.onlineCount = 0;
            this.pingCount = 0;
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
            get { return replyDnsipaddr; }
            set { replyDnsipaddr = value; }
        }
        
        public string Status
        {
            get { return status; }
            set {
                statusPrevious = status;
                status = value;              
            }
        }
        public int OnlineCount
        {
            get { return onlineCount;  }
            set { onlineCount = value; }
        }
        public int OfflineCount
        {
            get { return offlineCount; }
            set { offlineCount = value; }
        }

        public string PreviousStatus
        {
            get { return statusPrevious; }
        }
        public int Errorcode
        {
            get { return errorCode; }
            set {
                errorCode = value;
                if(errorCode == 0)
                {
                    onlineCount++;
                } else if (errorCode == 1)
                {
                    offlineCount++;
                }
            }
        }

        public string ErrorMsg
        {
            get { return errorMsg; }
            set { errorMsg = value; }
        }

        public int PingCount
        {
            get { return pingCount; }
            set { pingCount = value; }
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

        public DateTime StatusDate
        {
            get { return statusDate; }
            set {
                statusDatePrevious = statusDate;
                statusDate = DateTime.Now;
            }
        }        
        public long RoundTrip
        {
            get { return replyRoundTrip; }
            set { replyRoundTrip = value; }
        }
        public void Printout()
        {
            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine("VERBOSE:");
            Console.WriteLine("     pingCount = " + pingCount);
            Console.WriteLine("     hostNameOrAddress = " + hostNameOrAddress);
            Console.WriteLine("     dnsName = " + dnsName);
            Console.WriteLine("     dnsipaddr = " + dnsipaddr);
            Console.WriteLine("     replyDnsipaddr = " + replyDnsipaddr);
            Console.WriteLine("     dnsLookupStatus = " + dnsLookupStatus);
            Console.WriteLine("     status = " + status);
            Console.WriteLine("     statusPrevious = " + statusPrevious);
            Console.WriteLine("     errorMsg = " + errorMsg);
            Console.WriteLine("     errorCode = " + errorCode);
            Console.WriteLine("     replyRoundTrip = " + replyRoundTrip);
            Console.WriteLine("     offlineCount = " + offlineCount);
            Console.WriteLine("     onlineCount = " + onlineCount);
            Console.WriteLine("     startDate = " + startDate);
            Console.WriteLine("     endDate = " + endDate);
            Console.WriteLine("     statusDate = " + statusDate);
            Console.WriteLine("     statusDatePrevious = " + statusDatePrevious);
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
            bool smartping = false;
            bool return_code_only = false;
            string target = ""; // target IP address or DNS name to ping
            int defaultPollingTimeInMilliseconds = 1000; //iteration defaultPollingTimeInMilliseconds in ms or can be seen as polling
            int sleeptime = defaultPollingTimeInMilliseconds;               
            int runtimeError = 0;
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
                        smartping = true;
                        loop = true;
                        break;
                    case "-N":
                        loop = false;
              //          logThis(loop.ToString());
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
                            //bool success = int.TryParse(arguments[argIndex], out sleeptime);
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
            //            logThis("Target = " + target + ", Items =" + items);
                        break;
                }
            }

            if (runtimeError > 0)
                return runtimeError;

            if ( items > 1 || target.Length <= 0 )
            {
                //logThis("Choose to ping test one (1) host at a time (return code=1)"); 
                ShowSyntax();
                runtimeError = 1;
            }
            else
            {
                
                pt.Target = target;
                pt.Startdate = DateTime.Now;
               
                string[] dnsresults = DNSLookup(pt.Target);

                //if (dnsresults[0]) {
                if (dnsresults.Length > 0)
                {
                    pt.Hostname = dnsresults[0];
                    pt.IPAddress = dnsresults[1];
                    pt.DNSLookupStatus = dnsresults[2];
                } else
                {
                    pt.Hostname = pt.Target;
                }
                //}
                //
                //Console.WriteLine("Pinging {0} at {1} sec interval & timeout of {2} seconds", target, sleeptime/1000, timeout/1000);
                logThis("Pinging " + pt.Target +" at "+sleeptime / 1000 +"sec interval & timeout of " + timeout / 1000 +" seconds");
                logThis("DNS Lookup : ");
                logThis("      Hostname : " + pt.Hostname);
                logThis("      IPAddress: " + pt.IPAddress);
                //logThis("      Lookup Status: " + pt.DNSLookupStatus);
                logThis("");
                logThis("poltime,Target Device,Reply,Round Trip (ms),TTL,Ping Count\n");
                
                pt.PingCount = 1;
                do
                {
                    pt.StatusDate = DateTime.Now;
                    //VERBOSE for DEBUG
                    if (verbose) { pt.Printout(); }
                    //VERBOSE for DEBUG

                    try
                    {
                        options.DontFragment = true;
                        PingReply reply;
                        if (pt.IPAddress != null)
                        {
                            reply = pingSender.Send(pt.IPAddress, timeout, buffer, options);
                        } else
                        {
                            reply = pingSender.Send(pt.Hostname, timeout, buffer, options);
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
                        pt.Status = reply.Status.ToString(); //? reply.Status.ToString() : "-");
                                                             // logThis(reply.Status.ToString());
                        if (reply.Status.ToString() == "DestinationHostUnreachable")
                        {
                            pt.Errorcode = 1;
                            pt.Status = "DestinationHostUnreachable";
                        }


                        // Console.WriteLine("pt-ErrorCode = " + pt.Errorcode);
                        //pt.Status = (reply != null ?  : "Access Denied");
                        //status_curr = reply.Status.ToString();

                        //logThis("Previous status=" + pt.PreviousStatus + "  pt.Status=" + pt.Status);

                        if (loop)
                            Thread.Sleep(sleeptime);
                    }
                    catch (System.Net.Sockets.SocketException se)
                    {
                        pt.Errorcode = 1;
                        pt.ErrorMsg = se.Message;
                        pt.Status = se.Message;
                        Thread.Sleep(sleeptime);
                    }
                    catch (System.Net.NetworkInformation.PingException pe)
                    {
                        pt.Errorcode = 1;
                        pt.ErrorMsg = pe.Message;
                        pt.Status = pe.Message;
                        Thread.Sleep(sleeptime);
                    }
                    catch (System.NullReferenceException nre)
                    {
                        pt.Errorcode = 1;
                        pt.ErrorMsg = nre.Message;
                        // pt.Status = nre.Message;
                        pt.Status = "DestinationHostUnreachable";
                        Thread.Sleep(sleeptime);
                    }
                    finally
                    {
                        if (String.Equals(pt.PreviousStatus, pt.Status) && smartping)
                        {
                            // don't print out anything because the previous status is the same as the current. 
                        }
                        else
                        {
                            // Console.WriteLine("In HERE");
                            if (pt.Errorcode == 0)
                            {

                                for (int i = 0; i < 2; i++)
                                {
                                    Console.Beep();
                                }
                            }
                            else if (pt.Errorcode == 1)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    Console.Beep();
                                }
                            }
                            if (!return_code_only && !verbose)
                            {
                                //Console.WriteLine("{0},{1}({2}),{3},{4}ms,{5},{6}", pt.StatusDate, pt.Hostname, (pt.ReplyIPAddress == null ? "(unknown IP)" : pt.ReplyIPAddress), pt.Status, pt.RoundTrip, pt.OptionsTtl, pt.PingCount);
                                Console.WriteLine(pt.StatusDate +","+ pt.Hostname + "," + pt.Status + "," + pt.RoundTrip + "ms," + pt.OptionsTtl + "," + pt.PingCount);
                            }
                            else if (!return_code_only && verbose)
                            {
                                // for situations where you want the column headers inline with the results
                                //Console.WriteLine("poltime={0},trgt={1}(ifAdrr={2}),status={3},rndtrip={4}ms,ttl={5},pcount={6}\n", pt.StatusDate, pt.Hostname, (pt.ReplyIPAddress == null ? "(unknown IP)" : pt.ReplyIPAddress), pt.Status, pt.RoundTrip, pt.OptionsTtl, pt.PingCount);
                                //Console.WriteLine("poltime={0},trgt={1},status={3},rndtrip={4}ms,ttl={5},pcount={6}\n", pt.StatusDate, pt.Hostname, (pt.ReplyIPAddress == null ? "(unknown IP)" : pt.ReplyIPAddress), pt.Status, pt.RoundTrip, pt.OptionsTtl, pt.PingCount);
                                Console.WriteLine("poltime=" + pt.StatusDate + ",trgt=" + pt.Hostname + ",status=" + pt.Status + ",rndtrip=" + pt.RoundTrip + "ms,ttl=" + pt.OptionsTtl + ",pcount" + pt.PingCount);
                            }
                            else
                                Console.Write("\n");

                        }
                    }
                    pt.PingCount = pt.PingCount+1;
                //} while (loop || loopcount <= maxloopcount);
                } while (loop );

               // Set return codes
/*                if (!status)
                {
                    if (verbose) logThis("Bad end to this script  (return Code 1)");
                    error = 1;
                }
 * */
            }
            return pt.Errorcode;
        }


        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        static public void logThis(string msg)
        {
            Console.WriteLine(msg);
        }
        static public void verboseDEBUG(PingerTarget ptTemp)
        {

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
            logThis("[OPTIONS]: \n"+
                             "\t-n:\tNo loop. Stops pinger after one attempt \n"+
                             "\t-r:\tReturn Code only. Pinger does not output anything to screen.\n"+
                             "\t-s:\tSmart switch. Pinger only shows pinger response \n\t\tif the current ping status is different to the last one \n"+
                             "\t-p <n>:\tPolling period. Every 'n' seconds\n"+
                             "\t-t <n>:\tTimeout value. The script waits for 'n' seconds before calling it a ping timeout.\n" +
                             "\t-v: \tVerbose output\n" +
                             "\nReturn Codes:" + "\n" +
                             "\t0\tSuccessfull Ping" + "\n" +
                             "\t1\tUnsuccessfull or other errors reported" + "\n" +
                             "\nActions on Failure (To be implemented):" + "\n" +
                             "\t-traceroute\tPerform a traceroute on failure" + "\n" +
                             "\t-webcheck <fullURL>:\tPerform a url check on failure" + "\n");
        }
    }
}
