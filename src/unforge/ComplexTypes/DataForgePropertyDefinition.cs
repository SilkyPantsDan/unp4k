﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace unforge
{
    public class DataForgePropertyDefinition : DataForgeSerializable
    {
        public UInt32 NameOffset { get; set; }
        public String Name { get { return this.DocumentRoot.ValueMap[this.NameOffset]; } }
        public UInt16 StructIndex { get; set; }
        public EDataType DataType { get; set; }
        public EConversionType ConversionType { get; set; }
        public UInt16 Padding { get; set; }

        public DataForgePropertyDefinition(DataForge documentRoot)
            : base(documentRoot)
        {
            this.NameOffset = this._br.ReadUInt32();
            this.StructIndex = this._br.ReadUInt16();
            this.DataType = (EDataType)this._br.ReadUInt16();
            this.ConversionType = (EConversionType)this._br.ReadUInt16();
            this.Padding = this._br.ReadUInt16();
        }

        public XmlAttribute Read()
        {
            XmlAttribute attribute = this.DocumentRoot.CreateAttribute(this.Name);

            switch (this.DataType)
            {
                case EDataType.varReference:
                    attribute.Value = this._br.ReadGuid(false).ToString();
                    break;
                case EDataType.varLocale:
                    attribute.Value = this.DocumentRoot.ValueMap[this._br.ReadUInt32()];
                    break;
                case EDataType.varStrongPointer:
                    attribute.Value = String.Format("{0}:{1:X8} {2:X8}", this.DataType, this._br.ReadUInt32(), this._br.ReadUInt32());
                    break;
                case EDataType.varWeakPointer:
                    var structIndex = this._br.ReadUInt32();
                    var itemIndex = this._br.ReadUInt32();
                    attribute.Value = String.Format("{0}:{1:X8} {1:X8}", this.DataType, structIndex);
                    this.DocumentRoot.Require_WeakMapping2.Add(new ClassMapping { Node = attribute, StructIndex = (UInt16)structIndex, RecordIndex = (Int32)itemIndex });
                    break;
                case EDataType.varString:
                    attribute.Value = this.DocumentRoot.ValueMap[this._br.ReadUInt32()];
                    break;
                case EDataType.varBoolean:
                    attribute.Value = this._br.ReadByte().ToString();
                    break;
                case EDataType.varSingle:
                    attribute.Value = this._br.ReadSingle().ToString();
                    break;
                case EDataType.varDouble:
                    attribute.Value = this._br.ReadDouble().ToString();
                    break;
                case EDataType.varGuid:
                    attribute.Value = this._br.ReadGuid(false).ToString();
                    break;
                case EDataType.varSByte:
                    attribute.Value = this._br.ReadSByte().ToString();
                    break;
                case EDataType.varInt16:
                    attribute.Value = this._br.ReadInt16().ToString();
                    break;
                case EDataType.varInt32:
                    attribute.Value = this._br.ReadInt32().ToString();
                    break;
                case EDataType.varInt64:
                    attribute.Value = this._br.ReadInt64().ToString();
                    break;
                case EDataType.varByte:
                    attribute.Value = this._br.ReadByte().ToString();
                    break;
                case EDataType.varUInt16:
                    attribute.Value = this._br.ReadUInt16().ToString();
                    break;
                case EDataType.varUInt32:
                    attribute.Value = this._br.ReadUInt32().ToString();
                    break;
                case EDataType.varUInt64:
                    attribute.Value = this._br.ReadUInt64().ToString();
                    break;
                case EDataType.varEnum:
                    // var enumDefinition = this.DocumentRoot.EnumDefinitionTable[this.StructIndex];
                    attribute.Value = this.DocumentRoot.ValueMap[this._br.ReadUInt32()];
                    break;
                default:
                    throw new NotImplementedException();
            }

            return attribute;
        }

        public override String ToString()
        {
            return String.Format("<{0} />", this.Name);
        }

        public String Export()
        {
            var sb = new StringBuilder();

            sb.AppendFormat(@"        [XmlArrayItem(Type = typeof({0}))]", this.DocumentRoot.StructDefinitionTable[this.StructIndex].Name);
            sb.AppendLine();

            foreach (var structDefinition in this.DocumentRoot.StructDefinitionTable)
            {
                Boolean allowed = false;

                var baseStruct = structDefinition;
                while (baseStruct.ParentTypeIndex != 0xFFFFFFFF && !allowed)
                {
                    allowed |= (baseStruct.ParentTypeIndex == this.StructIndex);
                    baseStruct = this.DocumentRoot.StructDefinitionTable[baseStruct.ParentTypeIndex];
                }

                if (allowed)
                {
                    sb.AppendFormat(@"        [XmlArrayItem(Type = typeof({0}))]", structDefinition.Name);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}