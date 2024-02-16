This Ping utility alternative aims to deliver more features than the built-in 'ping' utility.
<img width="860" alt="image" src="https://github.com/teivarodiere/pinger/assets/13279923/a877e27a-4c9a-423c-8bf8-21926cb79e7e">


<pre>
Some features are:
- Dns Lookup 1st to obtain all resolvable IPs for a hostname or names from a reverse lookup. Uses local DNS - Cannot specify A DNS server.
- Only verboses on screen when there is a status change
- On status change pinger reports how long the target spent in previous status. ie. 5 minutes in Timeout
- Can specify multiple Ips and hostnames using comma separated list.
- Multiple options such as ping like a standard ping utility (verboses every return echo) using '-s'
- Ctrl+C to cancel the utility which provides a disconnection report (Start, End, and Lenght it was disconnected).
- Can ask pinger to only ping ipv6 addresses for a single target
- Refer to the options below for more options
        
This program is developed under Visual Studio Code under MacOS. 
</pre>
_Installation_
<pre>
[MacOS only]
        a) Download "pinger" from the release to where ever
        b) Set executable permission on pinger via terminal
                ❯ chmod +x ~/Downloads/pinger
        c) Run it 
                ❯ ~/Downloads/pinger
        d) You should get a warning from MacOS like 
                        <img width="270" alt="image" src="https://github.com/teivarodiere/pinger/assets/13279923/55bb87aa-e028-4156-9e78-605337f98e85">
                Then click [Allow Anyway]
                        <img width="720" alt="image" src="https://github.com/teivarodiere/pinger/assets/13279923/9fed5522-d8d3-4678-96f2-9afa5300abcf">
                Then run the code again
                        ❯ ~/Downloads/pinger
                Then click OK
                        <img width="272" alt="image" src="https://github.com/teivarodiere/pinger/assets/13279923/7092abd9-1e9d-4205-803e-cf2cd44b0132">
                This is what it looks like when it works
                        <img width="860" alt="image" src="https://github.com/teivarodiere/pinger/assets/13279923/5bf0bbdc-dfce-4975-ac1f-7999592994f8">

</pre>
_Syntax_
<pre>
pinger [HOST LIST] [OPTIONS], or 
pinger [OPTIONS] [HOST LIST] 

[HOSTS LIST]: 
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

</pre>
**Examples**

_Simple ping_
<pre>❯ pinger google.com.au,sdngad,8.8.8.8
Success | google.com.au (142.251.221.67) 
Success | dns.google (8.8.4.4) 
....Ctrl+C...
--- pinger statistics ---
google.com.au (142.251.221.67): 4 packets transmitted, 0.0% loss
dns.google (8.8.4.4): 4 packets transmitted, 0.0% loss
</pre>

_Same simple ping as before with a bit more info_
<pre>❯ pinger google.com.au,sdngad,8.8.8.8 -v
3 hosts, 1sec intervals, ttl=64, RoundTripMaxTimeout 1 sec
Target 1: google.com.au (142.251.221.67) DnsOK
Target 0: sdngad (nodename nor servname provided, or not known) <- Skipping
Target 3: dns.google (8.8.4.4) DnsOK
google.com.au,142.251.221.67,Success,RT=31ms,ttl=64,Frag=True,replyBuffer=64,count=1
dns.google,8.8.4.4,Success,RT=27ms,ttl=64,Frag=True,replyBuffer=64,count=1
....Ctrl+C...
--- pinger statistics ---
google.com.au (142.251.221.67): 36 packets transmitted, 0.0% loss
dns.google (8.8.4.4): 36 packets transmitted, 0.0% loss
</pre>

