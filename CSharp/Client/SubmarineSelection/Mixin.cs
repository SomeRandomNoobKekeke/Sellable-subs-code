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
    public class SubmarineSelectionMixin
    {
      public GUIButton sellButton;
      public GUITickBox sellCurrentTickBox;
      public GUILayoutGroup bottomContainer;
    }

    public static Dictionary<SubmarineSelection, SubmarineSelectionMixin> mixins = new Dictionary<SubmarineSelection, SubmarineSelectionMixin>();

  }
}