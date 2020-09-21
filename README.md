# Save-System
Save system for Unity game engine. Inspired by Component Save System: https://github.com/AlexMeesters/Component-Save-System

## Why one more save system?
I've looked at existing solutions from GitHub and Asset Store. But nothing looks solid and convinient to use. Most of them provide the same functionality which is ability to set and get values based on keys. I call them PlayerPrefs-like, cuz they literally do the same thing but write data to file. On top of that they can provide different serialization methods such as JSON and Binary. Only a few provide data encryption to prevent players from cheating. And that's it.

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
Yes, cool feature to have, but it can't guarantee that players will not cheat. If your goal is to have 100% secure game, you eventually need to use some kind of server where you will validate each action of the player. In other words, repeat all client's code on server side. Cloud saving also not 100% secure. That's it.

Anyway, I will add encryption in nearest future. 
