﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Jde.DB.Dialects {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Sql {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Sql() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Jde.DB.Dialects.Sql", typeof(Sql).Assembly);
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
        ///   Looks up a localized string similar to select	t.TABLE_NAME, t.TABLE_TYPE, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, COLUMN_TYPE, CHARACTER_MAXIMUM_LENGTH, EXTRA=&apos;auto_increment&apos; is_identity, 0 is_id, NUMERIC_PRECISION, NUMERIC_SCALE
        ///
        ///from		INFORMATION_SCHEMA.TABLES t	
        ///	inner join INFORMATION_SCHEMA.COLUMNS c on t.TABLE_CATALOG=c.TABLE_CATALOG and t.TABLE_SCHEMA=c.TABLE_SCHEMA and t.TABLE_NAME=c.TABLE_NAME
        ///
        ///
        ///where t.TABLE_SCHEMA=&apos;{0}&apos;
        ///{1} 
        ///order by t.TABLE_NAME, ORDINAL_POSITION.
        /// </summary>
        internal static string Columns {
            get {
                return ResourceManager.GetString("Columns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to select	fk.CONSTRAINT_NAME name, fk.TABLE_NAME foreign_table, fk.COLUMN_NAME fk, pk.TABLE_NAME primary_table, pk.COLUMN_NAME pk, pk.ORDINAL_POSITION ordinal
        ///from		INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS con inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk on con.CONSTRAINT_NAME=fk.CONSTRAINT_NAME
        ///	inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk on pk.CONSTRAINT_NAME=con.UNIQUE_CONSTRAINT_NAME and pk.ORDINAL_POSITION=fk.ORDINAL_POSITION
        ///
        ///where pk.TABLE_SCHEMA=&apos;{0}&apos;
        ///order by name, ordinal.
        /// </summary>
        internal static string ForeignKeys {
            get {
                return ResourceManager.GetString("ForeignKeys", resourceCulture);
            }
        }
    }
}
