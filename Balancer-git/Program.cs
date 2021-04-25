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
		public void ScanGrid()
		{
			Block.ClearProperties();
			GridBlocks.Clear();
			this.GridTerminalSystem.GetBlocks(GridBlocks.terminalBlocks);
			foreach (IMyTerminalBlock block in GridBlocks.terminalBlocks)
			{
				if (!block.IsSameConstructAs(Me))
				{
					continue;
				}
				if (block.EntityId == Me.EntityId)
				{
					continue;
				}
				if (GridBlocks.AddBlock(block))
				{
					GridBlocks.UpdateCount(block.DefinitionDisplayNameText);
				}
			}
			GridBlocks.EvaluateThrusters();
			GridBlocks.EvaluateCameraBlocks();
			GridBlocks.EvaluateRemoteControls();
			GridBlocks.LogDifferences();
		}

		public static class RemoteControl
		{
			public static IMyRemoteControl block = null;
			public static bool Present()
			{
				return block != null;
			}
			public static bool PresentOrLog()
			{
				if (Present())
				{
					return true;
				}

				return false;
			}
		}


		public static class Block
		{
			public static bool ValidType(ref IMyTerminalBlock block, Type type)
			{
				return ValidProfile(ref block, Profiles.perType[type]);
			}
			public static bool ValidProfile(ref IMyTerminalBlock block, BlockProfile profile)
			{
				bool customNameValid = CustomName.Sanitize(ref block, ref profile);
				bool customDataValid = CustomData.Sanitize(ref block, ref profile);
				return customNameValid || customDataValid;
			}
			private static Dictionary<long, Dictionary<string, string>> properties = new Dictionary<long, Dictionary<string, string>>();
			public static void UpdateProperty(long entityId, string property, string value)
			{
				if (properties.ContainsKey(entityId))
				{
					properties[entityId][property] = value;
				}
				else
				{
					properties[entityId] = new Dictionary<string, string> { { property, value } };
				}
			}
			public static void ClearProperties()
			{
				foreach (KeyValuePair<long, Dictionary<string, string>> entities in properties)
				{
					entities.Value.Clear();
				}
			}
			public static bool HasProperty(long entityId, string name)
			{
				if (!properties.ContainsKey(entityId))
				{
					return false;
				}
				if (!properties[entityId].ContainsKey(name))
				{
					return false;
				}
				return true;
			}
			public static bool GetProperty(long entityId, string name, ref string value)
			{
				if (!HasProperty(entityId, name))
				{
					return false;
				}
				value = properties[entityId][name];
				return true;
			}
			public static void RemoveProperty(long entityId, string name)
			{
				if (!properties.ContainsKey(entityId))
				{
					return;
				}
				properties[entityId].Remove(name);
			}
		}


		public class PairCounter
		{
			public int oldC;
			public int newC;
			public PairCounter()
			{
				this.oldC = 0;
				this.newC = 1;
			}
			public void Recount()
			{
				this.oldC = this.newC;
				this.newC = 0;
			}
			public int Diff()
			{
				return newC - oldC;
			}
		}
		public static class GridBlocks
		{
			public static IMyProgrammableBlock MasterProgrammableBlock;
			public static Dictionary<string, PairCounter> blockCount = new Dictionary<string, PairCounter>();
			public static List<IMyTerminalBlock> terminalBlocks = new List<IMyTerminalBlock>();
			public static List<IMyRemoteControl> remoteControls = new List<IMyRemoteControl>();
			public static List<IMyCameraBlock> cameraBlocks = new List<IMyCameraBlock>();
			public static List<IMyRadioAntenna> radioAntennas = new List<IMyRadioAntenna>();
			public static List<IMyLaserAntenna> laserAntennas = new List<IMyLaserAntenna>();
			public static List<IMyProgrammableBlock> programmableBlocks = new List<IMyProgrammableBlock>();
			public static List<IMyShipConnector> shipConnectors = new List<IMyShipConnector>();
			public static List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
			public static List<IMyGyro> gyroBlocks = new List<IMyGyro>();
			public static List<IMyThrust> thrustBlocks = new List<IMyThrust>();
			public static List<IMyTimerBlock> timerBlocks = new List<IMyTimerBlock>();
			public static List<IMyCockpit> cockpitBlocks = new List<IMyCockpit>();
			public static List<IMyBatteryBlock> batteryBlocks = new List<IMyBatteryBlock>();
			public static List<IMyCargoContainer> cargoBlocks = new List<IMyCargoContainer>();
			public static List<IMyGasTank> tankBlocks = new List<IMyGasTank>();
			public static IMyTerminalBlock terminalBlock;
			public static IMyRemoteControl remoteControl;
			public static IMyCameraBlock cameraBlock;
			public static IMyRadioAntenna radioAntenna;
			public static IMyLaserAntenna laserAntenna;
			public static IMyProgrammableBlock programmableBlock;
			public static IMyShipConnector shipConnector;
			public static IMyTextPanel textPanel;
			public static IMyGyro gyroBlock;
			public static IMyThrust thrustBlock;
			public static IMyTimerBlock timerBlock;
			public static IMyCockpit cockpitBlock;
			public static IMyBatteryBlock batteryBlock;
			public static IMyCargoContainer cargoBlock;
			public static IMyGasTank tankBlock;
			public static void Clear()
			{
				foreach (string key in blockCount.Keys)
				{
					blockCount[key].Recount();
				}
				terminalBlocks.Clear();
				remoteControls.Clear();
				cameraBlocks.Clear();
				radioAntennas.Clear();
				laserAntennas.Clear();
				programmableBlocks.Clear();
				shipConnectors.Clear();
				textPanels.Clear();
				gyroBlocks.Clear();
				thrustBlocks.Clear();
				timerBlocks.Clear();
				cockpitBlocks.Clear();
				batteryBlocks.Clear();
				cargoBlocks.Clear();
				tankBlocks.Clear();
			}
			public static void UpdateCount(string key)
			{
				if (blockCount.ContainsKey(key))
				{
					blockCount[key].newC++;
				}
				else
				{
					blockCount[key] = new PairCounter();
				}
			}
			public static void LogDifferences()
			{
				foreach (string key in blockCount.Keys)
				{
					var diff = blockCount[key].Diff();
					if (diff > 0)
					{
						Logger.Info(String.Format("Found {0}x {1}", diff, key));
					}
					else if (diff < 0)
					{
						Logger.Info(String.Format("Lost {0}x {1}", -diff, key));
					}
				}
			}




			public static bool AddBlock(IMyTerminalBlock block)
			{
				if ((remoteControl = block as IMyRemoteControl) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyRemoteControl)))
					{
						return false;
					}
					remoteControls.Add(remoteControl);
				}
				else if ((cameraBlock = block as IMyCameraBlock) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyCameraBlock)))
					{
						return false;
					}
					cameraBlocks.Add(cameraBlock);
				}
				else if ((radioAntenna = block as IMyRadioAntenna) != null)
				{
					radioAntennas.Add(radioAntenna);
				}
				else if ((laserAntenna = block as IMyLaserAntenna) != null)
				{
					laserAntennas.Add(laserAntenna);
				}
				else if ((programmableBlock = block as IMyProgrammableBlock) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyProgrammableBlock)))
					{
						return false;
					}
					programmableBlocks.Add(programmableBlock);
				}
				else if ((shipConnector = block as IMyShipConnector) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyShipConnector)))
					{
						return false;
					}
					shipConnectors.Add(shipConnector);
				}
				else if ((textPanel = block as IMyTextPanel) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyTextPanel)))
					{
						return false;
					}
					textPanels.Add(textPanel);
				}
				else if ((gyroBlock = block as IMyGyro) != null)
				{
					gyroBlocks.Add(gyroBlock);
				}
				else if ((thrustBlock = block as IMyThrust) != null)
				{
					if (Block.ValidType(ref block, typeof(IMyThrust)))
					{
						if (Block.HasProperty(block.EntityId, "IGNORE"))
						{
							return false;
						}
					}
					thrustBlocks.Add(thrustBlock);
				}
				else if ((timerBlock = block as IMyTimerBlock) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyTimerBlock)))
					{
						return false;
					}
					timerBlocks.Add(timerBlock);
				}
				else if ((cockpitBlock = block as IMyCockpit) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyCockpit)))
					{
						return false;
					}
					cockpitBlocks.Add(cockpitBlock);
				}
				else if ((batteryBlock = block as IMyBatteryBlock) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyBatteryBlock)))
					{
						return false;
					}
					batteryBlocks.Add(batteryBlock);
				}
				else if ((cargoBlock = block as IMyCargoContainer) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyCargoContainer)))
					{
						return false;
					}
					cargoBlocks.Add(cargoBlock);
				}
				else if ((tankBlock = block as IMyGasTank) != null)
				{
					if (!Block.ValidType(ref block, typeof(IMyGasTank)))
					{
						return false;
					}
					tankBlocks.Add(tankBlock);
				}
				else
				{
					return false;
				}
				return true;
			}
			private static string valStr;
			private static float valFloat;
			private static double valDouble;

			private static int xB, yB;
			private static int CompareThrusters(IMyThrust x, IMyThrust y)
			{
				xB = yB = 0;
				if (x.DefinitionDisplayNameText.Contains("Hydrogen "))
				{
					xB += 4;
				}
				else if (x.DefinitionDisplayNameText.Contains("Ion "))
				{
					xB += 2;
				}
				if (x.DefinitionDisplayNameText.Contains("Large "))
				{
					xB += 1;
				}
				if (y.DefinitionDisplayNameText.Contains("Hydrogen "))
				{
					yB += 4;
				}
				else if (y.DefinitionDisplayNameText.Contains("Ion "))
				{
					yB += 2;
				}
				if (y.DefinitionDisplayNameText.Contains("Large "))
				{
					yB += 1;
				}
				return xB - yB;
			}
			public static void EvaluateRemoteControls()
			{
				if (remoteControls.Count() == 1)
				{
					RemoteControl.block = remoteControls[0];
					//ErrorState.Reset(ErrorState.Type.NoRemoteController);
					//ErrorState.Reset(ErrorState.Type.TooManyControllers);
					return;
				};
				RemoteControl.block = null;
				//if (!ErrorState.Get(ErrorState.Type.TooManyControllers) && remoteControls.Count() > 1)
				//{
				//	ErrorState.Set(ErrorState.Type.TooManyControllers);
				//	Logger.Err("Too many remote controllers");
				//}
			}
			public static void EvaluateCameraBlocks()
			{
				foreach (IMyCameraBlock cameraBlock in cameraBlocks)
				{
					if (!cameraBlock.EnableRaycast)
					{
						cameraBlock.EnableRaycast = true;
					}
				}
			}
			public static void EvaluateThrusters()
			{
				thrustBlocks.Sort(CompareThrusters);
			}
		}

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
                foreach (IMyThrust thruster in GridBlocks.thrustBlocks)
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

                gridForwardVect = RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Forward);
                gridUpVect = RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Up);
                gridLeftVect = RemoteControl.block.CubeGrid.WorldMatrix.GetDirectionVector(Base6Directions.Direction.Left);
                mass = RemoteControl.block.CalculateShipMass().PhysicalMass;
                position = RemoteControl.block.CenterOfMass;
                orientation = RemoteControl.block.WorldMatrix.GetOrientation();
                radius = RemoteControl.block.CubeGrid.WorldVolume.Radius;
                forwardVector = RemoteControl.block.WorldMatrix.Forward;
                backwardVector = RemoteControl.block.WorldMatrix.Backward;
                rightVector = RemoteControl.block.WorldMatrix.Right;
                leftVector = RemoteControl.block.WorldMatrix.Left;
                upVector = RemoteControl.block.WorldMatrix.Up;
                downVector = RemoteControl.block.WorldMatrix.Down;
                linearVelocity = RemoteControl.block.GetShipVelocities().LinearVelocity;
                elevationVelocity = Vector3D.Dot(linearVelocity, upVector);
                planetDetected = RemoteControl.block.TryGetPlanetPosition(out planetCenter);
                naturalGravity = RemoteControl.block.GetNaturalGravity();
                inGravity = naturalGravity.Length() >= 0.5;
                if (inGravity)
                {
                    RemoteControl.block.TryGetPlanetElevation(MyPlanetElevation.Surface, out distanceToGround);
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
