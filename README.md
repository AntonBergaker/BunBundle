# BunBundle
BunBundle is a work in progress Monogame sprite organization tool and packer. It presents an easy to navigate tree structure for your sprite resources.
You can easily replace sprites, set their origins and combine multiple frames into an animation.
![Image of editor](https://github.com/AntonBergaker/BunBundle/blob/master/MarketingMaterial/InsideEditor.png?raw=true "Image of editor")

Because working with strings are the worst, upon building, a C# class is generated that contains all your sprites in the same tree format as your resource tree.
![Image of Visual Studio](https://github.com/AntonBergaker/BunBundle/blob/master/MarketingMaterial/StaticAvailable.png?raw=true "Static sprite access")

The tool was made for my own project and I only really made it public so some of my cool friends could also use it. All features will be heavily biased to my own needs. Sorry about that.

## Download
Since the tool is currently in progress and quite unstable, there are no public releases.

## Building from source
The tool is available as both a console line application (only for building) and as a graphical interface. Both projects require .NET5.0 and can be built like any other project.

## Upcoming features
* Texture page packing
* Rewrite view in Desktop Blazor
* Disable mipmapping for exported sprites
