﻿namespace unforge
{
	public class DataForgeString : DataForgeSerializable
	{
		public String Value { get; set; }

		public DataForgeString(DataForge documentRoot)
			: base(documentRoot)
		{
			this.Value = this._br.ReadCString();
		}

		public override String ToString()
		{
			return this.Value;
		}
	}
}
