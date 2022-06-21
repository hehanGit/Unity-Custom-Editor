/*************************************
*    ClassName: ExcelToXml
*
*    Explain: Excel文件转为Xml文件
*
*    Function:
*       1、读取Excel文件
*       2、生成Xml文件
*
**************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OfficeOpenXml;
using System.IO;
using System.Xml;
using System;

namespace UnityTools
{
    public class ExcelToXml : EditorWindow
    {

        public Texture icon;

        private string filePath;
        private string saveFilePath;
        private int sheetIndex = 1;
        private int startRow = 1;       //开始行
        private int endRow = 1;         //结束行
        private int startCol = 1;       //开始列
        private int endCol = 1;         //结束列

        

        private string rootName = "ErrorReportForm";
        private string firstNodeName = "ErrorInfo";

        //属性名
        [SerializeField]
        private List<string> attributesName = new List<string>();
        [SerializeField]
        private List<int> ignoreColIndex = new List<int>();
        [SerializeField]
        private List<int> ignoreRowIndex = new List<int>();
        [SerializeField]
        private List<int> chilldNode = new List<int>();

        private SerializedProperty attributesNameProperty;
        private SerializedProperty ignoreColIndexProperty;
        private SerializedProperty ignoreRowIndexProperty;
        private SerializedProperty chilldNodeProperty;

        private SerializedObject serializedObject;

        //文件路径相关的提示信息
        private string filePathMsgStr = "You need to carry the file name and file suffix！";
        private MessageType filePathMsgType = MessageType.Info;
        //工作表索引的提示信息
        private string worksheetIndexMsgStr = "The index of the worksheet starts with 1！";
        private MessageType worksheetIndexMsgType = MessageType.Info;
        //属性名个数提示
        private string attributeNameMsgStr = "The number of attribute names should be: (EndCol - StartCol + 1) - ignoreColIndex.Count";
        private MessageType attributeNameMsgType = MessageType.Info;

        //滚动区域的位置
        private Vector2 scrollViewPos = new Vector2(0, 0);



        [MenuItem("Window/UNIYT Tools/Excel To Xml", false, 1)]
        private static void OpenWindow()
        {
            GetWindowWithRect<ExcelToXml>(new Rect(300, 200, 600, 700), true, "Excel To Window");
            
        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            attributesNameProperty = serializedObject.FindProperty("attributesName");
            ignoreColIndexProperty = serializedObject.FindProperty("ignoreColIndex");
            ignoreRowIndexProperty = serializedObject.FindProperty("ignoreRowIndex");
            chilldNodeProperty = serializedObject.FindProperty("chilldNode");
        }

        private void OnGUI()
        {
            serializedObject.Update();

            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);

            EditorGUILayout.Space(10);
            filePath = EditorGUILayout.TextField("File Path", filePath);
            saveFilePath = EditorGUILayout.TextField("Save File Path", saveFilePath);
            EditorGUILayout.HelpBox(filePathMsgStr, filePathMsgType);

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            rootName = EditorGUILayout.TextField("Root Name", rootName);
            EditorGUILayout.Space(30);
            firstNodeName = EditorGUILayout.TextField("First Node Name", firstNodeName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            sheetIndex = EditorGUILayout.IntField("Sheet Index", sheetIndex);
            EditorGUILayout.HelpBox(worksheetIndexMsgStr, worksheetIndexMsgType);


            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            startRow = EditorGUILayout.IntField("Start Row", startRow);
            EditorGUILayout.Space(30);
            endRow = EditorGUILayout.IntField("End Row", endRow);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            startCol = EditorGUILayout.IntField("Start Col", startCol);
            EditorGUILayout.Space(30);
            endCol = EditorGUILayout.IntField("End Col", endCol);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(attributesNameProperty);
            EditorGUILayout.HelpBox(attributeNameMsgStr, attributeNameMsgType);
            EditorGUILayout.PropertyField(ignoreColIndexProperty);
            EditorGUILayout.PropertyField(ignoreRowIndexProperty);
            EditorGUILayout.PropertyField(chilldNodeProperty);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(20);
            if (GUILayout.Button("Start"))
            {
                ExcelWorksheet worksheet = GetExcelSheet();
                if (worksheet == null) return;
                TranslateToXML(worksheet);
            }
            EditorGUILayout.EndScrollView();
        }

        //得到Excel指定的表
        private ExcelWorksheet GetExcelSheet()
        {
            //路径的判定
            if (File.Exists(filePath) == false)
            {
                filePathMsgStr = "The specified file cannot be found！";
                filePathMsgType = MessageType.Error;
                return null;
            }
            else
            {
                filePathMsgStr = "You need to carry the file name and file suffix！";
                filePathMsgType = MessageType.Info;
            }
                

            FileInfo fs = new FileInfo(filePath);

            ExcelPackage excelPackage = new ExcelPackage(fs);

            //判定表的索引是否正确
            if (sheetIndex < 1 || sheetIndex > excelPackage.Workbook.Worksheets.Count)
            {
                worksheetIndexMsgStr = "The worksheet corresponding to the index does not exist!";
                worksheetIndexMsgType = MessageType.Error;
                return null;
            }
            else
            {
                worksheetIndexMsgStr = "The index of the worksheet starts with 1！";
                worksheetIndexMsgType = MessageType.Info;
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[sheetIndex];
                return worksheet;
            }
        }

        //创建Xml，并导出
        private void TranslateToXML(ExcelWorksheet worksheet)
        {
            //创建XML文档
            XmlDocument xml = new XmlDocument();
            XmlDeclaration header = xml.CreateXmlDeclaration("1.0", "utf-8", null);
            xml.AppendChild(header);

            XmlElement rootNode = xml.CreateElement(string.IsNullOrEmpty(rootName) == true ? "RootNode" : rootName) ;

            if ((endCol - startCol + 1) - ignoreColIndex.Count != attributesName.Count)
            {
                attributeNameMsgStr = "The number of attribute names is not equal to the number required. Please check or calculate carefully！The calculation formula is as follows：(EndCol - StartCol + 1) - ignoreColIndex.Count";
                attributeNameMsgType = MessageType.Error;
                return;
            }
            else
            {
                attributeNameMsgStr = "The number of attribute names should be: (EndCol - StartCol + 1) - ignoreColIndex.Count";
                attributeNameMsgType = MessageType.Info;
            }

            //创建节点
            for (int i = startRow; i <= endRow; i++)
            {
                int attributeIndex = 0;
                if (ignoreRowIndex.Contains(i)) continue;          //若要忽略这行，则直接跳过本次循环
                XmlElement node = xml.CreateElement(string.IsNullOrEmpty(firstNodeName) == true ? "FirstNode" : firstNodeName);          //节点名

                for (int j = startCol; j <= endCol; j++)
                { 
                    if (ignoreColIndex.Contains(j)) continue;     //若要忽略这列，则直接跳过本次循环
                    //创建子节点
                    if (chilldNode.Contains(j))
                    {
                        XmlElement childNode = xml.CreateElement(attributesName[attributeIndex]);          //节点名
                        childNode.InnerText = worksheet.Cells[i, j].Value == null ? "" : worksheet.Cells[i, j].Value.ToString();
                        node.AppendChild(childNode);
                    }
                    else
                        node.SetAttribute(attributesName[attributeIndex], worksheet.Cells[i, j].Value == null ? "" : worksheet.Cells[i, j].Value.ToString());

                    attributeIndex++;
                }
                rootNode.AppendChild(node);
            }
            xml.AppendChild(rootNode);

            //保存文件
            xml.Save(saveFilePath);
        }
    }
}
