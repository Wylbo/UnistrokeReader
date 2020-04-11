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
using UnityEngine;
using UnityEngine.UI;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class DollarRecognizer : MonoBehaviour {

        [SerializeField] protected int nbrResampledPoints = 64;
        [SerializeField] protected int size = 250;
        [SerializeField] protected List<Glyph> glyphs = null;
        [SerializeField] protected Text pattern;
        [SerializeField] protected Text percent;

        protected List<List<Vector2>> templates = new List<List<Vector2>>();

        // for math
        protected float teta = 45;
        protected float delta = 2;
        protected float phi = .5f * (-1 + Mathf.Sqrt(5));
        protected Vector2 origin = new Vector2(0, 0);

        protected void Start() {

            Glyph glyph;
            List<Vector2> glyphPoints = new List<Vector2>();
            for (int i = 0; i < glyphs.Count; i++) {
                glyph = glyphs[i];

                Debug.Log("[$1] before resample : " + glyphPoints.Count);
                glyphPoints = Resample(glyph.Points, nbrResampledPoints);
                Debug.Log("[$1] after resample : " + glyphPoints.Count);
                float indicativeAngle = IndicativeAngle(glyphPoints);
                glyphPoints = RotateBy(glyphPoints, indicativeAngle);
                glyphPoints = ScaleTo(glyphPoints, size);   
                glyphPoints = TranslateTo(glyphPoints, origin);

                templates.Add(glyphPoints);
            }
        }



        public bool StartRecognize(List<Vector2> points) {
            // step 1
            Debug.Log("[$1] before resample : " + points.Count);
            points = Resample(points, nbrResampledPoints);
            Debug.Log("[$1] after resample : " + points.Count);

            // step 2
            float indicativeAngle = IndicativeAngle(points);
            points = RotateBy(points, indicativeAngle);

            // step 3
            points = ScaleTo(points, size);
            points = TranslateTo(points, origin);

            //step 4
            Tuple<Glyph, float> result = Recognize(points);

            pattern.text = result.Item1.name;
            percent.text = result.Item2.ToString();
            //Debug.Log("[$1] template : " + result.Item1.Count + " percent : " + result.Item2);

            return result.Item2 >= .7f;
        }

        #region step1
        /// <summary>
        /// Repartie 'points' en 'n' points espac� evenly
        /// </summary>
        /// <param name="points"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected List<Vector2> Resample(List<Vector2> points, int n = 64) {
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
            if (newPoints.Count > 63) { 
                newPoints.RemoveAt(63);
            }
            return newPoints;
        }

        /// <summary>
        /// Return the total length of the drawing
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        protected float PathLength(List<Vector2> points) {
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
        protected Vector2 CalculateCentroid(List<Vector2> points) {
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
        protected float IndicativeAngle(List<Vector2> points) {
            Vector2 centroid = CalculateCentroid(points);
            return Mathf.Atan2(centroid.y - points[0].y, centroid.x - points[0].x);
        }

        /// <summary>
        /// Rotate de angle pour avoir un angle de 0 deg
        /// </summary>
        /// <param name="points"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        protected List<Vector2> RotateBy(List<Vector2> points, float angle) {
            List<Vector2> newPoints = new List<Vector2>();
            Vector2 centroid = CalculateCentroid(points);
            for (int i = 0; i < points.Count; i++) {
                Vector2 q = new Vector2((points[i].x - centroid.x) * Mathf.Cos(angle) - (points[i].y - centroid.y) * Mathf.Sin(angle + centroid.x),
                                        (points[i].y - centroid.y) * Mathf.Cos(angle) - (points[i].x - centroid.x) * Mathf.Sin(angle + centroid.y));
                newPoints.Add(q);
            }
            return newPoints;
        }

        #endregion

        #region step3
        /// <summary>
        /// Scale 'points' so that the resulting bounding box will be of size 'size'
        /// </summary>
        /// <param name="points"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        protected List<Vector2> ScaleTo(List<Vector2> points, float size) {
            List<Vector2> newPoints = new List<Vector2>();

            Bounds bounds = new Bounds(points[0], Vector2.zero);
            foreach (Vector2 point in points) {
                bounds.Encapsulate(point);
            }

            foreach (Vector2 point in points) {
                Vector2 q = new Vector2(point.x * size / bounds.size.x, point.y * size / bounds.size.y);
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
        protected List<Vector2> TranslateTo(List<Vector2> points, Vector2 origin) {
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
        protected Tuple<Glyph, float> Recognize(List<Vector2> points) {
            float b = Mathf.Infinity;
            //List<Vector2> supposedTemlate = null;
            Glyph supposedGlyph = null;

            foreach (Glyph glyph in glyphs) {
                float d = DistanceAtBestAngle(points, glyph.Points, -teta, teta, delta);
                if (d < b) {
                    b = d;
                    supposedGlyph = glyph;
                }
            }
            float score = 1 - b / .5f * Mathf.Sqrt(size * size + size * size);

            return new Tuple<Glyph, float>(supposedGlyph, score);
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
            template = Resample(template);
            try {
                for (int i = 0; i < newPoints.Count; i++) {
                    d += Vector2.Distance(newPoints[i], template[i]);
                }

            } catch (Exception e) {
                Debug.LogError(e);
                Debug.LogError("[$1] Nomber of sample point don't match, Points : " + newPoints.Count + " template : " + template.Count);
            }

            return d / newPoints.Count;
        }
        #endregion
    }
}