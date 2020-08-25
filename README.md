# SmartHunter - Monster Hunter: World Overlay

Based on [SmartHunter](https://github.com/gabrielefilipp/SmartHunter) and Inspired by [ReactHunter](https://github.com/Lenshang/ReactHunter) { Which is no longer updated [ Which is one of the reason I started this ( One of the other is that I wanted to made this game easier so my friend might actually try it out before they said this is too hard and not even bother to give a try ) ] }.

Localization and other parts are still the same, you change the JSON file (see the original repo for more info). Information for requirements, installation, usage, update, localization and skins can also be found in the original repo.

This way the screen can be cleaner.

## What it does

I simply added some functions to start a httpserver to respond to the page that pulls for the monster data once a while, which is passed on from the original function after they are done scanning.

## Edits made to the original

- Added [WebServer.cs](/SmartHunter/WebServer.cs)
- Added [PAGE.html](/SmartHunter/Resources/PAGE.html)
- Added Simplified Functions for passing data at the bottom of [MhwHelpers.cs](/SmartHunter/Game/Helpers/MhwHelpers.cs)
    - The simplified functions remembers the previously selected monsters and scan the addresses. The original method of scanning over the region and automatically find the monsters created an enormous lag for my keyboard input. It doesn't seems to be a problem with the original, but happens to me for some reason. The original cloned repo also behaves differently when debugging and actually using the release for no reason.
- Some changes were made for the conditions inside [MhwHelpers.cs](/SmartHunter/Game/MhwMemoryUpdater.cs)
    - Depending on the isVisible property of the UI, different functions were called.
- I personally is not a fan of knowing my teammates's damage, so this function is off by default.
    - Even if it was turned on, you need to manually click on the page to show the data. 
    - This function in the original also appears to be a little buggy, we sometimes had a total of damage higher than what's possible. For example, BEOTODUS seems to have 12k health, but for some reason, sometimes our total damage sums down to 16k.

## Usage for functions I added.
- Turn off the visibility of each UI.
- Enter [localhost:8080](http://localhost:8080/) in your browser.
    - Or, you can use your PC's ip address, just make sure that you:
        - Changed the string url under [WebServer.cs](/SmartHunter/WebServer.cs) from http://localhost:8080/ to  http://*:8080/
        - Check your firewall settings for the in and out bound rules to set the port, I used 8080 here, my typical testing port, you can find tutorials for [in](https://docs.microsoft.com/en-us/windows/security/threat-protection/windows-firewall/create-an-inbound-port-rule) and [out](https://docs.microsoft.com/en-us/windows/security/threat-protection/windows-firewall/create-an-outbound-port-rule).
        - The device you want to use to see the data in the same network.
- Some features are hidden as foldable, so you need to click to extend them. Like monster parts and team damage, etc.

## Releases
They are essentially the same version, but I planned on releasing multiple copy compiled in different way just in case anyone wants a specific version but don't know how to compile it. I will add a settings file in the future.

If you run it in admin it starts https server over lan automatically, if not, it starts at localhost.

## Credits
- Based on [SmartHunter](https://github.com/gabrielefilipp/SmartHunter)
- Inspired by [ReactHunter](https://github.com/Lenshang/ReactHunter)
- HTML template from [www.w3schools.com](https://www.w3schools.com/css/css_website_layout.asp)

