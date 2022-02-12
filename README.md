# IdleAppStarter 
![](https://raw.githubusercontent.com/DareFox/IdleAppStarter/master/githubMedia/IdleIcon.png)

## Purpose
The goal of IdleStarterApp is to run applications when the user is inactive, even when the user is on Logon screen.


## How to install
[Download latest version of IdleAppStarter](https://github.com/DareFox/IdleAppStarter/releases/latest "Download latest version of IdleAppStarter") and then unzip it. For making server-side work as Windows Service you need to download [NSSM](https://nssm.cc/ "NSSM") and unzip it somewhere too.

#### Service-side install: 

- Open cmd in NSSM folder (nssm/win64 or nssm/win32)

- Type `nssm install IdleAppStarter`

- Add "IdleLoginService.exe" as executable for service

- Add arguments for service

	  -e, --executables    Required. Programms to execute

	  -i, --idle           Required. Idle in ms

	  -p, --port           Required. Port for service communication
  
- Press "Install Service"



#### Client-side install:

- Make "IdleUserApp.exe" run at system startup

- Add arguments to it

	  -e, --executables    Required. Programms to execute

	  -i, --idle           Required. Idle in ms

	  -p, --port           Required. Port for service communication

## How it works
IdleAppStarter consits of two parts: 

`App` — User-side **client** 

`Service` — System-side **server** 

### General Logic

In short, the client watches the user's action and if it is idle, it launches the application. If there are no clients, then the Windows service launches applications on behalf of the system.

![](https://github.com/DareFox/IdleAppStarter/raw/master/githubMedia/Usage.png)

------------


### Client Logic
The Сlient starts automatically with user authorization in the system and joins the service server. The client watches the user's activity and, depending on his activity, launches or kills applications.

Connecting to the server is necessary in order to inform the service that this client is responsible for the user's behavior and we take all the work for his activity, and not the service.

![](https://github.com/DareFox/IdleAppStarter/raw/master/githubMedia/ClientLogic.png)



------------

### Service Logic

Service starts as SYSTEM process and start server, which works even if nobody are logged in. If no requests are received from the client, then the server part assumes that nobody are logged in (0 clients = 0 users) and runs applications on behalf of the system until the next request from any client.

![](https://github.com/DareFox/IdleAppStarter/raw/master/githubMedia/ServiceLogic.png)

## Why didn't you use Task Scheduler for this task
Task Scheduler doesn't work before login for some reason for this task ¯\_(ツ)_/¯

