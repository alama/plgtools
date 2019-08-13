using System;
using System.IO;

namespace plgthing
{
    public class PLGHeader
    {   
        // PLG0
        public char[] magic = new char[0x04];

        public uint _field_04 = 0x00;

        // relative to file start
        public uint obj_data_offset = 0x00;
        public uint vert_data_offset = 0x00;
        public uint faceidx_data_offset = 0x00;

        public uint file_size = 0x00;

        public uint _field_18 = 0x00;

        // for all objects
        public uint obj_count = 0x00;
        public uint vert_count = 0x00;

        public uint _field_24 = 0x00;

        // for all objects
        public uint faceidx_count = 0x00;

        public uint _field_2c = 0x00;
        public uint _field_30 = 0x00;
        public uint _field_34 = 0x00;

        public PLGHeader() { }

        public override string ToString()
        {
            return $"objects: {this.obj_count} verts: {this.vert_count} faceidx: {this.faceidx_count}";
        }

        public PLGHeader Read(BinaryReader br)
        {
            this.magic = br.ReadChars(magic.Length);
            this._field_04 = br.ReadUInt32();
            this.obj_data_offset = br.ReadUInt32();
            this.vert_data_offset = br.ReadUInt32();
            this.faceidx_data_offset = br.ReadUInt32();

            switch (this.obj_data_offset)
            {
                case 0x38:
                    this.file_size = br.ReadUInt32();
                    this._field_18 = br.ReadUInt32();
                    this.obj_count = br.ReadUInt32();
                    this.vert_count = br.ReadUInt32();
                    this._field_24 = br.ReadUInt32();
                    this.faceidx_count = br.ReadUInt32();
                    this._field_2c = br.ReadUInt32();
                    this._field_30 = br.ReadUInt32();
                    this._field_34 = br.ReadUInt32();
                    break;
                case 0x20:
                    this.obj_count = br.ReadUInt16();
                    this.vert_count = br.ReadUInt16();
                    this._field_24 = br.ReadUInt16();
                    this.faceidx_count = br.ReadUInt16();
                    this._field_2c = br.ReadUInt32();
                    break;
                default:
                    throw new Exception("unknown header format");
            }

            return this;
        }
    }
}
