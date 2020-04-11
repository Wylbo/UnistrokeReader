///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 11/04/2020 15:13
///-----------------------------------------------------------------

using System.IO;
using System.Text;
using UnityEngine;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class SaveLine {

        public static string directory = "Assets/_UnistrokeReader/Pattern/";

        public static bool SavePattern(Line line, string filename) {
            bool success = true;
            using (StreamWriter sw = new StreamWriter(directory + filename + ".xml", false, new UTF8Encoding(false))) {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                sw.WriteLine("<Line Name = \"{0}\">", filename);

                for (int i = 0; i < line.Points.Count; i++) {
                    sw.WriteLine("\t<Point X = \"{0}\" Y = \"{1}\"/>", line.Points[i].x, line.Points[i].y);
                }

                sw.WriteLine("</Line>");
            }
            Debug.Log("[SaveLine] File created");
            return success;
        }

    }
}