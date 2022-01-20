<div align="center">

[![Build](https://gist.github.com/gpetrounrt/12e53399727fc04da47e22494e6e2681/raw/BuildLogBadge.svg)](https://circleci.com/api/v1.1/project/github/gpetrounrt/buildlogreporter/latest/artifacts/0/Users/circleci/project/artifacts/Report/BuildLogReport.htm?branch=main)
[![Coverage](https://gist.github.com/gpetrounrt/12e53399727fc04da47e22494e6e2681/raw/CoverageBadge.svg)](https://circleci.com/api/v1.1/project/github/gpetrounrt/buildlogreporter/latest/artifacts/0/Users/circleci/project/artifacts/Coverage/Report/index.htm?branch=main)
[![CircleCI](https://img.shields.io/circleci/build/gh/gpetrounrt/BuildLogReporter/main?label=circleci&logo=circleci&style=plastic&token=8b571c9d36c58f851da996c00b86a356312ab969)](https://circleci.com/gh/gpetrounrt/BuildLogReporter/tree/main)

[![NuGet](https://img.shields.io/nuget/v/build-log-reporter?logo=nuget&style=plastic)](https://www.nuget.org/packages/build-log-reporter)
[![NuGet Prerelease](https://img.shields.io/nuget/vpre/build-log-reporter?logo=nuget&style=plastic)](https://www.nuget.org/packages/build-log-reporter)

</div>

# Background

I wanted to learn how to create a .NET tool. At the same time, I was looking for an easy way to extract errors and warnings from a .NET build log. Eventually, I ended up making this project.

# Usage

At minimum, you will need two arguments:

1. The logPath, which corresponds to the path of the log file (binary or text).
2. The reportPath, which corresponds to the path of report directory. If it does not exist, the tool will attempt to create it.

```
build-log-reporter [options] <logPath> <reportPath>
```

There are a few options that you can set before the two arguments:

```
-rt, --report-types <report-types>  The type of reports to generate [default: Html]
-v, --verbose                       Whether to use verbose output [default: False]
--version                           Show version information
-?, -h, --help                      Show help and usage information
```

The report types option is a semi-colon-separated list. There are five available types: Badge, HTML, JSON, Markdown and XML. If you use `"Badge;Html;Json;Markdown;Xml"` the tool will create all possible report types.

Normally, the tool does not show any output. It should return 0 exit code if it finishes successfully. If you enable the verbose option, it will display the appropriate information as it progresses with parsing the log and creating the report files.

For examples of report files, you can check the [Baseline](tests/BuildLogReporter.IntegrationTests/Baseline) directories.