using UnityEngine;
using UnityEngine.UI;

namespace SlotGame.Audio
{
    public class BtnClick : MonoBehaviour
    {
        private void Start()
        {
            if (TryGetComponent<Button>(out var btn))
                btn.onClick.AddListener(SlotAudioManager.Instance.PlayButtonClick);
        }
    }
}