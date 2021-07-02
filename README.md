# pinger
This is a custom ping utility. The main features are  - When the ping returns the same status, nothing else is printed on the screen until such time as the status differes.  - You can request a return code only so you can batch it.

Pinger is a custom ping utility written by Teiva Rodiere (Build 1.7.0.0)
Syntax  : Pinger.exe <hosts> [OPTIONS]
[HOSTS]:
	single host or multiple hosts comma separated (no spaces).

[OPTIONS]:
	-n:	Pinger runs once then exists
	-h <n>: Set the amount of time (in hours) pinger runs for before exiting - Specify a positive value 'n' greater than 1.
	-c <n>: Specify how many times pinger will poll before exiting - Specify a positive value 'n' greater than 1.
	-s:	Runs like a Standard ping which prints every ping results onscreen.
	-p <n>:	Specify how often (in seconds) Pinger will poll the target. Useful with '-s'. Specify a positive value 'n' greater than 1.
	-t <n>:	Set a Round Trip timeout value of 'n' seconds - Default value is 1 seconds. For high latency links above 4000ms latency,
		increase this value above 4. When this value is reached, pinger will assume the target is unreachable.
	-q: 	Mute default audible alarms. By default, pinger will beep when the status changes in the following instance.
		> 2 beeps when Status transitions from Timeout to Pingable
		> 4 beeps when Status transitions from Pingble to TimeOut
	-f: 	Fastping makes pinger starts a new poll as soon it receives the previous response. Fastping is automatically
		activated when the Round Trip is above 1 seconds. Use in combination with the '-s' switch.
	-csv: 	Saves all onscreen responses to a CSV. Does not yet take any arguments. The resultant CSV is prefixed with
		the target name in your current directory.
	-csvall:Saves all ping results to a CSV even regardless what's onscreen. Useful when wanting only the differences in
		results onscreen but all of the ping results in a CSV.
		The resultant CSV is prefixed with the target name in your current directory.
	-skipDNSLookup: 	Skip DNS lookup.
	-v: 	Verbose output.
	-r:	Return Code only. Pinger does verbose to screen (0=Pingable,1=failure).

Examples:
	Smart ping server1, and only report when the status changes
		mono pinger.exe server1
	Smart ping multiple servers and only report when the status changes
		mono pinger.exe server1,server2,server3
	Run a standard ping on a single server 10 times
		mono pinger.exe server1 -s -c 10
	Run a standard ping on a single server 10 times but verbose the output and stop the audible noise on status changes
		mono pinger.exe server1 -s -c 10 -v -q
