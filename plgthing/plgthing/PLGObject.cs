using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace plgthing
{
    public class PLGObject
    {
        // relative to the current object pos
        public uint vert_data_offset = 0x00;
        public uint faceidx_data_offset = 0x00;
        public uint eof_offset = 0x00;

        // for all "layers"
        public ushort vert_count = 0x00;
        public ushort faceidx_count = 0x00;

        public uint _field_10 = 0x00;

        // connect n verts per face
        public ushort n_gon = 0x00;

        public ushort _field_16 = 0x00;

        // viewport
        public float xmin = 0x00;
        public float xmax = 0x00;
        public float ymin = 0x00;
        public float ymax = 0x00;

        public byte[] name = new byte[0x20];

        public float Width => Math.Abs(this.xmax) + Math.Abs(this.xmin);
        public float Height => Math.Abs(this.ymax) + Math.Abs(this.ymin);
        public string Name { get; set; }
        public int Size { get; set; }
        public int ID { get; set; }

        public IList<PLGPoint> Verts = new List<PLGPoint>();
        public IList<short> FaceIdx = new List<short>();
        public IList<PLGPoint[]> Faces = new List<PLGPoint[]>();

        public PLGObject(int size, int id)
        {
            this.ID = id;
            this.Size = size;
        }

        public override string ToString()
        {
            return $"id: {this.ID} name: {this.Name} size: {this.Size}";
        }

        public PLGObject Read(BinaryReader br)
        {
            this.vert_data_offset = br.ReadUInt32();
            this.faceidx_data_offset = br.ReadUInt32();

            if (this.Size == 0x48)
                this.eof_offset = br.ReadUInt32();

            this.vert_count = br.ReadUInt16();
            this.faceidx_count = br.ReadUInt16();

            if (this.Size == 0x48)
                this._field_10 = br.ReadUInt32();

            this.n_gon = br.ReadUInt16();
            this._field_16 = br.ReadUInt16();
            this.xmin = br.ReadSingle();
            this.ymin = br.ReadSingle();
            this.xmax = br.ReadSingle();
            this.ymax = br.ReadSingle();
            this.name = br.ReadBytes(this.name.Length);

            // shift-jis enc
            this.Name = Encoding.GetEncoding(932).GetString(this.name);
            this.Name = this.Name.Substring(0, this.Name.IndexOf('\0'));

            return this;
        }

        public void CreateFaceList()
        {
            for (int i = 0; i < this.FaceIdx.Count; i += this.n_gon)
            {
                PLGPoint[] connect = new PLGPoint[this.n_gon];

                for (int j = 0; j < n_gon; j++) {
                    connect[j] = this.Verts[this.FaceIdx[i + j]];
                }

                this.Faces.Add(connect);
            }
        }
    }
}
