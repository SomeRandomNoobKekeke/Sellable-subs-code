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
    public static bool SubmarineSelection_ShowBuyPrompt_Replace(bool purchaseOnly, SubmarineSelection __instance)
    {
      SubmarineSelection _ = __instance;

      int price = _.selectedSubmarine.GetPrice();

      if (!GameMain.GameSession.Campaign.CanAfford(price))
      {
        new GUIMessageBox(TextManager.Get("purchasesubmarineheader"), TextManager.GetWithVariables("notenoughmoneyforpurchasetext",
            ("[currencyname]", _.currencyName),
            ("[submarinename]", _.selectedSubmarine.DisplayName)));
        return false;
      }

      GUIMessageBox msgBox;
      if (!purchaseOnly)
      {

        var sellCurrent = isCurSub("tosell") ? " + (" + TextManager.Get("campaignstoretab.sell") + " " + (Submarine.MainSub.Info.DisplayName) + ")" : "";

        var text = TextManager.GetWithVariables("purchaseandswitchsubmarinetext",
            ("[submarinename1]", _.selectedSubmarine.DisplayName + sellCurrent),
            ("[amount]", price.ToString()),
            ("[currencyname]", _.currencyName),
            ("[submarinename2]", SubmarineSelection.CurrentOrPendingSubmarine().DisplayName));
        text += _.GetItemTransferText();
        msgBox = new GUIMessageBox(TextManager.Get("purchaseandswitchsubmarineheader"), text, _.messageBoxOptions);

        msgBox.Buttons[0].OnClicked = (applyButton, obj) =>
        {
          if (!_.TransferItemsOnSwitch && !_.IsSelectedSubCurrentSub)
          {
            if (_.selectedSubmarine.NoItems)
            {
              ShowConfirmationPopup(TextManager.Get("noitemsheader"), TextManager.Get("noitemswarning"));
              return false;
            }
            if (!GameMain.GameSession.IsSubmarineOwned(_.selectedSubmarine) && !_.selectedSubmarine.IsManuallyOutfitted)
            {
              var (header, body) = _.GetItemTransferWarningText();
              ShowConfirmationPopup(header, body);
              return false;
            }
            if (_.selectedSubmarine.LowFuel)
            {
              ShowConfirmationPopup(TextManager.Get("lowfuelheader"), TextManager.Get("lowfuelwarning"));
              return false;
            }
          }
          return Confirm();
        };

        void ShowConfirmationPopup(LocalizedString header, LocalizedString textBody)
        {
          msgBox.Close();
          var extraConfirmationBox = new GUIMessageBox(header, textBody, new LocalizedString[2] { TextManager.Get("ok"), TextManager.Get("cancel") });
          extraConfirmationBox.Buttons[0].OnClicked = (b, o) => Confirm();
          extraConfirmationBox.Buttons[0].OnClicked += extraConfirmationBox.Close;
          extraConfirmationBox.Buttons[1].OnClicked += extraConfirmationBox.Close;
        }

        bool Confirm()
        {
          if (GameMain.Client == null)
          {
            if (GameMain.GameSession.TryPurchaseSubmarine(_.selectedSubmarine))
            {
              GameMain.GameSession.SwitchSubmarine(_.selectedSubmarine, _.TransferItemsOnSwitch);
            }
            _.RefreshSubmarineDisplay(true);
          }
          else
          {
            GameMain.Client.InitiateSubmarineChange(_.selectedSubmarine, _.TransferItemsOnSwitch, Barotrauma.Networking.VoteType.PurchaseAndSwitchSub);
          }
          return true;
        }
      }
      else
      {
        msgBox = new GUIMessageBox(TextManager.Get("purchasesubmarineheader"), TextManager.GetWithVariables("purchasesubmarinetext",
            ("[submarinename]", _.selectedSubmarine.DisplayName),
            ("[amount]", price.ToString()),
            ("[currencyname]", _.currencyName)) + '\n' + TextManager.Get("submarineswitchinstruction"), _.messageBoxOptions);

        msgBox.Buttons[0].OnClicked = (applyButton, obj) =>
        {
          if (GameMain.Client == null)
          {
            GameMain.GameSession.TryPurchaseSubmarine(_.selectedSubmarine);
            _.RefreshSubmarineDisplay(true);
          }
          else
          {
            GameMain.Client.InitiateSubmarineChange(_.selectedSubmarine, false, Barotrauma.Networking.VoteType.PurchaseSub);
          }
          return true;
        };
      }

      msgBox.Buttons[0].ClickSound = GUISoundType.ConfirmTransaction;
      msgBox.Buttons[0].OnClicked += msgBox.Close;
      msgBox.Buttons[1].OnClicked = msgBox.Close;

      return false;
    }

  }
}