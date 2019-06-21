# Notch Solution

![screenshot](.Documentation/images/ssMain.gif)

<img src="https://opencollective.com/notch-solution/tiers/backer/badge.svg?label=backer&color=brightgreen" />

<object type="image/svg+xml" data="https://opencollective.com/notch-solution/tiers/backer.svg?avatarHeight=36&width=600"></object>

A set of components and tools to solve notched/cutout phones layout problems for Unity. Whether you like it or not, the time has come for us designers to design in context of a notch and embrace it instead of hiding it. This tool also enables design-time preview which help you iterate your design without building the game. Minimum Unity version 2019.1.

## Reason for open source

I believe screen cutout problem must be solved collaboratively. I think there are many variations and potentially different permutation of problems that bound to happen later. Over time, having more inputs from users together we could make this more stable than I could ever made alone.

In this regard for financial support, instead of revenue from Asset Store, the main way will be through Open Collective. (https://opencollective.com/notch-solution) The backers are displayed just under the opening image.

Alternatively, you could also check out the **Sponsor** button on top of this page where there is a link to my other Asset Store items, that way you could have some Unity tools in return as my thanks for a contribution. Thank you!

The Discord channel [is also available here!](https://discord.gg/J4sCcj4) You could come and express your wishlist or issues. (You could also use the Issues section.)

## How to include with GitHub functionality of Unity Package Manager

Add this line `"com.e7.notch-solution": "git://github.com/5argon/NotchSolution.git",` to your `manifest.json`.

It does not update automatically when I push fixes to this repo. You must remove the lock line that appears in you `manifest.json` file to refetch. Otherwise you would better use Asset Store. (when it is available)

## Asset Store

It will be on Unity Asset Store later too, but currently I don't think it is Asset Store worthy. There are some weird glitches remaining. (See [Issues section](https://github.com/5argon/NotchSolution/issues))

# Documentation

* **[Components](.Documentation/Components.md)** : Attach to your game objects to make them notch-aware.
* **[Simulation](.Documentation/Simulation.md)** : Iterate your design in context of notch right from Unity editor.
* **[How-to and tricks](.Documentation/HowTo.md)** : Useful design patterns.

# Need help / TODO

Please see the [Issue section](https://github.com/5argon/NotchSolution/issues).