# StreamDeck Hunt-MMR plugin

View your last game's MMR

![image](https://user-images.githubusercontent.com/4050412/180929484-d781323d-ca7e-432c-830f-70cbb1490f09.png)

## Setup
The attributes.xml is found within [...]\steamapps\common\Hunt Showdown\user\profiles\default\attributes.xml
The username must match exactly the same used within Hunt, if not sure copy it from the xml file.

### Limitation:
The MMR is found in the attributes.xml which is created as a summary of the last game. This means you may see your MMR delayed per 1 match.

### ![Using BarRaider's Stream Deck Tools](https://github.com/BarRaider/streamdeck-tools)
C# library that wraps all the communication with the Stream Deck App, allowing you to focus on actually writing the Plugin's logic.

### TO DO
- Add keypress action to launch game
