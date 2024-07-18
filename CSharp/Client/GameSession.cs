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
    public static void GameSession_TryPurchaseSubmarine_Postfix(SubmarineInfo newSubmarine, ref bool __result)
    {
      if (!__result) return;

      if (isCurSubToSell()) markCurSubAsSold();
    }

    public static void clearSoldStates()
    {
      mainSubSold = null;
      mainSubToSell = null;

      if (screens != null)
      {
        screens.Clear();
        screens = null;
      }

      if (mixins != null)
      {
        mixins.Clear();
        mixins = null;
      }
    }
  }
}