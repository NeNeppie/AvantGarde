# Avant-Garde for Dalamud

Avant-Garde is a Dalamud plugin aiming to provide a comprehensive list for all hint categories in the Fashion Report minigame.
This plugin is a [crowdsourced project](#contributing), tracking valid gear pieces based on category and the slot they occupy.

<p align="center">
    <img src="Images/image1.png" width="400">
</p>

### How does Fashion Report work?

Each week, a unique theme is presented along with hints for specific slots. These hints are not unique to the theme, and may appear across many different weeks. The goal is to reach a minimum of 80 out of 100 points. Below is a somewhat-technical dive into how score is calculated:

- You are awarded a "base" of 10 points for every piece of gear you have equipped. Accessories award 8 points instead.
- If a particular slot has a hint attached to it, the base points earned drop down to 2. Thus, by simply filling all slots (except for the offhand slot) the minimum possible score is 68.
- Points can be gained either by choosing the correct item for the hint or the correct dye for the slot.
- On slots with a hint attached, earning points upgrades the stamp/medal. Correct items grant 8/6 points (left vs right side of equipment) and will display a gold medal near them.
- Rarely, particular items may award a bonus point, for a total of 9/7 points.
- Lastly, for each slot, choosing the correct shade of a dye grants a point (determined by the icon of the dye item. as of 7.5 the logic still applies but is not visible). Choosing the exact dye grants 2 points.

It's worth noting that dyes are tied to the weekly theme, and thus cannot be predetermined. The plugin *currently* only cares about gear, and will not display any information about valid dyes.

## Contributing

You may contribute to the project by submitting gear pieces (or dyes!) that match a certain category. The plugin now automates the submission process whenever you do Fashion Report but requires you to manually opt-in.

### Data Collection

Should you choose to opt-in, the plugin will collect the following data every time you play the minigame:
- Fashion Report week number and theme information
- Your performance (i.e. your score)
- Items and dyes used

Data collected is completely anonymized. No personal or private information is ever collected, stored or sent. You may change your mind and opt-out at any time! Your choice doesn't affect the functionality of the rest of the plugin.

### Feedback

If you find any bugs, or have any suggestions, feel free to open an issue here on github, or reply to the forum post in the [Dalamud discord server](https://discord.gg/3NMcUV5) @ `#plugin-help-forum` ([here](https://discord.com/channels/581875019861328007/1166794253553381456/1166794253553381456)).

---

Additionally, a link to the old data spreadsheet can be found [here](https://docs.google.com/spreadsheets/d/1b9NwL-Ba4tS0ROSy1_4HPfi7QSMQWuhXKqFSSY9Ovp4/edit?usp=sharing).

## Todo / TBD

-   [WIP] Display dye statistics
-   Provide information on gear sources. Drop location? Cost from NPCs? Crafting requirement? (Incl. localization)
