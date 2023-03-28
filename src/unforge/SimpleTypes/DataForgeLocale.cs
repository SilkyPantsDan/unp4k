﻿using System.Xml;

namespace unforge
{
	public class DataForgeLocale : DataForgeSerializable
	{
		private readonly UInt32 _value;
		public String Value { get { return this.DocumentRoot.ValueMap[this._value]; } }

		public DataForgeLocale(DataForge documentRoot)
			: base(documentRoot)
		{
			this._value = this._br.ReadUInt32();
		}

		public override String ToString()
		{
			return this.Value;
		}

		public XmlElement Read()
		{
			var element = this.DocumentRoot.CreateElement("LocID");
			var attribute = this.DocumentRoot.CreateAttribute("value");
			attribute.Value = this.Value.ToString();
			// TODO: More work here
			element.Attributes.Append(attribute);
			return element;
		}
	}
}
