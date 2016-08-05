# EmojiHunt

Get a task to find a set of emojis or things looking like them in the real world.
Use your camera to snap a photo.
Let the object recognition system of the game decide what you looked at.
Get a score for how good you did.
Get a rank.

## Roadmap towards release

* :white_check_mark: Create feed of previous photos
 * :white_check_mark: A card with Image, Emoji tags below and the scores and date.
 * :white_check_mark: Scrollable area with cards (load 10 at a time)
 * A notification card which sums up scores for all photos on the emoji set
* :white_check_mark: Let selection reset when :white_check_mark: (A) only 1 emoji remains or :white_check_mark: (B) week has passed.
* :white_check_mark: Let feed have buttons to other pages
* :white_check_mark: Create about page
* :white_check_mark: UI icons update
* :white_check_mark: Simply point to files on server rather than having software serve it.
* Create at least 16 (:white_check_mark: 5) emojis pixelated 64x64.
 * :white_check_mark: Make detection pad with transparent pixels to not cropaway images
* :white_check_mark: Make editor write out version ID directly to file to not make mistakes.

## Issues

* Feed doesn't scroll
* Feed doesn't load more at end of scroll
* Cards don't get right size
* :white_check_mark: On activating emojis need to clear image at once
* :white_check_mark: Emoji get distorted because padded
* Selection only works if starts up in selection mode (it's OK).
* :white_check_mark: Server crashes too frequently (solved simpler serving solution).
* (3 days without) Sometimes app gridlock crash while wating for server

