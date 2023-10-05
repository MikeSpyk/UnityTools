using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using System.Xml;

namespace BasicTools.Localization
{
    public class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager m_singleton;
        /// <summary>
        /// defines the language that will get loaded in the first try
        /// </summary>
        public static CultureInfo m_cultureInfo;
        /// <summary>
        /// fallback if m_cultureInfo has no available file
        /// </summary>
        public static CultureInfo m_defaultCultureInfo = CultureInfo.GetCultureInfo("en-US");

        /// <summary>
        /// name must be in format "Localization_[TwoLetterISOLanguageName]"
        /// </summary>
        [SerializeField] private TextAsset[] m_localizationFiles;
        private Dictionary<string, XmlDocument> m_languageCode_loadedXml = new Dictionary<string, XmlDocument>();
        private Dictionary<string, TextAsset> m_languageCode_availableFile = new Dictionary<string, TextAsset>();

        private void Awake()
        {
            for (int i = 0; i < m_localizationFiles.Length; i++)
            {
                string[] nameSplit = m_localizationFiles[i].name.Split('_');

                if (nameSplit.Length < 2)
                {
                    Debug.LogWarning("name wrong format: " + m_localizationFiles[i].name); //name must be in format "Localization_[TwoLetterISOLanguageName]"
                }
                else
                {
                    m_languageCode_availableFile.Add(nameSplit[1], m_localizationFiles[i]);
                }
            }

            m_cultureInfo = CultureInfo.CurrentCulture;

            if (!m_languageCode_availableFile.ContainsKey(m_cultureInfo.TwoLetterISOLanguageName))
            {
                Debug.Log("localization for \"" + m_cultureInfo.TwoLetterISOLanguageName + "\" not available. switching to \"" + m_defaultCultureInfo.TwoLetterISOLanguageName + "\"");
                m_cultureInfo = m_defaultCultureInfo;

                if (!m_languageCode_availableFile.ContainsKey(m_cultureInfo.TwoLetterISOLanguageName))
                {
                    Debug.LogWarning("default localization file not available: " + m_cultureInfo.TwoLetterISOLanguageName);
                }
            }

            m_singleton = this;
        }

