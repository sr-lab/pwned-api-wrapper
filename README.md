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
PwnedApiWrapper.FrequencyExtractor.exe 1000 pwned-passwords-2.0.txt
```

Patience is a virtue here. That text file is over 30GB at the time of writing and it'll take a while to go through. Code has been designed not to wreck your RAM too hard by trying to load the whole thing into memory. The example above extracts all frequencies above 1000 from the corpus and returns them in a newline-delimited file, sorted descending.

### PwnedApiWrapper.Local
A command-line application that works in the same way as `PwnedApiWrapper.App` but queries a a local version of Pwned Passwords instead. It won't work with the Pwned Passwords archive (a `*.7z` file) so you'll need to extract it first. Use it like so once you have the text file extracted:

```
PwnedApiWrapper.Local.exe passwords.txt pwned-passwords-2.0.txt plain
```

Where `passwords.txt` is the newline-delimited file containing passwords to grab frequencies for. Once again, this might take a while. The utility also supports "interactive mode", which brings up a prompt to get password frequencies one at a time:

```
PwnedApiWrapper.Local.exe -i pwned-passwords-2.0.txt
```

This will bring up a prompt for a password to search for, like so:

```
query> 
```

Enter the password you want to grab the frequency for, hit enter and be patient (we've got over 30GB of file to trawl).

## Building
The utility is written in C#, just build it like any other Visual Studio project.

## Responsible Use
Please don't change and then use this utility in batch/API mode to inconsiderately hammer the actual API with thousands and thousands of requests (it won't let you do this by default). Troy uses some clever architecture to give the API very high availability and fast performance with no rate limiting; please don't abuse it. If possible, torrent the database file to your machine, use this utility in batch/local mode, and go crazy with as many requests as you like.

## Acknowledgements
[Troy Hunt](https://www.troyhunt.com), creator of [Have I Been Pwned?](https://haveibeenpwned.com) maintains [Pwned Passwords](https://haveibeenpwned.com/Passwords) and its API for anyone to use free of charge. Consider [donating](https://haveibeenpwned.com/Donate) to the project to keep it just as awesome into the future. 
