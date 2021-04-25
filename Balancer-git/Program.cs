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

        IMyTextSurface _largePanelDrawingSurface;
        IMyTextSurface _leftPanelDrawingSurface;
        RectangleF _viewportLargePanel;
        RectangleF _viewportLeftPanel;
        IMyCockpit _cockpit;
        bool isActive;
      



        // Script constructor
        public Program()
        {
            isActive = false;
            // Me is the programmable block which is running this script.
            // Retrieve the Large Display, which is the first surface
            //_drawingSurface = Me.GetSurface(0);
            _cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;
            Cockpit.block = _cockpit;

            //_drawingSurface = GridTerminalSystem.GetBlockWithName("Screen") as IMyTextSurface;
            _largePanelDrawingSurface = _cockpit.GetSurface(0);
            _leftPanelDrawingSurface = _cockpit.GetSurface(1);




            // Set the continuous update frequency of this script
            Runtime.UpdateFrequency = UpdateFrequency.Update100 | UpdateFrequency.Update10;

            // Calculate the viewport offset by centering the surface size onto the texture size
            _viewportLargePanel = new RectangleF(
                (_largePanelDrawingSurface.TextureSize - _largePanelDrawingSurface.SurfaceSize) / 2f,
                _largePanelDrawingSurface.SurfaceSize
            );

            _viewportLeftPanel = new RectangleF(
                (_leftPanelDrawingSurface.TextureSize - _leftPanelDrawingSurface.SurfaceSize) / 2f,
                _leftPanelDrawingSurface.SurfaceSize
            );

            // Make the text surface display sprites
            PrepareTextSurfaceForSprites(_largePanelDrawingSurface);
            PrepareTextSurfaceForSprites(_leftPanelDrawingSurface);


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
            if ((updateSource & (UpdateType.Trigger | UpdateType.Terminal)) != 0)
            {
                if(argument == "1")
                {
                    isActive = true;
                }
                else {
                    isActive = false;
                }
            }


            if ((updateSource & UpdateType.Update100) != 0)
            {
                ScanGrid();

                var frameLargePanel = _largePanelDrawingSurface.DrawFrame();
                var frameLeftPanel = _leftPanelDrawingSurface.DrawFrame();


                // All sprites must be added to the frame here
                DrawLargePanel(ref frameLargePanel);
                DrawLeftPanel(ref frameLeftPanel);

                // We are done with the frame, send all the sprites to the text panel
                frameLargePanel.Dispose();
                frameLeftPanel.Dispose();

                Situation.RefreshParameters();
            }
            if ((updateSource & UpdateType.Update10) != 0 && isActive)
            {
                Guidance.Set();
                Guidance.Tick();
                if (Guidance.Done())
                {                    
                    Guidance.Release();
                }
            }
            if ((updateSource & UpdateType.Update10) != 0 && !isActive)
            {
                Guidance.Release();
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
        public void DrawLeftPanel(ref MySpriteDrawFrame frame)
        {
            // Set up the initial position - and remember to add our viewport offset
            var position = new Vector2(0, 0) + _viewportLeftPanel.Position;


            // Create our second line, we'll just reuse our previous sprite variable - this is not necessary, just
            // a simplification in this case.
            var sprite = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = "Gyro count:" + AppGridBlocks.gyros.Count(),
                Position = position,
                RotationOrScale = 1f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            // Add the sprite to the frame
            frame.Add(sprite);
        }
        public void DrawLargePanel(ref MySpriteDrawFrame frame)
        {
            // Set up the initial position - and remember to add our viewport offset
            var position = new Vector2(0, 0) + _viewportLargePanel.Position;

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
            public static List<IMyGyro> gyros = new List<IMyGyro>();


            private static IMyThrust thrustBlock;
            private static IMyGyro gyroBlock;


            public static void Clear()
            {
                terminalBlocks.Clear();
                thrusters.Clear();
                gyros.Clear();
            }
            public static bool AddBlock(IMyTerminalBlock block)
            {
                
                if ((thrustBlock = block as IMyThrust) != null)
                {
                    thrusters.Add(thrustBlock);                    
                }
                else if ((gyroBlock = block as IMyGyro) != null)
                {
                    gyros.Add(gyroBlock);
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
        private static float MAX_SPEED = 50f;

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



        private static float GUIDANCE_MIN_AIM_DISTANCE = 0.5f;
        private static double GYRO_GAIN = 1.0;
        private static double GYRO_MAX_ANGULAR_VELOCITY = Math.PI;
        private static double TICK_TIME = 0.16666f;
        private static float IDLE_POWER = 0.0000001f;

        public static class Guidance
        {
            private static Vector3D desiredPosition = new Vector3D();
            private static Vector3D desiredFront = new Vector3D();
            private static Vector3D desiredUp = new Vector3D();
            private static float desiredSpeed = MAX_SPEED;
            public static void Set()
            {
                desiredPosition = Cockpit.block.GetPosition() + Cockpit.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Forward) * 1000f;
                desiredFront = Cockpit.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Forward);
                desiredUp = Cockpit.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Up);
                desiredSpeed = 2f;
            }
            public static void Release()
            {
                foreach (IMyGyro gyro in AppGridBlocks.gyros)
                {
                    gyro.GyroOverride = false;
                }
                foreach (IMyThrust thruster in AppGridBlocks.thrusters)
                {
                    thruster.ThrustOverride = 0;
                }
            }
            public static void Tick()
            {
                Guidance.StanceTick();
                Guidance.GyroTick();
                Guidance.ThrusterTick();
            }
            public static bool Done()
            {
                return worldVector.Length() < 0.05 && pathLen <= 0.1f;
            }
            private static Vector3D pathNormal, path, aimTarget, upVector, aimVector;
            private static float pathLen;
            private static void StanceTick()
            {
                path = desiredPosition - Situation.position;
                pathLen = (float)path.Length();
                pathNormal = Vector3D.Normalize(path);
                if (desiredFront != Vector3D.Zero)
                {
                    aimTarget = Situation.position + desiredFront * Situation.radius;
                }
                else
                {
                    aimVector = (pathLen > GUIDANCE_MIN_AIM_DISTANCE) ? pathNormal : Situation.forwardVector;
                    if (Situation.inGravity)
                    {
                        aimTarget = Situation.position + Vector3D.Normalize(Vector3D.ProjectOnPlane(ref aimVector, ref Situation.gravityUpVector)) * Situation.radius;
                    }
                    else
                    {
                        aimTarget = Situation.position + aimVector * Situation.radius;
                    }
                }
                if (Situation.inGravity)
                {
                    upVector = (desiredUp == Vector3D.Zero) ? Situation.gravityUpVector : desiredUp;
                }
                else
                {
                    upVector = (desiredUp == Vector3D.Zero) ? Vector3D.Cross(aimVector, Situation.leftVector) : desiredUp;
                }
            }
            private static Quaternion invQuat;
            private static Vector3D direction, refVector, worldVector, localVector, realUpVect, realRightVect;
            private static double azimuth, elevation, roll;
            private static void GyroTick()
            {
                if (AppGridBlocks.gyros.Count == 0)
                {
                    return;
                }
                direction = Vector3D.Normalize(aimTarget - Situation.position);
                invQuat = Quaternion.Inverse(Quaternion.CreateFromForwardUp(Situation.forwardVector, Situation.upVector));
                refVector = Vector3D.Transform(direction, invQuat);
                Vector3D.GetAzimuthAndElevation(refVector, out azimuth, out elevation);
                realUpVect = Vector3D.ProjectOnPlane(ref upVector, ref direction);
                realUpVect.Normalize();
                realRightVect = Vector3D.Cross(direction, realUpVect);
                realRightVect.Normalize();
                roll = Vector3D.Dot(Situation.upVector, realRightVect);
                worldVector = Vector3.Transform((new Vector3D(elevation, azimuth, roll)), Situation.orientation);
                foreach (IMyGyro gyro in AppGridBlocks.gyros)
                {
                    localVector = Vector3.Transform(worldVector, Matrix.Transpose(gyro.WorldMatrix.GetOrientation()));
                    gyro.Pitch = (float)MathHelper.Clamp((-localVector.X * GYRO_GAIN), -GYRO_MAX_ANGULAR_VELOCITY, GYRO_MAX_ANGULAR_VELOCITY);
                    gyro.Yaw = (float)MathHelper.Clamp(((-localVector.Y) * GYRO_GAIN), -GYRO_MAX_ANGULAR_VELOCITY, GYRO_MAX_ANGULAR_VELOCITY);
                    gyro.Roll = (float)MathHelper.Clamp(((-localVector.Z) * GYRO_GAIN), -GYRO_MAX_ANGULAR_VELOCITY, GYRO_MAX_ANGULAR_VELOCITY);
                    gyro.GyroOverride = true;
                }
            }
            private static float forwardChange, upChange, leftChange, applyPower, massFix;
            private static Vector3D force, directVel, directNormal, indirectVel;
            private static double ttt, maxFrc, maxVel, maxAcc, TIME_STEP = 2.5 * TICK_TIME, smooth;
            private static float massA = 2000000.0f;
            private static float massB = 5000.0f;
            private static float massM = (massA - 2.0f * massB) / (massA - massB);
            private static float massN = 1.0f / (massA - massB);
            private static void ThrusterTick()
            {
                if (pathLen == 0.0f)
                {
                    return;
                }
                massFix = massM * Situation.mass + massN * Situation.mass * Situation.mass;

                force = Situation.mass * Situation.naturalGravity;
                directVel = Vector3D.ProjectOnVector(ref Situation.linearVelocity, ref pathNormal);
                directNormal = Vector3D.Normalize(directVel);
                if (!directNormal.IsValid())
                {
                    directNormal = Vector3D.Zero;
                }
                maxFrc = Situation.GetMaxThrust(pathNormal) - ((Vector3D.Dot(force, pathNormal) > 0) ? Vector3D.ProjectOnVector(ref force, ref pathNormal).Length() : 0.0);
                maxVel = Math.Sqrt(2.0 * pathLen * maxFrc / massFix);
                smooth = Math.Min(Math.Max((desiredSpeed + 1.0f - directVel.Length()) / 2.0f, 0.0f), 1.0f);
                maxAcc = 1.0f + (maxFrc / massFix) * smooth * smooth * (3.0f - 2.0f * smooth);
                ttt = Math.Max(TIME_STEP, Math.Abs(maxVel / maxAcc));
                force += massFix * -2.0 * (pathNormal * pathLen / ttt / ttt - directNormal * directVel.Length() / ttt);
                indirectVel = Vector3D.ProjectOnPlane(ref Situation.linearVelocity, ref pathNormal);
                force += massFix * indirectVel / TIME_STEP;
                forwardChange = (float)Vector3D.Dot(force, Situation.gridForwardVect);
                upChange = (float)Vector3D.Dot(force, Situation.gridUpVect);
                leftChange = (float)Vector3D.Dot(force, Situation.gridLeftVect);
                foreach (IMyThrust thruster in AppGridBlocks.thrusters)
                {
                    if (!thruster.IsWorking)
                    {
                        thruster.ThrustOverridePercentage = 0;
                        continue;
                    }
                    switch (thruster.Orientation.Forward)
                    {
                        case Base6Directions.Direction.Forward:
                            thruster.ThrustOverridePercentage = ((forwardChange < 0) ? IDLE_POWER : (Guidance.Drain(ref forwardChange, thruster.MaxEffectiveThrust)));
                            break;
                        case Base6Directions.Direction.Backward:
                            thruster.ThrustOverridePercentage = ((forwardChange > 0) ? IDLE_POWER : (Guidance.Drain(ref forwardChange, thruster.MaxEffectiveThrust)));
                            break;
                        case Base6Directions.Direction.Up:
                            thruster.ThrustOverridePercentage = ((upChange < 0) ? IDLE_POWER : (Guidance.Drain(ref upChange, thruster.MaxEffectiveThrust)));
                            break;
                        case Base6Directions.Direction.Down:
                            thruster.ThrustOverridePercentage = ((upChange > 0) ? IDLE_POWER : (Guidance.Drain(ref upChange, thruster.MaxEffectiveThrust)));
                            break;
                        case Base6Directions.Direction.Left:
                            thruster.ThrustOverridePercentage = ((leftChange < 0) ? IDLE_POWER : (Guidance.Drain(ref leftChange, thruster.MaxEffectiveThrust)));
                            break;
                        case Base6Directions.Direction.Right:
                            thruster.ThrustOverridePercentage = ((leftChange > 0) ? IDLE_POWER : (Guidance.Drain(ref leftChange, thruster.MaxEffectiveThrust)));
                            break;
                    }
                }
            }
            private static float Drain(ref float remainingPower, float maxEffectiveThrust)
            {
                applyPower = Math.Min(Math.Abs(remainingPower), maxEffectiveThrust);
                remainingPower = (remainingPower > 0) ? (remainingPower - applyPower) : (remainingPower + applyPower);
                return Math.Max(applyPower / maxEffectiveThrust, IDLE_POWER);
            }
        }

        public class Waypoint
        {
            public Stance stance;
            public float maxSpeed;
            public enum wpType
            {
                ALIGNING, DOCKING, UNDOCKING, CONVERGING, APPROACHING, NAVIGATING, TESTING, TAXIING
            };
            public wpType type;
            public Waypoint(Stance s, float m, wpType wt)
            {
                stance = s;
                maxSpeed = m;
                type = wt;
            }
          
            public static Waypoint FromString(string coordinates)
            {
                return new Waypoint(new Stance(Helper.UnserializeVector(coordinates), Vector3D.Zero, Vector3D.Zero), MAX_SPEED, wpType.CONVERGING);
            }
        }

        public class Stance
        {
            public Vector3D position;
            public Vector3D forward;
            public Vector3D up;
            public Stance(Vector3D p, Vector3D f, Vector3D u)
            {
                this.position = p;
                this.forward = f;
                this.up = u;
            }
        }

        public static class Helper
        {
            public static string FormatedWaypoint(bool stance, int pos)
            {
                return (stance ? "Ori " : "Pos ") + (++pos).ToString("D2");
            }
            public static string Capitalize(string s)
            {
                if (string.IsNullOrEmpty(s))
                {
                    return string.Empty;
                }
                return s.First().ToString().ToUpper() + s.Substring(1).ToLower();
            }
            public static Vector3D UnserializeVector(string str)
            {
                var parts = str.Split(':');
                Vector3D v = Vector3D.Zero;
                if (parts.Length != 6)
                {
                    return v;
                }
                try
                {
                    v = new Vector3D(double.Parse(parts[2]), double.Parse(parts[3]), double.Parse(parts[4]));
                }
                catch
                {
                }
                return v;
            }
        }
    }
}
