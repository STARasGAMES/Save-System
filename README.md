# Save-System
Save system for Unity game engine. Inspired by Component Save System: https://github.com/AlexMeesters/Component-Save-System

## Why one more save system?
I've looked at existing solutions from GitHub and Asset Store. But nothing looks solid and convinient to use. Most of them provide the same functionality: ability to save/load values based on keys. I call them PlayerPrefs-like systems, cuz they literally do the same thing but write data to file. On top of that they can provide different serialization methods such as JSON and Binary. Only a few provide data encryption to prevent players from cheating. And that's it.

Let's talk about this features in details.

### PlayerPrefs is enough?
Yes. But only if you are creating quite simple game and your game state fits several lines of code. For example, hyper-casual game, where you store only level number, coins and maybe some achievements data. Another example could be liniar game, where again, you need to store level number and player's inventory. 

Yeah, in this case all you really need is:
```c#
public void SaveGame()
{
  SaveSystemApi.SetInt("level_number", LevelManager.CurrentLevelNumber);
  // something else ...
}

public void LoadGame()
{
  LevelManager.CurrentLevelNumber = SaveSystemApi.GetInt("level_number");
  // something else ...
}
```

### Is JSON so bad?
On every tutorial online about saving in Unity you will find information about JSON and Binary serialization. In most cases they have such message: Binary serialization is the same as JSON but cheaper, faster, with lower memory usage. While all of that is true, in real world you will never suffer from JSON being slow. Why I think so? Because I think you ain't gonna create a game that needs more than 1 Mb for save file. In 1 Mb you can store huge amount of data. How much it costs to serialize/deserialize 1 Mb file using JSON? About 100 ms. Maybe a bit slower on old HDDs. I'm not saying JSON is fast as hell, but it's not critically slow. At least at medium-size games.

But what we are getting insted when using JSON?
1. Human-readable saves. This is must-have at develepment stage.
2. Backward compatibility with older versions of saves.
3. Ability to serialize complex data structures, not only primitives like strings and numbers.
4. Ability to serialize and (most importantly) deserialize abstract objects. This is a very handy feature when you creating somehow complex game. 

Important note: to have points 3 and 4 you need to use complex Json serializers like Newtonsoft.Json.

### Encryption?
Yes, cool feature to have, but it can't guarantee that players will not cheat. If your goal is to have 100% secure game, you need to use some kind of server where you will validate each action of the player. In other words, repeat all client's code on server side. Cloud saving also not 100% secure. That's it.

Anyway, I will add encryption in nearest future. 


## Save System structure
0. Guid References - an individual package
1. GameState management
2. Storages
3. Runtime instances management

System structured the way allowing you to decide what modules to use. For example, you may endup using only GameState management, and implementing storing by yourself. 

### Guid References
Provides a way to reference game objects by GUIDs (Global Unique IDentifiers). 
In the Save System it plays important role because it is responsible for creating and managing unique ids (GUIDs) for game objects that need to be saved.

GUID is a persistent ID which will not change if you rename game object, change it's parent or even move it to another scene - GUID remains the same.

Also, Guid References provides cross-scene references. Your project can highly benefit from using this package. More about Guid References: https://github.com/STARasGAMES/Guid-References

### GameState Management
The core peace of the system. 

This module is responsible for gathering game-state from objects and loading game-state back.

### Storages
Provides extensible interface to create your own storages. This is the PlayerPrefs-like part :)
Contains pre-made FileSystemStorage which provides a way to store savedata in files.

### Runtime Instances Management
This is an optional module. It provides a way to save game objects that were created at runtime. 

The way it works: everytime you need to Instantiate saveable gameobject you need to use `IRuntimeInstancesManager.Instantiate(string assetId, AssetSource source, Scene scene);`.
What it does is just:
1. Creates new instance based on assetId(it may be just path to prefab from Resources folder)
2. Generates new GUID for this instance and assotiates assetId with it. 
3. Instance has GUID which means it will be saved as any other gameobject.
4. When game is loading RuntimeInstancesManager will recreate instances using stored GUIDs and assetIds.

It's quite simple module but you can use it as an example on how to create similar systems **specifically for your game**.
