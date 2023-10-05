using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.Localization
{
    [RequireComponent(typeof(TMPro.TMP_Text)), DisallowMultipleComponent]
    public class LocalizationText : MonoBehaviour
    {
        [SerializeField] private string m_textId;
        private TMPro.TMP_Text m_Text;

        private void Awake()
        {
            m_Text = GetComponent<TMPro.TMP_Text>();
        }

        private void Start()
        {
            bool success;
            m_Text.text = LocalizationManager.getText(m_textId, m_Text.text, out success);

            if (!success)
            {
                Debug.LogWarning(string.Format("Localization text for language \"{0}\" could not be loaded. id:\"{1}\", Gameobject:\"{2}\"", LocalizationManager.m_cultureInfo.TwoLetterISOLanguageName, m_textId, gameObject.name));
            }
        }
    }
}