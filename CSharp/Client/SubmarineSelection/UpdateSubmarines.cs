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
    public static bool SubmarineSelection_UpdateSubmarines_Replace(SubmarineSelection __instance)
    {
      SubmarineSelection _ = __instance;

      _.subsToShow.Clear();
      if (_.transferService)
      {
        _.subsToShow.AddRange(GameMain.GameSession.OwnedSubmarines);
        _.subsToShow.Sort(ComparePrice);
        string currentSubName = SubmarineSelection.CurrentOrPendingSubmarine().Name;
        int currentIndex = _.subsToShow.FindIndex(s => s.Name == currentSubName);
        if (currentIndex != -1)
        {
          _.currentPage = (int)Math.Ceiling((currentIndex + 1) / (float)SubmarineSelection.submarinesPerPage);
        }
      }
      if (_.purchaseService)
      {
        _.subsToShow.AddRange((GameMain.Client is null ? SubmarineInfo.SavedSubmarines : MultiPlayerCampaign.GetCampaignSubs()).Where(s => s.IsCampaignCompatible && !GameMain.GameSession.OwnedSubmarines.Any(os => os.Name == s.Name)));

        if (GameMain.GameSession.Campaign?.Map?.CurrentLocation is Location currentLocation && !showAllSubs)
        {
          _.subsToShow.RemoveAll(sub => !currentLocation.IsSubmarineAvailable(sub));
        }
        _.subsToShow.AddRange(GameMain.GameSession.OwnedSubmarines);

        _.subsToShow.Sort(ComparePrice);

        // string currentSubName = SubmarineSelection.CurrentOrPendingSubmarine().Name;
        // int currentIndex = _.subsToShow.FindIndex(s => s.Name == currentSubName);
        // if (currentIndex != -1)
        // {
        //   _.currentPage = (int)Math.Ceiling((currentIndex + 1) / (float)SubmarineSelection.submarinesPerPage);
        // }
      }

      _.SetConfirmButtonState(_.selectedSubmarine != null && _.selectedSubmarine.Name != SubmarineSelection.CurrentOrPendingSubmarine().Name);

      _.pageCount = Math.Max(1, (int)Math.Ceiling(_.subsToShow.Count / (float)SubmarineSelection.submarinesPerPage));
      _.UpdatePaging();
      SubmarineSelection.ContentRefreshRequired = false;

      static int ComparePrice(SubmarineInfo x, SubmarineInfo y)
      {
        return x.Price.CompareTo(y.Price) * 100 + x.Name.CompareTo(y.Name);
      }

      return false;
    }

  }
}