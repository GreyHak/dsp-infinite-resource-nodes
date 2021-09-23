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
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DSPInfiniteResourceNodes
{
    public static class DSPInfiniteResourceNodes
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };
        public static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("DSP Infinite Resource Nodes");

        public const bool enableMinerPatchFlag = true;
        public const bool enableIcarusPatchFlag = true;

        public static void Patch(AssemblyDefinition assembly)
        {
            if (assembly.MainModule != null)  // Assembly-CSharp.dll
            {
                TypeDefinition minerComponent = assembly.MainModule.GetType("MinerComponent");
                foreach (MethodDefinition method in minerComponent.Methods)
                {
                    if (method.Name == "InternalUpdate")
                    {
                        /*for (int lineIdx = 0; lineIdx < method.Body.Instructions.Count; lineIdx++)
                        {
                            Console.WriteLine($"MinerComponent.InternalUpdate:{lineIdx}: {method.Body.Instructions[lineIdx]}");
                        }*/

                        ILProcessor ilProcessor = method.Body.GetILProcessor();

                        int instructionIdx_miner1 = Find(method.Body.Instructions, new string[]{  // 209
                            // veinPool[num2].amount = veinPool[num2].amount - 1;
                            "ldflda System.Int32 VeinData::amount",
                            "dup",
                            "ldind.i4",
                            "ldc.i4.1",  // Replace with ldc.i4.0
                            "sub",
                            "stind.i4"});

                        int instructionIdx_miner2 = Find(method.Body.Instructions, new string[]{  // 229
                            // factory.planet.veinAmounts[(int)veinPool[num].type] -= 1L;
                            "callvirt PlanetData PlanetFactory::get_planet()",
                            "ldfld System.Int64[] PlanetData::veinAmounts",
                            "ldarg.2",
                            "ldloc.1",
                            "ldelema VeinData",
                            "ldfld EVeinType VeinData::type",
                            "ldelema System.Int64",
                            "dup",
                            "ldind.i8",
                            "ldc.i4.1",  // Replace with ldc.i4.0
                            "conv.i8",
                            "sub"});

                        int instructionIdx_miner3 = Find(method.Body.Instructions, new string[]{  // 250
                            // veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - 1L;
                            "ldflda System.Int64 PlanetData/VeinGroup::amount",
                            "dup",
                            "ldind.i8",
                            "ldc.i4.1",  // Replace with ldc.i4.0
                            "conv.i8",
                            "sub",
                            "stind.i8"});

                        const int instructionOffset_miner1 = 3;
                        const int instructionOffset_miner2 = 9;
                        const int instructionOffset_miner3 = 3;

                        //Logger.LogDebug($"miner1={instructionIdx_miner1}+{instructionOffset_miner1}={instructionIdx_miner1 + instructionOffset_miner1}");
                        //Logger.LogDebug($"miner2={instructionIdx_miner2}+{instructionOffset_miner2}={instructionIdx_miner2 + instructionOffset_miner2}");
                        //Logger.LogDebug($"miner3={instructionIdx_miner3}+{instructionOffset_miner3}={instructionIdx_miner3 + instructionOffset_miner3}");

                        if (instructionIdx_miner1 == -1 ||
                            instructionIdx_miner2 == -1 ||
                            instructionIdx_miner3 == -1 ||
                            method.Body.Instructions[instructionIdx_miner1 + instructionOffset_miner1].OpCode != OpCodes.Ldc_I4_1 ||
                            method.Body.Instructions[instructionIdx_miner2 + instructionOffset_miner2].OpCode != OpCodes.Ldc_I4_1 ||
                            method.Body.Instructions[instructionIdx_miner3 + instructionOffset_miner3].OpCode != OpCodes.Ldc_I4_1)
                        {
                            Logger.LogError("ERROR: The Dyson Sphere Program appears to have been updated.  The Infinite Resource Nodes mod needs to be updated.  Until then miners will consume node resources.");
                        }
                        else if (enableMinerPatchFlag)
                        {
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_miner1 + instructionOffset_miner1], Instruction.Create(OpCodes.Ldc_I4_0));
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_miner2 + instructionOffset_miner2], Instruction.Create(OpCodes.Ldc_I4_0));
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_miner3 + instructionOffset_miner3], Instruction.Create(OpCodes.Ldc_I4_0));
                            Logger.LogInfo("Infinite resources patch applied for miners.");
                        }

                        break;
                    }
                }

                TypeDefinition playerAction_Mine = assembly.MainModule.GetType("PlayerAction_Mine");
                foreach (MethodDefinition method in playerAction_Mine.Methods)
                {
                    if (method.Name == "GameTick")
                    {
                        /*for (int lineIdx = 0; lineIdx < method.Body.Instructions.Count; lineIdx++)
                        {
                            Console.WriteLine($"PlayerAction_Mine.GameTick:{lineIdx}: {method.Body.Instructions[lineIdx]}");
                        }*/

                        ILProcessor ilProcessor = method.Body.GetILProcessor();

                        // Handles the node amount
                        int instructionIdx_icarus1 = Find(method.Body.Instructions, new string[] {  // 423
                            // veinPool[num13].amount = veinPool[num13].amount - 1;
                            "ldflda System.Int32 VeinData::amount",
                            "dup",
                            "ldind.i4",
                            "ldc.i4.1",
                            "sub",
                            "stind.i4"
                        });

                        // Handles the planet's total
                        int instructionIdx_icarus2 = Find(method.Body.Instructions, new string[] {  // 469
                            // factory.planet.veinAmounts[(int)veinData.type] -= 1L;
                            "ldloc.1",
                            "callvirt PlanetData PlanetFactory::get_planet()",
                            "ldfld System.Int64[] PlanetData::veinAmounts",
                            "ldloc.s V_26",  // On 9/22/2021 only this number changed.
                            "ldfld EVeinType VeinData::type",
                            "ldelema System.Int64",
                            "dup",
                            "ldind.i8",
                            "ldc.i4.1",
                            "conv.i8",
                            "sub",
                            "stind.i8"
                        });

                        // Handles the total in the node group
                        int instructionIdx_icarus3 = Find(method.Body.Instructions, new string[] {  // 487
                            // veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - 1L;
                            "ldflda System.Int64 PlanetData/VeinGroup::amount",
                            "dup",
                            "ldind.i8",
                            "ldc.i4.1",
                            "conv.i8",
                            "sub",
                            "stind.i8"
                        });

                        const int instructionOffset_icarus1 = 4;
                        const int instructionOffset_icarus2 = 10;
                        const int instructionOffset_icarus3 = 5;

                        //Logger.LogDebug($"icarus1={instructionIdx_icarus1}+{instructionOffset_icarus1}={instructionIdx_icarus1 + instructionOffset_icarus1}");
                        //Logger.LogDebug($"icarus2={instructionIdx_icarus2}+{instructionOffset_icarus2}={instructionIdx_icarus2 + instructionOffset_icarus2}");
                        //Logger.LogDebug($"icarus3={instructionIdx_icarus3}+{instructionOffset_icarus3}={instructionIdx_icarus3 + instructionOffset_icarus3}");

                        if (instructionIdx_icarus1 == -1 ||
                            instructionIdx_icarus2 == -1 ||
                            instructionIdx_icarus3 == -1 ||
                            method.Body.Instructions[instructionIdx_icarus1 + instructionOffset_icarus1].OpCode != OpCodes.Sub ||
                            method.Body.Instructions[instructionIdx_icarus2 + instructionOffset_icarus2].OpCode != OpCodes.Sub ||
                            method.Body.Instructions[instructionIdx_icarus3 + instructionOffset_icarus3].OpCode != OpCodes.Sub)
                        {
                            Logger.LogError("ERROR: The Dyson Sphere Program appears to have been updated.  The Infinite Resource Nodes mod needs to be updated.  Until then Icarus will consume node resources.");
                        }
                        else if (enableIcarusPatchFlag)
                        {
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_icarus1 + instructionOffset_icarus1], Instruction.Create(OpCodes.Pop));
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_icarus2 + instructionOffset_icarus2], Instruction.Create(OpCodes.Pop));
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_icarus3 + instructionOffset_icarus3], Instruction.Create(OpCodes.Pop));
                            Logger.LogInfo("Infinite resources patch applied for Icarus.");
                        }
                        break;
                    }
                }
            }
        }

        public static int Find(Mono.Collections.Generic.Collection<Instruction> haystack, string[] needle)
        {
            for (int hIdx = 0; hIdx < haystack.Count - needle.Length; hIdx++)
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
