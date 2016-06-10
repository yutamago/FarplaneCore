﻿//css_reference "..\bin\Debug\farplane.exe";
using System;
using System.Linq;
using System.Windows;

using Farplane;
using Farplane.Common;
using Farplane.FarplaneMod;
using Farplane.FFX;
using Farplane.FFX.Data;
using Farplane.FFX.Values;

public class AutoCameraMod : IFarplaneMod
{
    private bool _modActive = false;
    private static int offsetBattlePointer = Offsets.GetOffset(OffsetType.BattlePlayerPointer);
    private static int offsetCurrentRoom = Offsets.GetOffset(OffsetType.CurrentRoom);
    private static int offsetCameraFlag = Offsets.GetOffset(OffsetType.DebugFlags) + (int)DebugFlags.FreeCamera;
    private static int updateTicks = 0;
    private static int lastRoom = 0;

    private static ushort[] bannedRooms =
    {           // A list of known rooms which should not use auto camera

        0x00B0, // Macalania/Bevelle
        0x00B1, // Macalania/Bevelle
        0x0133, // Monster Arena
        0x0149, // Bevelle to Calm Lands
    };

    public string Name
    {
        get { return "Auto Free Camera"; }
    }

    public string Author
    {
        get { return "Topher"; }
    }

    public string Description
    {
        get { return "Automatically enables and disables the free camera debug option when entering combat and changing rooms.\n\nUse the right stick to move the camera. Can be glitchy, might need to change rooms to deactivate properly."; }
    }

    public GameType GameType
    {
        get { return GameType.FFX; }
    }

    public bool Activated
    {
        get { return _modActive; }
    }
	
    public void Activate()
    {
        if (_modActive) return;
        lastRoom = -1;
        ModLogger.WriteLine("Activating free camera mod");
        _modActive = true;
    }

    public void Deactivate()
    {
        if (!_modActive) return;
        ModLogger.WriteLine("Deactivating free camera mod");
        Memory.WriteByte(offsetCameraFlag, 0);
        _modActive = false;
    }

    public void Update()
    {
        if (!_modActive) return;

        updateTicks++;
        if (updateTicks < 50) return; // tick every ~400ms, faster and the game misses the flag on room change
        updateTicks = 0;

        var battlePointer = Memory.ReadUInt32(offsetBattlePointer);

        if (battlePointer == 0) // Not in battle, enable camera
        {
            // Check current room
            var currentRoom = Memory.ReadUInt16(offsetCurrentRoom);

            // Check if room is on banned rooms list
            if (Array.IndexOf(bannedRooms, currentRoom) != -1)
            {
                Memory.WriteByte(offsetCameraFlag, 0);
                lastRoom = currentRoom;
                return;
            }

            // If we have changed rooms, reset flag this tick
            if (currentRoom != lastRoom)
            {
                ModLogger.WriteLine("Room change detected: {0}", currentRoom.ToString("X2"));
                Memory.WriteByte(offsetCameraFlag, 0);
                lastRoom = currentRoom;
                return;
            }

            // Enable camera flag
            Memory.WriteByte(offsetCameraFlag, 1);
        }
        else // In battle, disable camera
        {
            Memory.WriteByte(offsetCameraFlag, 0);
        }
    }
}