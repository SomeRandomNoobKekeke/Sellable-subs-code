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
  public static class GUIComponentExtension
  {
    public static void hide(this GUIComponent component, bool update = false)
    {
      component.Visible = false;
      component.RectTransform.relativeSize = new Vector2(0f, 0f);
      if (update) component.Parent.RectTransform.RecalculateAll(resize: true, scale: false, withChildren: true);
    }

    public static void reveal(this GUIComponent component, float relX = 0.1f, float relY = 1f, bool update = false)
    {
      component.RectTransform.relativeSize = new Vector2(relX, relY);
      if (update) component.Parent.RectTransform.RecalculateAll(resize: true, scale: false, withChildren: true);
      component.Visible = true;
    }

    public static void hideIf(this GUIComponent component, bool condition, float relX = 0.1f, float relY = 1f, bool update = false)
    {
      if (condition) hide(component, update);
      else reveal(component, relX, relY, update);
    }

    public static void revealIf(this GUIComponent component, bool condition, float relX = 0.1f, float relY = 1f, bool update = false)
    {
      if (condition) reveal(component, relX, relY, update);
      else hide(component, update);
    }

    public static void recalcParent(this GUIComponent component)
    {
      component.Parent?.RectTransform.RecalculateAll(resize: true, scale: false, withChildren: true);
    }

    public static void recalc(this GUIComponent component)
    {
      component.RectTransform.RecalculateAll(resize: true, scale: false, withChildren: true);
    }
  }
}