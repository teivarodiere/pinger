# This is an alternative to the traditional PING alternative. Here is a quick comparision: 

Same as ping in the following way:
- It pings and displays every responses/results when you use the pinger '-s' switch

Not the same as ping in the following way:
- Without any switches pinger polls and displays the response each time the current response differs from the previous.
- The output displays various columns which can be imported into excel or calc via a CSV format (Comma delimited)
- You can control the ping intervals
- You can use the verbose switch '-v' to displays the following extra properties for each ping
- It performs a DNS resolution before the ping starts
	 ++++++++++++++++++++++++++++++++++++++++++++
	 VERBOSE:
		 hostPingCount = 660
		 hostNameOrAddress = google.com.au
		 dnsName = google.com.au
		 dnsipaddr = 216.58.203.99
		 dnsReplyIPaddr = 216.58.203.99
		 dnsLookupStatus = Success
		 pingStatus = Success
		 pingStatusPrevious = Unreachable
		 pingReplyRoundTripInMiliSec = 12
		 errorMsg = -
		 errorCode = 0
		 hostUnreachableCount = 13
		 hostReachableCount = 647
		 startDate = 16-Oct-17 10:56:45 AM
		 endDate = 01-Jan-01 12:00:00 AM
		 dateLatestStatus = 16-Oct-17 10:56:55 AM
		 dateLatestStatusPrevious = 16-Oct-17 10:56:45 AM
	++++++++++++++++++++++++++++++++++++++++++++


Command examples: 
#########################################################
# PING without anything shows the syntax
#########################################################

C:\>pinger.exe

Pinger is a custom ping utility written by Teiva Rodiere
Syntax  : Pinger.exe <host> [OPTIONS]
[OPTIONS]:
        -n:     No loop. Stops pinger after one attempt
        -r:     Return Code only. Pinger does not output anything to screen.
        -s:     Old switch. Pinger behaves the same way the traditional ping
                It displays every ping output to screen
        -p <n>: Polling period. Every 'n' seconds
        -t <n>: Timeout value. The script waits for 'n' seconds before calling it a ping timeout.
        -v:     Verbose output

Return Codes:
        0       Successfull Ping
        1       Unsuccessfull or other errors reported

Actions on Failure (To be implemented):
        -traceroute     Perform a traceroute on failure
        -webcheck <fullURL>:    Perform a url check on failure


#########################################################
# PING device (The default behaviour)
#########################################################
C:\>pinger.exe google.com.au
Pinging google.com.au at 1sec interval & timeout of 1 seconds
Looking up DNS :
      Hostname : google.com.au
      IPAddress: 216.58.203.99

poltime,Target Device,Reply,Round Trip (ms),TTL,Ping Count

16-Oct-17 11:04:54 AM,google.com.au,Success,12ms,56,1
16-Oct-17 11:13:40 AM,google.com.au,HostUnreachable,12ms,56,547
16-Oct-17 11:13:53 AM,google.com.au,Success,12ms,56,660

#########################################################
# PING device ONCE
#########################################################
C:\>pinger google.com.au -n
Pinging google.com.au at 1sec interval & timeout of 1 seconds
Looking up DNS :
      Hostname : google.com.au
      IPAddress: 216.58.203.99

poltime,Target Device,Reply,Round Trip (ms),TTL,Ping Count

16-Oct-17 11:07:56 AM,google.com.au,Success,12ms,56,1

#########################################################
# PING device every 10 seconds and verbose the output
#########################################################
C:\Users\trodiere\Google Drive\Devshed\Pinger\Pinger\bin\Release>pinger google.com.au -p 10 -v
Pinging google.com.au at 10sec interval & timeout of 1 seconds
Looking up DNS :
      Hostname : google.com.au
      IPAddress: 216.58.203.99

poltime,Target Device,Reply,Round Trip (ms),TTL,Ping Count

++++++++++++++++++++++++++++++++++++++++++++
VERBOSE:
     hostPingCount = 1
     hostNameOrAddress = google.com.au
     dnsName = google.com.au
     dnsipaddr = 216.58.203.99
     dnsReplyIPaddr = -
     dnsLookupStatus = Success
     pingStatus = -
     pingStatusPrevious = -
     pingReplyRoundTripInMiliSec = 0
     errorMsg = -
     errorCode = 0
     hostUnreachableCount = 0
     hostReachableCount = 0
     startDate = 16-Oct-17 11:09:29 AM
     endDate = 01-Jan-01 12:00:00 AM
     dateLatestStatus = 16-Oct-17 11:09:29 AM
     dateLatestStatusPrevious = 01-Jan-01 12:00:00 AM
