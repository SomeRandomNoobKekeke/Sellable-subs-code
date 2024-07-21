using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;

[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]



namespace SellableSubs
{
  public partial class Mod : IAssemblyPlugin
  {

    public static List<SubmarineSelection> screens = new List<SubmarineSelection>();

    public void InitializeClient()
    {
      if (GameMain.GameSession?.Campaign?.CampaignUI?.submarineSelection != null)
      {
        GameMain.GameSession.Campaign.CampaignUI.submarineSelection = null;
      }

      patchClient();

      GameMain.LuaCs.Networking.Receive("sellsub", (object[] args) =>
      {
        IReadMessage msg = args[0] as IReadMessage;

        string subName = msg.ReadString();
        SubmarineInfo subInfo = GameMain.GameSession.OwnedSubmarines.FirstOrDefault(s => s.Name == subName) ?? SubmarineInfo.SavedSubmarines.FirstOrDefault(s => s.Name == subName);

        sellOwnedSub(subInfo);
        screens?.ForEach(s => s.RefreshSubmarineDisplay(true));
      });

      GameMain.LuaCs.Networking.Receive("updatetosell", (object[] args) =>
      {
        IReadMessage msg = args[0] as IReadMessage;

        bool state = msg.ReadBoolean();

        markCurSubAs("tosell", state);

        mixins ??= new Dictionary<SubmarineSelection, SubmarineSelectionMixin>();
        foreach (var m in mixins)
        {
          if (m.Value.sellCurrentTickBox != null)
          {
            m.Value.sellCurrentTickBox.Selected = state;
          }
        }

        screens?.ForEach(s => s.RefreshSubmarineDisplay(true));

        info($"updatetosell {state}");
      });
    }

    public static void SubmarineSelection_Constructor_Postfix(SubmarineSelection __instance)
    {
      if (screens == null) screens = new List<SubmarineSelection>();
      screens.Add(__instance);
    }

    public void patchClient()
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
        original: typeof(SubmarineSelection).GetMethod("ShowBuyPrompt", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("SubmarineSelection_ShowBuyPrompt_Replace"))
      );

      harmony.Patch(
        original: typeof(VotingInterface).GetMethod("SetSubmarineVotingText", AccessTools.all),
        prefix: new HarmonyMethod(typeof(Mod).GetMethod("VotingInterface_SetSubmarineVotingText_Replace"))
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
    }

    public void DisposeClient()
    {
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