# GrandmaGreenCodeSamples
Samples of code that I worked on for the 2D mobile farming and pet simulator game, Grandma Green (2023)!

Collections:
- I created most of the collections system, which handles all game item data
- My contribution to the inventory system, which is part of the collections system, is all the backend for keeping track of the player's inventory
- Collections reads in a CSV of all the item data and processes it into whatever is needed by other systems in the game
[Note: This system is a bit hard-codey due to a lack of proper planning/coordination with design in the early stages of development and a lack of time to revamp. Classic student led project problems :') ]

Gardening:
- I was in charge of the garden customization, in which players can customize their gardens by dragging in decoration items that they buy in the shop, and personalizing their house, fence, and mailbox
- Each variation of the house, fence, and mailbox had to be carefully programmed so that they would fit nicely into their respective slots depending on the size and shape of the sprites
- I had to make sure that the customization saved properly by integrating it with the save system
- I also had to make customization compatible with the player's garden expansions that they would unlock later in the game

Shopkeeping:
- I implemented the backend for the gardening and decor shops
- Both shops had their own set of rules for which types of items would appear every day. This involved randomization, tracking the ratios of flowers, vegetables, and fruits, and more
- Much of this system pulled from the collections system

Apart from these three, I took on smaller tasks throughout the second half of the year. For example, I made it so that the camera would zoom in on whichever character was currently in dialogue, similar to how it is done in Animal Crossing: New Horizons.
