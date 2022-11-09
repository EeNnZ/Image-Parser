# Image-Parser
Parses wallpapers from various websites.
 Feel free to use any parts of this app.
## Why
I got the idea while I was searching for a good wallpapers switching between many browser tabs
## What it does
Parses selected websites for a specified range of pages or number of wallpapers, then downloads it
## How to add custom website to parse
You just need to implement an abstract `ImageParser` class and `IParserOptions` interface. Look at the built-in parsers as an example
