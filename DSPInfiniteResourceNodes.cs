//
// Copyright (c) 2021, Aaron Shumate
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE.txt file in the root directory of this source tree. 
//
// Dyson Sphere Program is developed by Youthcat Studio and published by Gamera Game.

using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DSPInfiniteResourceNodes
{
    public static class DSPInfiniteResourceNodes
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };
        public static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("DSP Infinite Resource Nodes");

        public static ConfigEntry<bool> enableMinerPatchFlag;
        public static ConfigEntry<bool> enableOilPatchFlag;
        public static ConfigEntry<bool> enableIcarusPatchFlag;

        public const bool enableDebug_miner = false;
        public const bool enableDebug_oil = false;
        public const bool enableDebug_icarus = false;

        public static void BindConfigs()
        {
            ConfigFile irConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "InfiniteResources.cfg"), true);
       
            enableMinerPatchFlag = irConfig.Bind<bool>("General", "EnableMinerPatchFlag", false, "Enable/Disable Infinite Ores.");
            enableOilPatchFlag = irConfig.Bind<bool>("General", "EnableOilPatchFlag", true, "Enable/Disable Infinite Oil.");
            enableIcarusPatchFlag = irConfig.Bind<bool>("General", "EnableIcarusPatchFlag", false, "Enable/Disable Infinite Icarus Mining.");
        }
        
        public static void Patch(AssemblyDefinition assembly)
        {
            BindConfigs();
            if (assembly.MainModule != null)  // Assembly-CSharp.dll
            {
                TypeDefinition minerComponent = assembly.MainModule.GetType("MinerComponent");
                foreach (MethodDefinition method in minerComponent.Methods)
                {
                    if (method.Name == "InternalUpdate")
                    {
                        if (enableDebug_miner || enableDebug_oil)
                        {
#pragma warning disable CS0162
                            for (int lineIdx = 0; lineIdx < method.Body.Instructions.Count; lineIdx++)
                            {
                                Console.WriteLine($"MinerComponent.InternalUpdate:{lineIdx}: {method.Body.Instructions[lineIdx]}");
                            }
#pragma warning restore CS0162
                        }

                        ILProcessor ilProcessor = method.Body.GetILProcessor();

                        int instructionIdx_miner1 = Find(method.Body.Instructions, 0, new string[]{  // :235:
                            // veinPool[num5].amount = veinPool[num5].amount - num4;
                            "ldflda System.Int32 VeinData::amount",
                            "dup",
                            "ldind.i4",
                            "ldloc.s V_7",
                            "sub",
                            "stind.i4"});

                        int instructionIdx_miner3 = Find(method.Body.Instructions, 0, new string[]{  // :258:
                            // veinGroups[num6].amount = veinGroups[num6].amount - (long)num4;
                            "ldflda System.Int64 VeinGroup::amount",
                            "dup",
                            "ldind.i8",
                            "ldloc.s V_7",
                            "conv.i8",
                            "sub",
                            "stind.i8"});

                        const int instructionOffset_miner1 = 3;
                        const int instructionOffset_miner3 = 3;

                        if (enableDebug_miner)
                        {
#pragma warning disable CS0162
                            Logger.LogDebug($"miner1={instructionIdx_miner1}+{instructionOffset_miner1}={instructionIdx_miner1 + instructionOffset_miner1}");
                            Logger.LogDebug($"miner3={instructionIdx_miner3}+{instructionOffset_miner3}={instructionIdx_miner3 + instructionOffset_miner3}");
#pragma warning restore CS0162
                        }

                        if (instructionIdx_miner1 == -1 ||
                            instructionIdx_miner3 == -1 ||
                            method.Body.Instructions[instructionIdx_miner1 + instructionOffset_miner1].OpCode != OpCodes.Ldloc_S ||
                            method.Body.Instructions[instructionIdx_miner3 + instructionOffset_miner3].OpCode != OpCodes.Ldloc_S)
                        {
                            Logger.LogError("ERROR: The Dyson Sphere Program appears to have been updated.  The Infinite Resource Nodes mod needs to be updated.  Until then miners will consume node resources.");
                        }
                        else if (enableMinerPatchFlag)
                        {
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_miner1 + instructionOffset_miner1], Instruction.Create(OpCodes.Ldc_I4_0));
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_miner3 + instructionOffset_miner3], Instruction.Create(OpCodes.Ldc_I4_0));
                            Logger.LogInfo("Infinite resources patch applied for miners.");
                        }

                        int oilStartPosition = Find(method.Body.Instructions, 0, new string[] { "ldsfld System.Single VeinData::oilSpeedMultiplier" });
                        int instructionIdx_oil1 = -1;
                        int instructionIdx_oil3 = -1;

                        if (oilStartPosition != -1)
                        {
                            instructionIdx_oil1 = Find(method.Body.Instructions, oilStartPosition, new string[]{  // :553:
                                // veinPool[num13].amount = veinPool[num13].amount - num11;
                                "ldflda System.Int32 VeinData::amount",
                                "dup",
                                "ldind.i4",
                                "ldloc.s V_16",
                                "sub",
                                "stind.i4"});

                            instructionIdx_oil3 = Find(method.Body.Instructions, oilStartPosition, new string[]{  // :566:
                                // veinGroups2[(int)groupIndex2].amount = veinGroups2[(int)groupIndex2].amount - (long)num11;
                                "ldflda System.Int64 VeinGroup::amount",
                                "dup",
                                "ldind.i8",
                                "ldloc.s V_16",
                                "conv.i8",
                                "sub",
                                "stind.i8"});
                        }

                        const int instructionOffset_oil1 = 3;
                        const int instructionOffset_oil3 = 3;

                        if (enableDebug_oil)
                        {
#pragma warning disable CS0162
                            Logger.LogDebug($"oil1={instructionIdx_oil1}+{instructionOffset_oil1}={instructionIdx_oil1 + instructionOffset_oil1}");
                            Logger.LogDebug($"oil3={instructionIdx_oil3}+{instructionOffset_oil3}={instructionIdx_oil3 + instructionOffset_oil3}");
#pragma warning restore CS0162
                        }

                        if (instructionIdx_oil1 == -1 ||
                            instructionIdx_oil3 == -1 ||
                            method.Body.Instructions[instructionIdx_oil1 + instructionOffset_oil1].OpCode != OpCodes.Ldloc_S ||
                            method.Body.Instructions[instructionIdx_oil3 + instructionOffset_oil3].OpCode != OpCodes.Ldloc_S)
                        {
                            Logger.LogError("ERROR: The Dyson Sphere Program appears to have been updated.  The Infinite Resource Nodes mod needs to be updated.  Until then oil extractors will consume crude oil seep resources.");
                        }
                        else if (enableOilPatchFlag)
                        {
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_oil1 + instructionOffset_oil1], Instruction.Create(OpCodes.Ldc_I4_0));
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_oil3 + instructionOffset_oil3], Instruction.Create(OpCodes.Ldc_I4_0));
                            Logger.LogInfo("Infinite resources patch applied for crude oil extraction.");
                        }

                        break;
                    }
                }

                TypeDefinition playerAction_Mine = assembly.MainModule.GetType("PlayerAction_Mine");
                foreach (MethodDefinition method in playerAction_Mine.Methods)
                {
                    if (method.Name == "GameTick")
                    {
                        if (enableDebug_icarus)
                        {
#pragma warning disable CS0162
                            for (int lineIdx = 0; lineIdx < method.Body.Instructions.Count; lineIdx++)
                            {
                                Console.WriteLine($"PlayerAction_Mine.GameTick:{lineIdx}: {method.Body.Instructions[lineIdx]}");
                            }
#pragma warning restore CS0162
                        }

                        ILProcessor ilProcessor = method.Body.GetILProcessor();

                        // Handles the node amount
                        int instructionIdx_icarus1 = Find(method.Body.Instructions, 0, new string[] {  // :638:
                            // veinPool[num16].amount = veinPool[num16].amount - 1;
                            "ldflda System.Int32 VeinData::amount",
                            "dup",
                            "ldind.i4",
                            "ldc.i4.1",
                            "sub",
                            "stind.i4"
                        });

                        // Handles the total in the node group
                        int instructionIdx_icarus3 = Find(method.Body.Instructions, 0, new string[] {  // :649:
                            // veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - 1L;
                            "ldflda System.Int64 VeinGroup::amount",
                            "dup",
                            "ldind.i8",
                            "ldc.i4.1",
                            "conv.i8",
                            "sub",
                            "stind.i8"
                        });

                        const int instructionOffset_icarus1 = 4;
                        const int instructionOffset_icarus3 = 5;

                        if (enableDebug_icarus)
                        {
#pragma warning disable CS0162
                            Logger.LogDebug($"icarus1={instructionIdx_icarus1}+{instructionOffset_icarus1}={instructionIdx_icarus1 + instructionOffset_icarus1}");
                            Logger.LogDebug($"icarus3={instructionIdx_icarus3}+{instructionOffset_icarus3}={instructionIdx_icarus3 + instructionOffset_icarus3}");
#pragma warning restore CS0162
                        }

                        if (instructionIdx_icarus1 == -1 ||
                            instructionIdx_icarus3 == -1 ||
                            method.Body.Instructions[instructionIdx_icarus1 + instructionOffset_icarus1].OpCode != OpCodes.Sub ||
                            method.Body.Instructions[instructionIdx_icarus3 + instructionOffset_icarus3].OpCode != OpCodes.Sub)
                        {
                            Logger.LogError("ERROR: The Dyson Sphere Program appears to have been updated.  The Infinite Resource Nodes mod needs to be updated.  Until then Icarus will consume node resources.");
                        }
                        else if (enableIcarusPatchFlag)
                        {
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_icarus1 + instructionOffset_icarus1], Instruction.Create(OpCodes.Pop));
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_icarus3 + instructionOffset_icarus3], Instruction.Create(OpCodes.Pop));
                            Logger.LogInfo("Infinite resources patch applied for Icarus.");
                        }
                        break;
                    }
                }
            }
        }

        public static int Find(Mono.Collections.Generic.Collection<Instruction> haystack, int startPosition, string[] needle)
        {
            for (int hIdx = startPosition; hIdx < haystack.Count - needle.Length; hIdx++)
            {
                bool matchFlag = true;
                for (int nIdx = 0; nIdx < needle.Length; nIdx++)
                {
                    string heyCheck = haystack[hIdx + nIdx].ToString().Substring(9);
                    //Logger.LogDebug($"Comparing [{hIdx + nIdx}] '{heyCheck}' to [{nIdx}] '{needle[nIdx]}'");
                    if (heyCheck != needle[nIdx])
                    {
                        matchFlag = false;
                        break;
                    }
                    //else Logger.LogDebug("Match");
                }
                if (matchFlag)
                {
                    return hIdx;
                }
            }
            return -1;
        }
    }
}
