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
    public static bool SubmarineSelection_CreateGUI_Replace(SubmarineSelection __instance)
    {
      SubmarineSelection _ = __instance;

      mixins ??= new Dictionary<SubmarineSelection, SubmarineSelectionMixin>();
      if (!mixins.ContainsKey(_)) mixins[_] = new SubmarineSelectionMixin();


      _.createdForResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);

      GUILayoutGroup content;
      _.GuiFrame = new GUIFrame(
        new RectTransform(
          new Vector2(0.9f, 0.9f),
          _.parent,
          Anchor.TopCenter,
          Pivot.TopCenter
        )
        {
          RelativeOffset = new Vector2(0.0f, 0.02f)
        }
      );
      _.selectionIndicatorThickness = HUDLayoutSettings.Padding / 2;

      GUIFrame background = new GUIFrame(
        new RectTransform(
          _.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin,
          _.GuiFrame.RectTransform,
          Anchor.Center
        ),
        color: Color.Black * 0.9f
      )
      {
        CanBeFocused = false
      };

      content = new GUILayoutGroup(
        new RectTransform(
          new Point(
            background.Rect.Width - HUDLayoutSettings.Padding * 4,
            background.Rect.Height - HUDLayoutSettings.Padding * 4
          ),
          background.RectTransform, Anchor.Center
        )
      )
      {
        AbsoluteSpacing = (int)(HUDLayoutSettings.Padding * 1.5f)
      };

      GUITextBlock header = new GUITextBlock(
        new RectTransform(
          new Vector2(1f, 0.0f),
          content.RectTransform
        ),
        _.transferService ? TextManager.Get("switchsubmarineheader") : TextManager.GetWithVariable(
          "outpostshipyard",
          "[location]",
          GameMain.GameSession.Map.CurrentLocation.DisplayName
        ),
        font: GUIStyle.LargeFont
      );
      header.CalculateHeightFromText(0, true);
      _.playerBalanceElement = CampaignUI.AddBalanceElement(header, new Vector2(1.0f, 1.5f));

      new GUIFrame(
        new RectTransform(
          new Vector2(1.0f, 0.01f),
          content.RectTransform
        ),
        style: "HorizontalLine"
      );

      GUILayoutGroup submarineContentGroup = new GUILayoutGroup(
        new RectTransform(
          new Vector2(1f, 0.4f),
          content.RectTransform
        )
      )
      {
        AbsoluteSpacing = HUDLayoutSettings.Padding,
        Stretch = true
      };

      _.submarineHorizontalGroup = new GUILayoutGroup(
        new RectTransform(
          new Vector2(1f, 0.9f),
          submarineContentGroup.RectTransform
        )
      )
      {
        IsHorizontal = true,
        AbsoluteSpacing = HUDLayoutSettings.Padding,
        Stretch = true
      };

      _.submarineControlsGroup = new GUILayoutGroup(
        new RectTransform(
          new Vector2(1f, 0.1f),
          submarineContentGroup.RectTransform
        ),
        true,
        Anchor.TopCenter
      );

      GUILayoutGroup infoFrame = new GUILayoutGroup(
        new RectTransform(
          new Vector2(1f, 0.4f),
          content.RectTransform
        )
      )
      {
        IsHorizontal = true,
        Stretch = true,
        AbsoluteSpacing = HUDLayoutSettings.Padding
      };

      new GUIFrame(
        new RectTransform(
          Vector2.One,
          infoFrame.RectTransform
        ),
        style: null,
        new Color(8, 13, 19)
      )
      {
        IgnoreLayoutGroups = true
      };

      _.listBackground = new GUIImage(
        new RectTransform(
          new Vector2(0.59f, 1f),
          infoFrame.RectTransform,
          Anchor.CenterRight
        ),
        style: null,
        true
      )
      {
        IgnoreLayoutGroups = true
      };

      new GUIListBox(
        new RectTransform(
          Vector2.One,
          infoFrame.RectTransform
        )
      )
      {
        IgnoreLayoutGroups = true,
        CanBeFocused = false
      };

      _.specsFrame = new GUIListBox(new RectTransform(new Vector2(0.39f, 1f), infoFrame.RectTransform), style: null)
      {
        CurrentSelectMode = GUIListBox.SelectMode.None,
        Spacing = GUI.IntScale(5),
        Padding = new Vector4(HUDLayoutSettings.Padding / 2f, HUDLayoutSettings.Padding, 0, 0)
      };

      new GUIFrame(
        new RectTransform(
          new Vector2(0.02f, 0.8f),
          infoFrame.RectTransform
        )
        {
          RelativeOffset = new Vector2(0.0f, 0.1f)
        },
        style: "VerticalLine"
      );

      GUIListBox descriptionFrame = new GUIListBox(
        new RectTransform(
          new Vector2(0.59f, 1f),
          infoFrame.RectTransform
        ),
        style: null
      )
      {
        Padding = new Vector4(
          HUDLayoutSettings.Padding / 2f,
          HUDLayoutSettings.Padding * 1.5f,
          HUDLayoutSettings.Padding * 1.5f,
          HUDLayoutSettings.Padding / 2f
        )
      };

      _.descriptionTextBlock = new GUITextBlock(
        new RectTransform(
          new Vector2(1, 0),
          descriptionFrame.Content.RectTransform
        ),
        string.Empty,
        font: GUIStyle.Font,
        wrap: true
      )
      {
        CanBeFocused = false
      };

      GUILayoutGroup bottomContainer = new GUILayoutGroup(
        new RectTransform(
          new Vector2(1.0f, 0.075f),
          content.RectTransform,
          Anchor.CenterRight
        ),
        childAnchor: Anchor.CenterRight
      )
      {
        IsHorizontal = true,
        AbsoluteSpacing = HUDLayoutSettings.Padding
      };
      float transferInfoFrameWidth = 1.0f;

      mixins[_].bottomContainer = bottomContainer;

      if (_.closeAction != null)
      {
        GUIButton closeButton = new GUIButton(
          new RectTransform(
            new Vector2(0.1f, 1f),
            bottomContainer.RectTransform
          ),
          TextManager.Get("Close"),
          style: "GUIButtonFreeScale"
        )
        {
          OnClicked = (button, userData) =>
          {
            _.closeAction();
            return true;
          }
        };
        transferInfoFrameWidth -= closeButton.RectTransform.RelativeSize.X;
      }

      if (_.purchaseService)
      {
        _.confirmButtonAlt = new GUIButton(
          new RectTransform(
            new Vector2(0.1f, 1f),
            bottomContainer.RectTransform
          ),
          _.purchaseOnlyText,
          style: "GUIButtonFreeScale"
        );
        transferInfoFrameWidth -= _.confirmButtonAlt.RectTransform.RelativeSize.X;
      }

      _.confirmButton = new GUIButton(
        new RectTransform(
          new Vector2(0.25f, 1f),
          bottomContainer.RectTransform
        ),
        _.purchaseService ? _.purchaseAndSwitchText : _.switchText,
        style: "GUIButtonFreeScale"
      );

      _.SetConfirmButtonState(false);
      transferInfoFrameWidth -= _.confirmButton.RectTransform.RelativeSize.X;

      mixins[_].sellButton = new GUIButton(
        new RectTransform(
          new Vector2(0.15f, 1f),
          bottomContainer.RectTransform
        ),
        TextManager.Get("campaignstoretab.sell"),
        style: "GUIButtonFreeScale"
      )
      {
        ClickSound = GUISoundType.ConfirmTransaction
      };
      //transferInfoFrameWidth -= mixins[_].sellButton.RectTransform.RelativeSize.X;




      // GUIFrame transferInfoFrame = new GUIFrame(
      //   new RectTransform(
      //     new Vector2(transferInfoFrameWidth, 1.0f),
      //     bottomContainer.RectTransform
      //   ),
      //   style: null
      // )
      // {
      //   CanBeFocused = false
      // };



      _.transferItemsTickBox = new GUITickBox(
        new RectTransform(
          new Vector2(0.2f, 1.0f),
          bottomContainer.RectTransform,
          Anchor.CenterRight
        ),
        TextManager.Get("transferitems"),
        font: GUIStyle.SubHeadingFont
      )
      {
        Selected = true, //_.TransferItemsOnSwitch,
        // Visible = false,
        OnSelected = (tb) => _.transferItemsOnSwitch = tb.Selected
      };

      // _.transferItemsTickBox.RectTransform.Resize(
      //   new Point(
      //     Math.Min((int)_.transferItemsTickBox.ContentWidth, transferInfoFrame.Rect.Width),
      //     _.transferItemsTickBox.Rect.Height
      //   )
      // );

      mixins[_].sellCurrentTickBox = new GUITickBox(
        new RectTransform(
          new Vector2(0.15f, 1.0f),
          bottomContainer.RectTransform,
          Anchor.CenterRight
        ),
        TextManager.Get("campaignstoretab.sell") + " " + (_?.selectedSubmarine?.DisplayName ?? ""),
        font: GUIStyle.SubHeadingFont
      )
      {
        Selected = isCurSub("tosell"),
        OnSelected = (tb) =>
        {
          info("OnSelected");

          if (GameMain.IsSingleplayer)
          {
            markCurSubAs("tosell", tb.Selected);
            _.RefreshSubmarineDisplay(true);
          }

          if (GameMain.IsMultiplayer)
          {
            IWriteMessage message = GameMain.LuaCs.Networking.Start("updatetosell");
            message.WriteBoolean(tb.Selected);
            GameMain.LuaCs.Networking.Send(message);
          }

          return tb.Selected;
        }
      };
      //transferInfoFrameWidth -= mixins[_].sellCurrentTickBox.RectTransform.RelativeSize.X;

      _.itemTransferInfoBlock = new GUITextBlock(
        new RectTransform(
          new Vector2(0.6f, 1.0f),
          bottomContainer.RectTransform,
          Anchor.CenterRight
        ),
        null,
        textColor: Color.Red,
        wrap: true
      )
      {
        TextAlignment = Alignment.CenterRight,
        Visible = false
      };

      _.pageIndicatorHolder = new GUIFrame(
        new RectTransform(
          new Vector2(1f, 1.5f),
          _.submarineControlsGroup.RectTransform
        ),
        style: null
      );

      _.pageIndicator = GUIStyle.GetComponentStyle("GUIPageIndicator").GetDefaultSprite();
      _.UpdatePaging();

      for (int i = 0; i < _.submarineDisplays.Length; i++)
      {
        SubmarineSelection.SubmarineDisplayContent submarineDisplayElement = new SubmarineSelection.SubmarineDisplayContent
        {
          background = new GUIFrame(
            new RectTransform(
              new Vector2(1f / SubmarineSelection.submarinesPerPage, 1f),
              _.submarineHorizontalGroup.RectTransform
            ),
            style: null,
            new Color(8, 13, 19)
          )
        };

        submarineDisplayElement.submarineImage = new GUIImage(
          new RectTransform(
            new Vector2(0.8f, 1f),
            submarineDisplayElement.background.RectTransform,
            Anchor.Center
          ),
          null,
          true
        );

        submarineDisplayElement.middleTextBlock = new GUITextBlock(new RectTransform(new Vector2(0.8f, 1f), submarineDisplayElement.background.RectTransform, Anchor.Center), string.Empty, textAlignment: Alignment.Center);
        submarineDisplayElement.submarineName = new GUITextBlock(new RectTransform(new Vector2(1f, 0.1f), submarineDisplayElement.background.RectTransform, Anchor.TopCenter) { AbsoluteOffset = new Point(0, HUDLayoutSettings.Padding) }, string.Empty, textAlignment: Alignment.Center, font: GUIStyle.SubHeadingFont);
        submarineDisplayElement.submarineFee = new GUITextBlock(new RectTransform(new Vector2(1f, 0.25f), submarineDisplayElement.background.RectTransform, Anchor.BottomCenter) { AbsoluteOffset = new Point(0, HUDLayoutSettings.Padding) }, string.Empty, textAlignment: Alignment.Center, font: GUIStyle.SubHeadingFont);
        submarineDisplayElement.selectSubmarineButton = new GUIButton(new RectTransform(Vector2.One, submarineDisplayElement.background.RectTransform), style: null);
        submarineDisplayElement.previewButton = new GUIButton(new RectTransform(Vector2.One * 0.12f, submarineDisplayElement.background.RectTransform, anchor: Anchor.BottomRight, scaleBasis: ScaleBasis.BothHeight) { AbsoluteOffset = new Point((int)(0.03f * background.Rect.Height)) }, style: "ExpandButton")
        {
          Color = Color.White,
          HoverColor = Color.White,
          PressedColor = Color.White
        };
        submarineDisplayElement.submarineClass = new GUITextBlock(new RectTransform(new Vector2(1f, 0.1f), submarineDisplayElement.background.RectTransform, Anchor.TopCenter) { AbsoluteOffset = new Point(0, HUDLayoutSettings.Padding + (int)GUIStyle.Font.MeasureString(submarineDisplayElement.submarineName.Text).Y) }, string.Empty, textAlignment: Alignment.Left);
        submarineDisplayElement.submarineTier = new GUITextBlock(new RectTransform(new Vector2(0.5f, 0.1f), submarineDisplayElement.background.RectTransform, Anchor.TopRight) { AbsoluteOffset = new Point(0, HUDLayoutSettings.Padding + (int)GUIStyle.Font.MeasureString(submarineDisplayElement.submarineName.Text).Y) }, string.Empty, textAlignment: Alignment.Right);

        _.submarineDisplays[i] = submarineDisplayElement;
      }

      _.selectedSubmarineIndicator = new GUICustomComponent(new RectTransform(Point.Zero, _.submarineHorizontalGroup.RectTransform), onDraw: (sb, component) => _.DrawSubmarineIndicator(sb, component.Rect)) { IgnoreLayoutGroups = true, CanBeFocused = false };

      return false;
    }
  }
}