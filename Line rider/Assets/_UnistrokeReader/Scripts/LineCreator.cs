///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 10/04/2020 18:38
///-----------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class LineCreator : MonoBehaviour {

        [SerializeField] private GameObject linePrefab = null;
        [SerializeField] private DollarRecognizer dollarRecognizer = default;

        private List<Line> lines = new List<Line>();

        private Line activeLine = null;

        public event Action<Line> OnMouseUp;

        private void Update() {
            if (Input.GetMouseButtonDown(0) && Input.mousePosition.x > 150) {
                for (int i = lines.Count - 1; i >= 0; i--) {
                    Destroy(lines[i].gameObject);
                    lines.RemoveAt(i);
                }

                GameObject lineGO = Instantiate(linePrefab);
                activeLine = lineGO.GetComponent<Line>();
                lines.Add(activeLine);
            }

            if (Input.GetMouseButtonUp(0) && activeLine != null) {
                OnMouseUp?.Invoke(activeLine);
                activeLine = null;
            }

            if (activeLine != null) {
                if (Input.mousePosition.x > 150) {
                    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    activeLine.UpdateLine(mousePos);
                }
            }
        }
    }
}