///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 11/04/2020 16:29
///-----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

namespace Com.MaximilienGalea.UnistrokeReader {
	public class PatternReader {

		/// <summary>
		/// Read a line from xml file
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static List<Vector2> ReadFromXML(string xml) {

			XmlTextReader xmlReader = null;
			List<Vector2> pattern = null;

			try {
				xmlReader = new XmlTextReader(File.OpenText(xml));
				pattern = ReadLine(xmlReader);

			} catch (Exception e) {
				Debug.LogError("[PatternReader] " + e);
				throw;
			} finally {
				if (xmlReader != null) {
					xmlReader.Close();
				}
			}

			return pattern;
		}

		/// <summary>
		/// read the line from the xml file
		/// </summary>
		/// <param name="xmlReader"></param>
		/// <returns></returns>
		private static List<Vector2> ReadLine(XmlTextReader xmlReader) {
			List<Vector2> points = new List<Vector2>();
			string lineName = null;
			try {

				while (xmlReader.Read()) {
					if (xmlReader.NodeType != XmlNodeType.Element) {
						continue;
					}
					switch (xmlReader.Name) {
						case "Line":
							lineName = xmlReader["Name"];
							break;
						case "Point":
							points.Add(new Vector2(
								float.Parse(xmlReader["X"]),
								float.Parse(xmlReader["Y"])));
							break;
						default:
							break;
					}

				}


			} catch (Exception e) {
				Debug.LogError("[PatternReader] " + e);
				throw;
			} finally {
				if (xmlReader != null) {
					xmlReader.Close();
				}
			}

			return points;
		}
	}
}