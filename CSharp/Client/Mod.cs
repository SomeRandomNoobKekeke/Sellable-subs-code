using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]



namespace SellableSubs
{
  public partial class Mod : IAssemblyPlugin
  {
    public Harmony harmony;

    public static bool showAllSubs = false;
    public static bool debug = false;

    public static float sellMult = 0.9f;

    public static List<SubmarineSelection> screens = new List<SubmarineSelection>();

    public void Initialize()
    {
      harmony = new Harmony("sellable.subs");

      patchAll();

      if (debug && GameMain.GameSession?.Campaign?.CampaignUI?.submarineSelection != null)
      {
        GameMain.GameSession.Campaign.CampaignUI.submarineSelection = null;
      }


      if (debug) log("Compiled");
    }

    public static void SubmarineSelection_Constructor_Postfix(SubmarineSelection __instance)
    {
      if (screens == null) screens = new List<SubmarineSelection>();
      screens.Add(__instance);
    }

    public void patchAll()
    {
      harmony.Patch(
        original: typeof(SubmarineSelection).GetMethod("CreateGUI", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("SubmarineSelection_CreateGUI_Replace"))
      );

      harmony.Patch(
        original: typeof(SubmarineSelection).GetMethod("UpdateSubmarines", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("SubmarineSelection_UpdateSubmarines_Replace"))
      );

      harmony.Patch(
        original: typeof(SubmarineSelection).GetMethod("RefreshSubmarineDisplay", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("SubmarineSelection_RefreshSubmarineDisplay_Replace"))
      );

      harmony.Patch(
        original: typeof(SubmarineSelection).GetMethod("SelectSubmarine", AccessTools.all, new Type[]{
          typeof(SubmarineInfo),
          typeof(Rectangle),
        }),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("SubmarineSelection_SelectSubmarine_Replace"))
      );

      harmony.Patch(
        original: typeof(SubmarineSelection).GetConstructors()[0],
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("SubmarineSelection_Constructor_Postfix"))
      );

      harmony.Patch(
        original: typeof(SubmarineInfo).GetMethod("GetPrice", AccessTools.all),
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("substractMainSubPrice"))
      );

      harmony.Patch(
        original: typeof(CampaignMode).GetMethod("SwitchSubs", AccessTools.all),
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("CampaignMode_SwitchSubs_Postfix"))
      );

      harmony.Patch(
        original: typeof(GameSession).GetMethod("TryPurchaseSubmarine", AccessTools.all),
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("GameSession_TryPurchaseSubmarine_Postfix"))
      );

      harmony.Patch(
        original: typeof(GameSession).GetMethod("StartRound", AccessTools.all, new Type[]{
          typeof(LevelData),
          typeof(bool),
          typeof(SubmarineInfo),
          typeof(SubmarineInfo),
        }),
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("clearSoldStates"))
      );
    }


    public static void log(object msg, Color? cl = null, [CallerLineNumber] int lineNumber = 0)
    {
      if (cl == null) cl = Color.Cyan;
      DebugConsole.NewMessage($"{lineNumber}| {msg ?? "null"}", cl);
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {
      harmony.UnpatchAll(harmony.Id);
      harmony = null;

      if (screens != null)
      {
        screens.ForEach(s =>
        {
          if (s.closeAction != null) s.closeAction();
          s.createdForResolution = new Point(0, 0);
        });

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