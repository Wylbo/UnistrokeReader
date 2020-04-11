///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 10/04/2020 18:26
///-----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class Line : MonoBehaviour {

        [SerializeField] private float pointSpace = .1f;


        [SerializeField] private LineRenderer lineRenderer = null;
        [SerializeField] private EdgeCollider2D edgeCol = null;

        private List<Vector2> _points;

        public List<Vector2> Points { get => _points; protected set => _points = value; }

        public void UpdateLine(Vector2 mousePos) {
            if (Points == null) {
                Points = new List<Vector2>();
                SetPoint(mousePos);
                Debug.Log("[Line] create new line");
                return;
            }
            // check if the mouse has mouve enough 
            if (Vector2.Distance(Points[Points.Count - 1], mousePos) > pointSpace) {
                SetPoint(mousePos);
                Debug.Log("[Line] " + Points.Count);
            }
        }

        private void SetPoint(Vector2 point) {
            Points.Add(point);

            lineRenderer.positionCount = Points.Count;
            lineRenderer.SetPosition(Points.Count - 1, point);

            if (Points.Count > 1) {
                edgeCol.points = Points.ToArray();
            }
        }
    }
}