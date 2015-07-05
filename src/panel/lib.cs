using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Gtk;
using Gdk;

namespace abanu.panel
{
	public static class AppLib
	{
		public static CssProvider GlobalCssProvider;
	}

	public static class AppConfig
	{

		public static XDocument doc;

		public static void Load(string file)
		{
			doc = XDocument.Load(file);
		}

		public static IEnumerable<PanelConfig> Panels {
			get {
				foreach (var el in doc.Root.Element("panels").Elements("panel"))
					yield return new PanelConfig(el);
			}
		}

	}

	public class ConfigXmlElement
	{
		public XElement el;

		public ConfigXmlElement(XElement el)
		{
			this.el = el;
		}

		public string GetString(string name)
		{
			var value = GetString(name, null);
			if (value == null)
				throw new Exception(string.Format("Property {0} not found", name));
			return value;
		}

		public string GetString(string name, string defaultValue)
		{
			var attr = el.Attribute(name);
			if (attr == null)
				return defaultValue;
			else
				return attr.Value;
		}

		public int GetInt(string name, int defaultValue)
		{
			var value = GetString(name, null);
			if (value == null)
				return defaultValue;
			else
				return int.Parse(value);
		}

		public int GetInt(string name)
		{
			var value = GetString(name, null);
			if (value == null)
				throw new Exception(string.Format("Property {0} not found", name));
			else
				return int.Parse(value);
		}

		public double GetDouble(string name, double defaultValue)
		{
			var value = GetString(name, null);
			if (value == null)
				return defaultValue;
			else
				return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
		}

		public double GetDouble(string name)
		{
			var value = GetString(name, null);
			if (value == null)
				throw new Exception(string.Format("Property {0} not found", name));
			else
				return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
		}

		public bool GetBool(string name)
		{
			var value = GetString(name, null);
			if (value == null)
				return false;
			else
				return value == "1" || value == "true";
		}

		public object GetEnum(string name, Type enumType, object defaultValue)
		{
			var value = GetString(name, null);
			if (value == null)
				return defaultValue;
			else
				return System.Enum.Parse(enumType, value, true);
		}

		public object GetEnum(string name, Type enumType)
		{
			var value = GetString(name, null);
			if (value == null)
				throw new Exception(string.Format("Property {0} not found", name));
			else
				return System.Enum.Parse(enumType, value, true);
		}

	}

	public class PanelConfig : ConfigXmlElement
	{

		public PanelConfig(XElement el)
			: base(el)
		{
		}

		public int X { 
			get {
				return GetInt("x");	
			}
		}

		public int Y { 
			get {
				return GetInt("y");	
			}
		}

		public int RowHeight { 
			get {
				return GetInt("row-height");	
			}
		}

		public int Rows { 
			get {
				return GetInt("rows");	
			}
		}

		public int Monitor { 
			get {
				return GetInt("monitor");	
			}
		}

		public string Size { 
			get {
				return GetString("size");	
			}
		}

		public EDock Dock { 
			get {
				return (EDock)GetEnum("dock", typeof(EDock));
			}
		}

		public Orientation Orientation { 
			get {
				return (Orientation)GetEnum("orientation", typeof(Orientation));
			}
		}

		public IEnumerable<PluginConfig> Plugins {
			get {
				foreach (var child in el.Element("plugins").Elements("plugin"))
					yield return new PluginConfig(child);
			}
		}

	}

	public class PluginConfig : ConfigXmlElement
	{

		public PluginConfig(XElement el)
			: base(el)
		{
		}

		public bool Expand {
			get {
				return GetBool("expand");
			}
		}

		public string Type { 
			get {
				return GetString("type");	
			}
		}

	}
}



