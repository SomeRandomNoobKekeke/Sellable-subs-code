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
    // this is switch on saveload
    public static void CampaignMode_SwitchSubs_Postfix()
    {
      if (Submarine.MainSub == null) return;

      if (isCurSubSold()) GameMain.GameSession.OwnedSubmarines.RemoveAll(s => s.Name == Submarine.MainSub.Info.Name);
    }
  }
}