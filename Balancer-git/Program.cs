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
        IMyCockpit _cockpit;
      



        // Script constructor
        public Program()
        {

            // Me is the programmable block which is running this script.
            // Retrieve the Large Display, which is the first surface
            //_drawingSurface = Me.GetSurface(0);
            _cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;
            Cockpit.block = _cockpit;

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
            if ((updateSource & UpdateType.Update100) != 0)
            {
                ScanGrid();

                var frame = _drawingSurface.DrawFrame();

                // All sprites must be added to the frame here
                DrawSprites(ref frame);

                // We are done with the frame, send all the sprites to the text panel
                frame.Dispose();

                Situation.RefreshParameters();
            }

            
            

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
                Data = "x:"+Situation.linearVelocity.X,
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
                Data = "y:" + Situation.linearVelocity.Y,
                Position = position,
                RotationOrScale = 1f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
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
                Data = "z:" + Situation.linearVelocity.Z,
                Position = position,
                RotationOrScale = 1f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            // Add the sprite to the frame
            frame.Add(sprite);
        }
     
        public static class Cockpit
        {
            public static IMyCockpit block;
        }

        public static class AppGridBlocks
        {
            public static List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
            public static List<IMyThrust> thrusters = new List<IMyThrust>();
            public static void Clear()
            {
                terminalBlocks.Clear();
                thrusters.Clear();
            }
            public static bool AddBlock(IMyTerminalBlock block)
            {
                IMyThrust thrustBlock;
                if ((thrustBlock = block as IMyThrust) != null)
                {
                    thrusters.Add(thrustBlock);                    
                }
               
                else
                {
                    return false;
                }
                return true;
            }
        }

        public void ScanGrid()
        {
            AppGridBlocks.Clear();
            this.GridTerminalSystem.GetBlocks(AppGridBlocks.terminalBlocks);
            foreach (IMyTerminalBlock block in AppGridBlocks.terminalBlocks)
            {
                AppGridBlocks.AddBlock(block);

            }
        }

        private static float HORIZONT_CHECK_DISTANCE = 2000.0f;
        private static float DISTANCE_TO_GROUND_IGNORE_PLANET = 1.2f * HORIZONT_CHECK_DISTANCE;
        
        public static class Situation
        {
            public static Vector3D position;
            public static Vector3D linearVelocity;
            public static double elevationVelocity;
            public static Vector3D naturalGravity;
            public static bool planetDetected;
            public static Vector3D planetCenter = new Vector3D();
            public static bool inGravity;
            public static double distanceToGround;
            public static double radius;
            public static float mass;
            public static Vector3D gravityUpVector;
            public static Vector3D gravityDownVector;
            public static Vector3D upVector;
            public static Vector3D forwardVector;
            public static Vector3D backwardVector;
            public static Vector3D downVector;
            public static Vector3D rightVector;
            public static Vector3D leftVector;
            public static MatrixD orientation;
            public static Vector3D gridForwardVect;
            public static Vector3D gridUpVect;
            public static Vector3D gridLeftVect;
            private static Dictionary<Base6Directions.Direction, double> maxThrust = new Dictionary<Base6Directions.Direction, double>() { { Base6Directions.Direction.Backward, 0 }, { Base6Directions.Direction.Down, 0 }, { Base6Directions.Direction.Forward, 0 }, { Base6Directions.Direction.Left, 0 }, { Base6Directions.Direction.Right, 0 }, { Base6Directions.Direction.Up, 0 }, };
            private static double forwardChange, upChange, leftChange;
            private static Vector3D maxT;
            public static double GetMaxThrust(Vector3D dir)
            {
                // return MAX_TRUST_UNDERESTIMATE_PERCENTAGE * maxThrust.MinBy(kvp => (float)kvp.Value).Value;
                forwardChange = Vector3D.Dot(dir, Situation.gridForwardVect);
                upChange = Vector3D.Dot(dir, Situation.gridUpVect);
                leftChange = Vector3D.Dot(dir, Situation.gridLeftVect);
                maxT = new Vector3D();
                maxT.X = forwardChange * maxThrust[(forwardChange > 0) ? Base6Directions.Direction.Forward : Base6Directions.Direction.Backward];
                maxT.Y = upChange * maxThrust[(upChange > 0) ? Base6Directions.Direction.Up : Base6Directions.Direction.Down];
                maxT.Z = leftChange * maxThrust[(leftChange > 0) ? Base6Directions.Direction.Left : Base6Directions.Direction.Right];
                return maxT.Length();
            }
            public static void RefreshParameters()
            {
                foreach (Base6Directions.Direction dir in maxThrust.Keys.ToList())
                {
                    maxThrust[dir] = 0;
                }
                foreach (IMyThrust thruster in AppGridBlocks.thrusters)
                {
                    if (!thruster.IsWorking)
                    {
                        continue;
                    }
                    maxThrust[thruster.Orientation.Forward] += thruster.MaxEffectiveThrust;
                }
                //var myList = maxThrust.ToList();
                //myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
                //for(int i=0; i<myList.Count-2; ++i) {
                //    maxThrust[myList[i].Key] = myList[i].Value / 2.0f;
                //}

             
                gridForwardVect = Cockpit.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Forward);
                gridUpVect = Cockpit.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Up);
                gridLeftVect = Cockpit.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Left);
                mass = Cockpit.block.CalculateShipMass().PhysicalMass;
                position = Cockpit.block.CenterOfMass;
                orientation = Cockpit.block.WorldMatrix.GetOrientation();
                radius = Cockpit.block.CubeGrid.WorldVolume.Radius;
                forwardVector = Cockpit.block.WorldMatrix.Forward;
                backwardVector = Cockpit.block.WorldMatrix.Backward;
                rightVector = Cockpit.block.WorldMatrix.Right;
                leftVector = Cockpit.block.WorldMatrix.Left;
                upVector = Cockpit.block.WorldMatrix.Up;
                downVector = Cockpit.block.WorldMatrix.Down;
                linearVelocity = Cockpit.block.GetShipVelocities().LinearVelocity;
                elevationVelocity = Vector3D.Dot(linearVelocity, upVector);
                planetDetected = Cockpit.block.TryGetPlanetPosition(out planetCenter);
                naturalGravity = Cockpit.block.GetNaturalGravity();
                inGravity = naturalGravity.Length() >= 0.5;
                if (inGravity)
                {
                    Cockpit.block.TryGetPlanetElevation(MyPlanetElevation.Surface, out distanceToGround);
                    gravityDownVector = Vector3D.Normalize(naturalGravity);
                    gravityUpVector = -1 * gravityDownVector;
                }
                else
                {
                    distanceToGround = DISTANCE_TO_GROUND_IGNORE_PLANET;
                    gravityDownVector = downVector;
                    gravityUpVector = upVector;
                }
            }
        }
    }
}
