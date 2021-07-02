# What is pinger

This is an alternative ping with some features not yet found in ping.

# Some Features
- Attempts to perform a DNS lookup unless you specify to skip DNS lookups
- By default, pinger displays a new line if and when the ping status changes
- If switch '-s' is specified, pinger displays a new line for every ping results (behaving like a normal ping)
- Can specify a single target device or multiple targets via fqdn, shortname, and IPs. A comma ',' must separate each host with not spaces
- Can specify ping Timeouts - give up trying to reach remote host after 'n' number of seconds
- Bypass DNS lookups
- Sound beeps when connecting and disconnecting
- Specify pinger to run for minutes or number of counts
- Export to CSV the results (while running)
- Quiet the beeps
- Colour coded outputs to distinguish between successfully pings, timedout, unknownIP
- Performs DNS lookups
- Can ping and report every checks or only report when there is a status change

# Features under or to develop
- Ability to specify DNS servers  to use
- Fix bug when DNS lookup returns multiple IPs 
- Handle IPs when reverse lookups are not configured
- Specify MTU size
- Summary
- Web URL check as well as ping


# Syntax and Options

Run pinger without options will display the following.

<p><code>Syntax  : Pinger.exe <hosts> [OPTIONS]<br>
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
</code></p>
# Sample outputs


