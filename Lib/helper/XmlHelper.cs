using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Lib.core;
using Lib.io;
using Lib.extension;

namespace Lib.helper
{
    /// <summary>
    /// xml帮助类
    /// </summary>
    [Obsolete("没啥用")]
    public static class XmlHelper
    {
        public static bool SaveXmlFromStream(Stream stream, string path, string filename, bool autoDispose = true)
        {
            if (stream == null) { return false; }
            IOHelper.CreatePathIfNotExist(path);
            var dom = new XmlDocument();
            dom.Load(stream);
            dom.Save(path + filename);
            return true;
        }

        public static XmlDocument GetXmlDom(string xmlFilePath, string xmlString)
        {
            var dom = new XmlDocument();
            if (xmlFilePath != null && File.Exists(xmlFilePath))
            {
                dom.Load(xmlFilePath);
                return dom;
            }
            if (ValidateHelper.IsPlumpString(xmlString))
            {
                dom.LoadXml(xmlString);
                return dom;
            }
            throw new Exception("未能提供有效数据");
        }
        public static XmlNode GetFirstChildNode(XmlNode node)
        {
            return node.FirstChild;
        }
        public static XmlNode GetLastChildNode(XmlNode node)
        {
            return node.LastChild;
        }

        public static XmlNodeList GetChildNodeList(XmlNode node)
        {
            return node.ChildNodes;
        }

        public static XmlNodeList GetNodeList(XmlNode node, string TagName)
        {
            return node.SelectNodes(TagName);
        }

        public static XmlNode GetRootNode(XmlDocument XmlDom, string RootTagName)
        {
            return XmlDom.SelectSingleNode(RootTagName);
        }

        public static string GetNodeAttributeValue(XmlNode node, string AttributeName)
        {
            XmlAttribute attr = node.Attributes[AttributeName];
            return attr == null ? "" : attr.Value;
        }

        public static Dictionary<string, string> GetNodeAttributeDict(XmlNode node)
        {
            var dict = new Dictionary<string, string>();
            XmlAttribute attr = null;
            for (int i = 0; i < node.Attributes.Count; ++i)
            {
                if ((attr = node.Attributes[i]) == null) { continue; }
                dict.Add(attr.Name, attr.Value);
            }
            return dict;
        }

        public static string GetNodeInnerText(XmlNode node)
        {
            return node.InnerText;
        }
    }

    public static class XmlExtension
    {
        public static IEnumerable<XmlAttribute> GetAttributes_(this XmlNode node) =>
            node.Attributes.AsEnumerable_<XmlAttribute>();


        public static IEnumerable<XmlNode> GetChildren_(this XmlNode node) =>
            node.ChildNodes.AsEnumerable_<XmlNode>();

        public static IEnumerable<XmlNode> SelectNodes_(this XmlNode node, string xpath,
            XmlNamespaceManager manager = null)
        {
            if (manager == null)
            {
                return node.SelectNodes(xpath).AsEnumerable_<XmlNode>();
            }
            else
            {
                return node.SelectNodes(xpath, manager).AsEnumerable_<XmlNode>();
            }
        }
    }
}
