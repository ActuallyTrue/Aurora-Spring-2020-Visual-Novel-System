# Aurora Visual Novel System Spring 2020
 A set of scripts intended for use with Unity to support dialogue and other functions found commonly in visual novels.
 
 Included assets not by me:
 - Carlito font (under SIL Open Font License)
 - UnityTimer by Alexander Briggs (MIT License)
 - PowerUp sound effect (Soniss GDC Audio Bundle)
 
------------------------------------------------------------

Notes:

- This repository is covered under the Mozilla Public License. Essentially, you can freely use, distribute, modify, and make derivative works that can be sold commercially, as long as the license is included with any use of the code/derivatives of the code and the source code used from this repository is made easily available (this is not legal advice, please read the full license)
- Please credit me if you use anything from this repository. Feel free to use any included art assets.
- I am building this system first and foremost for my own purposes, and not as a general tool for making visual novels. Thus, the scripts aren't very independent and you will have to rework them to fit your needs. I have done my best to organize functions in a way that's understandable, but comments are a bit sparse and there's probably a lot of organization that doesn't quite make sense.
- DialogeManager.cs contains most of the functionality for dialogue, including loading a text file, displaying text letter by letter, and a rudimentary history function. Controls are located in GameManager.cs. The Dialogue Manager looks for a text file named testScript located within Assets/Script.
- Command parsing is handled by TextCommands.cs, and commands are either immediately invoked from EventManager.cs or loaded into a queue in GameManager.cs depending on if the command is at the start of the line or the middle.
- EventManager.cs contains all functions that can be called from within the text. When writing a command into a text file, enter an open bracket, the method name, the argument type (list of acceptable argument type names inside TextCommands.cs ConvertParams()), an equal sign, the value being passed along, and a space to separate further argument types, finally ending with a closing bracket. Failure to follow this format generally results in an invocation error.
Examples:
[MethodName argumentType=argumentValue argumentTypeWithMultipleValues=Value1,Value2,Value3,ValueN]
[CharMove i=0 v3=4.20,6.9,0]
