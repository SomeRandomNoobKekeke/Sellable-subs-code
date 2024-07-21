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
    public static bool SubmarineSelection_RefreshSubmarineDisplay_Replace(bool updateSubs, bool setTransferOptionToTrue, SubmarineSelection __instance)
    {
      SubmarineSelection _ = __instance;

      info($"tosell: " + isCurSub("tosell") + " sold: " + isCurSub("sold"));

      updateRepairCost();

      if (!_.initialized)
      {
        _.Initialize();
      }
      if (GameMain.GraphicsWidth != _.createdForResolution.X || GameMain.GraphicsHeight != _.createdForResolution.Y)
      {
        _.CreateGUI();
      }
      else
      {
        _.playerBalanceElement = CampaignUI.UpdateBalanceElement(_.playerBalanceElement);
      }
      if (setTransferOptionToTrue)
      {
        _.TransferItemsOnSwitch = true;
      }
      if (updateSubs)
      {
        _.UpdateSubmarines();
      }

      if (_.pageIndicators != null)
      {
        for (int i = 0; i < _.pageIndicators.Length; i++)
        {
          _.pageIndicators[i].Color = i == _.currentPage - 1 ? Color.White : Color.Gray;
        }
      }

      int submarineIndex = (_.currentPage - 1) * SubmarineSelection.submarinesPerPage;

      for (int i = 0; i < _.submarineDisplays.Length; i++)
      {
        SubmarineInfo subToDisplay = _.GetSubToDisplay(submarineIndex);
        if (subToDisplay == null)
        {
          _.submarineDisplays[i].submarineImage.Sprite = null;
          _.submarineDisplays[i].submarineName.Text = string.Empty;
          _.submarineDisplays[i].submarineFee.Text = string.Empty;
          _.submarineDisplays[i].submarineClass.Text = string.Empty;
          _.submarineDisplays[i].submarineTier.Text = string.Empty;
          _.submarineDisplays[i].selectSubmarineButton.Enabled = false;
          _.submarineDisplays[i].selectSubmarineButton.OnClicked = null;
          _.submarineDisplays[i].displayedSubmarine = null;
          _.submarineDisplays[i].middleTextBlock.AutoDraw = false;
          _.submarineDisplays[i].previewButton.Visible = false;
        }
        else
        {
          _.submarineDisplays[i].displayedSubmarine = subToDisplay;
          Sprite previewImage = _.GetPreviewImage(subToDisplay);

          if (previewImage != null)
          {
            _.submarineDisplays[i].submarineImage.Sprite = previewImage;
            _.submarineDisplays[i].middleTextBlock.AutoDraw = false;
          }
          else
          {
            _.submarineDisplays[i].submarineImage.Sprite = null;
            _.submarineDisplays[i].middleTextBlock.Text = _.missingPreviewText;
            _.submarineDisplays[i].middleTextBlock.AutoDraw = true;
          }

          _.submarineDisplays[i].selectSubmarineButton.Enabled = true;

          int index = i;
          _.submarineDisplays[i].selectSubmarineButton.OnClicked = (button, userData) =>
          {
            _.SelectSubmarine(subToDisplay, _.submarineDisplays[index].background.Rect);
            return true;
          };

          _.submarineDisplays[i].submarineName.Text = subToDisplay.DisplayName;

          _.submarineDisplays[i].submarineClass.Text = TextManager.GetWithVariable("submarineclass.classsuffixformat", "[type]", TextManager.Get($"submarineclass.{subToDisplay.SubmarineClass}"));
          _.submarineDisplays[i].submarineClass.ToolTip = TextManager.Get("submarineclass.description") + "\n\n" + TextManager.Get($"submarineclass.{subToDisplay.SubmarineClass}.description");

          _.submarineDisplays[i].submarineTier.Text = TextManager.Get($"submarinetier.{subToDisplay.Tier}");
          _.submarineDisplays[i].submarineTier.ToolTip = TextManager.Get("submarinetier.description");

          if (!GameMain.GameSession.IsSubmarineOwned(subToDisplay))
          {
            _.submarineDisplays[i].submarineFee.Text = "";
            _.submarineDisplays[i].submarineFee.TextColor = Color.White;
          }
          else
          {
            if (subToDisplay.Name != SubmarineSelection.CurrentOrPendingSubmarine().Name)
            {
              if (subToDisplay.Name == Submarine.MainSub.Info.Name && isCurSub("sold"))
              {
                _.submarineDisplays[i].submarineFee.Text = "Sold\n";
                _.submarineDisplays[i].submarineFee.TextColor = Color.DarkRed;
              }
              else
              {
                _.submarineDisplays[i].submarineFee.Text = TextManager.Get("campaignstore.ownedspecific").Replace(": [nonempty] ([total])", "") + "\n";
                _.submarineDisplays[i].submarineFee.TextColor = Color.Goldenrod;
              }

            }
            else
            {
              _.submarineDisplays[i].submarineFee.Text = _.selectedSubText + "\n";
              _.submarineDisplays[i].submarineFee.TextColor = Color.Gold;
            }
          }

          LocalizedString amountString = TextManager.FormatCurrency(subToDisplay.GetPrice());
          _.submarineDisplays[i].submarineFee.Text += TextManager.GetWithVariable("price", "[amount]", amountString);

          if (_.transferService && subToDisplay.Name == SubmarineSelection.CurrentOrPendingSubmarine().Name && updateSubs)
          {
            if (_.selectedSubmarine == null)
            {
              CoroutineManager.StartCoroutine(_.SelectOwnSubmarineWithDelay(subToDisplay, _.submarineDisplays[i]));
            }
            else
            {
              _.SelectSubmarine(subToDisplay, _.submarineDisplays[i].background.Rect);
            }
          }
          else if (!_.transferService && _.selectedSubmarine == null || !_.transferService && GameMain.GameSession.IsSubmarineOwned(_.selectedSubmarine) || subToDisplay == _.selectedSubmarine)
          {
            _.SelectSubmarine(subToDisplay, _.submarineDisplays[i].background.Rect);
          }

          _.submarineDisplays[i].previewButton.Visible = true;
          _.submarineDisplays[i].previewButton.OnClicked = (btn, obj) =>
          {
            SubmarinePreview.Create(subToDisplay);
            return false;
          };
        }

        submarineIndex++;
      }

      if (_.subsToShow.Count == 0)
      {
        _.SelectSubmarine(null, Rectangle.Empty);
      }
      // else
      // {
      //   _.UpdateItemTransferInfoFrame();
      // }

      SyncState(_);

      return false;
    }

  }
}