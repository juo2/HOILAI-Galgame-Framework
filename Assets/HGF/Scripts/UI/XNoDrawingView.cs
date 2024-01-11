using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

namespace XGUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer))]
    public class XNoDrawingView : Graphic
    {

        public override void SetMaterialDirty()
        {

        }
        public override void SetVerticesDirty()
        {
            
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
