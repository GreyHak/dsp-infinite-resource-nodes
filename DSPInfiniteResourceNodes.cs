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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace DSPInfiniteResourceNodes
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInProcess("DSPGAME.exe")]
    public class DSPInfiniteResourceNodes : BaseUnityPlugin
    {
        public const string pluginGuid = "greyhak.dysonsphereprogram.infiniteresourcenodes";
        public const string pluginName = "DSP Infinite Resource Nodes";
        public const string pluginVersion = "1.0.0";
        new internal static ManualLogSource Logger;
        Harmony harmony;

        public void Awake()
        {
            Logger = base.Logger;  // "C:\Program Files (x86)\Steam\steamapps\common\Dyson Sphere Program\BepInEx\LogOutput.log"

            harmony = new Harmony(pluginGuid);
            harmony.PatchAll(typeof(DSPInfiniteResourceNodes));

            Logger.LogInfo("Initialization complete.");
        }
    }
}
