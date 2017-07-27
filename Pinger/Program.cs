// Syntax 
// use 'pinger.exe /?' for help
// 
using System;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;

namespace Pinger
{
    class Program
    {

        static int Main(string[] args)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            bool verbose = false; // true = print additional versbose stuff for the program
            bool loop = true; // true = ping will loop until Ctrl + C is pressed
            int items = -1; // compensate for "pinger" counting as 1 command line argument
            bool smartping = false;
            bool return_code_only = false;
            string target = ""; // target IP address or DNS name to ping
            string status_curr = "";
            string status_previous = "not the same";
            int defaultPollingTimeInMilliseconds = 1000; //iteration defaultPollingTimeInMilliseconds in ms or can be seen as polling
            int sleeptime = defaultPollingTimeInMilliseconds;                    
            int runtimeError = 0;
            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;
            
            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            // iterate through the arguments
            string[] arguments = Environment.GetCommandLineArgs();
            for (int argIndex=0; argIndex < arguments.Length; argIndex++)
            {
                //Console.WriteLine("Arguments " + arg);
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
              //          Console.WriteLine(loop.ToString());
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
                            Console.WriteLine("Please specify a valid polling interval in seconds.");
                            runtimeError = 1;
                        }
                        catch (System.FormatException)
                        {
                            Console.WriteLine("Please specify a valid polling interval in seconds.");
                            runtimeError = 1;
                        }
                        catch (System.OverflowException)
                        {
                            Console.WriteLine("Please specify a valid polling interval in seconds.");
                            runtimeError = 1;
                        }
                        catch (System.IndexOutOfRangeException)
                        {
                            Console.WriteLine("Please specify a valid polling interval in seconds.");
                            runtimeError = 1;
                        }
                        break;
                    default:
                        if (items == 0) target = arguments[argIndex];
                        items++; 
            //            Console.WriteLine("Target = " + target + ", Items =" + items);
                        break;
                }
            }

            if (runtimeError > 0)
                return runtimeError;

            if ( items > 1 || target.Length <= 0 )
            {
                //Console.WriteLine("Choose to ping test one (1) host at a time (return code=1)"); 
                ShowSyntax();
                runtimeError = 1;
            }
            else
            {
                Console.WriteLine("Pinging host {0} at {1} seconds interval", target, sleeptime/1000);
                do
                {
                    try
                    {                       
                        PingReply reply = pingSender.Send(target, timeout, buffer, options);
                        
                        status_curr = (reply != null ? reply.Status.ToString() : "Access Denied");
                        //status_curr = reply.Status.ToString();

                        if (String.Equals(status_previous, status_curr) && smartping)
                        {
                        }
                        else
                        {
                            if (!return_code_only && !verbose)
                            {
                                Console.WriteLine("{0},{1}({2}),{3},{4}ms", DateTime.Now, target, (reply.Address == null ? "(unknown IP)" : reply.Address.ToString()), status_curr, reply.RoundtripTime);
                            }
                            else if (!return_code_only && verbose)
                            {
                                Console.WriteLine("{0},{1}(ifAdrr={2}),status={3},rndtrip={4}ms\n", DateTime.Now, target, (reply.Address == null ? "(unknown IP)" : reply.Address.ToString()), status_curr, reply.RoundtripTime);
                            }
                            else
                                Console.Write("\n");

                        }
                        status_previous = status_curr;
                        runtimeError = 0;
                        //Console.WriteLine("status_previous=" + status_previous + "  status_curr=" + status_curr);
                        if (loop)
                            Thread.Sleep(sleeptime);
                    }
                    catch (System.Net.Sockets.SocketException se)
                    {
                        if (!return_code_only) Console.WriteLine("Host is invalid (return code=1) " + se.Message);
                        runtimeError = 1;
                        Thread.Sleep(sleeptime);
                    }
                    catch (System.Net.NetworkInformation.PingException pe)
                    {
                        if (!return_code_only) Console.WriteLine("No network connectivity (return code=1) " + pe.Message);
                        runtimeError = 1;
                        Thread.Sleep(sleeptime);
                    }
                    catch (System.NullReferenceException nre)
                    {
                        if (!return_code_only) Console.WriteLine("Null reference (return code=1) " + nre.Message);
                        runtimeError = 1;
                        Thread.Sleep(sleeptime);
                    }
                    finally
                    {
                        //Console.WriteLine("{0},{1},{2},Status Changed", DateTime.Now, target, status_curr);
                    }
                //} while (loop || loopcount <= maxloopcount);
                } while (loop );

               // Set return codes
/*                if (!status)
                {
                    if (verbose) Console.WriteLine("Bad end to this script  (return Code 1)");
                    error = 1;
                }
 * */
            }
            return runtimeError;
        }
        /// Function: ShowHeader
        /// Information: Displays Author and application details.
        /// </summary>
        static public void ShowHeader()
        {
            Console.WriteLine("\nPinger is a custom ping utility written by Teiva Rodiere");
        }
        /// <summary>
        /// Function: ShowSyntax
        /// Information: Display usage information for the application
        /// </summary>
        static public void ShowSyntax()
        {
            // Display Application Syntax
            ShowHeader();
            Console.WriteLine("Syntax  : Pinger.exe <host> [OPTIONS]");

            // Display Return Codes Information
            Console.WriteLine("[OPTIONS]: \n"+
                             "\t-n:\tNo loop. Stops pinger after one attempt \n"+
                             "\t-r:\tReturn Code only. Pinger does not output anything to screen.\n"+
                             "\t-s:\tSmart switch. Pinger only shows pinger response \n\t\tif the current ping status is different to the last one \n"+
                             "\t-p <n>:\tPolling period. Every 'n' seconds\n"+
                             "\nReturn Codes:" + "\n" +
                             "\t0\tSuccessfull Ping" + "\n" +
                             "\t1\tUnsuccessfull or other errors reported" + "\n" +
                             "\nActions on Failure (To be implemented):" + "\n" +
                             "\t-t\tPerform a traceroute on failure" + "\n" +
                             "\t-w <fullURL>:\tPerform a url check on failure" + "\n");
        }
    }
}
