using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.IO;

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

    public void Initialize()
    {
      harmony = new Harmony("sellable.subs");

      patchShared();

#if CLIENT
      InitializeClient();
#elif SERVER
      InitializeServer();
#endif

      info("Compiled");
    }

    public void patchShared()
    {
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
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("clearStates"))
      );
    }


    public static void log(object msg, Color? cl = null, string line = "")
    {
      if (cl == null) cl = Color.Cyan;
#if SERVER
      cl *= 0.8f;
#endif
      LuaCsLogger.LogMessage($"{line}{msg ?? "null"}", cl, cl);
    }
    public static void info(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      if (debug)
      {
        var fi = new FileInfo(source);

        log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Cyan * 0.5f);
        log(msg, Color.Cyan);
      }
    }
    public static void err(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      if (debug)
      {
        var fi = new FileInfo(source);

        log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Orange * 0.5f);
        log(msg, Color.Orange);
      }
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void Dispose()
    {
      harmony.UnpatchAll(harmony.Id);
      harmony = null;

#if CLIENT
      DisposeClient();
#endif
    }
  }
}
