using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace laftho.Airlocks
{
    class Program : Sandbox.ModAPI.Ingame.MyGridProgram
    {
        #region script

        /* Simple Airlock Script for Space Engineers
           Thomas LaFreniere aka laftho
           v1.0 - May 29, 2016

            This script matches closest pairs of doors on the 
            current grid and ensures one is closed before the 
            other opens.

            Add this script to a Programmable Block and set a
            Timer Block loop of trigger now.

            Simply add [airlock] to the name of each of the 
            doors you wish to have this behavior. Script 
            automatically will find the matching pair and
            manage their states.

            If you wish to use a different key for your doors
            just call the Programmable Block with the script
            in your timer with your key value as the argument.
            Default key is [airlock]
         */

        Airlocks airlocks;

        public Program()
        {
            airlocks = new Airlocks(this);
        }

        public void Main(string argument)
        {
            if (!string.IsNullOrEmpty(argument) && argument != airlocks.Key)
                airlocks = new Airlocks(this, argument);

            airlocks.Run();
        }

        class Airlocks
        {
            public string Key { get; private set; }
            private Dictionary<string, AirlockDoor> doors;
            private List<AirlockPair> airlocks;

            private Sandbox.ModAPI.Ingame.MyGridProgram Program;

            public Airlocks(Sandbox.ModAPI.Ingame.MyGridProgram program, string key = "[airlock]")
            {
                this.doors = new Dictionary<string, AirlockDoor>();
                this.Program = program;
                this.Key = key;
            }

            private void Init()
            {
                var blocks = new List<IMyTerminalBlock>();
                Program.GridTerminalSystem.GetBlocksOfType<IMyDoor>(blocks, block => block.CustomName.Contains(Key) && block.CubeGrid == Program.Me.CubeGrid);

                var unmatchedDoors = new List<AirlockDoor>();

                var deleteKeys = new List<string>(doors.Keys);

                foreach (var block in blocks)
                {
                    var door = new AirlockDoor(block as IMyDoor);

                    if (!doors.ContainsKey(door.Id))
                    {
                        doors.Add(door.Id, door);
                    }
                    else
                    {
                        deleteKeys.Remove(door.Id);
                        door = doors[door.Id];
                    }

                    unmatchedDoors.Add(door);
                }

                foreach (var key in deleteKeys)
                {
                    doors.Remove(key);
                }

                airlocks = new List<AirlockPair>();

                while (unmatchedDoors.Count > 0)
                {
                    var door = unmatchedDoors[0]; unmatchedDoors.RemoveAt(0);

                    int distance = Int32.MaxValue;
                    int index = -1;
                    AirlockDoor match = null;

                    for (int i = 0; i < unmatchedDoors.Count; i++)
                    {
                        var possibleMatch = unmatchedDoors[i];

                        int dist = door.Door.Position.RectangularDistance(possibleMatch.Door.Position);

                        if (dist < distance)
                        {
                            distance = dist;
                            match = possibleMatch;
                            index = i;
                        }
                    }

                    if (match != null)
                    {
                        airlocks.Add(new AirlockPair(door, match));
                        if (index > -1)
                        {
                            unmatchedDoors.RemoveAt(index);
                        }
                    }
                }
            }

            public void Run()
            {
                Init();
                foreach (var pair in airlocks) pair.Check();
                foreach (var door in doors.Values) door.Update();
            }
        }

        class AirlockPair
        {
            public AirlockDoor Outside { get; private set; }   
            public AirlockDoor Inside { get; private set; }
            public AirlockPair(AirlockDoor outside, AirlockDoor inside)
            {
                Outside = outside;
                Inside = inside;
            }

            public void Check()
            {
                if (Outside.RequestToOpen)
                {
                    if (!Inside.IsOpen && !Inside.IsClosing)
                        Outside.Open();

                    if (Inside.IsOpen && !Inside.IsClosing)
                    {
                        Inside.Close();
                        Outside.Close();
                    }
                }

                if (Inside.RequestToOpen)
                {
                    if (!Outside.IsOpen && !Outside.IsClosing)
                        Inside.Open();

                    if (Outside.IsOpen && !Outside.IsClosing)
                    {
                        Outside.Close();
                        Inside.Close();
                    }
                }
            }
        }
        
        class AirlockDoor
        {
            public IMyDoor Door { get; private set; }

            public string Id
            {
                get
                {
                    var pos = Door.Position;
                    return pos.X + ":" + pos.Y + ":" + pos.Z;
                }
            }

            private bool PreviousOpen = false;
            public bool IsOpen { get { return Door.Open; } }
            public bool RequestToOpen { get; private set; }
            private int Ticks { get; set; }
            public bool IsOpening { get; private set; }
            public bool IsClosing { get; private set; }

            public AirlockDoor(IMyDoor door) {
                Door = door;
                Update();
            }

            private void Tick() { if (Ticks > 0) Ticks -= 1; }

            public void Open()
            {
                Door.ApplyAction("Open_On");
                RequestToOpen = false;
                Ticks = 100;
            }

            public void Close()
            {
                Door.ApplyAction("Open_Off");
                Ticks = 100;
            }

            public void Update()
            {
                if (!PreviousOpen && IsOpen && Ticks <= 0)
                {
                    RequestToOpen = true;
                }

                if (!PreviousOpen && IsOpen)
                {
                    IsOpening = true;
                    IsClosing = false;
                    Ticks = 100;
                }
                else if (PreviousOpen && !IsOpen)
                {
                    IsClosing = true;
                    IsOpening = false;
                    Ticks = 100;
                }
                else if (Ticks <= 0)
                {
                    IsOpening = false;
                    IsClosing = false;
                }

                PreviousOpen = IsOpen;
                Tick();
            }
        }

        #endregion
    }
}
