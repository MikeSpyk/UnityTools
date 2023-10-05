using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.IO;

namespace BasicTools.Saving
{
    public class SaveFileManager
    {
        public SaveFileManager(string fileName)
        {
            m_fullFilePath = Path.Combine(Application.persistentDataPath, string.Format("{0}.xml", fileName));

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentOutOfRangeException("fileName can not be empty");
            }
        }

        private string m_fullFilePath;
        private XmlDocument m_xmlDocument = null;
        private XmlNamespaceManager m_namespaceManager = null;

        public string getDataField(string name, string defaultValue = "")
        {
            if (m_xmlDocument == null)
            {
                tryOpenXml();
            }

            if (m_xmlDocument == null)
            {
                return defaultValue;
            }
            else
            {
                XmlNode settingNode = m_xmlDocument.SelectSingleNode(string.Format("//{0}", name));

                if (settingNode == null)
                {
                    return defaultValue;
                }
                else
                {
                    return settingNode.InnerText;
                }
            }
        }

        public void setDataField(string name, string value)
        {
            if (tryOpenXml())
            {
                XmlNode settingNode = m_xmlDocument.SelectSingleNode(string.Format("//{0}", name));
                if (settingNode != null)
                {
                    settingNode.InnerText = value;
                }
                else
                {
                    XmlNode settingsParentNodeSelected = m_xmlDocument.SelectSingleNode("//Settings");
                    if (settingsParentNodeSelected == null)
                    {
                        XmlElement settingsParentNode = m_xmlDocument.CreateElement(string.Empty, "Settings", string.Empty);
                        m_xmlDocument.AppendChild(settingsParentNode);

                        XmlElement settingNode2 = m_xmlDocument.CreateElement(string.Empty, name, string.Empty);
                        XmlText settingValue = m_xmlDocument.CreateTextNode(value);
                        settingNode2.AppendChild(settingValue);
                        settingsParentNode.AppendChild(settingNode2);
                    }
                    else
                    {
                        XmlElement settingNode2 = m_xmlDocument.CreateElement(string.Empty, name, string.Empty);
                        XmlText settingValue = m_xmlDocument.CreateTextNode(value);
                        settingNode2.AppendChild(settingValue);
                        settingsParentNodeSelected.AppendChild(settingNode2);
                    }
                }

                m_xmlDocument.Save(m_fullFilePath);
            }
            else
            {
                // create document from scratch

                m_xmlDocument = new XmlDocument();

                XmlDeclaration xmlDeclaration = m_xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement root = m_xmlDocument.DocumentElement;
                m_xmlDocument.InsertBefore(xmlDeclaration, root);

                XmlElement settingsParentNode = m_xmlDocument.CreateElement(string.Empty, "Settings", string.Empty);
                m_xmlDocument.AppendChild(settingsParentNode);

                XmlElement settingNode = m_xmlDocument.CreateElement(string.Empty, name, string.Empty);
                XmlText settingValue = m_xmlDocument.CreateTextNode(value);
                settingNode.AppendChild(settingValue);
                settingsParentNode.AppendChild(settingNode);

                makeSureParentDirectoriesExist(m_fullFilePath);
                m_xmlDocument.Save(m_fullFilePath);
            }
        }

        private static void makeSureParentDirectoriesExist(string fileName)
        {
            string parentDirectory = Path.GetDirectoryName(fileName);
            int loops = 0;

            while (!Directory.Exists(parentDirectory))
            {
                if (loops > 100)
                {
                    throw new TimeoutException("create directory loop out after 100 iterations");
                }

                Directory.CreateDirectory(parentDirectory);
                parentDirectory = Path.GetDirectoryName(parentDirectory);
                loops++;
            }
        }

        private bool tryOpenXml()
        {
            if (File.Exists(m_fullFilePath))
            {
                try
                {
                    m_xmlDocument = new XmlDocument();
                    m_xmlDocument.Load(m_fullFilePath);
                    m_namespaceManager = new XmlNamespaceManager(m_xmlDocument.NameTable);
                    m_namespaceManager.AddNamespace("ehd", "urn:ehd/001");

                    return true;
                }
                catch (Exception ex)
                {
                    try
                    {
                        System.IO.File.Move(m_fullFilePath, string.Format("{0}_broken_{1}.xml", m_fullFilePath, DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")));
                    }
                    catch (Exception)
                    {
                        try
                        {
                            System.IO.File.Delete(m_fullFilePath);
                        }
                        catch (Exception)
                        {

                        }
                    }

                    throw new System.NotSupportedException("could not load XML-File: " + ex);
                }
            }
            else
            {
                return false;
            }
        }
    }
}