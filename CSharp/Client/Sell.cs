using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using PlayerBalanceElement = Barotrauma.CampaignUI.PlayerBalanceElement;



namespace SellableSubs
{
  public partial class Mod : IAssemblyPlugin
  {
    public static bool? mainSubSold = null;
    public static bool? mainSubToSell = null;
    public static int totalRepairCost = 0;

    public static void updateRepairCost()
    {
      if (!(GameMain.GameSession?.GameMode is CampaignMode campaign)) { return; }

      Location location = campaign.Map.CurrentLocation;

      int hullRepairCost = CampaignMode.GetHullRepairCost();
      int itemRepairCost = CampaignMode.GetItemRepairCost();
      //int shuttleRetrieveCost = CampaignMode.ShuttleReplaceCost;

      hullRepairCost = location.GetAdjustedMechanicalCost(hullRepairCost);
      itemRepairCost = location.GetAdjustedMechanicalCost(itemRepairCost);
      //shuttleRetrieveCost = location.GetAdjustedMechanicalCost(shuttleRetrieveCost);

      totalRepairCost = hullRepairCost + itemRepairCost; //+ shuttleRetrieveCost;
    }

    public static bool markCurSubAsSold(bool state = true)
    {
      if (Submarine.MainSub == null) return false;

      foreach (var i in Submarine.MainSub.GetItems(false))
      {
        if (i.HasTag("dock"))
        {
          if (state) i.AddTag("sold");
          else i.RemoveTag("sold");
        }
      }

      mainSubSold = state;

      return state; // why do i need to do this???
    }

    public static bool markCurSubAsToSell(bool state)
    {
      if (Submarine.MainSub == null) return false;

      foreach (var i in Submarine.MainSub.GetItems(false))
      {
        if (i.HasTag("dock"))
        {
          if (state) i.AddTag("tosell");
          else i.RemoveTag("tosell");
        }
      }

      mainSubToSell = state;

      return state; // why do i need to do this???
    }

    public static bool isCurSubToSell()
    {
      if (Submarine.MainSub == null) return false;
      if (mainSubToSell == null) mainSubToSell = Submarine.MainSub.GetItems(false).Any(i => i.HasTag("tosell"));
      return mainSubToSell ?? false;
    }

    public static bool isCurSubSold()
    {
      if (Submarine.MainSub == null) return false;
      if (mainSubSold == null) mainSubSold = Submarine.MainSub.GetItems(false).Any(i => i.HasTag("sold"));
      return mainSubSold ?? false;
    }

    // this is selling of owned not selected sub
    public static void sellOwnedSub(SubmarineInfo sub)
    {
      if (!(GameMain.GameSession?.GameMode is CampaignMode campaign)) { return; }

      int price = sub.GetPrice();
      Wallet wallet = campaign.Bank;
      wallet.Give(price);

      GameMain.GameSession.OwnedSubmarines.RemoveAll(s => s.Name == sub.Name);
    }
  }
}