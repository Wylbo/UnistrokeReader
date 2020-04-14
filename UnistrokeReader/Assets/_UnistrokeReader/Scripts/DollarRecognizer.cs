///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 10/04/2020 21:19
///-----------------------------------------------------------------

/*
 * inspired by https://depts.washington.edu/acelab/proj/dollar/dollar.pdf
 * todo : bounds if 0 on x or y
 * glyph creator
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class DollarRecognizer : MonoBehaviour {

        [SerializeField] protected static int nbrResampledPoints = 128;
        [SerializeField] protected static int size = 1000;
        [SerializeField] protected List<Glyph> glyphs = null;
        [SerializeField] protected Text pattern;
        [SerializeField] protected Text percent;

        protected string patternFolder = "Assets/_UnistrokeReader/Pattern/";

        protected List<Template> templates = new List<Template>();

        // for math
        const float teta = 45 * Mathf.Deg2Rad;
        const float delta = 2 * Mathf.Deg2Rad;
        private static float phi = .5f * (-1 + Mathf.Sqrt(5)); // nombre d'or
        private static float diagonal = Mathf.Sqrt(size * size + size * size);
        private static float halfDiagonal = .5f * diagonal;
        static private Vector2 origin = new Vector2(0, 0);

        protected virtual void Start() {
            DirectoryInfo directoryInfo = new DirectoryInfo(patternFolder);
            FileInfo[] files = directoryInfo.GetFiles("*.dollar.xml");

            try {
                foreach (FileInfo file in files) {
                    templates.Add(PatternReader.ReadFromXML(file.FullName));
                }


            } catch (Exception e) {
                Debug.LogError("[$1] " + e);
                throw;
            }

            Debug.Log("[$1] " + templates.Count + " patterns loaded");
        }



        public virtual bool StartRecognize(List<Vector2> points, bool useProtractor = false) {
            // step 1
            Debug.Log("[$1] before resample : " + points.Count);
            points = Normalize(points);
            Debug.Log("[$1] after resample : " + points.Count);
            //step 4
            Tuple<Template, float> result = Recognize(points, useProtractor); ;

            pattern.text = result.Item1.Name;
            percent.text = result.Item2.ToString();
            //Debug.Log("[$1] template : " + result.Item1.Count + " percent : " + result.Item2);

            return result.Item2 >= .7f;
        }

        public static List<Vector2> Normalize(List<Vector2> points) {
            points = Resample(points, nbrResampledPoints);


            // step 2
            float indicativeAngle = IndicativeAngle(points);
            points = RotateBy(points, indicativeAngle);

            // step 3
            points = ScaleTo(points, size);
            points = TranslateTo(points, origin);

            return points;
        }

        #region step1
        /// <summary>
        /// Repartie 'points' en 'n' points espac� evenly
        /// </summary>
        /// <param name="points"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected static List<Vector2> Resample(List<Vector2> points, int n) {
            float l = PathLength(points) / (n - 1);
            float D = 0;
            List<Vector2> newPoints = new List<Vector2>();
            newPoints.Add(points[0]);

            for (int i = 1; i < points.Count; i++) {
                float d = Vector2.Distance(points[i - 1], points[i]);
                if (D + d >= l) {
                    Vector2 q = new Vector2(points[i - 1].x + (l - D) / d * (points[i].x - points[i - 1].x),
                                            points[i - 1].y + (l - D) / d * (points[i].y - points[i - 1].y));

                    newPoints.Add(q);
                    points.Insert(i, q);
                    D = 0;
                } else {
                    D += d;
                }
            }
            if (newPoints.Count > nbrResampledPoints - 1) {
                newPoints.RemoveAt(63);
            }
            return newPoints;
        }

        /// <summary>
        /// Return the total length of the drawing
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        protected static float PathLength(List<Vector2> points) {
            float totalDistance = 0;
            for (int i = 1; i < points.Count; i++) {
                totalDistance += Vector2.Distance(points[i - 1], points[i]);
            }
            return totalDistance;
        }
        #endregion

        #region step2
        /// <summary>
        /// Calcul le centre global des points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        protected static Vector2 CalculateCentroid(List<Vector2> points) {
            Vector2 centroid = new Vector2();

            for (int i = 0; i < points.Count; i++) {
                centroid += points[i];
            }

            centroid /= points.Count;

            return centroid;
        }

        /// <summary>
        /// retourne l'angle entre le centroid et le premier point
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        protected static float IndicativeAngle(List<Vector2> points) {
            Vector2 centroid = CalculateCentroid(points);
            return Mathf.Atan2(centroid.y - points[0].y, centroid.x - points[0].x);
        }

        /// <summary>
        /// Rotate de angle pour avoir un angle de 0 deg
        /// </summary>
        /// <param name="points"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        protected static List<Vector2> RotateBy(List<Vector2> points, float angle) {
            List<Vector2> newPoints = new List<Vector2>();
            Vector2 centroid = CalculateCentroid(points);
            for (int i = 0; i < points.Count; i++) {
                Vector2 q = new Vector2((points[i].x - centroid.x) * Mathf.Cos(angle) - (points[i].y - centroid.y) * Mathf.Sin(angle) + centroid.x,
                                        (points[i].y - centroid.y) * Mathf.Sin(angle) - (points[i].x - centroid.x) * Mathf.Cos(angle) + centroid.y);
                newPoints.Add(q);
            }
            return newPoints;
        }

        #endregion

        #region step3

        protected static Rect BoundingBox(List<Vector2> points) {
            float minX = Mathf.Infinity, maxX = Mathf.NegativeInfinity, minY = Mathf.Infinity, maxY = Mathf.NegativeInfinity;

            for (int i = 0; i < points.Count; i++) {
                minX = Mathf.Min(minX, points[i].x);
                maxX = Mathf.Max(maxX, points[i].x);
                minY = Mathf.Min(minY, points[i].y);
                maxY = Mathf.Max(maxY, points[i].y);
            }

            return new Rect(minX, minY, maxX - minY, maxY - minY);
        }

        /// <summary>
        /// Scale 'points' so that the resulting bounding box will be of size 'size'
        /// </summary>
        /// <param name="points"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        protected static List<Vector2> ScaleTo(List<Vector2> points, float size) {
            List<Vector2> newPoints = new List<Vector2>();

            Rect bound = BoundingBox(points);

            foreach (Vector2 point in points) {
                Vector2 q = new Vector2(point.x * size / bound.width, point.y * size / bound.height);
                newPoints.Add(q);
            }
            return newPoints;
        }

        /// <summary>
        /// Translate points to origin
        /// </summary>
        /// <param name="points"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        protected static List<Vector2> TranslateTo(List<Vector2> points, Vector2 origin) {
            List<Vector2> newPoints = new List<Vector2>();

            Vector2 centroid = CalculateCentroid(points);

            foreach (Vector2 point in points) {
                Vector2 q = new Vector2(point.x + origin.x - centroid.x, point.y + origin.y - centroid.y);
                newPoints.Add(q);
            }
            return newPoints;
        }

        #endregion

        #region step4
        /// <summary>
        /// compare le dessins avec la liste des template et retourne le template et le pourcentage de match
        /// </summary>
        /// <param name="points"></param>
        /// <param name="templates"></param>
        /// <returns></returns>
        protected Tuple<Template, float> Recognize(List<Vector2> points, bool useProtractor) {
            float best = Mathf.Infinity;
            Template supposedTemlate = null;

            foreach (Template template in templates) {
                float d;
                if (useProtractor) {
                    d = OptimalCosineDistance(template.Points, points);
                } else {

                    d = DistanceAtBestAngle(points, template.Points, -teta, teta, delta);
                }

                if (d < best) {
                    best = d;
                    supposedTemlate = template;
                }
            }
            float score = 1 - best / halfDiagonal;

            return new Tuple<Template, float>(supposedTemlate, score);
        }

        private float OptimalCosineDistance(List<Vector2> points1, List<Vector2> points2) {
            float a = 0;
            float b = 0;

            for (int i = 0; i < points1.Count; i += 2) {
                //a += points1[i] * points2[i] + points1[i + 1] * points2[i + 1];
                //b += points1[i] * points2[i + 1] - points1[i + 1] * points2[i];
            }

            return 0;
        }

        /// <summary>
        /// calcule la distace pour le meilleur angle
        /// </summary>
        /// <param name="points"></param>
        /// <param name="template"></param>
        /// <param name="tetaA"></param>
        /// <param name="tetaB"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        protected float DistanceAtBestAngle(List<Vector2> points, List<Vector2> template, float tetaA, float tetaB, float delta) {
            float x1 = phi * tetaA + (1 - phi) * tetaB;
            float f1 = DistanceAtAngle(points, template, x1);

            float x2 = (1 - phi) * tetaA + phi * tetaB;
            float f2 = DistanceAtAngle(points, template, x2);

            while (Mathf.Abs(tetaB - tetaA) > delta) {
                if (f1 < f2) {
                    tetaB = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = phi * tetaA + (1 - phi) * tetaB;
                    f1 = DistanceAtAngle(points, template, x1);
                } else {
                    tetaA = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = (1 - phi) * tetaA + phi * tetaB;
                    f2 = DistanceAtAngle(points, template, x2);
                }
            }

            return Mathf.Min(f1, f2);
        }

        /// <summary>
        /// calcule l'angle entre le point du draw et le template
        /// </summary>
        /// <param name="points"></param>
        /// <param name="template"></param>
        /// <param name="teta"></param>
        /// <returns></returns>
        protected float DistanceAtAngle(List<Vector2> points, List<Vector2> template, float teta) {
            List<Vector2> newPoints = RotateBy(points, teta);
            float d = PathDistance(newPoints, template);
            return d;
        }

        /// <summary>
        /// Retourne la distance compar� du draw et du template
        /// </summary>
        /// <param name="newPoints"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        protected float PathDistance(List<Vector2> newPoints, List<Vector2> template) {
            float d = 0;
            try {
                for (int i = 0; i < newPoints.Count; i++) {
                    d += Vector2.Distance(newPoints[i], template[i]);
                }

            } catch (Exception e) {
                Debug.LogError("[$1] Nomber of sample point don't match, Points : " + newPoints.Count + " template : " + template.Count);
                throw;
            }

            return d / newPoints.Count;
        }
        #endregion
    }
}