        private bool loadXml(string languageCode)
        {
            if (m_languageCode_loadedXml.ContainsKey(languageCode))
            {
                Debug.LogWarning("the same language xml was loaded multiple times: " + languageCode);
                m_languageCode_loadedXml.Remove(languageCode);
            }

            TextAsset localizationXmlFile;

            if (m_languageCode_availableFile.TryGetValue(languageCode, out localizationXmlFile))
            {
                try
                {
                    if (localizationXmlFile == null || string.IsNullOrWhiteSpace(localizationXmlFile.text))
                    {
                        throw new System.NotSupportedException("could not load XML-File: File is null or empty");
                    }

                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(localizationXmlFile.text);
                    m_languageCode_loadedXml.Add(languageCode, xml);

                    return true;
                }
                catch (Exception ex)
                {
                    throw new System.NotSupportedException("could not load XML-File: " + ex);
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// gets a localized text for the culture set in m_cultureInfo. if language is not available: returns text for m_defaultCultureInfo. if m_defaultCultureInfo text is not available too: returns textId.
        /// </summary>
        /// <param name="textId">the Id-attribute of the text-node in the xml</param>
        /// <returns></returns>
        public static string getText(string textId)
        {
            bool success;

            string text = getText(textId, textId, m_cultureInfo, out success);

            if (!success)
            {
                Debug.LogWarning(string.Format("Localization text for language \"{0}\" could not be loaded. id:\"{1}\"", LocalizationManager.m_cultureInfo.TwoLetterISOLanguageName, textId));
            }

            return text;
        }
        /// <summary>
        /// gets a localized text for the culture set in m_cultureInfo. if language is not available: returns text for m_defaultCultureInfo. if m_defaultCultureInfo text is not available too: returns defaultText.
        /// </summary>
        /// <param name="textId">the Id-attribute of the text-node in the xml</param>
        /// <param name="defaultText">the reuturn value if not text was found</param>
        /// <returns></returns>
        public static string getText(string textId, string defaultText)
        {
            bool success;

            string text = getText(textId, defaultText, m_cultureInfo, out success);

            if (!success)
            {
                Debug.LogWarning(string.Format("Localization text for language \"{0}\" could not be loaded. id:\"{1}\"", LocalizationManager.m_cultureInfo.TwoLetterISOLanguageName, textId));
            }

            return text;
        }
        /// <summary>
        /// gets a localized text for the culture set in m_cultureInfo. if language is not available: returns text for m_defaultCultureInfo. if m_defaultCultureInfo text is not available too: returns defaultText.
        /// </summary>
        /// <param name="textId">the Id-attribute of the text-node in the xml</param>
        /// <param name="defaultText">the return value if no text was found</param>
        /// <param name="success">is true if text for m_cultureInfo was available</param>
        /// <returns></returns>
        public static string getText(string textId, string defaultText, out bool success)
        {
            return getText(textId, defaultText, m_cultureInfo, out success);
        }
        /// <summary>
        /// gets a localized text for the provided culture. if language is not available: returns the m_defaultCultureInfo text. if the m_defaultCultureInfo text is not available too: returns defaultText.
        /// </summary>
        /// <param name="textId">the Id-attribute of the text-node in the xml</param>
        /// <param name="defaultText">the return value if no text was found</param>
        /// <param name="culture">the culture with the language for the localized text</param>
        /// <param name="success">is true if text for provided culture was available</param>
        /// <returns></returns>
        public static string getText(string textId, string defaultText, CultureInfo culture, out bool success)
        {
            success = true;
            XmlDocument xmlDocument;
            bool isDefaultLanguage = culture.TwoLetterISOLanguageName.Equals(m_defaultCultureInfo.TwoLetterISOLanguageName);

            if (!m_singleton.m_languageCode_loadedXml.TryGetValue(culture.TwoLetterISOLanguageName, out xmlDocument))
            {
                if (m_singleton.loadXml(culture.TwoLetterISOLanguageName))
                {
                    xmlDocument = m_singleton.m_languageCode_loadedXml[culture.TwoLetterISOLanguageName];
                }
                else
                {
                    success = false;
                    if (m_singleton.m_languageCode_loadedXml.TryGetValue(m_defaultCultureInfo.TwoLetterISOLanguageName, out xmlDocument))
                    {
                        isDefaultLanguage = true;
                    }
                    else
                    {
                        if (m_singleton.loadXml(m_defaultCultureInfo.TwoLetterISOLanguageName))
                        {
                            xmlDocument = m_singleton.m_languageCode_loadedXml[m_defaultCultureInfo.TwoLetterISOLanguageName];
                            isDefaultLanguage = true;
                        }
                        else
                        {
                            Debug.LogWarning("default language localization file not available: " + m_defaultCultureInfo.TwoLetterISOLanguageName);
                            return defaultText;
                        }
                    }
                }
            }

            XmlNode textNode = xmlDocument.SelectSingleNode(string.Format("/Texts/Text[@Id='{0}']", textId));

            if (textNode == null)
            {
                success = false;

                if (isDefaultLanguage)
                {
                    return defaultText;
                }
                else
                {
                    if (m_singleton.m_languageCode_loadedXml.TryGetValue(m_defaultCultureInfo.TwoLetterISOLanguageName, out xmlDocument))
                    {
                        isDefaultLanguage = true;
                    }
                    else
                    {
                        if (m_singleton.loadXml(m_defaultCultureInfo.TwoLetterISOLanguageName))
                        {
                            xmlDocument = m_singleton.m_languageCode_loadedXml[m_defaultCultureInfo.TwoLetterISOLanguageName];
                            isDefaultLanguage = true;
                        }
                        else
                        {
                            Debug.LogWarning("default language localization file not available: " + m_defaultCultureInfo.TwoLetterISOLanguageName);
                            return defaultText;
                        }
                    }
                }

                textNode = xmlDocument.SelectSingleNode(string.Format("/Texts/Text[@Id='{0}']", textId));

                if (textNode == null)
                {
                    return defaultText;
                }
                else
                {
                    return textNode.InnerText;
                }
            }
            else
            {
                return textNode.InnerText;
            }
        }
    }
}