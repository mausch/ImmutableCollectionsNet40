using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
namespace System.Collections.Immutable
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
	internal class Strings
	{
		private static ResourceManager resourceMan;
		private static CultureInfo resourceCulture;
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Strings.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("System.Collections.Immutable.Strings", typeof(Strings).Assembly);
					Strings.resourceMan = resourceManager;
				}
				return Strings.resourceMan;
			}
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Strings.resourceCulture;
			}
			set
			{
				Strings.resourceCulture = value;
			}
		}
		internal static string ArrayInitializedStateNotEqual
		{
			get
			{
				return Strings.ResourceManager.GetString("ArrayInitializedStateNotEqual", Strings.resourceCulture);
			}
		}
		internal static string ArrayLengthsNotEqual
		{
			get
			{
				return Strings.ResourceManager.GetString("ArrayLengthsNotEqual", Strings.resourceCulture);
			}
		}
		internal static string CannotFindOldValue
		{
			get
			{
				return Strings.ResourceManager.GetString("CannotFindOldValue", Strings.resourceCulture);
			}
		}
		internal static string CollectionModifiedDuringEnumeration
		{
			get
			{
				return Strings.ResourceManager.GetString("CollectionModifiedDuringEnumeration", Strings.resourceCulture);
			}
		}
		internal static string DuplicateKey
		{
			get
			{
				return Strings.ResourceManager.GetString("DuplicateKey", Strings.resourceCulture);
			}
		}
		internal static string InvalidEmptyOperation
		{
			get
			{
				return Strings.ResourceManager.GetString("InvalidEmptyOperation", Strings.resourceCulture);
			}
		}
		internal Strings()
		{
		}
	}
}
