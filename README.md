# Notch Solution

![screenshot](.Documentation/images/ssMain.gif)

<a href="https://opencollective.com/NotchSolution" alt="Financial Contributors on Open Collective"><img src="https://opencollective.com/NotchSolution/all/badge.svg?label=financial+contributors" /></a> <img src="https://opencollective.com/notch-solution/tiers/backer/badge.svg?label=backer&color=brightgreen" />

<object type="image/svg+xml" data="https://opencollective.com/notch-solution/tiers/backer.svg?avatarHeight=36&width=600"></object>

A set of components and tools to solve notched/cutout phones layout problems for Unity. Whether you like it or not, the time has come for us designers to design in context of a notch and embrace it instead of hiding it. This tool also enables design-time preview which help you iterate your design without building the game. Minimum Unity version 2019.1.

## Reason for open source

I believe screen cutout problem must be solved collaboratively. I think there are many variations and potentially different permutation of problems that bound to happen later. Over time, having more inputs from users together we could make this more stable than I could ever made alone.

In this regard for financial support, instead of revenue from Asset Store, the main way will be through Open Collective. (https://opencollective.com/notch-solution) The backers are displayed just under the opening image.

Alternatively, you could also check out the **Sponsor** button on top of this page where there is a link to my other Asset Store items, that way you could have some Unity tools in return as my thanks for a contribution. Thank you!

The Discord channel [is also available here!](https://discord.gg/J4sCcj4) You could come and express your wishlist or issues. (You could also use the Issues section.)

# Installation

## Include with GitHub functionality of Unity Package Manager

Add this line `"com.e7.notch-solution": "git://github.com/5argon/NotchSolution.git",` to your `manifest.json`.

It does not update automatically when I push fixes to this repo. You must remove the lock line that appears in you `manifest.json` file to refetch. Otherwise you would better do a direct copy.

It will be on Unity Asset Store later too, but currently I don't think it is Asset Store worthy. There are some weird glitches remaining. (See [Issues section](https://github.com/5argon/NotchSolution/issues))

## `Screen.safeArea` requirement

Some key components are using `Screen.safeArea`. For Android, **your player's phone has to be on Android P AND also you have to use Unity 2019.1 or over**. Otherwise I believe Android builds with black bar over the notch/cutout  and non-Pie Android do not have a dedicated API to report cutouts.

I think there is no problem on iOS. (?)

## Project settings

![enable rendering into cutout](.Documentation/images/renderIntoCutout.png)

You did all the work for this moment. Enable **Render outside safe area** under **Resolution and Presentation** for Android. Android has an explicit mode on the app to enable the black bar (Maybe [LAYOUT_IN_DISPLAY_CUTOUT_MODE_NEVER](https://developer.android.com/guide/topics/display-cutout/#never_render_content_in_the_display_cutout_area)). This check mark should ended up just enabling that mode without you touching the game.

For iOS, I think there is no option to do black bar as Apple discourages and may denies app that tries to hide the notch, therefore it already renders outside the safe area.

# `Screen.cutout` research started

New entry in **Unity 2019.2**, this time not just a safe area but it returns a rectangle **surrounding the notch**! Currently Notch Solution do not use this anywhere yet, but I am conducting a research to see what the phone reports for this property.

If you have a phone with cutouts, you could **join the research** by downloading a debug APK in the [release page](https://github.com/5argon/NotchSolution/releases). Then after running it on your phone, take a screenshot on both portrait and landscape orientation and submit your result in [this issue](https://github.com/5argon/NotchSolution/issues/2) so we know what it actually looks like, and we might be able to make use of it in the future! Thank you!

Some problems we could solve with components utilizing this API :

1. For centered cutout, we want those that could fit the corner to stay that way. This now create a problem for device with a cutout on the corner like Galaxy S10+. However if we setup it to go down according to safe area, now on a device like iPhoneX looks like there is nothing on the corner yet it moves down. In this image, I choose to prioritize the look on iPhoneX, therefore letting the hole punched through the UI in S10+.

![cutout problem 1](.Documentation/images/cutoutProblem1.jpg)

2. For device with a very small cutout like Galaxy S10's corner hole or Galaxy Note 10+'s center dot, if we trust only safe area, any small hole cause the entire width to be unusable and being padded equally. This may looks awkward as it looks like a wasted area.

![cutout problem 2](.Documentation/images/cutoutProblem2.jpg)

# Documentation

* **[Components](.Documentation/Components.md)** : Attach to your game objects to make them notch-aware.
* **[Simulation](.Documentation/Simulation.md)** : Iterate your design in context of notch right from Unity editor.
* **[How-to and tricks](.Documentation/HowTo.md)** : Useful design patterns.

# Need help / TODO

Please see the [Issue section](https://github.com/5argon/NotchSolution/issues).
## Contributors

### Code Contributors

This project exists thanks to all the people who contribute. [[Contribute](CONTRIBUTING.md)].
<a href="https://github.com/5argon/NotchSolution/graphs/contributors"><img src="https://opencollective.com/NotchSolution/contributors.svg?width=890&button=false" /></a>

### Financial Contributors

Become a financial contributor and help us sustain our community. [[Contribute](https://opencollective.com/NotchSolution/contribute)]

#### Individuals

<a href="https://opencollective.com/NotchSolution"><img src="https://opencollective.com/NotchSolution/individuals.svg?width=890"></a>

#### Organizations

Support this project with your organization. Your logo will show up here with a link to your website. [[Contribute](https://opencollective.com/NotchSolution/contribute)]

<a href="https://opencollective.com/NotchSolution/organization/0/website"><img src="https://opencollective.com/NotchSolution/organization/0/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/1/website"><img src="https://opencollective.com/NotchSolution/organization/1/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/2/website"><img src="https://opencollective.com/NotchSolution/organization/2/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/3/website"><img src="https://opencollective.com/NotchSolution/organization/3/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/4/website"><img src="https://opencollective.com/NotchSolution/organization/4/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/5/website"><img src="https://opencollective.com/NotchSolution/organization/5/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/6/website"><img src="https://opencollective.com/NotchSolution/organization/6/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/7/website"><img src="https://opencollective.com/NotchSolution/organization/7/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/8/website"><img src="https://opencollective.com/NotchSolution/organization/8/avatar.svg"></a>
<a href="https://opencollective.com/NotchSolution/organization/9/website"><img src="https://opencollective.com/NotchSolution/organization/9/avatar.svg"></a>
