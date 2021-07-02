# What is pinger

This is an alternative ping with some features not yet found in ping.

# Some Features
- Attempts to perform a DNS lookup unless you specify to skip DNS lookups
- By default, pinger displays a new line if and when the ping status changes
- If switch '-s' is specified, pinger displays a new line for every ping results (behaving like a normal ping)
- Can specify a single target device or multiple targets via fqdn, shortname, and IPs. A comma ',' must separate each host with not spaces
- Can specify ping Timeouts - give up trying to reach remote host after 'n' number of seconds
- Bypass DNS lookups
- Issues an audible beep when there is a ping status change - usefull when not looking at screen. Can be silenced with '-q'
- Specify pinger to run for a specified amount of time (in minutes) or for a number of ping count
- Export to CSV the results as it is while running
- Colour coded outputs to distinguish between successfully pings, timedout, unknownIP, and other status


# Features under development
- Trying to make ipng Asynmetric rather than sequential for several reasons
- Enable pinger to perform a DNS lookup using specified DNS rather than the current system's DNS server
- Better handling DNS lookup returns multiple IPs - make a decision to ping all IPs or a single IP from the lookup
- Handle IPs when reverse lookups are not configured
- Specify MTU size
- Web URL check as well as ping
- Provide some kind of summary like ping does


# Syntax and Options

Run pinger without options will display the following.
```
Syntax  : Pinger.exe <hosts> [OPTIONS]<br>
<br>
[HOSTS]:<br>
	single host or multiple hosts comma separated (no spaces).<br>
<br>
[OPTIONS]:<br>
	-n:	Pinger runs once then exists<br>
	-h <n>: Set the amount of time (in hours) pinger runs for before exiting - Specify a positive value 'n' greater than 1.
	-c <n>: Specify how many times pinger will poll before exiting - Specify a positive value 'n' greater than 1.
	-s:	Runs like a Standard ping which prints every ping results onscreen.
	-p <n>:	Specify how often (in seconds) Pinger will poll the target. Useful with '-s'. Specify a positive value 'n' greater than 1.
	-t <n>:	Set a Round Trip timeout value of 'n' seconds - Default value is 4 seconds. For high latency links above 4000ms latency,
		increase this value above 4. When this value is reached, pinger will assume the target is unreachable.
	-q: 	Mute default audible alarms. By default, pinger will beep when the status changes in the following instance.
		> 2 beeps when Status transitions from Timeout to Success
		> 4 beeps when Status transitions from Success to Timeout
	-f: 	Fastping makes pinger starts a new poll as soon it receives the previous response. Fastping is automatically
		activated when the Round Trip is above 1 seconds. Use in comibnation with the '-s' switch.
	-csv: 	Saves all onscreen responses to a CSV. Does not yet take any arguments. The resultant CSV is prefixed with
		the target name in your current directory.
	-csvall:Saves all ping results to a CSV even regardless what's onscreen. Useful when wanting only the differences in
		results onscreen but all of the ping results in a CSV.
		The resultant CSV is prefixed with the target name in your current directory.
	-skipDNSLookup: 	Skip DNS lookup.
	-v: 	Verbose output.
	-r:	Return Code only. Pinger does verbose to screen (0=success,1=failure).

Examples:
	Smart ping server1, and only report when the status changes
		mono pinger.exe server1
	Smart ping multiple servers and only report when the status changes
		mono pinger.exe server1,server2,server3
	Run a standard ping on a single server 10 times
		mono pinger.exe server1 -s -c 10
	Run a standard ping on a single server 10 times but verbose the output and stop the audible noise on status changes
		mono pinger.exe server1 -s -c 10 -v -q
```
# Sample outputs

```
iterm:~ admin$ mono pinger server1,server2,server3,nas1,router1
Pinging the following hosts at 1sec interval (Round Trip timeout set at 1 seconds)
Hostname 0: server1.acme.local  (192.168.0.1)
Hostname 1: server2.acme.local  (192.168.0.20)
Hostname 2: server3.acme.local  (192.168.0.18)
Hostname 3: nas1.acme.local  (192.168.0.19)
Hostname 4: router1.acme.local  (192.168.0.200)
Count,TTL,Date,Target,Reply,Time Since Same Reply State (sec),Round Trip Latency (ms)
1,128,3/07/2021 7:17:15 AM,server1.acme.local,Success,1sec,11ms
1,128,3/07/2021 7:17:15 AM,server2.acme.local,Success,1sec,8ms
1,128,3/07/2021 7:17:15 AM,server3.acme.local,Success,1sec,7ms
1,128,3/07/2021 7:17:15 AM,nas1.acme.local,Success,1sec,9ms
1,128,3/07/2021 7:17:15 AM,router1.acme.local,Success,1sec,10ms
183,128,3/07/2021 7:18:44 AM,server2.acme.local,TimedOut,90sec,2005ms
84,128,3/07/2021 7:18:51 AM,NAS1.acme.local,TimedOut,97sec,2006ms
87,128,3/07/2021 7:19:16 AM,server1.acme.local,TimedOut,122sec,2009ms
87,128,3/07/2021 7:19:20 AM,server3.acme.local,TimedOut,126sec,2012ms
88,128,3/07/2021 7:19:41 AM,router1.acme.local,TimedOut,147sec,2009ms
121,128,3/07/2021 7:31:18 AM,router1.acme.local,Success,697sec,10ms
126,128,3/07/2021 7:32:35 AM,server1.acme.local,Success,800sec,10ms
126,128,3/07/2021 7:32:38 AM,server3.acme.local,Success,798sec,9ms
130,128,3/07/2021 7:33:14 AM,nas1.acme.local,Success,863sec,6ms
136,128,3/07/2021 7:33:42 AM,server2.acme.local,Success,899sec,146ms
```