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

      if (isCurSub("tosell")) markCurSubAs("sold");
    }

    public static void clearStates()
    {
      mainSubStateCache.Clear();
      totalRepairCost = 0;

      info($"round start, tosell = " + isCurSub("tosell").ToString() + " sold = " + isCurSub("sold").ToString());

#if CLIENT
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

      if (GameMain.GameSession?.Campaign?.CampaignUI?.submarineSelection != null)
      {
        GameMain.GameSession.Campaign.CampaignUI.submarineSelection = null;
      }
#endif
    }
  }
}