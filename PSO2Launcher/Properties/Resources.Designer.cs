﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Dogstar.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Dogstar.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files (x86)\SEGA.
        /// </summary>
        internal static string DefaultInstallDir {
            get {
                return ResourceManager.GetString("DefaultInstallDir", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://rex.flyingcat.org/donate.
        /// </summary>
        internal static string DogstarDonation {
            get {
                return ResourceManager.GetString("DogstarDonation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://github.com/LightningDragon/Dogstar.
        /// </summary>
        internal static string DogstarGithub {
            get {
                return ResourceManager.GetString("DogstarGithub", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://widget01.mibbit.com/?server=irc.badnik.net&amp;channel=%23PSO2Proxypublic.
        /// </summary>
        internal static string DogstarSupport {
            get {
                return ResourceManager.GetString("DogstarSupport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://twitter.com/DogstarTeam.
        /// </summary>
        internal static string DogstarTwitter {
            get {
                return ResourceManager.GetString("DogstarTwitter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Game executable (pso2.exe)|pso2.exe.
        /// </summary>
        internal static string GameFilter {
            get {
                return ResourceManager.GetString("GameFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 10 11 12 13 14 15 16 17 18 19 1A 1B 1C 1D 1E 1F 20 21 22 23 24 25 26 27 28 29 2A 2B 2C 2D 2E 2F 30 31 32 33 34 35 36 37 38 39 3A 3B 3C 3D 3E 3F 40 41 42 43 44 45 46 47 48 49 4A 4B 4C 4D 4E 4F 50 51 52 53 54 55 56 57 58 59 5A 5B 5C 5D 5E 5F 60 61 62 63 64 65 66 67 68 69 6A 6B 6C 6D 6E 6F 70 71 72 73 74 75 76 77 78 79 7A 7B 7C 7D 7E 7F 80 81 82 83 84 85 86 87 88 89 8A 8B 8C 8D 8E 8F 90 91 92 93 94 95 96 97 98 99 9A 9B 9C 9D 9E 9F A0 A1 A2 A3 A4 A5 A6 A7 A8 A9 AA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string HexTable {
            get {
                return ResourceManager.GetString("HexTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function table_print (tt, indent, done)
        ///  done = done or {}
        ///  indent = indent or 0
        ///  if type(tt) == &quot;table&quot; then
        ///    local sb = {}
        ///    for key, value in pairs (tt) do
        ///      table.insert(sb, string.rep (&quot; &quot;, indent)) -- indent it
        ///      if type (value) == &quot;table&quot; and not done [value] then
        ///        done [value] = true
        ///        table.insert(sb, string.format(&quot;%s = {\n&quot;, tostring(key)));
        ///        table.insert(sb, table_print (value, indent + 2, done))
        ///        table.insert(sb, string.rep (&quot; &quot;, indent)) -- [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Lua_table_print {
            get {
                return ResourceManager.GetString("Lua_table_print", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function to_string( tbl )
        ///    if  &quot;nil&quot;       == type( tbl ) then
        ///        return tostring(nil)
        ///    elseif  &quot;table&quot; == type( tbl ) then
        ///        return string.sub(table_print(tbl), 1,-3)
        ///    elseif  &quot;string&quot; == type( tbl ) then
        ///        return tbl
        ///    else
        ///        return tostring(tbl)
        ///    end
        ///end.
        /// </summary>
        internal static string Lua_to_string {
            get {
                return ResourceManager.GetString("Lua_to_string", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The MIT License (MIT)
        ///
        ///Copyright (c) 2007 James Newton-King
        ///
        ///Permission is hereby granted, free of charge, to any person obtaining a copy of
        ///this software and associated documentation files (the &quot;Software&quot;), to deal in
        ///the Software without restriction, including without limitation the rights to
        ///use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
        ///the Software, and to permit persons to whom the Software is furnished to do so,
        ///subject to the following conditions:
        ///
        ///The ab [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MitLicense {
            get {
                return ResourceManager.GetString("MitLicense", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&amp;hosted_button_id=UB7UN9MQ7WZ44.
        /// </summary>
        internal static string PolarisDonation {
            get {
                return ResourceManager.GetString("PolarisDonation", resourceCulture);
            }
        }
    }
}
