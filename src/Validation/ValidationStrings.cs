using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
namespace Validation
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
	internal class ValidationStrings
	{
		private static ResourceManager resourceMan;
		private static CultureInfo resourceCulture;
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(ValidationStrings.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("System.Collections.Immutable.Validation.ValidationStrings", typeof(ValidationStrings).Assembly);
					ValidationStrings.resourceMan = resourceManager;
				}
				return ValidationStrings.resourceMan;
			}
		}
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return ValidationStrings.resourceCulture;
			}
			set
			{
				ValidationStrings.resourceCulture = value;
			}
		}
		internal static string Argument_EmptyArray
		{
			get
			{
				return ValidationStrings.ResourceManager.GetString("Argument_EmptyArray", ValidationStrings.resourceCulture);
			}
		}
		internal static string Argument_EmptyString
		{
			get
			{
				return ValidationStrings.ResourceManager.GetString("Argument_EmptyString", ValidationStrings.resourceCulture);
			}
		}
		internal static string Argument_NullElement
		{
			get
			{
				return ValidationStrings.ResourceManager.GetString("Argument_NullElement", ValidationStrings.resourceCulture);
			}
		}
		internal static string Argument_Whitespace
		{
			get
			{
				return ValidationStrings.ResourceManager.GetString("Argument_Whitespace", ValidationStrings.resourceCulture);
			}
		}
		internal ValidationStrings()
		{
		}
	}
}
