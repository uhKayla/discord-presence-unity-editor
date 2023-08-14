# Unity Editor Discord Rich Presence

Discord's Rich Presence for Unity Editor (editor only!)

Refactored and cleaned up a bit. I am still new to C# but I have the basics down. 
I moved the update loop to its own async with a 5s delay on updates, I've also fixed the bug where it would log an exception every frame if Discord was not open.

Created a package.json file for ease of use.