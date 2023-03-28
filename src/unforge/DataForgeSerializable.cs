using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
