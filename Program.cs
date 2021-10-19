using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

using System.Reflection;

namespace BuildXMLTable
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"C:\Users\AADenisenko\Downloads\piev_65011262668025.xml");
            XmlElement root = doc.DocumentElement;
            DataTable dt = new DataTable();
            dt.Rows.Add();

            int repeatingColumnCounter = 2;
            List<int> depths = new List<int>();
            List<string> parentNodes = new List<string>();
            List<string> arrays = new List<string>();

            FillTable(root, dt, ref repeatingColumnCounter, depths, 0, parentNodes, arrays);

            List<bool> temp = new List<bool>();
            for (int i = 0; i < parentNodes.Count; i++)
            {
                if (!arrays.Contains(parentNodes[i]))
                {
                    string val = "";
                    foreach (DataRow row in dt.Rows)
                        if (!String.IsNullOrEmpty(row[i].ToString()))
                        {
                            val = row[i].ToString();
                            break;
                        }
                    foreach (DataRow row in dt.Rows)
                        row[i] = val;
                }
            }

            Console.WriteLine("");
        }

        static void FillTable(XmlElement root, DataTable dt, ref int repeatingColumnCounter, List<int> depths, int depth, List<string> parentNodes, List<string> arrays)
        {
            if (root.FirstChild.HasChildNodes)
                foreach (XmlElement node in root)
                    FillTable(node, dt, ref repeatingColumnCounter, depths, depth+1, parentNodes, arrays);
            else
            {
                bool addColumn = true;
                string name = root.LocalName;
                string neededName = name;
                int colIndex = 0;
                for (int i = dt.Columns.Count - 1; i >= 0; i--)
                    if (Regex.Match(dt.Columns[i].ColumnName, @"\w+").Value == name)
                    {
                        if (depths[i] == depth)
                        {
                            neededName = dt.Columns[i].ColumnName;
                            colIndex = i;
                            addColumn = false;
                        }
                        else
                        {
                            neededName = name + repeatingColumnCounter.ToString();
                            repeatingColumnCounter++;
                        }
                    }

                bool arrayFlag = false;
                if ((root.ParentNode.PreviousSibling != null) && (root.ParentNode.PreviousSibling.LocalName == root.ParentNode.LocalName))
                {
                    arrayFlag = true;
                }
                else if ((root.ParentNode.NextSibling != null) && (root.ParentNode.NextSibling.LocalName == root.ParentNode.LocalName) && (root.PreviousSibling == null))
                {
                    arrayFlag = true;
                    if (dt.Rows.Count > 1)
                        dt.Rows.Add();
                }

                if (arrayFlag && (parentNodes[colIndex] != "OIp"))
                    arrays.Add(parentNodes[colIndex]);

                if (!addColumn)
                {
                    if (!String.IsNullOrEmpty(dt.Rows[^1][colIndex].ToString()))
                    {
                        dt.Rows.Add();
                        for (int i = colIndex; i > 0; i--)
                            if ((parentNodes[i] == parentNodes[colIndex]) && !String.IsNullOrEmpty(dt.Rows[^2][i].ToString()))
                                dt.Rows[^1][i] = dt.Rows[^2][i];
                    }
                }
                else
                {
                    dt.Columns.Add(neededName);
                    depths.Add(depth);
                    if (root.ParentNode.LocalName == "OIp")
                        parentNodes.Add(((XmlElement)root.ParentNode).LocalName);
                    else
                    {
                        var temp_node = root;
                        while (temp_node.ParentNode.LocalName != "OIp")
                            temp_node = (XmlElement)temp_node.ParentNode;
                        parentNodes.Add(temp_node.LocalName);
                    }
                }

                dt.Rows[^1][neededName] = root.InnerText;
            }
        }
    }
}

