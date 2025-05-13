# McpClientServerDemo

A simple MCP Server &amp; Client example with mcp.json config.
The mcp.json roughly matches VSCode's [settings.json](https://code.visualstudio.com/docs/copilot/chat/mcp-servers), so in theory it can just import VSCode's settings.

**Server** defines the following MCP servers:
  * Youtube subtitles extractor

**Client** consumes the following MCP servers:
  * Youtube subtitles extractor ^
  * Yahoo Finance server
  * Airbnb server
  * Hacker News
  * Simple Calculator
(just a bunch of random public cloud MCP servers for the demo purposes)

Prompt example:
```
Short explanation of the problem described in this video https://www.youtube.com/watch?v=iSNsgj1OCLA.
Also, print the current price of MSFT stock
```
Answer:
```
 ** calling func ExtractYoutubeSubtitles(...) **
 ** calling func get_ticker_info(...) **

Short explanation of the problem in the video:
The video describes the "100 prisoners problem," a counterintuitive logic puzzle. There are
100 prisoners, each numbered 1 to 100, and 100 boxes, each hiding a slip with a unique prisoner
number. Each prisoner, one at a time, can open up to 50 boxes to find their own number. If all
prisoners succeed, they go free; if even one fails, all are executed. If they choose boxes
randomly, the chance of everyone surviving is astronomically small. However, using a clever
strategy-each prisoner following a chain by starting at the box with their own number and
always moving to the box whose number is on the slip they find-the probability of all surviving
rises to about 31%. The catch: it relies on the arrangement of numbered loops in the boxes,
and the math behind it is strikingly unintuitive.

Current price of MSFT stock:
Microsoft Corporation (MSFT) is currently trading at $449.26 USD.
```
