namespace unforge
{
	public abstract class DataForgeSerializable
	{
		internal DataForge DocumentRoot { get; private set; }
		internal BinaryReader _br;

		public DataForgeSerializable(DataForge documentRoot)
		{
			this.DocumentRoot = documentRoot;
			this._br = documentRoot._br;
		}
	}
}
