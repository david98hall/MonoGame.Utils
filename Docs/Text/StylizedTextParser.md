# How to use [StylizedTextParser](https://github.com/Davuskus/MonoGame.Utils/tree/master/MonoGame.Utils/Text/Stylized)

StylizedTextParser is a utility class for parsing text with a certain syntax which will be explained below. The syntax allows for a lot of customization within the text, e.g. different fonts, colors and opacities. It makes it easy to customize text the way you want just by writing it as plain text and inputting it as a string.

For an example of how to draw the stylized text with MonoGame, see the [MonoGame.ECS](https://github.com/Davuskus/MonoGame.ECS) repository file [RenderSystem.cs](https://github.com/Davuskus/MonoGame.ECS/blob/master/MonoGame.ECS/Systems/RenderSystem.cs).

## Syntax examples

### Example 1:
```
{Hello[color=Green]}, {World![color=Magenta, font = silkscreen20, opacity=0.75]}
{My [color=Red]} name is David.
{Goodbye! [color=lime green]} [font=silkscreen48, color=Blue]
```
### Example 2:
```
Play Game[font=silkscreen48, color=red, opacity=0.5]
```

#### Appearance when rendered with MonoGame:
<img src="Assets/stylized_text2.jpg" width="40%">

### Example 3:
```
Play Game
```

#### Appearance when rendered with MonoGame:
<img src="Assets/stylized_text3.jpg" width="40%">

## Parsing
The parser will look for braced areas, find the so called "style blocks" (surrounded by brackets), extract the specified styles (their order do not matter) and apply them to the other text within the braced area. Text that does not have a style block gets the default style which can be configured. Style blocks affect text to their left and multiple style blocks in the same direct scope does not work (you have to use braces for that).

New lines are by default defined with actual new lines (\n) but this can be configured as well.

Braces and brackets can be parsed as plain text if an escape character is placed in front of them. The default escape character is the backslash: \\.

The output from the parsing methods is a list of different "rows" containing all of the stylized words ready for rendering with MonoGame.

### Fitting to a certain width
There is a method for fitting the text to a certain width, thereby overriding any new lines in the original input text if necessary.

#### Fitting the text from Example 1:
<img src="Assets/fitted_stylized_text.jpg" width="40%">

## Tips

### Strings in XML content files
A tip is to put all strings in XML content files and load them via the content manager to avoid taking up a lot of space in the code.

```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:ns="Microsoft.Xna.Framework">
  <Asset Type="System.Collections.Generic.Dictionary[System.String, System.String]">
    <Item>
      <Key>Example1</Key>
      <Value>
        {Hello[color=Green]}, {World![color=Magenta, font = silkscreen20, opacity=0.75]}
        {My [color=Red]} name is David.
        {Goodbye! [color=lime green]} [font=silkscreen48, color=Blue]
      </Value>
    </Item>
    <Item>
      <Key>PlayButton</Key>
      <Value>
        Play Game[font=silkscreen48, color=black]
      </Value>
    </Item>
  </Asset>
</XnaContent>
```
