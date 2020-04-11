///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 11/04/2020 00:23
///-----------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class Glyph : MonoBehaviour {

        [SerializeField] private new string name;

        private List<Vector2> _points = new List<Vector2>();
        private LineRenderer lr;

        public List<Vector2> Points {
            get {
                _points = new List<Vector2>();

                lr = GetComponent<LineRenderer>();
                for (int i = 0; i < lr.positionCount; i++) {
                    _points.Add(lr.GetPosition(i));
                }
                Debug.Log("[Glyph] nbr of points : " + _points.Count);
                return _points;
            }
        }

     
    }
}