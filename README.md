# SSDStressTest

Solid State Drive (SSD) Stress Testing Tool. Continuously write to the drive while monitoring one or more SMART values.

## Usage
_WARNING! Using this tool will degrade the lifespan of your drive. Also this tool may heat up the drive until it fails. Use at your own risk and only if you know what you are doing._

    -d, --drive=VALUE          Drive letter to test (REQUIRED)
    -s, --smart[=VALUE]        SMART value to log (REQUIRED)
                                 Without value, available values are listed
    -4, --four                 Interpret SMART values as 4 bytes
                                 (otherwise 2, default)
    -t, --timeout=VALUE        Timeout between measurements (in ms)
                                 Must be >= 500 (default 2000)
    -l, --limit=VALUE          Time in minutes to run the test
                                 Zero means indefinite (default 1)
    -b, --blocksize=VALUE      Blocksize in KByte (default 16 MByte)
    -k, --testsize=VALUE       Testsize in KByte (default 512 MByte)
    -o, --output=VALUE         Output CSV file name
    -h, --help                 Show help

    
### Example
Starting the tool using:

    SSDStressTest.exe --drive=c --smart=PowerOnHoursPOH --smart=PowerCycleCount
    
Will test drive/partition C: and monitor the SMART values "PowerOnHoursPOH" and "PowerCycleCount" (just for an example, monitoring these values is pointless.). An output file will then be generated:

    'Log file name: Corsair_Performance_Pro_0.csv
    'Logging started on 29.07.2015 11:40:36
    'Product name: Corsair Performance Pro
    'Disk PNP ID: IDE\DISKCORSAIR_PERFORMANCE_PRO_________________1.0_____\5&61C381C&0&0.0.0
    'Running test for 1 minute(s)
    Time,MBytesWritten,Performance,InstPerformance,PowerOnHoursPOH,PowerCycleCount
    2.0041147,336,190.39,190.39,4020,936
    4.0162298,656,173.83,159.28,4020,936
    6.0283449,992,171.15,166.17,4020,936
    [lines omitted]
    
Use the tool of your choice to plot this data.

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
