<pre>Syntax  :
Pinger <hosts> [OPTIONS]
[HOSTS]: 
        single or multiple hostnames,fqdn,ipv4, and ipv6 IP addresses. Must comma separate (no spaces).
        
[OPTIONS]:
        -n:     Pinger runs once then exists
                -d <n>: Set the amount of duration in Decimal pinger runs for before exiting - Specify a positive value such as 0.25 for 15 minutes or 1.5 for 1hr20mins.
        -c <n>: Specify how many times pinger will poll before exiting - Specify a positive value 'n' greater than 1.
        -s:     Runs like a Standard ping which prints every ping results onscreen.
        -p <n>: Specify how often (in seconds) Pinger will poll the target. Useful with '-s'. Specify a positive value 'n' greater than 1.
        -t <n>: Set a Round Trip timeout value of 'n' seconds - Default value is 1 seconds. For high latency links above 4000ms latency, 
                increase this value above 4. When this value is reached, pinger will assume the target is unreachable.
        -q:     (Windows only) Mute default audible alarms. By default, pinger will beep when the status changes in the following instance.
                        > 2 beeps when Status transitions from Timeout to Pingable
                > 4 beeps when Status transitions from Pingble to TimeOut
        -f:     Fastping makes pinger starts a new poll as soon it receives the previous response. Fastping is automatically 
                activated when the Round Trip is above 1 seconds. Use in combination with the '-s' switch.
                -csv:   Saves all onscreen responses to a CSV. Does not yet take any arguments. The resultant CSV is prefixed with 
                the target name in your current directory.
                -csvall:Saves all ping results to a CSV even regardless what's onscreen. Useful when wanting only the differences in
                results onscreen but all of the ping results in a CSV. 
                The resultant CSV is prefixed with the target name in your current directory.
        -skipDnsLookup:         Skip DNS lookup.
        -dnsonly:       Ping DNS resolvable targets only from the list.
        -i:     Ping all IP addresses enumerated from the NSLOOKUP query.
        -ipv4:  Ping all IPv4 addresses only.
        -ipv6:  Ping all IPv6 addresses only.
        -v:     Verbose a bit more info on screen.
        -vv:    Verbose .
        -r:     Return Code only. Pinger does verbose to screen (0=Pingable,1=failure).

++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
Example 1# Simple ping
        ❯ pinger google.com.au
        Success | google.com.au (142.250.204.3) 
        
        ....Ctrl+C
        
        --- pinger statistics ---
        google.com.au (142.250.204.3): 4 packets transmitted, 0.0% loss
        
Example 2# Ping All interfaces resolved by DNS, and show a bit more info
        ❯ pinger google.com.au -v -i
        2 hosts, 1sec intervals, ttl=64, RoundTripMaxTimeout 1 sec
        Target 1: google.com.au (142.251.221.67) DnsOK
        Target 2: google.com.au-IP-1 (2404:6800:4006:809::2003) DnsOK
        google.com.au,142.251.221.67,Success,RT=25ms,ttl=64,Frag=True,replyBuffer=64,count=1
        google.com.au-IP-1,2404:6800:4006:809::2003,NoReply,RT=-,ttl=-,Frag=-,replyBuffer=-,count=1
        google.com.au,142.251.221.67,NoReply,RT=-,ttl=-,Frag=-,replyBuffer=-,count=9(In previous state [Success] for 8 seconds)
        google.com.au,142.251.221.67,Success,RT=25ms,ttl=64,Frag=True,replyBuffer=64,count=10(In previous state [TimedOut] for 3 seconds)
        
        ....Ctrl+C

        --- pinger statistics ---
        google.com.au (142.251.221.67): 15 packets transmitted, 1 lost(Unreachable for a Total of 3 seconds), 6.67% packet loss
                : ----- Disconnection Report ------
                : (3 seconds) Between 2/16/2024 12:10:20PM - 2/16/2024 12:10:23PM
        google.com.au-IP-1 (2404:6800:4006:809::2003): 14 packets transmitted, 14 lost, 100% packet loss

Example: Simple ping but skip DNS lookup pre-routing
         ❯ pinger 8.8.8.8 -skipDnsLookup
        
Developed under MacOS VSCODE
 
        (MaOS) pinger google.com.au,fd8a:4d23:a340:4960:250:56ff:febb:a99d,192.168.0.1
        (MaOS) mono pinger.exe google.com.au,fd8a:4d23:a340:4960:250:56ff:febb:a99d,192.168.0.1
        (Win)  pinger.exe google.com.au,fd8a:4d23:a340:4960:250:56ff:febb:a99d,192.168.0.1

        Smart ping server1, and only update onscreen when the status changes 
                >pinger server1
        Tranditional ping behavior
                pinger server1 -s
        Smart ping multiple servers and only report when the status changes
                pinger server1,server2,server3
        Run a standard ping on a single server 10 times
                pinger server1 -s -c 10
        Run a standard ping on a single server 10 times but verbose the output and stop the audible noise on status changes 
                pinger server1 -s -c 10 -v -q
</pre>
