===================
WARNING
===================
Do NOT run this tool in a live environment!
It will cause a _very_ heavy load on your I/O disk subsystem!!!


Use SQLIO to test disk performance of a SQL Server BEFORE MSSQL is installed,
or before any load is put on it.
SQLIO will simulate SQL Server by doing writes and reads to ONE specified disk.

Usage:
- Put all files on the disk you want to measure (or only the .dat file but then ou need to modify param.txt)
- Start StartSQLIO.bat
- Wait
- Wait
- Look at result.txt

==============
Microsoft recomendations
==============
On well-tuned I/O subsystems, ideal values would be:
1–5 ms for Log (ideally 1 ms on arrays with cache)
4–20 ms for Data on OLTP systems (ideally 10 ms or less)
30 ms or less on DSS (decision support system) type. Latencies here can vary significantly depending on the number of simultaneous queries being issued against the system. Sustained values of more than this when the total throughput is less than expected should be investigated.
see: http://technet.microsoft.com/en-us/library/cc966412.aspx


==============
Benchmark
==============
The result is presented as (sample from 5-ear old physical server at Teleopti):

D:\SQLIO>sqlio -kW -t2 -s30 -o1 -frandom -b8 -BH -LS -Fparam.txt 
sqlio v1.5.SG
using system counter for latency timings, 2922685 counts per second
parameter file used: param.txt
	file testfile.dat with 4 threads (0-3) using mask 0x0 (0)
4 threads writing for 30 secs to file testfile.dat
	using 8KB random IOs
	enabling multiple I/Os per thread with 1 outstanding
	buffering set to use hardware disk cache (but not file cache)
using specified size: 2000 MB for file: testfile.dat
initialization done
CUMULATIVE DATA:
throughput metrics:
IOs/sec:   421.88
MBs/sec:     3.29
latency metrics:
Min_Latency(ms): 0
Avg_Latency(ms): 9
Max_Latency(ms): 1586
histogram:
ms: 0  1  2  3  4  5  6  7  8  9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24+
%: 78  2  2  2  2  2  2  1  1  1  1  0  0  0  0  0  0  0  0  0  0  0  0  0  4

===============
interpret
===============
Look at raw throuput MB/s, and perhaps more importantly average latency. Sometimes latency is very spread out and ou may look at the histogram for that.
It will test writes and reads in multiple ways. We tpically want to see average latency consistently under 20ms for both reads and writes.