﻿// Decompiled with JetBrains decompiler
// Type: ZuneUI.HipPropertyEditor
// Assembly: ZuneShell, Version=4.7.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: FC8028F3-A47B-4FB4-B35B-11D1752D8264
// Assembly location: C:\Program Files\Zune\ZuneShell.dll

namespace ZuneUI
{
    public class HipPropertyEditor : WizardPropertyEditor
    {
        private static PropertyDescriptor[] s_dataProviderProperties;
        public static PropertyDescriptor s_HipCharacters = new PropertyDescriptor("HIPSolution", string.Empty, string.Empty, true);

        public override PropertyDescriptor[] PropertyDescriptors
        {
            get
            {
                if (HipPropertyEditor.s_dataProviderProperties == null)
                    HipPropertyEditor.s_dataProviderProperties = new PropertyDescriptor[1]
                    {
            HipPropertyEditor.s_HipCharacters
                    };
                return HipPropertyEditor.s_dataProviderProperties;
            }
        }

        public static PropertyDescriptor HipCharacters => HipPropertyEditor.s_HipCharacters;
    }
}
