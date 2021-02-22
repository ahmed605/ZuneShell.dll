﻿// Decompiled with JetBrains decompiler
// Type: ZuneUI.ContactInfoParentPropertyEditor
// Assembly: ZuneShell, Version=4.7.0.0, Culture=neutral, PublicKeyToken=ddd0da4d3e678217
// MVID: FC8028F3-A47B-4FB4-B35B-11D1752D8264
// Assembly location: C:\Program Files\Zune\ZuneShell.dll

namespace ZuneUI
{
    public class ContactInfoParentPropertyEditor : BaseContactInfoPropertyEditor
    {
        private static PropertyDescriptor[] s_dataProviderProperties;
        public static BirthdayPropertyDescriptor s_Birthday = new BirthdayPropertyDescriptor(nameof(Birthday), string.Empty, string.Empty, true);
        public static CountryPropertyDescriptor s_Country = new CountryPropertyDescriptor(nameof(Country), string.Empty, string.Empty, true);

        public override PropertyDescriptor[] PropertyDescriptors
        {
            get
            {
                if (ContactInfoParentPropertyEditor.s_dataProviderProperties == null)
                    ContactInfoParentPropertyEditor.s_dataProviderProperties = new PropertyDescriptor[7]
                    {
            (PropertyDescriptor) BaseContactInfoPropertyEditor.s_FirstName,
            (PropertyDescriptor) BaseContactInfoPropertyEditor.s_LastName,
            (PropertyDescriptor) BaseContactInfoPropertyEditor.s_PhoneNumber,
            (PropertyDescriptor) BaseContactInfoPropertyEditor.s_PhoneExtension,
            (PropertyDescriptor) BaseContactInfoPropertyEditor.s_Email,
            (PropertyDescriptor) ContactInfoParentPropertyEditor.s_Birthday,
            (PropertyDescriptor) ContactInfoParentPropertyEditor.s_Country
                    };
                return ContactInfoParentPropertyEditor.s_dataProviderProperties;
            }
        }

        public static PropertyDescriptor Birthday => (PropertyDescriptor)ContactInfoParentPropertyEditor.s_Birthday;

        public static PropertyDescriptor Country => (PropertyDescriptor)ContactInfoParentPropertyEditor.s_Country;
    }
}
