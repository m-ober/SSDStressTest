# SSDStressTest

Solid State Drive (SSD) Stress Testing Tool. Continuously write to the drive while monitoring one or more SMART values. The tool Hard Disk Sentinel is also supported for monitoring where SMART values are not available via WMI.

![Example](http://www.hardwareluxx.de/images/stories/galleries/reviews/samsung-sm951/temperature/en_sm951-no-cooling-500.png)

## Usage
_WARNING! Using this tool will degrade the lifespan of your drive. Also this tool may heat up the drive until it fails. Use at your own risk and only if you know what you are doing._

    -d, --drive=VALUE          Drive letter to test (always required)
    -s, --smart[=VALUE]        SMART value to log / list available values
    -x, --xml=VALUE            Use HDSentinel.xml, specify drive Id to use
    -w, --disablewmi           Do not query WMI    
    -4, --four                 Interpret SMART values as 4 (not 2) bytes
    -i, --interval=VALUE       Timeout between measurements (in ms)
                                Must be >= 500 (default 2000)
    -l, --limit=VALUE          Time in minutes to run the test
                                (0 = run until stopped, default 1)
    -b, --blocksize=VALUE      Blocksize in K/M/G Byte (default 16 MByte)
    -t, --testsize=VALUE       Testsize in K/M/G Byte (default 512 MByte)
    -o, --output=VALUE         Output CSV file name
    -h, --help                 Show help


### Examples
#### Monitor the drive using SMART:

    SSDStressTest.exe --drive=C --smart=Temperature --blocksize=8m --interval=10000

This command will test drive/partition C: and monitor the SMART value "Temperature". The write requests will have a size of 8M and performance and SMART values are measured every 10 seconds.

#### Monitor the drive using Hard Disk Sentinel:

    SSDStressTest.exe --drive=C --xml=3

Test drive/partition C: and monitor the Temperature of the (third) drive as reported by Hard Disk Sentinel. The file HDSentinel.xml needs to be placed in the same directory as the SSDStressTest executable.

*Note*: If `--smart` is given, the `--xml` switch is ignored, i.e. they cannot be used at the same time.

*Note*: If you get unexplainable errors regarding WMI, try the `--disablewmi` switch. Note that the `--smart` option will no longer work without WMI. You can still use the `--xml` option or use the tool without monitoring the drive SMART values.

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
Icon made by [Vectors Market](https://www.flaticon.com/authors/vectors-market) from [www.flaticon.com](https://www.flaticon.com/). 
