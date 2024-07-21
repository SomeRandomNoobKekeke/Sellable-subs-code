using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace SellableSubs
{
  public partial class Mod : IAssemblyPlugin
  {
    public static void substractMainSubPrice(SubmarineInfo __instance, ref int __result)
    {
      if (Submarine.MainSub == null) return;

      if (GameMain.GameSession.IsSubmarineOwned(__instance))
      {
        __result = (int)Math.Floor(__result * sellMult);
      }

      if (__instance.Name == Submarine.MainSub.Info.Name)
      {
        __result -= totalRepairCost;
      }

      if (!GameMain.GameSession.IsSubmarineOwned(__instance) && isCurSub("tosell") && !isCurSub("sold") && __instance.Name != Submarine.MainSub.Info.Name)
      {
        __result -= Submarine.MainSub.Info.GetPrice();
      }
    }
  }
}