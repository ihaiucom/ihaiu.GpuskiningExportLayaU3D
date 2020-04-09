using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LayaExportSetting 
{

    private static LayaExportSetting instance = null;
    public static LayaExportSetting Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LayaExportSetting();
                instance.Load();
            }
            return instance;
        }
    }

    public string xmlPath = "Assets/LayaAir3D/LayaTool/Configuration.xml";
    public XmlDocument xmlDocument;

    public string SavePath;
    public string CustomizeDirectoryName;
    public bool CustomizeDirectory;

    public void Load(bool isExportGameResourcesSettings = false)
    {
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(xmlPath);
        this.xmlDocument = xmlDocument;
        XmlNode config1 = xmlDocument.SelectSingleNode("LayaExportSetting/config1");

        SavePath = config1.SelectSingleNode("SavePath").InnerText;
        CustomizeDirectoryName = config1.SelectSingleNode("CustomizeDirectoryName").InnerText;
        CustomizeDirectory = config1.SelectSingleNode("CustomizeDirectory").InnerText == "True";

        if(isExportGameResourcesSettings)
        {
            ExportGameResourcesSettings.Instance.gameBinPath = SavePath;
            ExportGameResourcesSettings.Instance.dirName = CustomizeDirectoryName;
            ExportGameResourcesSettings.Instance.isUseDir = CustomizeDirectory;
        }

    }

    public void Save(bool isExportGameResourcesSettings = false)
    {

        if (isExportGameResourcesSettings)
        {
            SavePath = ExportGameResourcesSettings.Instance.exportRoot;
            CustomizeDirectoryName = ExportGameResourcesSettings.Instance.dirName;
            CustomizeDirectory = ExportGameResourcesSettings.Instance.isUseDir;
        }

        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(xmlPath);
        this.xmlDocument = xmlDocument;
        XmlNode config1 = xmlDocument.SelectSingleNode("LayaExportSetting/config1");
        config1.SelectSingleNode("SavePath").InnerText = SavePath;
        config1.SelectSingleNode("CustomizeDirectoryName").InnerText = CustomizeDirectoryName;
        config1.SelectSingleNode("CustomizeDirectory").InnerText = CustomizeDirectory ? "True" : "False";
        xmlDocument.Save(xmlPath);
        LayaAir3D.ReadExportConfig();
    }



}