_Same simple ping as before with a bit more info, AND ping all resolvable interfaces (ipv4 and ipv6)_
<pre>❯ pinger google.com.au,sdngad,8.8.8.8 -v -i
7 hosts, 1sec intervals, ttl=64, RoundTripMaxTimeout 1 sec
Target 1: google.com.au (142.251.221.67) DnsOK
Target 2: google.com.au-IP-1 (2404:6800:4006:80b::2003) DnsOK
Target 2: sdngad (nodename nor servname provided, or not known) <- Skipping
Target 4: dns.google (8.8.4.4) DnsOK
Target 5: dns.google-IP-2 (8.8.8.8) DnsOK
Target 6: dns.google-IP-3 (2001:4860:4860::8844) DnsOK
Target 7: dns.google-IP-4 (2001:4860:4860::8888) DnsOK
google.com.au,142.251.221.67,Success,RT=71ms,ttl=64,Frag=True,replyBuffer=64,count=1
google.com.au-IP-1,2404:6800:4006:80b::2003,NoReply,RT=-,ttl=-,Frag=-,replyBuffer=-,count=1
dns.google,8.8.4.4,Success,RT=76ms,ttl=64,Frag=True,replyBuffer=64,count=1
dns.google-IP-2,8.8.8.8,Success,RT=24ms,ttl=64,Frag=True,replyBuffer=64,count=1
dns.google-IP-3,2001:4860:4860::8844,NoReply,RT=-,ttl=-,Frag=-,replyBuffer=-,count=1
dns.google-IP-4,2001:4860:4860::8888,NoReply,RT=-,ttl=-,Frag=-,replyBuffer=-,count=1

--- pinger statistics ---
google.com.au (142.251.221.67): 12 packets transmitted, 0.0% loss
google.com.au-IP-1 (2404:6800:4006:80b::2003): 12 packets transmitted, 12 lost, 100% packet loss
dns.google (8.8.4.4): 12 packets transmitted, 0.0% loss
dns.google-IP-2 (8.8.8.8): 12 packets transmitted, 0.0% loss
dns.google-IP-3 (2001:4860:4860::8844): 12 packets transmitted, 12 lost, 100% packet loss
dns.google-IP-4 (2001:4860:4860::8888): 11 packets transmitted, 11 lost, 100% packet loss
</pre>

_Resolve and ping only ipv6 addresses_
<pre>❯ ❯ pinger google.com.au,sdngad,8.8.8.8 -i -ipv6
NoReply | google.com.au (2404:6800:4006:809::2003)  
NoReply | dns.google (2001:4860:4860::8844)  
NoReply | dns.google-IP-1 (2001:4860:4860::8888)  
...Ctrl+C
--- pinger statistics ---
google.com.au (2404:6800:4006:809::2003): 2 packets transmitted, 2 lost, 100% packet loss
dns.google (2001:4860:4860::8844): 2 packets transmitted, 2 lost, 100% packet loss
dns.google-IP-1 (2001:4860:4860::8888): 2 packets transmitted, 2 lost, 100% packet loss
</pre>

_Example of disconnection report_
<pre>
❯ pinger google.com.au -v -i
2 hosts, 1sec intervals, ttl=64, RoundTripMaxTimeout 1 sec
Target 1: google.com.au (142.251.221.67) DnsOK
Target 2: google.com.au-IP-1 (2404:6800:4006:809::2003) DnsOK
google.com.au,142.251.221.67,Success,RT=25ms,ttl=64,Frag=True,replyBuffer=64,count=1
google.com.au-IP-1,2404:6800:4006:809::2003,NoReply,RT=-,ttl=-,Frag=-,replyBuffer=-,count=1
google.com.au,142.251.221.67,NoReply,RT=-,ttl=-,Frag=-,replyBuffer=-,count=9(In previous state [Success] for 8 seconds)
google.com.au,142.251.221.67,Success,RT=25ms,ttl=64,Frag=True,replyBuffer=64,count=10(In previous state [TimedOut] for 3 seconds)
....Ctrl+C...
--- pinger statistics ---
google.com.au (142.251.221.67): 15 packets transmitted, 1 lost(Unreachable for a Total of 3 seconds), 6.67% packet loss
        : ----- Disconnection Report ------
        : (3 seconds) Between 2/16/2024 12:10:20PM - 2/16/2024 12:10:23PM
google.com.au-IP-1 (2404:6800:4006:809::2003): 14 packets transmitted, 14 lost, 100% packet loss
</pre>
_Simple ping but skip DNS lookup_
<pre>
❯  pinger 8.8.8.8 -v -i -skipDnslookup
1 hosts, 1sec intervals, ttl=64, RoundTripMaxTimeout 1 sec
Target 1: 8.8.8.8 (DNS resolution skipped)
8.8.8.8,8.8.8.8,Success,RT=26ms,ttl=64,Frag=True,replyBuffer=64,count=1

--- pinger statistics ---
8.8.8.8 (8.8.8.8): 4 packets transmitted, 0.0% loss
</pre>