poltime=16-Oct-17 11:09:29 AM,trgt=google.com.au,status=Success,rndtrip=12ms,ttl=56,pcount1
++++++++++++++++++++++++++++++++++++++++++++
VERBOSE:
     hostPingCount = 2
     hostNameOrAddress = google.com.au
     dnsName = google.com.au
     dnsipaddr = 216.58.203.99
     dnsReplyIPaddr = 216.58.203.99
     dnsLookupStatus = Success
     pingStatus = Success
     pingStatusPrevious = -
     pingReplyRoundTripInMiliSec = 12
     errorMsg = -
     errorCode = 0
     hostUnreachableCount = 0
     hostReachableCount = 1
     startDate = 16-Oct-17 11:09:29 AM
     endDate = 01-Jan-01 12:00:00 AM
     dateLatestStatus = 16-Oct-17 11:09:39 AM
     dateLatestStatusPrevious = 16-Oct-17 11:09:29 AM



+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
[Feature not yet implemented]

# to perform a traceroute 
C:\pinger -s <dns or ip> -traceroute

# to perform a web url check and get the code
C:\pinger -s <dns or ip> -webcheck http://google.com

TTL Values reply by OS

 /*
                Device / OS     Version Protocol    TTL
                AIX         TCP     60
                AIX UDP     30
                AIX     3.2, 4.1    ICMP    255
                BSDI BSD/ OS 3.1 and 4.0  ICMP    255
                Compa Tru64 v5.0  ICMP    64
                Cisco ICMP    254
                DEC Pathworks   V5 TCP and UDP     30
                Foundry ICMP    64
                FreeBSD     2.1R TCP and UDP     64
                FreeBSD     3.4, 4.0    ICMP    255
                FreeBSD     5   ICMP    64
                HP - UX   9.0x TCP and UDP     30
                HP - UX   10.01   TCP and UDP     64
                HP - UX   10.2    ICMP    255
                HP - UX   11  ICMP    255
                HP - UX   11  TCP     64
                Irix    5.3     TCP and UDP     60
                Irix    6.x     TCP and UDP     60
                Irix    6.5.3, 6.5.8    ICMP    255
                juniper ICMP    64
                MPE / IX(HP)         ICMP    200
                Linux   2.0.x kernel ICMP    64
                Linux   2.2.14 kernel ICMP    255
                Linux   2.4 kernel ICMP    255
                Linux Red Hat 9   ICMP and TCP    64
                MacOS / MacTCP    2.0.x   TCP and UDP     60
                MacOS / MacTCP    X(10.5.6)  ICMP / TCP / UDP    64
                NetBSD ICMP    255
                Netgear FVG318      ICMP and UDP    64
                OpenBSD     2.6 & 2.7   ICMP    255
                OpenVMS     07.01.2002  ICMP    255
                OS / 2    TCP / IP 3.0      64
                OSF / 1   V3.2A TCP     60
                OSF / 1   V3.2A UDP     30
                Solaris     2.5.1, 2.6, 2.7, 2.8    ICMP    255
                Solaris     2.8     TCP     64
                Stratus TCP_OS  ICMP    255
                Stratus TCP_OS (14.2 -)  TCP and UDP     30
                Stratus     TCP_OS(14.3 +)  TCP and UDP     64
                Stratus     STCP    ICMP / TCP / UDP    60
                SunOS   4.1.3 / 4.1.4     TCP and UDP     60
                SunOS   5.7     ICMP and TCP    255
                Ultrix  V4.1 / V4.2A  TCP     60
                Ultrix  V4.1 / V4.2A  UDP     30
                Ultrix  V4.2 – 4.5  ICMP    255
                VMS / Multinet        TCP and UDP     64
                VMS / TCPware         TCP     60
                VMS / TCPware         UDP     64
                VMS / Wollongong  1.1.1.1     TCP     128
                VMS / Wollongong  1.1.1.1     UDP     30
                VMS / UCX         TCP and UDP     128
                Windows     for Workgroups TCP and UDP     32
                Windows     95  TCP and UDP     32
                Windows     98  ICMP    32
                Windows     98, 98 SE   ICMP    128
                Windows     98  TCP     128
                Windows     NT 3.51     TCP and UDP     32
                Windows     NT 4.0  TCP and UDP     128
                Windows     NT 4.0 SP5 - 32
                Windows     NT 4.0 SP6 + 128
                Windows     NT 4 WRKS SP 3, SP 6a   ICMP    128
                Windows     NT 4 Server SP4     ICMP    128
                Windows     ME  ICMP    128
                Windows     2000 pro    ICMP / TCP / UDP    128
                Windows     2000 family     ICMP    128
                Windows     Server 2003         128
                Windows     XP  ICMP / TCP / UDP    128
                Windows     Vista   ICMP / TCP / UDP    128
                Windows     7   ICMP / TCP / UDP    128
                Windows     Server 2008     ICMP / TCP / UDP    128
                Windows     10  ICMP / TCP / UDP    128
                */
+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++