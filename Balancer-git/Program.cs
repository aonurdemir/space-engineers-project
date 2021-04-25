using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        IMyTextSurface _drawingSurface;
        RectangleF _viewport;

        // Script constructor
        public Program()
        {
            // Me is the programmable block which is running this script.
            // Retrieve the Large Display, which is the first surface
            //_drawingSurface = Me.GetSurface(0);
            IMyCockpit _cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;

            //_drawingSurface = GridTerminalSystem.GetBlockWithName("Screen") as IMyTextSurface;
            _drawingSurface = _cockpit.GetSurface(0);




            // Set the continuous update frequency of this script
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            // Calculate the viewport offset by centering the surface size onto the texture size
            _viewport = new RectangleF(
                (_drawingSurface.TextureSize - _drawingSurface.SurfaceSize) / 2f,
                _drawingSurface.SurfaceSize
            );

            // Make the text surface display sprites
            PrepareTextSurfaceForSprites(_drawingSurface);
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.

            // Begin a new frame
            var frame = _drawingSurface.DrawFrame();

            // All sprites must be added to the frame here
            DrawSprites(ref frame);

            // We are done with the frame, send all the sprites to the text panel
            frame.Dispose();
        }
        public void PrepareTextSurfaceForSprites(IMyTextSurface textSurface)
        {
            // Set the sprite display mode
            textSurface.ContentType = ContentType.SCRIPT;
            // Make sure no built-in script has been selected
            textSurface.Script = "";
        }

        // Drawing Sprites

        // Drawing Sprites
        public void DrawSprites(ref MySpriteDrawFrame frame)
        {
            // Set up the initial position - and remember to add our viewport offset
            var position = new Vector2(0, 0) + _viewport.Position;

            // Create our first line
            var sprite = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = "Abdullah Onur DEMIR",
                Position = position,
                RotationOrScale = 1f /* 80 % of the font's default size */,
                Color = Color.Red,
                Alignment = TextAlignment.LEFT /* Center the text on the position */,
                FontId = "White"
            };
            // Add the sprite to the frame
            frame.Add(sprite);

            // Move our position 20 pixels down in the viewport for the next line
            position += new Vector2(0, 20);

            // Create our second line, we'll just reuse our previous sprite variable - this is not necessary, just
            // a simplification in this case.
            sprite = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = "Halil Ibrahim DEMIR",
                Position = position,
                RotationOrScale = 1f,
                Color = Color.Blue,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            // Add the sprite to the frame
            frame.Add(sprite);
        }
    }
}
