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
    public static void SyncState(SubmarineSelection _)
    {
      int linenum = 0;
      try
      {

        mixins ??= new Dictionary<SubmarineSelection, SubmarineSelectionMixin>();

        bool owned = false;

        if (_.selectedSubmarine != null) owned = GameMain.GameSession.IsSubmarineOwned(_.selectedSubmarine);

        bool isTargetSub = false;
        if (_.selectedSubmarine != null && SubmarineSelection.CurrentOrPendingSubmarine() != null)
        {
          isTargetSub = _.selectedSubmarine.Name == SubmarineSelection.CurrentOrPendingSubmarine().Name;
        }

        // SubmarineInfo currentSub = SubmarineSelection.CurrentOrPendingSubmarine();

        if (mixins.ContainsKey(_))
        {
          mixins[_].sellCurrentTickBox.hideIf(isCurSubSold() || owned, 0.15f);
          mixins[_].sellCurrentTickBox.Text = TextManager.Get("campaignstoretab.sell") + " " + (Submarine.MainSub.Info.DisplayName);

          mixins[_].sellCurrentTickBox.RectTransform.Resize(
            new Point(
              (int)mixins[_].sellCurrentTickBox.ContentWidth,
              mixins[_].sellCurrentTickBox.Rect.Height
            )
          );

          mixins[_].sellButton.revealIf(owned && !(_.IsSelectedSubCurrentSub || isTargetSub), 0.15f);
        }

        if (_.purchaseService)
        {
          _.transferItemsTickBox.hideIf(_.IsSelectedSubCurrentSub, 0.2f);
          _.confirmButtonAlt.hideIf(owned, 0.1f);
          _.confirmButton.hideIf(_.IsSelectedSubCurrentSub && isCurSubSold(), 0.25f);
        }

        if (_.transferService)
        {
          _.transferItemsTickBox.hideIf(_.IsSelectedSubCurrentSub, 0.2f);
          _.confirmButton.hideIf(_.IsSelectedSubCurrentSub && isCurSubSold(), 0.25f);
        }


        if (_.selectedSubmarine != null)
        {
          if (_.IsSelectedSubCurrentSub)
          {
            _.TransferItemsOnSwitch = false;
            _.transferItemsTickBox.hide();
            _.itemTransferInfoBlock.revealIf(_.confirmButton.Enabled && !isCurSubSold(), 0.6f);
            _.itemTransferInfoBlock.Text = TextManager.Get("switchingbacktocurrentsub");
          }
          else if (GameMain.GameSession?.Campaign?.PendingSubmarineSwitch?.Name == _.selectedSubmarine.Name)
          {
            _.transferItemsTickBox.hide();
            _.itemTransferInfoBlock.reveal(0.6f);
            _.itemTransferInfoBlock.Text = GameMain.GameSession.Campaign.TransferItemsOnSubSwitch ? TextManager.Get("itemtransferenabledreminder") : TextManager.Get("itemtransferdisabledreminder");
          }
          else
          {
            _.transferItemsTickBox.Selected = true; //_.TransferItemsOnSwitch;
            _.transferItemsTickBox.reveal(0.2f);
            _.itemTransferInfoBlock.hide();
          }
        }
        else
        {
          _.transferItemsTickBox.hide();
          _.itemTransferInfoBlock.hide();
        }

        if (mixins != null && mixins.ContainsKey(_))
        {
          mixins[_].bottomContainer?.recalc();
        }

      }
      catch (Exception e) { log(e, Color.Orange); }
    }
  }
}