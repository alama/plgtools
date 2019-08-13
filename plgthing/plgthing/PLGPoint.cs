using System;
using System.IO;

namespace plgthing
{
    public class PLGPoint
    {
        // vert colors
        public byte r = 0;
        public byte g = 0;
        public byte b = 0;
        public byte a = 0;

        // relative to the top left corner
        public float x = 0;
        public float y = 0;

        // layer
        public uint _field_0c = 0;

        public int ID { get; set; }
        public uint RGBA { get; set; }

        public PLGPoint(int id) {
            this.ID = id;
        }

        public override string ToString()
        {
            return $"id: {this.ID} rgba: {this.RGBA:x8} xy:{this.x:0.00},{this.y:0.00} _field_0c:{this._field_0c:x8}";
        }

        public PLGPoint Read(BinaryReader br)
        {
            byte[] buffer = br.ReadBytes(0x04);
            this.r = buffer[0];
            this.g = buffer[1];
            this.b = buffer[2];
            this.a = buffer[3];
            Array.Reverse(buffer);
            this.RGBA = BitConverter.ToUInt32(buffer, 0);

            this.x = br.ReadSingle();
            this.y = br.ReadSingle();

            this._field_0c = br.ReadUInt32();

            return this;
        }
    }
}
