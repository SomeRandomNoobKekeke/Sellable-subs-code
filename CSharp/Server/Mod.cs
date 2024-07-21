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

    public void InitializeServer()
    {
      GameMain.LuaCs.Networking.Receive("sellsub", (object[] args) =>
      {
        IReadMessage msg = args[0] as IReadMessage;
        Client client = args[1] as Client;

        string subName = msg.ReadString();
        SubmarineInfo subInfo = GameMain.GameSession.OwnedSubmarines.FirstOrDefault(s => s.Name == subName) ?? SubmarineInfo.SavedSubmarines.FirstOrDefault(s => s.Name == subName);

        sellOwnedSub(subInfo);

        IWriteMessage message = GameMain.LuaCs.Networking.Start("sellsub");
        message.WriteString(subName);
        GameMain.LuaCs.Networking.Send(message);
      });

      GameMain.LuaCs.Networking.Receive("updatetosell", (object[] args) =>
      {
        IReadMessage msg = args[0] as IReadMessage;
        Client client = args[1] as Client;

        bool state = msg.ReadBoolean();

        info($"updatetosell {state}");

        markCurSubAs("tosell", state);

        IWriteMessage message = GameMain.LuaCs.Networking.Start("updatetosell");
        message.WriteBoolean(state);
        GameMain.LuaCs.Networking.Send(message);
      });
    }


    public void DisposeServer()
    {

    }
  }
}