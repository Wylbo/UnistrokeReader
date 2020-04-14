///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 14/04/2020 15:31
///-----------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class PRecognizer {

        protected static int nbrResampledPoints = 128;
        protected static int size = 1000;
        protected List<Glyph> glyphs = null;
        protected string patternFolder = "Assets/_UnistrokeReader/Pattern/";
        protected List<Template> templates = new List<Template>();

        // for math
        const float cloudSize = 32;
        const float lookupTable = 64;
        const float teta = 45 * Mathf.Deg2Rad;
        const float delta = 2 * Mathf.Deg2Rad;
        private static float phi = .5f * (-1 + Mathf.Sqrt(5)); // nombre d'or
        private static float diagonal = Mathf.Sqrt(size * size + size * size);
        private static float halfDiagonal = .5f * diagonal;
        static private Vector2 origin = new Vector2(0, 0);


        private Tuple<Template, float> Recognizer(List<Vector2> points, List<Template> templates) {
            Normalize(points, cloudSize);
            float score = Mathf.Infinity;
            float d = 0;
            Template result = null;

            for (int i = 0; i < templates.Count; i++) {
                Normalize(templates[i].Points, cloudSize); // should be pre process
                d = GreedyCloudMatch(points, templates[i], cloudSize);
                if (d < score) {
                    score = d;
                    result = templates[i];
                }
            }
            score = Mathf.Max((2 - score) / 2, 0);
            return new Tuple<Template, float>(result, score);
        }

        private float GreedyCloudMatch(List<Vector2> points, Template template, float cloudSize) {


            int step = (int)Mathf.Floor(Mathf.Pow(cloudSize, 0.5f));
            float min = Mathf.Infinity;

            for (int i = 0; i < cloudSize; i += step) {
                float d1 = CloudDistance(points, template.Points, i);
                float d2 = CloudDistance(template.Points, points, i);
                min = Mathf.Min(min, d1, d2);
            }

            return min;
        }

        private float CloudDistance(List<Vector2> points1, List<Vector2> points2, int start) {

            bool[] matched = new bool[points1.Count]; //|points1| == |points2| == cloudSize
            float sum = 0;
            int i = start;
            float min = Mathf.Infinity;
            int index = -1;

            do {
                index = -1;
                min = Mathf.Infinity;

                for (int j = 0; j < matched.Length; j++) {
                    if (!matched[j]) {
                        float distance = Vector2.Distance(points1[i], points2[i]);
                        if (distance < min) {
                            min = distance;
                            index = j;
                        }
                    }
                }

                matched[index] = true;
                float weight = 1 - ((i - start + points1.Count) % points1.Count) / points1.Count;
                sum += weight * min;
                i = (i + 1) % points1.Count;

            } while (i == start);

            return sum;
        }

        private void Normalize(List<Vector2> points, float cloudSize) {
            points = Resample(points, cloudSize);
            Scale(points);
            TranslateToOrigin(points, cloudSize);
        }

        private void TranslateToOrigin(List<Vector2> points, float cloudSize) {
            throw new NotImplementedException();
        }

        private void Scale(List<Vector2> points) {
            throw new NotImplementedException();
        }

        private List<Vector2> Resample(List<Vector2> points, float cloudSize) {
            float length = PathLength(points) / (cloudSize - 1);
            float D = 0;
            List<Vector2> newPoints = new List<Vector2>();
            newPoints.Add(points[0]);

            for (int i = 1; i < points.Count; i++) {
                float d = Vector2.Distance(points[i - 1], points[i]);
                if (D + d >= length) {
                    Vector2 q = new Vector2(points[i - 1].x + (length - D) / d * (points[i].x - points[i - 1].x),
                                            points[i - 1].y + (length - D) / d * (points[i].y - points[i - 1].y));

                    newPoints.Add(q);
                    points.Insert(i, q);
                    D = 0;
                } else {
                    D += d;
                }
            }

            return newPoints;
        }

        protected float PathLength(List<Vector2> points) {
            float totalDistance = 0;
            for (int i = 1; i < points.Count; i++) {
                totalDistance += Vector2.Distance(points[i - 1], points[i]);
            }
            return totalDistance;
        }
    }
}