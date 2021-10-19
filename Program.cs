using System;
using System.IO;
using System.Xml.Serialization;
using System.Data;
using System.Collections.Generic;

using XMLStructure;
using System.Reflection;

namespace BuildXMLTable
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(OIp));
            OIp ser;

            using (Stream reader = new FileStream(@"C:\Users\AADenisenko\Desktop\piev_65201209261721.xml", FileMode.Open))
            {
                ser = (OIp)serializer.Deserialize(reader);
            }

            DataTable dt = new DataTable();
            int depth = 0;
            List<string> propertyNames = new List<string> { };
            List<bool> arrayFlags = new List<bool> { };

            // add columns to dt
            {
                bool arrayFlag = false;
                List<int> depths = new List<int> { };
                foreach (var property in typeof(OIp).GetProperties())
                    GetProperties(property, propertyNames, 0, depths, ref arrayFlag, arrayFlags);

                int repeatedCounter = 1;

                List<string> columnNames = new List<string> { };
                for (int i = 0; i < propertyNames.Count; i++)
                {
                    columnNames.Add(propertyNames[i]);
                    if (propertyNames.GetRange(0, i).Contains(propertyNames[i]))
                    {
                        repeatedCounter++;
                        columnNames[i] = propertyNames[i] + repeatedCounter.ToString();
                    }
                }

                for (int i = 0; i < propertyNames.Count; i++)
                    propertyNames[i] = propertyNames[i] + depths[i];

                foreach (var name in columnNames)
                    dt.Columns.Add(name);
            }

            dt.Rows.Add();
            foreach (var property in typeof(OIp).GetProperties())
                FillTable(dt, property, ser, propertyNames, 0, arrayFlags);

            Console.WriteLine("");
        }

        static void GetProperties(System.Reflection.PropertyInfo propertyInput, List<string> names, int depth, List<int> depths, ref bool arrayFlag, List<bool> arrayFlags)
        {
            if (propertyInput.PropertyType.IsArray)
            {
                foreach (var property in propertyInput.PropertyType.GetElementType().GetProperties())
                {
                    arrayFlag = true;
                    ++depth;
                    GetProperties(property, names, depth, depths, ref arrayFlag, arrayFlags);
                    --depth;
                    arrayFlag = false;
                }
            }
            else if (propertyInput.PropertyType.IsClass && propertyInput.PropertyType != typeof(String))
                foreach (var property in propertyInput.PropertyType.GetProperties())
                {
                    ++depth;
                    GetProperties(property, names, depth, depths, ref arrayFlag, arrayFlags);
                    --depth;
                }
            else
            {
                names.Add(propertyInput.Name);
                depths.Add(depth);
                arrayFlags.Add(arrayFlag);
            }
        }

        static void FillTable<T>(DataTable dt, System.Reflection.PropertyInfo propertyInput, T ser, List<String> names, int depth, List<bool> arrayFlags)
        {
            if (propertyInput.PropertyType.IsArray)
            {
                var arr = propertyInput.GetValue(ser) as Array;
                var property = propertyInput.PropertyType.GetElementType().GetProperties()[0];
                addArray(dt, property, arr, ++depth, names, arrayFlags);
            }

            else if (propertyInput.PropertyType.IsClass && propertyInput.PropertyType != typeof(String))
                foreach (var property in propertyInput.PropertyType.GetProperties())
                {
                    var serChild = propertyInput.GetValue(ser);
                    FillTable(dt, property, serChild, names, depth + 1, arrayFlags);
                }
            else
            {
                string name = propertyInput.Name + depth;
                int index = names.IndexOf(propertyInput.Name + depth);
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i][index] = propertyInput.GetValue(ser);
            }
        }

        static void addArray<T>(DataTable dt, System.Reflection.PropertyInfo propertyInput, T arrayStructure, int depth, List<string> names, List<bool> arrayFlags)
        {
            var arr = arrayStructure as Array;

            int rowNumber = arr.Length;
            int rowCounter = 0;

            int firstRowNumber;
            if (dt.Rows.Count == 1)
                firstRowNumber = 0;
            else
                firstRowNumber = dt.Rows.Count;


            var columns = propertyInput.PropertyType.GetProperties();

            foreach (var rowStructure in arr)
            {
                ++depth;
                var row = propertyInput.GetValue(rowStructure);
                int columnCounter = 0;
                foreach (var column in columns)
                {
                    var val = column.GetValue(row);names.IndexOf(column.Name + depth);
                    int index = names.IndexOf(column.Name + depth);

                    for (int i = firstRowNumber+rowCounter; i < firstRowNumber + rowNumber; i++)
                    {
                        if (i != 0)
                        {
                            if (i >= dt.Rows.Count)
                                dt.Rows.Add();
                            for (int j = 0; j < index-columnCounter; j++)
                                if (arrayFlags[j])
                                    dt.Rows[i][j] = "";
                                else
                                    dt.Rows[i][j] = dt.Rows[i - 1][j];

                        }
                        dt.Rows[i][index] = val;
                    }
                    ++columnCounter;
                }
                ++rowCounter;
                --depth;
            }
        }
    }
}

