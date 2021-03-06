﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mustache.Properties {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///    A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        internal Resources() {
        }
        
        /// <summary>
        ///    Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Mustache.Properties.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///    Overrides the current thread's CurrentUICulture property for all
        ///    resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to An attempt was made to define a parameter with a null or an invalid identifier..
        /// </summary>
        public static string BlankParameterName {
            get {
                return ResourceManager.GetString("BlankParameterName", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to An attempt was made to define a tag with a null or an invalid identifier..
        /// </summary>
        public static string BlankTagName {
            get {
                return ResourceManager.GetString("BlankTagName", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to A parameter with the same name already exists within the tag..
        /// </summary>
        public static string DuplicateParameter {
            get {
                return ResourceManager.GetString("DuplicateParameter", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The {0} tag has already been registered..
        /// </summary>
        public static string DuplicateTagDefinition {
            get {
                return ResourceManager.GetString("DuplicateTagDefinition", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Expected a matching {0} tag but none was found..
        /// </summary>
        public static string MissingClosingTag {
            get {
                return ResourceManager.GetString("MissingClosingTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to A partial template named {0} could not be found..
        /// </summary>
        public static string PartialNotDefined {
            get {
                return ResourceManager.GetString("PartialNotDefined", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to Encountered an unknown tag: {0}. It was either not registered or exists in a different context..
        /// </summary>
        public static string UnknownTag {
            get {
                return ResourceManager.GetString("UnknownTag", resourceCulture);
            }
        }
        
        /// <summary>
        ///    Looks up a localized string similar to The wrong number of arguments were passed to an {0} tag..
        /// </summary>
        public static string WrongNumberOfArguments {
            get {
                return ResourceManager.GetString("WrongNumberOfArguments", resourceCulture);
            }
        }
    }
}
