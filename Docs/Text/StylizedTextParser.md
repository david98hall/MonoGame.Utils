# How to use [StylizedTextParser](https://github.com/Davuskus/MonoGame.Utils/blob/master/MonoGame.Utils/Text/StylizedTextParser.cs)

StylizedTextParser is a utility class for parsing text with a certain syntax which will be explained below. The syntax allows for a lot of customization within the text, e.g. different fonts, colors and opacities.

# Syntax examples

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

# Parsing
The parser will look for braced areas, find the so called "style blocks" (surrounded by brackets), extract the specified styles (their order do not matter) and apply them to the other text within the braced area. Text that does not have a style block gets the default style which can be configured. Style blocks affect text to their left. 

New lines are by default defined with actual new lines (\n) but this can be configured as well.

The output from the parsing methods is a list of different "rows" containing all of the stylized words ready for rendering with MonoGame.

## Fitting to a certain width
There is a method for fitting the text to a certain width, thereby overriding any new lines in the original input text if necessary.

#### Fitting the text from Example 1:
<img src="Assets/fitted_stylized_text.jpg" width="40%">
