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
                            Console.WriteLine(lineIdx.ToString() + ": " + method.Body.Instructions[lineIdx].ToString());
                        }*/

                        const int instructionIdx_miner1 = 187;
                        const int instructionIdx_miner2 = 213;
                        const int instructionIdx_miner3 = 227;

                        if (method.Body.Instructions[instructionIdx_miner1].OpCode != OpCodes.Ldc_I4_1 ||
                            method.Body.Instructions[instructionIdx_miner2].OpCode != OpCodes.Ldc_I4_1 ||
                            method.Body.Instructions[instructionIdx_miner3].OpCode != OpCodes.Ldc_I4_1)
                        {
                            Logger.LogInfo(method.Body.Instructions[instructionIdx_miner1].ToString());
                            Logger.LogInfo(method.Body.Instructions[instructionIdx_miner2].ToString());
                            Logger.LogInfo(method.Body.Instructions[instructionIdx_miner3].ToString());

                            Logger.LogError("ERROR: The Dyson Sphere Program appears to have been updated.  The Infinite Resource Nodes mod needs to be updated.  Until then miners will consume node resources.");
                        }
                        else
                        {
                            ILProcessor ilProcessor = method.Body.GetILProcessor();

                            // veinPool[num2].amount = veinPool[num2].amount - 1;
                            // 186: IL_01d5: ldfld System.Int32 VeinData::amount
                            // 187: IL_01da: ldc.i4.1 <<-- Replace with ldc.i4.0
                            // 188: IL_01db: sub
                            // 189: IL_01dc: stfld System.Int32 VeinData::amount
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_miner1], Instruction.Create(OpCodes.Ldc_I4_0));

                            // factory.planet.veinAmounts[(int)veinPool[num].type] -= 1L;
                            // 204: IL_020b: callvirt PlanetData PlanetFactory::get_planet()
                            // 205: IL_0210: ldfld System.Int64[] PlanetData::veinAmounts
                            // 206: IL_0215: ldarg.2
                            // 207: IL_0216: ldloc.1
                            // 208: IL_0217: ldelema VeinData
                            // 209: IL_021c: ldfld EVeinType VeinData::type
                            // 210: IL_0221: ldelema System.Int64
                            // 211: IL_0226: dup
                            // 212: IL_0227: ldind.i8
                            // 213: IL_0228: ldc.i4.1 <<-- Replace with ldc.i4.0
                            // 214: IL_0229: conv.i8
                            // 215: IL_022a: sub
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_miner2], Instruction.Create(OpCodes.Ldc_I4_0));

                            // veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - 1L;
                            // 226: IL_0249: ldfld System.Int64 PlanetData/VeinGroup::amount
                            // 227: IL_024e: ldc.i4.1 <<-- Replace with ldc.i4.0
                            // 228: IL_024f: conv.i8
                            // 229: IL_0250: sub
                            // 230: IL_0251: stfld System.Int64 PlanetData/VeinGroup::amount
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_miner3], Instruction.Create(OpCodes.Ldc_I4_0));

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
                            Console.WriteLine(lineIdx.ToString() + ": " + method.Body.Instructions[lineIdx].ToString());
                        }*/

                        const int instructionIdx_icarus1 = 446;
                        const int instructionIdx_icarus2 = 499;
                        const int instructionIdx_icarus3 = 511;

                        if (method.Body.Instructions[instructionIdx_icarus1].OpCode != OpCodes.Sub ||
                            method.Body.Instructions[instructionIdx_icarus2].OpCode != OpCodes.Sub ||
                            method.Body.Instructions[instructionIdx_icarus3].OpCode != OpCodes.Sub)
                        {
                            Logger.LogInfo(method.Body.Instructions[instructionIdx_icarus1].ToString());
                            Logger.LogInfo(method.Body.Instructions[instructionIdx_icarus2].ToString());
                            Logger.LogInfo(method.Body.Instructions[instructionIdx_icarus3].ToString());

                            Logger.LogError("ERROR: The Dyson Sphere Program appears to have been updated.  The Infinite Resource Nodes mod needs to be updated.  Until then Icarus will consume node resources.");
                        }
                        else
                        {
                            ILProcessor ilProcessor = method.Body.GetILProcessor();

                            // Handles the node amount
                            // veinPool[num13].amount = veinPool[num13].amount - num12;
                            // 444: IL_052c: ldfld System.Int32 VeinData::amount
                            // 445: IL_0531: ldloc.s V_24
                            // 446: IL_0533: sub
                            // 447: IL_0534: stfld System.Int32 VeinData::amount
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_icarus1], Instruction.Create(OpCodes.Pop));

                            // Handles the planet's total
                            // factory.planet.veinAmounts[(int)veinData.type] -= (long)num12;
                            // 489: IL_05c6: ldloc.1
                            // 490: IL_05c7: callvirt PlanetData PlanetFactory::get_planet()
                            // 491: IL_05cc: ldfld System.Int64[] PlanetData::veinAmounts
                            // 492: IL_05d1: ldloca.s V_22
                            // 493: IL_05d3: ldfld EVeinType VeinData::type
                            // 494: IL_05d8: ldelema System.Int64
                            // 495: IL_05dd: dup
                            // 496: IL_05de: ldind.i8
                            // 497: IL_05df: ldloc.s V_24
                            // 498: IL_05e1: conv.i8
                            // 499: IL_05e2: sub
                            // 500: IL_05e3: stind.i8
                            // 501: IL_05e4: ldloc.1
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_icarus2], Instruction.Create(OpCodes.Pop));

                            // Handles the total in the node group
                            // veinGroups[(int)groupIndex].amount = veinGroups[(int)groupIndex].amount - (long)num12;
                            // 508: IL_05fc: ldfld System.Int64 PlanetData/VeinGroup::amount
                            // 509: IL_0601: ldloc.s V_24
                            // 510: IL_0603: conv.i8
                            // 511: IL_0604: sub
                            // 512: IL_0605: stfld System.Int64 PlanetData/VeinGroup::amount
                            ilProcessor.Replace(ilProcessor.Body.Instructions[instructionIdx_icarus3], Instruction.Create(OpCodes.Pop));

                            Logger.LogInfo("Infinite resources patch applied for Icarus.");
                        }
                        break;
                    }
                }
            }
        }
    }
}
