# SSDStressTest

Solid State Drive (SSD) Stress Testing Tool. Continuously write to the drive while monitoring one or more SMART values.

![Example](http://www.hardwareluxx.de/images/stories/galleries/reviews/samsung-sm951/temperature/en_sm951-no-cooling-500.png)

## Usage
_WARNING! Using this tool will degrade the lifespan of your drive. Also this tool may heat up the drive until it fails. Use at your own risk and only if you know what you are doing._

    -d, --drive=VALUE          Drive letter to test (always required)
    -s, --smart[=VALUE]        SMART value to log / list available values
    -x, --xml=VALUE            Use HDSentinel.xml, specify drive Id to use
    -4, --four                 Interpret SMART values as 4 (not 2) bytes
    -i, --interval=VALUE       Timeout between measurements (in ms)
                                Must be >= 500 (default 2000)
    -l, --limit=VALUE          Time in minutes to run the test
                                (0 = run until stopped, default 1)
    -b, --blocksize=VALUE      Blocksize in K/M/G Byte (default 16 MByte)
    -t, --testsize=VALUE       Testsize in K/M/G Byte (default 512 MByte)
    -o, --output=VALUE         Output CSV file name
    -h, --help                 Show help

    
### Example
Starting the tool using:

    SSDStressTest.exe --drive=C --smart=PowerOnHoursPOH --blocksize=8m --interval=10000
    
Will test drive/partition C: and monitor the SMART values "PowerOnHoursPOH" and "PowerCycleCount" (just for an example, monitoring these values is pointless.). An output file will then be generated:

    'Log file name: Corsair_Performance_Pro_0.csv
    'Logging started on 04.08.2015 17:23:16
    'Product name: Corsair Performance Pro
    'Disk PNP ID: IDE\DISKCORSAIR_PERFORMANCE_PRO_________________1.0_____\5&61C381C&0&0.0.0
    'Blocksize: 8 MByte
    'Testsize: 512 MByte
    'Free disk space: 86.646 GByte
    'Total disk space: 238.374 GByte
    'Running test for 1 minute(s)
    Time,MBytesWritten,Performance,InstPerformance,PowerOnHoursPOH
    10.0915772,1712,173.89,173.89,4032
    20.0781484,3456,174.03,174.18,4032
    30.1017217,4864,162.82,140.58,4032
    40.1002936,6712,168.28,184.59,4032
    50.0968654,8320,166.84,161.08,4032
    60.1584409,9168,153.82,87.11,4032
    
Use the tool of your choice to plot this data.

### Download

Binaries can be found here: https://github.com/m-ober/SSDStressTest/releases

## License
This project is partially based on the work from other people.
### SmartTools.cs (modified)
(C) Microsoft Corporation, Author: Clemens Vasters (clemensv@microsoft.com)

Code subject to MS-PL: http://opensource.org/licenses/ms-pl.html 

### Options.cs

Copyright (C) 2008 Novell (http://www.novell.com), Copyright (C) 2009 Federico Di Gregorio, Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)

### Icon 
License: Creative Commons (Attribution 3.0 Unported)

https://www.iconfinder.com/icons/289617/fire_flame_match_icon#size=128
