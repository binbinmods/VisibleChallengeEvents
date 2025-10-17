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
using System.Text;
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


        public static bool IsPopupNode;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PopupNode), "WriteTitle")]
        public static void WriteTitlePrefix(PopupNode __instance, bool ___popupDone, int num, string text, Enums.MapIconShader shader = Enums.MapIconShader.None, Sprite spriteMap = null)
        {
            LogDebug($"Popup Done {___popupDone}");
            IsPopupNode = true;

        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PopupNode), "WriteTitle")]
        public static void WriteTitlePostfix(PopupNode __instance, bool ___popupDone, int num, string text, Enums.MapIconShader shader = Enums.MapIconShader.None, Sprite spriteMap = null)
        {
            LogDebug($"Popup Done {___popupDone}");
            LogDebug($"WriteTitlePostfix - Text to Write {text}");
            IsPopupNode = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PopupNode), "DoPopup")]
        public static void DoPopupPrefix(Node _node, PopupNode __instance, bool ___popupDone)
        {
            LogDebug($"DoPopupPrefix - Popup Done {___popupDone}");
            LogDebug($"DoPopupPrefix - Popup for node {_node?.nodeData?.NodeId ?? "null node"}, setting IsPopupNode from {IsPopupNode} to true");
            IsPopupNode = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PopupNode), "DoPopup")]
        public static void DoPopupPostfix(Node _node, PopupNode __instance, bool ___popupDone)
        {
            LogDebug($"DoPopupPostfix - Popup Done {___popupDone}");
            LogDebug($"DoPopupPostfix - Popup for node {_node?.nodeData?.NodeId ?? "null node"}, setting IsPopupNode from {IsPopupNode} to false");
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
            if (GetIsPopup())
            {
                LogDebug($"IsObeliskChallengePostfix - IsPopUpNode - Setting __result from {__result} to false");
                __result = false;
            }
            // __result = false;
            // PopupNode.


        }

        public static bool GetIsPopup()
        {
            return IsPopupNode;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PopupNode), "DoPopup")]
        public static bool DoPopup(ref PopupNode __instance, Node _node, ref bool ___popupDone, ref int ___elementsNum)
        {
            __instance.monsterSpriteFrontChampion.gameObject.SetActive(value: false);
            __instance.monsterSpriteBackChampion.gameObject.SetActive(value: false);
            Enums.MapIconShader shader = Enums.MapIconShader.None;
            Sprite spriteMap = null;
            if (!___popupDone)
            {
                if (_node.nodeData == null)
                {
                    __instance.Hide();
                    return false;
                }
                if (_node.nodeData.NodeName == "")
                {
                    __instance.Hide();
                    return false;
                }
                string text = "";
                text = ((true) ? Globals.Instance.GetNodeData(_node.nodeData.NodeId).NodeName : Texts.Instance.GetText("ObeliskChallenge"));
                if (text == "")
                {
                    text = _node.nodeData.NodeName;
                }
                WriteTitle(__instance, 0, text);
                __instance.Icon.gameObject.SetActive(value: true);
                __instance.Icon.sprite = _node.nodeImage.sprite;
                string text2 = "";
                bool flag = false;
                if (PlayerManager.Instance.IsNodeUnlocked(_node.GetNodeAssignedId()) || PlayerManager.Instance.IsNodeUnlocked(_node.nodeData.NodeId))
                {
                    flag = true;
                }
                bool flag2 = false;
                if (_node.GetNodeAction() == "combat" && _node.nodeData.NodeCombatTier != 0 && (MadnessManager.Instance.IsMadnessTraitActive("randomcombats") || AtOManager.Instance.IsChallengeTraitActive("randomcombats")))
                {
                    flag2 = true;
                }
                if (_node.nodeData != null && _node.nodeData.DisableRandom)
                {
                    flag2 = false;
                }
                if (flag2)
                {
                    string nodeId = _node.nodeData.NodeId;
                    string nodeAssignedId = _node.GetNodeAssignedId();
                    NodeData nodeData = Globals.Instance.GetNodeData(nodeId);
                    string text3 = "";
                    CombatData combatData = Globals.Instance.GetCombatData(nodeAssignedId);
                    if (combatData != null)
                    {
                        text3 = combatData.CombatId;
                    }
                    NPCData[] array = Functions.GetRandomCombat(seed: (nodeId + AtOManager.Instance.GetGameId() + text3).GetDeterministicHashCode(), combatTier: nodeData.NodeCombatTier, nodeSelectedId: nodeId);
                    if (array != null)
                    {
                        ___elementsNum = 2;
                        ShowHideElement(ref __instance, 1, state: false);
                        ShowHideElement(ref __instance, 2, state: true);
                        float num = -1.02f;
                        float num2 = 0.54f;
                        int num3 = 0;
                        int num4 = 0;
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i] != null)
                            {
                                num4++;
                            }
                        }
                        if (num4 == 3)
                        {
                            num = -0.75f;
                        }
                        for (int j = 0; j < 4; j++)
                        {
                            if (j < array.Length && array[j] != null)
                            {
                                __instance.CharIcon[j].sprite = array[j].SpriteSpeed;
                                __instance.CharIcon[j].gameObject.SetActive(value: true);
                                __instance.CharIcon[j].transform.localPosition = new Vector3(num + num2 * (float)num3, __instance.CharIcon[j].transform.localPosition.y, __instance.CharIcon[j].transform.localPosition.z);
                                num3++;
                            }
                            else
                            {
                                __instance.CharIcon[j].gameObject.SetActive(value: false);
                            }
                        }
                        if (array[0] != null && array[0].IsNamed)
                        {
                            string auraCurseImmune = Functions.GetAuraCurseImmune(array[0], nodeId);
                            AuraCurseData auraCurseData = Globals.Instance.GetAuraCurseData(auraCurseImmune);
                            if (auraCurseData != null)
                            {
                                __instance.monsterSpriteFrontChampion.gameObject.SetActive(value: true);
                                __instance.monsterSpriteFrontChampion.GetComponent<SpriteRenderer>().sprite = auraCurseData.Sprite;
                                __instance.monsterSpriteFrontChampionIcoBack.sprite = auraCurseData.Sprite;
                            }
                        }
                        if (array[3] != null && array[3].IsNamed)
                        {
                            string auraCurseImmune2 = Functions.GetAuraCurseImmune(array[3], nodeId);
                            AuraCurseData auraCurseData2 = Globals.Instance.GetAuraCurseData(auraCurseImmune2);
                            if (auraCurseData2 != null)
                            {
                                __instance.monsterSpriteBackChampion.gameObject.SetActive(value: true);
                                __instance.monsterSpriteBackChampion.GetComponent<SpriteRenderer>().sprite = auraCurseData2.Sprite;
                                __instance.monsterSpriteBackChampionIcoBack.sprite = auraCurseData2.Sprite;
                            }
                        }
                        if (AtOManager.Instance.Sandbox_lessNPCs != 0)
                        {
                            SortedDictionary<int, int> sortedDictionary = new SortedDictionary<int, int>();
                            for (int k = 0; k < array.Length; k++)
                            {
                                if (array[k] != null && !array[k].IsNamed && !array[k].IsBoss)
                                {
                                    sortedDictionary.Add(array[k].Hp * 10000 + k, k);
                                }
                            }
                            int num5 = AtOManager.Instance.Sandbox_lessNPCs;
                            if (num5 >= num4)
                            {
                                num5 = num4 - 1;
                            }
                            if (num5 > sortedDictionary.Count)
                            {
                                num5 = sortedDictionary.Count;
                            }
                            for (int l = 0; l < num5; l++)
                            {
                                __instance.CharIcon[sortedDictionary.ElementAt(l).Value].gameObject.SetActive(value: false);
                            }
                        }
                    }
                }
                else if ((!flag) || (false && !AtOManager.Instance.mapVisitedNodes.Contains(_node.nodeData.NodeId)))
                {
                    if (_node != null)
                    {
                        EventData eventData = Globals.Instance.GetEventData(_node.GetNodeAssignedId());
                        if (eventData != null)
                        {
                            shader = eventData.EventIconShader;
                            spriteMap = eventData.EventSpriteMap;
                        }
                    }
                    text2 = "?????";
                    ___elementsNum = 1;
                    ShowHideElement(ref __instance, 1, state: true);
                    ShowHideElement(ref __instance, 2, state: false);
                }
                else if (_node.GetNodeAction() == "combat")
                {
                    ___elementsNum = 2;
                    ShowHideElement(ref __instance, 1, state: false);
                    ShowHideElement(ref __instance, 2, state: true);
                    CombatData combatData2 = Globals.Instance.GetCombatData(_node.GetNodeAssignedId());
                    float num6 = -1.02f;
                    float num7 = 0.54f;
                    int num8 = 0;
                    int num9 = 0;
                    for (int m = 0; m < combatData2.NPCList.Length; m++)
                    {
                        if (combatData2.NPCList[m] != null)
                        {
                            num9++;
                        }
                    }
                    if (((GameManager.Instance.IsGameAdventure() && AtOManager.Instance.GetMadnessDifficulty() == 0) || (GameManager.Instance.IsSingularity() && AtOManager.Instance.GetSingularityMadness() == 0)) && combatData2.NpcRemoveInMadness0Index > -1 && AtOManager.Instance.GetActNumberForText() < 3)
                    {
                        num9--;
                    }
                    switch (num9)
                    {
                        case 3:
                            num6 = -0.75f;
                            break;
                        case 2:
                            num6 = -0.48f;
                            break;
                    }
                    for (int n = 0; n < 4; n++)
                    {
                        __instance.CharIcon[n].gameObject.SetActive(value: false);
                    }
                    for (int num10 = 0; num10 < combatData2.NPCList.Length; num10++)
                    {
                        if ((((!GameManager.Instance.IsGameAdventure() || AtOManager.Instance.GetMadnessDifficulty() != 0) && (!GameManager.Instance.IsSingularity() || AtOManager.Instance.GetSingularityMadness() != 0)) || combatData2.NpcRemoveInMadness0Index != num10 || AtOManager.Instance.GetActNumberForText() >= 3) && combatData2.NPCList[num10] != null)
                        {
                            __instance.CharIcon[num10].sprite = combatData2.NPCList[num10].SpriteSpeed;
                            __instance.CharIcon[num10].gameObject.SetActive(value: true);
                            __instance.CharIcon[num10].transform.localPosition = new Vector3(num6 + num7 * (float)num8, __instance.CharIcon[num10].transform.localPosition.y, __instance.CharIcon[num10].transform.localPosition.z);
                            num8++;
                        }
                    }
                    if (AtOManager.Instance.Sandbox_lessNPCs != 0)
                    {
                        SortedDictionary<int, int> sortedDictionary2 = new SortedDictionary<int, int>();
                        for (int num11 = 0; num11 < combatData2.NPCList.Length; num11++)
                        {
                            if (combatData2.NPCList[num11] != null && !combatData2.NPCList[num11].IsNamed && !combatData2.NPCList[num11].IsBoss)
                            {
                                sortedDictionary2.Add(combatData2.NPCList[num11].Hp * 10000 + num11, num11);
                            }
                        }
                        int num12 = AtOManager.Instance.Sandbox_lessNPCs;
                        if (num12 >= num8)
                        {
                            num12 = num8 - 1;
                        }
                        if (num12 > sortedDictionary2.Count)
                        {
                            num12 = sortedDictionary2.Count;
                        }
                        for (int num13 = 0; num13 < num12; num13++)
                        {
                            __instance.CharIcon[sortedDictionary2.ElementAt(num13).Value].gameObject.SetActive(value: false);
                        }
                    }
                }
                else
                {
                    string nodeAssignedId2 = _node.GetNodeAssignedId();
                    if (!(nodeAssignedId2 != ""))
                    {
                        return false;
                    }
                    if (nodeAssignedId2 != "town" && nodeAssignedId2 != "destination")
                    {
                        EventData eventData2 = Globals.Instance.GetEventData(nodeAssignedId2);
                        string text4 = Texts.Instance.GetText(eventData2.EventId + "_nm", "events");
                        text2 = ((!(text4 != "")) ? eventData2.EventName : text4);
                        shader = eventData2.EventIconShader;
                        spriteMap = eventData2.EventSpriteMap;
                        ___elementsNum = 1;
                        ShowHideElement(ref __instance, 1, state: true);
                        ShowHideElement(ref __instance, 2, state: false);
                    }
                    else
                    {
                        string text5 = AtOManager.Instance.GetTownZoneId().ToLower();
                        string text6 = ((!Globals.Instance.ZoneDataSource.ContainsKey(text5)) ? Texts.Instance.GetText(text5) : Texts.Instance.GetText(Globals.Instance.ZoneDataSource[text5].ZoneName));
                        text2 = ((!(text6 != "")) ? text5 : text6);
                        __instance.Icon.gameObject.SetActive(value: false);
                        ___elementsNum = 0;
                        ShowHideElement(ref __instance, 1, state: true);
                        ShowHideElement(ref __instance, 2, state: false);
                    }
                }
                if (_node.nodeData.NodeGround != 0)
                {
                    if (!__instance.Ground.gameObject.activeSelf)
                    {
                        __instance.Ground.gameObject.SetActive(value: true);
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(Functions.GetNodeGroundSprite(_node.nodeData.NodeGround));
                    if (stringBuilder.ToString() != "")
                    {
                        stringBuilder.Append(Texts.Instance.GetText(Enum.GetName(typeof(Enums.NodeGround), _node.nodeData.NodeGround)));
                    }
                    __instance.GroundText.text = stringBuilder.ToString();
                    if (_node.GetNodeAction() == "combat")
                    {
                        __instance.Ground.localPosition = new Vector3(__instance.Ground.localPosition.x, -1.32f, __instance.Ground.localPosition.z);
                    }
                    else
                    {
                        __instance.Ground.localPosition = new Vector3(__instance.Ground.localPosition.x, -1.02f, __instance.Ground.localPosition.z);
                    }
                }
                else if (__instance.Ground.gameObject.activeSelf)
                {
                    __instance.Ground.gameObject.SetActive(value: false);
                }
                WriteTitle(__instance, 1, text2, shader, spriteMap);
            }
            ___popupDone = true;
            ShowAction(ref __instance);
            return false;
        }

        public static void ShowAction(ref PopupNode __instance)
        {
            __instance.gameObject.SetActive(value: true);
            // __instance.show = true;
            Vector3 ori = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // __instance.transform.localPosition = __instance.CalcDestination(ori);

        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(PopupNode), "WriteTitle")]
        public static void WriteTitle(object instance, int num, string text, Enums.MapIconShader shader = Enums.MapIconShader.None, Sprite spriteMap = null)
        {

        }


        public static void ShowHideElement(ref PopupNode __instance, int num, bool state)
        {
            switch (num)
            {
                case 0:
                    __instance.Bg0.gameObject.SetActive(state);
                    break;
                case 1:
                    __instance.Bg1.gameObject.SetActive(state);
                    __instance.Element1.gameObject.SetActive(state);
                    break;
                case 2:
                    __instance.Bg2.gameObject.SetActive(state);
                    __instance.Element2.gameObject.SetActive(state);
                    break;
            }
        }






    }
}