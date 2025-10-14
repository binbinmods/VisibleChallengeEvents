using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
// using static Obeliskial_Essentials.Essentials;
using System;
using static VisibleChallengeEvents.Plugin;
using static VisibleChallengeEvents.CustomFunctions;
using static VisibleChallengeEvents.VisibleChallengeEventsFunctions;
using System.Collections.Generic;
using static Functions;
using UnityEngine;
// using Photon.Pun;
using TMPro;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
// using Unity.TextMeshPro;

// Make sure your namespace is the same everywhere
namespace VisibleChallengeEvents
{

    [HarmonyPatch] // DO NOT REMOVE/CHANGE - This tells your plugin that this is part of the mod

    public class VisibleChallengeEventsPatches
    {
        public static bool devMode = false; //DevMode.Value;
        public static bool bSelectingPerk = false;
        public static bool IsHost()
        {
            return GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster();
        }


        public static bool IsPopupNode = false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PopupNode), "WriteTitle")]
        public static void WriteTitlePrefix()
        {
            IsPopupNode = true;

        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PopupNode), "WriteTitle")]
        public static void WriteTitlePostfix()
        {
            IsPopupNode = false;
        }

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(GameManager), "IsObeliskChallenge")]
        // public static void WriteTitlePrefix(ref bool __result)
        // {
        //     IsPopupNode = true;

        // }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "IsObeliskChallenge")]
        public static void IsObeliskChallengePostfix(ref bool __result)
        {
            if (IsPopupNode)
                __result = true;
        }






    }
}