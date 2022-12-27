# EncryptedJournal
This is a simple encrypted journal, run from the terminal.
It works with a password and stores all journal entries in human readable files, encrypted.
The size of a journal file is currently 20 KBs, but it is adjustable in the code.

To start you simply need to fill out JournalBuilderConfig.txt, then run JournalBuilder.ps1.
To use you need to invoke it from a terminal (Command Prompt, PowerShell, etc.) with options.
There is a built in help option (EncryptedJournal.exe -h), you can use options with - or / or as plain text in a simple string
          (EncryptedJournal.exe -o /l wk)
