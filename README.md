# zFRAG
zFRAG is a little Zen Game in which you can manually defrag a virtual Hard Disk. You can also turn on the AutoDefrag and sit back and watch.

Developed in Unity, the game is [available on itch.io](https://losttraindude.itch.io/zfrag)

## Things to know

I strongly suggest you to download the project as a whole and run it in Unity yourself.
Although I did my best to make the logic self-explanatory, there are things that are only understandable by looking at how they are implemented in the Editor.

Examples being: initial values for some variables, Button Events and Graphics, AudioClips.

### Resolution
The game's reference resolution is 640x400. I made use of the [2D Pixel Perfect package](https://blogs.unity3d.com/2019/03/13/2d-pixel-perfect-how-to-set-up-your-unity-project-for-retro-8-bits-games/).

### Font
I only used `TextMeshPro` components.
The Atlas for the specific font I used (PxPlus_IBM_VGA9) has been set up in such a way to include all of the "special" characters (i.e. Block Elements to represent sectors) I needed.

### Audio
To handle `AudioClip`s volume I use Audio Mixers.
If you want to know more about Audio Mixers I suggest you [to give a look at these tips](https://johnleonardfrench.com/articles/10-unity-audio-tips-that-you-wont-find-in-the-tutorials/).

### Button Graphics
As you can tell, I tried my best to emulate a DOS-like interface. Each Button in the game has a `MoveTextOnClick` script that shifts the Button's Text component to the right upon click, so to follow the `Pressed Sprite` and the default one.

## Game States
The main game logic makes use of a Finite State Machine with 6 states: START, PAUSE, DEFAULT, AUTODEFRAG, FREEPAINTING, COMPLETE

### START
![START state](https://i.imgur.com/KSto7CZ.png)

Only available upon starting the game. This switches to DEFAULT once players close the `Start Menu`.

Time doesn't advance.

### PAUSE
![PAUSE state](https://i.imgur.com/nWNOE8K.gif)


Players either hit **ESC** to display the `Exit Menu` or are in the `Options Menu`

Time doesn't advance.

### DEFAULT
![DEFAULT state](https://i.imgur.com/Zl80t2L.gif)

The default state in which players can operate, after closing the `Start Menu` and when `AutoDefrag` and `Free Painting` modes are switched off.

Here players can drag and drop sectors in order to form a continuous line from the first cluster of the grid.
After placing a sector, the game will update the grid and the `Progress Bar` to check if there are new sectors to be considered defragmented.
If there are, the game "paints them" yellow and makes them unmovable.

Time advances.

### AUTODEFRAG
![AUTODEFRAG state](https://i.imgur.com/Zj92tQf.gif)

The grid is updated in the same way as in the DEFAULT state, but here the game does so automatically at a given speed (modifiable in the `Options Menu`).

Here players can't interact with the grid.

Time advances.

### FREEPAINTING
![FREEPAINTING state](https://i.imgur.com/IQBYMN7.gif)

Here players can freely drag and drop sectors wherever they want without having the game preventing them to do so, as this time it won't check for defragmented sectors.

### COMPLETE
![COMPLETE state](https://i.imgur.com/BxMJpnB.gif)

Defragmented sectors are all in a row. Players can't move sectors again unless they `Reset` or switch to FREEPAINTING.

Time doesn't advance.

## Credits

### Hard Disk Sound Effects
The hard disk sound effects have been edited from a small recording I took of the wonderful audio of the [TwitchDefrags](https://www.twitch.tv/twitchdefrags) stream.

### The "clack" sound
The sound you hear when dragging and dropping sectors on the grid was made using [Bfxr](https://www.bfxr.net/) by [Stephen Lavelle](https://www.increpare.com/).

### Font
The font - PxPlus_IBM_VGA9 - is part of the [Ultimate Oldschool PC Font Pack](https://int10h.org/oldschool-pc-fonts/) © 2016 VileR, and is licensed under a [Creative Commons Attribution-ShareAlike 4.0 International License](https://creativecommons.org/licenses/by-sa/4.0/).

### UI Drag and Drop
I modified, according to my own purposes, the code provided by [this tutorial by Omnirift](https://www.youtube.com/watch?v=fhBJWTO09Lw).

### Special Thanks
Thanks **a lot** to the [Multimedia HyperGuide community](http://vga256.com/podcast/) who have been the first people to know of this little project of mine and provided feedback, support and suggestions along the way.
