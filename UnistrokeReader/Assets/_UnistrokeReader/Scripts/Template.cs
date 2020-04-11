///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 11/04/2020 18:37
///-----------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class Template {
        private string _name;
        private List<Vector2> _points;

        public string Name { get => _name; }
        public List<Vector2> Points { get => _points; }

        public Template(string name, List<Vector2> points) {
            _name = name;
            _points = DollarRecognizer.Resample(points,64);
        }

    }
}