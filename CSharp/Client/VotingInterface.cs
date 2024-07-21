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

namespace SellableSubs
{
  public partial class Mod : IAssemblyPlugin
  {
    public static bool VotingInterface_SetSubmarineVotingText_Replace(Client starter, SubmarineInfo info, bool transferItems, VoteType type, VotingInterface __instance)
    {
      VotingInterface _ = __instance;

      int price = info.GetPrice();
      string name = starter.Name;
      JobPrefab prefab = starter?.Character?.Info?.Job?.Prefab;
      Color nameColor = prefab != null ? prefab.UIColor : Color.White;
      string characterRichString = $"‖color:{nameColor.R},{nameColor.G},{nameColor.B}‖{name}‖color:end‖";
      string submarineRichString = $"‖color:{VotingInterface.SubmarineColor.R},{VotingInterface.SubmarineColor.G},{VotingInterface.SubmarineColor.B}‖{info.DisplayName}‖color:end‖";
      string tag = string.Empty;
      LocalizedString text = string.Empty;
      switch (type)
      {
        case VoteType.PurchaseAndSwitchSub:
          tag = transferItems ? "submarinepurchaseandswitchwithitemsvote" : "submarinepurchaseandswitchvote";
          var sellCurrent = isCurSub("tosell") ? " + (" + TextManager.Get("campaignstoretab.sell") + " " + (Submarine.MainSub.Info.DisplayName) + ")" : "";

          text = TextManager.GetWithVariables(tag,
              ("[playername]", characterRichString),
              ("[submarinename]", submarineRichString + sellCurrent),
              ("[amount]", price.ToString()),
              ("[currencyname]", TextManager.Get("credit").ToLower()));
          break;
        case VoteType.PurchaseSub:
          text = TextManager.GetWithVariables("submarinepurchasevote",
              ("[playername]", characterRichString),
              ("[submarinename]", submarineRichString),
              ("[amount]", price.ToString()),
              ("[currencyname]", TextManager.Get("credit").ToLower()));
          break;
        case VoteType.SwitchSub:
          tag = transferItems ? "submarineswitchwithitemsnofeevote" : "submarineswitchnofeevote";
          text = TextManager.GetWithVariables(tag,
              ("[playername]", characterRichString),
              ("[submarinename]", submarineRichString));
          break;
      }
      _.votingOnText = RichString.Rich(text);

      return false;
    }

  }
}