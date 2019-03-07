# Overview

JumpDir is a (currently) Windows utility for jumping to directories used frequently using directory name fragments. It remembers where you've spent time and how you got there so you can go back with a single command instead of lots of `cd` commands to traverse your directory tree.

This project was inspired by these projects:
- https://github.com/wting/autojump (https://olivierlacan.com/posts/cd-is-wasting-your-time/)
- https://github.com/rupa/z

# Implementation

The utility is implemented as a set of hook scripts that call the DotNet Core program.
The hook scripts are necessary to bring the change directory commands back to the shell.
The hook scripts live in a directory that needs to be in your path.

# "Installing"
Installing is rather crude at the moment. You'll need to build it locally and add the hook scripts directory to your path. Try these steps:
1. Clone the repo to you Windows machine
2. Run `build.cmd` to perform the DotNet Core build
3. Run `installToPath.cmd` script. It will offer a choice of what path to update (session, user, system; system doesn't work yet).

# Usage

    jd <search term>

JumpDir will look at the current list of sub directories and find the first that contains the provided term.
If only one matches, you'll change directory into it and you're done.

For example, while sitting in Windows C drive root (`C:\`):

    C:\>jd x8
    
    C:\Program Files (x86)>jl

If more than one directory matches, you'll change directory to the first one, and JumpDir will return a list of all the possible locations, and indicate where it landed you and numbers for all the entries.

For example, while sitting in Windows C drive root (`C:\`):

    C:\>jd prog
    ==>  1  C:\Program Files  <==
         2  C:\Program Files (x86)
         3  C:\ProgramData
    ...
    C:\Program Files>

You then have 2 options:
1. Just repeat the command (`UP-ARROW`, `ENTER`). JumpDir will take you to the next in the list and again display the list (updated to indicate the new location).

       C:\Program Files>jd prog
            1  C:\Program Files
       ==>  2  C:\Program Files (x86)  <==
            3  C:\ProgramData
       ...
       C:\Program Files (x86)>

2. Use `jd <num>` with the number of the desired entry

       C:\Program Files>jd 3
            1  C:\Program Files
            2  C:\Program Files (x86)
       ==>  3  C:\ProgramData  <==
       ...
       C:\ProgramData>

## [Arbitrary jumping](https://youtu.be/XhzpxjuwZy0?t=83)
A key feature of JumpDir is that it remembers where you've been.
Matched paths are saved with the term you entered to find it.
You can then use the command with the pervious term to jump to that location without having to traverse the directory tree or use pushd/popd.
This works from directories unrelated to the one you are currently in.

For example, starting from the Windows drive root (`C:\`):

    C:\>jd prog
    ==>  1  C:\Program Files  <==
         2  C:\Program Files (x86)
         3  C:\ProgramData
    ...
    C:\Program Files>cd \Users
    
    C:\Users>jd prog
    ==>  1  C:\Program Files  <==
         2  C:\Program Files (x86)
    ...
    C:\Program Files>

***Important note:** Notice above that only the first `jd` call includes `C:\ProgramData` while they both includes both `Program Files` locations.
This is because JumpDir remembers that I've visited those and they match my jump term `prog`. The third entry in the first call is a directory that is actually found under `C:\` so it's included in the list as well.*

## Location Ranking
As you visit (and stay in) locations, they get saved and ranked.
You can see the rankings with the `jd -l` command. 
(There's also a helper hook script command for this: `jl`.)
For example, after a few visits, I get these stats:

    C:\>jl
     jumpDir usage statistics
    
      #  Rank  Path [key(s)]
     ========================================
      1   6.0  C:\Program Files (x86)  [prog|x8]
      2   4.0  C:\Program Files  [prog]
      3   2.0  D:\Development  [dev]
      4   1.0  D:\Development\jumpDir  [jump]

The more frequently you visit a location, the higher it is ranked.
When you attempt to jump using a term matching several locations, you land on the higher ranked ones first.
So even though `C:\Program Files (x86)` comes after `C:\Program Files` under normal circumstances, JumpDir knows I've visited it more often so it's the preferred one.

    C:\>jd prog
    ==>  1  C:\Program Files (x86)  <==
         2  C:\Program Files
         3  C:\ProgramData

But remember, you can always just repeat the same jump command (within the repeat time limit) to go to the next one in the list:

    C:\Program Files (x86)>jd prog
         1  C:\Program Files (x86)
    ==>  2  C:\Program Files  <==

## "Flyovers" 
When you are traversing directories with JumpDir, if you jump again within a predefined length of time, the last jump will be ignored.
This is to prevent unnecessary clutter in the stats list as well as avoiding conflicts between the saved keys for directories you never really spend time in and those that you do.
Locations that are persisted just won't increase in rank until you jump to and stay there a while.

# Contributions
Find a bug? Let me know in [Issues](https://github.com/peterlanoie/jumpDir/issues). Or fork the repo and try to fix it.

Want to make it better? Fork the repo and submit a pull request with your improvement.