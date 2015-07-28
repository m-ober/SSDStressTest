# SSDStressTest

Solid State Drive (SSD) Stress Testing Tool. Continuously write to the drive while monitoring SMART.

## Usage
_WARNING! Using this tool will degrade the lifespan of your drive. Also this tool may heat up the drive until it fails. Use at your own risk and only if you know what you are doing._

    Help:
      -d, --drive=VALUE          Drive letter to test (required)
      -s, --smart[=VALUE]        SMART value to log
                                   without value, available values are listed
      -4, --four                 Interpret SMART values as 4 bytes
                                   (otherwise 2, default)
      -t, --timeout=VALUE        Timeout between measurements (in ms)
                                   must be >= 500 (default 2000)
      -l, --limit=VALUE          Time in minutes to run the test
                                   Zero means indefinite (default 0)
      -o, --output=VALUE         Output CSV file name (default results.csv)
      -h, --help                 Show help


## License
This project is partially based on the work from other people.
### SmartTools.cs
(c) Microsoft Corporation, Author: Clemens Vasters (clemensv@microsoft.com)

Code subject to MS-PL: http://opensource.org/licenses/ms-pl.html 

### Icon 
License: Creative Commons (Attribution 3.0 Unported)

https://www.iconfinder.com/icons/289617/fire_flame_match_icon#size=128
