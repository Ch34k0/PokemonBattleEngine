# PokemonBattleEngine

A C# library that can emulate Pok�mon battles.

The library comes with a client and server, so it is possible to host a battle server and have clients matchmake into it and battle each other.
The engine only emulates as if it were Pok�mon B2W2 versions, so there will not be features introduced after generation 5.

Also included is "anti-cheating" code that protects information a player should not know.
Information slowly gets revealed to each player over time.
For example, your opponent will not know your ability until your ability does something. He/she will also be unaware of your moves until you use them.
Therefore, a modified client cannot do anything more than an ordinary player (unless my code has exploits... in that case... create an issue!)

There is a class with the engine constants that you can change for custom battles. Ever wanted to have level 200 Pok�mon? Or 8 moves? You can do that!
Check [Constants.cs](PokemonBattleEngine/Data/Constants.cs)

----
# To Do:
* Switching out and multiple Pok�mon per team
* Pok�mon nicknames
* Accuracy/Evasion math
* Critical hits
* Friendship
* Struggle
* Weather
* Turn order
* Move targetting
* Double/triple/rotation battles
* Add most moves, items, Pok�mon, secondary statuses (underwater, cursed, mud sport, etc.)
* Add timeouts for waiting for a client. A modified client can remove response packets to troll and the server will currently wait infinitely
* Client UI
* Spectators

----
# PokemonBattleEngine Uses:
* [My fork of Ether.Network](https://github.com/Kermalis/Ether.Network)