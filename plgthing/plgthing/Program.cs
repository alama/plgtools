using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Drawing.Drawing2D;

namespace plgthing
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Directory.Exists("plgs")) { return; }

            string[] files = Directory.GetFiles("plgs", "*.plg", SearchOption.AllDirectories);
            IDictionary<string, PLGFile> plgs = new Dictionary<string, PLGFile>();

            foreach (string file in files)
            {
                Console.WriteLine($"parsing {file}");
                plgs[file] = ImportPLG(file);
            }

            string nameInvalid = new string(Path.GetInvalidFileNameChars());
            Regex nameRegex = new Regex($"[{Regex.Escape(nameInvalid)}]");

            foreach (string file in plgs.Keys)
            {
                Console.WriteLine($"exporting {file}");

                int empty = 0;
                string outDir = Path.Combine($"{file}_ex");
                Directory.CreateDirectory(outDir);

                foreach (PLGObject o in plgs[file].Objects)
                {
                    string outFileName = o.Name;
                    outFileName = nameRegex.Replace(outFileName, "");

                    if (string.IsNullOrEmpty(outFileName))
                    {
                        outFileName = string.Format($"_emptyname_{empty++:00}");
                    }

                    string outPath = string.Format($"{outDir}\\{o.ID:00}_{outFileName}");

                    ExportPng(o, outPath);
                    ExportSvg(o, outPath);
                    ExportPly(o, outPath);
                }
            }
        }

        public static PLGFile ImportPLG(string filePath)
        {
            Stream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (BinaryReader br = new BinaryReader(fs))
                {
                    PLGFile plg = new PLGFile().Read(br);
                    return plg;
                }
            }
            finally
            {
                fs?.Dispose();
            }
        }

        public static void ExportPng(PLGObject o, string savePath, int pad = 40)
        {
            Image img = new Bitmap((int)o.Width + pad, (int)o.Height + pad);
            Graphics g = Graphics.FromImage(img);
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (PLGPoint[] face in o.Faces)
            {
                PointF[] pf = face.Select(v => new PointF(v.x - o.xmin + pad / 2, v.y - o.ymin + pad / 2)).ToArray();
                g.FillPolygon(Brushes.Black, pf);
                g.DrawPolygon(Pens.Black, pf);
            }

            img.Save($"{savePath}.png");
        }

        public static void ExportSvg(PLGObject o, string savePath, int pad = 0)
        {
            // create basic svg doc
            XNamespace svgNs = XNamespace.Get("http://www.w3.org/2000/svg");
            XDocument svgDoc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(svgNs + "svg",
                    new XAttribute("width", o.Width),
                    new XAttribute("height", o.Height),
                    new XAttribute("viewBox", $"{o.xmin} {o.ymin} {o.Width} {o.Height}"),
                    new XElement("title", o.Name)));

            XElement f = new XElement("g", new XAttribute("name", "faces"));
            XElement v = new XElement("g", new XAttribute("name", "verts"));

            // create faces
            foreach (PLGPoint[] face in o.Faces) {
                XElement xef = new XElement("polygon",
                    new XAttribute("points", string.Join(" ", face.Select(pt => $"{pt.x},{pt.y}"))));
                f.Add(xef);
            }

            // create verts
            foreach (PLGPoint p in o.Verts)
            {
                XElement xep = new XElement("circle",
                    new XAttribute("cx", p.x),
                    new XAttribute("cy", p.y),
                    new XAttribute("r", 0.5),
                    new XAttribute("fill", $"#{p.RGBA:x8}"),
                    new XAttribute("unk", $"{p._field_0c:x8}"),
                    new XAttribute("stroke", "#80808080"),
                    new XAttribute("stroke-width", 0.2));

                v.Add(xep);
            }

            svgDoc.Root.Add(f);
            svgDoc.Root.Add(v);

            // apply the same xmlns to all elements
            foreach (XElement e in svgDoc.Descendants())
                if (e.Name.Namespace == string.Empty)
                    e.Name = svgNs + e.Name.LocalName;

            svgDoc.Save($"{savePath}.svg");
        }

        static void ExportPly(PLGObject o, string savePath)
        {
            // ply header
            StringBuilder ply = new StringBuilder()
                .AppendLine("ply")
                .AppendLine("format ascii 1.0")
                .AppendLine($"element vertex {o.vert_count}")
                .AppendLine("property float x")
                .AppendLine("property float y")
                .AppendLine("property float z")
                .AppendLine("property uchar red")
                .AppendLine("property uchar green")
                .AppendLine("property uchar blue")
                .AppendLine("property uchar alpha")
                .AppendLine("property uint _field_0c")
                .AppendLine($"element face {o.Faces.Count}")
                .AppendLine("property list uchar int vertex_index")
                .AppendLine("end_header");

            // add verts
            foreach(PLGPoint v in o.Verts)
            {
                // x y 0 r g b a
                ply.AppendFormat($"{v.x} {v.y} 0 {v.r} {v.g} {v.b} {v.a} {v._field_0c}")
                    .AppendLine();
            }

            // add faces
            foreach (PLGPoint[] face in o.Faces)
            {
                ply.Append($"{o.n_gon} ");
                
                foreach (PLGPoint v in face)
                {
                    ply.Append($"{v.ID} ");
                }

                ply.AppendLine();
            }

            File.WriteAllText($"{savePath}.ply", ply.ToString());
        }
    }
}
