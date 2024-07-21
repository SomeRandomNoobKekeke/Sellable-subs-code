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
    public static bool SubmarineSelection_SelectSubmarine_Replace(SubmarineInfo info, Rectangle backgroundRect, SubmarineSelection __instance)
    {
      SubmarineSelection _ = __instance;

      if (debug) log($"tosell: " + isCurSub("tosell") + " sold: " + isCurSub("sold"));

#if !DEBUG
      //if (_.selectedSubmarine == info) return false;
#endif
      _.specsFrame.Content.ClearChildren();
      _.selectedSubmarine = info;

      if (info != null)
      {
        bool owned = GameMain.GameSession.IsSubmarineOwned(info);

        if (owned)
        {
          _.confirmButton.Text = _.switchText;
          _.confirmButton.OnClicked = (button, userData) =>
          {
            _.ShowTransferPrompt();
            return true;
          };

          if (mixins.ContainsKey(_))
          {
            mixins[_].sellButton.OnClicked = (button, userData) =>
            {
              if (GameMain.IsSingleplayer)
              {
                sellOwnedSub(_.selectedSubmarine);
                _.RefreshSubmarineDisplay(true);
              }

              if (GameMain.IsMultiplayer)
              {
                if (GameMain.Client.IsServerOwner || GameMain.Client.HasPermission(ClientPermissions.All))
                {
                  IWriteMessage message = GameMain.LuaCs.Networking.Start("sellsub");
                  message.WriteString(_.selectedSubmarine.Name);
                  GameMain.LuaCs.Networking.Send(message);
                }
                else
                {
                  new GUIMessageBox("Sry no, ask host to sell this", "Author is too lazy to implement voting, and he can't let everybody sell whatever they want, so only host or players with all permissions can sell other subs");
                }
              }

              return true;
            };
          }
        }
        else
        {
          _.confirmButton.Text = _.purchaseAndSwitchText;
          _.confirmButton.OnClicked = (button, userData) =>
          {
            _.ShowBuyPrompt(false);
            return true;
          };

          _.confirmButtonAlt.Text = _.purchaseOnlyText;
          _.confirmButtonAlt.OnClicked = (button, userData) =>
          {
            _.ShowBuyPrompt(true);
            return true;
          };
        }

        //_.SetConfirmButtonState();
        if (_.confirmButtonAlt != null)
        {
          _.confirmButtonAlt.Enabled = _.selectedSubmarine.Name != SubmarineSelection.CurrentOrPendingSubmarine().Name && !isCurSub("tosell");
        }

        if (_.confirmButton != null)
        {
          _.confirmButton.Enabled = _.selectedSubmarine.Name != SubmarineSelection.CurrentOrPendingSubmarine().Name;
        }

        _.selectedSubmarineIndicator.RectTransform.NonScaledSize = backgroundRect.Size;
        _.selectedSubmarineIndicator.RectTransform.AbsoluteOffset = new Point(backgroundRect.Left - _.submarineHorizontalGroup.Rect.Left, 0);

        Sprite previewImage = _.GetPreviewImage(info);
        _.listBackground.Sprite = previewImage;
        _.listBackground.SetCrop(true);

        GUIFont font = GUIStyle.Font;
        info.CreateSpecsWindow(_.specsFrame, font, includeCrushDepth: true);
        _.descriptionTextBlock.Text = info.Description;
        _.descriptionTextBlock.CalculateHeightFromText();
      }
      else
      {
        _.listBackground.Sprite = null;
        _.listBackground.SetCrop(false);
        _.descriptionTextBlock.Text = string.Empty;
        _.selectedSubmarineIndicator.RectTransform.NonScaledSize = Point.Zero;
        _.SetConfirmButtonState(false);
      }

      //_.UpdateItemTransferInfoFrame();

      SyncState(_);

      return false;
    }
  }
}