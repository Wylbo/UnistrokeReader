///-----------------------------------------------------------------
/// Author : Maximilien Galea
/// Date : 11/04/2020 14:42
///-----------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Com.MaximilienGalea.UnistrokeReader {
	public class RecognizeButton : MonoBehaviour {

		[SerializeField] private LineCreator lineCreator;
		[SerializeField] private DollarRecognizer dollarRecognizer;
		private Line line;
		Button button;
		private void Start () {
			button = GetComponent<Button>();
			button.onClick.AddListener(OnClick);
			button.interactable = false;

			lineCreator.OnMouseUp += LineCreator_OnMouseUp;
			
		}

		private void LineCreator_OnMouseUp(Line line) {
			button.interactable = true;
			this.line = line;
		}

		private void OnClick() {
			Debug.Log("[RecognizeButton] onClick");
			dollarRecognizer.StartRecognize(line.Points);
			button.interactable = false;
		}
	}
}