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
    public class CurveGenerater : Editor
    {
        public static GameObject curveProfile;

        [MenuItem("GameObject/Curve", false, 11)]
        private static void CurveGenerate()
        {
            ResetParams();
            CreateCurveNode();
        }

        private static void CreateCurveNode()
        {
            curveProfile = new GameObject("Curve Profile");
            curveProfile.AddComponent<Curve>();
        }

        private static void ResetParams()
        {
            curveProfile = null;

            nodeIndex = 0;
            mesh = null;
            material = null;
            scale = new Vector3(0.3f, 0.3f, 0.3f);
            showMessage = false;
            message = "";
            messageType = MessageType.None;

            lockPanel = false;
            referenceObj = null;
            lockX = false;
            lockY = false;
            lockZ = false;

            Color labelColor = Color.white;
        }

        //在Editor中使用的参数
        public static int nodeIndex = 0;
        public static Mesh mesh;
        public static Material material;
        public static Vector3 scale = new Vector3(0.3f, 0.3f, 0.3f);
        public static bool lockPanel;
        public static GameObject referenceObj;
        public static bool lockX; 
        public static bool lockY; 
        public static bool lockZ;

        public static bool showMessage;
        public static string message;
        public static MessageType messageType;

        public static Color labelColor = Color.white;              //Label颜色

    }
}
