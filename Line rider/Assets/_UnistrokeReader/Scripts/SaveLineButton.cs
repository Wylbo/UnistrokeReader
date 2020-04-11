///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 11/04/2020 15:13
///-----------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MaximilienGalea.UnistrokeReader {
    public class SaveLineButton : MonoBehaviour {

        [SerializeField] private LineCreator lineCreator;
        [SerializeField] private DollarRecognizer dollarRecognizer;
        [SerializeField] new private Text name;
        private Line line;
        Button button;
        private void Start() {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            button.interactable = false;

            lineCreator.OnMouseUp += LineCreator_OnMouseUp;

        }

        private void OnClick() {
            if (name.text.Length <= 0) {
                Debug.LogError("[SaveLineButton] New pattern must be named");
                return;
            }

            Debug.Log("[SaveLineButton] " + line.Points.Count + name.text);
            //dollarRecognizer.StartRecognize(line.Points);
            SaveLine.SavePattern(line, name.text);
            button.interactable = false;
        }


        private void LineCreator_OnMouseUp(Line line) {
            button.interactable = true;
            this.line = line;
        }
    }
}