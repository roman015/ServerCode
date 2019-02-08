#!/bin/bash

# Script to download source code from GitHub 
# and build and deploy to local server into 
# the prod folder

# Default Argument Values
appName="ProxyLayer"
appArguments="--port=8080"
gitRepository="git@github.com:roman015/ServerCode.git"

# Get Values from ARG
if [ -z $1 ]; then echo "No appName provided, using Default"; else appName=$1; fi
if [ -z $2 ]; then echo "No appArguments provided, using Default"; else appArguments=$2; fi
if [ -z $3 ]; then echo "No gitRepository provided, using Default"; else gitRepository=$3; fi

# Display values used
echo "Starting $appName using repository $gitRepository with arguments \"$appArguments\""

# Check If An Instance Is Already Running
isAppRunning=$((screen -S $appName -Q select . ; echo $? ) \
		| head -c 1)

# Terminate Existing Instance First
if [ $isAppRunning == 0 ]
then
	echo "$appName is Running, Terminating Instance before Creating a new one"
	screen -X -S $appName quit
else
	echo "$appName Instance Not Found, Creating New Instance Directly"
fi

# Download source from GitHub
cd ~/temp
git clone $gitRepository Source

# Build From Source
cd Source
cd $appName
buildOutput=$(dotnet build -c Release -v m -o ./bin/$appName)

# Proceed Only If Build Was Successful
if echo "$buildOutput" | grep "Build succeeded."
then
	# Copy Build to Prod
	rm -rf ~/prod/$appName

	echo "Copying build to  ~/prod/$appName ..."
	mkdir ~/prod/$appName
	cp -r ./bin/$appName/* ~/prod/$appName
else
	echo "------------"
	echo "BUILD FAILED"
	echo "------------"
	echo "$buildOutput"
	exit 1
fi

# Start The New Instance
cd ~/prod/$appName
screen -mdS $appName
sleep 2
screen -S $appName -X stuff "dotnet $appName.dll $appArguments \n"

# Cleanup
echo "Reload Successful, cleaning up..."
rm -rf ~/temp/Source

