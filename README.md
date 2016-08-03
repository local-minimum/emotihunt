# EmojiHunt

Get a task to find a set of emojis or things looking like them in the real world.
Use your camera to snap a photo.
Let the object recognition system of the game decide what you looked at.
Get a score for how good you did.
Get a rank.

## Current status

Roadmap towards release

* Create feed of previous photos
 * :white_chekc_mark: A card with Image, Emoji tags below and the scores and date.
 * Scrollable area with cards (load 10 at a time)
 * A notification card which sums up scores for all photos on the emoji set
* Let selection reset when (A) only 1 emoji remains or (B) week has passed.
* :white_check_mark: Let feed have buttons to other pages
* :white_check_mark: Create about page
* :white_check_mark: UI icons update
* :white_check_mark: Simply point to files on server rather than having software serve it.
* Create at least 16 emojis pixelated 64x64.
 * Make detection padd with transparent pixels to not cropaway images
* Make editor write out version ID directly to file to not make mistakes.

## Issues

* Selection only works if starts up in selection mode
* :white_check_mark: Server crashes too frequently (solved simpler serving solution).
* Sometimes app gridlock crash while wating for server

