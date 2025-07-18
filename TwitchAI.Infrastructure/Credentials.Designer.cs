﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TwitchAI.Infrastructure {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Credentials {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Credentials() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TwitchAI.Infrastructure.Credentials", typeof(Credentials).Assembly);
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
        ///   Looks up a localized string similar to https://api.openai.com/v1/chat/completions.
        /// </summary>
        internal static string GptEngine {
            get {
                return ResourceManager.GetString("GptEngine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to org-nm3GGAGABvgGhDfQXdSNTXYE.
        /// </summary>
        internal static string GptOrganizationId {
            get {
                return ResourceManager.GetString("GptOrganizationId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to proj_pYzvmFrsIpqtbWQATVglxUeh.
        /// </summary>
        internal static string GptProjectId {
            get {
                return ResourceManager.GetString("GptProjectId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to gp762nuuoqcoxypju8c569th9wz7q5.
        /// </summary>
        internal static string TwitchClientId {
            get {
                return ResourceManager.GetString("TwitchClientId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to irc.chat.twitch.tv.
        /// </summary>
        internal static string TwitchIRC {
            get {
                return ResourceManager.GetString("TwitchIRC", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to lh8y8xl2haun2i63dgmfs27qm1o8tnry5aoaxsrq9f2ym39u6s.
        /// </summary>
        internal static string TwitchRefreshToken {
            get {
                return ResourceManager.GetString("TwitchRefreshToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to bg5s7vpm3h987d5o7ci8v2ieh4y9wa.
        /// </summary>
        internal static string TwitchTokenAccess {
            get {
                return ResourceManager.GetString("TwitchTokenAccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to nekotyan_ai.
        /// </summary>
        internal static string TwitichUserName {
            get {
                return ResourceManager.GetString("TwitichUserName", resourceCulture);
            }
        }
    }
}
