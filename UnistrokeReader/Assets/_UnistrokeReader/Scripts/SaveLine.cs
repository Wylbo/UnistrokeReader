///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 11/04/2020 15:13
///-----------------------------------------------------------------

using System.IO;
using UnityEngine;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class SaveLine {

        public static bool SavePattern(Line line, string filename) {
            bool success = true;
            using (StreamWriter sw = new StreamWriter(filename + ".xml", false, System.Text.Encoding.UTF8)) {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                sw.WriteLine("<Line Name = \"{0}\">", filename);

                for (int i = 0; i < line.Points.Count; i++) {
                    sw.WriteLine("\t<Point X = \"{0}\" Y = \"{1}\">", line.Points[i].x, line.Points[i].y);
                }

                sw.WriteLine("</Line Name>");
            }
            Debug.Log("[SaveLine] File created");
            return success;
        }

    }
}