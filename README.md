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
A command-line application that offers you the functionality of `PwnedApiWrapper` in batch mode (checking multiple passwords at once from a newline-delimited file). Use it like so:

```
PwnedApiWrapper.App.exe my_password_list.txt plain > output.txt
```

Specifying a file containing a newline-delimited list of passwords and either `plain` (most common) or `coq` which will generate a lookup structure containing results for use from the [Coq proof assistant](https://coq.inria.fr). Output will be printed to the console, but can obviously be redirected to a file using `>` (as in the example).

*Use this utility [considerately](#responsible-use)!*

### PwnedApiWrapper.FrequencyExtractor
A command-line application that will just extract frequencies from the local data file for you (after you torrent it of course) without any regard for hashes etc. Useful for building statistical models of password frequencies when we don't necessarily care about the passwords themselves. Use it like so:

```
PwnedApiWrapper.FrequencyExtractor.exe 1000 pwned_passwords.txt
```

Patience is a virtue here. That text file is over 30GB at the time of writing and it'll take a while to go through. Code has been designed not to wreck your RAM too hard by trying to load the whole thing into memory. The example above extracts all frequencies above 1000 from the corpus and returns them in a newline-delimited file, sorted descending.

## Building
The utility is written in C#, just build it like any other Visual Studio project.

## Responsible Use
Please don't change and then use this utility in batch/API mode to inconsiderately hammer the actual API with thousands and thousands of requests (it won't let you do this by default). Troy uses some clever architecture to give the API very high availability and fast performance with no rate limiting; please don't abuse it. If possible, torrent the database file to your machine, use this utility in batch/local mode, and go crazy with as many requests as you like.

## Acknowledgements
[Troy Hunt](https://www.troyhunt.com), creator of [Have I Been Pwned?](https://haveibeenpwned.com) maintains [Pwned Passwords](https://haveibeenpwned.com/Passwords) and its API for anyone to use free of charge. Consider [donating](https://haveibeenpwned.com/Donate) to the project to keep it just as awesome into the future. 
