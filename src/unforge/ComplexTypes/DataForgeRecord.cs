﻿namespace unforge
{
	public class DataForgeRecord : DataForgeSerializable
	{
		public UInt32 NameOffset { get; set; }
		public String Name { get { return this.DocumentRoot.ValueMap[this.NameOffset]; } }

		public String FileName { get { return this.DocumentRoot.ValueMap[this.FileNameOffset]; } }
		public UInt32 FileNameOffset { get; set; }

		public String StructIndexString { get { return String.Format("{0:X4}", this.StructIndex); } }
		public UInt32 StructIndex { get; set; }

		public Guid? Hash { get; set; }

		public String VariantIndexString { get { return String.Format("{0:X4}", this.VariantIndex); } }
		public UInt16 VariantIndex { get; set; }

		public String OtherIndexString { get { return String.Format("{0:X4}", this.OtherIndex); } }
		public UInt16 OtherIndex { get; set; }

		public DataForgeRecord(DataForge documentRoot)
			: base(documentRoot)
		{
			this.NameOffset = this._br.ReadUInt32();

			if (!this.DocumentRoot.IsLegacy)
			{
				this.FileNameOffset = this._br.ReadUInt32();
			}

			this.StructIndex = this._br.ReadUInt32();
			this.Hash = this._br.ReadGuid(false);

			this.VariantIndex = this._br.ReadUInt16();
			this.OtherIndex = this._br.ReadUInt16();
		}

		public override String ToString()
		{
			return String.Format("<{0} {1:X4} />", this.Name, this.StructIndex);
		}
	}
}
