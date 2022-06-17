/*************************************
*    ClassName:
*
*    Explain:
*
*    Function:
*
**************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UKEInterCo.SeRoLife.Helper
{
    [CustomEditor(typeof(Curve))]
    public class CurveEditor : Editor
    {
        private Curve curve;

        private void OnEnable()
        {
            curve = (Curve)target;
        }

        private void OnSceneGUI()
        {
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = CurveGenerater.labelColor;

            for (int i = 0; i < curve.transform.childCount; i++)
            {
                Transform currentPoint = curve.transform.GetChild(i);
                Handles.Label(currentPoint.position, currentPoint.gameObject.name, labelStyle);
            }
            
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Internally, the third-order Bezier curve is used to generate, so at least four control nodes need to be created to generate lines", MessageType.Info);

            curve.density = EditorGUILayout.IntField("Density", curve.density);
            EditorGUI.BeginChangeCheck();
            curve.HideCtrlPoint = EditorGUILayout.Toggle("Hide Ctrl Line", curve.HideCtrlPoint);
            if (EditorGUI.EndChangeCheck())
            {
                if (curve.HideCtrlPoint == true)
                {
                    for (int i = 0; i < curve.transform.childCount; i++)
                    {
                        curve.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                else
                {
                    for (int i = 0; i < curve.transform.childCount; i++)
                    {
                        curve.transform.GetChild(i).gameObject.SetActive(true);
                    }
                }

            };

            //锁定平面
            CurveGenerater.lockPanel = EditorGUILayout.Toggle("Lock Plane", CurveGenerater.lockPanel);
            if (CurveGenerater.lockPanel == true)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUI.skin.box);
                CurveGenerater.referenceObj = (GameObject)EditorGUILayout.ObjectField("Reference Object", CurveGenerater.referenceObj, typeof(GameObject), true);
                CurveGenerater.lockX = EditorGUILayout.Toggle("X", CurveGenerater.lockX);
                CurveGenerater.lockY = EditorGUILayout.Toggle("Y", CurveGenerater.lockY);
                CurveGenerater.lockZ = EditorGUILayout.Toggle("Z", CurveGenerater.lockZ);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;

                if (CurveGenerater.referenceObj != null)
                {
                    Vector3 pos = CurveGenerater.referenceObj.transform.position;
                    float referenceValue = (CurveGenerater.lockX == true ? pos.x : (CurveGenerater.lockY == true ? pos.y : (CurveGenerater.lockZ == true ? pos.z : 0)));
                    for (int i = 0; i < curve.transform.childCount; i++)
                    {
                        Transform targetCtrlPoint = curve.transform.GetChild(i);
                        if (CurveGenerater.lockX)
                            targetCtrlPoint.position = new Vector3(referenceValue, targetCtrlPoint.position.y, targetCtrlPoint.position.z);
                        else if (CurveGenerater.lockY)
                            targetCtrlPoint.position = new Vector3(targetCtrlPoint.position.x, referenceValue, targetCtrlPoint.position.z);
                        else if (CurveGenerater.lockZ)
                            targetCtrlPoint.position = new Vector3(targetCtrlPoint.position.x, targetCtrlPoint.position.y, referenceValue);
                    }
                    
                }


            }
            else
            {
                CurveGenerater.referenceObj = null;
                CurveGenerater.lockX = false;
                CurveGenerater.lockY = false;
                CurveGenerater.lockZ = false;
            }

            //颜色设置
            EditorGUILayout.Space(6);

            curve.ctrlLineColor = EditorGUILayout.ColorField("Ctrl Line", curve.ctrlLineColor);
            curve.curveColor = EditorGUILayout.ColorField("Curve Line", curve.curveColor);
            CurveGenerater.labelColor = EditorGUILayout.ColorField("Label", CurveGenerater.labelColor);

            //控制点设置
            EditorGUILayout.Space(6);
            EditorGUI.BeginChangeCheck();
            CurveGenerater.mesh = (Mesh)EditorGUILayout.ObjectField("Node Mesh", CurveGenerater.mesh, typeof(Mesh), true);
            CurveGenerater.material = (Material)EditorGUILayout.ObjectField("Node Material", CurveGenerater.material, typeof(Material), true);
            CurveGenerater.scale = EditorGUILayout.Vector3Field("Scale", CurveGenerater.scale);

            if (EditorGUI.EndChangeCheck())
            {
                if (CurveGenerater.mesh != null && CurveGenerater.material != null) CurveGenerater.showMessage = false;

                if (curve.nodes != null && curve.nodes.Count > 0)
                {
                    foreach (var item in curve.nodes)
                    {
                        MeshFilter meshFilter = item.GetComponent<MeshFilter>();
                        MeshRenderer mr = item.GetComponent<MeshRenderer>();

                        meshFilter.mesh = CurveGenerater.mesh;
                        mr.material = CurveGenerater.material;
                        item.localScale = CurveGenerater.scale;
                    }
                }
            }

            //消息盒子设置
            EditorGUILayout.Space(6);

            if (CurveGenerater.showMessage) EditorGUILayout.HelpBox(CurveGenerater.message, CurveGenerater.messageType);


            //按钮设置
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Node"))
            {
                
                AddNode();
            }

            if (GUILayout.Button("- Node"))
            {
                ReduceNode();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 增加节点
        /// </summary>
        private void AddNode()
        {
            if (CurveGenerater.curveProfile == null)
            {
                HandlerCurveprofile();
                return;
            }
            else
            {
                CurveGenerater.showMessage = false;
                if (CurveGenerater.mesh == null || CurveGenerater.material == null)
                {
                    CurveGenerater.messageType = MessageType.Error;
                    CurveGenerater.message = "Specify the mesh and material to control point!";
                    CurveGenerater.showMessage = true;
                }
                else
                {
                    //创建控制点
                    GameObject node = new GameObject("Node " + CurveGenerater.nodeIndex);
                    node.AddComponent<MeshFilter>().mesh = CurveGenerater.mesh;
                    node.AddComponent<MeshRenderer>().material = CurveGenerater.material;
                    node.transform.localScale = CurveGenerater.scale;
                    node.transform.SetParent(CurveGenerater.curveProfile.transform);
                    //控制创建出来的点的位置
                    if (CurveGenerater.lockPanel == true)
                    {
                        if (CurveGenerater.referenceObj != null)
                        {
                            Vector3 pos = CurveGenerater.referenceObj.transform.position;
                            float referenceValue = (CurveGenerater.lockX == true ? pos.x : (CurveGenerater.lockY == true ? pos.y : (CurveGenerater.lockZ == true ? pos.z : 0)));
                            for (int i = 0; i < curve.transform.childCount; i++)
                            {
                                Transform targetCtrlPoint = curve.transform.GetChild(i);
                                if (CurveGenerater.lockX)
                                    targetCtrlPoint.position = new Vector3(referenceValue, targetCtrlPoint.position.y, targetCtrlPoint.position.z);
                                else if (CurveGenerater.lockY)
                                    targetCtrlPoint.position = new Vector3(targetCtrlPoint.position.x, referenceValue, targetCtrlPoint.position.z);
                                else if (CurveGenerater.lockZ)
                                    targetCtrlPoint.position = new Vector3(targetCtrlPoint.position.x, targetCtrlPoint.position.y, referenceValue);
                            }

                        }
                    }
                    CurveGenerater.nodeIndex++;

                    if (curve.nodes == null) curve.nodes = new List<Transform>();
                    curve.nodes.Add(node.transform);
                }
            }
            
        }

        //删除顶点
        private void ReduceNode()
        {
            if (CurveGenerater.curveProfile == null)
            {
                HandlerCurveprofile();
                return;
            }
            else
            {
                CurveGenerater.showMessage = false;
                if (curve.nodes == null || curve.nodes.Count <= 0)
                {
                    CurveGenerater.messageType = MessageType.Warning;
                    CurveGenerater.message = "Delete operation failed. There are currently no control points";
                    CurveGenerater.showMessage = true;
                }
                else
                {
                    CurveGenerater.showMessage = false;
                    DestroyImmediate(CurveGenerater.curveProfile.transform.GetChild(CurveGenerater.curveProfile.transform.childCount - 1).gameObject);
                    curve.nodes = new List<Transform>();
                    for (int i = 0; i < CurveGenerater.curveProfile.transform.childCount; i++)
                    {
                        curve.nodes.Add(CurveGenerater.curveProfile.transform.GetChild(i));
                    }
                }
            }
            
        }

        private void HandlerCurveprofile()
        {
            CurveGenerater.messageType = MessageType.Error;
            CurveGenerater.message = "Operation error, please delete this object and retry the operation";
            CurveGenerater.showMessage = true;
            
        }
    }
}
