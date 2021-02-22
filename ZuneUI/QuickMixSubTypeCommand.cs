﻿// Decompiled with JetBrains decompiler
// Type: ZuneUI.QuickMixSubTypeCommand
// Assembly: ZuneShell, Version=4.7.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: FC8028F3-A47B-4FB4-B35B-11D1752D8264
// Assembly location: C:\Program Files\Zune\ZuneShell.dll

using Microsoft.Iris;
using Microsoft.Zune.QuickMix;
using System;

namespace ZuneUI
{
    public class QuickMixSubTypeCommand : Command
    {
        public QuickMixSubTypeCommand(string description, EQuickMixType type, QuickMixSession session)
          : base((IModelItemOwner)null, description, (EventHandler)null)
          => this.Available = session.GetQuickMixTypeAvailable(type);
    }
}
