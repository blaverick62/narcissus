# Narcissus RAT

### Proof of concept for a remote access trojan that utilizes Twitter direct messages for command, control, and exfiltration.

## Requirements:
 - Narcissus requires one Twitter developer account. Any messages sent to this account while Narcissus is running will be interpreted as commands.
 - Once you have created a developer account, create a new user environment variable and set these values, separated by semicolons:
   - Consumer Key
   - Consumer Key Secret
   - Access Token
   - Access Token Secret
 - For delivery to a remote system, hardcode these values as strings to the appropriate values. *AT YOUR OWN RISK*
   - This will expose your API keys and secrets, but is the only way to interact with the API with no interaction to other delivery domains.
 - Target must be a Windows host with a compatible version of .NET
 
## Delivery Methods:
 - Compile Narcissus as a standalone executable and execute on target
 - Deliver as string to host and execute in powershell using Add-Type
 
## Commands:
 - Any simple PowerShell command can be executed in Narcissus through it's custom pipeline. Results are limited to Twitter message limits.
 - Kill: Stops Narcissus agent on target
 
## Upcoming Features:
 - Screenshots
 - Keylogger
 - System info collection
 - Large script delivery
 - Troll/Proof of compromise
