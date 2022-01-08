using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using System;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnhollowerBaseLib;
using Hazel;

namespace TownOfHost
{
    [HarmonyPatch(typeof(ShipStatus), nameof(AmongUsClient.OnGameJoined))]
    class OnGameJoinedPatch {
        public static void Postfix(AmongUsClient __instance) {
            main.PlayerVersions = new Dictionary<byte, (string, bool)>();
            main.SyncVersionRPC();
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(AmongUsClient.OnPlayerJoined))]
    class OnPlayerJoinedPatch {
        public static void Postfix(AmongUsClient __instance) {
            main.SyncVersionRPC();
        }
    }
}