# SimpleAirlock
Simple Airlock Script for Space Engineers
Thomas LaFreniere aka laftho
v1.0 - May 29, 2016

This script matches closest pairs of doors on the 
current grid and ensures one is closed before the 
other opens.

Add this script to a Programmable Block and set a
Timer Block loop of trigger now.

Simply add [airlock] to the name of each of the 
doors you wish to have this behavior. Script 
automatically will find the matching pair and
manage their states.

If you wish to use a different key for your doors
just call the Programmable Block with the script
in your timer with your key value as the argument.
Default key is [airlock]
