using System.Collections.Generic;
using System.IO;

namespace plgthing
{
    public class PLGFile
    {
        public PLGHeader Header = new PLGHeader();
        public IList<PLGObject> Objects = new List<PLGObject>();

        public PLGFile() { }

        public PLGFile Read(BinaryReader br)
        {
            // parse header
            this.Header = new PLGHeader().Read(br);

            // parse objects
            for (int i = 0; i < Header.obj_count; i++)
            {
                int size = Header.obj_data_offset == 0x38 ? 0x48 : 0x40;
                PLGObject o = new PLGObject(size, i);
                this.Objects.Add(o.Read(br));
            }

            // backup vert/face data offsets
            uint vdo = this.Header.vert_data_offset - this.Header.obj_data_offset;
            uint fdo = this.Header.faceidx_data_offset - this.Header.obj_data_offset;

            // parse verts and faces
            for (int i = 0; i < Header.obj_count; i++) {
                PLGObject o = Objects[i];

                // workaround, all of the objects in p5's fclItem.plg have this set to 0
                if (o.vert_data_offset == 0)
                    o.vert_data_offset = vdo;
                vdo += 0x10u * o.vert_count - (uint)o.Size;

                // workaround, all of the objects in p5's fclItem.plg have this set to 0
                if (o.faceidx_data_offset == 0)
                    o.faceidx_data_offset = fdo;
                fdo += 0x02u * o.faceidx_count - (uint)o.Size;

                long currObjOffset = Header.obj_data_offset + o.Size * i;

                // parse verts
                br.BaseStream.Seek(currObjOffset + o.vert_data_offset, SeekOrigin.Begin);
                for (int j = 0; j < o.vert_count; j++)
                    o.Verts.Add(new PLGPoint(j).Read(br));

                // parse face idx
                br.BaseStream.Seek(currObjOffset + o.faceidx_data_offset, SeekOrigin.Begin);
                for (int j = 0; j < o.faceidx_count; j++)
                    o.FaceIdx.Add(br.ReadInt16());

                // create faces
                o.CreateFaceList();

                if (i < this.Header.obj_count - 1)
                    br.BaseStream.Seek(currObjOffset + o.Size, SeekOrigin.Begin);
            }

            return this;
        }
    }
}
