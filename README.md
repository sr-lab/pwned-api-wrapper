# Pwned Passwords API Wrapper
Wraps the Pwned Passwords API (or, preferably, your local Pwned Passwords database) offering batch queries etc.

## Overview
This repository consists of several projects.

### PwnedApiWrapper
This is the class library that can be used to query the API. Usage is very simple, add it as a reference to your project and use it like so:

```csharp
using PwnedApiWrapper;

// ...

var client = new PwnedPasswordsClient("https://api.pwnedpasswords.com/");
var occurrences = client.getNumberOfAppearances("hunter2");
```

### PwnedApiWrapper.Shared
Just contains shared code used in each project in the solution (some file I/O and hashing stuff). Don't even worry about it, but make sure `PwnedApiWrapper.Shared.dll` stays alongside the compiled versions of other projects or they won't work.

### PwnedApiWrapper.App
A command-line application that offers you the functionality of `PwnedApiWrapper` in from the command line. To get usage information from the utility itself, use the `-h` option like so:

```
PwnedApiWrapper.App.exe -h
```

This utility can be run in three different primary modes:

* `-i`: Interactive mode. Opens up a shell-like interface around a data source allowing you to grab frequencies for one password at a time.
* `-b <passwords_file>`: Batch mode. Allows you to pass in a newline-delimited list of passwords in a file and grab frequencies for them all.
* `-c <pwned_passwords_db_file>`: Frequency-only mode. Allows you to perform actions involving frequencies only (no passwords).

#### Interactive Mode
After specifying interactive mode, pass one of the following options to specify a data source:

* `-a`: Uses the official API. Use this option with consideration please.
* `-f <pwned_passwords_db_file>`: Uses a local Pwned Password file. Plain text only, must not be archived.

Example: Start interactive mode against the API:

```
ppexp -i -a
```

Example: Start interactive mode against a local copy of Pwned Passwords:

```
ppexp -i -f pwned-passwords-2.0.txt
```

#### Batch Mode
After specifying batch mode, pass one of the following options to specify a data source:

* -a: Uses the official API. Use this option with consideration please.
* -f <pwned_passwords_db_file>: Uses a local Pwned Password file. Plain text only, must not be archived.

Example: Run `passwords.txt` through the API:

```
ppexp -b passwords.txt -a
```

Example: Run `passwords.txt` through a local copy of Pwned Passwords:

```
ppexp -b passwords.txt -f pwned-passwords-2.0.txt
```

#### Frequency-Only Mode
After specifying frequency-only mode, pass one of the following options to specify a query mode:

* -l <limit>: Gets all frequencies above a threshold.
* -t <count>: Gets a number of the highest frequencies. Note that this assumes the data file is sorted by frequency.

Example: Get all frequencies above 1000000:

```
ppexp -c pwned-passwords-2.0.txt -l 1000000
```

Example: Get top 100 frequencies:

```
ppexp -c pwned-passwords-2.0.txt -t 100
```

Specifying a file containing a newline-delimited list of passwords and either `plain` (most common) or `coq` which will generate a lookup structure containing results for use from the [Coq proof assistant](https://coq.inria.fr). Output will be printed to the console, but can obviously be redirected to a file using `>` (as in the example).

*Please use this utility [considerately](#responsible-use)!*

## Building
The utility is written in C#, just build it like any other Visual Studio project.

## Responsible Use
Please don't change and then use this utility in batch/API mode to inconsiderately hammer the actual API with thousands and thousands of requests (it won't let you do this by default). Troy uses some clever architecture to give the API very high availability and fast performance with no rate limiting; please don't abuse it. If possible, torrent the database file to your machine, use this utility in batch/local mode, and go crazy with as many requests as you like.

## Acknowledgements
[Troy Hunt](https://www.troyhunt.com), creator of [Have I Been Pwned?](https://haveibeenpwned.com) maintains [Pwned Passwords](https://haveibeenpwned.com/Passwords) and its API for anyone to use free of charge. Consider [donating](https://haveibeenpwned.com/Donate) to the project to keep it just as awesome into the future. 
