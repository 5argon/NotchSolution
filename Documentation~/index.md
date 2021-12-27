> [!WARNING]
> Requires Unity 2019.4 LTS or newer. Player's device must also be ensured to be at high enough OS version to report `safeArea`/`cutouts`.

Whether you like it or not, the time has come for us designers to design in context of a notch and embrace it instead of hiding it. This tool also enables design-time preview which help you iterate your design without building the game.

![Main screenshot](images/main-screenshot.gif)

- [Components](components/overview.md) to attach to your `GameObject`, they will stay safe by staying inside [`safeArea`](https://docs.unity3d.com/ScriptReference/Screen-safeArea.html) and out of the way of any [`cutouts`](https://docs.unity3d.com/ScriptReference/Screen-cutouts.html).
- [Notch Simulator](simulator/notch-simulator.md) to iterate your design in editor with various devices available. See the components react immediately without building the game or access to real devices.

## Easy way to pay for this software

Are you looking for a way to say thanks to this open source work other than code contribution?

It is easy! You can take a look at my myriad of niche Unity Asset Store **audio plugins** in [my publisher page](https://assetstore.unity.com/publishers/18007), grab something for your game, or tell your audio-caring friends about them. Thank you!

## Getting started

### 1. Install

- Using [the Asset Store version](http://u3d.as/1FEw), install and update using the usual way. Soon [Unity Package Manager (UPM)](https://docs.unity3d.com/Manual/upm-ui.html) will be able to go directly to Asset Store. After installed you can also take the package out of project and use [local UPM feature](https://docs.unity3d.com/Manual/upm-ui-local.html) to link to `package.json`, freeing your `Assets` folder from things that aren't your game.
- You can pull from GitHub's `master` branch with UPM's Git functionality. Add the line below to your `manifest.json` : 

  ```
  "com.e7.notch-solution": "git://github.com/5argon/NotchSolution.git",
  ```

  However it will not update automatically when I push fixes to this repo. You must remove the lock line that appears in you `manifest.json` file to refetch.

The package is properly "UPM shaped" with [assembly definition files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html). If you also use one, the name of an assembly to link is `E7.NotchSolution` (GUID : `06dd7692457a446f7a9de9613998f95d`). C# namespace is also `E7.NotchSolution` if you want to extend the built-in components.

### 2. Use the components, iterate with the simulator

Learn available [components](components/overview.md) and use them in your design. Use [Notch Simulator](simulator/notch-simulator.md) to validate the design instantly.

You can also see the [how-to section](how-to/index.md) for some tricks and recipes.

### 3. Set the Project Settings before you build

![enable rendering into cutout](images/render-into-cutout.png)

All the work for this moment. Enable **Render outside safe area** under **Resolution and Presentation** for Android. Otherwise you get black bars.

For iOS, I think there is no option to do black bar as Apple discourages and may denies app that tries to hide the notch, therefore it already renders outside the safe area.

### 4. License

[The license is MIT](https://github.com/5argon/NotchSolution/blob/master/LICENSE). You should do your part in the open source software movement.

## See Notch Solution in action

I have in fact dogfood my own plugin so you don't have to worry much if the support for the package dies out because of "no demand", I demand it myself. The same goes to my other products.

The game is called [Duel Otters](https://duelotters.com/) which is free. Notch Solution is especially important in this game since it is a 2-player game where the other player will have to be on the notched side. Try it with various devices and see the UI adapts!

## It's open source

At first I am going to make it a normal Asset Store package like my other works. But I realized that this is the first one that is [not](http://exceed7.com/introloop/) [so](http://exceed7.com/native-audio/) [niche](http://exceed7.com/native-touch) in its use and could have widespread benefits to many, and as an open source that effect could be multiplied greatly. I only see notched devices increasing in the recent year.

I am not sure if I could come up with an another package with this potential, so I decided to take this opportunity for the first time. There is really no strings attached if that is what you were worrying. What I get by doing this?

- Screen cutout problems can be solved collaboratively. With so many devices in the world the problem space is HUGE. I think there are many variations and potentially different permutation of problems that bound to happen later. Over time, having more inputs from users together we could make this more stable than I could ever made alone.
- I got to proof I have open source development experience added to my portfolio and [my publisher page](https://assetstore.unity.com/publishers/18007). It says something differently about me than before.
- I get exposure to my other products, where you can expect similar quality and code discipline to Notch Solution.
- It is not necessary a bad financial/business move. The author of the popular [Odin Inspector](https://odininspector.com/) has [open sourced their Odin Serializer](https://devdog.io/blog/odin-serializer-goes-open-source/) with good reasons. More often than not, it also shows that they are capable of writing quality code.
