/*************************************
*    ClassName: Curve
* 
*    Explain: 曲线
*
*    Function:
*
**************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UKEInterCo.SeRoLife.Helper
{
    public class Curve : MonoBehaviour
    {
        public List<Transform> nodes;               //控制点
        public int density = 1000;                  //密度（决定了线条的平滑度）

        public Color ctrlLineColor = Color.red;             //控制线颜色
        public Color curveColor = Color.white;              //曲线颜色

        private bool hideCtrlLine = false;
        public bool HideCtrlPoint
        {
            set 
            {
                hideCtrlLine = value;
                if (hideCtrlLine == true)
                {
                    for (int i = 0; i < this.transform.childCount; i++)
                    {
                        this.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                else
                {
                    for (int i = 0; i < this.transform.childCount; i++)
                    {
                        this.transform.GetChild(i).gameObject.SetActive(true);
                    }
                }
            }
            get { return hideCtrlLine; }
        }

        private void Awake()
        {
            HideCtrlPoint = true;
        }

        /// <summary>
        /// 得到曲线点
        /// </summary>
        /// <returns></returns>
        public List<Vector3> GetCurve()
        {
            return GetDrawPoint(nodes, density);
        }

        private void OnDrawGizmos()
        {
            if (nodes == null) return;

            //绘制控制线
            if(hideCtrlLine == false)
            {
                Gizmos.color = ctrlLineColor;
                for (int i = 0; i < nodes.Count - 1; i++)
                {
                    Gizmos.DrawLine(nodes[i].position, nodes[i + 1].position);
                }
            }
            

            //绘制曲线
            Gizmos.color = curveColor;
            List<Vector3> points = GetDrawPoint(nodes, density);
            for (int i = 0; i < points.Count - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }

        }

        /// <summary>
        /// 得到曲线点
        /// </summary>
        /// <param name="controlPoint"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        private List<Vector3> GetDrawPoint(List<Transform> controlPoint, int density)
        {
            List<Vector3> points = new List<Vector3>();
            //下一段的起点 等于 上一段的终点
            for (int i = 0; i < controlPoint.Count - 3; i += 3)
            {
                //得到控制点
                var p0 = nodes[i];
                var p1 = nodes[i + 1];
                var p2 = nodes[i + 2];
                var p3 = nodes[i + 3];

                //绘制曲线
                for (int j = 0; j <= density; j++)
                {
                    var t = j / (float)density;        //用来取点（分成很对细小的点）
                    points.Add(CalculateBezierPoint(t, p0.position, p1.position, p2.position, p3.position));
                }
            }
            return points;
        }

        private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 result;

            Vector3 p0p1 = (1 - t) * p0 + t * p1;
            Vector3 p1p2 = (1 - t) * p1 + t * p2;
            Vector3 p2p3 = (1 - t) * p2 + t * p3;

            Vector3 p0p1p2 = (1 - t) * p0p1 + t * p1p2;
            Vector3 p1p2p3 = (1 - t) * p1p2 + t * p2p3;

            result = (1 - t) * p0p1p2 + t * p1p2p3;
            return result;
        }
    }
}
